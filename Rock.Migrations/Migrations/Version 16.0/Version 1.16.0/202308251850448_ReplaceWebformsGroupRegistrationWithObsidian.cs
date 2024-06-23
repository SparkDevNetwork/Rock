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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class ReplaceWebformsGroupRegistrationWithObsidian : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChopBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ChopBlocksDown();
        }

        /// <summary>
        /// PA: Chop Block Type
        /// </summary>
        private void ChopBlocksUp()
        {

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Group Registration",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "5E000376-FF90-4962-A053-EC1473DA5C45" }, // Group Registration
                },
                migrationStrategy: "Chop",
                jobGuid: "72D9EC04-517A-4CA0-B631-9F9A41F1790D" );
        }

        /// <summary>
        /// PA: Chop Block Types
        /// </summary>
        private void ChopBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( "72D9EC04-517A-4CA0-B631-9F9A41F1790D", "ServiceJob" );
        }
    }
}
