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

    /// <summary>
    ///
    /// </summary>
    public partial class AddCommunicationDetailBlockType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddCommunicationDetailBlockType_20250703_Up();
            JPH_PopulateCommunicationRecipientDeliveredDateTime_20250703_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddCommunicationDetailBlockType_20250703_Down();
        }

        /// <summary>
        /// JPH: Add communication detail block type - up.
        /// </summary>
        private void JPH_AddCommunicationDetailBlockType_20250703_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationDetail", "Communication Detail", "Rock.Blocks.Communication.CommunicationDetail, Rock.Blocks, Version=18.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "32838848-2423-4BD9-B5EF-5F7E6AC7F5F4" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Detail", "Used for displaying details of an existing communication that has already been created.", "Rock.Blocks.Communication.CommunicationDetail", "Communication", "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" );

            // Attribute for BlockType
            //   BlockType: Communication Detail
            //   Category: Communication
            //   Attribute: Enable Personal Templates
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B63C6ED-20D5-467E-9A6A-C608E1D953E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Personal Templates", "EnablePersonalTemplates", "Enable Personal Templates", @"Should support for personal templates be enabled? These are templates that a user can create and are personal to them. If enabled, they will be able to create a new template based on the current communication.", 0, @"False", "6A35D776-6CCF-43B5-B92A-0371EA9FF5E3" );
        }

        /// <summary>
        /// JPH: Add communication detail block type - down.
        /// </summary>
        private void JPH_AddCommunicationDetailBlockType_20250703_Down()
        {
            // Attribute for BlockType
            //   BlockType: Communication Detail
            //   Category: Communication
            //   Attribute: Enable Personal Templates
            RockMigrationHelper.DeleteAttribute( "6A35D776-6CCF-43B5-B92A-0371EA9FF5E3" );

            // Delete BlockType 
            //   Name: Communication Detail
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Detail
            RockMigrationHelper.DeleteBlockType( "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" );

            // Delete Block EntityType: Rock.Blocks.Communication.CommunicationDetail
            RockMigrationHelper.DeleteEntityType( "32838848-2423-4BD9-B5EF-5F7E6AC7F5F4" );
        }

        /// <summary>
        /// JPH: Add a post update job to to populate the newly-added [CommunicationRecipient].[DeliveredDateTime] field.
        /// </summary>
        private void JPH_PopulateCommunicationRecipientDeliveredDateTime_20250703_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Populate [CommunicationRecipient].[DeliveredDateTime]",
                description: "This job will populate the newly-added [CommunicationRecipient].[DeliveredDateTime] field by attempting to parse the DateTime value from the [StatusNote] field for email records and by copying the value directly from the [SendDateTime] field for non-email records.",
                jobType: "Rock.Jobs.PostV18PopulateCommunicationRecipientDeliveredDateTime",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_POPULATE_COMMUNICATIONRECIPIENT_DELIVEREDDATETIME );
        }
    }
}
