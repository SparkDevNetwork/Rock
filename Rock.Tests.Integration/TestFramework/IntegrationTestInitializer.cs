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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Bus;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Settings;
using Rock.WebStartup;

namespace Rock.Tests.Integration.TestFramework
{
    [TestClass]
    [DeploymentItem( "app.TestSettings.config" )]
    [DeploymentItem( "app.ConnectionStrings.config" )]
    public sealed class IntegrationTestInitializer
    {
        public static bool IsContainersEnabled { get; private set; }

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
            if ( ConfigurationManager.ConnectionStrings["RockContext"] != null )
            {
                DatabaseTestsBase.IsContainersEnabled = false;
                RockInstanceConfig.Database.SetConnectionString( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
                RockInstanceConfig.SetDatabaseIsAvailable( true );
            }

            AddTestContextSettingsFromConfigurationFile( context );

            LogHelper.SetTestContext( context );
            LogHelper.Log( $"Initialize Test Environment: started..." );

            LogHelper.Log( $"Initializing Rock Message Bus..." );
            await RockMessageBus.StartTestMemoryBusAsync();
            LogHelper.Log( $"Initializing Rock Message Bus: completed." );

            LogHelper.Log( "Initializing Save Hooks..." );
            RockApplicationStartupHelper.ConfigureEntitySaveHooks();
            LogHelper.Log( "Initializing Save Hooks: completed." );

            LogHelper.Log( $"Initialize Test Environment: completed." );
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
