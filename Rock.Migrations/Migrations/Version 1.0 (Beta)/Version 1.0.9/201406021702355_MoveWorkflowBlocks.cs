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
    public partial class MoveWorkflowBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update block paths for all workflow blocks
            Sql( @"
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/ActivateWorkflow.ascx'       ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/ActivateWorkflow.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/MyWorkflowsLiquid.ascx' 		,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/MyWorkflows.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowDetail.ascx' 		,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowDetail.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowEntry.ascx'			,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowEntry.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowList.ascx'			,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowList.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowNavigation.ascx'     ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowNavigation.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowTriggerDetail.ascx'  ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowTriggerDetail.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowTriggerList.ascx'	,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowTriggerList.ascx'
    UPDATE [BlockType] SET [Path] = '~/Blocks/WorkFlow/WorkflowTypeDetail.ascx' 	,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/Core/WorkflowTypeDetail.ascx'

    -- Move Entry Page to correct parent
    DECLARE @ParentPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54')
    UPDATE [Page] SET [ParentPageId] = @ParentPageId WHERE [Guid] = '0550D2AA-A705-4400-81FF-AB124FDF83D7'

    -- Update pages to not display in breadcrumb
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] IN ( 'CDB27DB2-977C-415A-AED5-D0751DFD5DF2','61E1B4B6-EACE-42E8-A2FB-37465E6D0004', '0550D2AA-A705-4400-81FF-AB124FDF83D7' )
" );

            // Delete the existing Workflow Detail page
            RockMigrationHelper.DeletePage( "BA547EED-5537-49CF-BD4E-C583D760788C" );

            RockMigrationHelper.AddPage("61E1B4B6-EACE-42E8-A2FB-37465E6D0004","D65F783D-87A9-4CC9-8110-E83466A0EADB","Workflow Detail","","BA547EED-5537-49CF-BD4E-C583D760788C",""); // Site:Rock RMS
            RockMigrationHelper.AddPage("CDB27DB2-977C-415A-AED5-D0751DFD5DF2","195BCD57-1C10-4969-886F-7324B6287B75","My Workflows","","F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0",""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Activate Workflow","Activates a workflow and then redirects user to workflow entry page.","~/Blocks/WorkFlow/ActivateWorkflow.ascx","WorkFlow","83CB0C72-4F0A-44A7-98D0-260CE33788E9");
            RockMigrationHelper.UpdateBlockType("My Workflows","Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user.","~/Blocks/WorkFlow/MyWorkflows.ascx","WorkFlow","689B434F-DD2D-464A-8DA3-21F8768BB5BF");
            RockMigrationHelper.UpdateBlockType("My Workflows Liquid","Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a liquid template.","~/Blocks/WorkFlow/MyWorkflowsLiquid.ascx","WorkFlow","4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1");
            RockMigrationHelper.UpdateBlockType("Workflow Detail","Displays the details of a workflow instance.","~/Blocks/WorkFlow/WorkflowDetail.ascx","WorkFlow","4A9D62CE-5822-490F-B9EE-6D80037B4F5F");

            // Add Block to Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock("BA547EED-5537-49CF-BD4E-C583D760788C","","4A9D62CE-5822-490F-B9EE-6D80037B4F5F","Workflow Detail","Main","","",0,"D6627C0C-90D4-495B-9770-26D5822151BF"); 
            // Add Block to Page: My Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlock("F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0","","689B434F-DD2D-464A-8DA3-21F8768BB5BF","My Workflows","Main","","",0,"358A942A-54A8-4BFA-BEC8-ECBF05CA17E2"); 
            // Add Block to Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlock("20F97A93-7949-4C2A-8A5E-C756FE8585CA","","4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","My Workflows Liquid","Sidebar1","","",3,"2C90BDF8-48FF-4A7C-AA70-97B7E3780177"); 
            
            // Attrib for BlockType: Workflow Navigation:Categories
            RockMigrationHelper.AddBlockTypeAttribute("DDC6B004-9ED1-470F-ABF5-041250082168","775899FB-AC17-4C2C-B809-CF3A1D2AA4E1","Categories","Categories","","The categories to display",0,@"","FB420F14-3D9D-4304-878F-124902E2CEAB");
            // Attrib for BlockType: Workflow Navigation:Include Child Categories
            RockMigrationHelper.AddBlockTypeAttribute("DDC6B004-9ED1-470F-ABF5-041250082168","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Include Child Categories","IncludeChildCategories","","Should descendent categories of the selected Categories be included?",1,@"True","61F01133-C84E-4380-ADE3-42EF894A3E2A");
            // Attrib for BlockType: My Workflows:Entry Page
            RockMigrationHelper.AddBlockTypeAttribute("689B434F-DD2D-464A-8DA3-21F8768BB5BF","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Entry Page","EntryPage","","Page used to entery form information for a workflow.",0,@"","F061EC60-2D56-4D77-B6C9-210B9E34115B");
            // Attrib for BlockType: My Workflows Liquid:Contents
            RockMigrationHelper.AddBlockTypeAttribute("4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Contents","Contents","", @"
The Liquid template to use for displaying activities assigned to current user. 
The following object model is available to the liquid template (Note: and workflow or activity attributes will also be available 
as fields on the workflow or activity)...
<pre>
{
  ""Actions"": [
    {
      ""ActivityId"": 0,
      ""ActionTypeId"": 0,
      ""LastProcessedDateTime"": null,
      ""CompletedDateTime"": null,
      ""FormAction"": null,
      ""IsActive"": true,
      ""CreatedDateTime"": null,
      ""ModifiedDateTime"": null,
      ""CreatedByPersonAliasId"": null,
      ""ModifiedByPersonAliasId"": null,
      ""Id"": 0,
      ""Guid"": ""68673d26-fbeb-464c-882d-992c5bd60e19"",
      ""ForeignId"": null,
      ""UrlEncodedKey"": ""EAAAABul!2fMqcPFoFJEAauTEsj0aH1yCR7B3RI6v9wDzBsy4OPcDXF28gnUw90KVGKZCtX2dea449c9jYMO!2b71NLpdBk!3d"",
      ""Activity"": {
        ""WorkflowId"": 0,
        ""ActivityTypeId"": 0,
        ""AssignedPersonAliasId"": null,
        ""AssignedGroupId"": null,
        ""ActivatedDateTime"": null,
        ""LastProcessedDateTime"": null,
        ""CompletedDateTime"": null,
        ""IsActive"": false,
        ""Actions"": [],
        ""CreatedDateTime"": null,
        ""ModifiedDateTime"": null,
        ""CreatedByPersonAliasId"": null,
        ""ModifiedByPersonAliasId"": null,
        ""Id"": 0,
        ""Guid"": ""75bb018e-cdd4-467f-bfe8-dfeea4588d7b"",
        ""ForeignId"": null,
        ""UrlEncodedKey"": ""EAAAAPhlc7JODOrteeyeyY0j8P4gMZJVp2krvh2233xa0Nk1GZL4QvXIxSWGzugZVgafMjSgmua5xkaUugJzMMuHlcU!3d"",
        ""Workflow"": {
          ""WorkflowTypeId"": 0,
          ""Name"": null,
          ""Description"": null,
          ""Status"": null,
          ""IsProcessing"": false,
          ""ActivatedDateTime"": null,
          ""LastProcessedDateTime"": null,
          ""CompletedDateTime"": null,
          ""IsActive"": false,
          ""Activities"": [],
          ""IsPersisted"": false,
          ""CreatedDateTime"": null,
          ""ModifiedDateTime"": null,
          ""CreatedByPersonAliasId"": null,
          ""ModifiedByPersonAliasId"": null,
          ""Id"": 0,
          ""Guid"": ""bff03650-ac70-4172-9915-ec3dae54a84c"",
          ""ForeignId"": null,
          ""UrlEncodedKey"": ""EAAAAFe1QzuSp!2bDm5xz7DXcbsTZVbgtGLqL59qBA2bC9NQLBfyHm3NgsLjGJ93CGvA!2fXIec6B5CXAnS70oyVuLXPTwk!3d"",
          ""WorkflowType"": {
            ""IsSystem"": false,
            ""IsActive"": null,
            ""Name"": null,
            ""Description"": null,
            ""CategoryId"": null,
            ""Order"": 0,
            ""WorkTerm"": null,
            ""ProcessingIntervalSeconds"": null,
            ""IsPersisted"": false,
            ""LoggingLevel"": 0,
            ""IconCssClass"": null,
            ""Category"": null,
            ""ActivityTypes"": [],
            ""CreatedDateTime"": null,
            ""ModifiedDateTime"": null,
            ""CreatedByPersonAliasId"": null,
            ""ModifiedByPersonAliasId"": null,
            ""Id"": 0,
            ""Guid"": ""c5436b8c-3318-42ce-955e-8c92941e31e7"",
            ""ForeignId"": null,
            ""UrlEncodedKey"": ""EAAAAGSb5DpmqLSq!2bDDN0k1JVmEfc6UTBMebv9YISx0q4PzxZ33Mu8!2boX!2bGWkGPq8z3KBBOLEhLGY8vvnw7sN3kLFM8!3d""
          }
        },
        ""ActivityType"": {
          ""IsActive"": null,
          ""WorkflowTypeId"": 0,
          ""Name"": null,
          ""Description"": null,
          ""IsActivatedWithWorkflow"": false,
          ""Order"": 0,
          ""ActionTypes"": [],
          ""CreatedDateTime"": null,
          ""ModifiedDateTime"": null,
          ""CreatedByPersonAliasId"": null,
          ""ModifiedByPersonAliasId"": null,
          ""Id"": 0,
          ""Guid"": ""dba263b3-1d8c-492b-8e3b-f22b0ec8de74"",
          ""ForeignId"": null,
          ""UrlEncodedKey"": ""EAAAAAdvo6mI5UkO9Nl2hFwQORghGVY431blosTie11MmydvePVfZty5vE!2fPQddx40e3bAP50K6oqpPhlxkfeerZ278!3d""
        }
      },
      ""ActionType"": {
        ""ActivityTypeId"": 0,
        ""Name"": null,
        ""Order"": 0,
        ""EntityTypeId"": 0,
        ""IsActionCompletedOnSuccess"": false,
        ""IsActivityCompletedOnSuccess"": false,
        ""WorkflowFormId"": null,
        ""CriteriaAttributeGuid"": null,
        ""CriteriaComparisonType"": 0,
        ""CriteriaValue"": null,
        ""EntityType"": null,
        ""WorkflowForm"": null,
        ""CreatedDateTime"": null,
        ""ModifiedDateTime"": null,
        ""CreatedByPersonAliasId"": null,
        ""ModifiedByPersonAliasId"": null,
        ""Id"": 0,
        ""Guid"": ""b2da5aea-0a29-468b-b793-629712a80d5d"",
        ""ForeignId"": null,
        ""UrlEncodedKey"": ""EAAAANd2YTTVJlcnjRwqg0FLMcyUD9NZ8CF6T!2bAkdZqMiQv!2fc6GU8A2vbLPR187daK8B!2bgBZeKPEz4UUvV9K1Xyey70!3d""
      }
    }
  ]
}
</pre>
",2,@"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My Tasks</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }} ({{ action.Activity.ActivityType.Name }})</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
","D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2");
            // Attrib for BlockType: My Workflows Liquid:Include Child Categories
            RockMigrationHelper.AddBlockTypeAttribute("4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Include Child Categories","IncludeChildCategories","","Should descendent categories of the selected Categories be included?",1,@"True","5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC");
            // Attrib for BlockType: My Workflows Liquid:Categories
            RockMigrationHelper.AddBlockTypeAttribute("4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","775899FB-AC17-4C2C-B809-CF3A1D2AA4E1","Categories","Categories","","Optional categories to limit display to.",0,@"","28DF6F17-AF47-49DF-824F-9E7C8B94AD5D");
            // Attrib Value for Block:Workflow Navigation, Attribute:Categories Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2D20CEC4-328E-4C2B-8059-78DFC49D8E35","FB420F14-3D9D-4304-878F-124902E2CEAB",@"");
            // Attrib Value for Block:Workflow Navigation, Attribute:Include Child Categories Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2D20CEC4-328E-4C2B-8059-78DFC49D8E35","61F01133-C84E-4380-ADE3-42EF894A3E2A",@"True");
            // Attrib Value for Block:My Workflows, Attribute:Entry Page Page: My Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("358A942A-54A8-4BFA-BEC8-ECBF05CA17E2","F061EC60-2D56-4D77-B6C9-210B9E34115B",@"0550d2aa-a705-4400-81ff-ab124fdf83d7");
            // Attrib Value for Block:My Workflows Liquid, Attribute:Contents Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2C90BDF8-48FF-4A7C-AA70-97B7E3780177","D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2",@"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My Tasks</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }} ({{ action.Activity.ActivityType.Name }})</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
