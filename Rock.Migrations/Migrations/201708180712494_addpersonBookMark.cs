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
    public partial class addpersonBookMark : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonBookmark",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Url = c.String(nullable: false, maxLength: 2083),
                        Order = c.Int(),
                        CategoryId = c.Int(),
                        PersonAliasId = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonBookmark", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "CategoryId", "dbo.Category");
            DropIndex("dbo.PersonBookmark", new[] { "Guid" });
            DropIndex("dbo.PersonBookmark", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "PersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "CategoryId" });
            DropTable("dbo.PersonBookmark");
        }
    }
}
