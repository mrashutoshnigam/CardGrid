# 03.02-web-surface: Port controllers, views, and EF6 data access #skill:managing-shared-database-schema

## Objective
Move the CardGrid web surface into the new ASP.NET Core host: controller actions, Razor views, models, and EF6 data access. Preserve the current behavior of the grid page and JSON endpoint while the legacy app remains in place.

## Scope
- `CardGrid\Controllers\DefaultController.cs`
- `CardGrid\Database\CardGridContext.cs`
- `CardGrid\Models\Employee.cs`
- `CardGrid\Models\GridResponse.cs`
- `CardGrid\Views\Default\Index.cshtml`
- `CardGrid\Views\Shared\_Layout.cshtml`
- `CardGrid\Views\_ViewStart.cshtml`
- Shared static assets used by the page if the new host needs them

## Scope Inventory
- Projects affected: legacy `CardGrid` web project and the new `CardGrid.Core` host
- Distinct concerns: controller porting, EF6 data access, Razor view/layout migration, static asset parity, endpoint routing parity
- Change signals: one MVC/Web API controller, one EF6 `DbContext`, one JSON endpoint, one view entry point, shared layout, jQuery/Bootstrap-heavy client-side behavior
- Skill matches: ASP.NET Framework → ASP.NET Core migration, controller/action-result migration, shared-database discipline, build validation

## Research Findings
- `DefaultController` is an MVC controller returning a view and a JSON payload from `GetData`, so the ASP.NET Core version should stay on `Controller` rather than switch to `ControllerBase`.
- `CardGridContext` is an EF6 `DbContext` with constructor-time seeding logic; the new host needs the same context behavior preserved without introducing startup migrations.
- `GridResponse<T>` and `Employee` are plain model types and can move across without framework coupling.
- `Index.cshtml` depends on `Url.Action("GetData")`, a jQuery card-grid plugin, and Bootstrap/Font Awesome assets.
- `_Layout.cshtml` includes `cardgrid.css`, Bootstrap, Font Awesome, jQuery, and the custom `CardGrid.js` script.
- `RouteConfig` maps the default route to `Default/Index`; `WebApiConfig` exposes the old `api/{controller}/{id}` route. These routes are the behavior baseline for the new host.
- The controller dependency graph shows the only non-project dependencies are MVC and EF6; no additional service container or auth dependencies surfaced in the current surface.

## Steps
1. Analyze the legacy controller, view, and DbContext dependencies and mirror the behavior in the Core host.
2. Port the controller, model, and EF6 access code into the new project with any ASP.NET Core adjustments needed for compilation and runtime.
3. Move or recreate the Razor views and supporting assets so the default page renders in the new host.
4. Validate that the migrated surface builds and that the legacy project remains intact.

## Done when
- The migrated controller/view path is present in the Core host.
- The data access layer compiles in the new host and still respects the shared-database constraints.
- The solution builds successfully with both hosts present.
