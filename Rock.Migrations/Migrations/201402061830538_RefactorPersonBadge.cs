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
    public partial class RefactorPersonBadge : Rock.Migrations.RockMigration
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
            
            Sql( @"
    DECLARE @BlockTypeId int
    SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'D8CCD577-2200-44C5-9073-FD16F174D364') 

    UPDATE [Block] SET 
	    [BlockTypeId] = @BlockTypeId,
	    [Name] = 'Person Badge List'
    WHERE [Guid] = 'C5B56466-6EAF-404A-A803-C2314B36C38F'
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DECLARE @BlockTypeId int
    SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '21F5F466-59BC-40B2-8D73-7314D936C3CB') 

    UPDATE [Block] SET 
	    [BlockTypeId] = @BlockTypeId,
	    [Name] = 'Badge Components'
    WHERE [Guid] = 'C5B56466-6EAF-404A-A803-C2314B36C38F'
" );

            // Attrib for BlockType: Person Badge List:Detail Page
            DeleteAttribute( "C4F9BFD0-8529-437A-9BAA-3A39289639E4" );
            // Remove Block: Person Badge Detail, from Page: Person Profile Badge Detail, Site: Rock RMS
            DeleteBlock( "F20D430A-3E15-4AD9-A015-4F4D5A5A6DED" );
            DeleteBlockType( "D8CCD577-2200-44C5-9073-FD16F174D364" ); // Person Badge List
            DeleteBlockType( "A79336CD-2265-4E36-B915-CF49956FD689" ); // Person Badge Detail
            DeletePage( "D376EFD7-5B0D-44BF-A44D-03C466D2D30D" ); // Page: Person Profile Badge DetailLayout: Full Width Panel, Site: Rock RMS

            DropForeignKey( "dbo.PersonBadge", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey("dbo.PersonBadge", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.PersonBadge", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonBadge", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonBadge", new[] { "EntityTypeId" });
            DropIndex("dbo.PersonBadge", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.PersonBadge");
        }
    }
}
