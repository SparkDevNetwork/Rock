// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupRequirements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupRequirement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        GroupRequirementTypeId = c.Int(nullable: false),
                        GroupRoleId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .ForeignKey("dbo.GroupRequirementType", t => t.GroupRequirementTypeId, cascadeDelete: true)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupRoleId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupId)
                .Index(t => t.GroupRequirementTypeId)
                .Index(t => t.GroupRoleId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.GroupRequirementType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        CanExpire = c.Boolean(nullable: false),
                        ExpireInDays = c.Int(),
                        RequirementCheckType = c.Int(nullable: false),
                        SqlExpression = c.String(),
                        DataViewId = c.Int(),
                        PositiveLabel = c.String(maxLength: 150),
                        NegativeLabel = c.String(maxLength: 150),
                        CheckboxLabel = c.String(maxLength: 150),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.DataViewId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.GroupMemberRequirement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupMemberId = c.Int(nullable: false),
                        GroupRequirementId = c.Int(nullable: false),
                        RequirementMetDateTime = c.DateTime(),
                        LastRequirementCheckDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupMember", t => t.GroupMemberId, cascadeDelete: true)
                .ForeignKey("dbo.GroupRequirement", t => t.GroupRequirementId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupMemberId)
                .Index(t => t.GroupRequirementId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.Group", "MustMeetRequirementsToAddMember", c => c.Boolean());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupMemberRequirement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupMemberRequirement", "GroupRequirementId", "dbo.GroupRequirement");
            DropForeignKey("dbo.GroupMemberRequirement", "GroupMemberId", "dbo.GroupMember");
            DropForeignKey("dbo.GroupMemberRequirement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "GroupRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.GroupRequirement", "GroupRequirementTypeId", "dbo.GroupRequirementType");
            DropForeignKey("dbo.GroupRequirementType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirementType", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.GroupRequirementType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupRequirement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.GroupMemberRequirement", new[] { "ForeignId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "Guid" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "GroupRequirementId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "GroupMemberId" });
            DropIndex("dbo.GroupRequirementType", new[] { "ForeignId" });
            DropIndex("dbo.GroupRequirementType", new[] { "Guid" });
            DropIndex("dbo.GroupRequirementType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupRequirementType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupRequirementType", new[] { "DataViewId" });
            DropIndex("dbo.GroupRequirement", new[] { "ForeignId" });
            DropIndex("dbo.GroupRequirement", new[] { "Guid" });
            DropIndex("dbo.GroupRequirement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupRequirement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupRequirement", new[] { "GroupRoleId" });
            DropIndex("dbo.GroupRequirement", new[] { "GroupRequirementTypeId" });
            DropIndex("dbo.GroupRequirement", new[] { "GroupId" });
            DropColumn("dbo.Group", "MustMeetRequirementsToAddMember");
            DropTable("dbo.GroupMemberRequirement");
            DropTable("dbo.GroupRequirementType");
            DropTable("dbo.GroupRequirement");
        }
    }
}
