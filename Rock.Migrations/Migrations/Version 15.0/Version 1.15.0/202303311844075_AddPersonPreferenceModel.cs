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
    public partial class AddPersonPreferenceModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonPreference",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        Key = c.String(nullable: false, maxLength: 250),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        Value = c.String(),
                        IsEnduring = c.Boolean(nullable: false),
                        LastAccessedDateTime = c.DateTime(nullable: false),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => new { t.PersonAliasId, t.Key }, unique: true, name: "IX_PersonAliasIdKey")
                .Index(t => t.EntityTypeId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonPreference", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonPreference", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.PersonPreference", new[] { "Guid" });
            DropIndex("dbo.PersonPreference", new[] { "EntityTypeId" });
            DropIndex("dbo.PersonPreference", "IX_PersonAliasIdKey");
            DropTable("dbo.PersonPreference");
        }
    }
}
