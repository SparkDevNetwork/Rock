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
    public partial class AddContentChannelItemListBlockSettings : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Total Views Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Total Views Columns", "ShowTotalViewsColumns", "Show Total Views Columns", @"Determines if the Views columns should be shown.", 4, @"True", "DB851ABC-ACB3-49CE-9F4B-8B83E5726924" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Item URL Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Item URL Column", "ShowItemUrlColumn", "Show Item URL Column", @"Determines if the Item URL column should be shown.", 6, @"True", "121D02CA-1E54-4CA3-B40F-4DAC6C919D1C" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Linked Media Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Linked Media Column", "ShowLinkedMediaColumn", "Show Linked Media Column", @"Determines if the Linked Media column should be shown.", 7, @"False", "E01AE103-1298-4DD0-A467-A0E1C10CAC12" );

            // ----------------------------------
            // Update the ContentChannelItemList block type and any instances to reflect the current block type name and description.
            // If they've changed the name of any block instances from the previous default, leave their names as-is.

            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'B995BE3F-A9EB-4A18-AE24-E93A8796AEDE');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Content Channel Item List'
        , [Description] = 'Displays a list of content channel items.'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Content Channel Item List'
    WHERE [BlockTypeId] = @BlockTypeId
        AND [Name] = 'Content Item List';
END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Linked Media Column
            RockMigrationHelper.DeleteAttribute( "E01AE103-1298-4DD0-A467-A0E1C10CAC12" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Item URL Column
            RockMigrationHelper.DeleteAttribute( "121D02CA-1E54-4CA3-B40F-4DAC6C919D1C" );

            // Attribute for BlockType
            //   BlockType: Content Item List
            //   Category: CMS
            //   Attribute: Show Total Views Columns
            RockMigrationHelper.DeleteAttribute( "DB851ABC-ACB3-49CE-9F4B-8B83E5726924" );

            // ----------------------------------
            // Revert the ContentChannelItemList block type name and description.

            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'B995BE3F-A9EB-4A18-AE24-E93A8796AEDE');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Content Item List'
        , [Description] = 'Lists content items.'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Content Item List'
    WHERE [BlockTypeId] = @BlockTypeId
        AND [Name] = 'Content Channel Item List';
END" );
        }
    }
}
