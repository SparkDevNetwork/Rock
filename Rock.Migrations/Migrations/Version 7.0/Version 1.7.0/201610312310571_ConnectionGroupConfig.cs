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

            AddColumn( "dbo.ConnectionRequest", "AssignedGroupMemberRoleId", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "AssignedGroupMemberStatus", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "AssignedGroupMemberAttributeValues", c => c.String());

            DropColumn("dbo.ConnectionOpportunity", "GroupTypeId");
            DropColumn("dbo.ConnectionOpportunity", "GroupMemberRoleId");
            DropColumn("dbo.ConnectionOpportunity", "GroupMemberStatus");
            DropColumn("dbo.ConnectionOpportunity", "UseAllGroupsOfType");

            #region Migration Rollups

            // DT - Add bio attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Custom Content", "CustomContent", "", "Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.", 6, @"", "921C9ED8-483D-4532-BAC5-437E08516311" );

            // DT - New fieldtype and workflow action
            RockMigrationHelper.UpdateFieldType( "Workflow", "", "Rock", "Rock.Field.Types.WorkflowFieldType", "0F72D8FD-983F-41FB-A7A3-E6403EB04EDB" );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetStatusInOtherWorkflow", "8C66083E-1A6E-40F7-B3ED-3077FA82E65E", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8C66083E-1A6E-40F7-B3ED-3077FA82E65E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "588EEB91-0AC8-48C2-BA66-EBE465EA77B7" ); // Rock.Workflow.Action.SetStatusInOtherWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8C66083E-1A6E-40F7-B3ED-3077FA82E65E", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Workflow", "Workflow", "The workflow to set the status of.", 1, @"", "123914AB-1981-40B2-821F-033AA24E5F34" ); // Rock.Workflow.Action.SetStatusInOtherWorkflow:Workflow
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8C66083E-1A6E-40F7-B3ED-3077FA82E65E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Status", "Status", "The status to set workflow to. <span class='tip tip-lava'></span>", 0, @"", "A9DA2209-01DE-43EC-900C-FB580BE4E3E1" ); // Rock.Workflow.Action.SetStatusInOtherWorkflow:Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8C66083E-1A6E-40F7-B3ED-3077FA82E65E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "01AD93D4-D009-47EA-80FD-432176C608CA" ); // Rock.Workflow.Action.SetStatusInOtherWorkflow:Order

            // DT - Add Attribute block attribute
            RockMigrationHelper.UpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "", "A comma separated list of category guids to limit the display of attributes to.", 4, @"", "0C2BCD33-05CC-4B03-9F57-C686B8911E64" );

            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_DropIndexes );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnCrm_GetAge );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnCrm_GetGradeOffset );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnCrm_GetParentEmails );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnCrm_GetParentNames );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnCrm_GetParentPhones );
            Sql( MigrationSQL._201610312310571_ConnectionGroupConfig_ufnUtility_CsvToTable );

            #endregion
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
