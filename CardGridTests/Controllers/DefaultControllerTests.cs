using CardGrid.Core.Controllers;
using CardGrid.Core.Services;
using CardGrid.Database;
using CardGrid.Models;
using CardGridTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CardGridTests.Controllers
{
    /// <summary>
    /// Validates the action results of the ported <see cref="DefaultController"/>.
    /// Replaces the legacy placeholder test that only called <c>Assert.Fail()</c>.
    /// </summary>
    [TestClass]
    public sealed class DefaultControllerTests
    {
        /// <summary>Per-test database with 25 employees.</summary>
        private SqliteDatabase database;

        /// <summary>Context over <see cref="database"/>.</summary>
        private CardGridContext context;

        /// <summary>Controller under test.</summary>
        private DefaultController controller;

        /// <summary>Creates a fresh 25-employee database and controller for each test.</summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.database = new SqliteDatabase(TestData.Employees(25));
            this.context = this.database.CreateContext();
            this.controller = new DefaultController(new EmployeeGridService(this.context));
        }

        /// <summary>Disposes the per-test controller, context and database.</summary>
        [TestCleanup]
        public void TestCleanup()
        {
            this.controller?.Dispose();
            this.context?.Dispose();
            this.database?.Dispose();
        }

        /// <summary>
        /// The constructor must reject a null service. Expected: <see cref="ArgumentNullException"/>.
        /// </summary>
        [TestMethod]
        public void Constructor_NullService_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultController(null));
        }

        /// <summary>
        /// <c>Index</c> renders its default view. Expected: a <see cref="ViewResult"/> with no explicit view name.
        /// </summary>
        [TestMethod]
        public void Index_ReturnsDefaultView()
        {
            var result = this.controller.Index();

            var view = Assert.IsInstanceOfType<ViewResult>(result);
            Assert.IsNull(view.ViewName);
        }

        /// <summary>
        /// <c>GetData</c> wraps the requested page in JSON. Expected: a <see cref="JsonResult"/> whose value is
        /// the second page of 10 (ids 11..20) with 25 total rows.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task GetData_ReturnsJsonGridResponse()
        {
            var result = await this.controller.GetData(new GridRequest { Page = 2, Rows = 10 }, CancellationToken.None);

            var response = Assert.IsInstanceOfType<GridResponse<Employee>>(result.Value);
            Assert.AreEqual(25, response.total);
            Assert.AreEqual(2, response.page);
            CollectionAssert.AreEqual(Enumerable.Range(11, 10).ToList(), response.Data.Select(e => e.Id).ToList());
        }
    }
}
