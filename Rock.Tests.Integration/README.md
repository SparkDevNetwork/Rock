
# Integration Tests

The goal of this project is to be a permanent place to store tests that require things such as a database context to fetch data, configuration settings, save data, etc.  At the moment, you (the developer) will run these tests when needed, but ultimately our automated build system (AppVeyor) will automatically run all these tests after every commit (or push to a particular branch). 

So, in the meantime, you will need to create an `app.ConnectionStrings.config` file in this project (just like you do with your `web.ConnectionStrings.config`) in order to point these tests to a properly seeded database.  Then include it into the project and set its "Copy to Output Directory" property to "Copy always".

A Rock database is automatically created for your tests before they run. The source data is pulled from a ZIP file specified in the `app.config` file and by default will target the `develop` branch. If you need to target a different branch (for example an older version of Rock) then you will need to update the URL to point to a different archived database.

Creating a new database archive is simple and straight forward. Initialize your Rock installation so that it runs all initial migrations. Then shut down Rock so it disconnects from the database. Open up SQL Server Management Studio and use the Shrink File options to shrink both the Data and Log files so they don't have excess unused space. Next Detach the database so SQL Server is no longer accessing the files. Finally, find those two files in Windows Explorer and compress them into a ZIP file. No special filenames are necessary. The first MDF and LDF files in the archive will be used.

> *NOTE: If you are using a custom database archive then use a fresh database with the PowerTools &gt; SampleData loaded so we all are testing against the same expected sample data.  If your test needs different data, you are responsible for adding it and cleaning it up as to not interfere with other tests.*

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

## Database Creation Modes

There are two different modes you can design your tests in. If the individual tests in your class are able to clean up after themselves then you can instantiate the database once and use the same database for each test.

Example Code:

```csharp
namespace Rock.Tests.Integration.Model
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// </summary>
    [TestClass]
    public class AttendanceCodeTests
    {
        #region Setup

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            DatabaseTests.ResetDatabase();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DatabaseTests.DeleteDatabase();
        }

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            /* Code to restore the database back to it's original state. */
        }

        #endregion

        #region Tests

        /* Individual tests go here */

        #endregion
    }
}
```

Alternatively, if you are not sure your test can cleanup after itself then you will need to reset the database before every single test. An example of this would be if you are testing Person Merge code, since it would be extremely difficult (if not impossible) to undo that change.

Example Code:

```csharp
namespace Rock.Tests.Integration.Model
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// </summary>
    [TestClass]
    public class AttendanceCodeTests
    {
        #region Setup

        /// <summary>
        /// Runs before each test in this class is executed.
        /// </summary>
        [TestInitialize]
        public void TestInitialize( TestContext testContext )
        {
            DatabaseTests.ResetDatabase();
        }

        /// <summary>
        /// Runs after each test in this class is executed.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            DatabaseTests.DeleteDatabase();
        }

        #endregion

        #region Tests

        /* Individual tests go here */

        #endregion
    }
}
```

Whenever possible, you should use the first method that resets the database one time before the entire collection of tests run. This will drastically improve performance as it does not need to create and restore the database contents nearly as much.

## MSTest LifeCycle

The following decorators can give you more control over setup and cleanup for your test suite:

* [AssemblyInitialize] - called once before running the tests of the assembly.
* [AssemblyCleanup] - called after all tests of the assembly are executed.
* [ClassInitialize] - called once before running the tests of the class.
* [ClassCleanup] - called after all tests from all classes finish.
* [TestInitialize] - called before running each test of the class.
* [TestCleanup] - called after running each test of the class.
