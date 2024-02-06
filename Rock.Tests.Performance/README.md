
# Performance Tests
This project is intended to store console applications and test harnesses that measure the performance of various Rock components.

## Setup
This project requires a connection to a Rock database, initialized with the standard Rock sample data.

This project follows the same database access pattern as the Rock.Tests.Integration project, so please see the Readme file there for details on configuring your local database.

### Lava Performance Test Console
This console is used to measure and compare the performance of the various Liquid frameworks which can be used by the Rock Lava library.
It is useful for comparing the performance of existing engines, and also for establishing performance benchmarks before and after critical changes to a specific engine.

Some things to note about this test console:

1. It's important to verify that the test templates render correctly in all of the engines being tested before using them to establish a benchmark.
The _showFirstRenderResult flag can be set to send the rendered result of each test template to the console output.
2. The _templateCachingEnabled flag can be used to disable caching for Lava library engines such as Fluid, but it cannot disable caching for RockLiquid.
Therefore, caching should only be disabled if the performance result is being compared to anoter Lava library engine.
