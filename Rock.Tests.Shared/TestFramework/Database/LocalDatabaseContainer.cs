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
using System.Threading.Tasks;
using Rock.Configuration;
using Rock.Tests.Shared.Lava;
using Rock.Utility.Settings;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Manages Rock databases for a LocalDb instance used during developer testing.
    /// </summary>
    public class LocalDatabaseContainer : ITestDatabaseContainer
    {
        public LocalDatabaseContainer( LocalDatabaseManager manager )
        {
            _manager = manager;
        }

        private LocalDatabaseManager _manager;
        private bool _hasCurrentInstance = false;
        private bool _containerIsInitialized = false;

        public bool HasCurrentInstance
        {
            get
            {
                return _hasCurrentInstance;
            }
        }

        public Task DisposeAsync()
        {
            // The LocalDB container does not need to be disposed.
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            var task = Task.Run( () =>
            {
                _manager.ResetDatabase();
                _hasCurrentInstance = true;

                if ( !_containerIsInitialized )
                {
                    // If this is the first database request, start the Lava Service and load shortcode definitions from the database.
                    // Re-initialization is not required for subsequent resets because the restored database has identical data.
                    _containerIsInitialized = true;

                    LogHelper.Log( $"Initializing Lava Database Elements..." );

                    // This flag must be set to ensure that RockEntity commands are initialized correctly.
                    var dbConfig = RockApp.Current.GetDatabaseConfiguration() as DatabaseConfiguration;
                    dbConfig.IsDatabaseAvailable = true;

                    LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: true );

                    LogHelper.Log( $"Initializing Lava Database Elements: completed." );
                }

            } );
            return task;
        }
    }
}