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
    public partial class RefactorPersonBadge : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonBadge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.ModifiedByPersonAliasId);
            
            CreateIndex( "dbo.PersonBadge", "Guid", true );

            AddPage( "26547B83-A92D-4D7E-82ED-691F403F16B6", "195BCD57-1C10-4969-886F-7324B6287B75", "Person Profile Badge Detail", "", "D376EFD7-5B0D-44BF-A44D-03C466D2D30D", "" ); // Site:Rock RMS
            AddBlockType( "Person Badge Detail", "Shows the details of a particular person badge.", "~/Blocks/Administration/PersonBadgeDetail.ascx", "A79336CD-2265-4E36-B915-CF49956FD689" );
            AddBlockType( "Person Badge List", "Shows a list of all person badges.", "~/Blocks/Administration/PersonBadgeList.ascx", "D8CCD577-2200-44C5-9073-FD16F174D364" );

            // Add Block to Page: Person Profile Badge Detail, Site: Rock RMS
            AddBlock( "D376EFD7-5B0D-44BF-A44D-03C466D2D30D", "", "A79336CD-2265-4E36-B915-CF49956FD689", "Person Badge Detail", "Main", "", "", 0, "F20D430A-3E15-4AD9-A015-4F4D5A5A6DED" );

            // Attrib for BlockType: Person Badge List:Detail Page
            AddBlockTypeAttribute( "D8CCD577-2200-44C5-9073-FD16F174D364", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C4F9BFD0-8529-437A-9BAA-3A39289639E4" );

            // Attrib Value for Block:Person Badge List, Attribute:Detail Page Page: Person Profile Badges, Site: Rock RMS
            AddBlockAttributeValue( "C5B56466-6EAF-404A-A803-C2314B36C38F", "C4F9BFD0-8529-437A-9BAA-3A39289639E4", @"d376efd7-5b0d-44bf-a44d-03c466d2d30d" );

            UpdateFieldType( "Person Badges", "", "Rock", "Rock.Field.Types.PersonBadgesFieldType", "3F1AE891-7DC8-46D2-865D-11543B34FB60" );

            Sql(@"
    -- Update person badge list to use new block type
    DECLARE @BlockTypeId int
    SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'D8CCD577-2200-44C5-9073-FD16F174D364') 

    UPDATE [Block] SET 
	    [BlockTypeId] = @BlockTypeId,
	    [Name] = 'Person Badge List'
    WHERE [Guid] = 'C5B56466-6EAF-404A-A803-C2314B36C38F'

    -- Insert badge instance records
    DECLARE @ConnectionStatusBadge int
    DECLARE @CampusBadge int
    DECLARE @RecordStatusBadge int
    DECLARE @16WeekAttendanceBadge int
    DECLARE @FamilyAttendanceBadge int
    DECLARE @BaptizedBadge int

    SET @ConnectionStatusBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus')
    SET @CampusBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Campus')
    SET @RecordStatusBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.RecordStatus')
    SET @16WeekAttendanceBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.eraAttendanceAttendance')
    SET @FamilyAttendanceBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.FamilyAttendance')
    SET @BaptizedBadge = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.Baptized')

    INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
    VALUES
	    ('Connection Status', 'Displays the connection status of the individual.', @ConnectionStatusBadge, 0, '43B4800D-B995-459E-82FC-5FD575F9E348'),
	    ('Campus', 'Shows the campus of the individual.', @CampusBadge, 1, 'B21DCD49-AC35-4B2B-9857-75213209B643'),
	    ('Record Status', 'Displays the record status of individual.', @RecordStatusBadge, 1, 'A999125F-E2B8-48AD-AA25-DF94147E65C2'),
	    ('16 Week Attendance', 'Displays the number of times a family has checked-in in the last given number of weeks.', @16WeekAttendanceBadge, 1, '452CF317-D3A1-49B5-84B1-4206DDADC653'),
	    ('Family Attendance', 'Shows a chart of the attendance history with each bar representing one month.', @FamilyAttendanceBadge, 1, '3F7D648D-D6BA-4F03-931C-AFBDFA24BBD8'),
	    ('Baptized', 'Baptized Badge', @BaptizedBadge, 1,'44E4D9CD-8143-4333-8C66-89DF95B9F580')

    -- Update the Bio and BadgeList attributes to be a Person Badge field type
    DECLARE @PersonBadgeFieldTypeId int
    SET @PersonBadgeFieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '3F1AE891-7DC8-46D2-865D-11543B34FB60')
    UPDATE [Attribute]
    SET [FieldTypeId] = @PersonBadgeFieldTypeId
    WHERE [Guid] in ('8E11F65B-7272-4E9F-A4F1-89CE08E658DE','F5AB231E-3836-4D52-BD03-BF79773C237A')

    -- Set Bio badges
    UPDATE [AttributeValue] SET
    VALUE = '43B4800D-B995-459E-82FC-5FD575F9E348,B21DCD49-AC35-4B2B-9857-75213209B643,A999125F-E2B8-48AD-AA25-DF94147E65C2'
    WHERE [Guid] = 'E0C5374F-3B93-4A78-BAB3-854D022B0E96'

    -- Set first badge list badges
    UPDATE [AttributeValue] SET
    VALUE = '452CF317-D3A1-49B5-84B1-4206DDADC653,3F7D648D-D6BA-4F03-931C-AFBDFA24BBD8'
    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'

    -- Set second badge list badges
    UPDATE [AttributeValue] SET
    VALUE = ''
    WHERE [Guid] = 'F14BE42A-D356-4F06-9EE0-0396B1C487F6'

    -- Set third badge list badges
    UPDATE [AttributeValue] SET
    VALUE = '44E4D9CD-8143-4333-8C66-89DF95B9F580'
    WHERE [Guid] = 'AC977001-8059-43A1-AB35-55F37DA40695'
");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonBadge", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBadge", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.PersonBadge", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonBadge", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonBadge", new[] { "EntityTypeId" });
            DropIndex("dbo.PersonBadge", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.PersonBadge");
        }
    }
}
