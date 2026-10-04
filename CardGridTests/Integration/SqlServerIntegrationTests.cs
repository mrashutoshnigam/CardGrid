using System.Data.Common;
using CardGrid.Core.Services;
using CardGrid.Database;
using CardGrid.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CardGridTests.Integration
{
    /// <summary>
    /// Verifies the EF Core port against real SQL Server (LocalDB): schema parity with the legacy EF6 schema,
    /// seed idempotency, SQL translation of the grid query, and creation of an <c>AttachDbFilename</c> database.
    /// Each run uses a uniquely named throwaway database. Tests report Inconclusive when LocalDB is unavailable.
    /// </summary>
    [TestClass]
    [TestCategory("SqlServer")]
    public sealed class SqlServerIntegrationTests
    {
        /// <summary>LocalDB server used for all tests.</summary>
        private const string Server = @"(LocalDB)\MSSQLLocalDB";

        /// <summary>Connection string of the throwaway catalog for this run.</summary>
        private static string connectionString;

        /// <summary>Why LocalDB could not be used, or <c>null</c> when it is available.</summary>
        private static string unavailableReason;

        /// <summary>
        /// Creates and seeds a uniquely named database through the production initializer.
        /// </summary>
        /// <param name="context">MSTest context (unused).</param>
        /// <returns>A task representing the setup.</returns>
        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            connectionString = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = "CardGridTests_" + Guid.NewGuid().ToString("N"),
                IntegratedSecurity = true,
                ConnectTimeout = 30,
            }.ConnectionString;

            try
            {
                await using var db = CreateContext();
                await CardGridDatabaseInitializer.InitializeAsync(
                    db,
                    new DatabaseOptions { CreateIfMissing = true, SeedOnStartup = true },
                    NullLogger.Instance);
            }
            catch (SqlException ex)
            {
                unavailableReason = "LocalDB unavailable: " + ex.Message;
            }
        }

        /// <summary>Drops the throwaway database.</summary>
        /// <returns>A task representing the cleanup.</returns>
        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (unavailableReason == null)
            {
                await using var db = CreateContext();
                await db.Database.EnsureDeletedAsync();
            }
        }

        /// <summary>Marks the current test Inconclusive when LocalDB is not available.</summary>
        [TestInitialize]
        public void RequireLocalDb()
        {
            if (unavailableReason != null)
            {
                Assert.Inconclusive(unavailableReason);
            }
        }

        /// <summary>
        /// EF Core must create exactly the schema EF6 created for the legacy app, so both can share a database.
        /// Expected: int identity key, datetime (not datetime2) dates with HireDate NOT NULL, nullable nvarchar(max) strings.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task Schema_MatchesLegacyEf6Schema()
        {
            await using var db = CreateContext();
            var columns = await ReadColumnsAsync(db.Database.GetDbConnection());

            var expected = new[]
            {
                "Id:int:NOT NULL:4:identity",
                "Name:nvarchar:NULL:-1:",
                "JobTitle:nvarchar:NULL:-1:",
                "BirthDate:datetime:NULL:8:",
                "HireDate:datetime:NOT NULL:8:",
                "Gender:nvarchar:NULL:-1:",
                "ContactNo:nvarchar:NULL:-1:",
                "Email:nvarchar:NULL:-1:",
            };
            CollectionAssert.AreEqual(expected, columns);
        }

        /// <summary>
        /// Startup seeding is idempotent. Expected: the initial run inserted 1000 rows; seeding again inserts none.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task SeedAsync_SecondRun_InsertsNothing()
        {
            await using var db = CreateContext();

            Assert.AreEqual(0, await CardGridDatabaseInitializer.SeedAsync(db));
            Assert.AreEqual(1000, await db.Employees.CountAsync());
        }

        /// <summary>
        /// Initializing an existing database never alters it. Expected: EnsureCreated reports nothing created.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task EnsureCreated_ExistingDatabase_IsNoOp()
        {
            await using var db = CreateContext();

            Assert.IsFalse(await db.Database.EnsureCreatedAsync());
        }

        /// <summary>
        /// The grid query translates to SQL Server with the same results the EF6 app produced (captured from
        /// the EF6 host before the port): Name desc, page 2 of 3 rows. Expected: ids 369, 900, 511.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SortByNameDesc_MatchesEf6Results()
        {
            var result = await GetPageAsync(new GridRequest { Sidx = "Name", Sord = "desc", Page = 2, Rows = 3 });

            Assert.AreEqual(1000, result.total);
            CollectionAssert.AreEqual(new List<int> { 369, 900, 511 }, result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Search runs server-side with SQL Server semantics: case-insensitive collation, exact id match, and
        /// <c>BirthDate.ToString()</c> translated to SQL Server's default date text (as EF6 did).
        /// Expected: each term finds employee 1 (Daven, born 1989-07-12).
        /// </summary>
        /// <param name="search">Search term.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow("DCRIBBINS0@MAPY.CZ")]
        [DataRow("Jul 12 1989")]
        [DataRow("62-(815)769-2540")]
        public async Task GetPageAsync_Search_TranslatesLikeEf6(string search)
        {
            var result = await GetPageAsync(new GridRequest { SearchParam = search });

            Assert.AreEqual(1, result.total);
            Assert.AreEqual(1, result.Data.Single().Id);
        }

        /// <summary>
        /// Id search is exact while text and date columns still match by substring. Expected: "999" returns
        /// employee 999, and every other hit contains "999" in its contact number, email or birth year;
        /// none matches merely because its id contains "999".
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SearchById_IsExact()
        {
            var result = await GetPageAsync(new GridRequest { SearchParam = "999", Rows = 100 });

            Assert.IsTrue(result.Data.Any(e => e.Id == 999));
            Assert.IsTrue(result.Data.Where(e => e.Id != 999).All(e =>
                (e.ContactNo ?? string.Empty).Contains("999")
                || (e.Email ?? string.Empty).Contains("999")
                || (e.BirthDate?.Year.ToString() ?? string.Empty).Contains("999")));
        }

        /// <summary>
        /// LIKE wildcards are escaped by EF Core. Expected: "%" matches no rows rather than all 1000.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_PercentSearch_IsLiteral()
        {
            var result = await GetPageAsync(new GridRequest { SearchParam = "%" });

            Assert.AreEqual(0, result.total);
        }

        /// <summary>
        /// A SQL Server <c>AttachDbFilename</c> connection without <c>Initial Catalog</c> (the legacy connection
        /// string shape) can be created from scratch, which EF Core's EnsureCreated alone cannot do.
        /// Expected: first initialization creates the .mdf and schema; a plain attach-by-file connection then
        /// opens it; a second initialization is a no-op.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task InitializeAsync_AttachDbFilenameWithoutCatalog_CreatesDatabase()
        {
            var directory = Path.Combine(Path.GetTempPath(), "cardgrid-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var original = AppDomain.CurrentDomain.GetData("DataDirectory");
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);
            var attachConnection = $@"Server={Server};AttachDbFilename=|DataDirectory|\Attach.mdf;Integrated Security=True;Connect Timeout=30;";
            var options = new DatabaseOptions { CreateIfMissing = true };
            try
            {
                await using (var db = CreateContext(attachConnection))
                {
                    await CardGridDatabaseInitializer.InitializeAsync(db, options, NullLogger.Instance);
                }

                Assert.IsTrue(File.Exists(Path.Combine(directory, "Attach.mdf")));

                await using (var db = CreateContext(attachConnection))
                {
                    Assert.AreEqual(0, await db.Employees.CountAsync());
                    Assert.IsFalse(await db.Database.EnsureCreatedAsync());
                }
            }
            finally
            {
                SqlConnection.ClearAllPools();
                await DropDatabaseByFileAsync(Path.Combine(directory, "Attach.mdf"));
                AppDomain.CurrentDomain.SetData("DataDirectory", original);
                Directory.Delete(directory, recursive: true);
            }
        }

        /// <summary>Creates a context on SQL Server.</summary>
        /// <param name="connection">Connection string; defaults to this run's catalog.</param>
        /// <returns>A new context; the caller disposes it.</returns>
        private static CardGridContext CreateContext(string connection = null)
        {
            var options = new DbContextOptionsBuilder<CardGridContext>()
                .UseSqlServer(connection ?? connectionString)
                .Options;
            return new CardGridContext(options);
        }

        /// <summary>Runs the grid service against this run's catalog.</summary>
        /// <param name="request">Grid request.</param>
        /// <returns>The page returned by the service.</returns>
        private static async Task<GridResponse<Employee>> GetPageAsync(GridRequest request)
        {
            await using var db = CreateContext();
            return await new EmployeeGridService(db).GetPageAsync(request);
        }

        /// <summary>
        /// Reads <c>dbo.Employees</c> column metadata as <c>name:type:nullability:max_length:identity</c> strings.
        /// </summary>
        /// <param name="connection">Open-able connection to the database.</param>
        /// <returns>One descriptor per column, in column order.</returns>
        private static async Task<List<string>> ReadColumnsAsync(DbConnection connection)
        {
            const string sql = @"
SELECT c.name + ':' + t.name + ':' + CASE c.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END + ':'
     + CAST(c.max_length AS varchar(10)) + ':' + CASE c.is_identity WHEN 1 THEN 'identity' ELSE '' END
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Employees')
ORDER BY c.column_id";

            await connection.OpenAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                await using var reader = await command.ExecuteReaderAsync();
                var columns = new List<string>();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(0));
                }

                return columns;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        /// <summary>Drops the LocalDB database whose primary data file is <paramref name="mdfPath"/>, if attached.</summary>
        /// <param name="mdfPath">Full path of the .mdf file.</param>
        /// <returns>A task representing the cleanup.</returns>
        private static async Task DropDatabaseByFileAsync(string mdfPath)
        {
            await using var connection = new SqlConnection($"Server={Server};Database=master;Integrated Security=True;");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
DECLARE @name sysname = (SELECT DB_NAME(database_id) FROM sys.master_files WHERE file_id = 1 AND physical_name = @path);
IF @name IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@name) + N';';
    EXEC sp_executesql @sql;
END";
            command.Parameters.AddWithValue("@path", mdfPath);
            await command.ExecuteNonQueryAsync();
        }
    }
}
