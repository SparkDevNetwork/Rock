namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Indicates to the framework that we need to provide an isolated
    /// database to this test, or all tests in this class. If specified
    /// then a new database will be spun up for just this test instead
    /// of the entire class of tests.
    /// </summary>
    public sealed class IsolatedTestDatabaseAttribute : System.Attribute
    {
    }
}
