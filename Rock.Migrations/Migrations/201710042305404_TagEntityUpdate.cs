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
    public partial class TagEntityUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.TaggedItem", "EntityTypeId", c => c.Int( nullable: false ) );

            Sql( @"
    UPDATE I SET [EntityTypeId] = T.[EntityTypeId]
    FROM [TaggedItem] I
    INNER JOIN [Tag] T ON T.[Id] = I.[TagId]
" );
            CreateIndex( "dbo.TaggedItem", "EntityTypeId" );
            AddForeignKey( "dbo.TaggedItem", "EntityTypeId", "dbo.EntityType", "Id" );

            DropIndex( "dbo.Tag", new[] { "EntityTypeId" });
            AlterColumn( "dbo.Tag", "EntityTypeId", c => c.Int() );
            CreateIndex( "dbo.Tag", "EntityTypeId" );

            AddColumn( "dbo.EntityType", "LinkUrlLavaTemplate", c => c.String());

            AddColumn("dbo.ContentChannel", "IsTaggingEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentChannel", "ItemTagCategoryId", c => c.Int());
            CreateIndex("dbo.ContentChannel", "ItemTagCategoryId");
            AddForeignKey("dbo.ContentChannel", "ItemTagCategoryId", "dbo.Category", "Id");
            DropColumn("dbo.ContentChannel", "ItemTagCategories");

            Sql( @"
    UPDATE [EntityType] SET [LinkUrlLavaTemplate] = '~/Person/{{ Entity.Id }}' WHERE [Name] = 'Rock.Model.Person'
" );

            Sql( @"
    DECLARE @TagEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Tag' )

    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
    SELECT @TagEntityTypeId, 0, ( ROW_NUMBER() OVER( ORDER BY [Name] DESC) ) - 1, 'Tag', 'A', 0, [Id], NEWID()
    FROM [Group] 
    WHERE [Guid] IN ( '628C51A8-4613-43ED-A18D-4A6FB999273E', '2C112948-FF4C-46E7-981A-0257681EADF4', '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )

    DECLARE @Order int = ( SELECT MAX([Order]) FROM [Auth] WHERE [EntityTypeId] = @TagEntityTypeId AND [EntityId] = 0 AND [Action] = 'Tag' )
    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
    VALUES ( @TagEntityTypeId, 0, @Order, 'Tag', 'D', 1, NEWID() )
" );

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Tag Categories", "", "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F", "fa fa-tag" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", @"", @"", 0, "EEFCAF48-E520-4383-A550-34CB1339951C" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "C405A507-7889-4287-8342-105B89710044", @"d34258d0-d366-4efb-aa76-84b059fb5434" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "8AFA681F-27F8-4BDC-90C1-F5FF4A112159", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "57530519-239F-473E-A44A-EC1E441C998E", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "EEFCAF48-E520-4383-A550-34CB1339951C", "69BB4619-AA98-4B36-962B-9062C9A55CB8", @"" );

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "EEFCAF48-E520-4383-A550-34CB1339951C" );
            RockMigrationHelper.DeletePage( "86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F" );

            AddColumn("dbo.ContentChannel", "ItemTagCategories", c => c.String(maxLength: 100));
            DropForeignKey("dbo.TaggedItem", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.ContentChannel", "ItemTagCategoryId", "dbo.Category");
            DropIndex("dbo.Tag", new[] { "EntityTypeId" });
            DropIndex("dbo.TaggedItem", new[] { "EntityTypeId" });
            DropIndex("dbo.ContentChannel", new[] { "ItemTagCategoryId" });
            AlterColumn("dbo.Tag", "EntityTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.TaggedItem", "EntityTypeId");
            DropColumn("dbo.ContentChannel", "ItemTagCategoryId");
            DropColumn("dbo.ContentChannel", "IsTaggingEnabled");
            DropColumn("dbo.EntityType", "LinkUrlLavaTemplate");
            CreateIndex("dbo.Tag", "EntityTypeId");
        }
    }
}
