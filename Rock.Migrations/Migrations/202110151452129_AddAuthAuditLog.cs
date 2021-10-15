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
    public partial class AddAuthAuditLog : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AuthAuditLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        Action = c.String(nullable: false, maxLength: 50),
                        ChangeType = c.Int(nullable: false),
                        ChangeDateTime = c.DateTime(nullable: false),
                        ChangeByPersonAliasId = c.Int(),
                        PreAllowOrDeny = c.String(maxLength: 1),
                        PostAllowOrDeny = c.String(maxLength: 1),
                        PreOrder = c.Int(),
                        PostOrder = c.Int(),
                        GroupId = c.Int(),
                        SpecialRole = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.ChangeByPersonAliasId, cascadeDelete: true)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.ChangeByPersonAliasId)
                .Index(t => t.GroupId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AuthAuditLog", "GroupId", "dbo.Group");
            DropForeignKey("dbo.AuthAuditLog", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.AuthAuditLog", "ChangeByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.AuthAuditLog", new[] { "Guid" });
            DropIndex("dbo.AuthAuditLog", new[] { "GroupId" });
            DropIndex("dbo.AuthAuditLog", new[] { "ChangeByPersonAliasId" });
            DropIndex("dbo.AuthAuditLog", new[] { "EntityTypeId" });
            DropTable("dbo.AuthAuditLog");
        }
    }
}
