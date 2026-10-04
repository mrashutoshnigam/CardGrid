# 04-convert-cardgrid-tests Progress

- Merged with 05 at the user's request. The legacy test project held only an `Assert.Fail()` placeholder, so an intermediate SDK-style net48 build would not have checked anything.
- `CardGridTests.csproj` is now SDK-style. `packages.config`, `app.config` and `Properties/AssemblyInfo.cs` were removed.
