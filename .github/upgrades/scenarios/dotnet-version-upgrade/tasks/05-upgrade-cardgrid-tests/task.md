# 05-upgrade-cardgrid-tests: upgrade-cardgrid-tests: Retarget the tests to .NET 10 and verify behavior

Retarget CardGridTests to .NET 10 and adjust its references so it tests the migrated Core host rather than the old framework app. Resolve any compile or test failures introduced by the framework change, including the placeholder failing test, so the test project becomes a valid validation target for the upgraded solution.

**Done when**: the test project targets .NET 10, builds successfully, and the test suite passes against the migrated web host.
