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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Bus;
using Rock.Tests.Integration.Core.Lava;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    [TestClass]
    public sealed class IntegrationTestInitializer
    {
        public static bool InitializeDatabaseOnStartup = true;

        /// <summary>
        /// This will run before any tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize( TestContext context )
        {
            Rock.AssemblyInitializer.Initialize();

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
                TestDatabaseHelper.DatabaseCreatorId = context.Properties["DatabaseCreatorId"].ToStringSafe();
                TestDatabaseHelper.DatabaseRefreshStrategy = context.Properties["DatabaseRefreshStrategy"].ToStringSafe().ConvertToEnum<DatabaseRefreshStrategySpecifier>(DatabaseRefreshStrategySpecifier.Verified);
                TestDatabaseHelper.SampleDataUrl = context.Properties["SampleDataUrl"].ToStringSafe();

                TestDatabaseHelper.InitializeTestDatabase();

                // Reinitialize the Lava Engine and configure it to load dynamic shortcodes from the test database.
                LogHelper.Log( $"Initializing Lava Engine (Pass 2)..." );
                LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: true );
            }
            else
            {
                LogHelper.Log( $"Initializing test database... (disabled)" );
            }

            // TODO: Initializing the bus requires a database connection.
            // TODO: When database is restored from archive, this fails - why?
            //EntityTypeService.RegisterEntityTypes();

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

            LogHelper.Log( $"Initialization completed." );
        }
    }

    [TestClass]
    public class DatabaseTests
    {
        /// <summary>
        /// Execute this test method as a placeholder to create/update/verify the test database.
        /// </summary>
        [TestMethod]
        public void CreateTestDatabase()
        {

            LogHelper.Log( "Database created." );
        }
    }
}
