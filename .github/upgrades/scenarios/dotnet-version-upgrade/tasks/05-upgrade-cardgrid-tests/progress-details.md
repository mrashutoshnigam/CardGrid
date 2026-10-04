# 05-upgrade-cardgrid-tests Progress

- Retargeted to `net10.0` with `MSTest` 4.4.1 and `Microsoft.AspNetCore.Mvc.Testing` 10.0.12. It now references `CardGrid.Core` instead of the legacy app.
- Replaced the placeholder with 64 tests:
  - grid service: search, sort whitelist with tie-breaker, paging, clamping, overflow
  - controller action results
  - `GridResponse` page math
  - seed script integrity
  - `WebApplicationFactory` integration tests for routes, static assets and the JSON contract
- No test needs SQL Server; an in-memory `IEmployeeStore` stands in for the database.
- Validation: `dotnet test CardGridTests` passed 64 of 64.
