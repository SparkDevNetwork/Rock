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
    public partial class UpdateGroupRequirementFeatures : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupRequirement", "AppliesToAgeClassification", c => c.Int(nullable: false));
            AddColumn("dbo.GroupRequirement", "AppliesToDataViewId", c => c.Int());
            AddColumn("dbo.GroupRequirement", "AllowLeadersToOverride", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupRequirement", "DueDateAttributeId", c => c.Int());
            AddColumn("dbo.GroupRequirement", "DueDateStaticDate", c => c.DateTime());
            AddColumn("dbo.GroupRequirementType", "IconCssClass", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupRequirementType", "DueDateType", c => c.Int(nullable: false));
            AddColumn("dbo.GroupRequirementType", "DueDateOffsetInDays", c => c.Int());
            AddColumn("dbo.GroupRequirementType", "CategoryId", c => c.Int());
            AddColumn("dbo.GroupRequirementType", "DoesNotMeetWorkflowTypeId", c => c.Int());
            AddColumn("dbo.GroupRequirementType", "ShouldAutoInitiateDoesNotMeetWorkflow", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupRequirementType", "DoesNotMeetWorkflowLinkText", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupRequirementType", "WarningWorkflowTypeId", c => c.Int());
            AddColumn("dbo.GroupRequirementType", "ShouldAutoInitiateWarningWorkflow", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupRequirementType", "WarningWorkflowLinkText", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupRequirementType", "Summary", c => c.String(maxLength: 2000));
            AddColumn("dbo.GroupMemberRequirement", "DoesNotMeetWorkflowId", c => c.Int());
            AddColumn("dbo.GroupMemberRequirement", "WarningWorkflowId", c => c.Int());
            AddColumn("dbo.GroupMemberRequirement", "WasManuallyCompleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupMemberRequirement", "ManuallyCompletedByPersonAliasId", c => c.Int());
            AddColumn("dbo.GroupMemberRequirement", "ManuallyCompletedDateTime", c => c.DateTime());
            AddColumn("dbo.GroupMemberRequirement", "WasOverridden", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupMemberRequirement", "OverriddenByPersonAliasId", c => c.Int());
            AddColumn("dbo.GroupMemberRequirement", "OverriddenDateTime", c => c.DateTime());
            AddColumn("dbo.GroupMemberRequirement", "DueDate", c => c.DateTime());
            CreateIndex("dbo.GroupRequirement", "AppliesToDataViewId");
            CreateIndex("dbo.GroupRequirement", "DueDateAttributeId");
            CreateIndex("dbo.GroupRequirementType", "CategoryId");
            CreateIndex("dbo.GroupRequirementType", "DoesNotMeetWorkflowTypeId");
            CreateIndex("dbo.GroupRequirementType", "WarningWorkflowTypeId");
            CreateIndex("dbo.GroupMemberRequirement", "DoesNotMeetWorkflowId");
            CreateIndex("dbo.GroupMemberRequirement", "WarningWorkflowId");
            CreateIndex("dbo.GroupMemberRequirement", "ManuallyCompletedByPersonAliasId");
            CreateIndex("dbo.GroupMemberRequirement", "OverriddenByPersonAliasId");
            AddForeignKey("dbo.GroupRequirement", "AppliesToDataViewId", "dbo.DataView", "Id");
            AddForeignKey("dbo.GroupRequirement", "DueDateAttributeId", "dbo.Attribute", "Id");
            AddForeignKey("dbo.GroupRequirementType", "CategoryId", "dbo.Category", "Id");
            AddForeignKey("dbo.GroupRequirementType", "DoesNotMeetWorkflowTypeId", "dbo.WorkflowType", "Id");
            AddForeignKey("dbo.GroupRequirementType", "WarningWorkflowTypeId", "dbo.WorkflowType", "Id");
            AddForeignKey("dbo.GroupMemberRequirement", "DoesNotMeetWorkflowId", "dbo.Workflow", "Id");
            AddForeignKey("dbo.GroupMemberRequirement", "ManuallyCompletedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.GroupMemberRequirement", "OverriddenByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.GroupMemberRequirement", "WarningWorkflowId", "dbo.Workflow", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupMemberRequirement", "WarningWorkflowId", "dbo.Workflow");
            DropForeignKey("dbo.GroupMemberRequirement", "OverriddenByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupMemberRequirement", "ManuallyCompletedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupMemberRequirement", "DoesNotMeetWorkflowId", "dbo.Workflow");
            DropForeignKey("dbo.GroupRequirementType", "WarningWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.GroupRequirementType", "DoesNotMeetWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.GroupRequirementType", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.GroupRequirement", "DueDateAttributeId", "dbo.Attribute");
            DropForeignKey("dbo.GroupRequirement", "AppliesToDataViewId", "dbo.DataView");
            DropIndex("dbo.GroupMemberRequirement", new[] { "OverriddenByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "ManuallyCompletedByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "WarningWorkflowId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "DoesNotMeetWorkflowId" });
            DropIndex("dbo.GroupRequirementType", new[] { "WarningWorkflowTypeId" });
            DropIndex("dbo.GroupRequirementType", new[] { "DoesNotMeetWorkflowTypeId" });
            DropIndex("dbo.GroupRequirementType", new[] { "CategoryId" });
            DropIndex("dbo.GroupRequirement", new[] { "DueDateAttributeId" });
            DropIndex("dbo.GroupRequirement", new[] { "AppliesToDataViewId" });
            DropColumn("dbo.GroupMemberRequirement", "DueDate");
            DropColumn("dbo.GroupMemberRequirement", "OverriddenDateTime");
            DropColumn("dbo.GroupMemberRequirement", "OverriddenByPersonAliasId");
            DropColumn("dbo.GroupMemberRequirement", "WasOverridden");
            DropColumn("dbo.GroupMemberRequirement", "ManuallyCompletedDateTime");
            DropColumn("dbo.GroupMemberRequirement", "ManuallyCompletedByPersonAliasId");
            DropColumn("dbo.GroupMemberRequirement", "WasManuallyCompleted");
            DropColumn("dbo.GroupMemberRequirement", "WarningWorkflowId");
            DropColumn("dbo.GroupMemberRequirement", "DoesNotMeetWorkflowId");
            DropColumn("dbo.GroupRequirementType", "Summary");
            DropColumn("dbo.GroupRequirementType", "WarningWorkflowLinkText");
            DropColumn("dbo.GroupRequirementType", "ShouldAutoInitiateWarningWorkflow");
            DropColumn("dbo.GroupRequirementType", "WarningWorkflowTypeId");
            DropColumn("dbo.GroupRequirementType", "DoesNotMeetWorkflowLinkText");
            DropColumn("dbo.GroupRequirementType", "ShouldAutoInitiateDoesNotMeetWorkflow");
            DropColumn("dbo.GroupRequirementType", "DoesNotMeetWorkflowTypeId");
            DropColumn("dbo.GroupRequirementType", "CategoryId");
            DropColumn("dbo.GroupRequirementType", "DueDateOffsetInDays");
            DropColumn("dbo.GroupRequirementType", "DueDateType");
            DropColumn("dbo.GroupRequirementType", "IconCssClass");
            DropColumn("dbo.GroupRequirement", "DueDateStaticDate");
            DropColumn("dbo.GroupRequirement", "DueDateAttributeId");
            DropColumn("dbo.GroupRequirement", "AllowLeadersToOverride");
            DropColumn("dbo.GroupRequirement", "AppliesToDataViewId");
            DropColumn("dbo.GroupRequirement", "AppliesToAgeClassification");
        }
    }
}
