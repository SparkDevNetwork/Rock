// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Tests.Integration;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

/// <summary>
/// A set of "test" actions that can be used to create LocalDb databases for Rock application testing.
/// </summary>
[TestClass]
public class LocalDatabaseTests
{
    private static bool _utilityTestsEnabled = false;

    /// <summary>
    /// Initialize the Rock application environment for integration testing.
    /// </summary>
    /// <param name="context">The context.</param>
    [ClassInitialize()]
    public static void Initialize( TestContext context )
    {
        _utilityTestsEnabled = context.Properties["UtilityTestsEnabled"].ToStringOrDefault( string.Empty ).AsBoolean();

        Rock.AssemblyInitializer.Initialize();
    }

    /// <summary>
    /// Execute this test method as a placeholder to create/update/verify the test database.
    /// </summary>
    [TestMethod]
    [TestProperty( "Utility", "LocalDatabase" )]
    public void CreateEmptyDatabase()
    {
        AssertUtilityTestsAreEnabled();

        var taskGuid = LogHelper.StartTask( $"Create New Database" );

        // Set properties of the database manager from the test context.
        var manager = new LocalDatabaseManager();

        manager.ConnectionString = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
        manager.DatabaseCreatorKey = ConfigurationManager.AppSettings["DatabaseCreatorKey"].ToStringSafe();
        manager.DatabaseRefreshStrategy = DatabaseRefreshStrategySpecifier.Force;

        manager.InitializeTestDatabase();

        LogHelper.StopTask( taskGuid );
    }

    /// <summary>
    /// Execute this test method as a placeholder to create/update/verify the test database.
    /// </summary>
    [TestMethod]
    [TestProperty( "Utility", "LocalDatabase" )]
    public void CreateSampleDatabase()
    {
        AssertUtilityTestsAreEnabled();

        var taskGuid = LogHelper.StartTask( $"Create New Database" );

        // Set properties of the database manager from the test context.
        var manager = new LocalDatabaseManager();

        manager.ConnectionString = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
        manager.DatabaseCreatorKey = ConfigurationManager.AppSettings["DatabaseCreatorKey"].ToStringSafe();
        manager.DatabaseRefreshStrategy = DatabaseRefreshStrategySpecifier.Force;
        manager.SampleDataUrl = ConfigurationManager.AppSettings[LocalDatabaseManager.SampleDataSourceKey].ToStringSafe();

        manager.InitializeTestDatabase();

        LogHelper.StopTask( taskGuid );

        LogHelper.Log( "Database created." );
    }

    private void AssertUtilityTestsAreEnabled()
    {
        if ( !_utilityTestsEnabled )
        {
            Assert.Inconclusive( "Utility Test Methods are disabled in this configuration. To enable utility tests, set the configuration option [UtilityTestsEnabled]=true" );
        }
    }
}
