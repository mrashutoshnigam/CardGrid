using CardGrid.Models;

namespace CardGrid.Database
{
    /// <summary>
    /// Read-only source of employees. Decouples query logic from EF6 so it can be unit tested
    /// against in-memory data.
    /// </summary>
    public interface IEmployeeStore
    {
        /// <summary>Gets a composable, untracked query over all employees.</summary>
        IQueryable<Employee> Employees { get; }
    }
}
