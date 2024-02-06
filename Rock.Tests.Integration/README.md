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

### SampleDataUrl

This setting specifies the URL from which the Rock sample data can be downloaded.

# Managing the Test Database
The majority of tests in the integration test project require a Rock database populated with the appropriate version of the Rock sample data.

In order for the tests to run successfully a clean database state is required. To achieve this, we use Docker containers so that each test suite can execute on a clean database. Without making any other changes the tests will run in this mode. This requires that you have [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed on your machine.

When the test project is executed, it will look for an image tagged to the current migration number in Rock. If one is not found, it will automatically be created. This can take 15 - 25 minutes, or longer depending on your environment as it also installs the sample data. Once the base image is built it will be used whenever a clean database is required.

Whenever possible a test should put the database back the way it was found. This is not a hard requirement - for example, you don't need to hunt down any History records that might have been created. But if you change Ted's birth date for the test, change it back after the test finishes.

Each test suite (C# class) will get a new database instance for all tests contained in that suite. Usually this is sufficient as those tests are generally related and expect the same test data. When that is not the case and your individual tests each require a clean environment, you can apply the `[IsolatedDatabase]` attribute to the test method for whichever tests want a clean environment. You can also put that attribute on the class and each test will get a clean database.

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
