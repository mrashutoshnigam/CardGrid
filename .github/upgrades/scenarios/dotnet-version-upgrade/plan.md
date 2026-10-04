# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the CardGrid solution from .NET Framework 4.8 to .NET 10.
**Scope**: 2 projects — one legacy ASP.NET MVC/Web API web application and one framework test project. The web app requires a side-by-side ASP.NET Core migration, and the two hosts will share the same database during the transition.

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | Bottom-Up (Dependency-First) | The solution contains .NET Framework projects, so dependency-ordered migration is required. |
| Project Approach | Side-by-side | The web app uses System.Web/MVC5/Web API and needs an ASP.NET Core host alongside the legacy app for incremental migration. |
| Package Management | Per-Project (defer CPM to post-migration) | This is a Framework-to-modern migration with packages.config, so central package management should wait until the migration stabilizes. |
| Unsupported Packages | Defer Resolution | The legacy ASP.NET package set has no direct net10 equivalent, so incompatible references need follow-up work after the new host exists. |
| System.Web Adapters | Use System.Web Adapters | System.Web and MVC/Web API usage is present, and adapters support an incremental migration path. |
| Assembly Binding Redirects | Remove Binding Redirects | The current web.config has standard bindingRedirect entries that are not needed on modern .NET. |
| Entity Framework | Keep EF6 | EF6 6.5.2 is already compatible enough for the .NET upgrade, and simultaneous EF Core migration would add risk. |
| Nullable Reference Types | Leave Disabled | This is already a high-risk migration, so nullable warnings would add noise right now. |

## Selected Strategy

**Bottom-Up (Dependency-First)** — Upgrade from the legacy web app outward through the dependent test project.
**Rationale**: The solution has two .NET Framework projects and a classic ASP.NET web app, so migration must proceed dependency-first with the web tier handled as a side-by-side migration.

### Dependency Graph

```
Tier 2: [CardGridTests]
   ↓
Tier 1: [CardGrid] (legacy ASP.NET MVC/Web API web app)
```

### Tier Summary

**Tier 1 — CardGrid**
- Legacy ASP.NET MVC/Web API web application.
- Will be handled through side-by-side scaffold/migrate tasks rather than an in-place TFM retarget.
- Must keep the existing app live while the ASP.NET Core host is introduced and validated.

**Tier 2 — CardGridTests**
- Framework test project that depends on the web app.
- Will be converted to SDK-style first, then retargeted to .NET 10 and re-pointed to the new Core host after the web migration is in place.
- Needs to stay buildable while the web app transition is happening.

## Tasks

### 01-prerequisites: Validate upgrade readiness and migration constraints

Confirm the toolchain and solution state needed for the upgrade, including the .NET 10 SDK and the solution's current dependency shape. Capture the upgrade constraints that matter for execution: the web app is a side-by-side migration, the test project follows after the web host exists, and the database remains shared while both hosts are live.

**Done when**: toolchain readiness is verified, the solution shape is understood, and the migration constraints are recorded for execution.

---

### 02-scaffold-cardgrid-web: Create the ASP.NET Core side-by-side host #skill:managing-shared-database-schema

Create the new ASP.NET Core web project alongside the existing CardGrid web application. The new host must be wired for incremental migration and keep the legacy app operational while the modern host is introduced. The shared database must be treated as live during the overlap, so the scaffold task needs the shared-database schema safety discipline from the start.

**Done when**: a new ASP.NET Core project exists in the solution, it builds, and the legacy app still remains available alongside it.

---

### 03-migrate-cardgrid-web: Move the web application into the new host #skill:managing-shared-database-schema

Port the CardGrid web application surface into the new ASP.NET Core host. This includes the controllers, views, startup behavior, routing, static assets, and data access patterns needed for the app to run on .NET 10 while the legacy site remains in place. The database remains shared during the migration window, so schema-touching work must stay additive and coordinated.

**Done when**: the migrated web app builds and runs in the new host, the legacy app is still intact, and the migration path is documented as a side-by-side transition.

---

### 04-convert-cardgrid-tests: Convert the test project to SDK-style on the current framework

Convert the CardGridTests project from the legacy project system to SDK-style without changing its target framework yet. This isolates the structural project-system change from the target framework change and prepares the test project for the later retargeting step.

**Done when**: the test project is SDK-style, still builds on its current framework, and the test assembly continues to load correctly in the solution.

---

### 05-upgrade-cardgrid-tests: Retarget the tests to .NET 10 and verify behavior

Retarget CardGridTests to .NET 10 and adjust its references so it tests the migrated Core host rather than the old framework app. Resolve any compile or test failures introduced by the framework change, including the placeholder failing test, so the test project becomes a valid validation target for the upgraded solution.

**Done when**: the test project targets .NET 10, builds successfully, and the test suite passes against the migrated web host.

---

### 06-final-validation: Validate the upgraded solution end to end

Run the full solution build and test pass after the migration work is complete. Confirm the new host, the updated tests, and the remaining legacy project state are all coherent, and document any post-upgrade cleanup the user still needs to perform manually.

**Done when**: the solution builds cleanly, tests pass, and the remaining legacy cleanup items are documented for the user.
