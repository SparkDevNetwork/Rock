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
    using Rock.Data;

    /// <summary>
    ///
    /// </summary>
    public partial class EntityAchievements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PagesAndBlocksFromHotfixMigrationUp();
            EntityTypeChangesUp();
            TableChangesUp();
            DataMigrationUp();
            PagesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesDown();
            TableChangesDown();
            EntityTypeChangesDown();
            PagesAndBlocksFromHotfixMigrationDown();
        }

        /// <summary>
        /// Originally hotfix 94
        /// </summary>
        private void PagesAndBlocksFromHotfixMigrationUp()
        {
            RockMigrationHelper.AddPage( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Achievement Types", "", "FCE0D006-F854-4107-9298-667563FA8D77", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "FCE0D006-F854-4107-9298-667563FA8D77", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Achievement Type", "", "1C378B3C-9721-4A9B-857A-E3C5188C5BF8", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Achievement Attempts", "", "4AC3D8B7-1A8A-40F9-8F51-8E09B863BA40", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "1C378B3C-9721-4A9B-857A-E3C5188C5BF8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attempt", "", "75CDD408-3E1B-4EF3-9A6F-4DC76B92A80F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "4AC3D8B7-1A8A-40F9-8F51-8E09B863BA40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attempt", "", "D3BE86DE-7237-4FE9-9F5D-3C08BD7F9F97", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Achievement Type List", "Shows a list of all achievement types.", "~/Blocks/Streaks/AchievementTypeList.ascx", "Streaks", "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD" );
            RockMigrationHelper.UpdateBlockType( "Achievement Type Detail", "Displays the details of the given Achievement Type for editing.", "~/Blocks/Streaks/AchievementTypeDetail.ascx", "Streaks", "4C4A46CD-1622-4642-A655-11585C5D3D31" );
            RockMigrationHelper.UpdateBlockType( "Achievement Attempt List", "Lists all the people that have made an attempt at earning an achievement.", "~/Blocks/Streaks/AchievementAttemptList.ascx", "Streaks", "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB" );
            RockMigrationHelper.UpdateBlockType( "Attempt Detail", "Displays the details of the given attempt for editing.", "~/Blocks/Streaks/AchievementAttemptDetail.ascx", "Streaks", "7E4663CD-2176-48D6-9CC2-2DBC9B880C23" );
            // Add Block to Page: Exclusion Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "68EF459F-5D23-4930-8EA8-80CDF986BB94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4DB69FBA-32C7-448A-B322-EDFBCEF2D124".AsGuid(), "Streak Map Editor", "Main", @"", @"", 1, "03F9510D-4E40-4ED4-8727-5AC0D62E4A78" );
            // Add Block to Page: Achievement Types Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FCE0D006-F854-4107-9298-667563FA8D77".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD".AsGuid(), "Achievement Type List", "Main", @"", @"", 0, "B131FE34-E236-4044-B5D0-22AD3F71A471" );
            // Add Block to Page: Achievement Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1C378B3C-9721-4A9B-857A-E3C5188C5BF8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4C4A46CD-1622-4642-A655-11585C5D3D31".AsGuid(), "Achievement Type Detail", "Main", @"", @"", 0, "169078DD-3255-4E84-A4C1-01C0440C1922" );
            // Add Block to Page: Achievement Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1C378B3C-9721-4A9B-857A-E3C5188C5BF8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB".AsGuid(), "Achievement Attempt List", "Main", @"", @"", 1, "D3E699FC-2D8E-4698-90D6-CA351C886AEC" );
            // Add Block to Page: Achievement Attempts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4AC3D8B7-1A8A-40F9-8F51-8E09B863BA40".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB".AsGuid(), "Achievement Attempt List", "Main", @"", @"", 0, "88079A6C-F2E8-4BC9-8C7F-DA7BBB5CEE70" );
            // Add Block to Page: Attempt Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "75CDD408-3E1B-4EF3-9A6F-4DC76B92A80F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7E4663CD-2176-48D6-9CC2-2DBC9B880C23".AsGuid(), "Attempt Detail", "Main", @"", @"", 0, "E9FAD400-E681-491D-9183-B719F78B5A49" );
            // Add Block to Page: Attempt Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D3BE86DE-7237-4FE9-9F5D-3C08BD7F9F97".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7E4663CD-2176-48D6-9CC2-2DBC9B880C23".AsGuid(), "Attempt Detail", "Main", @"", @"", 0, "09AA3694-A9C7-485D-9F1E-A14ADCDA5161" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '169078DD-3255-4E84-A4C1-01C0440C1922'" );  // Page: Achievement Type,  Zone: Main,  Block: Achievement Type Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'A88315E9-28A1-41E4-9B2D-D9B577C5CA13'" );  // Page: Exclusion,  Zone: Main,  Block: Streak Type Exclusion Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '03F9510D-4E40-4ED4-8727-5AC0D62E4A78'" );  // Page: Exclusion,  Zone: Main,  Block: Streak Map Editor
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'D3E699FC-2D8E-4698-90D6-CA351C886AEC'" );  // Page: Achievement Type,  Zone: Main,  Block: Achievement Attempt List
            // Attrib for BlockType: Streak Type Detail:Achievements Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievements Page", "AchievementsPage", "Achievements Page", @"Page used for viewing a list of streak type achievement types.", 3, @"", "6D4C96F2-8FDC-4A6D-AC35-CCD6EF180694" );
            // Attrib for BlockType: Achievement Type List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "6B9C55E0-8F30-4A17-BB6C-248208AD6CA6" );
            // Attrib for BlockType: Streak Detail:Achievement Attempts Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA9857FF-6703-4E4E-A6FF-65C23EBD2216", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Attempts Page", "AttemptsPage", "Achievement Attempts Page", @"Page used for viewing a list of achievement attempts for this streak.", 1, @"", "B718AC3A-BD27-49EF-86CB-73DEF7A57E28" );
            // Attrib for BlockType: Achievement Attempt List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page navigated to when a grid item is clicked.", 1, @"", "9C433A89-4D86-4E44-8B5E-03E4B00AEB9D" );
            // Attrib for BlockType: Attempt Detail:Streak Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Streak Page", "StreakPage", "Streak Page", @"Page used for viewing the streak that these attempts are derived from.", 1, @"", "888C0B9A-CB02-4EF9-822E-F4BE36117395" );
            // Attrib for BlockType: Attempt Detail:Achievement Type Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Type Page", "AchievementPage", "Achievement Type Page", @"Page used for viewing the achievement type that this attempt is toward.", 2, @"", "4B405599-63FB-4704-8CB6-4A4CCA4F3EA5" );
            // Attrib for BlockType: Workflow Entry:Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCssClass", "Block Title Icon CSS Class", @"The CSS class for the icon displayed in the block title. If not specified, the icon for the Workflow Type will be shown.", 3, @"", "A5A2897F-6448-45C2-BC9D-44F0E35E9192" );
            // Attrib for BlockType: Workflow Entry:Block Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Block Title Template", "BlockTitleTemplate", "Block Title Template", @"Lava template for determining the title of the block. If not specified, the name of the Workflow Type will be shown.", 2, @"", "6E7AC3BE-50D0-4219-BC6A-703821DC785F" );
            // Attrib Value for Block:Streak Type Detail, Attribute:Achievements Page Page: Streak Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "89C2453B-3290-4A7C-8ED9-499170AAFDC5", "6D4C96F2-8FDC-4A6D-AC35-CCD6EF180694", @"fce0d006-f854-4107-9298-667563fa8d77" );
            // Attrib Value for Block:Streak Detail, Attribute:Achievement Attempts Page Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E3EF3E69-C5C7-418E-8153-A4B8B1774B39", "B718AC3A-BD27-49EF-86CB-73DEF7A57E28", @"4ac3d8b7-1a8a-40f9-8f51-8e09b863ba40" );
            // Attrib Value for Block:Achievement Type List, Attribute:Detail Page Page: Achievement Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B131FE34-E236-4044-B5D0-22AD3F71A471", "6B9C55E0-8F30-4A17-BB6C-248208AD6CA6", @"1c378b3c-9721-4a9b-857a-e3c5188c5bf8" );
            // Attrib Value for Block:Achievement Type List, Attribute:core.CustomGridEnableStickyHeaders Page: Achievement Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B131FE34-E236-4044-B5D0-22AD3F71A471", "8EC071FC-02D7-408B-9775-9E2B37542FB2", @"False" );
            // Attrib Value for Block:Achievement Attempt List, Attribute:Detail Page Page: Achievement Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3E699FC-2D8E-4698-90D6-CA351C886AEC", "9C433A89-4D86-4E44-8B5E-03E4B00AEB9D", @"75cdd408-3e1b-4ef3-9a6f-4dc76b92a80f" );
            // Attrib Value for Block:Achievement Attempt List, Attribute:core.CustomGridEnableStickyHeaders Page: Achievement Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3E699FC-2D8E-4698-90D6-CA351C886AEC", "A57EE256-953B-457C-A76F-1781382CFB6C", @"False" );
            // Attrib Value for Block:Achievement Attempt List, Attribute:Detail Page Page: Achievement Attempts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88079A6C-F2E8-4BC9-8C7F-DA7BBB5CEE70", "9C433A89-4D86-4E44-8B5E-03E4B00AEB9D", @"d3be86de-7237-4fe9-9f5d-3c08bd7f9f97" );
            // Attrib Value for Block:Achievement Attempt List, Attribute:core.CustomGridEnableStickyHeaders Page: Achievement Attempts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88079A6C-F2E8-4BC9-8C7F-DA7BBB5CEE70", "A57EE256-953B-457C-A76F-1781382CFB6C", @"False" );
            // Attrib Value for Block:Attempt Detail, Attribute:Streak Page Page: Attempt, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9FAD400-E681-491D-9183-B719F78B5A49", "888C0B9A-CB02-4EF9-822E-F4BE36117395", @"488be67c-eda0-489e-8d80-8cc67f5854d4" );
            // Attrib Value for Block:Attempt Detail, Attribute:Achievement Type Page Page: Attempt, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "09AA3694-A9C7-485D-9F1E-A14ADCDA5161", "4B405599-63FB-4704-8CB6-4A4CCA4F3EA5", @"1c378b3c-9721-4a9b-857a-e3c5188c5bf8" );

            RockMigrationHelper.UpdatePageBreadcrumb( "1C378B3C-9721-4A9B-857A-E3C5188C5BF8", false );
            RockMigrationHelper.UpdatePageBreadcrumb( "75CDD408-3E1B-4EF3-9A6F-4DC76B92A80F", false );
            RockMigrationHelper.UpdatePageBreadcrumb( "D3BE86DE-7237-4FE9-9F5D-3C08BD7F9F97", false );
        }

        /// <summary>
        /// Originally hotfix 94
        /// </summary>
        private void PagesAndBlocksFromHotfixMigrationDown()
        {
            RockMigrationHelper.DeleteAttribute( "6E7AC3BE-50D0-4219-BC6A-703821DC785F" );
            // Attrib for BlockType: Workflow Entry:Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "A5A2897F-6448-45C2-BC9D-44F0E35E9192" );
            // Attrib for BlockType: Attempt Detail:Achievement Type Page
            RockMigrationHelper.DeleteAttribute( "4B405599-63FB-4704-8CB6-4A4CCA4F3EA5" );
            // Attrib for BlockType: Attempt Detail:Streak Page
            RockMigrationHelper.DeleteAttribute( "888C0B9A-CB02-4EF9-822E-F4BE36117395" );
            // Attrib for BlockType: Achievement Attempt List:Detail Page
            RockMigrationHelper.DeleteAttribute( "9C433A89-4D86-4E44-8B5E-03E4B00AEB9D" );
            // Attrib for BlockType: Streak Detail:Achievement Attempts Page
            RockMigrationHelper.DeleteAttribute( "B718AC3A-BD27-49EF-86CB-73DEF7A57E28" );
            // Attrib for BlockType: Achievement Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "6B9C55E0-8F30-4A17-BB6C-248208AD6CA6" );
            // Attrib for BlockType: Streak Type Detail:Achievements Page
            RockMigrationHelper.DeleteAttribute( "6D4C96F2-8FDC-4A6D-AC35-CCD6EF180694" );
            // Remove Block: Attempt Detail, from Page: Attempt, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "09AA3694-A9C7-485D-9F1E-A14ADCDA5161" );
            // Remove Block: Attempt Detail, from Page: Attempt, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E9FAD400-E681-491D-9183-B719F78B5A49" );
            // Remove Block: Streak Map Editor, from Page: Exclusion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "03F9510D-4E40-4ED4-8727-5AC0D62E4A78" );
            // Remove Block: Achievement Attempt List, from Page: Achievement Attempts, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "88079A6C-F2E8-4BC9-8C7F-DA7BBB5CEE70" );
            // Remove Block: Achievement Attempt List, from Page: Achievement Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D3E699FC-2D8E-4698-90D6-CA351C886AEC" );
            // Remove Block: Achievement Type Detail, from Page: Achievement Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "169078DD-3255-4E84-A4C1-01C0440C1922" );
            // Remove Block: Achievement Type List, from Page: Achievement Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B131FE34-E236-4044-B5D0-22AD3F71A471" );
            RockMigrationHelper.DeleteBlockType( "7E4663CD-2176-48D6-9CC2-2DBC9B880C23" ); // Attempt Detail
            RockMigrationHelper.DeleteBlockType( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB" ); // Achievement Attempt List
            RockMigrationHelper.DeleteBlockType( "4C4A46CD-1622-4642-A655-11585C5D3D31" ); // Achievement Type Detail
            RockMigrationHelper.DeleteBlockType( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD" ); // Achievement Type List
            RockMigrationHelper.DeletePage( "D3BE86DE-7237-4FE9-9F5D-3C08BD7F9F97" ); //  Page: Attempt, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "75CDD408-3E1B-4EF3-9A6F-4DC76B92A80F" ); //  Page: Attempt, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "4AC3D8B7-1A8A-40F9-8F51-8E09B863BA40" ); //  Page: Achievement Attempts, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1C378B3C-9721-4A9B-857A-E3C5188C5BF8" ); //  Page: Achievement Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "FCE0D006-F854-4107-9298-667563FA8D77" ); //  Page: Achievement Types, Layout: Full Width, Site: Rock RMS
        }

        private void PagesUp()
        {
            RockMigrationHelper.MovePage( SystemGuid.Page.ACHIEVEMENT_TYPES, SystemGuid.Page.ENGAGEMENT );
            RockMigrationHelper.RenamePage( SystemGuid.Page.ACHIEVEMENT_TYPES, "Achievements" );
        }

        private void PagesDown()
        {
            RockMigrationHelper.MovePage( SystemGuid.Page.ACHIEVEMENT_TYPES, SystemGuid.Page.STREAK_TYPES );
        }

        private void DataMigrationUp()
        {
            // Set defaults on achievement type knowing all existing records are related to streaks
            Sql(
@"UPDATE [AchievementType]
SET
    [SourceEntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Streak'),
    [AchieverEntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonAlias')" );

            // Transform the streak id value stored in AchieverEntityId (because of field rename) to a person alias id value
            Sql(
@"UPDATE aa
SET aa.[AchieverEntityId] = s.[PersonAliasId]
FROM 
	[AchievementAttempt] aa
	JOIN [Streak] s ON s.Id = aa.[AchieverEntityId]" );

            // Fix qualifier column values because of renamed fields
            Sql(
$@"UPDATE a
SET a.[EntityTypeQualifierColumn] = 'ComponentEntityTypeId'
FROM
    [Attribute] a
    JOIN [EntityType] et ON et.Id = a.EntityTypeId
WHERE
    a.[EntityTypeQualifierColumn] = 'AchievementEntityTypeId' AND
    et.[Guid] = '{SystemGuid.EntityType.ACHIEVEMENT_TYPE}'" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Streak", SystemGuid.EntityType.STREAK, true, true );
            Sql( $"UPDATE [EntityType] SET [IsAchievementsEnabled] = 1 WHERE [Guid] = '{ SystemGuid.EntityType.STREAK }';" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Interaction", SystemGuid.EntityType.INTERACTION, true, true );
            Sql( $"UPDATE [EntityType] SET [IsAchievementsEnabled] = 1 WHERE [Guid] = '{ SystemGuid.EntityType.INTERACTION }';" );

            RockMigrationHelper.AddEntityAttribute(
                "Rock.Model.AchievementType",
                "F1411F4A-BD4B-4F80-9A83-94026C009F4D",
                "ComponentEntityTypeId",
                "' + (SELECT Id FROM EntityType WHERE Guid = '174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD') + '",
                "Streak Type",
                string.Empty,
                "The source streak type from which achievements are earned.",
                4,
                string.Empty,
                "E926DAAE-980A-4BEE-9CF8-C3BF52F28D9D" );

            RockMigrationHelper.AddEntityAttribute(
                "Rock.Model.AchievementType",
                "F1411F4A-BD4B-4F80-9A83-94026C009F4D",
                "ComponentEntityTypeId",
                "' + (SELECT Id FROM EntityType WHERE Guid = '05D8CD17-E07D-4927-B9C4-5018F7C4B715') + '",
                "Streak Type",
                string.Empty,
                "The source streak type from which achievements are earned.",
                4,
                string.Empty,
                "BEDD14D0-450E-475C-8D9F-404DDE350530" );

            Sql(
$@"INSERT INTO AttributeValue (
	AttributeId,
	EntityId,
	Value,
	Guid,
	CreatedDateTime,
	ForeignKey,
	IsSystem
) SELECT
	a.Id,
	at.Id,
	st.Guid,
	NEWID(),
	GETDATE(),
	'Migrated from streak attempts',
	0
FROM
	AchievementType at
	JOIN StreakType st ON st.Id = at.StreakTypeId
	JOIN EntityType et ON et.Id = at.ComponentEntityTypeId
	JOIN Attribute a ON 
		a.EntityTypeQualifierColumn = 'ComponentEntityTypeId' 
		AND a.[Key] = 'StreakType'
		AND a.EntityTypeQualifierValue = at.ComponentEntityTypeId" );

            Sql(
@"UPDATE at
SET at.ComponentConfigJson = '{""StreakType"":""' + CONVERT(varchar(100), st.Guid) + '""}'
FROM 
	AchievementType at
	JOIN StreakType st ON st.Id = at.StreakTypeId" );

            DropColumn( "dbo.AchievementType", "StreakTypeId" );
        }

        private void EntityTypeChangesUp()
        {
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_ATTEMPT, "Rock.Model.StreakAchievementAttempt", "Rock.Model.AchievementAttempt", "Achievement Attempt" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE, "Rock.Model.StreakTypeAchievementType", "Rock.Model.AchievementType", "Achievement Type" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE_PREREQUISITE, "Rock.Model.StreakTypeAchievementTypePrerequisite", "Rock.Model.AchievementTypePrerequisite", "Achievement Type Prerequisite" );

            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.AccumulativeAchievement", "05D8CD17-E07D-4927-B9C4-5018F7C4B715", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.StreakAchievement", "174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD", false, true );
        }

        private void EntityTypeChangesDown()
        {
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_ATTEMPT, "Rock.Model.AchievementAttempt", "Rock.Model.StreakAchievementAttempt", "Streak Achievement Attempt" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE, "Rock.Model.AchievementType", "Rock.Model.StreakTypeAchievementType", "Streak Type Achievement Type" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE_PREREQUISITE, "Rock.Model.AchievementTypePrerequisite", "Rock.Model.StreakTypeAchievementTypePrerequisite", "Streak Type Achievement Type Prerequisite" );
        }

        private void RenameEntity( string guidString, string oldName, string newName, string friendlyName )
        {
            RockMigrationHelper.UpdateEntityType( oldName, guidString, true, true );
            RockMigrationHelper.RenameEntityType( guidString, newName, friendlyName, newName + ", Rock, Version=1.11.0.20, Culture=neutral, PublicKeyToken=null", true, true );
        }

        private void TableChangesUp()
        {
            DropForeignKey( "dbo.StreakTypeAchievementType", "StreakTypeId", "dbo.StreakType" );
            DropIndex( "dbo.StreakTypeAchievementType", new[] { "StreakTypeId" } );
            RenameTable( name: "dbo.StreakTypeAchievementType", newName: "AchievementType" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "PrerequisiteStreakTypeAchievementTypeId", newName: "PrerequisiteAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "StreakTypeAchievementTypeId", newName: "AchievementTypeId" );
            RenameColumn( table: "dbo.StreakAchievementAttempt", name: "StreakTypeAchievementTypeId", newName: "AchievementTypeId" );
            RenameIndex( table: "dbo.StreakAchievementAttempt", name: "IX_StreakTypeAchievementTypeId", newName: "IX_AchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_StreakTypeAchievementTypeId", newName: "IX_AchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_PrerequisiteStreakTypeAchievementTypeId", newName: "IX_PrerequisiteAchievementTypeId" );
            AddColumn( "dbo.EntityType", "IsAchievementsEnabled", c => c.Boolean( nullable: false ) );

            DropForeignKey( "dbo.StreakAchievementAttempt", "StreakId", "dbo.Streak" );
            RenameTable( name: "dbo.StreakAchievementAttempt", newName: "AchievementAttempt" );
            RenameTable( name: "dbo.StreakTypeAchievementTypePrerequisite", newName: "AchievementTypePrerequisite" );
            DropIndex( "dbo.AchievementAttempt", new[] { "StreakId" } );
            RenameColumn( table: "dbo.AchievementType", name: "AchievementEntityTypeId", newName: "ComponentEntityTypeId" );
            RenameIndex( table: "dbo.AchievementType", name: "IX_AchievementEntityTypeId", newName: "IX_ComponentEntityTypeId" );
            AddColumn( "dbo.AchievementType", "SourceEntityTypeId", c => c.Int() );
            AddColumn( "dbo.AchievementType", "AchieverEntityTypeId", c => c.Int( nullable: false ) );
            RenameColumn( table: "dbo.AchievementAttempt", name: "StreakId", newName: "AchieverEntityId" );

            AddColumn( "dbo.AchievementType", "ComponentConfigJson", c => c.String() );
        }

        private void TableChangesDown()
        {
            DropColumn( "dbo.AchievementType", "ComponentConfigJson" );

            RenameColumn( table: "dbo.AchievementAttempt", name: "AchieverEntityId", newName: "StreakId" );
            DropColumn( "dbo.AchievementType", "AchieverEntityTypeId" );
            DropColumn( "dbo.AchievementType", "SourceEntityTypeId" );
            RenameIndex( table: "dbo.AchievementType", name: "IX_ComponentEntityTypeId", newName: "IX_AchievementEntityTypeId" );
            RenameColumn( table: "dbo.AchievementType", name: "ComponentEntityTypeId", newName: "AchievementEntityTypeId" );
            CreateIndex( "dbo.AchievementAttempt", "StreakId" );
            AddForeignKey( "dbo.StreakAchievementAttempt", "StreakId", "dbo.Streak", "Id" );
            RenameTable( name: "dbo.AchievementTypePrerequisite", newName: "StreakTypeAchievementTypePrerequisite" );
            RenameTable( name: "dbo.AchievementAttempt", newName: "StreakAchievementAttempt" );

            DropColumn( "dbo.EntityType", "IsAchievementsEnabled" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_PrerequisiteAchievementTypeId", newName: "IX_PrerequisiteStreakTypeAchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_AchievementTypeId", newName: "IX_StreakTypeAchievementTypeId" );
            RenameIndex( table: "dbo.StreakAchievementAttempt", name: "IX_AchievementTypeId", newName: "IX_StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakAchievementAttempt", name: "AchievementTypeId", newName: "StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "AchievementTypeId", newName: "StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "PrerequisiteAchievementTypeId", newName: "PrerequisiteStreakTypeAchievementTypeId" );
            CreateIndex( "dbo.AchievementType", "StreakTypeId" );
            RenameTable( name: "dbo.AchievementType", newName: "StreakTypeAchievementType" );
            AddForeignKey( "dbo.StreakTypeAchievementType", "StreakTypeId", "dbo.StreakType", "Id", cascadeDelete: true );
        }
    }
}