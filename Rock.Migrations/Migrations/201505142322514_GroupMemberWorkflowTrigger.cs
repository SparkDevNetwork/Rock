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
    public partial class GroupMemberWorkflowTrigger : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupMemberWorkflowTrigger",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(nullable: false),
                        GroupTypeId = c.Int(),
                        GroupId = c.Int(),
                        Name = c.String(maxLength: 100),
                        WorkflowTypeId = c.Int(nullable: false),
                        TriggerType = c.Int(nullable: false),
                        TypeQualifier = c.String(maxLength: 200),
                        WorkflowName = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.WorkflowTypeId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "WorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "GroupId", "dbo.Group");
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "ForeignId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "Guid" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "WorkflowTypeId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "GroupId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "GroupTypeId" });
            DropTable("dbo.GroupMemberWorkflowTrigger");
        }
    }
}
