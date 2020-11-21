
# Integration Tests
The goal of this project is to be a permanent place to store tests that require things such as a database context to fetch data, configuration settings, save data, etc.  At the moment, you (the developer) will run these tests when needed, but ultimately our automated build system (AppVeyor) will automatically run all these tests after every commit (or push to a particular branch). 

> *We've merged in Daniel's changes [described here](https://github.com/SparkDevNetwork/Rock/issues/3227#issuecomment-583567407).*

## Running (for the first time)
If you've not done this, you'll need to set your Test > Test Settings > Test Settings File to something like the `test.runsettings` file which would have any custom secret settings in there (like Amazon S3 Storage provider connection/key settings, etc.)  Note: Only some tests use that file so many tests can run without doing this.

### Timings
The very first initial run, meaning where it has to generate the archive, may take around 4 minutes.

Once that archive is created, the very first call to ResetDatabase() takes about 30 seconds (this seems to be due to the fact it has to load a bunch of additional DLLs into memory).

Subsequent calls to ResetDatabase() take about 6 seconds.

### LocalDb
Note that this change "requires" LocalDB so the concept of an app.ConnectionStrings.config goes out the window.


## MS Test

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

### Test Methods

Tests in this project use the "Assert.That.[Assertion]" pattern in preference to the standard "Assert.[Assertion]" pattern.
eg. "Assert.That.AreEqual(a, b);" is preferred to "Assert.AreEqual(a, b);

This is because "Assert.That" allows a consistent syntax for accessing both the standard MSBuild Assert methods and any custom extension methods we have added to extend the Assert functionality as needed.

### MSTest LifeCycle

The following decorators can give you more control over setup and cleanup for your test suite:

* [AssemblyInitialize] - called once before running the tests of the assembly. 
* [AssemblyCleanup] - called after all tests of the assembly are executed.
* [ClassInitialize] - called once before running the tests of the class.
* [ClassCleanup] - called after all tests from all classes finish.
* [TestInitialize] - called before running each test of the class. 
* [TestCleanup] - called after running each test of the class.
 

## Running a Test
To run or debug a test, simply right-click the class name and choose `Run Tests` or `Debug Tests` -- but you should probably set a breakpoint in your test if you're going to select Debug Tests.  Alternatively you can choose the Test > Windows > Test Explorer from the menu to run tests a bit easier.

# Test Rules

Tests...

1. should have method names that say what the test is testing .(ex: NumericCodesShouldNotContain911And666). If it fails, you should immediately know what is not working.
2. must have at least one Assert.  ("No Assert, then it's not a test.")
3. must test only one thing. However, you can Assert multiple things about that test to proof it's true.
4. must not depend on the order that tests are run.
5. must not depend on data that may have been destroyed by another test.
6. must not destroy data that other tests are expecting.
7. should not be overly complex with many layers. (KISS principle)
8. shall not write to hard-coded folders. (C://foo/...)
9. should always be able to run in a CI/CD (AppVeyor) environment and without specific/manual setup**.  (Otherwise mark manual tests as [Ignore])

>     **Human only run-able tests (tests that require some specific environment or environment access) should be in a separate test project and class.
