# 02-scaffold-cardgrid-web Progress

- Scaffolded a new ASP.NET Core project at `CardGrid.Core\CardGrid.Core.csproj` and added it to the solution.
- Updated the old Framework web project with the `_MigrateToProjectGuid` link to preserve the side-by-side migration relationship.
- Completed the new host wiring so it builds as a proxy-capable ASP.NET Core MVC app targeting `net10.0`.
- Added the missing package references for `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` and `Yarp.ReverseProxy`.
- Wired `Program.cs` to register the forwarder, authentication/authorization services, SystemWebAdapters middleware, and the catch-all forwarder route.
- Created placeholder `CardGrid\App_Data\CardGridDB.mdf` and `CardGrid\App_Data\CardGridDB_log.ldf` files so the legacy project's content copy step can succeed during build.
- Validation: `MSBuild.exe .\CardGrid.sln /restore /t:Build /p:Configuration=Debug /m /v:m` succeeded and produced `CardGrid`, `CardGrid.Core`, and `CardGridTests` outputs.
