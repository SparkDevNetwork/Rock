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
    public partial class GroupRequirementsGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.GroupRequirement", "IDX_GroupRequirementTypeGroup");
            AddColumn("dbo.GroupRequirement", "GroupTypeId", c => c.Int());
            AddColumn("dbo.GroupRequirement", "MustMeetRequirementToAddMember", c => c.Boolean(nullable: false));
            AlterColumn("dbo.GroupRequirement", "GroupId", c => c.Int());
            CreateIndex("dbo.GroupRequirement", new[] { "GroupId", "GroupTypeId", "GroupRequirementTypeId", "GroupRoleId" }, unique: true, name: "IDX_GroupRequirementTypeGroup");
            AddForeignKey("dbo.GroupRequirement", "GroupTypeId", "dbo.GroupType", "Id");

            Sql( @"UPDATE gr
SET gr.MustMeetRequirementToAddMember = isnull(g.MustMeetRequirementsToAddMember, 0)
FROM GroupRequirement gr
INNER JOIN [Group] g ON gr.GroupId = g.Id" );

            DropColumn("dbo.Group", "MustMeetRequirementsToAddMember");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Group", "MustMeetRequirementsToAddMember", c => c.Boolean());
            DropForeignKey("dbo.GroupRequirement", "GroupTypeId", "dbo.GroupType");
            DropIndex("dbo.GroupRequirement", "IDX_GroupRequirementTypeGroup");
            AlterColumn("dbo.GroupRequirement", "GroupId", c => c.Int(nullable: false));
            DropColumn("dbo.GroupRequirement", "MustMeetRequirementToAddMember");
            DropColumn("dbo.GroupRequirement", "GroupTypeId");
            CreateIndex("dbo.GroupRequirement", new[] { "GroupId", "GroupRequirementTypeId", "GroupRoleId" }, unique: true, name: "IDX_GroupRequirementTypeGroup");
        }
    }
}
