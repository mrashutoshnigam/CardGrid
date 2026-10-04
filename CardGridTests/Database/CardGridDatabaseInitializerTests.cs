using System.Text.RegularExpressions;
using CardGrid.Database;
using Microsoft.Extensions.Logging.Abstractions;

namespace CardGridTests.Database
{
    /// <summary>
    /// Validates the database initializer pieces that do not need SQL Server: argument guards and
    /// the embedded seed script.
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
        /// <c>Initialize</c> validates its arguments before touching the database.
        /// Expected: <see cref="ArgumentNullException"/> for a null context.
        /// </summary>
        [TestMethod]
        public void Initialize_NullContext_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => CardGridDatabaseInitializer.Initialize(null, new DatabaseOptions(), NullLogger.Instance));
        }

        /// <summary>
        /// <c>Seed</c> validates its argument. Expected: <see cref="ArgumentNullException"/> for a null context.
        /// </summary>
        [TestMethod]
        public void Seed_NullContext_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => CardGridDatabaseInitializer.Seed(null));
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
