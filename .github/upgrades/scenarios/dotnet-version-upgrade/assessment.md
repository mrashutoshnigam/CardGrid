# Assessment

## Summary
- The solution contains two legacy .NET Framework 4.8 projects:
  - `CardGrid\CardGrid.csproj` — classic ASP.NET MVC/Web API application
  - `CardGridTests\CardGridTests.csproj` — .NET Framework test project referencing the web app
- The recommended target from the upgrade options is **.NET 10 (LTS)**.
- This project cannot be retargeted to .NET 10 in place as-is. It uses the legacy `System.Web` ASP.NET stack, MVC 5, Web API 2, WebPages, and `Global.asax`/`Web.config` hosting, which are not supported on ASP.NET Core/.NET 10.

## Primary Findings

### Legacy web stack blocks direct retargeting
`CardGrid\CardGrid.csproj` is a classic web application project:
- Uses `ProjectTypeGuids` for ASP.NET web app support.
- Targets `v4.8`.
- Imports `Microsoft.WebApplication.targets`.
- References `System.Web`, `System.Web.Mvc`, `System.Web.Http`, `System.Web.WebPages`, and related legacy assemblies.
- Includes `Global.asax`, `Web.config`, `App_Start\RouteConfig.cs`, and `App_Start\WebApiConfig.cs`.

### Package set is legacy ASP.NET Framework-oriented
`packages.config` for the app includes:
- `Microsoft.AspNet.Mvc 5.3.0`
- `Microsoft.AspNet.WebApi 5.3.0`
- `Microsoft.AspNet.WebPages 3.3.0`
- `Microsoft.CodeDom.Providers.DotNetCompilerPlatform 4.1.0`
- `EntityFramework 6.5.2`
- client-side assets (`bootstrap`, `jQuery`, `Modernizr`, `FontAwesome`)

These packages are compatible with .NET Framework 4.8, but the ASP.NET MVC/Web API pieces are not part of the ASP.NET Core stack.

### Configuration is framework-specific
`CardGrid\Web.config` contains:
- `system.web` compilation/httpRuntime settings for .NET Framework
- `system.webServer` handlers for the classic ASP.NET pipeline
- `system.codedom` compiler provider configuration
- EF6 configuration and binding redirects

This configuration is not directly portable to a net10 ASP.NET Core app.

### Code-behind and data access depend on `System.Web` + EF6
Relevant source patterns found in the app:
- `Global.asax.cs` uses `System.Web.*` and `GlobalConfiguration.Configure(...)`
- `DefaultController.cs` uses `Controller`, `ActionResult`, `JsonResult`, and `System.Web.Mvc`
- `CardGridContext` derives from `DbContext` and seeds data in the constructor
- Models include legacy `using System.Web;` imports

### Test project also targets .NET Framework
`CardGridTests\CardGridTests.csproj` is also `v4.8` and references the web app project. It carries a large set of `Microsoft.*` and `System.*` packages, but it is still built around the .NET Framework test runner and the legacy app project.

## Risks / Blockers
- **Direct in-place upgrade to net10 is blocked** by the legacy ASP.NET stack.
- **API surface migration required**: MVC 5/Web API 2 controllers, routes, and startup need an ASP.NET Core equivalent.
- **Hosting model change required**: `Global.asax` and `Web.config` startup must be replaced with `Program.cs`/ASP.NET Core configuration.
- **Data layer review needed**: EF6 may be retained short term, but EF Core should be evaluated during the migration.
- **Test migration required**: tests need to move to a .NET 10-compatible test project and adapt to the new web host.

## Recommended Approach
1. Treat the current project as the **legacy source** rather than a direct framework-retarget candidate.
2. Create a new ASP.NET Core web app targeting **.NET 10**.
3. Port controllers, routes, views, static assets, and EF/data access incrementally.
4. Move the test project to a .NET 10-compatible test setup after the application surface is available.
5. Keep the legacy app functional during the transition if side-by-side migration is needed.

## Package Notes
- `EntityFramework 6.5.2` can remain only if the migration keeps EF6 for a while; otherwise plan an EF Core move.
- Legacy ASP.NET packages (`Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi`, `Microsoft.AspNet.WebPages`) should not be carried into the new net10 app.
- Client-side packages (`bootstrap`, `jQuery`, `FontAwesome`) can be reused, but the asset wiring will change under ASP.NET Core.
