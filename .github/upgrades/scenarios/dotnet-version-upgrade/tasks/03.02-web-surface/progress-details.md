# 03.02-web-surface Progress

- Ported `Employee`, `GridResponse<T>` and `CardGridContext` into `CardGrid.Core`, keeping the legacy `CardGrid.Models` / `CardGrid.Database` namespaces so both hosts resolve the same EF6 model and context key on the shared database.
- EF6 6.5.2 now runs on `Microsoft.EntityFramework.SqlServer` (`Microsoft.Data.SqlClient`) via `MicrosoftSqlDbConfiguration`, replacing the deprecated `System.Data.SqlClient` provider.
- Removed the per-request `COUNT` + seed from the context constructor. Startup initialization is explicit and opt-in (`Database:CreateIfMissing`, `Database:SeedOnStartup`, enabled only in `appsettings.Development.json`). The seed runs as one transaction under `UPDLOCK, HOLDLOCK`, so the two hosts cannot both seed. Shared-database rule: schema work is limited to creating a missing database. Existing schema is never altered.
- Moved the search/sort/page logic into `EmployeeGridService` behind `IEmployeeStore`. `DefaultController` keeps its routes (`/`, `/Default/Index`, `/Default/GetData`), and the JSON keeps the legacy member names.
- Ported views to Bootstrap 5 (user decision) and ported `cardgrid.ts` off Bootstrap 3 classes. Fixed stored XSS (data rendered as text). Dates are parsed from ISO-8601.
- Client assets are vendored under `wwwroot/lib` (Bootstrap 5.3.8, jQuery 3.7.1, Font Awesome 4.7, bootpag 1.0.7).
- The YARP fallback is mapped only when `ProxyTo` is set.
- Deleted the 0-byte `App_Data/CardGridDB.mdf`/`.ldf` placeholders created in task 02. They blocked database creation with SQL error 5170 for both hosts.
- Validation: the Core host was started in Development and created and seeded LocalDB (1000 rows). `/Default/GetData` paging, sorting, search and clamping were exercised over HTTP. In Chrome, the card view, table view, pagination and the dropdown all rendered and worked. `MSBuild CardGrid.sln` succeeded with 0 warnings.
