# 02-scaffold-cardgrid-web: scaffold-cardgrid-web: Create the ASP.NET Core side-by-side host #skill:managing-shared-database-schema

Create the new ASP.NET Core web project alongside the existing CardGrid web application. The new host must be wired for incremental migration and keep the legacy app operational while the modern host is introduced. The shared database must be treated as live during the overlap, so the scaffold task needs the shared-database schema safety discipline from the start.

## Scope Inventory
- Projects affected: `CardGrid\CardGrid.csproj` and the new `CardGrid.Core\CardGrid.Core.csproj`
- Distinct concerns: scaffolded ASP.NET Core host, YARP proxy wiring, shared-database safety, legacy host preservation
- Change signals: classic ASP.NET MVC/Web API web app, `System.Web` usage, old-style project file, shared SQL database behavior from the legacy app
- Skill matches: side-by-side YARP scaffold, ASP.NET Framework → ASP.NET Core migration orchestration, shared-database schema discipline, build validation

## Research Findings
- The scaffolded project was created at `CardGrid.Core\CardGrid.Core.csproj` and added to the solution.
- The new host targets `net10.0`, uses `Microsoft.NET.Sdk.Web`, and enables nullable/implicit usings.
- `Program.cs` currently contains the YARP/SystemWebAdapters bootstrap with a stub proxy host; it is not yet wired to the old app beyond the generated `ProxyTo` launch setting.
- `launchSettings.json` points the new host at the legacy app URL `http://localhost:61061`.
- The scaffold helper resolved `Yarp.ReverseProxy` and `Microsoft.AspNetCore.SystemWebAdapters.CoreServices` at `2.3.0`.
- The old `CardGrid.csproj` now contains `_MigrateToProjectGuid`, preserving the side-by-side migration link.

## Validation Notes
- The scaffold operation completed successfully with the new project and solution entry created.
- The legacy web project remains in the solution and was not removed.
- The scaffolded solution now builds successfully after adding the missing YARP/SystemWebAdapters package references and proxy wiring.

**Done when**: a new ASP.NET Core project exists in the solution, it builds, and the legacy app still remains available alongside it.
