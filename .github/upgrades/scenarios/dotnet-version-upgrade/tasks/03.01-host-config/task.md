# 03.01-host-config: Set up the ASP.NET Core host and routing

## Objective
Establish the new ASP.NET Core host structure for CardGrid so the migrated app can run beside the legacy web app. This includes the Core project configuration, application startup, proxy/default routing, and the baseline static-file/runtime wiring needed before feature code is moved across.

## Scope
- `CardGrid.Core\CardGrid.Core.csproj`
- `CardGrid.Core\Program.cs`
- `CardGrid.Core\appsettings*.json`
- `CardGrid.Core\Properties\launchSettings.json`
- Existing legacy routing in `CardGrid\App_Start\RouteConfig.cs` and `CardGrid\App_Start\WebApiConfig.cs` for behavior parity

## Scope Inventory
- Projects affected: `CardGrid.Core\CardGrid.Core.csproj` and the legacy `CardGrid` web project for routing parity review
- Distinct concerns: host startup, endpoint routing, proxy forwarding, static-file startup, and solution-level build validation
- Change signals: side-by-side migration, YARP proxy host, default MVC route on the old app, and a legacy `System.Web` app that must continue running during the transition
- Skill matches: ASP.NET Framework → ASP.NET Core migration, MVC routing conversion, and build validation

## Research Findings
- The scaffolded Core host already targets `net10.0` and uses `Microsoft.NET.Sdk.Web`.
- `Program.cs` already wires the host for this phase: forwarding, `UseRouting`, `UseStaticFiles`, `UseAuthentication`, `UseAuthorization`, `UseSystemWebAdapters`, `MapDefaultControllerRoute`, and a catch-all forwarder to the legacy app URL.
- `appsettings.json` already carries the `ProxyTo` setting and fail-closed forwarded-header defaults.
- `launchSettings.json` points the new host at `http://localhost:61061`, the legacy app's local URL.
- The old app's routing surface is conventional MVC/Web API (`RouteConfig.RegisterRoutes` plus `WebApiConfig.Configure`), so the Core host only needs the default MVC route and proxy fallback at this stage.
- No additional host-config edits were required beyond the scaffolded setup.

## Validation Notes
- The solution was already rebuilt successfully during the scaffold task after the Core host was added.
- No code changes were necessary for this task because the scaffolded host already satisfied the host/routing requirements.

## Steps
1. Align the Core host configuration with the existing scaffold and the legacy app URL.
2. Wire startup, routing, proxying, and static files so the host can accept migrated endpoints while still forwarding unmatched traffic.
3. Validate that the Core host still builds in the solution after the startup changes.

## Done when
- The Core host starts with the intended routing and proxy behavior.
- The project builds cleanly in the solution.
- No legacy-only startup concepts remain in the new host configuration.
