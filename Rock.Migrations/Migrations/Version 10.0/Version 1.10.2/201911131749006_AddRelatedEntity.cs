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
    public partial class AddRelatedEntity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RelatedEntity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SourceEntityTypeId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                        TargetEntityTypeId = c.Int(nullable: false),
                        TargetEntityId = c.Int(nullable: false),
                        PurposeKey = c.String(maxLength: 100),
                        IsSystem = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        QualifierValue = c.String(maxLength: 200),
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
                .ForeignKey("dbo.EntityType", t => t.SourceEntityTypeId)
                .ForeignKey("dbo.EntityType", t => t.TargetEntityTypeId)
                .Index(t => new { t.SourceEntityTypeId, t.SourceEntityId, t.TargetEntityTypeId, t.TargetEntityId, t.PurposeKey }, unique: true, name: "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey")
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RelatedEntity", "TargetEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.RelatedEntity", "SourceEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.RelatedEntity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RelatedEntity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RelatedEntity", new[] { "Guid" });
            DropIndex("dbo.RelatedEntity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RelatedEntity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RelatedEntity", "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey");
            DropTable("dbo.RelatedEntity");
        }
    }
}
