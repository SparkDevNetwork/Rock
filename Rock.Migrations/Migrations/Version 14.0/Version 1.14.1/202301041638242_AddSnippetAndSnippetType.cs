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
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        SnippetTypeId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Content = c.String(),
                        Order = c.Int(nullable: false),
                        OwnerPersonAliasId = c.Int(),
                        CategoryId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.OwnerPersonAliasId)
                .ForeignKey("dbo.SnippetType", t => t.SnippetTypeId)
                .Index(t => t.SnippetTypeId)
                .Index(t => t.OwnerPersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.SnippetType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        HelpText = c.String(),
                        IsPersonalAllowed = c.Boolean(nullable: false),
                        IsSharedAllowed = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Snippet", "SnippetTypeId", "dbo.SnippetType");
            DropForeignKey("dbo.SnippetType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SnippetType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Snippet", "OwnerPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Snippet", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Snippet", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Snippet", "CategoryId", "dbo.Category");
            DropIndex("dbo.SnippetType", new[] { "Guid" });
            DropIndex("dbo.SnippetType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SnippetType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Snippet", new[] { "Guid" });
            DropIndex("dbo.Snippet", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Snippet", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Snippet", new[] { "CategoryId" });
            DropIndex("dbo.Snippet", new[] { "OwnerPersonAliasId" });
            DropIndex("dbo.Snippet", new[] { "SnippetTypeId" });
            DropTable("dbo.SnippetType");
            DropTable("dbo.Snippet");
        }
    }
}
