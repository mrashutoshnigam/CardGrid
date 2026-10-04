using CardGrid.Core.Services;
using CardGrid.Database;
using CardGrid.Models;
using CardGridTests.Infrastructure;

namespace CardGridTests.Services
{
    /// <summary>
    /// Validates the search, sort and paging rules of <see cref="EmployeeGridService"/> through EF Core
    /// against a SQLite in-memory database (real relational translation, real async execution).
    /// </summary>
    [TestClass]
    public sealed class EmployeeGridServiceTests
    {
        /// <summary>Ordinal string comparison, matching SQLite's default collation, for expected orderings.</summary>
        private static readonly IComparer<object> OrdinalComparer = Comparer<object>.Create((a, b) =>
            a is string sa && b is string sb ? string.CompareOrdinal(sa, sb) : Comparer<object>.Default.Compare(a, b));

        /// <summary>Per-test database with 30 generated employees.</summary>
        private SqliteDatabase database;

        /// <summary>Context over <see cref="database"/>.</summary>
        private CardGridContext context;

        /// <summary>Service under test.</summary>
        private EmployeeGridService service;

        /// <summary>Creates a fresh 30-employee database for each test.</summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.database = new SqliteDatabase(TestData.Employees(30));
            this.context = this.database.CreateContext();
            this.service = new EmployeeGridService(this.context);
        }

        /// <summary>Disposes the per-test context and database.</summary>
        [TestCleanup]
        public void TestCleanup()
        {
            this.context?.Dispose();
            this.database?.Dispose();
        }

