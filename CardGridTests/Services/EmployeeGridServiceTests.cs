using CardGrid.Core.Services;
using CardGrid.Models;
using CardGridTests.Infrastructure;

namespace CardGridTests.Services
{
    /// <summary>
    /// Validates the search, sort and paging rules of <see cref="EmployeeGridService"/>
    /// against in-memory data.
    /// </summary>
    [TestClass]
    public sealed class EmployeeGridServiceTests
    {
        /// <summary>
        /// Builds a service over <paramref name="count"/> generated employees.
        /// </summary>
        /// <param name="count">Number of employees.</param>
        /// <returns>The service under test.</returns>
        private static EmployeeGridService CreateService(int count = 30)
        {
            return new EmployeeGridService(new FakeEmployeeStore(TestData.Employees(count)));
        }

        /// <summary>
        /// The constructor must reject a null store. Expected: <see cref="ArgumentNullException"/>.
        /// </summary>
        [TestMethod]
        public void Constructor_NullStore_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new EmployeeGridService(null));
        }

        /// <summary>
        /// A null request is a programming error. Expected: <see cref="ArgumentNullException"/>.
        /// </summary>
        [TestMethod]
        public void GetPage_NullRequest_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => CreateService().GetPage(null));
        }

        /// <summary>
        /// Default request (empty search, Id asc, page 1, 10 rows). Expected: all 30 rows counted,
        /// first 10 by id returned, 3 pages, echo of normalized parameters.
        /// </summary>
        [TestMethod]
        public void GetPage_Defaults_ReturnsFirstPageOrderedById()
        {
            var result = CreateService().GetPage(new GridRequest());

            Assert.AreEqual(30, result.total);
            Assert.AreEqual(10, result.rows);
            Assert.AreEqual(1, result.page);
            Assert.AreEqual(3, result.noOfPages);
            Assert.AreEqual(SortDirection.asc, result.sord);
            Assert.AreEqual("Id", result.sidx);
            Assert.AreEqual(string.Empty, result.searchParam);
            CollectionAssert.AreEqual(Enumerable.Range(1, 10).ToList(), result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Whitespace-only search is treated as no filter. Expected: every row is counted.
        /// </summary>
        [TestMethod]
        public void GetPage_WhitespaceSearch_DoesNotFilter()
        {
            var result = CreateService().GetPage(new GridRequest { SearchParam = "   " });

            Assert.AreEqual(30, result.total);
            Assert.AreEqual(string.Empty, result.searchParam);
        }

        /// <summary>
        /// Search matches a text column by substring. Expected: "user7@" finds only employee 7.
        /// </summary>
        [TestMethod]
        public void GetPage_SearchByEmail_MatchesSubstring()
        {
            var result = CreateService().GetPage(new GridRequest { SearchParam = "user7@" });

            Assert.AreEqual(1, result.total);
            Assert.AreEqual(7, result.Data.Single().Id);
        }

        /// <summary>
        /// The id column is matched exactly, never as a substring. Expected: searching "42" returns
        /// employee 42 but not employee 142 (whose text columns do not contain "42").
        /// </summary>
        [TestMethod]
        public void GetPage_SearchById_MatchesExactIdOnly()
        {
            var store = new FakeEmployeeStore(new[]
            {
                new Employee { Id = 42, Name = "Alpha", JobTitle = "Dev", Gender = "Male", ContactNo = "n/a", Email = "a@example.com" },
                new Employee { Id = 142, Name = "Beta", JobTitle = "Dev", Gender = "Male", ContactNo = "n/a", Email = "b@example.com" },
            });

            var result = new EmployeeGridService(store).GetPage(new GridRequest { SearchParam = "42" });

            Assert.AreEqual(1, result.total);
            Assert.AreEqual(42, result.Data.Single().Id);
        }

        /// <summary>
        /// Search is trimmed and matched against text columns. Expected: " Engineer " finds the
        /// 10 employees whose id is a multiple of 3, and the echoed term is trimmed.
        /// </summary>
        [TestMethod]
        public void GetPage_SearchByJobTitle_TrimsAndFilters()
        {
            var result = CreateService().GetPage(new GridRequest { SearchParam = " Engineer ", Rows = 50 });

            Assert.AreEqual(10, result.total);
            Assert.IsTrue(result.Data.All(e => e.Id % 3 == 0));
            Assert.AreEqual("Engineer", result.searchParam);
        }

        /// <summary>
        /// Search with no matches. Expected: zero total, empty data, zero pages.
        /// </summary>
        [TestMethod]
        public void GetPage_SearchWithoutMatches_ReturnsEmpty()
        {
            var result = CreateService().GetPage(new GridRequest { SearchParam = "no-such-value" });

            Assert.AreEqual(0, result.total);
            Assert.AreEqual(0, result.Data.Count);
            Assert.AreEqual(0, result.noOfPages);
        }

        /// <summary>
        /// Sorting by each whitelisted column, case-insensitively, in both directions.
        /// Expected: the returned ids follow the ordering of that column, ties broken by id.
        /// </summary>
        /// <param name="sidx">Requested sort column.</param>
        /// <param name="sord">Requested direction.</param>
        [TestMethod]
        [DataRow("Name", "asc")]
        [DataRow("name", "desc")]
        [DataRow("Email", "asc")]
        [DataRow("ContactNo", "desc")]
        [DataRow("BirthDate", "desc")]
        [DataRow("HireDate", "asc")]
        [DataRow("JobTitle", "asc")]
        [DataRow("Gender", "desc")]
        [DataRow("Id", "desc")]
        public void GetPage_SortByColumn_OrdersWithIdTieBreaker(string sidx, string sord)
        {
            var employees = TestData.Employees(30);
            var service = new EmployeeGridService(new FakeEmployeeStore(employees));

            var result = service.GetPage(new GridRequest { Sidx = sidx, Sord = sord, Rows = 30 });

            Func<Employee, object> key = sidx.ToLowerInvariant() switch
            {
                "name" => e => e.Name,
                "email" => e => e.Email,
                "contactno" => e => e.ContactNo,
                "birthdate" => e => e.BirthDate,
                "hiredate" => e => e.HireDate,
                "jobtitle" => e => e.JobTitle,
                "gender" => e => e.Gender,
                _ => e => e.Id,
            };
            var ordered = sord == "asc" ? employees.OrderBy(key) : employees.OrderByDescending(key);
            var expected = (sidx.Equals("Id", StringComparison.OrdinalIgnoreCase) ? ordered : ordered.ThenBy(e => e.Id))
                .Select(e => e.Id)
                .ToList();

            CollectionAssert.AreEqual(expected, result.Data.Select(e => e.Id).ToList());
            Assert.AreEqual(sord == "asc" ? SortDirection.asc : SortDirection.desc, result.sord);
        }

        /// <summary>
        /// Unknown sort columns must not reach the query (no dynamic ordering). Expected: falls back to Id.
        /// </summary>
        [TestMethod]
        public void GetPage_UnknownSortColumn_FallsBackToId()
        {
            var result = CreateService().GetPage(new GridRequest { Sidx = "Salary; DROP TABLE Employees", Sord = "desc", Rows = 3 });

            CollectionAssert.AreEqual(new List<int> { 30, 29, 28 }, result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Anything other than "desc" sorts ascending. Expected: <see cref="SortDirection.asc"/>.
        /// </summary>
        /// <param name="sord">Direction value from the client.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("ASC")]
        [DataRow("sideways")]
        public void GetPage_NonDescDirection_SortsAscending(string sord)
        {
            var result = CreateService().GetPage(new GridRequest { Sord = sord, Rows = 2 });

            Assert.AreEqual(SortDirection.asc, result.sord);
            CollectionAssert.AreEqual(new List<int> { 1, 2 }, result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Second page of 10. Expected: ids 11..20.
        /// </summary>
        [TestMethod]
        public void GetPage_SecondPage_SkipsFirstPage()
        {
            var result = CreateService().GetPage(new GridRequest { Page = 2, Rows = 10 });

            CollectionAssert.AreEqual(Enumerable.Range(11, 10).ToList(), result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Last, partial page (page 3 of 14-row pages over 30 rows). Expected: only ids 29 and 30.
        /// </summary>
        [TestMethod]
        public void GetPage_LastPartialPage_ReturnsRemainder()
        {
            var result = CreateService().GetPage(new GridRequest { Page = 3, Rows = 14 });

            CollectionAssert.AreEqual(new List<int> { 29, 30 }, result.Data.Select(e => e.Id).ToList());
            Assert.AreEqual(3, result.noOfPages);
        }

        /// <summary>
        /// Page beyond the end. Expected: empty data, but total still reported.
        /// </summary>
        [TestMethod]
        public void GetPage_PageBeyondEnd_ReturnsEmptyData()
        {
            var result = CreateService().GetPage(new GridRequest { Page = 99, Rows = 10 });

            Assert.AreEqual(30, result.total);
            Assert.AreEqual(0, result.Data.Count);
        }

        /// <summary>
        /// Huge page index must not overflow the skip computation. Expected: empty data, no exception.
        /// </summary>
        [TestMethod]
        public void GetPage_MaxPageIndex_DoesNotOverflow()
        {
            var result = CreateService().GetPage(new GridRequest { Page = int.MaxValue, Rows = EmployeeGridService.MaxPageSize });

            Assert.AreEqual(0, result.Data.Count);
            Assert.AreEqual(int.MaxValue, result.page);
        }

        /// <summary>
        /// Page indexes below 1 are clamped. Expected: page 1.
        /// </summary>
        /// <param name="page">Requested page.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-5)]
        [DataRow(int.MinValue)]
        public void GetPage_PageBelowOne_ClampedToFirstPage(int page)
        {
            var result = CreateService().GetPage(new GridRequest { Page = page, Rows = 5 });

            Assert.AreEqual(1, result.page);
            Assert.AreEqual(1, result.Data.First().Id);
        }

        /// <summary>
        /// Page size is clamped to 1..<see cref="EmployeeGridService.MaxPageSize"/> to prevent
        /// unbounded queries and division by zero. Expected: the clamped size is applied and echoed.
        /// </summary>
        /// <param name="requested">Requested page size.</param>
        /// <param name="expected">Effective page size.</param>
        [TestMethod]
        [DataRow(0, 1)]
        [DataRow(-10, 1)]
        [DataRow(1, 1)]
        [DataRow(100, 100)]
        [DataRow(100_000, 100)]
        public void GetPage_RowsOutOfRange_Clamped(int requested, int expected)
        {
            var result = CreateService(150).GetPage(new GridRequest { Rows = requested });

            Assert.AreEqual(expected, result.rows);
            Assert.AreEqual(expected, result.Data.Count);
        }
    }
}
