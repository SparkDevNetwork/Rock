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
    public partial class Sequences : Rock.Migrations.RockMigration
    {
        private string _badgeGuid = "eb54dd37-d1f3-43dd-b949-e5d92bf5f05d";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelsUp();
            JobUp();
            PagesAndBlocksUp();
            BadgeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            BadgeDown();
            PagesAndBlocksDown();
            JobDown();
            ModelsDown();
        }

        private void BadgeUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.SequenceEngagement", _badgeGuid, false, false );
        }

        private void BadgeDown()
        {
            RockMigrationHelper.DeleteEntityType( _badgeGuid );
        }

        private void JobUp()
        {
            Sql( $@"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus,
                    CronExpression
                ) VALUES (
                    1, -- IsSystem
                    1, -- IsActive
                    'Rebuild Streak Data', -- Name
                    'Rebuild streak maps. This runs on demand and has the cron expression set to the distant future since it does not run on a schedule.', -- Description
                    'Rock.Jobs.RebuildStreakMaps', -- Class
                    '{Rock.SystemGuid.ServiceJob.REBUILD_STREAK}', -- Guid
                    GETDATE(), -- Created
                    1, -- All notifications
                    '{Rock.Model.ServiceJob.NeverScheduledCronExpression}' -- In the year 2200, so basically never run this scheduled since it runs on demand
                );" );
        }

        private void JobDown()
        {
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.REBUILD_STREAK}';" );
        }

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Sequences", "", "F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Sequence Detail", "", "CA566B33-0265-45C5-B1B2-6FFA6D4743F4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Map Editor", "", "E7D5B636-5F44-46D3-AE9F-E2681ACC7039", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Exclusions", "", "1EEDBA14-0EE1-43F7-BB8D-70455FD425E5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Enrollment", "", "488BE67C-EDA0-489E-8D80-8CC67F5854D4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "1EEDBA14-0EE1-43F7-BB8D-70455FD425E5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Exclusion", "", "68EF459F-5D23-4930-8EA8-80CDF986BB94", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Sequence Detail", "Displays the details of the given Sequence for editing.", "~/Blocks/Sequences/SequenceDetail.ascx", "Sequences", "7F15E482-5536-4E05-937E-8BE4DD96A57A" );
            RockMigrationHelper.UpdateBlockType( "Sequence Enrollment Detail", "Displays the details of the given Enrollment for editing.", "~/Blocks/Sequences/SequenceEnrollmentDetail.ascx", "Sequences", "F2E0795C-2512-49E5-AF08-F7B5FB67C596" );
            RockMigrationHelper.UpdateBlockType( "Sequence Enrollment List", "Lists all the enrollments in a sequence.", "~/Blocks/Sequences/SequenceEnrollmentList.ascx", "Sequences", "71004E9D-AA0F-49D9-B7E2-A430C9B89678" );
            RockMigrationHelper.UpdateBlockType( "Sequence Exclusion Detail", "Displays the details of the given Exclusion for editing.", "~/Blocks/Sequences/SequenceExclusionDetail.ascx", "Sequences", "F2B2F556-EA2F-47F3-8EAD-21BEF7D782AA" );
            RockMigrationHelper.UpdateBlockType( "Sequence Exclusion List", "Lists all the exclusions for a sequence.", "~/Blocks/Sequences/SequenceExclusionList.ascx", "Sequences", "0A8142BB-33B8-44EC-93BA-90AAE56B7F4D" );
            RockMigrationHelper.UpdateBlockType( "Sequence List", "Shows a list of all sequences.", "~/Blocks/Sequences/SequenceList.ascx", "Sequences", "394FF91F-5F61-4688-9582-889A6A89A272" );
            RockMigrationHelper.UpdateBlockType( "Sequence Map Editor", "Allows editing a sequence occurrence, engagement, or exclusion map.", "~/Blocks/Sequences/SequenceMapEditor.ascx", "Sequences", "E161B093-4330-40BB-BE39-8A56A9B0EC8B" );
            // Add Block to Page: Sequences Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "394FF91F-5F61-4688-9582-889A6A89A272".AsGuid(), "Sequence List", "Main", @"", @"", 0, "272EEF99-F0E2-44AC-B3CA-7EE0C7C91B0B" );
            // Add Block to Page: Sequence Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7F15E482-5536-4E05-937E-8BE4DD96A57A".AsGuid(), "Sequence Detail", "Main", @"", @"", 0, "DEFD1874-1673-498D-8482-5254E5962E44" );
            // Add Block to Page: Sequence Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CA566B33-0265-45C5-B1B2-6FFA6D4743F4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "71004E9D-AA0F-49D9-B7E2-A430C9B89678".AsGuid(), "Sequence Enrollment List", "Main", @"", @"", 1, "5B754C42-A96D-49E3-9791-C859D80F06DE" );
            // Add Block to Page: Map Editor Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E7D5B636-5F44-46D3-AE9F-E2681ACC7039".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E161B093-4330-40BB-BE39-8A56A9B0EC8B".AsGuid(), "Sequence Map Editor", "Main", @"", @"", 0, "F744F091-F2B1-46E5-8E5E-517F15789E82" );
            // Add Block to Page: Exclusions Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1EEDBA14-0EE1-43F7-BB8D-70455FD425E5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0A8142BB-33B8-44EC-93BA-90AAE56B7F4D".AsGuid(), "Sequence Exclusion List", "Main", @"", @"", 0, "6214A664-991E-4676-A60D-C326D642EC6A" );
            // Add Block to Page: Enrollment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E161B093-4330-40BB-BE39-8A56A9B0EC8B".AsGuid(), "Sequence Map Editor", "Main", @"", @"", 1, "B1263EE7-1A82-4BBD-A67D-8BF74561EBD1" );
            // Add Block to Page: Enrollment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "488BE67C-EDA0-489E-8D80-8CC67F5854D4".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F2E0795C-2512-49E5-AF08-F7B5FB67C596".AsGuid(), "Sequence Enrollment Detail", "Main", @"", @"", 0, "67E7938F-5464-4599-A1A2-8395840C983F" );
            // Add Block to Page: Exclusion Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "68EF459F-5D23-4930-8EA8-80CDF986BB94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F2B2F556-EA2F-47F3-8EAD-21BEF7D782AA".AsGuid(), "Sequence Exclusion Detail", "Main", @"", @"", 0, "3DDBC309-6E26-4216-90B0-D051A4F4689F" );
            // Add Block to Page: Exclusion Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "68EF459F-5D23-4930-8EA8-80CDF986BB94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E161B093-4330-40BB-BE39-8A56A9B0EC8B".AsGuid(), "Sequence Map Editor", "Main", @"", @"", 1, "142E2432-DF91-48C9-850C-FC2C00B31CBF" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '3DDBC309-6E26-4216-90B0-D051A4F4689F'" );  // Page: Exclusion,  Zone: Main,  Block: Sequence Exclusion Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '67E7938F-5464-4599-A1A2-8395840C983F'" );  // Page: Enrollment,  Zone: Main,  Block: Sequence Enrollment Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'DEFD1874-1673-498D-8482-5254E5962E44'" );  // Page: Sequence Detail,  Zone: Main,  Block: Sequence Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '142E2432-DF91-48C9-850C-FC2C00B31CBF'" );  // Page: Exclusion,  Zone: Main,  Block: Sequence Map Editor
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5B754C42-A96D-49E3-9791-C859D80F06DE'" );  // Page: Sequence Detail,  Zone: Main,  Block: Sequence Enrollment List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B1263EE7-1A82-4BBD-A67D-8BF74561EBD1'" );  // Page: Enrollment,  Zone: Main,  Block: Sequence Map Editor
                                                                                                             // Attrib for BlockType: Sequence Detail:Exclusions Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "7F15E482-5536-4E05-937E-8BE4DD96A57A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Exclusions Page", "ExclusionsPage", "", @"Page used for viewing a list of sequence exclusions.", 2, @"", "7BBB10A4-A5E1-4C02-883B-C612B76F000E" );
            // Attrib for BlockType: Sequence Detail:Map Editor Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "7F15E482-5536-4E05-937E-8BE4DD96A57A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Map Editor Page", "MapEditorPage", "", @"Page used for editing the sequence map.", 1, @"", "56157936-20AB-4EA9-BD33-70B911836C2C" );
            // Attrib for BlockType: Sequence Enrollment List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "71004E9D-AA0F-49D9-B7E2-A430C9B89678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "3157563C-373B-43AF-9D71-2B40FF68AF5A" );
            // Attrib for BlockType: Sequence Enrollment List:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "71004E9D-AA0F-49D9-B7E2-A430C9B89678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", @"Page used for viewing a person's profile. If set, a view profile button will show for each enrollment.", 2, @"", "7F9B1D76-BC45-40B3-8B58-EA32E8287C3C" );
            // Attrib for BlockType: Sequence Exclusion List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "0A8142BB-33B8-44EC-93BA-90AAE56B7F4D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "2E62C45F-DD05-4316-90D6-7DB593B9F5FA" );
            // Attrib for BlockType: Sequence List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "394FF91F-5F61-4688-9582-889A6A89A272", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "0B5830D9-473A-481C-A040-B98DD723ABEF" );
            // Attrib Value for Block:Sequence List, Attribute:Detail Page Page: Sequences, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "272EEF99-F0E2-44AC-B3CA-7EE0C7C91B0B", "0B5830D9-473A-481C-A040-B98DD723ABEF", @"ca566b33-0265-45c5-b1b2-6ffa6d4743f4" );
            // Attrib Value for Block:Sequence Detail, Attribute:Exclusions Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DEFD1874-1673-498D-8482-5254E5962E44", "7BBB10A4-A5E1-4C02-883B-C612B76F000E", @"1eedba14-0ee1-43f7-bb8d-70455fd425e5" );
            // Attrib Value for Block:Sequence Detail, Attribute:Map Editor Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DEFD1874-1673-498D-8482-5254E5962E44", "56157936-20AB-4EA9-BD33-70B911836C2C", @"e7d5b636-5f44-46d3-ae9f-e2681acc7039" );
            // Attrib Value for Block:Sequence Enrollment List, Attribute:Detail Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5B754C42-A96D-49E3-9791-C859D80F06DE", "3157563C-373B-43AF-9D71-2B40FF68AF5A", @"488be67c-eda0-489e-8d80-8cc67f5854d4" );
            // Attrib Value for Block:Sequence Enrollment List, Attribute:Person Profile Page Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5B754C42-A96D-49E3-9791-C859D80F06DE", "7F9B1D76-BC45-40B3-8B58-EA32E8287C3C", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Attrib Value for Block:Sequence Enrollment List, Attribute:core.CustomGridEnableStickyHeaders Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5B754C42-A96D-49E3-9791-C859D80F06DE", "D3B38811-EABF-4AC1-9A79-FAFA792107FF", @"False" );
        }

        private void PagesAndBlocksDown()
        {
            // Attrib for BlockType: Sequence List:Detail Page
            RockMigrationHelper.DeleteAttribute( "0B5830D9-473A-481C-A040-B98DD723ABEF" );
            // Attrib for BlockType: Sequence Exclusion List:Detail Page
            RockMigrationHelper.DeleteAttribute( "2E62C45F-DD05-4316-90D6-7DB593B9F5FA" );
            // Attrib for BlockType: Sequence Enrollment List:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "7F9B1D76-BC45-40B3-8B58-EA32E8287C3C" );
            // Attrib for BlockType: Sequence Enrollment List:Detail Page
            RockMigrationHelper.DeleteAttribute( "3157563C-373B-43AF-9D71-2B40FF68AF5A" );
            // Attrib for BlockType: Sequence Detail:Map Editor Page
            RockMigrationHelper.DeleteAttribute( "56157936-20AB-4EA9-BD33-70B911836C2C" );
            // Attrib for BlockType: Sequence Detail:Exclusions Page
            RockMigrationHelper.DeleteAttribute( "7BBB10A4-A5E1-4C02-883B-C612B76F000E" );
            // Remove Block: Sequence Map Editor, from Page: Exclusion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "142E2432-DF91-48C9-850C-FC2C00B31CBF" );
            // Remove Block: Sequence Exclusion Detail, from Page: Exclusion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3DDBC309-6E26-4216-90B0-D051A4F4689F" );
            // Remove Block: Sequence Exclusion List, from Page: Exclusions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6214A664-991E-4676-A60D-C326D642EC6A" );
            // Remove Block: Sequence Map Editor, from Page: Map Editor, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F744F091-F2B1-46E5-8E5E-517F15789E82" );
            // Remove Block: Sequence Map Editor, from Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B1263EE7-1A82-4BBD-A67D-8BF74561EBD1" );
            // Remove Block: Sequence Enrollment Detail, from Page: Enrollment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "67E7938F-5464-4599-A1A2-8395840C983F" );
            // Remove Block: Sequence Enrollment List, from Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5B754C42-A96D-49E3-9791-C859D80F06DE" );
            // Remove Block: Sequence Detail, from Page: Sequence Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DEFD1874-1673-498D-8482-5254E5962E44" );
            // Remove Block: Sequence List, from Page: Sequences, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "272EEF99-F0E2-44AC-B3CA-7EE0C7C91B0B" );
            RockMigrationHelper.DeleteBlockType( "E161B093-4330-40BB-BE39-8A56A9B0EC8B" ); // Sequence Map Editor
            RockMigrationHelper.DeleteBlockType( "394FF91F-5F61-4688-9582-889A6A89A272" ); // Sequence List
            RockMigrationHelper.DeleteBlockType( "0A8142BB-33B8-44EC-93BA-90AAE56B7F4D" ); // Sequence Exclusion List
            RockMigrationHelper.DeleteBlockType( "F2B2F556-EA2F-47F3-8EAD-21BEF7D782AA" ); // Sequence Exclusion Detail
            RockMigrationHelper.DeleteBlockType( "71004E9D-AA0F-49D9-B7E2-A430C9B89678" ); // Sequence Enrollment List
            RockMigrationHelper.DeleteBlockType( "F2E0795C-2512-49E5-AF08-F7B5FB67C596" ); // Sequence Enrollment Detail
            RockMigrationHelper.DeleteBlockType( "7F15E482-5536-4E05-937E-8BE4DD96A57A" ); // Sequence Detail
            RockMigrationHelper.DeletePage( "68EF459F-5D23-4930-8EA8-80CDF986BB94" ); //  Page: Exclusion, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "488BE67C-EDA0-489E-8D80-8CC67F5854D4" ); //  Page: Enrollment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1EEDBA14-0EE1-43F7-BB8D-70455FD425E5" ); //  Page: Exclusions, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "E7D5B636-5F44-46D3-AE9F-E2681ACC7039" ); //  Page: Map Editor, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "CA566B33-0265-45C5-B1B2-6FFA6D4743F4" ); //  Page: Sequence Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3" ); //  Page: Sequences, Layout: Full Width, Site: Rock RMS
        }

        private void ModelsUp()
        {
            CreateTable(
                "dbo.SequenceOccurrenceExclusion",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SequenceId = c.Int( nullable: false ),
                    LocationId = c.Int(),
                    ExclusionMap = c.Binary(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Location", t => t.LocationId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Sequence", t => t.SequenceId, cascadeDelete: true )
                .Index( t => t.SequenceId )
                .Index( t => t.LocationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.Sequence",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 250 ),
                    Description = c.String(),
                    StructureType = c.Int(),
                    StructureEntityId = c.Int(),
                    EnableAttendance = c.Boolean( nullable: false ),
                    RequiresEnrollment = c.Boolean( nullable: false ),
                    OccurrenceFrequency = c.Int( nullable: false ),
                    StartDate = c.DateTime( nullable: false, storeType: "date" ),
                    OccurrenceMap = c.Binary(),
                    IsActive = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.SequenceEnrollment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    SequenceId = c.Int( nullable: false ),
                    PersonAliasId = c.Int( nullable: false ),
                    EnrollmentDate = c.DateTime( nullable: false, storeType: "date" ),
                    LocationId = c.Int(),
                    EngagementMap = c.Binary(),
                    CurrentStreakStartDate = c.DateTime(),
                    CurrentStreakCount = c.Int( nullable: false ),
                    LongestStreakStartDate = c.DateTime(),
                    LongestStreakEndDate = c.DateTime(),
                    LongestStreakCount = c.Int( nullable: false ),
                    EngagementCount = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Location", t => t.LocationId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true )
                .ForeignKey( "dbo.Sequence", t => t.SequenceId, cascadeDelete: true )
                .Index( t => t.SequenceId )
                .Index( t => new { t.SequenceId, t.PersonAliasId }, unique: true )
                .Index( t => t.PersonAliasId )
                .Index( t => t.LocationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void ModelsDown()
        {
            DropForeignKey( "dbo.SequenceOccurrenceExclusion", "SequenceId", "dbo.Sequence" );
            DropForeignKey( "dbo.SequenceEnrollment", "SequenceId", "dbo.Sequence" );
            DropForeignKey( "dbo.SequenceEnrollment", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SequenceEnrollment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SequenceEnrollment", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo.SequenceEnrollment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Sequence", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Sequence", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SequenceOccurrenceExclusion", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SequenceOccurrenceExclusion", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo.SequenceOccurrenceExclusion", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.SequenceEnrollment", new[] { "Guid" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "LocationId" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "PersonAliasId" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "SequenceId", "PersonAliasId" } );
            DropIndex( "dbo.SequenceEnrollment", new[] { "SequenceId" } );
            DropIndex( "dbo.Sequence", new[] { "Guid" } );
            DropIndex( "dbo.Sequence", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Sequence", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.SequenceOccurrenceExclusion", new[] { "Guid" } );
            DropIndex( "dbo.SequenceOccurrenceExclusion", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SequenceOccurrenceExclusion", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.SequenceOccurrenceExclusion", new[] { "LocationId" } );
            DropIndex( "dbo.SequenceOccurrenceExclusion", new[] { "SequenceId" } );
            DropTable( "dbo.SequenceEnrollment" );
            DropTable( "dbo.Sequence" );
            DropTable( "dbo.SequenceOccurrenceExclusion" );
        }
    }
}
