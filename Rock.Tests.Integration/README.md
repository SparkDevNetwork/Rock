# The Integration Tests Project
The goal of this project is to be a permanent place to store tests that require things such as a database context to fetch data, configuration settings, save data, etc.  At the moment, you (the developer) will run these tests when needed, but ultimately our automated build system (AppVeyor) will automatically run all these tests after every commit (or push to a particular branch). 

** For general information about the Rock Test projects, refer to the README file in the Rock.Tests.UnitTests project.

## Configuring the Test Environment
Configuration settings for the test environment are stored in the `integrationtests.runsettings` file.

### Test Database Configuration

**SampleDataUrl**
This setting specifies the URL from which the Rock sample data can be downloaded.
**DatabaseCreatorId**
A name that is embedded in databases created by the integration tests project, to distinguish them from other LocalDB databases on your system.
The name is stored in the Rock System Settings Key `com.rockrms.test.DatabaseCreator`.
**DatabaseRefreshStrategy**
Set this option to determine if and when the integration test runner can replace existing databases on your database server.
*Force*
The local database will always be overwritten if it exists.
*Verified*
The local database will be overwritten if it exists, but only if the DatabaseCreatorId matches the identifier set for the integration tests project. This prevents databases created by other applications from being deleted.
This is the recommended setting for common use cases.
*Never*
The local database will never be overwritten. If the database exists, it will be used without modification.
This strategy is recommended when developing new features and manually creating test data, to prevent the new data from being overwritten when your integration tests are executed.

## Managing the Test Database
The majority of tests in the integration test project require a Rock database populated with the appropriate version of the Rock sample data.

The test database can be remote or local.

When the test project is executed, a test database for the current Rock project version is automatically created and populated with sample data if it does not already exist.
A snapshot of the database is stored in the local machine temp directory, named after the most recent migration (eg. `Snapshot-202209091915177_PersonNoteColors.zip`)

The name of the database identifies the Rock version for which it was created.
If a database snapshot that matches the Rock project version already exists, it will be restored and used for the current test run.
Using this method, the database state can be quickly restored to its initial state at any time during a test run.

A snapshot of the database is created for the current Rock version
If the database exists, it will not be replaced. However, any unapplied migrations will be run for the database to ensure it is compatible with the current test project.
The process of verifying and creating or initializing the database is performed by the [AssemblyInitialize] method when one or more tests are executed.

Tests can and should use the sample data where it is sufficient for the purpose, but should not make changes to the data in any way because this may disrupt other tests that rely on it.
Any custom data required by a group of tests should be created in the related [ClassInitialize] method and removed (where necessary) in the [ClassCleanup] method.
If the data is not removed, be sure that it is not added multiple times during subsequent test executions.

### Using a LocalDB Database
When executing tests in this project using a standard test configuration, a new database is created containing the standard Rock sample data set.
The database server is configured to use LocalDB and database instances are managed automatically by the testing framework, so no configuration is necessary.

LocalDB is a minimal instance of SQL Server Express that is specifically targeted for local development and testing.
For more information about installing and configuring LocalDB, refer to the documentation.
https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16).

The LocalDb instance can be managed through the SQL Server Object Explorer window in Visual Studio.
Select "Add SQL Server" and open the "Local" node of the server browser to view LocalDB instances on your machine.


To recreate the default LocalDb database:
SqlLocalDb delete MSSQLLocalDB.

### Using a Development Database
There may be times when it is necessary or desirable to preserve your test data from one test run to another. This is particularly true during the initial development of a new feature where test data is being added throughout the development process.
In that case, be sure to set the DatabaseRefreshStrategy configuration option to "never", to prevent your database from being replaced by a new sample database at the start of each test run.

### Test LifeCycle

The following decorators can give you more control over setup and cleanup for your test suite:

* [AssemblyInitialize] - called once before running the tests of the assembly. 
* [AssemblyCleanup] - called after all tests of the assembly are executed.
* [ClassInitialize] - called once before running the tests of the class.
* [ClassCleanup] - called after all tests from all classes finish.
* [TestInitialize] - called before running each test of the class. 
* [TestCleanup] - called after running each test of the class.

## Running a Test
To run or debug a test, simply right-click the class name and choose `Run Tests` or `Debug Tests` -- but you should probably set a breakpoint in your test if you're going to select Debug Tests.  Alternatively you can choose the Test > Windows > Test Explorer from the menu to run tests a bit easier.

### Timings
The very first initial run, meaning where it has to generate the archive, may take around 4 minutes.
Once that archive is created, the very first call to ResetDatabase() takes about 30 seconds (this seems to be due to the fact it has to load a bunch of additional DLLs into memory).
Subsequent calls to ResetDatabase() take about 6 seconds.

## Designing Tests
Many of these test rules are taken from the following best practices document prepared by Microsoft for MSTest:
https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

### Test Project Structure

Tests are organized into the following main categories:
* Modules - tests that verify the features of Rock, further subdivided by the application domain.
* ThirdPartyIntegrations - test that specifically verify an integration with a third party service or library.
* Issues - tests that verify specific bugfixes.

## Naming Tests
1. Test Names should use the following naming convention:
MethodOrComponentName_TestConditionOrScenario_ExpectedResult.
Examples:
Add_SingleNumber_ReturnsSameNumber
ExecuteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage

Following this pattern makes it easier to identify and locate tests, and also provides an indication of the scenario in which the test is failing.

2. Test classes should be placed in the namespace corresponding to the Rock module or feature to which they are related.
For example, "Rock.Tests.Integration.Engagement.Steps" contains the 


## Test Setup/Teardown
Groups of related tests should be repeatable where it is possible to do so.
Each group of tests is responsible for creating and removing any additional data needed for testing.
Sample data should be used for testing purposes where possible, but it should never be modified because this may affect the operation of other tests that rely on the same data.

It is critical that the complete set of the tests in the integration test suite can be executed from start to finish with a new sample database.
When adding or modifying tests, be sure to verify that the test suite is able to run correctly to completion.

### Assert

Tests in this project use the "Assert.That.[Assertion]" pattern in preference to the standard "Assert.[Assertion]" pattern.
eg. "Assert.That.AreEqual(a, b);" is preferred to "Assert.AreEqual(a, b);

This is because "Assert.That" allows a consistent syntax for accessing both the standard MSBuild Assert methods and any custom extension methods we have added to extend the Assert functionality as needed.

## Other Rules

Tests...

2. must have at least one Assert.  ("No Assert, then it's not a test.")
If the test is intended to measure performance rather than test for valid operation, consider adding it to the Rock.Tests.Performance project instead.

3. must test only one thing. However, you can Assert multiple things about that test to proof it's true.
4. must not depend on the order that tests are run.
5. must not depend on data that may have been destroyed by another test.
6. must not destroy data that other tests are expecting.
7. should not be overly complex with many layers. (KISS principle)
8. shall not write to hard-coded folders. (C://foo/...)
9. should always be able to run in a CI/CD (AppVeyor) environment and without specific/manual setup**.  (Otherwise mark manual tests as [Ignore])

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
