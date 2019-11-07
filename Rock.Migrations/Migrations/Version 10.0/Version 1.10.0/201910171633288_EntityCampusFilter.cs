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
    public partial class EntityCampusFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EntityCampusFilter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
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
                .ForeignKey("dbo.Campus", t => t.CampusId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CampusId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.EntityCampusFilter", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntityCampusFilter", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.EntityCampusFilter", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntityCampusFilter", "CampusId", "dbo.Campus");
            DropIndex("dbo.EntityCampusFilter", new[] { "Guid" });
            DropIndex("dbo.EntityCampusFilter", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EntityCampusFilter", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EntityCampusFilter", new[] { "CampusId" });
            DropIndex("dbo.EntityCampusFilter", new[] { "EntityTypeId" });
            DropTable("dbo.EntityCampusFilter");
        }
    }
}
