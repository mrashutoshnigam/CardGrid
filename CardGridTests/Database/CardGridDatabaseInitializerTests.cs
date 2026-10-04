using System.Text.RegularExpressions;
using CardGrid.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CardGridTests.Database
{
    /// <summary>
    /// Validates the database initializer pieces that do not need SQL Server: argument guards, the embedded
    /// seed script, <c>|DataDirectory|</c> expansion and provider-neutral schema creation (on SQLite).
    /// </summary>
    [TestClass]
    public sealed class CardGridDatabaseInitializerTests
    {
        /// <summary>
        /// The seed script ships as an embedded resource with all 1000 legacy rows and pins
        /// DATEFORMAT so month/day literals parse identically on any server language.
        /// Expected: resource loads, contains <c>SET DATEFORMAT mdy</c> and exactly 1000 inserts into Employees.
        /// </summary>
        [TestMethod]
        public void LoadSeedScript_ContainsAllLegacyRows()
        {
            var sql = CardGridDatabaseInitializer.LoadSeedScript();

            StringAssert.Contains(sql, "SET DATEFORMAT mdy;");
            var inserts = Regex.Matches(sql, @"^insert into Employees \(", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Assert.AreEqual(1000, inserts.Count);
        }

        /// <summary>
        /// <c>InitializeAsync</c> validates its arguments before touching the database.
        /// Expected: <see cref="ArgumentNullException"/> for a null context.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task InitializeAsync_NullContext_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(
                () => CardGridDatabaseInitializer.InitializeAsync(null, new DatabaseOptions(), NullLogger.Instance));
        }

        /// <summary>
        /// <c>SeedAsync</c> validates its argument. Expected: <see cref="ArgumentNullException"/> for a null context.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task SeedAsync_NullContext_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => CardGridDatabaseInitializer.SeedAsync(null));
        }

        /// <summary>
        /// With creation enabled on a non-SQL Server provider, initialization defers to EF Core's EnsureCreated.
        /// Expected: an empty SQLite database gets the Employees table; a second call reports nothing created.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task InitializeAsync_CreateIfMissing_CreatesSchemaOnce()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CardGridContext>().UseSqlite(connection).Options;
            var settings = new DatabaseOptions { CreateIfMissing = true };

            await using (var context = new CardGridContext(options))
            {
                await CardGridDatabaseInitializer.InitializeAsync(context, settings, NullLogger.Instance);
            }

            await using (var context = new CardGridContext(options))
            {
                Assert.AreEqual(0, await context.Employees.CountAsync());
                Assert.IsFalse(await context.Database.EnsureCreatedAsync());
            }
        }

        /// <summary>
        /// <c>|DataDirectory|</c> expands against the AppDomain value, with or without a separator after the token.
        /// Expected: both forms resolve to the same full path under the configured directory.
        /// </summary>
        /// <param name="input">Path containing the token.</param>
        [TestMethod]
        [DataRow(@"|DataDirectory|\Db.mdf")]
        [DataRow("|DataDirectory|Db.mdf")]
        [DataRow("|datadirectory|/Db.mdf")]
        public void ExpandDataDirectory_ReplacesToken(string input)
        {
            var original = AppDomain.CurrentDomain.GetData("DataDirectory");
            var root = Path.Combine(Path.GetTempPath(), "cardgrid-datadir");
            try
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", root);

                Assert.AreEqual(Path.Combine(root, "Db.mdf"), CardGridDatabaseInitializer.ExpandDataDirectory(input));
            }
            finally
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", original);
            }
        }

        /// <summary>
        /// Paths without the token (or empty) are returned unchanged. Expected: identity.
        /// </summary>
        /// <param name="input">Path without the token.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(@"C:\data\Db.mdf")]
        public void ExpandDataDirectory_NoToken_Unchanged(string input)
        {
            Assert.AreEqual(input, CardGridDatabaseInitializer.ExpandDataDirectory(input));
        }

        /// <summary>
        /// Write-capable startup steps are opt-in. Expected: a fresh <see cref="DatabaseOptions"/> has both disabled.
        /// </summary>
        [TestMethod]
        public void DatabaseOptions_DefaultsAreReadOnly()
        {
            var options = new DatabaseOptions();

            Assert.IsFalse(options.CreateIfMissing);
            Assert.IsFalse(options.SeedOnStartup);
        }
    }
}