");
            // Attrib Value for Block:My Workflows Liquid, Attribute:Include Child Categories Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2C90BDF8-48FF-4A7C-AA70-97B7E3780177","5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC",@"True");
            // Attrib Value for Block:My Workflows Liquid, Attribute:Categories Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2C90BDF8-48FF-4A7C-AA70-97B7E3780177","28DF6F17-AF47-49DF-824F-9E7C8B94AD5D",@"");

            RockMigrationHelper.UpdateFieldType("Group Type Group","","Rock","Rock.Field.Types.GroupTypeGroupFieldType","CC34CE2C-0B0E-4BB3-9549-454B2A7DF218");
            RockMigrationHelper.UpdateFieldType("Security Role","","Rock","Rock.Field.Types.SecurityRoleFieldType","7BD25DC9-F34A-478D-BEF9-0C787F5D39B8");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: My Workflows Liquid:Categories
            RockMigrationHelper.DeleteAttribute( "28DF6F17-AF47-49DF-824F-9E7C8B94AD5D" );
            // Attrib for BlockType: My Workflows Liquid:Include Child Categories
            RockMigrationHelper.DeleteAttribute( "5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC" );
            // Attrib for BlockType: My Workflows Liquid:Contents
            RockMigrationHelper.DeleteAttribute( "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2" );
            // Attrib for BlockType: My Workflows:Entry Page
            RockMigrationHelper.DeleteAttribute( "F061EC60-2D56-4D77-B6C9-210B9E34115B" );
            // Attrib for BlockType: Workflow Navigation:Include Child Categories
            RockMigrationHelper.DeleteAttribute( "61F01133-C84E-4380-ADE3-42EF894A3E2A" );
            // Attrib for BlockType: Workflow Navigation:Categories
            RockMigrationHelper.DeleteAttribute( "FB420F14-3D9D-4304-878F-124902E2CEAB" );
            // Remove Block: My Workflows Liquid, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2C90BDF8-48FF-4A7C-AA70-97B7E3780177" );
            // Remove Block: My Workflows, from Page: My Workflows, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "358A942A-54A8-4BFA-BEC8-ECBF05CA17E2" );
            // Remove Block: Workflow Detail, from Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D6627C0C-90D4-495B-9770-26D5822151BF" );
            RockMigrationHelper.DeleteBlockType( "4A9D62CE-5822-490F-B9EE-6D80037B4F5F" ); // Workflow Detail
            RockMigrationHelper.DeleteBlockType( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1" ); // My Workflows Liquid
            RockMigrationHelper.DeleteBlockType( "689B434F-DD2D-464A-8DA3-21F8768BB5BF" ); // My Workflows
            RockMigrationHelper.DeleteBlockType( "83CB0C72-4F0A-44A7-98D0-260CE33788E9" ); // Activate Workflow
            RockMigrationHelper.DeletePage( "F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0" ); // Page: My WorkflowsLayout: Full Width Panel, Site: Rock RMS
            RockMigrationHelper.DeletePage( "BA547EED-5537-49CF-BD4E-C583D760788C" ); // Page: Workflow DetailLayout: Full Width, Site: Rock RMS

            Sql( @"
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/ActivateWorkflow.ascx'       ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/ActivateWorkflow.ascx'       
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/MyWorkflows.ascx'            ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/MyWorkflowsLiquid.ascx' 		
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowDetail.ascx'         ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowDetail.ascx' 		
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowEntry.ascx'          ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowEntry.ascx'			
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowList.ascx'           ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowList.ascx'			
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowNavigation.ascx'     ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowNavigation.ascx'     
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowTriggerDetail.ascx'  ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowTriggerDetail.ascx'  
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowTriggerList.ascx'    ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowTriggerList.ascx'	
    UPDATE [BlockType] SET [Path] = '~/Blocks/Core/WorkflowTypeDetail.ascx'     ,[Category] = 'WorkFlow' WHERE [Path] = '~/Blocks/WorkFlow/WorkflowTypeDetail.ascx' 	
" );
        }
    }
}
