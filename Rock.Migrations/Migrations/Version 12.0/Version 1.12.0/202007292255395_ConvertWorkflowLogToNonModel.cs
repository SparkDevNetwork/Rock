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
    public partial class ConvertWorkflowLogToNonModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.WorkflowLog", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowLog", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.WorkflowLog", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowLog", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowLog", new[] { "Guid" });
            DropColumn("dbo.WorkflowLog", "CreatedDateTime");
            DropColumn("dbo.WorkflowLog", "ModifiedDateTime");
            DropColumn("dbo.WorkflowLog", "CreatedByPersonAliasId");
            DropColumn("dbo.WorkflowLog", "ModifiedByPersonAliasId");
            DropColumn("dbo.WorkflowLog", "Guid");
            DropColumn("dbo.WorkflowLog", "ForeignId");
            DropColumn("dbo.WorkflowLog", "ForeignGuid");
            DropColumn("dbo.WorkflowLog", "ForeignKey");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.WorkflowLog", "ForeignKey", c => c.String(maxLength: 100));
            AddColumn("dbo.WorkflowLog", "ForeignGuid", c => c.Guid());
            AddColumn("dbo.WorkflowLog", "ForeignId", c => c.Int());
            AddColumn("dbo.WorkflowLog", "Guid", c => c.Guid(nullable: false));
            AddColumn("dbo.WorkflowLog", "ModifiedByPersonAliasId", c => c.Int());
            AddColumn("dbo.WorkflowLog", "CreatedByPersonAliasId", c => c.Int());
            AddColumn("dbo.WorkflowLog", "ModifiedDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowLog", "CreatedDateTime", c => c.DateTime());
            CreateIndex("dbo.WorkflowLog", "Guid", unique: true);
            CreateIndex("dbo.WorkflowLog", "ModifiedByPersonAliasId");
            CreateIndex("dbo.WorkflowLog", "CreatedByPersonAliasId");
            AddForeignKey("dbo.WorkflowLog", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.WorkflowLog", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
        }
    }
}
