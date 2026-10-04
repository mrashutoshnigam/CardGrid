using CardGrid.Models;

namespace CardGridTests.Infrastructure
{
    /// <summary>
    /// Deterministic employee fixtures shared across tests.
    /// </summary>
    internal static class TestData
    {
        /// <summary>
        /// Creates <paramref name="count"/> employees with ids 1..count. Every text column is non-null
        /// (LINQ-to-Objects would throw on null where SQL returns no match). Gender alternates so
        /// sorting by it produces ties that exercise the <c>Id</c> tie-breaker.
        /// </summary>
        /// <param name="count">Number of employees.</param>
        /// <returns>The employees, ordered by id.</returns>
        public static List<Employee> Employees(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Employee
                {
                    Id = i,
                    Name = $"Name{i:D3}",
                    JobTitle = i % 3 == 0 ? "Engineer" : "Analyst",
                    BirthDate = new DateTime(1980, 1, 1).AddDays(i),
                    HireDate = new DateTime(2010, 1, 1).AddDays(-i),
                    Gender = i % 2 == 0 ? "Female" : "Male",
                    ContactNo = $"555-{i:D4}",
                    Email = $"user{i}@example.com",
                })
                .ToList();
        }
    }
}
