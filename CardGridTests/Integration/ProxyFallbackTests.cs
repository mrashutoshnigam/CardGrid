using System.Net;
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
    /// Runs the host the way the launch profile does: with <c>ProxyTo</c> pointing at the legacy app.
    /// The legacy app is deliberately unreachable, so a forwarded request surfaces as 502 Bad Gateway.
    /// Guards the regression where the YARP catch-all outranked the MVC routes and every page returned 502.
    /// </summary>
    [TestClass]
    public sealed class ProxyFallbackTests
    {
        /// <summary>Address nothing listens on (port 1), standing in for a stopped legacy app.</summary>
        private const string UnreachableLegacyApp = "http://127.0.0.1:1";

        /// <summary>Shared in-process host.</summary>
        private static WebApplicationFactory<Program> factory;

        /// <summary>Database backing the host; kept open for the class lifetime.</summary>
        private static SqliteDatabase database;

        /// <summary>
        /// Starts one host with the legacy proxy enabled and a SQLite in-memory database of 30 employees.
        /// </summary>
        /// <param name="context">MSTest context (unused).</param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            database = new SqliteDatabase(TestData.Employees(30));
            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ProxyTo", UnreachableLegacyApp);
                builder.ConfigureServices(services =>
                {
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
        /// Routes the host implements must be served locally even when a proxy target is configured.
        /// Expected: 200 from the MVC controller, not 502 from the forwarder.
        /// </summary>
        /// <param name="path">Route handled by <c>DefaultController</c>.</param>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        [DataRow("/")]
        [DataRow("/Default/Index")]
        [DataRow("/Default/GetData?searchParam=&rows=2")]
        public async Task ControllerRoutes_WithProxyConfigured_AreServedLocally(string path)
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Routes the host does not implement still fall through to the legacy app.
        /// Expected: 502, proving the request reached the forwarder (the target is unreachable).
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod]
        public async Task UnknownRoute_WithProxyConfigured_IsForwarded()
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/legacy-only");

            Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
        }
    }
}
