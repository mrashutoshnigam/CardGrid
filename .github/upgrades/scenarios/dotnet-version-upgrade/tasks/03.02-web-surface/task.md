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

## Steps
1. Analyze the legacy controller, view, and DbContext dependencies and mirror the behavior in the Core host.
2. Port the controller, model, and EF6 access code into the new project with any ASP.NET Core adjustments needed for compilation and runtime.
3. Move or recreate the Razor views and supporting assets so the default page renders in the new host.
4. Validate that the migrated surface builds and that the legacy project remains intact.

## Done when
- The migrated controller/view path is present in the Core host.
- The data access layer compiles in the new host and still respects the shared-database constraints.
- The solution builds successfully with both hosts present.
