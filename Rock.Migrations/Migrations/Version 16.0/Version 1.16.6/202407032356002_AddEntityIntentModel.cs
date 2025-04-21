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
    public partial class AddEntityIntentModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EntityIntent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        IntentValueId = c.Int(nullable: false),
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.IntentValueId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.IntentValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.EntityIntent", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntityIntent", "IntentValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.EntityIntent", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.EntityIntent", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.EntityIntent", new[] { "Guid" });
            DropIndex("dbo.EntityIntent", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EntityIntent", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EntityIntent", new[] { "IntentValueId" });
            DropIndex("dbo.EntityIntent", new[] { "EntityTypeId" });
            DropTable("dbo.EntityIntent");
        }
    }
}
