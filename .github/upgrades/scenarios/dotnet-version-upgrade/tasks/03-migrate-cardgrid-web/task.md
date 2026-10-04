# 03-migrate-cardgrid-web: migrate-cardgrid-web: Move the web application into the new host #skill:managing-shared-database-schema

Port the CardGrid web application surface into the new ASP.NET Core host. This includes the controllers, views, startup behavior, routing, static assets, and data access patterns needed for the app to run on .NET 10 while the legacy site remains in place. The database remains shared during the migration window, so schema-touching work must stay additive and coordinated.

**Done when**: the migrated web app builds and runs in the new host, the legacy app is still intact, and the migration path is documented as a side-by-side transition.
