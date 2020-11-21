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
    public partial class GroupDemographics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupDemographicType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        ComponentEntityTypeId = c.Int(nullable: false),
                        RoleFilter = c.String(maxLength: 100),
                        IsAutomated = c.Boolean(nullable: false),
                        LastRunDurationSeconds = c.Int(),
                        RunOnPersonUpdate = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.EntityType", t => t.ComponentEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.ComponentEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.GroupDemographicValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        GroupDemographicTypeId = c.Int(nullable: false),
                        RelatedEntityTypeId = c.Int(),
                        RelatedEntityId = c.Int(),
                        Value = c.String(),
                        ValueAsGuid = c.Guid(),
                        ValueAsNumeric = c.Decimal(precision: 18, scale: 2),
                        ValueAsBoolean = c.Boolean(),
                        LastCalculatedDateTime = c.DateTime(),
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
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupDemographicType", t => t.GroupDemographicTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.RelatedEntityTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.GroupDemographicTypeId)
                .Index(t => t.RelatedEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupDemographicValue", "RelatedEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.GroupDemographicValue", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupDemographicValue", "GroupDemographicTypeId", "dbo.GroupDemographicType");
            DropForeignKey("dbo.GroupDemographicValue", "GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupDemographicValue", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupDemographicType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupDemographicType", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.GroupDemographicType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupDemographicType", "ComponentEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.GroupDemographicValue", new[] { "Guid" });
            DropIndex("dbo.GroupDemographicValue", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupDemographicValue", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupDemographicValue", new[] { "RelatedEntityTypeId" });
            DropIndex("dbo.GroupDemographicValue", new[] { "GroupDemographicTypeId" });
            DropIndex("dbo.GroupDemographicValue", new[] { "GroupId" });
            DropIndex("dbo.GroupDemographicType", new[] { "Guid" });
            DropIndex("dbo.GroupDemographicType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupDemographicType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupDemographicType", new[] { "ComponentEntityTypeId" });
            DropIndex("dbo.GroupDemographicType", new[] { "GroupTypeId" });
            DropTable("dbo.GroupDemographicValue");
            DropTable("dbo.GroupDemographicType");
        }
    }
}
