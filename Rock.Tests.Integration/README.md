# The Integration Tests Project
The goal of this project is to be a permanent place to store tests that require things such as a database context to fetch data, configuration settings, save data, etc.  At the moment, you (the developer) will run these tests when needed, but ultimately our automated build system (AppVeyor) will automatically run all these tests after every commit (or push to a particular branch). 

**For general information about the Rock Test projects, refer to the README file in the Rock.Tests.UnitTests project.**

# Configuring the Test Environment
Settings for the test environment are configured in local `*.config` files.
The `app.TestSettings.config` file stores file locations, test account details, and other configuration parameters relevant to executing tests.
The `app.ConnectionStrings.config` file stores connection strings for the test databases.

These files are not subject to source control, but they will automatically be created from the example files when the project is first built. You do not need to modify these files unless you need to target your local database.

## Test Environment Configuration

### DataEncryptionKey
Sets the key used to encrypt/decrypt data in the Rock test database.
The key must match the value used to create the database.

### PasswordKey
Sets the key used to encrypt/decrypt passwords stored in the Rock test database.
The key must match the value used to create the database.

## Test Database Configuration

### DatabaseHost Setting
This setting determines the type of container that will be used for the test database.
1. **docker** - (default) uses a Docker container hosting a SQL Server image.
   Configure and manage Docker containers using the Docker Desktop application.
   Docker images provide a reliable means of testing against different database versions.
   The docker image is best suited to executing a full suite of tests.
2. **localdb** - uses an MSSQLLocalDB instance to host the test database.
   Database images are stored in local file archives.
   Configure and manage LocalDB databases from Visual Studio or SQL Server Management Studio.
   This type of container supports multiple databases, each with its own dataset. The database for the current test run can be specified.
3. **remote** - uses a remote database server; these databases are not created or replaced automatically.

### SampleDataUrl
This setting specifies the URL from which the Rock sample data can be downloaded.

### DatabaseInitializer
This setting specifies a `System.Type` name of a class that is capable of initializing the data for a new test database instance.
The initializer is executed once when a new test database image is built, and the database image is then archived and reused for each test run.
This eliminates the need to repopulate large test datasets for each test execution.

These initializers are currently available:
1. **BasicDataset** - an empty initializer that results in a new Rock database containing only basic configuration data.
2. **SampleDataset (default)** - adds the default set of Rock sample data.

The intention is that other initializers will be added in the future, targeted toward testing specific Rock feature sets.
For example, a CommunicationDataset could provide a variety of email and SMS communications for testing communication features.

Note that the testing framework also provides methods for creating specific bulk data, and these can be executed at any time during the testing process.
However, if these operations are expensive they should be packaged into an initializer so that the test data is included
in the base snapshot and does not need to be recreated for each test run.

### ExternalIntegrationsEnabled
This flag specifies if tests that depend on external resources should be executed or ignored.
The option should only be enabled when testing features that require correctly configured resources or services that are external to Rock.
If not specified or set to "false", tests that require external resources will return a result of Inconclusive.

### UtilityTestActionsEnabled
This flag specifies if tests that are intended to be perform utility actions should be available for execution or ignored.
These tests are intended for manual execution during development, and the flag should be disabled for automated test runs.
If not specified or set to "false", tests that perform utility actions will return a result of Inconclusive.

## Test Database Configuration (LocalDb Only)

###  DatabaseCreatorId
A name that is embedded in databases created by the integration tests project, to distinguish them from other LocalDB databases on your system.
The name is stored in the Rock System Settings Key `com.rockrms.test.DatabaseCreator`.

### LocalDbResetMigrationName
This setting specifies an optional migration to which the test database should be reset.
If not specified, the most recent migration is restored.
This can be useful for testing new migrations.

### LocalDbArchiveRetentionDays
This setting specifies the number of days after which a database image will be deleted from the local file system.
If not specified or set to "0", database images will be retained indefinitely.

### LocalDbResetDatabaseName
This setting specifies the name of the database that can be reset to a default image during test executions.
If a different target name is specified in the database connection string, database resets will be disabled.
This operates as a safety mechanism to prevent accidental database overwrites, and it may be useful for testing during the development process
to prevent the loss of test data that has been manually added.

# Managing the Test Database
The majority of tests in the integration test project require a Rock database populated with the appropriate version of the Rock sample data.
The currently configured database container manages the process of creating, archiving and restoring snapshots of the test database.

In order for tests to run in a predictable environment, the initial database state is restored for each set of tests.
When a test set is executed, the container locates a snapshot matching the current configuration in which the tests are executing.
The snapshot is tagged with the current migration number of the Rock instance, and the identifier of the specified `DatabaseInitializer`.
If the snapshot exists, it is restored and testing proceeds.
If the snapshot is not found, it is automatically created. This process can take a significant amount of time (15 - 25 minutes, depending on environment) as it also installs the sample data.
Once the base image is built it will be used whenever a clean database is required.

