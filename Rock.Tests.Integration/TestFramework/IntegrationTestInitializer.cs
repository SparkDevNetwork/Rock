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
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Bus;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.WebStartup;

namespace Rock.Tests.Integration.TestFramework
{
    [TestClass]
    [DeploymentItem( "app.TestSettings.config" )]
    [DeploymentItem( "app.ConnectionStrings.config" )]
    public sealed class IntegrationTestInitializer
    {
        public static bool IsContainersEnabled { get; private set; }

        public static string DatabaseHostSettingKey = "DatabaseHost";

        [TestMethod]
        public void ForceDeployment()
        {
            // This method is required to workaround an issue with the MSTest deployment process.
            // It exists to ensure that the DeploymentItem attributes decorating this class are processed,
            // so that the required files are copied to the test deployment directory.
            // For further details, see https://github.com/microsoft/testfx/issues/634.
        }

        /// <summary>
        /// This will run before any tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static async Task AssemblyInitialize( TestContext context )
        {
            await InitializeTestEnvironment( context );
        }

        /// <summary>
        /// Initialize the basic test environment. This should only perform
        /// the bare minimum required to get the Rock internals up and
        /// running for tests.
        /// </summary>
        /// <param name="context">The test context.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        public static async Task InitializeTestEnvironment( TestContext context )
        {
            AddTestContextSettingsFromConfigurationFile( context );

            var databaseHost = ConfigurationManager.AppSettings[DatabaseHostSettingKey]?.ToStringSafe().Trim().ToLower();
            var connectionString = ConfigurationManager.ConnectionStrings["RockContext"]?.ConnectionString;

            // Initialize a database container for the test environment.
            ITestDatabaseContainer container;
            var databaseRefreshDisabled = false;
            if ( databaseHost == "localdb" )
            {
                var manager = GetConfiguredLocalDatabaseManager();

                databaseRefreshDisabled = !manager.IsDatabaseResetPermitted;

                container = new LocalDatabaseContainer( manager );
            }
            else if ( databaseHost == "remote" )
            {
                // Initialize the remote database connection.
                databaseRefreshDisabled = true;

                if ( connectionString.IsNullOrWhiteSpace() )
                {
                    throw new Exception( @"Database Connection string not found. The remote database container requires a configured ConnectionString for ""RockContext""." );
                }

                // For consistency, this should be implemented as a singleton instance of a container implementing ITestDatabaseContainer in the future.
                container = null;
            }
            else if ( databaseHost == "docker" || databaseHost.IsNullOrWhiteSpace() )
            {
                // Initialize the Docker Sql Server container.
                container = new TestDatabaseContainer();
            }
            else
            {
                throw new Exception( $"Database Host is invalid. The DatabaseHost configuration setting should be (docker|localdb|remote) . [DatabaseHost=\"{databaseHost}\"]" );
            }

            TestHelper.ConfigureRockApp( connectionString );

            DatabaseTestsBase.InitializeContainer( container );

            LogHelper.SetTestContext( context );

            LogHelper.Log( $"Initialize Test Environment: started..." );

            LogHelper.Log( $"Initializing Rock Message Bus..." );
            await RockMessageBus.StartTestMemoryBusAsync();
            LogHelper.Log( $"Initializing Rock Message Bus: completed." );

            LogHelper.Log( "Initializing Save Hooks..." );
            RockApplicationStartupHelper.ConfigureEntitySaveHooks();
            LogHelper.Log( "Initializing Save Hooks: completed." );

            if ( databaseRefreshDisabled )
            {
                LogHelper.LogWarning( $"The test database refresh strategy is set to use the existing database without replacement.\nThis may cause unpredictable results for some tests." );
            }

            LogHelper.Log( $"Initialize Test Environment: completed." );
        }

        /// <summary>
        /// Get an instance of the LocalDatabaseManager configured for the current test session.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LocalDatabaseManager GetConfiguredLocalDatabaseManager()
        {
            var manager = new LocalDatabaseManager();
            manager.ConnectionString = ConfigurationManager.ConnectionStrings["RockContext"]?.ConnectionString;

            // Initialize the LocalDb container.
            if ( manager.ConnectionString.IsNullOrWhiteSpace() )
            {
                throw new Exception( @"Database Connection string not found. The LocalDb database container requires a configured ConnectionString for ""RockContext""." );
            }

            var csb = new SqlConnectionStringBuilder( manager.ConnectionString );
            var targetDatabaseName = csb.InitialCatalog;

            manager.DatabaseCreatorKey = ConfigurationManager.AppSettings["DatabaseCreatorKey"].ToStringSafe();
            manager.TargetMigrationName = ConfigurationManager.AppSettings["LocalDbResetMigrationName"].ToStringSafe();
            manager.ArchiveRetentionDays = ConfigurationManager.AppSettings["LocalDbArchiveRetentionDays"].ToIntSafe();

            // To enable database resets, the configured reset database name must match the database name in the connection string.
            // This operates as a safeguard against accidental replacement of development databases containing custom data.
            var resetDatabaseName = ConfigurationManager.AppSettings["LocalDbResetDatabaseName"].ToStringSafe();

            if ( targetDatabaseName.Equals( resetDatabaseName, StringComparison.OrdinalIgnoreCase ) )
            {
                manager.IsDatabaseResetPermitted = true;
                manager.IsCreatorKeyVerificationRequiredForDatabaseReset = false;
            }
            else
            {
                manager.IsDatabaseResetPermitted = false;
            }

            // Create a database initializer instance to prepare the test data.
            var initializerTypeName = ConfigurationManager.AppSettings["DatabaseInitializer"].ToStringSafe();
            if ( string.IsNullOrWhiteSpace( initializerTypeName ) )
            {
                initializerTypeName = nameof( Rock.Tests.Shared.TestFramework.Database.Initializer.SampleDataset );
            }

            var initializerTypesMap = Reflection.FindTypes( typeof( ITestDatabaseInitializer ) );
            var initializerType = initializerTypesMap.Where( kv => kv.Key == initializerTypeName || kv.Key.EndsWith( "." + initializerTypeName ) )
                .Select( kv => kv.Value )
                .FirstOrDefault();

            if ( initializerType == null )
            {
                throw new Exception( $@"Database Initializer not found. The configuration setting ""DatabaseInitializer"" must specify a known System.Type that implements the {nameof( ITestDatabaseInitializer )} interface." );
            }

            var initializerInstance = Activator.CreateInstance( initializerType ) as ITestDatabaseInitializer;

            manager.DatabaseInitializer = initializerInstance;

            return manager;
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
}
