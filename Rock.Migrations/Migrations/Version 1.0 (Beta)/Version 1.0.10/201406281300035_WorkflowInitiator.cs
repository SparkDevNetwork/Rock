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
    public partial class WorkflowInitiator : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Workflow", "InitiatorPersonAliasId", c => c.Int());
            CreateIndex("dbo.Workflow", "InitiatorPersonAliasId");
            AddForeignKey("dbo.Workflow", "InitiatorPersonAliasId", "dbo.PersonAlias", "Id");

            // Attrib for BlockType: My Workflows:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "689B434F-DD2D-464A-8DA3-21F8768BB5BF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view status of a workflow.", 0, @"", "246DFB54-78CF-4A51-A68F-B52EBE3C7C74" );

            // Attrib Value for Block:My Workflows, Attribute:Detail Page Page: My Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "358A942A-54A8-4BFA-BEC8-ECBF05CA17E2", "246DFB54-78CF-4A51-A68F-B52EBE3C7C74", @"ba547eed-5537-49cf-bd4e-c583d760788c" );

            RockMigrationHelper.AddPageRoute( "BA547EED-5537-49CF-BD4E-C583D760788C", "Workflow/{WorkflowId}" );

            Sql( @"
    DECLARE @LayoutId int = ( SELECT [Id] FROM [Layout] WHERE [Guid] = '195BCD57-1C10-4969-886F-7324B6287B75' )
    UPDATE [Page] SET [LayoutId] = @LayoutId WHERE [Guid] = 'BA547EED-5537-49CF-BD4E-C583D760788C'
" );
            
            // Add Block to Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlock( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1", "My Workflows Liquid", "Sidebar1", "", "", 4, "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341" );
            // Attrib for BlockType: My Workflows Liquid:Role
            RockMigrationHelper.AddBlockTypeAttribute( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Role", "Role", "", "Display the active workflows that the current user Initiated, or is currently Assigned To.", 0, @"0", "C8816900-0772-4E15-8D41-D20874F560BE" );

            // Attrib Value for Block:My Workflows Liquid, Attribute:Contents Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2C90BDF8-48FF-4A7C-AA70-97B7E3780177", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2", @"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My {% if Role == '0' %}Tasks{% else %}Requests{% endif %}</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if Role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
" );
            // Attrib Value for Block:My Workflows Liquid, Attribute:Role Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2C90BDF8-48FF-4A7C-AA70-97B7E3780177", "C8816900-0772-4E15-8D41-D20874F560BE", @"0" );

            // Attrib Value for Block:My Workflows Liquid, Attribute:Contents Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2", @"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My {% if Role == '0' %}Tasks{% else %}Requests{% endif %}</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if Role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
" );
            // Attrib Value for Block:My Workflows Liquid, Attribute:Include Child Categories Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341", "5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC", @"True" );
            // Attrib Value for Block:My Workflows Liquid, Attribute:Categories Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341", "28DF6F17-AF47-49DF-824F-9E7C8B94AD5D", @"" );
            // Attrib Value for Block:My Workflows Liquid, Attribute:Role Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341", "C8816900-0772-4E15-8D41-D20874F560BE", @"1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: My Workflows Liquid:Role
            RockMigrationHelper.DeleteAttribute( "C8816900-0772-4E15-8D41-D20874F560BE" );

            // Remove Block: My Workflows Liquid, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4AA6DB52-D44E-43FB-8A6F-43BEC93AA341" );
            
            // Attrib for BlockType: My Workflows:Detail Page
            RockMigrationHelper.DeleteAttribute( "246DFB54-78CF-4A51-A68F-B52EBE3C7C74" );

            DropForeignKey("dbo.Workflow", "InitiatorPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Workflow", new[] { "InitiatorPersonAliasId" });
            DropColumn("dbo.Workflow", "InitiatorPersonAliasId");
        }
    }
}
