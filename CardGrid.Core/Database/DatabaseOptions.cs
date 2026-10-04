namespace CardGrid.Database
{
    /// <summary>
    /// Startup database behaviour, bound from the <c>Database</c> configuration section.
    /// Both switches default to off so a host pointed at a shared or production database never
    /// writes to it unless explicitly configured to.
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>Configuration section name.</summary>
        public const string SectionName = "Database";

        /// <summary>
        /// Gets or sets the directory substituted for <c>|DataDirectory|</c> in the connection string.
        /// Relative paths resolve against the content root. Ignored when empty.
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create the database (schema only, via EF6)
        /// at startup when it does not exist. Never alters an existing database.
        /// </summary>
        public bool CreateIfMissing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to insert the sample employees at startup when
        /// the <c>Employees</c> table is empty.
        /// </summary>
        public bool SeedOnStartup { get; set; }
    }
}
