# 06-final-validation Progress

- `MSBuild CardGrid.sln -restore -t:Build` built all three projects (legacy `CardGrid`, `CardGrid.Core`, `CardGridTests`) with 0 warnings and 0 errors.
- `dotnet test CardGridTests` passed 64 of 64.
- `dotnet list CardGrid.Core package --vulnerable --include-transitive` found no vulnerable packages.
- Manual run: the Core host on .NET 10 serves the full CardGrid surface against LocalDB.

## Remaining manual cleanup
- **Legacy app is broken by the pre-upgrade package update.** Its `_Layout.cshtml` references the deleted `jquery-3.1.1.min.js`, and its Bootstrap 3 markup now runs on Bootstrap 5.3.8. Nothing else from it needs migrating; the Core host covers every route.
- Retire the legacy side once you are satisfied:
  - remove `CardGrid/` from the solution
  - drop `Yarp.ReverseProxy` and `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` and their wiring in `Program.cs`
  - move `CardGridDB.mdf` or point `Database:DataDirectory` elsewhere
- Production configuration:
  - set a real connection string; the default is LocalDB with `AttachDbFilename`
  - restrict `AllowedHosts`
  - keep `Database:*` switches off unless the database should be created or seeded
- Optional: move from EF6 to EF Core, enable nullable reference types, and adopt Central Package Management.
- The card template loads images from `unsplash.it`, an external host that redirects to picsum.photos. This was kept from the legacy app.
