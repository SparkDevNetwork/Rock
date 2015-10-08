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
    public partial class GroupRequirementsCommunicationApproval : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupRequirementType", "WarningSqlExpression", c => c.String());
            AddColumn("dbo.GroupRequirementType", "WarningDataViewId", c => c.Int());
            AddColumn("dbo.GroupMemberRequirement", "RequirementFailDateTime", c => c.DateTime());
            AddColumn("dbo.GroupMemberRequirement", "RequirementWarningDateTime", c => c.DateTime());
            AddColumn( "dbo.GroupRequirementType", "WarningLabel", c => c.String() );
            CreateIndex("dbo.GroupRequirementType", "WarningDataViewId");
            AddForeignKey("dbo.GroupRequirementType", "WarningDataViewId", "dbo.DataView", "Id");

            // remove edit access to the communications page for staff/staff-like
            Sql("DELETE FROM [Auth] WHERE [Guid] IN ('B4D9AE64-A51A-4ED1-88CA-ADB0DF279BE0', '352CA4FC-9F50-434E-B063-A2E63E8CF92C')");

            // new security role for communication approvals
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Communication Approvers", "Group of individuals who can approve communication requests.", Rock.SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS );

            // add approver rights to the communication blocks
            RockMigrationHelper.AddSecurityAuthForBlock( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", 0, "Approve", true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS, Model.SpecialRole.None, "24A17A65-72E6-E6AE-420A-7CC914D543B3" ); // communication entry
            RockMigrationHelper.AddSecurityAuthForBlock( "A02F7695-4C6E-44E9-84CB-42E6F51F285F", 0, "Approve", true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS, Model.SpecialRole.None, "E6501106-55A4-C3B9-4C00-4848DF89CC0D" ); // communication details

            RockMigrationHelper.AddSecurityAuthForBlock( "4578A82D-0C8A-4316-9047-7FEF2C13190B", 0, "Approve", true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS, Model.SpecialRole.None, "0CC29FA1-0D3D-5790-4889-137924AEC624" ); // communication list
            RockMigrationHelper.AddSecurityAuthForBlock( "4578A82D-0C8A-4316-9047-7FEF2C13190B", 0, "Approve", true, Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS, Model.SpecialRole.None, "2A173BEB-D20E-6584-4683-417140ED08B9" ); // communication list for communication admin    
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupRequirementType", "WarningDataViewId", "dbo.DataView");
            DropIndex("dbo.GroupRequirementType", new[] { "WarningDataViewId" });
            DropColumn("dbo.GroupMemberRequirement", "RequirementWarningDateTime");
            DropColumn("dbo.GroupMemberRequirement", "RequirementFailDateTime");
            DropColumn("dbo.GroupRequirementType", "WarningDataViewId");
            DropColumn("dbo.GroupRequirementType", "WarningSqlExpression");

            // remove security approver rights
            RockMigrationHelper.DeleteSecurityAuth( "24A17A65-72E6-E6AE-420A-7CC914D543B3" );
            RockMigrationHelper.DeleteSecurityAuth( "E6501106-55A4-C3B9-4C00-4848DF89CC0D" );
            RockMigrationHelper.DeleteSecurityAuth( "0CC29FA1-0D3D-5790-4889-137924AEC624" );

            RockMigrationHelper.DeleteSecurityAuth( "2A173BEB-D20E-6584-4683-417140ED08B9" );

            RockMigrationHelper.DeleteSecurityRoleGroup( Rock.SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS );
        }
    }
}
