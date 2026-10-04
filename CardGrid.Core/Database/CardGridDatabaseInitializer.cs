using System.Data.Entity;

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
        public static void Initialize(CardGridContext context, DatabaseOptions options, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);

            if (options.CreateIfMissing && context.Database.CreateIfNotExists())
            {
                logger.LogInformation("Created CardGrid database.");
            }

            if (options.SeedOnStartup)
            {
                var inserted = Seed(context);
                if (inserted > 0)
                {
                    logger.LogInformation("Seeded {Count} employees.", inserted);
                }
            }
        }

        /// <summary>
        /// Inserts the sample employees when the table is empty. The emptiness check and the inserts
        /// run in one transaction under an update/range lock, so concurrent hosts (including the
        /// legacy app during side-by-side migration) cannot seed twice.
        /// </summary>
        /// <param name="context">Context connected to the target database.</param>
        /// <returns>Number of rows inserted; <c>0</c> when the table already had data.</returns>
        public static int Seed(CardGridContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var sql =
                "IF NOT EXISTS (SELECT 1 FROM Employees WITH (UPDLOCK, HOLDLOCK))\nBEGIN\n" +
                LoadSeedScript() +
                "END";

            return context.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sql);
        }

        /// <summary>Reads the embedded seed script.</summary>
        /// <returns>The SQL batch that inserts the sample employees.</returns>
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
