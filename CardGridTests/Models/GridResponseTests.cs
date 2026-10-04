using CardGrid.Models;

namespace CardGridTests.Models
{
    /// <summary>
    /// Validates <see cref="GridResponse{T}"/> defaults and page-count arithmetic.
    /// </summary>
    [TestClass]
    public sealed class GridResponseTests
    {
        /// <summary>
        /// A new response carries the legacy defaults. Expected: total 12, rows 1, page 1, asc,
        /// empty strings, and a non-null empty data list.
        /// </summary>
        [TestMethod]
        public void Constructor_SetsLegacyDefaults()
        {
            var response = new GridResponse<Employee>();

            Assert.AreEqual(12, response.total);
            Assert.AreEqual(1, response.rows);
            Assert.AreEqual(1, response.page);
            Assert.AreEqual(SortDirection.asc, response.sord);
            Assert.AreEqual(string.Empty, response.sidx);
            Assert.AreEqual(string.Empty, response.searchParam);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.Count);
        }

        /// <summary>
        /// Page count rounds up and is zero for non-positive page sizes (legacy code divided by zero).
        /// Expected: the value in <paramref name="expected"/>.
        /// </summary>
        /// <param name="total">Total rows.</param>
        /// <param name="rows">Page size.</param>
        /// <param name="expected">Expected page count.</param>
        [TestMethod]
        [DataRow(0, 10, 0)]
        [DataRow(1, 10, 1)]
        [DataRow(10, 10, 1)]
        [DataRow(11, 10, 2)]
        [DataRow(1000, 12, 84)]
        [DataRow(5, 0, 0)]
        [DataRow(5, -1, 0)]
        public void NoOfPages_RoundsUp(int total, int rows, int expected)
        {
            var response = new GridResponse<Employee> { total = total, rows = rows };

            Assert.AreEqual(expected, response.noOfPages);
        }

        /// <summary>
        /// <see cref="Employee.PhotoUrl"/> is derived from the id. Expected: <c>\Photos\{id}.jpg</c>.
        /// </summary>
        [TestMethod]
        public void EmployeePhotoUrl_DerivedFromId()
        {
            Assert.AreEqual(@"\Photos\17.jpg", new Employee { Id = 17 }.PhotoUrl);
        }
    }
}
