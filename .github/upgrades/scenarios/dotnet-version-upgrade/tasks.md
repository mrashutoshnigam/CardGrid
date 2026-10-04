# Migration Progress

**Progress**: 3/8 tasks complete <progress value="38" max="100"></progress> 38%
**Status**: In Progress - Task 03-migrate-cardgrid-web

## Tasks

- ✅ 01-prerequisites: prerequisites: Validate upgrade readiness and migration constraints ([Content](tasks/01-prerequisites/task.md), [Progress](tasks/01-prerequisites/progress-details.md))
- ✅ 02-scaffold-cardgrid-web: scaffold-cardgrid-web: Create the ASP.NET Core side-by-side host #skill:managing-shared-database-schema ([Content](tasks/02-scaffold-cardgrid-web/task.md), [Progress](tasks/02-scaffold-cardgrid-web/progress-details.md))
- 🔄 03-migrate-cardgrid-web: migrate-cardgrid-web: Move the web application into the new host #skill:managing-shared-database-schema ([Content](tasks/03-migrate-cardgrid-web/task.md))
  - ✅ 03.01-host-config: Set up the ASP.NET Core host and routing ([Content](tasks/03.01-host-config/task.md), [Progress](tasks/03.01-host-config/progress-details.md))
  - 🔲 03.02-web-surface: Port controllers, views, and EF6 data access #skill:managing-shared-database-schema
- 🔲 04-convert-cardgrid-tests: Convert the test project to SDK-style on the current framework ([Content](tasks/04-convert-cardgrid-tests/task.md))
- 🔲 05-upgrade-cardgrid-tests: Retarget the tests to .NET 10 and verify behavior ([Content](tasks/05-upgrade-cardgrid-tests/task.md))
- 🔲 06-final-validation: Validate the upgraded solution end to end ([Content](tasks/06-final-validation/task.md))

**Legend**: ✅ Complete | 🔄 In Progress | 🔲 Pending | ⚠️ Blocked | ❌ Failed
