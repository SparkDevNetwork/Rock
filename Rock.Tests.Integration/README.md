
# Integration Tests
The goal of this project is to be a permanent place to store tests that require things such as a database context to fetch data, configuration settings, save data, etc.  At the moment, you (the developer) will run these tests when needed, but ultimately our automated build system (AppVeyor) will automatically run all these tests after every commit (or push to a particular branch). 

> *NOTE: Use a fresh database with the PowerTools &gt; SampleData loaded so we all are testing against the same expected sample data.  If your test needs different data, you are responsible for adding it and cleaning it up as to not interfere with other tests.*

## Setup Instructions

1. Create an `app.ConnectionStrings.config` file in this project.
2. Set the "Copy to Output Directory" property to "Copy always".
3. Under menu Test > Test Settings, choose "Select Test Settings File" and select the `run.testsettings` file in the project.
![test settings](https://rockrms.blob.core.windows.net/public-images/githubdocs/vs-test-testsettings.png "In Visual Studio under Test Settings")
![test settings file selection](https://rockrms.blob.core.windows.net/public-images/githubdocs/vs-test-testsettings-fileselection.png "found in the Rock.Tests.Integration project")


## MS Unit Test vs XUnit

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

            Assert.AreEqual( "100", code.Code );
        }
    }
}
```

> NOTE: The existing `Rock.Tests` project is an XUnit type project, but it may be converted over to MS Test. 

## Running a Test
To run or debug a test, simply right-click the class name and choose `Run Tests` or `Debug Tests` -- but you should probably set a breakpoint in your test if you're going to select Debug Tests.  Alternatively you can choose the Test > Windows > Test Explorer from the menu to run tests a bit easier.

## Conversion Game Plan
Any tests that require a database will need to be moved from the existing `Rock.Tests` project into the proper class in this project.   So, tests like this...

```csharp
        [Fact]
        public void GraduatesThisYear()
        {
            InitGlobalAttributesCache();
            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.True( Person.GraduationYear == RockDateTime.Now.AddYears( 1 ).Year );
        }
```

...needs to become something like this:

```csharp
        [TestMethod]
        public void GraduatesThisYear()
        {
            InitGlobalAttributesCache();
            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.IsTrue( Person.GraduationYear == RockDateTime.Now.AddYears( 1 ).Year );
        }
   ```

## MSTest LifeCycle

The following decorators can give you more control over setup and cleanup for your test suite:

* [AssemblyInitialize] - called once before running the tests of the assembly. 
* [AssemblyCleanup] - called after all tests of the assembly are executed.
* [ClassInitialize] - called once before running the tests of the class.
* [ClassCleanup] - called after all tests from all classes finish.
* [TestInitialize] - called before running each test of the class. 
* [TestCleanup] - called after running each test of the class.
 
