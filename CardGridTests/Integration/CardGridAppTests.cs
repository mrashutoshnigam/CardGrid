using System.Net;
using System.Text.Json;
using CardGrid.Database;
using CardGridTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CardGridTests.Integration
{
    /// <summary>
    /// End-to-end HTTP tests of the .NET 10 host with SQL Server replaced by a SQLite in-memory database.
    /// They pin the routes and the JSON contract that <c>cardgrid.js</c> depends on.
    /// </summary>
    [TestClass]
    public sealed class CardGridAppTests
    {
        /// <summary>Shared in-process host.</summary>
        private static WebApplicationFactory<Program> factory;

        /// <summary>Database backing the host; kept open for the class lifetime.</summary>
        private static SqliteDatabase database;

        /// <summary>
        /// Starts one host for the class: "Testing" environment (so no Development create/seed),
        /// no legacy proxy, and the pooled SQL Server <see cref="CardGridContext"/> re-registered on SQLite
        /// in-memory with 30 employees.
        /// </summary>
        /// <param name="context">MSTest context (unused).</param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            database = new SqliteDatabase(TestData.Employees(30));
            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ProxyTo", string.Empty);
                builder.ConfigureServices(services =>
                {
                    // Drop the UseSqlServer configuration so only one provider is registered.
                    services.RemoveAll<IDbContextOptionsConfiguration<CardGridContext>>();
                    services.AddDbContextPool<CardGridContext>(options => options.UseSqlite(database.Connection));
                });
            });
        }

        /// <summary>Disposes the shared host and database.</summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            factory?.Dispose();
            database?.Dispose();
        }

        /// <summary>
        /// The site root maps to <c>Default/Index</c>. Expected: 200 HTML containing the grid
        /// container, the plugin script and the <c>GetData</c> URL.
        /// </summary>
        /// <param name="path">Route to request.</param>
        [TestMethod]
        [DataRow("/")]
        [DataRow("/Default")]
        [DataRow("/Default/Index")]
        public async Task Index_Routes_RenderGridPage(string path)
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync(path);
            var html = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.Contains(html, "class=\"card-grid\"");
            StringAssert.Contains(html, "/js/cardgrid.js");
            StringAssert.Contains(html, "/Default/GetData");
        }

        /// <summary>
        /// <c>GetData</c> keeps the legacy JSON member names (no camelCase), emits the enum as a number,
        /// and serializes dates as ISO-8601. Expected: those exact shapes for page 2 sorted by Id desc.
        /// </summary>
        [TestMethod]
        public async Task GetData_ReturnsLegacyJsonContract()
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/Default/GetData?searchParam=&sidx=Id&sord=desc&page=2&rows=5");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual(30, root.GetProperty("total").GetInt32());
            Assert.AreEqual(6, root.GetProperty("noOfPages").GetInt32());
            Assert.AreEqual(2, root.GetProperty("page").GetInt32());
            Assert.AreEqual(1, root.GetProperty("sord").GetInt32());

            var first = root.GetProperty("Data")[0];
            Assert.AreEqual(25, first.GetProperty("Id").GetInt32());
            Assert.AreEqual("Name025", first.GetProperty("Name").GetString());
            Assert.AreEqual("2009-12-07T00:00:00", first.GetProperty("HireDate").GetString());
        }

        /// <summary>
        /// Query-string binding is case-insensitive and applies the search. Expected: one matching row.
        /// </summary>
        [TestMethod]
        public async Task GetData_SearchParam_Filters()
        {
            using var client = factory.CreateClient();

            using var json = JsonDocument.Parse(await client.GetStringAsync("/Default/GetData?SEARCHPARAM=user7%40"));

            Assert.AreEqual(1, json.RootElement.GetProperty("total").GetInt32());
        }

        /// <summary>
        /// Static assets referenced by the layout are served from wwwroot. Expected: 200 for each.
        /// </summary>
        /// <param name="path">Asset path.</param>
        [TestMethod]
        [DataRow("/lib/bootstrap/css/bootstrap.min.css")]
        [DataRow("/lib/bootstrap/js/bootstrap.bundle.min.js")]
        [DataRow("/lib/jquery/jquery-3.7.1.min.js")]
        [DataRow("/lib/bootpag/jquery.bootpag.min.js")]
        [DataRow("/lib/font-awesome/css/font-awesome.min.css")]
        [DataRow("/lib/font-awesome/fonts/fontawesome-webfont.woff2")]
        [DataRow("/css/cardgrid.css")]
        [DataRow("/js/cardgrid.js")]
        public async Task StaticAssets_AreServed(string path)
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// With no <c>ProxyTo</c> configured there is no legacy fallback. Expected: 404 for unknown routes.
        /// </summary>
        [TestMethod]
        public async Task UnknownRoute_WithoutProxy_Returns404()
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/does-not-exist");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
