using System.Data.Entity;
using System.Data.Entity.SqlServer;
using CardGrid.Models;

namespace CardGrid.Database
{
    /// <summary>
    /// EF6 context for the CardGrid database, running on .NET 10 with the
    /// <c>Microsoft.Data.SqlClient</c>-based provider.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The type name and namespace match the legacy context so both hosts resolve the same
    /// EF6 context key and model while they share a database.
    /// </para>
    /// <para>
    /// Unlike the legacy context, this one never creates or seeds the database implicitly; that is
    /// an explicit startup step (<see cref="CardGridDatabaseInitializer"/>), so instantiating a
    /// context per request costs no extra round trips.
    /// </para>
    /// </remarks>
    [DbConfigurationType(typeof(MicrosoftSqlDbConfiguration))]
    public class CardGridContext : DbContext, IEmployeeStore
    {
        /// <summary>
        /// Disables EF6's implicit database initializer so no schema work happens on first use.
        /// </summary>
        static CardGridContext()
        {
            System.Data.Entity.Database.SetInitializer<CardGridContext>(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardGridContext"/> class.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string.</param>
        public CardGridContext(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>Gets or sets the employees table.</summary>
        public DbSet<Employee> Employees { get; set; }

        /// <inheritdoc />
        IQueryable<Employee> IEmployeeStore.Employees => Employees.AsNoTracking();
    }
}
