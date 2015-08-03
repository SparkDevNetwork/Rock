// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class CalendarContentChannel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EventItemOccurrenceChannelItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemOccurrenceId = c.Int(nullable: false),
                        ContentChannelItemId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentChannelItem", t => t.ContentChannelItemId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventItemOccurrence", t => t.EventItemOccurrenceId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EventItemOccurrenceId)
                .Index(t => t.ContentChannelItemId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.EventCalendarContentChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventCalendarId = c.Int(nullable: false),
                        ContentChannelId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentChannel", t => t.ContentChannelId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventCalendar", t => t.EventCalendarId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EventCalendarId)
                .Index(t => t.ContentChannelId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);

            RockMigrationHelper.AddPage( "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Channel Item", "", "6DFA80C3-E2A4-479F-ADDF-98EAC31169E0", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Calendar Item Occurrence Content Channel Item List", "Lists the content channel items associated to a particular calendar item occurrence.", "~/Blocks/Event/CalendarContentChannelItemList.ascx", "Event", "8418C3B8-5E87-469F-BAE9-E15C32873FBD" );
            RockMigrationHelper.UpdateBlockType( "Calendar Navigation", "Displays icons to help with calendar administration navigation.", "~/Blocks/Event/CalendarNavigation.ascx", "Event", "84CC5DAC-238E-48B5-8499-8E97FB289EA9" );

            // Add Block to Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "", "8418C3B8-5E87-469F-BAE9-E15C32873FBD", "Calendar Item Occurrence Content Channel Item List", "Main", "", "", 2, "F0C1F229-EC1F-45F0-81A9-A41812280B68" );
            // Add Block to Page: Content Channel Item, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6DFA80C3-E2A4-479F-ADDF-98EAC31169E0", "", "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E", "Content Item Detail", "Main", "", "", 1, "BFE4D8F6-A626-4BBD-AA8D-A86A944F40A4" );
            // Add Block to Page: Calendars, Site: Rock RMS

            // Update block order of pages with nav so that nav can be inserted above them
            Sql( @" 
    UPDATE B SET [Order] = B.[Order] + 1
    FROM [Block] B
    INNER JOIN [Page] P ON P.[Id] = B.[PageId]
    WHERE P.[Guid] IN ( '63990874-0DFF-45FC-9F09-81B0B0D375B4', 'B54725E1-3640-4419-B580-2AF77DAF6568', '7FB33834-F40A-4221-8849-BB8C06903B04', '4B0C44EE-28E3-4753-A95B-8C57CD958FD1' )
" );
            RockMigrationHelper.AddBlock( "63990874-0DFF-45FC-9F09-81B0B0D375B4", "", "84CC5DAC-238E-48B5-8499-8E97FB289EA9", "Calendar Navigation", "Main", "", "", 0, "B5BD5EA1-D735-4CA1-B3E2-8B153CFE2D4E" );
            // Add Block to Page: Event Calendar, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B54725E1-3640-4419-B580-2AF77DAF6568", "", "84CC5DAC-238E-48B5-8499-8E97FB289EA9", "Calendar Navigation", "Main", "", "", 0, "CAD9D2AF-64E6-416F-A552-3FBFBDAE9BA2" );
            // Add Block to Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7FB33834-F40A-4221-8849-BB8C06903B04", "", "84CC5DAC-238E-48B5-8499-8E97FB289EA9", "Calendar Navigation", "Main", "", "", 0, "FA8FA49B-110E-4BC1-94D2-C674AC4C71AA" );
            // Add Block to Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "", "84CC5DAC-238E-48B5-8499-8E97FB289EA9", "Calendar Navigation", "Main", "", "", 0, "9039BEB5-31AF-4E72-9334-983496547376" );
            // Add Block to Page: Content Channel Item, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6DFA80C3-E2A4-479F-ADDF-98EAC31169E0", "", "84CC5DAC-238E-48B5-8499-8E97FB289EA9", "Calendar Navigation", "Main", "", "", 0, "3C2582C9-35F2-475A-A801-7EF09FAA5487" );

            // Attrib for BlockType: Calendar Item Occurrence Detail:Default Account
            RockMigrationHelper.AddBlockTypeAttribute( "C18CB1DC-B2BC-4D3F-918A-A047183E4024", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Default Account", "DefaultAccount", "", "The default account to use for new registration instances", 0, @"2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5", "02BD75A4-9C87-4B0A-9356-D995135F9080" );
            // Attrib for BlockType: Calendar Item Occurrence Detail:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "C18CB1DC-B2BC-4D3F-918A-A047183E4024", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page for viewing details about a group", 2, @"", "ECFDF94C-16A6-4182-9978-687E2B1D8969" );
            // Attrib for BlockType: Calendar Item Occurrence Detail:Registration Instance Page
            RockMigrationHelper.AddBlockTypeAttribute( "C18CB1DC-B2BC-4D3F-918A-A047183E4024", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "", "The page to view registration details", 1, @"", "14AF0BB1-7973-42DB-9F40-39E599776D27" );
            // Attrib for BlockType: Calendar Item Occurrence Content Channel Item List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "8418C3B8-5E87-469F-BAE9-E15C32873FBD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "B349EF4A-5BE2-470D-BEA6-F2C099907EBF" );
            // Attrib for BlockType: Calendar Item Occurrence List:Content Item Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Detail Page", "ContentItemDetailPage", "", "The page for viewing details about a content item", 3, @"", "D4C4DDD1-E99E-499B-A388-15EEDB29A9AE" );

            // Attrib Value for Block:Calendar Item Campus Detail, Attribute:Default Account Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "55564BCB-CC7D-40CE-BED9-B84BB9B88BDC", "02BD75A4-9C87-4B0A-9356-D995135F9080", @"2a6f9e5f-6859-44f1-ab0e-ce9cf6b08ee5" );
            // Attrib Value for Block:Calendar Item Campus Detail, Attribute:Group Detail Page Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "55564BCB-CC7D-40CE-BED9-B84BB9B88BDC", "ECFDF94C-16A6-4182-9978-687E2B1D8969", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Calendar Item Campus Detail, Attribute:Registration Instance Page Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "55564BCB-CC7D-40CE-BED9-B84BB9B88BDC", "14AF0BB1-7973-42DB-9F40-39E599776D27", @"844dc54b-daec-47b3-a63a-712dd6d57793" );
            // Attrib Value for Block:Calendar Item Occurrence Content Channel Item List, Attribute:Detail Page Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F0C1F229-EC1F-45F0-81A9-A41812280B68", "B349EF4A-5BE2-470D-BEA6-F2C099907EBF", @"6dfa80c3-e2a4-479f-addf-98eac31169e0" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:Content Item Detail Page Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "D4C4DDD1-E99E-499B-A388-15EEDB29A9AE", @"d18e837c-9e65-4a38-8647-dff04a595d97" );


            Sql( @"
-- Event Occurrence Page
UPDATE [Page] SET 
    [InternalName] = 'Event Occurrence',
    [PageTitle] = 'Event Occurrence',
    [BrowserTitle] = 'Event Occurrence'
WHERE [GUID] = '4B0C44EE-28E3-4753-A95B-8C57CD958FD1'
" );

            #region Migration Rollups

            // DT: Change registration template icon in category tree view to fa-clipboard
            RockMigrationHelper.AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Default Icon CSS Class", "DefaultIconCSSClass", "", "The icon CSS class to use when the treeview displays items that do not have an IconCSSClass property", 0, @"fa fa-list-ol", "D2596ADF-4455-42A4-848F-6DFD816C2867" );
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fa fa-clipboard" );

            // TC: Opportunity Lava Updates
            RockMigrationHelper.DeleteBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "8A631314-EAB9-458F-9A95-292C2F15F957" ); 
            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "8A631314-EAB9-458F-9A95-292C2F15F957", @"{% include '~~/Assets/Lava/OpportunitySearch.lava' %}" ); 
            RockMigrationHelper.DeleteBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "2DA45081-2E5B-47DF-B82B-1B8461FECBA7"); 
            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "2DA45081-2E5B-47DF-B82B-1B8461FECBA7", @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}" );

            // MP: Communication History Block
            RockMigrationHelper.UpdateBlockType( "Communication Recipient List", "Lists communications sent to an individual", "~/Blocks/Communication/CommunicationRecipientList.ascx", "Communication", "EBEA5996-5695-4A42-A21C-29E11E711BE8" );
            RockMigrationHelper.AddBlock( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "", "EBEA5996-5695-4A42-A21C-29E11E711BE8", "Communication History", "SectionC1", "", "", 2, "27F84ADB-AA13-439E-A130-FBF73698B172" );
            RockMigrationHelper.AddBlockTypeAttribute( "EBEA5996-5695-4A42-A21C-29E11E711BE8", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "B359D61E-6644-4BB3-BF12-6D4F9CFD3CE1" );
            RockMigrationHelper.AddBlockTypeAttribute( "EBEA5996-5695-4A42-A21C-29E11E711BE8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "8127D8B3-7698-46FD-B239-C924807F1EE4" );

            // JE: Fix misspelling
            RockMigrationHelper.AddBlockAttributeValue( "2356DEDC-803F-4782-A8E9-D0D88393EC2E", "1B0E8904-196B-433E-B1CC-937AD3CA5BF2", @"My Dashboard^~/MyDashboard|" );

            // MP: Followings Security
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Following", 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, 0, "81FD8765-C8FD-44B4-B4B7-107B23FEC07A" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Following", 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, 0, "A8D61688-4FC9-46A5-8669-5414EAE4838B" );

            Sql( MigrationSQL._201507281546100_CalendarContentChannel );

            // JE: Add missing communication detail page from Communication History
            RockMigrationHelper.AddBlockAttributeValue( "27F84ADB-AA13-439E-A130-FBF73698B172", "8127D8B3-7698-46FD-B239-C924807F1EE4", "2a22d08d-73a8-4aaf-ac7e-220e8b2e7857" );

            #endregion

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Calendar Item Occurrence List:Content Item Detail Page
            RockMigrationHelper.DeleteAttribute( "D4C4DDD1-E99E-499B-A388-15EEDB29A9AE" );
            // Attrib for BlockType: Calendar Item Occurrence Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute( "B349EF4A-5BE2-470D-BEA6-F2C099907EBF" );
            // Attrib for BlockType: Calendar Item Occurrence Detail:Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "14AF0BB1-7973-42DB-9F40-39E599776D27" );
            // Attrib for BlockType: Calendar Item Occurrence Detail:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "ECFDF94C-16A6-4182-9978-687E2B1D8969" );
            // Attrib for BlockType: Calendar Item Occurrence Detail:Default Account
            RockMigrationHelper.DeleteAttribute( "02BD75A4-9C87-4B0A-9356-D995135F9080" );

            // Remove Block: Calendar Admin Navigation, from Page: Content Channel Item, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3C2582C9-35F2-475A-A801-7EF09FAA5487" );
            // Remove Block: Calendar Admin Navigation, from Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9039BEB5-31AF-4E72-9334-983496547376" );
            // Remove Block: Calendar Admin Navigation, from Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FA8FA49B-110E-4BC1-94D2-C674AC4C71AA" );
            // Remove Block: Calendar Admin Navigation, from Page: Event Calendar, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CAD9D2AF-64E6-416F-A552-3FBFBDAE9BA2" );
            // Remove Block: Calendar Admin Navigation, from Page: Calendars, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B5BD5EA1-D735-4CA1-B3E2-8B153CFE2D4E" );
            // Remove Block: Content Item Detail, from Page: Content Channel Item, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BFE4D8F6-A626-4BBD-AA8D-A86A944F40A4" );
            // Remove Block: Calendar Item Occurrence Content Channel Item List, from Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F0C1F229-EC1F-45F0-81A9-A41812280B68" );

            RockMigrationHelper.DeleteBlockType( "84CC5DAC-238E-48B5-8499-8E97FB289EA9" ); // Calendar Admin Navigation
            RockMigrationHelper.DeleteBlockType( "8418C3B8-5E87-469F-BAE9-E15C32873FBD" ); // Calendar Item Occurrence Content Channel Item List

            RockMigrationHelper.DeletePage( "6DFA80C3-E2A4-479F-ADDF-98EAC31169E0" ); //  Page: Content Channel Item, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.EventCalendarContentChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventCalendarContentChannel", "EventCalendarId", "dbo.EventCalendar");
            DropForeignKey("dbo.EventCalendarContentChannel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventCalendarContentChannel", "ContentChannelId", "dbo.ContentChannel");
            DropForeignKey("dbo.EventItemOccurrenceChannelItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemOccurrenceChannelItem", "EventItemOccurrenceId", "dbo.EventItemOccurrence");
            DropForeignKey("dbo.EventItemOccurrenceChannelItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemOccurrenceChannelItem", "ContentChannelItemId", "dbo.ContentChannelItem");
            DropIndex("dbo.EventCalendarContentChannel", new[] { "ForeignId" });
            DropIndex("dbo.EventCalendarContentChannel", new[] { "Guid" });
            DropIndex("dbo.EventCalendarContentChannel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventCalendarContentChannel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventCalendarContentChannel", new[] { "ContentChannelId" });
            DropIndex("dbo.EventCalendarContentChannel", new[] { "EventCalendarId" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "ForeignId" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "Guid" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "ContentChannelItemId" });
            DropIndex("dbo.EventItemOccurrenceChannelItem", new[] { "EventItemOccurrenceId" });
            DropTable("dbo.EventCalendarContentChannel");
            DropTable("dbo.EventItemOccurrenceChannelItem");
        }
    }
}
