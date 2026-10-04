# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Pace**: Standard
- **Target Framework**: .NET 10 (LTS)

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options

### Strategy
- **Upgrade Strategy**: Bottom-Up (Dependency-First)

### Project Structure
- **Project Approach**: Side-by-side
- **Package Management**: Per-Project (defer CPM to post-migration)

### Compatibility
- **Unsupported Packages**: Defer Resolution
- **System.Web Adapters**: Use System.Web Adapters

### Modernization
- **Assembly Binding Redirects**: Remove Binding Redirects
- **Entity Framework**: Keep EF6
- **Nullable Reference Types**: Leave Disabled

## Strategy
**Selected**: Bottom-Up (Dependency-First)
**Rationale**: The solution has two .NET Framework 4.8 projects and a classic ASP.NET web app, so migration must proceed dependency-first with the web tier handled as a side-by-side migration.

### Execution Constraints
- Strict tier ordering: validate each tier before moving upward.
- Keep SDK-style conversion separate from TFM upgrade.
- Side-by-side web migration injects scaffold and migrate tasks.
- The legacy web project remains old-style and deployable throughout the transition.
- Treat the database as shared between the old and new web hosts; schema-touching tasks must carry the shared-database skill pin.

## Custom Instructions
- For any task touching the shared database, include `#skill:managing-shared-database-schema` in the task description.
