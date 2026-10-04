**Progress**: 2/6 tasks complete <progress value="33" max="100"></progress> 33%
**Status**: Not Started
**Progress**: 1/6 tasks complete <progress value="17" max="100"></progress> 17%
**Status**: In Progress - Task 02-scaffold-cardgrid-web
Upgrade the CardGrid solution from .NET Framework 4.8 to .NET 10 by introducing a side-by-side ASP.NET Core web host, migrating the legacy web app incrementally, and then moving the dependent test project onto the modern target framework.
- ✅ 01-prerequisites: prerequisites: Validate upgrade readiness and migration constraints ([Content](tasks/01-prerequisites/task.md), [Progress](tasks/01-prerequisites/progress-details.md))
- 🔄 01-prerequisites: prerequisites: Validate upgrade readiness and migration constraints ([Content](tasks/01-prerequisites/task.md))

- ✅ 02-scaffold-cardgrid-web: scaffold-cardgrid-web: Create the ASP.NET Core side-by-side host #skill:managing-shared-database-schema ([Content](tasks/02-scaffold-cardgrid-web/task.md), [Progress](tasks/02-scaffold-cardgrid-web/progress-details.md))

- 🔲 01-prerequisites: Validate upgrade readiness and migration constraints
- 🔲 02-scaffold-cardgrid-web: Create the ASP.NET Core side-by-side host
- 🔲 03-migrate-cardgrid-web: Move the web application into the new host
- 🔲 04-convert-cardgrid-tests: Convert the test project to SDK-style on the current framework
- 🔲 05-upgrade-cardgrid-tests: Retarget the tests to .NET 10 and verify behavior
- 🔲 06-final-validation: Validate the upgraded solution end to end
