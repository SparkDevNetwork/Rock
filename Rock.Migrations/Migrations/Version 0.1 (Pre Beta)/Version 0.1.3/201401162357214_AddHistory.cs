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
    public partial class AddHistory : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.History",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Caption = c.String(maxLength: 200),
                        Summary = c.String(),
                        RelatedEntityTypeId = c.Int(),
                        RelatedEntityId = c.Int(),
                        CreatedByPersonId = c.Int(),
                        CreationDateTime = c.DateTime(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Person", t => t.CreatedByPersonId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.EntityType", t => t.RelatedEntityTypeId)
                .Index(t => t.CategoryId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.RelatedEntityTypeId);
            
            CreateIndex( "dbo.History", "Guid", true );

            AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "History Categories", "", "95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC", "fa fa-folder" ); // Site:Rock Internal
            AddBlockType( "Person History", "Block for displaying the history of changes to a particular user.", "~/Blocks/Crm/PersonDetail/PersonHistory.ascx", "854C7AE2-6FA4-4D1A-BBB5-012484EA436E" );
            // Add Block to Page: History Categories, Site: Rock Internal
            AddBlock( "95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Feature", "", "", 0, "405A3E43-6890-45E0-BE38-86E520C59004" );
            // Add Block to Page: History, Site: Rock Internal
            AddBlock( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "", "854C7AE2-6FA4-4D1A-BBB5-012484EA436E", "Person History", "SectionC1", "", "", 0, "F98649D7-E522-46CB-8F67-01DB7F59E3AA" );
            // Attrib Value for Block:Categories, Attribute:Entity Type Page: History Categories, Site: Rock Internal
            AddBlockAttributeValue( "405A3E43-6890-45E0-BE38-86E520C59004", "C405A507-7889-4287-8342-105B89710044", @"546d5f43-1184-47c9-8265-2d7bf4e1bca5" );

            UpdateEntityType( "Rock.Model.History", "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", true, true );

            Sql( @"
    UPDATE [Category] SET IconCssClass = 'fa fa-book' WHERE [Guid] = 'E919E722-F895-44A4-B86D-38DB8FBA1844'
    UPDATE [Category] SET IconCssClass = 'fa fa-compass' WHERE [Guid] = '7B879922-5DA6-41EE-AC0B-45CEFFB99458'
    UPDATE [Category] SET IconCssClass = 'fa fa-smile-o' WHERE [Guid] = '752DC692-836E-4A3E-B670-4325CD7724BF'
    UPDATE [Category] SET IconCssClass = 'fa fa-suitcase' WHERE [Guid] = 'F6B98D0C-197D-433A-917B-0C39A80A79E8'
    UPDATE [Category] SET IconCssClass = 'fa fa-certificate' WHERE [Guid] = '9AF28593-E631-41E4-B696-78015A4D6F7B'

    DECLARE @CategoryId INT
    DECLARE @HistoryEntityTypeId INT
    SET @HistoryEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.History')

    INSERT INTO [Category] ([IsSystem], [EntityTypeId], [Name], [Guid], [Order])
    VALUES (1, @HistoryEntityTypeId, 'Person', '6F09163D-7DDD-4E1E-8D18-D7CAA04451A7', 0)
    SET @CategoryId = SCOPE_IDENTITY()

    INSERT INTO [Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [Guid], [Order]) VALUES
    (1, @CategoryId, @HistoryEntityTypeId, 'Demographic Changes', '51D3EC5A-D079-45ED-909E-B0AB2FD06835' , 0),
    (1, @CategoryId, @HistoryEntityTypeId, 'Group Membership', '325278A4-FACA-4F38-A405-9C090B3BAA34' , 2),
    (1, @CategoryId, @HistoryEntityTypeId, 'Communications', 'F291034B-7581-48F3-B522-E31B8534D529' , 3),
    (1, @CategoryId, @HistoryEntityTypeId, 'Activity', '0836845E-5ED8-4ABE-8787-3B61EF2F0FA5' , 4),
    (1, @CategoryId, @HistoryEntityTypeId, 'Family Changes', '5C4CCE5A-D7D0-492F-A241-96E13A3F7DF8' , 1)
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Person History, from Page: History, Site: Rock Internal
            DeleteBlock( "F98649D7-E522-46CB-8F67-01DB7F59E3AA" );
            // Remove Block: Categories, from Page: History Categories, Site: Rock Internal
            DeleteBlock( "405A3E43-6890-45E0-BE38-86E520C59004" );
            DeleteBlockType( "854C7AE2-6FA4-4D1A-BBB5-012484EA436E" ); // Person History
            DeletePage( "95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC" ); // Page: History CategoriesLayout: Full Width, Site: Rock Internal

            DropForeignKey( "dbo.History", "RelatedEntityTypeId", "dbo.EntityType" );
            DropForeignKey("dbo.History", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.History", "CreatedByPersonId", "dbo.Person");
            DropForeignKey("dbo.History", "CategoryId", "dbo.Category");
            DropIndex("dbo.History", new[] { "RelatedEntityTypeId" });
            DropIndex("dbo.History", new[] { "EntityTypeId" });
            DropIndex("dbo.History", new[] { "CreatedByPersonId" });
            DropIndex("dbo.History", new[] { "CategoryId" });
            DropTable("dbo.History");

            Sql( @"
    DELETE [Category] WHERE [Guid] IN (
        '51D3EC5A-D079-45ED-909E-B0AB2FD06835',
        '325278A4-FACA-4F38-A405-9C090B3BAA34',
        'F291034B-7581-48F3-B522-E31B8534D529',
        '0836845E-5ED8-4ABE-8787-3B61EF2F0FA5',
        '5C4CCE5A-D7D0-492F-A241-96E13A3F7DF8'
    ) 

    DELETE [Category] WHERE [Guid] = '6F09163D-7DDD-4E1E-8D18-D7CAA04451A7'

" );
        
        }
    }
}
