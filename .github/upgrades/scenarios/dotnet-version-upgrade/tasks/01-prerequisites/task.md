# 01-prerequisites: prerequisites: Validate upgrade readiness and migration constraints

Confirm the toolchain and solution state needed for the upgrade, including the .NET 10 SDK and the solution's current dependency shape. Capture the upgrade constraints that matter for execution: the web app is a side-by-side migration, the test project follows after the web host exists, and the database remains shared while both hosts are live.

## Scope Inventory
- Projects affected: `CardGrid\CardGrid.csproj` and `CardGridTests\CardGridTests.csproj`
- Distinct concerns: legacy ASP.NET web host, framework test project, shared database, SDK/toolchain readiness
- Key signals: .NET Framework 4.8 targets, old-style csproj files, `packages.config`, `System.Web`/MVC/Web API usage, EF6, binding redirects, placeholder failing test
- Skill matches: build validation, shared-database migration discipline, and task-execution workflow

## Findings
- `validate_dotnet_sdk_installation` confirmed the machine has a compatible .NET 10 SDK installed.
- The assessment query tool was unavailable in this session, so the task was researched from the solution files, project dependencies, and the written assessment instead.
- The solution contains exactly two projects, both targeting .NET Framework 4.8.
- `CardGrid` is a classic ASP.NET MVC/Web API application with `System.Web` dependencies and `Web.config`/`Global.asax` startup.
- `CardGridTests` is a .NET Framework test project that references `CardGrid` and currently contains a placeholder failing test.

## Constraints to Carry Forward
- Use a side-by-side ASP.NET Core host for the web app rather than an in-place rewrite.
- Treat the database as shared while the legacy and new web hosts are both live.
- Keep SDK-style conversion and TFM upgrade as separate steps for the test project.
- The test project must be upgraded after the web host exists so it can point at the migrated app.

**Done when**: toolchain readiness is verified, the solution shape is understood, and the migration constraints are recorded for execution.
