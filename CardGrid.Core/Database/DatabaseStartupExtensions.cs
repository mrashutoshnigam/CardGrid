using Microsoft.Extensions.Options;

namespace CardGrid.Database
{
    /// <summary>
    /// Startup hooks that prepare the CardGrid database before the host starts serving requests.
    /// </summary>
    public static class DatabaseStartupExtensions
    {
        /// <summary>
        /// Resolves <c>|DataDirectory|</c> and runs the opt-in create/seed steps from <see cref="DatabaseOptions"/>.
        /// Call after <c>Build()</c> so late configuration overrides (e.g. from integration tests) are honoured.
        /// </summary>
        /// <param name="app">The built application.</param>
        /// <returns>A task that completes when the database is ready.</returns>
        public static async Task InitializeCardGridDatabaseAsync(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            var options = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.DataDirectory))
            {
                AppDomain.CurrentDomain.SetData(
                    "DataDirectory",
                    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, options.DataDirectory)));
            }

            if (!options.CreateIfMissing && !options.SeedOnStartup)
            {
                return;
            }

            await using var scope = app.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<CardGridContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(CardGridDatabaseInitializer).FullName);
            await CardGridDatabaseInitializer.InitializeAsync(context, options, logger, app.Lifetime.ApplicationStopping);
        }
    }
}
