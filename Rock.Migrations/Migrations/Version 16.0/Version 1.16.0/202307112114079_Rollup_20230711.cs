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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230711 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSmsOptInLableSystemSetting();
            AddFamilyPreRegistrationObsidianBlock();
            ReplaceWebFormFamilyPreRegistrationWithObsidianFamilyPreRegistration();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void AddSmsOptInLableSystemSetting()
        {
            RockMigrationHelper.UpdateSystemSetting( SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL, "Give your consent to receive SMS messages by simply checking the box." );
        }

        /// <summary>
        /// GJ: Fix Breadcrumb Display
        /// </summary>
        private void AddFamilyPreRegistrationObsidianBlock()
        {
            // Add the FamilyPreRegistration Obsidian Block EntityType
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.FamilyPreRegistration", "Family Pre Registration", "Rock.Blocks.Crm.FamilyPreRegistration, Rock.Blocks, Version=1.15.1.1, Culture=neutral, PublicKeyToken=null", false, false, "C03CE9ED-8572-4BE5-AB2A-FF7498494905" );

            // Add/Update Obsidian Block Type
            //   Name: Family Pre Registration
            //   Category: Obsidian > CRM
            //   EntityType:Rock.Blocks.Crm.FamilyPreRegistration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Family Pre Registration", "Provides a way to allow people to pre-register their families for weekend check-in.", "Rock.Blocks.Crm.FamilyPreRegistration", "Obsidian > CRM", "1D6794F5-876B-47B9-9C9B-5C2C2CC81074" );
        }

        private void ReplaceWebFormFamilyPreRegistrationWithObsidianFamilyPreRegistration()
        {
            // Webforms 463A454A-6370-4B4A-BCA1-415F2D9B0CB7
            // Obsidian 1D6794F5-876B-47B9-9C9B-5C2C2CC81074

            var FullyQualifiedJobClassName = "Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks";

            // Configure run-once job by modifying these variables.
            var commandTimeout = 14000;
            var blockTypeReplacements = new Dictionary<string, string>
            {
            { "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1D6794F5-876B-47B9-9C9B-5C2C2CC81074" }
            };

            Sql( $@"
                IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{FullyQualifiedJobClassName}' AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}' )
                BEGIN
                    INSERT INTO [ServiceJob] (
                          [IsSystem]
                        , [IsActive]
                        , [Name]
                        , [Description]
                        , [Class]
                        , [CronExpression]
                        , [NotificationStatus]
                        , [Guid] )
                    VALUES ( 
                          0
                        , 1
                        , 'Rock Update Helper v15.2 - Replace WebForms Blocks with Obsidian Blocks'
                        , 'This job will replace WebForms blocks with their Obsidian blocks on all sites, pages, and layouts.'
                        , '{FullyQualifiedJobClassName}'
                        , '0 0 21 1/1 * ? *'
                        , 1
                        , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}');
                END" );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Command Timeout
            var commandTimeoutAttributeGuid = "4F15F5A5-F83C-4CA6-B7F9-9DCDAC0B4DEF";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.INTEGER, "Class", FullyQualifiedJobClassName, "Command Timeout", "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, "14000", commandTimeoutAttributeGuid, "CommandTimeout" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, commandTimeoutAttributeGuid, commandTimeout.ToString() );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Block Type Guid Replacement Pairs
            var blockTypeReplacementsAttributeGuid = "CDDB8075-E559-499F-B12F-B8DC8CCD73B5";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", FullyQualifiedJobClassName, "Block Type Guid Replacement Pairs", "Block Type Guid Replacement Pairs", "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.", 1, "", blockTypeReplacementsAttributeGuid, "BlockTypeGuidReplacementPairs" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, blockTypeReplacementsAttributeGuid, SerializeDictionary( blockTypeReplacements ) );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Migration Strategy
            var migrationStrategyAttributeGuid = "FA99E828-2388-4CDF-B69B-DBC36332D6A4";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.SINGLE_SELECT, "Class", FullyQualifiedJobClassName, "Migration Strategy", "Migration Strategy", "Determines if the blocks should be chopped instead of swapped. By default,the old blocks are swapped with the new ones.", 2, "Swap", migrationStrategyAttributeGuid, "MigrationStrategy" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, migrationStrategyAttributeGuid, "Chop" );

            // Attribute: Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Should Keep Old Blocks
            var shouldKeepOldBlocksAttributeGuid = "768BDB5D-BA09-4246-AA25-F1C37097CC7D";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", FullyQualifiedJobClassName, "Should Keep Old Blocks", "Should Keep Old Blocks", "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.", 3, "False", shouldKeepOldBlocksAttributeGuid, "ShouldKeepOldBlocks" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, shouldKeepOldBlocksAttributeGuid, "False" );
        }

        private string SerializeDictionary( Dictionary<string, string> dictionary )
        {
            const string keyValueSeparator = "^";

            if ( dictionary?.Any() != true )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var first = dictionary.First();
            sb.Append( $"{first.Key}{keyValueSeparator}{first.Value}" );

            foreach ( var kvp in dictionary.Skip( 1 ) )
            {
                sb.Append( $"|{kvp.Key}{keyValueSeparator}{kvp.Value}" );
            }

            return sb.ToString();
        }
    }
}
