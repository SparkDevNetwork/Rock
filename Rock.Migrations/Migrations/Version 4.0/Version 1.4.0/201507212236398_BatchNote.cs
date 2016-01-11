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
    public partial class BatchNote : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add new Note column to Financial Batch
            AddColumn("dbo.FinancialBatch", "Note", c => c.String());
            
            // remove Alternate Placements columns (to be replaced with new "Place Elsewhere" feature)
            DropColumn("dbo.Group", "AcceptAlternatePlacements");
            DropColumn("dbo.GroupType", "EnableAlternatePlacements");

            // JE - Add/Update HtmlContent for Block: Intro
RockMigrationHelper.UpdateHtmlContentBlock("6DE44644-65FE-4321-A09D-36B329D6AE04",@"<h1>{{ CurrentPerson.NickName | Possessive}} Dashboard</h1>","F0BCB32C-CEB0-41CC-B43A-26FC66CCBD36"); 

RockMigrationHelper.AddBlockAttributeValue("415575C3-70AC-4A7A-8936-B98464C5557F","D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2",@"<div class='panel panel-block'> 
    <div class='panel-heading'>
        <h4 class='panel-title'>My Assigned Tasks</h4>
    </div>
    <div class='panel-body'>
        {% if Actions.size > 0 %}
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        {% else %}
            <div class='alert alert-info'>There are no open tasks assigned to you.</div>
        {% endif %}
    </div>
</div>");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.GroupType", "EnableAlternatePlacements", c => c.Boolean(nullable: false));
            AddColumn("dbo.Group", "AcceptAlternatePlacements", c => c.Boolean(nullable: false));
            DropColumn("dbo.FinancialBatch", "Note");
        }
    }
}
