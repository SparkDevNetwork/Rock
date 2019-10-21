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
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Integration.Migrations
{
    /// <summary>
    /// Integration tests for an Entity Framework migration.
    /// </summary>
    public class MigrationTests
    {
        [TestClass]
        public class DatabaseMigrationTests
        {
            #region Migration Up/Down Tests

            /// <summary>
            /// Runs a single pending migration in the current database.
            /// Useful for testing a migration during the development process.
            /// </summary>
            [TestMethod]
            [TestCategory( TestCategories.DeveloperSetup )]
            [TestProperty( "Feature", TestFeatures.DataMaintenance )]
            public void ApplySingleDataMigration()
            {
                var configuration = new Rock.Migrations.Configuration();

                var migrator = new DbMigrator( configuration );

                var pendingMigrationNames = migrator.GetPendingMigrations();

                Assert.AreNotEqual( 0, pendingMigrationNames.Count(), "There is no pending migration." );
                Assert.AreEqual( 1, pendingMigrationNames.Count(), "There is more than one pending migration." );

                var pendingMigrationName = pendingMigrationNames.First();

                Debug.Print( $"Applying migration: { pendingMigrationName }" );

                migrator.Update();
            }

            /// <summary>
            /// Reverts the most recent migration in the current database.
            /// Useful for testing a migration during the development process.
            /// </summary>
            [TestMethod]
            [TestCategory( TestCategories.DeveloperSetup )]
            [TestProperty( "Feature", TestFeatures.DataMaintenance )]
            public void RollbackSingleDataMigration()
            {
                var configuration = new Rock.Migrations.Configuration();

                var migrator = new DbMigrator( configuration );

                // Get the second-last migration to be applied.
                // Migrations are returned in order of last-applied first.
                var targetMigrationName = migrator.GetDatabaseMigrations().Skip( 1 ).Take( 1 ).FirstOrDefault();

                Debug.Print( $"Rolling back to target migration \"{ targetMigrationName }\"..." );

                // Update to the second-last migration.
                migrator.Update( targetMigrationName );

                var newLastMigrationName = migrator.GetDatabaseMigrations().FirstOrDefault();

                Assert.AreEqual( targetMigrationName, newLastMigrationName );
            }

            #endregion
        }
    }
}
