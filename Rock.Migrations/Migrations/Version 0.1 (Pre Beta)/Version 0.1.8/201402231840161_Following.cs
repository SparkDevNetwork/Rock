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
    public partial class Following : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Following",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.PersonAliasId);
            
            CreateIndex( "dbo.Following", "Guid", true );
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Following", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Following", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Following", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.Following", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Following", new[] { "PersonAliasId" });
            DropIndex("dbo.Following", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Following", new[] { "EntityTypeId" });
            DropIndex("dbo.Following", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.Following");
        }
    }
}
