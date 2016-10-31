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
    public partial class ConnectionGroupConfig : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.ConnectionOpportunity", "GroupMemberRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.ConnectionOpportunity", "GroupTypeId", "dbo.GroupType");

            DropIndex("dbo.ConnectionOpportunity", new[] { "GroupTypeId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "GroupMemberRoleId" });

            CreateTable(
                "dbo.ConnectionOpportunityGroupConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionOpportunityId = c.Int(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                        GroupMemberRoleId = c.Int(),
                        GroupMemberStatus = c.Int(nullable: false),
                        UseAllGroupsOfType = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupMemberRoleId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.GroupMemberRoleId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);

            Sql( @"
    INSERT INTO [dbo].[ConnectionOpportunityGroupConfig] ( [ConnectionOpportunityId], [GroupTypeId], [GroupMemberRoleId], [GroupMemberStatus], [UseAllGroupsOfType], [Guid] )
    SELECT [Id], [GroupTypeId], [GroupMemberRoleId], [GroupMemberStatus], [UseAllGroupsOfType], NEWID()
    FROM [dbo].[ConnectionOpportunity]
" );

            AddColumn("dbo.ConnectionRequest", "AssignedGroupMemberRoleId", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "AssignedGroupMemberStatus", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "AssignedGroupMemberAttributeValues", c => c.String());

            DropColumn("dbo.ConnectionOpportunity", "GroupTypeId");
            DropColumn("dbo.ConnectionOpportunity", "GroupMemberRoleId");
            DropColumn("dbo.ConnectionOpportunity", "GroupMemberStatus");
            DropColumn("dbo.ConnectionOpportunity", "UseAllGroupsOfType");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.ConnectionOpportunity", "UseAllGroupsOfType", c => c.Boolean(nullable: false));
            AddColumn("dbo.ConnectionOpportunity", "GroupMemberStatus", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionOpportunity", "GroupMemberRoleId", c => c.Int());
            AddColumn("dbo.ConnectionOpportunity", "GroupTypeId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ConnectionOpportunityGroupConfig", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityGroupConfig", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.ConnectionOpportunityGroupConfig", "GroupMemberRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.ConnectionOpportunityGroupConfig", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityGroupConfig", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "ForeignKey" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "ForeignGuid" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "Guid" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "GroupMemberRoleId" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "GroupTypeId" });
            DropIndex("dbo.ConnectionOpportunityGroupConfig", new[] { "ConnectionOpportunityId" });
            DropColumn("dbo.ConnectionRequest", "AssignedGroupMemberAttributeValues");
            DropColumn("dbo.ConnectionRequest", "AssignedGroupMemberStatus");
            DropColumn("dbo.ConnectionRequest", "AssignedGroupMemberRoleId");
            DropTable("dbo.ConnectionOpportunityGroupConfig");
            CreateIndex("dbo.ConnectionOpportunity", "GroupMemberRoleId");
            CreateIndex("dbo.ConnectionOpportunity", "GroupTypeId");
            AddForeignKey("dbo.ConnectionOpportunity", "GroupTypeId", "dbo.GroupType", "Id");
            AddForeignKey("dbo.ConnectionOpportunity", "GroupMemberRoleId", "dbo.GroupTypeRole", "Id");
        }
    }
}
