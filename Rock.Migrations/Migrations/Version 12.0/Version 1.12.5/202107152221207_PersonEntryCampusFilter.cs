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
    public partial class PersonEntryCampusFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowActionForm", "PersonEntryCampusStatusValueId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryCampusTypeValueId", c => c.Int());
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryCampusStatusValueId");
            CreateIndex("dbo.WorkflowActionForm", "PersonEntryCampusTypeValueId");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryCampusStatusValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.WorkflowActionForm", "PersonEntryCampusTypeValueId", "dbo.DefinedValue", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryCampusTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.WorkflowActionForm", "PersonEntryCampusStatusValueId", "dbo.DefinedValue");
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryCampusTypeValueId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "PersonEntryCampusStatusValueId" });
            DropColumn("dbo.WorkflowActionForm", "PersonEntryCampusTypeValueId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryCampusStatusValueId");
        }
    }
}
