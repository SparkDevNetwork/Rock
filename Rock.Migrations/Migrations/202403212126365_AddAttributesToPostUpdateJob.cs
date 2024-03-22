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
using Rock.SystemGuid;

namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class AddAttributesToPostUpdateJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the Block Attribute Key of the old webforms Login Legacy block to match the corresponding one in the new Obsidian version.
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'HideNewAccountOption' WHERE [GUID] = '7D47046D-5D66-45BB-ACFA-7460DE112FC2'" );

            // Update the Block Attribute Keys of the old webforms Group Registration block to match the corresponding ones in the new Obsidian version.
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'RequireEmail' WHERE [GUID] = '37E22E5F-19C9-4F17-8E1D-8C0E5F52DE1D'" );
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'RequireMobilePhone' WHERE [GUID] = '7CE78567-4438-47E7-B0D0-25D5B6498515'" );

            // Add attribute keys to ignore for existing Chop/Swap Jobs.

            // Add the attribute to the existing Chop/Swap jobs if needed.
            var jobGuids = new string[] { ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY,
                ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2,
                ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1,
                "7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE",
                "A65D26C1-229E-4198-B388-E269C3534BC0",
                "72D9EC04-517A-4CA0-B631-9F9A41F1790D",
                "54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5",
                "8390C1AC-88D6-474A-AC05-8FFBD358F75D",
                ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS
            };
            foreach ( string jobGuid in jobGuids )
            {
                RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", "Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks", "Webform BlockType Attribute Keys To Ignore During Validation", "Webform BlockType Attribute Keys To Ignore During Validation", "A Guid [key] of the old Webform BlockType and the [value] as a comma delimited list of BlockType Attribute keys to ignore when validating the Obsidian and Webforms blocks have the same keys.", 3, "", jobGuid, "BlockAttributeKeysToIgnore" );
            }

            // Exclude Attribute UnsubscribefromListText from Email Preference Entry block.
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY,
                key: "BlockAttributeKeysToIgnore", value: "B3C076C7-1325-4453-9549-456C23702069^UnsubscribefromListText" );

            // Exclude Attributes SignupInstructions and DeclineReasonPage from Group Schedule Toolbox.
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2,
                key: "BlockAttributeKeysToIgnore", value: "18A6DCE3-376C-4A62-B1DD-5E5177C11595^SignupInstructions,DeclineReasonPage" );
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1,
                key: "BlockAttributeKeysToIgnore",
                value: "7F9CEA6F-DCE5-4F60-A551-924965289F1D^SignupInstructions,DeclineReasonPage,FutureWeeksToShow,EnableSignup" );

            // Exclude Attribute SummaryLavaTemplate from Batch List Blocks
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: "7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE",
                key: "BlockAttributeKeysToIgnore",
                value: "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25^SummaryLavaTemplate" );

            // Exclude Attribute RemoteAuthorizationTypes from Login Legacy block
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: "A65D26C1-229E-4198-B388-E269C3534BC0",
                key: "BlockAttributeKeysToIgnore",
                value: "7B83D513-1178-429E-93FF-E76430E038E4^RemoteAuthorizationTypes" );

            // Exclude Attribute Enable Debug from Group Registration block
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: "72D9EC04-517A-4CA0-B631-9F9A41F1790D",
                key: "BlockAttributeKeysToIgnore",
                value: "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7^EnableDebug" );

            // Exclude Attribute DetailPage from Batch Detail block
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: "54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5",
                key: "BlockAttributeKeysToIgnore",
                value: "CE34CE43-2CCF-4568-9AEB-3BE203DB3470^DetailPage" );

            // Exclude Attribute NoteViewLavaTemplate from Notes block
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: "8390C1AC-88D6-474A-AC05-8FFBD358F75D",
                key: "BlockAttributeKeysToIgnore",
                value: "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3^NoteViewLavaTemplate" );

            // Exclude Attributes Columns, ShowCampusType, ShowCampusStatus from Family Pre Registration block
            RockMigrationHelper.AddOrUpdatePostUpdateJobAttributeValue( serviceJobGuid: ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS,
                key: "BlockAttributeKeysToIgnore",
                value: "463A454A-6370-4B4A-BCA1-415F2D9B0CB7^Columns,ShowCampusType,ShowCampusStatus" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
