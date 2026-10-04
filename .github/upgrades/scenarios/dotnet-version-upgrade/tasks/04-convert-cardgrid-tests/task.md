# 04-convert-cardgrid-tests: convert-cardgrid-tests: Convert the test project to SDK-style on the current framework

Convert the CardGridTests project from the legacy project system to SDK-style without changing its target framework yet. This isolates the structural project-system change from the target framework change and prepares the test project for the later retargeting step.

**Done when**: the test project is SDK-style, still builds on its current framework, and the test assembly continues to load correctly in the solution.
