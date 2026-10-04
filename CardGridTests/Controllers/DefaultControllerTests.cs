using CardGrid.Core.Controllers;
using CardGrid.Core.Services;
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
        /// <summary>
        /// Builds a controller over <paramref name="count"/> in-memory employees.
        /// </summary>
        /// <param name="count">Number of employees.</param>
        /// <returns>The controller under test.</returns>
        private static DefaultController CreateController(int count = 25)
        {
            return new DefaultController(new EmployeeGridService(new FakeEmployeeStore(TestData.Employees(count))));
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
            var result = CreateController().Index();

            var view = Assert.IsInstanceOfType<ViewResult>(result);
            Assert.IsNull(view.ViewName);
        }

        /// <summary>
        /// <c>GetData</c> wraps the requested page in JSON. Expected: a <see cref="JsonResult"/> whose value is
        /// the second page of 10 (ids 11..20) with 25 total rows.
        /// </summary>
        [TestMethod]
        public void GetData_ReturnsJsonGridResponse()
        {
            var result = CreateController().GetData(new GridRequest { Page = 2, Rows = 10 });

            var response = Assert.IsInstanceOfType<GridResponse<Employee>>(result.Value);
            Assert.AreEqual(25, response.total);
            Assert.AreEqual(2, response.page);
            CollectionAssert.AreEqual(Enumerable.Range(11, 10).ToList(), response.Data.Select(e => e.Id).ToList());
        }
    }
}