By default, each test suite (C# class) will get a new database instance for all tests contained in that suite.
Usually this is sufficient as those tests are generally related and expect the same test data.
When that is not the case and your individual tests each require a clean environment, you can apply the `[IsolatedDatabase]` attribute to the test method for whichever tests want a clean environment. You can also put that attribute on the class and each test will get a clean database.

Whenever possible a test should put the database back the way it was found. This is not a hard requirement - for example, you don't need to hunt down any History records that might have been created. But if you change Ted's birth date for the test, change it back after the test finishes.

## Configuring Docker Desktop
[Docker Desktop](https://www.docker.com/products/docker-desktop/) is simple to install on a Windows PC.

However, configuring Docker to run on a Virtual Machine requires some additional configuration, because Docker is a virtualization tool itself.
General advice on how to run Docker Desktop in a virtualized environment can be found [here](https://docs.docker.com/desktop/vm-vdi/).

To configure Docker for a Hyper-V virtual machine, follow the more specific instructions [here](https://learn.microsoft.com/en-us/virtualization/hyper-v-on-windows/user-guide/enable-nested-virtualization).

## Configuring LocalDb
LocalDB is a minimal instance of SQL Server Express that is specifically targeted for local development and testing.
This database container is best suited to managing non-standard database configurations, such as when testing a feature
that requires a specific dataset or a very large number of randomized records.

For more information about installing and configuring LocalDB, refer to the documentation [here](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16).

The LocalDb instance can be managed through the SQL Server Object Explorer window in Visual Studio.
Select "Add SQL Server" and open the "Local" node of the server browser to view LocalDB instances on your machine.

## Using a Development Database
There may be times when it is necessary or desirable to preserve your test data from one test run to another. This is particularly true during the initial development of a new feature where test data is being added throughout the development process.
In that case, be sure to set the DatabaseRefreshStrategy configuration option to "never", to prevent your database from being replaced by a new sample database at the start of each test run.

## Test LifeCycle
The following decorators can give you more control over setup and cleanup for your test suite:

* [AssemblyInitialize] - called once before running the tests of the assembly. 
* [AssemblyCleanup] - called after all tests of the assembly are executed.
* [ClassInitialize] - called once before running the tests of the class.
* [ClassCleanup] - called after all tests from all classes finish.
* [TestInitialize] - called before running each test of the class. 
* [TestCleanup] - called after running each test of the class.

# Running a Test
To run or debug a test, simply right-click the class name and choose `Run Tests` or `Debug Tests` -- but you should probably set a breakpoint in your test if you're going to select Debug Tests.  Alternatively you can choose the Test > Windows > Test Explorer from the menu to run tests a bit easier.

## Timings
The very first initial run, meaning where it has to generate the database image, may take around 15 - 20 minutes.
Once that image is created, it takes about 5 seconds to bring up a clean database.

# Designing Tests
Many of these test rules are taken from the following best practices document prepared by Microsoft for MSTest:
https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

### Test Project Structure

Tests are organized into the following main categories:
* Modules - tests that verify the features of Rock, further subdivided by the application domain.
* ThirdPartyIntegrations - test that specifically verify an integration with a third party service or library.
* Issues - tests that verify specific bugfixes.

## Naming Tests
1. Test Names should use the following naming convention:
    * MethodOrComponentName_TestConditionOrScenario_ExpectedResult.

    Examples:
    * Add_SingleNumber_ReturnsSameNumber
    * ExecuteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage

    Following this pattern makes it easier to identify and locate tests, and also provides an indication of the scenario in which the test is failing.

2. Test classes should be placed in the namespace corresponding to the Rock module or feature to which they are related.
For example, "Rock.Tests.Integration.Modules.Engagement.Steps" contains the tests related to classes in Rock.Engagement.Steps namespace.


## Test Setup/Teardown
Groups of related tests should be repeatable where it is possible to do so.
Each group of tests is responsible for creating and removing any additional data needed for testing.

It is critical that the complete set of the tests in the integration test suite can be executed from start to finish with a new sample database.
When adding or modifying tests, be sure to verify that the test suite is able to run correctly to completion.

### Assert

Tests in this project use the "Assert.That.[Assertion]" pattern in preference to the standard "Assert.[Assertion]" pattern.
eg. "Assert.That.AreEqual(a, b);" is preferred to "Assert.AreEqual(a, b);

This is because "Assert.That" allows a consistent syntax for accessing both the standard MSBuild Assert methods and any custom extension methods we have added to extend the Assert functionality as needed.

## Other Rules

Tests...

1. must have at least one Assert.  ("No Assert, then it's not a test.")
If the test is intended to measure performance rather than test for valid operation, consider adding it to the Rock.Tests.Performance project instead.
2. must test only one thing. However, you can Assert multiple things about that test to prove it's true and identify the exact point of failure.
3. must not depend on the order that tests are run.
4. should not be overly complex with many layers. (KISS principle)
5. shall not write to hard-coded folders. (C://foo/...)
6. should always be able to run in a CI/CD (AppVeyor) environment and without specific/manual setup**.  (Otherwise mark manual tests as [Ignore])

>     **Human only run-able tests (tests that require some specific environment or environment access) should be in a separate test project and class.

## Additional Information

### MS Test

> If you're interested in a comparison of the three most popular test frameworks, see this chart: https://xunit.net/docs/comparisons.html

This project uses the [Microsoft unit test framework (aka MS Test)](https://docs.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code).  You can read more about it in their [Walkthrough](https://docs.microsoft.com/en-us/visualstudio/test/walkthrough-creating-and-running-unit-tests-for-managed-code) but it's pretty simple.  You create test classes decorated with `[TestClass]` and individual unit test methods decorated with `[TestMethod]`.

Example:
```csharp
namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class AttendanceCodeTests
    {
        [TestMethod]
        public void Increment100SequentialNumericCodes()
        {
            AttendanceCode code = null;
            for ( int i = 0; i < 100; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
            }

            Assert.That.AreEqual( "100", code.Code );
        }
    }
}
```
