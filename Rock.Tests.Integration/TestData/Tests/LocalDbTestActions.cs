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
using Rock.Tests.Integration.TestFramework;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.TestFramework.Database.Initializer;

namespace Rock.Tests.Integration.TestData
{
    /// <summary>
    /// Actions that can be used to create LocalDb databases for Rock application testing.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.LocalDbUtilities )]
    public class LocalDbTestActions : UtilityTestActionsBase
    {
        /// <summary>
        /// Execute this test method to create/update/verify the LocalDb test database.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateEmptyDatabaseImage()
        {
            var taskGuid = LogHelper.StartTask( $"Create/Update Basic Database Image" );

            // Set properties of the database manager from the test context.
            var manager = GetLocalDatabaseManager();
            manager.DatabaseInitializer = new BasicDataset();

            manager.InitializeTestDatabase( rebuildArchiveImage:true );

            LogHelper.StopTask( taskGuid );
        }

        /// <summary>
        /// Execute this test method to create/update/verify the LocalDb test database.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSampleDatabaseImage()
        {
            var taskGuid = LogHelper.StartTask( $"Create/Update Sample Database Image" );

            // Set properties of the database manager from the test context.
            var manager = GetLocalDatabaseManager();
            manager.DatabaseInitializer = new SampleDataset();

            manager.InitializeTestDatabase( rebuildArchiveImage: true );

            LogHelper.StopTask( taskGuid );
        }

        /// <summary>
        /// Execute this test method to create/update/verify the LocalDb test database.
        /// </summary>
        [TestMethod]
        public void RestoreOrCreateEmptyDatabase()
        {
            var taskGuid = LogHelper.StartTask( $"Restore/Create Empty Database" );

            // Set properties of the database manager from the test context.
            var manager = GetLocalDatabaseManager();
            manager.DatabaseInitializer = new BasicDataset();

            manager.InitializeTestDatabase();

            LogHelper.StopTask( taskGuid );
        }

        /// <summary>
        /// Execute this test method to create/update/verify the LocalDb test database.
        /// </summary>
        [TestMethod]
        public void RestoreOrCreateSampleDatabase()
        {
            var taskGuid = LogHelper.StartTask( $"Restore/Create Sample Database" );

            // Set properties of the database manager from the test context.
            var manager = GetLocalDatabaseManager();
            manager.DatabaseInitializer = new SampleDataset();

            manager.InitializeTestDatabase();

            LogHelper.StopTask( taskGuid );
        }

        /// <summary>
        /// Execute this test method to create/update/verify the LocalDb test database.
        /// </summary>
        [TestMethod]
        public void TestPluginMigrations()
        {
            var taskGuid = LogHelper.StartTask( $"Running Plugin Migrations" );

            var manager = GetLocalDatabaseManager();

            manager.ApplyPluginMigrations( System.Reflection.Assembly.GetExecutingAssembly() );

            LogHelper.StopTask( taskGuid );
        }

        public static LocalDatabaseManager GetLocalDatabaseManager()
        {
            // Set properties of the database manager from the test context.
            var manager = IntegrationTestInitializer.GetConfiguredLocalDatabaseManager();

            manager.IsCreatorKeyVerificationRequiredForDatabaseReset = true;
            manager.IsDatabaseResetPermitted = true;

            return manager;
        }
    }
}