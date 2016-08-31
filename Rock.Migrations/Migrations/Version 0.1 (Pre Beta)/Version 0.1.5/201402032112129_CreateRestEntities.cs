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
    public partial class CreateRestEntities : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RestAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ControllerId = c.Int(nullable: false),
                        Method = c.String(maxLength: 100),
                        ApiId = c.String(maxLength: 2000),
                        Path = c.String(maxLength: 2000),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RestController", t => t.ControllerId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ControllerId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId);
            
            CreateIndex( "dbo.RestAction", "Guid", true );
            
            CreateTable(
                "dbo.RestController",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        ClassName = c.String(maxLength: 500),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId);
            
            CreateIndex( "dbo.RestController", "Guid", true );

            UpdateEntityType( "Rock.Model.RestController", "65CDFD5B-A9AA-48FA-8D22-669612D5EA7D", true, true );

            Sql( @"

    UPDATE [Page] SET 
        [InternalName] = 'REST Controller Actions',
        [PageTitle] = 'REST Controller Actions',
        [BrowserTitle] = 'REST Controller Actions',
        [BreadCrumbDisplayName] = 0 WHERE [Guid] = '7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5'

    UPDATE [BlockType] SET
	    [Path] = '~/Blocks/Core/RestActionList.ascx',
	    [Name] = 'Rest Action List',
	    [Description] = 'Displays the actions for a given REST controller.'
    WHERE [Guid] = '20AD75DD-0DF3-49E9-9DB1-8537C12B1664'

    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid])
        SELECT [Id], 0, 0, 'Edit', 'A', 2, '665FABEC-2715-4D20-8F5E-E702176151F7'
        FROM [EntityType] 
        WHERE [Name] = 'Rock.Model.RestController'

" );

        }
        

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [Page] SET 
        [InternalName] = 'REST Controller Detail',
        [PageTitle] = 'REST Controller Detail',
        [BrowserTitle] = 'REST Controller Detail',
        [BreadCrumbDisplayName] = 1 WHERE [Guid] = '7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5'
    UPDATE [BlockType] SET
	    [Path] = '~/Blocks/Core/RestControllerDetail.ascx',
	    [Name] = 'Rest Controller Detail',
	    [Description] = 'Displays the details of the given rest controller.'
    WHERE [Guid] = '20AD75DD-0DF3-49E9-9DB1-8537C12B1664'
" );

            DropForeignKey( "dbo.RestAction", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey("dbo.RestAction", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RestAction", "ControllerId", "dbo.RestController");
            DropForeignKey("dbo.RestController", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RestController", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RestAction", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RestAction", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RestAction", new[] { "ControllerId" });
            DropIndex("dbo.RestController", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RestController", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.RestController");
            DropTable("dbo.RestAction");
        }
    }
}
