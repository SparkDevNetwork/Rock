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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AchievementIndexes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add achiever entity id to achievement type index
            RockMigrationHelper.CreateIndexIfNotExists( "AchievementAttempt",
                new[] { "AchievementTypeId", "AchieverEntityId" },
                new string[0] );

            RockMigrationHelper.DropIndexIfExists( "AchievementAttempt", "IX_AchievementTypeId" );

            // Add step type id to person alias index
            RockMigrationHelper.CreateIndexIfNotExists( "Step",
                new[] { "PersonAliasId", "StepTypeId" },
                new[] { "CompletedDateTime" } );

            RockMigrationHelper.DropIndexIfExists( "Step", "IX_PersonAliasId" );

            // Add person alias to step type id index
            RockMigrationHelper.CreateIndexIfNotExists( "Step",
                new[] { "StepTypeId", "PersonAliasId" },
                new[] { "CompletedDateTime" } );

            RockMigrationHelper.DropIndexIfExists( "Step", "IX_StepTypeId" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // There is no need to go back for compatability with older code
        }
    }
}
