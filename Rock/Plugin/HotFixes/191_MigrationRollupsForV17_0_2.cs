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

using System.Collections.Generic;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 191, "1.16.1" )]
    public class MigrationRollupsForV17_0_2 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateIndexesOnInteraction();
            ChopLegacyCommunicationRecipientList();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// PA: Updated indexes on the Interaction table.
        /// </summary>
        private void UpdateIndexesOnInteraction()
        {
            // note: the cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - Interaction Index Post Migration Job",
                description: "This job adds the IX_InteractionSessionId_CreatedDateTime index on the Interaction Table.",
                jobType: "Rock.Jobs.PostV17InteractionIndexPostMigration", cronExpression: "0 0 21 1/1 * ? *",
                guid: "9984C806-FAEE-4005-973B-9FBE21948972" );
        }

        /// <summary>
        /// PA: Chop Legacy Communication Recipient List Webforms Block
        /// </summary>
        private void ChopLegacyCommunicationRecipientList()
        {

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Chop CommunicationRecipientList",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "EBEA5996-5695-4A42-A21C-29E11E711BE8", "3F294916-A02D-48D5-8FE4-E8D7B98F61F7" }
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_COMMUNICATION_RECIPIENT_LIST_BLOCK );
        }
    }
}
