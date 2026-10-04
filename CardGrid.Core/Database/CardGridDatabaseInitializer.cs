using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CardGrid.Database
{
    /// <summary>
    /// Explicit, idempotent startup initialization for the CardGrid database. Replaces the legacy
    /// pattern of counting and seeding inside the <c>DbContext</c> constructor on every request.
    /// </summary>
    public static class CardGridDatabaseInitializer
    {
        /// <summary>Manifest name of the embedded seed script.</summary>
        private const string SeedResourceName = "CardGrid.Core.Database.Seed.Employees.sql";

        /// <summary>
        /// Applies the configured startup steps to the database behind <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Context connected to the target database.</param>
        /// <param name="options">Which steps to run.</param>
        /// <param name="logger">Logger for progress messages.</param>
        /// <param name="cancellationToken">Cancels the database calls.</param>
        /// <returns>A task that completes when initialization is done.</returns>
        public static async Task InitializeAsync(
            CardGridContext context,
            DatabaseOptions options,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);

            // EnsureCreated only acts on a missing (or table-less) database and never alters an existing
            // schema, which keeps it safe on the database shared with the legacy EF6 app.
            if (options.CreateIfMissing && await EnsureCreatedAsync(context, cancellationToken))
            {
                logger.LogInformation("Created CardGrid database.");
            }

            if (options.SeedOnStartup)
            {
                var inserted = await SeedAsync(context, cancellationToken);
                if (inserted > 0)
                {
                    logger.LogInformation("Seeded {Count} employees.", inserted);
                }
            }
        }

        /// <summary>
        /// Creates the database and schema when missing. Delegates to EF Core's <c>EnsureCreated</c>, except for a
        /// SQL Server <c>AttachDbFilename</c> connection without <c>Initial Catalog</c> whose file does not exist yet:
        /// EF Core cannot create such an auto-named database, so it is created under an explicit name (as EF6 did).
        /// Later connections attach by file path and find it regardless of that name.
        /// </summary>
        /// <param name="context">Context connected to the target database.</param>
        /// <param name="cancellationToken">Cancels the database calls.</param>
        /// <returns><c>true</c> if the database or its schema was created.</returns>
        private static async Task<bool> EnsureCreatedAsync(CardGridContext context, CancellationToken cancellationToken)
        {
            if (!context.Database.IsSqlServer())
            {
                return await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            var builder = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
            var file = ExpandDataDirectory(builder.AttachDBFilename);
            if (string.IsNullOrEmpty(file) || !string.IsNullOrEmpty(builder.InitialCatalog) || File.Exists(file))
            {
                return await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            builder.AttachDBFilename = file;
            builder.InitialCatalog = "CARDGRIDDB_" + Guid.NewGuid().ToString("N");
            var creatorOptions = new DbContextOptionsBuilder<CardGridContext>()
                .UseSqlServer(builder.ConnectionString)
                .Options;

            await using var creator = new CardGridContext(creatorOptions);
            return await creator.Database.EnsureCreatedAsync(cancellationToken);
        }

        /// <summary>
        /// Substitutes the <c>|DataDirectory|</c> token the way SqlClient does, using the AppDomain's
        /// <c>DataDirectory</c> value (falling back to the application base directory).
        /// </summary>
        /// <param name="path">Path that may start with <c>|DataDirectory|</c>.</param>
        /// <returns>The expanded full path, or <paramref name="path"/> unchanged when it is empty or has no token.</returns>
        internal static string ExpandDataDirectory(string path)
        {
            const string token = "|DataDirectory|";
            if (string.IsNullOrEmpty(path) || !path.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var root = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(root))
            {
                root = AppContext.BaseDirectory;
            }

            return Path.GetFullPath(Path.Combine(root, path.Substring(token.Length).TrimStart('\\', '/')));
        }

        /// <summary>
        /// Inserts the sample employees when the table is empty (SQL Server only). The emptiness check and
        /// the inserts run in one transaction under an update/range lock, so concurrent hosts (including the
        /// legacy app during side-by-side migration) cannot seed twice.
        /// </summary>
        /// <param name="context">Context connected to the target database.</param>
        /// <param name="cancellationToken">Cancels the database calls.</param>
        /// <returns>Number of rows inserted; <c>0</c> when the table already had data.</returns>
        public static async Task<int> SeedAsync(CardGridContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var sql =
                "IF NOT EXISTS (SELECT 1 FROM dbo.Employees WITH (UPDLOCK, HOLDLOCK))\nBEGIN\n" +
                LoadSeedScript() +
                "END";

            // The locks only span the check and the inserts if both run inside one explicit transaction.
            // Wrapped in the execution strategy so the unit is retried as a whole if retries are enabled.
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                var inserted = await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // SqlClient reports -1 when no statement affected rows (the IF branch was skipped).
                return Math.Max(0, inserted);
            });
        }

        /// <summary>Reads the embedded seed script.</summary>
        /// <returns>The SQL batch that inserts the sample employees. Contains no format placeholders.</returns>
        /// <exception cref="InvalidOperationException">The resource is missing from the assembly.</exception>
        internal static string LoadSeedScript()
        {
            using var stream = typeof(CardGridDatabaseInitializer).Assembly.GetManifestResourceStream(SeedResourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{SeedResourceName}' not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
