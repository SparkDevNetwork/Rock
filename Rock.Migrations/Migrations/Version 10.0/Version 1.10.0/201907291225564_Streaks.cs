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
    public partial class Streaks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            TableChangesUp();
            PagesAndBlocksUp();
            JobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JobDown();
            PagesAndBlocksDown();
            TableChangesDown();
        }

        private void TableChangesUp()
        {
            RenameTable( name: "dbo.SequenceOccurrenceExclusion", newName: "StreakTypeExclusion" );
            RenameTable( name: "dbo.Sequence", newName: "StreakType" );
            RenameTable( name: "dbo.SequenceEnrollment", newName: "Streak" );
            RenameColumn( table: "dbo.Streak", name: "SequenceId", newName: "StreakTypeId" );
            RenameColumn( table: "dbo.StreakTypeExclusion", name: "SequenceId", newName: "StreakTypeId" );
            RenameIndex( table: "dbo.Streak", name: "IX_SequenceId", newName: "IX_StreakTypeId" );
            RenameIndex( table: "dbo.Streak", name: "IX_SequenceId_PersonAliasId", newName: "IX_StreakTypeId_PersonAliasId" );
            RenameIndex( table: "dbo.StreakTypeExclusion", name: "IX_SequenceId", newName: "IX_StreakTypeId" );
            AddColumn( "dbo.Streak", "InactiveDateTime", c => c.DateTime() );
            AddColumn( "dbo.Streak", "ExclusionMap", c => c.Binary() );
        }

        private void TableChangesDown()
        {
            DropColumn( "dbo.Streak", "ExclusionMap" );
            DropColumn( "dbo.Streak", "InactiveDateTime" );
            RenameIndex( table: "dbo.StreakTypeExclusion", name: "IX_StreakTypeId", newName: "IX_SequenceId" );
            RenameIndex( table: "dbo.Streak", name: "IX_StreakTypeId_PersonAliasId", newName: "IX_SequenceId_PersonAliasId" );
            RenameIndex( table: "dbo.Streak", name: "IX_StreakTypeId", newName: "IX_SequenceId" );
            RenameColumn( table: "dbo.StreakTypeExclusion", name: "StreakTypeId", newName: "SequenceId" );
            RenameColumn( table: "dbo.Streak", name: "StreakTypeId", newName: "SequenceId" );
            RenameTable( name: "dbo.Streak", newName: "SequenceEnrollment" );
            RenameTable( name: "dbo.StreakType", newName: "Sequence" );
            RenameTable( name: "dbo.StreakTypeExclusion", newName: "SequenceOccurrenceExclusion" );
        }

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Engagement", "", SystemGuid.Page.ENGAGEMENT, "" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.ENGAGEMENT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "7EF5C2D7-E20B-4955-B09B-E31F3CC20B42" );
            RockMigrationHelper.AddBlockAttributeValue( "7EF5C2D7-E20B-4955-B09B-E31F3CC20B42", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            RockMigrationHelper.MovePage( SystemGuid.Page.STEP_PROGRAMS, SystemGuid.Page.ENGAGEMENT );
            RockMigrationHelper.MovePage( SystemGuid.Page.STREAK_TYPES, SystemGuid.Page.ENGAGEMENT );

            RockMigrationHelper.UpdatePageIcon( SystemGuid.Page.STEP_PROGRAMS, "fa fa-map-marked-alt" );
            RockMigrationHelper.UpdatePageIcon( SystemGuid.Page.STREAK_TYPES, "fa fa-list-ol" );

            Sql( @"DELETE FROM BlockType WHERE Category = 'Sequences'" );

            RockMigrationHelper.UpdateBlockType( "Streak Detail", "Displays the details of the given streak for editing.", "~/Blocks/Streaks/StreakDetail.ascx", "Streaks", "EA9857FF-6703-4E4E-A6FF-65C23EBD2216" );
            RockMigrationHelper.UpdateBlockType( "Streak List", "Lists all the people enrolled in a streak type.", "~/Blocks/Streaks/StreakList.ascx", "Streaks", "46A5143E-8DE7-4E3D-96B3-674E8FD12949" );
            RockMigrationHelper.UpdateBlockType( "Streak Map Editor", "Allows editing a streak occurrence, engagement, or exclusion map.", "~/Blocks/Streaks/StreakMapEditor.ascx", "Streaks", "4DB69FBA-32C7-448A-B322-EDFBCEF2D124" );
            RockMigrationHelper.UpdateBlockType( "Streak Type Detail", "Displays the details of the given Streak Type for editing.", "~/Blocks/Streaks/StreakTypeDetail.ascx", "Streaks", "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6" );
            RockMigrationHelper.UpdateBlockType( "Streak Type Exclusion Detail", "Displays the details of the given Exclusion for editing.", "~/Blocks/Streaks/StreakTypeExclusionDetail.ascx", "Streaks", "21E9D4D3-9111-4E2F-A605-C4556BD62430" );
            RockMigrationHelper.UpdateBlockType( "Streak Type Exclusion List", "Lists all the exclusions for a streak type.", "~/Blocks/Streaks/StreakTypeExclusionList.ascx", "Streaks", "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2" );
            RockMigrationHelper.UpdateBlockType( "Streak Type List", "Shows a list of all streak types.", "~/Blocks/Streaks/StreakTypeList.ascx", "Streaks", "DDE31844-B024-472E-9B21-E094DFC40CAB" );
            // Add Block to Page: Sequences Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "DDE31844-B024-472E-9B21-E094DFC40CAB".AsGuid(), "Streak Type List", "Main", @"", @"", 0, "95714972-7E6A-4CB9-A928-846A44B741BC" );
            // Add Block to Page: Sequence Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6".AsGuid(), "Streak Type Detail", "Main", @"", @"", 0, "89C2453B-3290-4A7C-8ED9-499170AAFDC5" );
            // Add Block to Page: Sequence Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "46A5143E-8DE7-4E3D-96B3-674E8FD12949".AsGuid(), "Streak List", "Main", @"", @"", 1, "6EC93A4A-EABD-4FCE-8E25-5D1E18289D73" );
            // Add Block to Page: Map Editor Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E7D5B636-5F44-46D3-AE9F-E2681ACC7039".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4DB69FBA-32C7-448A-B322-EDFBCEF2D124".AsGuid(), "Streak Map Editor", "Main", @"", @"", 0, "3BF17B61-95D7-4EF9-A326-50D14D658088" );
            // Add Block to Page: Exclusions Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1EEDBA14-0EE1-43F7-BB8D-70455FD425E5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2".AsGuid(), "Streak Type Exclusion List", "Main", @"", @"", 0, "2317FDF3-AB03-4F40-B640-B2DE41F07134" );
            // Add Block to Page: Enrollment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "EA9857FF-6703-4E4E-A6FF-65C23EBD2216".AsGuid(), "Streak Detail", "Main", @"", @"", 0, "E3EF3E69-C5C7-418E-8153-A4B8B1774B39" );
            // Add Block to Page: Enrollment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4DB69FBA-32C7-448A-B322-EDFBCEF2D124".AsGuid(), "Streak Map Editor", "Main", @"", @"", 1, "366B8DDB-EBD1-455B-A426-4C6A35CE0842" );
            // Add Block to Page: Enrollment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4DB69FBA-32C7-448A-B322-EDFBCEF2D124".AsGuid(), "Streak Map Editor", "Main", @"", @"", 2, "71680B7B-0922-478C-B0F3-61A75D17F8CC" );
            // Add Block to Page: Exclusion Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "68EF459F-5D23-4930-8EA8-80CDF986BB94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21E9D4D3-9111-4E2F-A605-C4556BD62430".AsGuid(), "Streak Type Exclusion Detail", "Main", @"", @"", 0, "A88315E9-28A1-41E4-9B2D-D9B577C5CA13" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '89C2453B-3290-4A7C-8ED9-499170AAFDC5'" );  // Page: Sequence Detail,  Zone: Main,  Block: Streak Type Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'E3EF3E69-C5C7-418E-8153-A4B8B1774B39'" );  // Page: Enrollment,  Zone: Main,  Block: Streak Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '366B8DDB-EBD1-455B-A426-4C6A35CE0842'" );  // Page: Enrollment,  Zone: Main,  Block: Streak Map Editor
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '6EC93A4A-EABD-4FCE-8E25-5D1E18289D73'" );  // Page: Sequence Detail,  Zone: Main,  Block: Streak List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '71680B7B-0922-478C-B0F3-61A75D17F8CC'" );  // Page: Enrollment,  Zone: Main,  Block: Streak Map Editor
            // Attrib for BlockType: Streak List:Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "46A5143E-8DE7-4E3D-96B3-674E8FD12949", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set, a view profile button will show for each enrollment.", 2, @"", "D5A04517-BE11-49CF-90FD-CDF22E438BC8" );
            // Attrib for BlockType: Streak List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "46A5143E-8DE7-4E3D-96B3-674E8FD12949", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "1A9C5661-7547-4755-9197-FCDB62E1CBBC" );
            // Attrib for BlockType: Streak Map Editor:Show Streak Enrollment Exclusion Map
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4DB69FBA-32C7-448A-B322-EDFBCEF2D124", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Streak Enrollment Exclusion Map", "IsEngagementExclusion", "Show Streak Enrollment Exclusion Map", @"If this map editor is placed in the context of a streak enrollment, should it show the person exclusion map for that streak enrollment?", 0, @"False", "F86A78D5-1DEA-4622-9462-E0499A543297" );
            // Attrib for BlockType: Streak Type Detail:Exclusions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Exclusions Page", "ExclusionsPage", "Exclusions Page", @"Page used for viewing a list of streak type exclusions.", 2, @"", "C254E0FA-42C1-4F8B-A215-BD01EAD8796A" );
            // Attrib for BlockType: Streak Type Detail:Map Editor Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Map Editor Page", "MapEditorPage", "Map Editor Page", @"Page used for editing the streak type map.", 1, @"", "DB88757E-4D16-4025-A7DF-F4F4EB9822B2" );
            // Attrib for BlockType: Streak Type Exclusion List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "982BAD01-5818-408B-9ECF-2926BC440BCA" );
            // Attrib for BlockType: Streak Type List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDE31844-B024-472E-9B21-E094DFC40CAB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "70AAF361-DDB2-4372-8803-CF54DEFD9D91" );
            // Attrib Value for Block:Streak Type List, Attribute:Detail Page Page: Sequences, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "95714972-7E6A-4CB9-A928-846A44B741BC", "70AAF361-DDB2-4372-8803-CF54DEFD9D91", @"ca566b33-0265-45c5-b1b2-6ffa6d4743f4" );
            // Attrib Value for Block:Streak Type Detail, Attribute:Exclusions Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "89C2453B-3290-4A7C-8ED9-499170AAFDC5", "C254E0FA-42C1-4F8B-A215-BD01EAD8796A", @"1eedba14-0ee1-43f7-bb8d-70455fd425e5" );
            // Attrib Value for Block:Streak Type Detail, Attribute:Map Editor Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "89C2453B-3290-4A7C-8ED9-499170AAFDC5", "DB88757E-4D16-4025-A7DF-F4F4EB9822B2", @"e7d5b636-5f44-46d3-ae9f-e2681acc7039" );
            // Attrib Value for Block:Streak List, Attribute:Person Profile Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6EC93A4A-EABD-4FCE-8E25-5D1E18289D73", "D5A04517-BE11-49CF-90FD-CDF22E438BC8", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Attrib Value for Block:Streak List, Attribute:Detail Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6EC93A4A-EABD-4FCE-8E25-5D1E18289D73", "1A9C5661-7547-4755-9197-FCDB62E1CBBC", @"488be67c-eda0-489e-8d80-8cc67f5854d4" );
            // Attrib Value for Block:Streak List, Attribute:core.CustomGridEnableStickyHeaders Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6EC93A4A-EABD-4FCE-8E25-5D1E18289D73", "6D7E59B9-67E7-4F39-9B06-6D66B930E28B", @"False" );
            // Attrib Value for Block:Streak Type Exclusion List, Attribute:Detail Page Page: Exclusions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2317FDF3-AB03-4F40-B640-B2DE41F07134", "982BAD01-5818-408B-9ECF-2926BC440BCA", @"68ef459f-5d23-4930-8ea8-80cdf986bb94" );
            // Attrib Value for Block:Streak Map Editor, Attribute:Show Streak Enrollment Exclusion Map Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "71680B7B-0922-478C-B0F3-61A75D17F8CC", "F86A78D5-1DEA-4622-9462-E0499A543297", @"True" );
            RockMigrationHelper.UpdateFieldType( "Streak Type", "", "Rock", "Rock.Field.Types.StreakTypeFieldType", "F1411F4A-BD4B-4F80-9A83-94026C009F4D" );

            RockMigrationHelper.UpdatePageBreadcrumb( SystemGuid.Page.STREAK_TYPE_DETAIL, false );
            RockMigrationHelper.UpdatePageBreadcrumb( SystemGuid.Page.STREAK_TYPE_EXCLUSION_DETAIL, false );
            RockMigrationHelper.UpdatePageBreadcrumb( SystemGuid.Page.STREAK, false );

            RockMigrationHelper.RenamePage( SystemGuid.Page.STREAK_TYPES, "Streaks" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.STREAK_TYPE_DETAIL, "Streak Type" );
        }

        private void PagesAndBlocksDown()
        {
            // Attrib for BlockType: Streak Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "70AAF361-DDB2-4372-8803-CF54DEFD9D91" );
            // Attrib for BlockType: Streak Type Exclusion List:Detail Page
            RockMigrationHelper.DeleteAttribute( "982BAD01-5818-408B-9ECF-2926BC440BCA" );
            // Attrib for BlockType: Streak Type Detail:Map Editor Page
            RockMigrationHelper.DeleteAttribute( "DB88757E-4D16-4025-A7DF-F4F4EB9822B2" );
            // Attrib for BlockType: Streak Type Detail:Exclusions Page
            RockMigrationHelper.DeleteAttribute( "C254E0FA-42C1-4F8B-A215-BD01EAD8796A" );
            // Attrib for BlockType: Streak Map Editor:Show Streak Enrollment Exclusion Map
            RockMigrationHelper.DeleteAttribute( "F86A78D5-1DEA-4622-9462-E0499A543297" );
            // Attrib for BlockType: Streak List:Detail Page
            RockMigrationHelper.DeleteAttribute( "1A9C5661-7547-4755-9197-FCDB62E1CBBC" );
            // Attrib for BlockType: Streak List:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "D5A04517-BE11-49CF-90FD-CDF22E438BC8" );
            // Remove Block: Streak Map Editor, from Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "71680B7B-0922-478C-B0F3-61A75D17F8CC" );
            // Remove Block: Streak Map Editor, from Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "366B8DDB-EBD1-455B-A426-4C6A35CE0842" );
            // Remove Block: Streak Detail, from Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E3EF3E69-C5C7-418E-8153-A4B8B1774B39" );
            // Remove Block: Streak Type Exclusion Detail, from Page: Exclusion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A88315E9-28A1-41E4-9B2D-D9B577C5CA13" );
            // Remove Block: Streak Type Exclusion List, from Page: Exclusions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2317FDF3-AB03-4F40-B640-B2DE41F07134" );
            // Remove Block: Streak Map Editor, from Page: Map Editor, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3BF17B61-95D7-4EF9-A326-50D14D658088" );
            // Remove Block: Streak List, from Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6EC93A4A-EABD-4FCE-8E25-5D1E18289D73" );
            // Remove Block: Streak Type Detail, from Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "89C2453B-3290-4A7C-8ED9-499170AAFDC5" );
            // Remove Block: Streak Type List, from Page: Sequences, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "95714972-7E6A-4CB9-A928-846A44B741BC" );
            RockMigrationHelper.DeleteBlockType( "DDE31844-B024-472E-9B21-E094DFC40CAB" ); // Streak Type List
            RockMigrationHelper.DeleteBlockType( "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2" ); // Streak Type Exclusion List
            RockMigrationHelper.DeleteBlockType( "21E9D4D3-9111-4E2F-A605-C4556BD62430" ); // Streak Type Exclusion Detail
            RockMigrationHelper.DeleteBlockType( "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6" ); // Streak Type Detail
            RockMigrationHelper.DeleteBlockType( "4DB69FBA-32C7-448A-B322-EDFBCEF2D124" ); // Streak Map Editor
            RockMigrationHelper.DeleteBlockType( "46A5143E-8DE7-4E3D-96B3-674E8FD12949" ); // Streak List
            RockMigrationHelper.DeleteBlockType( "EA9857FF-6703-4E4E-A6FF-65C23EBD2216" ); // Streak Detail

            RockMigrationHelper.MovePage( SystemGuid.Page.STREAK_TYPES, SystemGuid.Page.REPORTING );
            RockMigrationHelper.MovePage( SystemGuid.Page.STEP_PROGRAMS, SystemGuid.Page.MANAGE );
            RockMigrationHelper.DeletePage( SystemGuid.Page.ENGAGEMENT ); //  Page: Engagement, Layout: Full Width, Site: Rock RMS
        }

        private void JobUp()
        {
            Sql( $@"
                UPDATE ServiceJob 
                SET
                    Name = 'Rebuild Streak Data',
                    Description = 'Rebuild streak maps. This runs on demand and has the cron expression set to the distant future since it does not run on a schedule.',
                    Class = 'Rock.Jobs.RebuildStreakMaps'
                WHERE
                    Guid = '{Rock.SystemGuid.ServiceJob.REBUILD_STREAK}';" );
        }

        private void JobDown()
        {
            // Intentionally blank
            // The name of sequences changed to streaks in the code. This migration fixes the class name of the job. Changing it back
            // to sequences will break since the code class name is the Streaks name.
        }
    }
}