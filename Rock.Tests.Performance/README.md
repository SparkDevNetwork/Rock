# Benchmarks

This project contains a set of benchmarks that can be run from one version to the next to determine if performance has changed.
While these are "unit tests", they are not run as standard XUnit or MSTest unit tests.
They are run by the BenchmarkDotNet package which handles all the boilerplate to make sure the resulting numbers are valid over a large set of runs.

Currently, these benchmarks are only run as needed by hand.
In the future, we might automate this to be a nightly run and build charts of the results to more quickly spot performance issues.
When that happens, we will probably use a docker image for the database which will mean all these performance tests will need to work with "out of the box" database content.

# Organization

All benchmark classes should be placed under the Benchmarks folder and then into the proper Rock Domain folder.

# Database Access

While benchmarks might require a valid database in order to get initial data or cache populated, the actual test run should not require or generate any database access.
Hitting the database during the benchmark would skew the results and give us data that really can't be used to compare to previous runs.

As an example of this, the Fluid tests require access to the database to load some defaults from global attribute cache.
The first time the cache is hit it loads from the database, but subsequent calls will just use the in-memory cache.
So we first force the cache to populate in the GlobalSetup method.

When building new benchmark logic, a good way to make sure that the database does not get hit is to run the SQL Server Profile at the same time.
With how many iterations the benchmark will run it should be easy to spot any unexpected database hits.

If you need to write benchmarks against the database - like testing the performance of the Person Search methods - then place those in the Rock.Tests.Integration.Performance project.
