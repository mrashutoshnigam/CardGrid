using CardGrid.Database;
using CardGrid.Models;

namespace CardGridTests.Infrastructure
{
    /// <summary>
    /// In-memory <see cref="IEmployeeStore"/> for tests that must not touch SQL Server.
    /// </summary>
    internal sealed class FakeEmployeeStore : IEmployeeStore
    {
        /// <summary>Backing rows.</summary>
        private readonly List<Employee> employees;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeEmployeeStore"/> class.
        /// </summary>
        /// <param name="employees">Rows exposed by <see cref="Employees"/>.</param>
        public FakeEmployeeStore(IEnumerable<Employee> employees)
        {
            this.employees = employees.ToList();
        }

        /// <inheritdoc />
        public IQueryable<Employee> Employees => this.employees.AsQueryable();
    }
}
