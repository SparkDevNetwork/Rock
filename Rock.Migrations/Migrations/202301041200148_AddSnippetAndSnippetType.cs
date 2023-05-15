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
    public partial class AddSnippetAndSnippetType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Snippet",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    SnippetTypeId = c.Int( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    Content = c.String(),
                    Order = c.Int( nullable: false ),
                    OwnerPersonAliasId = c.Int(),
                    CategoryId = c.Int(),
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
                .ForeignKey( "dbo.Category", t => t.CategoryId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.OwnerPersonAliasId )
                .ForeignKey( "dbo.SnippetType", t => t.SnippetTypeId )
                .Index( t => t.SnippetTypeId )
                .Index( t => t.OwnerPersonAliasId )
                .Index( t => t.CategoryId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.SnippetType",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    HelpText = c.String(),
                    IsPersonalAllowed = c.Boolean( nullable: false ),
                    IsSharedAllowed = c.Boolean( nullable: false ),
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

            // Add SMS Snippet Type
            Sql( @"DECLARE @Guid uniqueidentifier = (SELECT [Guid] FROM [SnippetType] WHERE [Name] = 'SMS');
                IF @Guid IS NULL
                BEGIN
                    INSERT INTO [SnippetType] (
                        [Name],[Description],[HelpText],[IsPersonalAllowed],[IsSharedAllowed],[Guid])
                    VALUES(
                        'SMS', 'Snippets to be used for various SMS replies.', '', 1,1,'D6074803-9405-47E3-974C-E95C9AD05874')
                END" );

            // Add Snippets List Page to Communications Page
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.COMMUNICATIONS_ROCK_SETTINGS, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "SMS Snippets", string.Empty, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB", "fa fa-keyboard" );

            // Add Snippet Details Page
            RockMigrationHelper.AddPage( true, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB", SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Snippet Detail", string.Empty, "E315FCD1-3942-415E-BED2-E30428928955" );

            // Add Snippet Detail Obsidian block entity Type
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SnippetDetail", "Snippet Detail", "Rock.Blocks.Communication.SnippetDetail, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "4b445492-20e7-41e3-847a-f4d4723e9973" );

            // Add Snippet Detail Obsidian block
            UpdateBlockType( "Snippet Detail", "Displays details of a particular Snippet.", null, "Communication", "8b0f3048-99ba-4ed1-8de6-6a34f498f556", "4b445492-20e7-41e3-847a-f4d4723e9973" );

            // Update Snippet Type block setting attribute.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8b0f3048-99ba-4ed1-8de6-6a34f498f556", SystemGuid.FieldType.SINGLE_SELECT, "Snippet Type", "SnippetType", "Snippet Type", "", 0, "D6074803-9405-47E3-974C-E95C9AD05874", "48AE0214-5AB4-4307-ADD8-78BFB30462E0" );

            // Add Snippet Detail block to Snippet Detail Page
            RockMigrationHelper.AddBlock( true, "E315FCD1-3942-415E-BED2-E30428928955".AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "8B0F3048-99BA-4ED1-8DE6-6A34F498F556".AsGuid(), "Snippet Detail", "Main", @"", @"", 0, "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB" );

            // Set Snippet Type block setting Attribute Value to SMS Snippet Type for the newly added Snippet Detail block.
            RockMigrationHelper.AddBlockAttributeValue( "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB", "48AE0214-5AB4-4307-ADD8-78BFB30462E0", "D6074803-9405-47E3-974C-E95C9AD05874" );

            // Add Snippet List block Type
            RockMigrationHelper.UpdateBlockType( "Snippet List", "Displays Snippets to be used for various SMS replies.", "~/Blocks/Communication/SnippetList.ascx", "Communication", "2EDAD934-6129-480B-9812-4BA7B9978AD2" );

            // Update Snippet Detail block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.PAGE_REFERENCE, "Snippet Detail", "SnippetDetail", "Snippet Detail", "", 0, "", "58C5B522-7A82-4304-B1D4-CDDB73980B95" );

            // Update Snippet Type block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.SINGLE_SELECT, "Snippet Type", "SnippetType", "Snippet Type", "", 1, "", "A2CF5EF3-36A5-4340-A8BA-03C9BF7BEC96" );

            // Update Show Personal Column block setting attribute guid value.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", SystemGuid.FieldType.BOOLEAN, "Show Personal Column", "ShowPersonalColumn", "Show Personal Column", "", 2, "", "B83AAC68-1480-40DD-8812-12A7A2E72BD8" );

            // Add Snippet List block to Snippet List Page
            RockMigrationHelper.AddBlock( true, "67661F85-ECA6-4791-AE7A-D1454D7B1FEB".AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "2EDAD934-6129-480B-9812-4BA7B9978AD2".AsGuid(), "Snippet List", "Main", @"", @"", 0, "7171F489-F370-47BB-B012-14DB03011E1A" );

            // Set Snippet Type block setting Attribute Value to SMS Snippet Type for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "A2CF5EF3-36A5-4340-A8BA-03C9BF7BEC96", "D6074803-9405-47E3-974C-E95C9AD05874" );

            // Update Snippet List Page block setting DetailPage Attribute Value to SnippetDetail page for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "58C5B522-7A82-4304-B1D4-CDDB73980B95", "E315FCD1-3942-415E-BED2-E30428928955" );

            // Update Snippet List Page block setting ShowPersonal Attribute Value to True for the newly added Snippet List block.
            RockMigrationHelper.AddBlockAttributeValue( "7171F489-F370-47BB-B012-14DB03011E1A", "B83AAC68-1480-40DD-8812-12A7A2E72BD8", "True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Snippet", "SnippetTypeId", "dbo.SnippetType" );
            DropForeignKey( "dbo.SnippetType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SnippetType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Snippet", "OwnerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Snippet", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Snippet", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Snippet", "CategoryId", "dbo.Category" );
            DropIndex( "dbo.SnippetType", new[] { "Guid" } );
            DropIndex( "dbo.SnippetType", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SnippetType", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Snippet", new[] { "Guid" } );
            DropIndex( "dbo.Snippet", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Snippet", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Snippet", new[] { "CategoryId" } );
            DropIndex( "dbo.Snippet", new[] { "OwnerPersonAliasId" } );
            DropIndex( "dbo.Snippet", new[] { "SnippetTypeId" } );
            DropTable( "dbo.SnippetType" );
            DropTable( "dbo.Snippet" );

            // Remove Block - Name: Snippet List, from Page: SMS Snippets, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7171F489-F370-47BB-B012-14DB03011E1A" );

            // Remove Block - Name: Snippet Detail, from Page: Snippet Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DA0A5925-499C-4CE3-8ACA-A40C9D30B1CB" );

            // Delete Page Internal Name: Snippet Detail: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( "E315FCD1-3942-415E-BED2-E30428928955" );

            // Delete Page Internal Name: SMS Snippets: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( "67661F85-ECA6-4791-AE7A-D1454D7B1FEB" );
        }

        /// <summary>
        /// Updates the BlockType by Guid and sets the entity type id.
        /// otherwise it inserts a new record. In either case it will be marked IsSystem.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="path">The path.</param>
        /// <param name="category">The category.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="entityTypeGuid">The entity type GUID.</param>
        public void UpdateBlockType( string name, string description, string path, string category, string guid, string entityTypeGuid )
        {
            Sql( string.Format( @"
                DECLARE @Id int = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{4}' )
                DECLARE @EntityTypeId int = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{5}' )
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid],[EntityTypeId])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}',
                        '{4}',@EntityTypeId)
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET
                        [IsSystem] = 1,
                        [Category] = '{1}',
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [Guid] = '{4}',
                        [EntityTypeId] = @EntityTypeId
                    WHERE [Guid] = '{4}'
                END
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    entityTypeGuid
                    ) );
        }
    }
}
