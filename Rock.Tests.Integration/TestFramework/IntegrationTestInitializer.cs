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
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Bus;
using Rock.Tests.Integration.Core.Lava;
using Rock.Tests.Integration.Database;
using Rock.Tests.Shared;
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    [TestClass]
    public sealed class IntegrationTestInitializer
    {
        public static bool InitializeDatabaseOnStartup = true;
        public static ITestDatabaseInitializer DatabaseInitializer = new IntegrationTestDatabaseInitializer();
        public static bool InitializeSampleDataOnStartup = true;

        /// <summary>
        /// This will run before any tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize( TestContext context )
        {
            var testClassType = System.Type.GetType( context.FullyQualifiedTestClassName );
            var requirement = testClassType.GetCustomAttributes( typeof( DatabaseInitializationStateAttribute ), inherit: false )
                .Cast<DatabaseInitializationStateAttribute>()
                .FirstOrDefault();

            if ( requirement == null )
            {
                var testMethodType = testClassType.GetMethod( context.TestName );
                requirement = testMethodType.GetCustomAttributes( typeof( DatabaseInitializationStateAttribute ), inherit: false )
                    .Cast<DatabaseInitializationStateAttribute>()
                    .FirstOrDefault();
            }

            if ( requirement != null )
            {
                if ( requirement.RequiredState == DatabaseInitializationStateSpecifier.None
                    || requirement.RequiredState == DatabaseInitializationStateSpecifier.Custom )
                {
                    InitializeDatabaseOnStartup = false;
                }
                else if ( requirement.RequiredState == DatabaseInitializationStateSpecifier.New )
                {
                    InitializeDatabaseOnStartup = true;
                    InitializeSampleDataOnStartup = false;

                }
                else
                {
                    InitializeDatabaseOnStartup = true;
                    InitializeSampleDataOnStartup = true;
                }
            }

            Initialize( context );
        }

        /// <summary>
        /// Initialize the Rock application environment for integration testing.
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize( TestContext context )
        {
            Rock.AssemblyInitializer.Initialize();

            // Copy the configuration settings to the TestContext so they can be accessed by the integration tests project initializer.
            AddTestContextSettingsFromConfigurationFile( context );

            LogHelper.SetTestContext( context );
            LogHelper.Log( $"Initializing test environment..." );

            // Initialize the Lava Engine first, because it may be needed by the sample data loader when the database is initialized.
            LogHelper.Log( $"Initializing Lava Engine (Pass 1)..." );
            LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: false );

            if ( InitializeDatabaseOnStartup )
            {
                LogHelper.Log( $"Initializing test database..." );

                // Set properties of the database manager from the test context.
                TestDatabaseHelper.ConnectionString = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
                TestDatabaseHelper.DatabaseCreatorKey = context.Properties["DatabaseCreatorKey"].ToStringSafe();
                TestDatabaseHelper.DatabaseRefreshStrategy = context.Properties["DatabaseRefreshStrategy"].ToStringSafe().ConvertToEnum<DatabaseRefreshStrategySpecifier>( DatabaseRefreshStrategySpecifier.Never );
                TestDatabaseHelper.SampleDataUrl = context.Properties["SampleDataUrl"].ToStringSafe();
                TestDatabaseHelper.DefaultSnapshotName = context.Properties["DefaultSnapshotName"].ToStringSafe();
                TestDatabaseHelper.DatabaseInitializer = DatabaseInitializer;
                TestDatabaseHelper.SampleDataIsEnabled = InitializeSampleDataOnStartup;

                TestDatabaseHelper.InitializeTestDatabase();

                RockInstanceConfig.SetDatabaseIsAvailable( true );

                // Reinitialize the Lava Engine and configure it to load dynamic shortcodes from the test database.
                LogHelper.Log( $"Initializing Lava Engine (Pass 2)..." );
                LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: true );

                LogHelper.Log( $"Initializing Rock Message Bus..." );

                // Verify that the InMemory transport component is registered.
                // If not, the Rock Message Bus will fail to start.
                var cacheEntity = EntityTypeCache.Get( typeof( Rock.Bus.Transport.InMemory ), createIfNotFound: false );
                if ( cacheEntity == null )
                {
                    throw new System.Exception( "Rock Message Bus failure. The InMemoryTransport Entity Type is not registered. To correct this error, re-create the test database with the \"ForceReplaceExistingDatabase\" configuration option." );
                }

                // Start the Message Bus and poll until it is ready.
                _ = RockMessageBus.StartAsync();

                while ( !RockMessageBus.IsReady() )
                {
                    LogHelper.Log( $"Waiting on Rock Message Bus..." );
                    Thread.Sleep( 500 );
                }
                RockMessageBus.IsRockStarted = true;
            }
            else
            {
                LogHelper.Log( $"Initializing test database... (disabled)" );
            }

            LogHelper.Log( $"Initialization completed." );
        }

        public static void AddTestContextSettingsFromConfigurationFile( TestContext context )
        {
            // Copy the application configuration settings to the TestContext.
            foreach ( var key in ConfigurationManager.AppSettings.AllKeys )
            {
                context.Properties[key] = ConfigurationManager.AppSettings[key];
            }
        }
    }

    [TestClass]
    public class DatabaseTests
    {
        /// <summary>
        /// Execute this test method as a placeholder to create/update/verify the test database.
        /// </summary>
        [TestMethod]
        [DatabaseInitializationState( DatabaseInitializationStateSpecifier.None )]
        public void CreateEmptyDatabase()
        {
            var taskGuid = LogHelper.StartTask( $"Create New Database" );

            // Set properties of the database manager from the test context.
            TestDatabaseHelper.ConnectionString = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
            TestDatabaseHelper.DatabaseCreatorKey = ConfigurationManager.AppSettings["DatabaseCreatorKey"].ToStringSafe();
            TestDatabaseHelper.DatabaseRefreshStrategy = DatabaseRefreshStrategySpecifier.Force;
            TestDatabaseHelper.SampleDataIsEnabled = false;

            TestDatabaseHelper.InitializeTestDatabase();

            LogHelper.StopTask( taskGuid );
        }

        /// <summary>
        /// Execute this test method as a placeholder to create/update/verify the test database.
        /// </summary>
        [TestMethod]
        [DatabaseInitializationState(DatabaseInitializationStateSpecifier.SampleData)]
        public void CreateSampleDatabase()
        {
            LogHelper.Log( "Database created." );
        }
    }
}