        /// <summary>
        /// The constructor must reject a null context. Expected: <see cref="ArgumentNullException"/>.
        /// </summary>
        [TestMethod]
        public void Constructor_NullContext_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new EmployeeGridService(null));
        }

        /// <summary>
        /// A null request is a programming error. Expected: <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_NullRequest_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => this.service.GetPageAsync(null));
        }

        /// <summary>
        /// A cancelled token aborts the query. Expected: <see cref="OperationCanceledException"/> (or a subtype).
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_CancelledToken_Throws()
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await Assert.ThrowsAsync<OperationCanceledException>(() => this.service.GetPageAsync(new GridRequest(), cts.Token));
        }

        /// <summary>
        /// Default request (empty search, Id asc, page 1, 10 rows). Expected: all 30 rows counted,
        /// first 10 by id returned, 3 pages, echo of normalized parameters.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_Defaults_ReturnsFirstPageOrderedById()
        {
            var result = await this.service.GetPageAsync(new GridRequest());

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
        /// Results are read without change tracking. Expected: no entities tracked after a query.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_DoesNotTrackEntities()
        {
            await this.service.GetPageAsync(new GridRequest());

            Assert.AreEqual(0, this.context.ChangeTracker.Entries().Count());
        }

        /// <summary>
        /// Whitespace-only search is treated as no filter. Expected: every row is counted.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_WhitespaceSearch_DoesNotFilter()
        {
            var result = await this.service.GetPageAsync(new GridRequest { SearchParam = "   " });

            Assert.AreEqual(30, result.total);
            Assert.AreEqual(string.Empty, result.searchParam);
        }

        /// <summary>
        /// Search matches a text column by substring. Expected: "user7@" finds only employee 7.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SearchByEmail_MatchesSubstring()
        {
            var result = await this.service.GetPageAsync(new GridRequest { SearchParam = "user7@" });

            Assert.AreEqual(1, result.total);
            Assert.AreEqual(7, result.Data.Single().Id);
        }

        /// <summary>
        /// LIKE wildcards in the search term are escaped, not interpreted. Expected: "%" and "_" match nothing
        /// (no employee field contains them) instead of matching everything.
        /// </summary>
        /// <param name="search">Wildcard-only search term.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow("%")]
        [DataRow("_")]
        public async Task GetPageAsync_LikeWildcards_AreLiteral(string search)
        {
            var result = await this.service.GetPageAsync(new GridRequest { SearchParam = search });

            Assert.AreEqual(0, result.total);
        }

        /// <summary>
        /// The id column is matched exactly, never as a substring. Expected: searching "42" returns
        /// employee 42 but not employee 142 (whose text columns do not contain "42").
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SearchById_MatchesExactIdOnly()
        {
            using var database = new SqliteDatabase(new[]
            {
                new Employee { Id = 42, Name = "Alpha", JobTitle = "Dev", Gender = "Male", ContactNo = "n/a", Email = "a@example.com", HireDate = new DateTime(2020, 1, 1) },
                new Employee { Id = 142, Name = "Beta", JobTitle = "Dev", Gender = "Male", ContactNo = "n/a", Email = "b@example.com", HireDate = new DateTime(2020, 1, 1) },
            });
            await using var context = database.CreateContext();

            var result = await new EmployeeGridService(context).GetPageAsync(new GridRequest { SearchParam = "42" });

            Assert.AreEqual(1, result.total);
            Assert.AreEqual(42, result.Data.Single().Id);
        }

        /// <summary>
        /// Search is trimmed and matched against text columns. Expected: " Engineer " finds the
        /// 10 employees whose id is a multiple of 3, and the echoed term is trimmed.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SearchByJobTitle_TrimsAndFilters()
        {
            var result = await this.service.GetPageAsync(new GridRequest { SearchParam = " Engineer ", Rows = 50 });

            Assert.AreEqual(10, result.total);
            Assert.IsTrue(result.Data.All(e => e.Id % 3 == 0));
            Assert.AreEqual("Engineer", result.searchParam);
        }

        /// <summary>
        /// Search with no matches. Expected: zero total, empty data, zero pages.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SearchWithoutMatches_ReturnsEmpty()
        {
            var result = await this.service.GetPageAsync(new GridRequest { SearchParam = "no-such-value" });

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
        /// <returns>A task representing the test.</returns>
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
        public async Task GetPageAsync_SortByColumn_OrdersWithIdTieBreaker(string sidx, string sord)
        {
            var employees = TestData.Employees(30);

            var result = await this.service.GetPageAsync(new GridRequest { Sidx = sidx, Sord = sord, Rows = 30 });

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
            var ordered = sord == "asc" ? employees.OrderBy(key, OrdinalComparer) : employees.OrderByDescending(key, OrdinalComparer);
            var expected = (sidx.Equals("Id", StringComparison.OrdinalIgnoreCase) ? ordered : ordered.ThenBy(e => e.Id))
                .Select(e => e.Id)
                .ToList();

            CollectionAssert.AreEqual(expected, result.Data.Select(e => e.Id).ToList());
            Assert.AreEqual(sord == "asc" ? SortDirection.asc : SortDirection.desc, result.sord);
        }

        /// <summary>
        /// Unknown sort columns must not reach the query (no dynamic ordering). Expected: falls back to Id.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_UnknownSortColumn_FallsBackToId()
        {
            var result = await this.service.GetPageAsync(new GridRequest { Sidx = "Salary; DROP TABLE Employees", Sord = "desc", Rows = 3 });

            CollectionAssert.AreEqual(new List<int> { 30, 29, 28 }, result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Anything other than "desc" sorts ascending. Expected: <see cref="SortDirection.asc"/>.
        /// </summary>
        /// <param name="sord">Direction value from the client.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("ASC")]
        [DataRow("sideways")]
        public async Task GetPageAsync_NonDescDirection_SortsAscending(string sord)
        {
            var result = await this.service.GetPageAsync(new GridRequest { Sord = sord, Rows = 2 });

            Assert.AreEqual(SortDirection.asc, result.sord);
            CollectionAssert.AreEqual(new List<int> { 1, 2 }, result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Second page of 10. Expected: ids 11..20.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_SecondPage_SkipsFirstPage()
        {
            var result = await this.service.GetPageAsync(new GridRequest { Page = 2, Rows = 10 });

            CollectionAssert.AreEqual(Enumerable.Range(11, 10).ToList(), result.Data.Select(e => e.Id).ToList());
        }

        /// <summary>
        /// Last, partial page (page 3 of 14-row pages over 30 rows). Expected: only ids 29 and 30.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_LastPartialPage_ReturnsRemainder()
        {
            var result = await this.service.GetPageAsync(new GridRequest { Page = 3, Rows = 14 });

            CollectionAssert.AreEqual(new List<int> { 29, 30 }, result.Data.Select(e => e.Id).ToList());
            Assert.AreEqual(3, result.noOfPages);
        }

        /// <summary>
        /// Page beyond the end. Expected: empty data, but total still reported.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_PageBeyondEnd_ReturnsEmptyData()
        {
            var result = await this.service.GetPageAsync(new GridRequest { Page = 99, Rows = 10 });

            Assert.AreEqual(30, result.total);
            Assert.AreEqual(0, result.Data.Count);
        }

        /// <summary>
        /// Huge page index must not overflow the skip computation. Expected: empty data, no exception.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetPageAsync_MaxPageIndex_DoesNotOverflow()
        {
            var result = await this.service.GetPageAsync(new GridRequest { Page = int.MaxValue, Rows = EmployeeGridService.MaxPageSize });

            Assert.AreEqual(0, result.Data.Count);
            Assert.AreEqual(int.MaxValue, result.page);
        }

        /// <summary>
        /// Page indexes below 1 are clamped. Expected: page 1.
        /// </summary>
        /// <param name="page">Requested page.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-5)]
        [DataRow(int.MinValue)]
        public async Task GetPageAsync_PageBelowOne_ClampedToFirstPage(int page)
        {
            var result = await this.service.GetPageAsync(new GridRequest { Page = page, Rows = 5 });

            Assert.AreEqual(1, result.page);
            Assert.AreEqual(1, result.Data.First().Id);
        }

        /// <summary>
        /// Page size is clamped to 1..<see cref="EmployeeGridService.MaxPageSize"/> to prevent
        /// unbounded queries and division by zero. Expected: the clamped size is applied and echoed.
        /// </summary>
        /// <param name="requested">Requested page size.</param>
        /// <param name="expected">Effective page size.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow(0, 1)]
        [DataRow(-10, 1)]
        [DataRow(1, 1)]
        [DataRow(100, 100)]
        [DataRow(100_000, 100)]
        public async Task GetPageAsync_RowsOutOfRange_Clamped(int requested, int expected)
        {
            using var database = new SqliteDatabase(TestData.Employees(150));
            await using var context = database.CreateContext();

            var result = await new EmployeeGridService(context).GetPageAsync(new GridRequest { Rows = requested });

            Assert.AreEqual(expected, result.rows);
            Assert.AreEqual(expected, result.Data.Count);
        }
    }
}
