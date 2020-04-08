# Testing in Rock

Rock includes two types of tests; tests that require a database and tests that don't require a database.

This project is for tests that do NOT require a database.

If your test(s) requires a database, a Data.RockContext(), or SqlServerTypes library move over to the `Rock.Tests.Integration` project.

## Example Test Class
Use [this example](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Rock/Utility/ExtensionMethods/StringExtensionsTests.cs) or one of the other similar test classes as a pattern.

## Setup / Test Explorer

If you don't see a Test Explorer tab next to your Solution Explorer, select the "Test" option
      from the main VS menu. Then select Windows > Test Explorer. You should then see the tab. Once
      you build the test project, each test method should appear in the
      explorer. Test methods must be annotated with the [TestMethod] Attribute, and must exist in a class with a [TestClass] Attribute.

> NOTE: If you don't see them you may need to delete your `%TEMP%\VisualStudioTestExplorerExtensions`
folder (while VS is shut down) to get the tests to show up.

## Test Methods

Tests in this project use the "Assert.That.[Assertion]" pattern in preference to the standard "Assert.[Assertion]" pattern.
eg. "Assert.That.AreEqual(a, b);" is preferred to "Assert.AreEqual(a, b);

This is because "Assert.That" allows a consistent syntax for accessing both the standard MSBuild Assert methods and any custom extension methods we have added to extend the Assert functionality as needed.
