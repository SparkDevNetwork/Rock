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
    public partial class ConnectionRequests : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Connection Requests", "Allows you to view connection requests of a particular person.", "~/Blocks/Crm/PersonDetail/ConnectionRequests.ascx", "CRM > Person Detail", "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027" );
            RockMigrationHelper.UpdateBlockType( "Calendar Item Occurrence List By Audience Lava", "Block that takes a audience and displays calendar item occurrences for it using Lava.", "~/Blocks/Event/EventItemOccurrenceListByAudienceLava.ascx", "Event", "E4703964-7717-4C93-BD40-7DFF85EAC5FD" );
            RockMigrationHelper.UpdateBlockType( "Prayer Request List Lava", "List Prayer Requests using a Lava template.", "~/Blocks/Prayer/PrayerRequestListLava.ascx", "Prayer", "AF0B20C3-B969-4246-81CD-76CC443CFDEB" );
            RockMigrationHelper.UpdateBlockType( "Package Rating", "Enters ratings for a given package.", "~/Blocks/Store/PackageRating.ascx", "Store", "5A7C11C2-4E9F-4AF6-8149-CB2093CE9727" );
            
            // Add Block to Page: Person Profile, Site: Rock RMS
            RockMigrationHelper.AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "", "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027", "Connection Requests", "SectionA2", "", "", 1, "0A0C5C73-7BEF-447C-A5E8-3128F20832BA" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'BCFCB62D-9B83-4AF7-B884-DEB232A72506'" );  // Page: Person Profile,  Zone: SectionA2,  Block: Bookmarked Attributes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '0A0C5C73-7BEF-447C-A5E8-3128F20832BA'" );  // Page: Person Profile,  Zone: SectionA2,  Block: Connection Requests
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'D5CD2A87-D34E-42E0-A58A-5FFC9B72F136'" );  // Page: Person Profile,  Zone: SectionA2,  Block: Relationships
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '32847AAF-15F5-4F8B-9F84-92D6AE827857'" );  // Page: Person Profile,  Zone: SectionA2,  Block: Implied Relationship
            
            // Attrib for BlockType: Connection Requests:Connection Request Detail
            RockMigrationHelper.AddBlockTypeAttribute( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Request Detail", "ConnectionRequestDetail", "", "", 0, @"", "16016681-4FF6-4D51-B6FB-798C15A378F4" );

            // Attrib for BlockType: Person Search:Show Performance
            RockMigrationHelper.AddBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Performance", "ShowPerformance", "", "Displays how long the search took.", 0, @"False", "72AA66DB-C8BF-4CC7-94F5-4B8C3500A564" );

            // Attrib Value for Block:Connection Requests, Attribute:Connection Request Detail Page: Person Profile, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A0C5C73-7BEF-447C-A5E8-3128F20832BA", "16016681-4FF6-4D51-B6FB-798C15A378F4", @"50f04e77-8d3b-4268-80ab-bc15dd6cb262" );
            
            // MP: Updated Installers
            // update new location of checkscanner installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.4.0/checkscanner.exe' where [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
            // update new location of jobscheduler installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.4.0/jobscheduler.exe' where [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'" );
            // update new location of statementgenerator installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.4.0/statementgenerator.exe' where [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'" );
            // update new location of checkinclient installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.4.0/checkinclient.exe' where [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'" );

            // DT: Add index to transaction table for giving analytics
            Sql( @"   
    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_TransactionTypeValueId_TransactionDateTime' AND object_id = OBJECT_ID('FinancialTransaction'))
    BEGIN
		CREATE NONCLUSTERED INDEX [IX_TransactionTypeValueId_TransactionDateTime]
		ON [dbo].[FinancialTransaction] ([TransactionTypeValueId],[TransactionDateTime])
		INCLUDE ([Id],[AuthorizedPersonAliasId])
	END" );

            // DT: Remove caching on Transaction Links block
            // Attrib Value for Block:Transaction Links, Attribute:Cache Duration Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // MP: Update Email Footers
            Sql( @"
UPDATE Attribute
SET DefaultValue = Replace(DefaultValue, 'href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}"">{{ ''Global'' | Attribute:''OrganizationWebsite'' }}</a>', 'href=""{{ ''Global'' | Attribute:''OrganizationWebsite'' }}"">{{ ''Global'' | Attribute:''OrganizationWebsite'' }}</a>')
WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A'
    AND DefaultValue LIKE '%href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}"">{{ ''Global'' | Attribute:''OrganizationWebsite'' }}</a>%'

UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, '<a href=\""{{ GlobalAttribute.PublicApplicationRoot }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ GlobalAttribute.OrganizationWebsite }}</a>', '<a href=\""{{ GlobalAttribute.OrganizationWebsite }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ GlobalAttribute.OrganizationWebsite }}</a>')
WHERE [Guid] = 'AFE2ADD1-5278-441E-8E84-1DC743D99824'
    AND MediumDataJson LIKE '%<a href=\""{{ GlobalAttribute.PublicApplicationRoot }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ GlobalAttribute.OrganizationWebsite }}</a>%'
" );

            // add new note type
            Sql( @"IF  NOT EXISTS (SELECT * FROM [NoteType] WHERE [GUID] = 'FFFC3644-60CD-4D14-A714-E8DCC202A0E1')
  BEGIN
	DECLARE @GroupMemberEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '49668B95-FEDC-43DD-8085-D2B0D6343C48')
	
	INSERT INTO [NoteType]
		([IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue])
	VALUES
		(1, @GroupMemberEntityTypeId, 'Group Member Note', 'FFFC3644-60CD-4D14-A714-E8DCC202A0E1', 1, 'fa fa-users', '', '')
  END" );
            
            // add group member notes
            RockMigrationHelper.AddBlock( "3905C63F-4D57-40F0-9721-C60A2F681911", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "66443F51-D210-48E3-A8A9-063D3806F43F" );
            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"49668b95-fedc-43dd-8085-d2b0d6343c48" ); // Entity Type

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Group Member Notes" ); // Heading

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-comments-o" ); // Heading Icon CSS Class

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order

            RockMigrationHelper.AddBlockAttributeValue( "66443F51-D210-48E3-A8A9-063D3806F43F", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes

            // Add/Update PageContext for Page:Group Member Detail, Entity: Rock.Model.GroupMember, Parameter: GroupMemberId
            RockMigrationHelper.UpdatePageContext( "3905C63F-4D57-40F0-9721-C60A2F681911", "Rock.Model.GroupMember", "GroupMemberId", "5683CC10-31CA-4D63-8EA6-4E2186055BD1" );



        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Connection Requests:Connection Request Detail
            RockMigrationHelper.DeleteAttribute( "16016681-4FF6-4D51-B6FB-798C15A378F4" );

            // Remove Block: Connection Requests, from Page: Person Profile, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0A0C5C73-7BEF-447C-A5E8-3128F20832BA" );

            RockMigrationHelper.DeleteBlockType( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027" ); // Connection Requests
        }
    }
}
