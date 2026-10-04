using CardGrid.Database;
using CardGrid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CardGridTests.Infrastructure
{
    /// <summary>
    /// Private SQLite in-memory database with the <see cref="CardGridContext"/> schema. The database lives as
    /// long as this object keeps its connection open, so every test gets an isolated, real relational store.
    /// </summary>
    /// <remarks>
    /// SQLite compares strings ordinally and case-sensitively, unlike SQL Server's default collation;
    /// SQL Server-specific behaviour is covered by <c>SqlServerIntegrationTests</c>.
    /// </remarks>
    internal sealed class SqliteDatabase : IDisposable
    {
        /// <summary>
        /// Opens the connection, creates the schema and inserts <paramref name="employees"/>.
        /// </summary>
        /// <param name="employees">Rows to insert; may be <c>null</c> for an empty table.</param>
        public SqliteDatabase(IEnumerable<Employee> employees = null)
        {
            this.Connection = new SqliteConnection("DataSource=:memory:");
            this.Connection.Open();
            this.Options = new DbContextOptionsBuilder<CardGridContext>().UseSqlite(this.Connection).Options;

            using var context = this.CreateContext();
            context.Database.EnsureCreated();
            if (employees != null)
            {
                context.Employees.AddRange(employees);
                context.SaveChanges();
            }
        }

        /// <summary>Gets the open connection that keeps the in-memory database alive.</summary>
        public SqliteConnection Connection { get; }

        /// <summary>Gets context options bound to <see cref="Connection"/>.</summary>
        public DbContextOptions<CardGridContext> Options { get; }

        /// <summary>Creates a new context over this database. The caller disposes it.</summary>
        /// <returns>A new context.</returns>
        public CardGridContext CreateContext()
        {
            return new CardGridContext(this.Options);
        }

        /// <summary>Closes the connection, discarding the database.</summary>
        public void Dispose()
        {
            this.Connection.Dispose();
        }
    }
}
