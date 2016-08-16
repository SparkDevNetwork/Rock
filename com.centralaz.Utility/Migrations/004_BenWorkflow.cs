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

namespace com.centralaz.Utility.Migrations
{
    [MigrationNumber( 4, "1.3.4" )]
    public class BenWorkflow : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.UpdateFieldType( "Address", "", "Rock", "Rock.Field.Types.AddressFieldType", "CD8701C1-DD09-4EEE-A4BD-EB33D707B6A6" );
            RockMigrationHelper.UpdateFieldType( "Content Channel Types", "", "Rock", "Rock.Field.Types.ContentChannelTypesFieldType", "E030DDA6-B794-4AC0-B261-AA4B61670DCC" );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToPerson", "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersistWorkflow", "F1A39347-6FE0-43D4-89FB-544195088ECF", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetWorkflowName", "36005473-BD5D-470B-B28D-98E6D7ED808D", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0A800013-51F7-4902-885A-5BE215D67D3D" ); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5D95C15A-CCAE-40AD-A9DD-F929DA587115" ); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 4, @"False", "62D0E0B1-47CB-437F-95DA-B40B8127CAE8" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 2, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" ); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Persist Immediately", "PersistImmediately", "This action will normally cause the workflow to be persisted (saved) once all the current activites/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.", 0, @"False", "82744A46-0110-4728-BD3D-66C85C5FCB2F" ); // Rock.Workflow.Action.PersistWorkflow:Persist Immediately
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" ); // Rock.Workflow.Action.PersistWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0B768E17-C64A-4212-BAD5-8A16B9F05A5C" ); // Rock.Workflow.Action.AssignActivityToPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5C5F7DB4-51DE-4293-BD73-CABDEB6564AC" ); // Rock.Workflow.Action.AssignActivityToPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person to assign this activity to.", 0, @"", "7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8" ); // Rock.Workflow.Action.AssignActivityToPerson:Person
            RockMigrationHelper.UpdateWorkflowType( false, true, "Campus Observations", "", "11BBA6D4-F8A1-46BA-AEF4-D4392048CA5F", "Campus Observations", "fa fa-list-ol", 28800, false, 0, "34DC4749-682B-4992-A09A-CF49AB4F1FC0" ); // Campus Observations
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Observer", "Observer", "Observer", 1, @"", "D8F07238-C43F-45E3-8A85-3E8944E2BB92" ); // Campus Observations:Observer
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Observation Date", "ObservationDate", "Observation Date", 2, @"", "BC8C13C1-5E89-45AA-9AB1-EE7426B4A9AE" ); // Campus Observations:Observation Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "What did you observe that made you proud of central?", "WhatDidYou", "What did you observe that made you proud of central?", 3, @"", "8C38B6A4-D738-4987-B451-ACF5E2783973" ); // Campus Observations:What did you observe that made you proud of central?
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "What did you observe that you found odd or out of the ordinary?", "Odd", "What did you observe that you found odd or out of the ordinary?", 4, @"", "E9F6F9FA-904E-4474-9939-333D9C093A62" ); // Campus Observations:What did you observe that you found odd or out of the ordinary?
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campus Observed", "CampusObserved", "Campus Observed", 0, @"", "8FEB83A7-B151-4FE1-BD96-30EB2327E6DB" ); // Campus Observations:Campus Observed
            RockMigrationHelper.AddAttributeQualifier( "D8F07238-C43F-45E3-8A85-3E8944E2BB92", "ispassword", @"False", "C951D80C-A7A2-403B-B149-87EBE7B956BA" ); // Campus Observations:Observer:ispassword
            RockMigrationHelper.AddAttributeQualifier( "BC8C13C1-5E89-45AA-9AB1-EE7426B4A9AE", "displayCurrentOption", @"False", "E159F2C5-5546-4F72-8CC7-BEE9C293333E" ); // Campus Observations:Observation Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "BC8C13C1-5E89-45AA-9AB1-EE7426B4A9AE", "displayDiff", @"False", "21EC9ACB-B81F-4FCB-B71B-BDA2C5718713" ); // Campus Observations:Observation Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "BC8C13C1-5E89-45AA-9AB1-EE7426B4A9AE", "format", @"MMMM dd, yyyy", "864FD891-7830-4664-B280-B39D1CBC9F9E" ); // Campus Observations:Observation Date:format
            RockMigrationHelper.AddAttributeQualifier( "8C38B6A4-D738-4987-B451-ACF5E2783973", "ispassword", @"False", "C9C02339-453C-4B04-ADB9-C01FDBB3A425" ); // Campus Observations:What did you observe that made you proud of central?:ispassword
            RockMigrationHelper.AddAttributeQualifier( "E9F6F9FA-904E-4474-9939-333D9C093A62", "ispassword", @"False", "C116A9BD-DCB6-479B-B111-4B0A95C720B8" ); // Campus Observations:What did you observe that you found odd or out of the ordinary?:ispassword
            RockMigrationHelper.AddAttributeQualifier( "8FEB83A7-B151-4FE1-BD96-30EB2327E6DB", "includeInactive", @"False", "535A2541-E0D6-4FF6-BF73-220AC6FFA57A" ); // Campus Observations:Campus Observed:includeInactive
            RockMigrationHelper.UpdateWorkflowActivityType( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", true, "Request", "Prompt user to fill out the form with their campus specific observations", true, 0, "94862999-4E59-477C-86DF-A48B4293B8B0" ); // Campus Observations:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", true, "Notify", "Notify Jeff Thomas with the campus specific observations", false, 1, "6488C854-2DB6-4E3F-8DCA-3E06326827FA" ); // Campus Observations:Notify
            RockMigrationHelper.UpdateWorkflowActivityType( "34DC4749-682B-4992-A09A-CF49AB4F1FC0", true, "Complete Workflow", "When worker is notified, workflow is completed", false, 2, "B7A95BEB-161B-4937-BAB7-9A21D40AF8C1" ); // Campus Observations:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|", "", false, "", "2A8550E4-7DA4-462E-916A-3EC0B73AD270" ); // Campus Observations:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "D8F07238-C43F-45E3-8A85-3E8944E2BB92", 1, true, false, true, "A2FD8CAD-6066-4A43-A95E-00A1F25297ED" ); // Campus Observations:Request:Prompt User:Observer
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "BC8C13C1-5E89-45AA-9AB1-EE7426B4A9AE", 2, true, false, true, "7387C885-CB14-4984-85EC-957CAD83020F" ); // Campus Observations:Request:Prompt User:Observation Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "8C38B6A4-D738-4987-B451-ACF5E2783973", 3, true, false, true, "11EB1FA1-7C1B-4FFC-A158-88951ACE82D2" ); // Campus Observations:Request:Prompt User:What did you observe that made you proud of central?
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "E9F6F9FA-904E-4474-9939-333D9C093A62", 4, true, false, true, "8678319C-3FCE-4D44-AEF4-23CBC5BEE8E6" ); // Campus Observations:Request:Prompt User:What did you observe that you found odd or out of the ordinary?
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "8FEB83A7-B151-4FE1-BD96-30EB2327E6DB", 0, true, false, true, "FD39CC08-A90C-4739-BA7E-528E2004F98B" ); // Campus Observations:Request:Prompt User:Campus Observed
            RockMigrationHelper.UpdateWorkflowActionType( "94862999-4E59-477C-86DF-A48B4293B8B0", "Set Worker", 2, "FB2981B7-7922-42E1-8ACF-7F63BB7989E6", true, false, "", "", 1, "", "F1F24F62-54D8-43B4-BE9F-DAFA2D269021" ); // Campus Observations:Request:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "94862999-4E59-477C-86DF-A48B4293B8B0", "Prompt User", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "2A8550E4-7DA4-462E-916A-3EC0B73AD270", "", 1, "", "EC8A767A-75C9-48BF-AC80-809A6D6A6F93" ); // Campus Observations:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType( "94862999-4E59-477C-86DF-A48B4293B8B0", "Persist the Workflow", 3, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, true, "", "", 1, "", "55ED9A70-95A2-4C77-8E83-5B5A8C82E422" ); // Campus Observations:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "94862999-4E59-477C-86DF-A48B4293B8B0", "Set Name", 1, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "AAF6730F-F48E-4D14-B2A9-A8249698C95D" ); // Campus Observations:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "6488C854-2DB6-4E3F-8DCA-3E06326827FA", "Notify Worker", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC" ); // Campus Observations:Notify:Notify Worker
            RockMigrationHelper.UpdateWorkflowActionType( "B7A95BEB-161B-4937-BAB7-9A21D40AF8C1", "Workflow Completed", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "359E8C35-E4B6-4083-84E9-7596D0F663D9" ); // Campus Observations:Complete Workflow:Workflow Completed
            RockMigrationHelper.AddActionTypeAttributeValue( "EC8A767A-75C9-48BF-AC80-809A6D6A6F93", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Campus Observations:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EC8A767A-75C9-48BF-AC80-809A6D6A6F93", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Campus Observations:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AAF6730F-F48E-4D14-B2A9-A8249698C95D", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Campus Observations:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AAF6730F-F48E-4D14-B2A9-A8249698C95D", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Campus Observations:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AAF6730F-F48E-4D14-B2A9-A8249698C95D", "93852244-A667-4749-961A-D47F88675BE4", @"Campus Observations" ); // Campus Observations:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F1F24F62-54D8-43B4-BE9F-DAFA2D269021", "0B768E17-C64A-4212-BAD5-8A16B9F05A5C", @"False" ); // Campus Observations:Request:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F1F24F62-54D8-43B4-BE9F-DAFA2D269021", "5C5F7DB4-51DE-4293-BD73-CABDEB6564AC", @"" ); // Campus Observations:Request:Set Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "F1F24F62-54D8-43B4-BE9F-DAFA2D269021", "7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8", @"277ba09c-edad-4d7a-a19d-ff756199e725" ); // Campus Observations:Request:Set Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "55ED9A70-95A2-4C77-8E83-5B5A8C82E422", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Campus Observations:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "55ED9A70-95A2-4C77-8E83-5B5A8C82E422", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Campus Observations:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "55ED9A70-95A2-4C77-8E83-5B5A8C82E422", "82744A46-0110-4728-BD3D-66C85C5FCB2F", @"False" ); // Campus Observations:Request:Persist the Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Campus Observations:Notify:Notify Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Campus Observations:Notify:Notify Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"donotreply@centralaz.com" ); // Campus Observations:Notify:Notify Worker:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "0C4C13B8-7076-4872-925A-F950886B5E16", @"jeff.thomas@centralaz.com" ); // Campus Observations:Notify:Notify Worker:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{ Workflow.Campus }} Campus Observations" ); // Campus Observations:Notify:Notify Worker:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>The observations submitted for the {{ Workflow.Campus }} Campus have been submitted by {{ Workflow.Observer }}:</p>

<h4><a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}'>{{ Workflow.Name }}</a></h4>
<p>{{ Workflow.Details }}</p>

{{ 'Global' | Attribute:'EmailFooter' }}" ); // Campus Observations:Notify:Notify Worker:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "9F5BDC3D-3558-4CF5-AD6A-C37BEC81B7AC", "62D0E0B1-47CB-437F-95DA-B40B8127CAE8", @"False" ); // Campus Observations:Notify:Notify Worker:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "359E8C35-E4B6-4083-84E9-7596D0F663D9", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Campus Observations:Complete Workflow:Workflow Completed:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "359E8C35-E4B6-4083-84E9-7596D0F663D9", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Campus Observations:Complete Workflow:Workflow Completed:Order
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
