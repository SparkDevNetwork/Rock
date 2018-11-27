// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Care.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class AddNotificationWorkflow : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Care", "fa fa-leaf", "", "DD74EC43-13B3-48A4-8437-BD8B355BD81D" );
            RockMigrationHelper.UpdatePersonAttribute( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "DD74EC43-13B3-48A4-8437-BD8B355BD81D", "Do you want to receive PRT Notifications?", "ReceivePrtNotifications", "", "", 1002, "True", "ADE54D1F-383F-4682-8BE8-2D17D2C2D765" );
            var attributeId = SqlScalar( "Select Top 1 Id From Attribute Where [Guid] = 'ADE54D1F-383F-4682-8BE8-2D17D2C2D765' " ).ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append( @"[{""Guid"":""00000000-0000-0000-0000-000000000000"",""Name"":"""",""Header"":""<div class=\""panel panel - block\"" >\n <div class=\""panel-heading\"">\n<h1 class=\""panel-title\""><i class=\""fa fa-leaf\""></i> PRT Notification Settings</h1>\n</div>\n<div class=\""panel-body\"">\n        \n"",""Footer"":""    </div>\n</div>"",""Order"":0,""Expanded"":true,""Fields"":[{""Guid"":""7b2e88fd-b3fb-45e1-981a-d85760489780"",""AttributeId"":" );
            sb.Append( attributeId );
            sb.Append( @",""ShowCurrentValue"":true,""IsRequired"":false,""Order"":0,""PreText"":"""",""PostText"":""""}]}]" );

            // Add Block to Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "18B8EB25-B9F2-48C6-B047-51A512A8F1C9", "", "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "Person Attribute Forms", "Main", "", "", 1, "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9" );
            // Attrib Value for Block:Person Attribute Forms, Attribute:Save Values Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9", "37D4E976-8228-41EA-9D89-2296375FAA2E", @"END" );
            // Attrib Value for Block:Person Attribute Forms, Attribute:Display Progress Bar Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9", "B14B6FF2-B981-4BE3-8B47-180D3A6BAFB5", @"True" );
            // Attrib Value for Block:Person Attribute Forms, Attribute:Forms Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9", "598BAB2B-AAEE-4DDF-ACAA-2B686285976A", sb.ToString() );
            // Attrib Value for Block:Person Attribute Forms, Attribute:Done Page Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9", "B892FD34-2AE5-42A0-88F0-2E2B32EDED2C", @"18b8eb25-b9f2-48c6-b047-51a512a8f1c9" );

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "65E69B78-37D8-4A88-B8AC-71893D2F75EF" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "FEEBEC86-3AEA-456A-8AC3-F67E76C2B1A3" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "12465E6A-1261-417E-A0A2-8F5D179F3D1C" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "07CB7DBC-236D-4D38-92A4-47EE448BA89A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "PRT Request Workflows", "fa fa-leaf", "", "18FB5E17-8D4D-4048-ACDE-8BE85854CB2D", 0 ); // PRT Request Workflows

            #endregion

            #region Notify Worker of New PRT Request Assignment

            RockMigrationHelper.UpdateWorkflowType( false, true, "Notify Worker of New PRT Request Assignment", "", "18FB5E17-8D4D-4048-ACDE-8BE85854CB2D", "Work", "fa fa-list-ol", 28800, true, 0, "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", 0 ); // Notify Worker of New PRT Request Assignment
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "", 0, @"", "C46722A7-F40A-4DEB-9A01-CC3151076745", false ); // Notify Worker of New PRT Request Assignment:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requestor", "Requestor", "", 1, @"", "F0283C61-E871-44AC-8FDF-87E1CA1ACA28", false ); // Notify Worker of New PRT Request Assignment:Requestor
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Can Receive Notifications", "CanReceiveNotifications", "", 2, @"", "5146E75A-1116-4261-B356-B1900F5E603F", false ); // Notify Worker of New PRT Request Assignment:Can Receive Notifications
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "ConnectionRequestId", "ConnectionRequestId", "", 3, @"", "6AD0FE91-1446-4262-B3B5-24B88FFAA719", false ); // Notify Worker of New PRT Request Assignment:ConnectionRequestId
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "ConnectionOpportunityId", "ConnectionOpportunityId", "", 4, @"", "67C11B16-56FD-42C9-957C-ABCE32B5A9D1", false ); // Notify Worker of New PRT Request Assignment:ConnectionOpportunityId
            RockMigrationHelper.AddAttributeQualifier( "C46722A7-F40A-4DEB-9A01-CC3151076745", "EnableSelfSelection", @"False", "33CC67B0-D3FA-408C-B60A-6F7E076273F3" ); // Notify Worker of New PRT Request Assignment:Worker:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "F0283C61-E871-44AC-8FDF-87E1CA1ACA28", "EnableSelfSelection", @"False", "6034AAF0-FC85-40D0-8160-E7683014EF45" ); // Notify Worker of New PRT Request Assignment:Requestor:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "5146E75A-1116-4261-B356-B1900F5E603F", "falsetext", @"No", "0C227F7E-DB05-446E-A5CA-E5E5AEF2B171" ); // Notify Worker of New PRT Request Assignment:Can Receive Notifications:falsetext
            RockMigrationHelper.AddAttributeQualifier( "5146E75A-1116-4261-B356-B1900F5E603F", "truetext", @"Yes", "19F94016-B7D3-422E-A32B-B8C6D7FFD6FE" ); // Notify Worker of New PRT Request Assignment:Can Receive Notifications:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "2B3E7C53-F94D-4CA1-BB96-E30BD5D27D21", true, "Start", "", true, 0, "D8E02147-F2E6-427B-A30D-B9291616E72C" ); // Notify Worker of New PRT Request Assignment:Start
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Set Worker", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "D9CF6376-F3EB-45EA-B5FF-34C2724B650E" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Set Requestor", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "06165CAB-B8DF-4D5E-8F01-5E38365E9C05" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Set Can Receive Notifications", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "A489FE30-C4F4-4BBC-BECA-FF71B200D8A9" ); // Notify Worker of New PRT Request Assignment:Start:Set Can Receive Notifications
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Set ConnectionRequestId", 3, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Set ConnectionOpportunityId", 4, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Send Notifications", 5, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "5146E75A-1116-4261-B356-B1900F5E603F", 1, "Yes", "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications
            RockMigrationHelper.UpdateWorkflowActionType( "D8E02147-F2E6-427B-A30D-B9291616E72C", "Complete Workflow", 6, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "FEABC64D-4294-4D2D-8AB0-F19AAFF62802" ); // Notify Worker of New PRT Request Assignment:Start:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"c46722a7-f40a-4deb-9a01-cc3151076745" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "12465E6A-1261-417E-A0A2-8F5D179F3D1C", @"True" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "D9CF6376-F3EB-45EA-B5FF-34C2724B650E", "FEEBEC86-3AEA-456A-8AC3-F67E76C2B1A3", @"{{ Entity.ConnectorPersonAlias.Guid }}" ); // Notify Worker of New PRT Request Assignment:Start:Set Worker:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"f0283c61-e871-44ac-8fdf-87e1ca1aca28" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "12465E6A-1261-417E-A0A2-8F5D179F3D1C", @"True" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "06165CAB-B8DF-4D5E-8F01-5E38365E9C05", "FEEBEC86-3AEA-456A-8AC3-F67E76C2B1A3", @"{{ Entity.PersonAlias.Guid }}" ); // Notify Worker of New PRT Request Assignment:Start:Set Requestor:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "A489FE30-C4F4-4BBC-BECA-FF71B200D8A9", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set Can Receive Notifications:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A489FE30-C4F4-4BBC-BECA-FF71B200D8A9", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"5146e75a-1116-4261-b356-b1900f5e603f" ); // Notify Worker of New PRT Request Assignment:Start:Set Can Receive Notifications:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A489FE30-C4F4-4BBC-BECA-FF71B200D8A9", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Notify Worker of New PRT Request Assignment:Start:Set Can Receive Notifications:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A489FE30-C4F4-4BBC-BECA-FF71B200D8A9", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{%assign worker = Workflow | Attribute:'Worker','Object'%}{{worker | Attribute:'ReceivePrtNotifications'}}" ); // Notify Worker of New PRT Request Assignment:Start:Set Can Receive Notifications:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"6ad0fe91-1446-4262-b3b5-24b88ffaa719" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "12465E6A-1261-417E-A0A2-8F5D179F3D1C", @"True" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "3E2933B4-5657-4A8C-BA91-5DDAA9111E8C", "FEEBEC86-3AEA-456A-8AC3-F67E76C2B1A3", @"{{ Entity.Id}}" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionRequestId:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"67c11b16-56fd-42c9-957c-abce32b5a9d1" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "12465E6A-1261-417E-A0A2-8F5D179F3D1C", @"True" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "52DA2AD9-DE27-4B0C-8E81-83881C3CDFD3", "FEEBEC86-3AEA-456A-8AC3-F67E76C2B1A3", @"{{ Entity.ConnectionOpportunityId }}" ); // Notify Worker of New PRT Request Assignment:Start:Set ConnectionOpportunityId:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "0C4C13B8-7076-4872-925A-F950886B5E16", @"c46722a7-f40a-4deb-9a01-cc3151076745" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F", @"" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Send to Group Role
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"New PRT Request" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign worker =  Workflow | Attribute:'Worker','Object' %}
{% capture url %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}page/536?ConnectionRequestId={{ Workflow | Attribute:'ConnectionRequestId' }}&ConnectionOpportunityId={{ Workflow | Attribute:'ConnectionOpportunityId' }}{% endcapture %}
<p>{{ worker.FirstName }},</p>
<p>A new PRT Request has been assigned to you:<p>
<p>
    <a href='{{url}}'>{{ Workflow | Attribute:'Requestor' }}</a>
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "3F139C6A-A7D3-4230-B8D7-6B81FB0D81B0", "65E69B78-37D8-4A88-B8AC-71893D2F75EF", @"True" ); // Notify Worker of New PRT Request Assignment:Start:Send Notifications:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "FEABC64D-4294-4D2D-8AB0-F19AAFF62802", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Notify Worker of New PRT Request Assignment:Start:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FEABC64D-4294-4D2D-8AB0-F19AAFF62802", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Notify Worker of New PRT Request Assignment:Start:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "FEABC64D-4294-4D2D-8AB0-F19AAFF62802", "07CB7DBC-236D-4D38-92A4-47EE448BA89A", @"Completed" ); // Notify Worker of New PRT Request Assignment:Start:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "54BB9E3A-2FEC-4BAF-9181-EC217E0161E9" );
        }
    }
}
