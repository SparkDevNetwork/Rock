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
    public partial class MetricAndWorkflow : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowLog", "CreatedDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowLog", "ModifiedDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowLog", "CreatedByPersonAliasId", c => c.Int());
            AddColumn("dbo.WorkflowLog", "ModifiedByPersonAliasId", c => c.Int());
            AlterColumn("dbo.MetricValue", "XValue", c => c.String());
            CreateIndex("dbo.WorkflowLog", "CreatedByPersonAliasId");
            CreateIndex("dbo.WorkflowLog", "ModifiedByPersonAliasId");
            AddForeignKey("dbo.WorkflowLog", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.WorkflowLog", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowLog", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowLog", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.WorkflowLog", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowLog", new[] { "CreatedByPersonAliasId" });
            AlterColumn("dbo.MetricValue", "XValue", c => c.String(nullable: false));
            DropColumn("dbo.WorkflowLog", "ModifiedByPersonAliasId");
            DropColumn("dbo.WorkflowLog", "CreatedByPersonAliasId");
            DropColumn("dbo.WorkflowLog", "ModifiedDateTime");
            DropColumn("dbo.WorkflowLog", "CreatedDateTime");
        }
    }
}
