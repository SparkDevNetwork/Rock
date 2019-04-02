// <copyright>
// Copyright by LCBC Church
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
namespace com.lcbcchurch.Workflow.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class GeneralContactWorkflow : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "System", "Holds system-level group types.", "Group", "Member", false, true, true, "fa fa-cogs", 0, null, 0, null, "15708E27-CC26-435F-B8B8-5E6CC48DDDA6" );
            RockMigrationHelper.AddGroupType( "Workflow Routing", "Holds groups that control routing of a Workflow to a worker.", "Group", "Member", false, true, true, "fa fa-code-fork", 0, null, 0, null, "943708E0-170B-4935-AA3A-3DF958D8D2C8" );

            AddGroupTypeAssociation( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6" );  // System > System
            AddGroupTypeAssociation( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "943708E0-170B-4935-AA3A-3DF958D8D2C8" );  // System > Workflow Routing

            RockMigrationHelper.UpdateGroupTypeRole( "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Member", "", 0, null, null, "88C1908F-7FAD-421D-B9F6-272E7C2DEF5B", false, false, true ); // System > Member
            RockMigrationHelper.UpdateGroupTypeRole( "943708E0-170B-4935-AA3A-3DF958D8D2C8", "Member", "", 0, null, null, "AF296AD9-CAC8-4941-8645-30DBDB8B6E4B", false, false, true ); // Workflow Routing > Member

            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "943708E0-170B-4935-AA3A-3DF958D8D2C8", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campus", @"", 0, "", "66AE33E8-B374-4F07-BBE3-4096F1CBCE88" );

            RockMigrationHelper.UpdateGroup( null, "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "System", "", null, 0, "D791D411-2434-4D2B-A9AC-E621BFAEC6E5", false, false, true ); // System
            RockMigrationHelper.UpdateGroup( "D791D411-2434-4D2B-A9AC-E621BFAEC6E5", "15708E27-CC26-435F-B8B8-5E6CC48DDDA6", "Workflows", "", null, 0, "351E56B1-5ED6-4C38-9D0E-E7EFAFD4DCA9", false, false, true ); // Workflows
            RockMigrationHelper.UpdateGroup( "351E56B1-5ED6-4C38-9D0E-E7EFAFD4DCA9", "943708E0-170B-4935-AA3A-3DF958D8D2C8", "General Contact Routing", "Group to hold people the General Contact Workflow can be assigned to. Mostly Campus Admins.", null, 0, "CDA9CAE4-0674-4CD2-A275-8104F953D5CB", false, false, true ); // General Contact Routing

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.ActivateActivity","38907A90-1634-4A93-8017-619326A4A582",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.AssignActivityFromAttributeValue","F100A31F-E93A-4C7A-9E55-0FAF41A101C4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.GetPersonFromFields","E5E7CA24-7030-4D48-9C39-04B5809E71A8",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersistWorkflow","F1A39347-6FE0-43D4-89FB-544195088ECF",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunLava","BC21E57A-1477-44B3-A7C2-61A806118945",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunSQL","A41216D6-6FB0-4019-B222-2C29B4519CF4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeFromPerson","17962C23-2E94-4E06-8461-0FB8B94E2FEA",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeValue","C789E457-0783-44B3-9D8F-2EBAB5F11110",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetWorkflowName","36005473-BD5D-470B-B28D-98E6D7ED808D",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","CE28B79D-FBC2-4894-9198-D923D0217549"); // Rock.Workflow.Action.SetAttributeFromPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The person attribute to set the value of.",0,@"","7AC47975-71AC-4A2F-BF1F-115CF5578D6F"); // Rock.Workflow.Action.SetAttributeFromPerson:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","18EF907D-607E-4891-B034-7AA379D77854"); // Rock.Workflow.Action.SetAttributeFromPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","The person to set attribute value to. Leave blank to set person to nobody.",1,@"","5C803BD1-40FA-49B1-AE7E-68F43D3687BB"); // Rock.Workflow.Action.SetAttributeFromPerson:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0A800013-51F7-4902-885A-5BE215D67D3D"); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","NameValue","The value to use for the workflow's name. <span class='tip tip-lava'></span>",1,@"","93852244-A667-4749-961A-D47F88675BE4"); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","5D95C15A-CCAE-40AD-A9DD-F929DA587115"); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E8ABD802-372C-47BE-82B1-96F50DB5169E"); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","739FD425-5B8C-4605-B775-7E4D9D4C11DB","Activity","Activity","The activity type to activate",0,@"","02D5A7A5-8781-46B4-B9FC-AF816829D240"); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3809A78C-B773-440C-8E3F-A8E81D0DAE08"); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","SQLQuery","SQLQuery","The SQL query to run. <span class='tip tip-lava'></span>",0,@"","F3B9908B-096F-460B-8320-122CF046D1F9"); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","A18C3143-0586-4565-9F36-E603BC674B4E"); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Continue On Error","ContinueOnError","Should processing continue even if SQL Error occurs?",3,@"False","423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC"); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","33E6DF69-BDFA-407A-9744-C175B60643AE","Result Attribute","ResultAttribute","An optional attribute to set to the scaler result of SQL query.",2,@"","56997192-2545-4EA1-B5B2-313B04588984"); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","73B02051-0D38-4AD9-BF81-A2D477DE4F70","Parameters","Parameters","The parameters to supply to the SQL query. <span class='tip tip-lava'></span>",1,@"","9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5"); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","FA7C685D-8636-41EF-9998-90FFF3998F76"); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava","Value","The <span class='tip tip-lava'></span> to run.",0,@"","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4"); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","F1924BDC-9B79-4018-9D4A-C3516C87A514"); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to store the result in.",1,@"","431273C6-342D-4030-ADC7-7CDEDC7F8B27"); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","The Lava commands that should be enabled for this action.",2,@"","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5"); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","1B833F48-EFC2-4537-B1E3-7793F6863EAA"); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","D7EAA859-F500-4521-9523-488B12EAA7D2"); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",0,@"","44A0B977-4730-4519-8FF6-B0A01A95B212"); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","Value","The text or attribute to set the value from. <span class='tip tip-lava'></span>",1,@"","E5272B11-A2B8-49DC-860D-8D574E2BC15C"); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","57093B41-50ED-48E5-B72B-8829E62704C8"); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","FE0EA0F6-9612-4E7C-A1EF-ADF0724F00BF"); // Rock.Workflow.Action.GetPersonFromFields:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","33E6DF69-BDFA-407A-9744-C175B60643AE","Default Campus","DefaultCampus","The attribute value to use as the default campus when creating a new person.",10,@"","CB3D18DB-E19C-48C4-B9ED-0764373E2598"); // Rock.Workflow.Action.GetPersonFromFields:Default Campus
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The person attribute to set the value to the person found or created.",7,@"","16307EEA-9646-42F7-9A31-0B5933B3C53C"); // Rock.Workflow.Action.GetPersonFromFields:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Birth Day|Attribute Value","BirthDay","The number corresponding to the birth day of a person or the attribute that contains the number corresponding to a birth day for a person  <span class='tip tip-lava'></span>",4,@"","577AF9C7-95FB-4B68-9A59-D74C87C71841"); // Rock.Workflow.Action.GetPersonFromFields:Birth Day|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Birth Month|Attribute Value","BirthMonth","The number corresponding to the birth month of a person or the attribute that contains the number corresponding to a birth month for a person  <span class='tip tip-lava'></span>",5,@"","388BF556-5D3E-4058-AF83-5AC1B1AE1486"); // Rock.Workflow.Action.GetPersonFromFields:Birth Month|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Birth Year|Attribute Value","BirthYear","The number corresponding to the birth year of a person or the attribute that contains the number corresponding to a birth year for a person  <span class='tip tip-lava'></span>",6,@"","3DA7C481-0929-45D8-A6BD-1F3D7851F3AB"); // Rock.Workflow.Action.GetPersonFromFields:Birth Year|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Email Address|Attribute Value","Email","The email address or an attribute that contains the email address of the person. <span class='tip tip-lava'></span>",2,@"","42B3DAD7-307A-4453-A7EA-674945DA72B4"); // Rock.Workflow.Action.GetPersonFromFields:Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","First Name|Attribute Value","FirstName","The first name or an attribute that contains the first name of the person. <span class='tip tip-lava'></span>",0,@"","02A1EA9F-AB3F-4D1A-91C0-173FBE974BDC"); // Rock.Workflow.Action.GetPersonFromFields:First Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Last Name|Attribute Value","LastName","The last name or an attribute that contains the last name of the person. <span class='tip tip-lava'></span>",1,@"","94A570D3-EC23-4EBA-A412-F43F91D91E3F"); // Rock.Workflow.Action.GetPersonFromFields:Last Name|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Mobile Number|Attribute Value","MobileNumber","The mobile phone number or an attribute that contains the mobile phone number of the person ) <span class='tip tip-lava'></span>",3,@"","00EF16C9-4F52-4982-956D-8C8CFFC012D5"); // Rock.Workflow.Action.GetPersonFromFields:Mobile Number|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","59D5A94C-94A0-4630-B80A-BB25697D74C7","Default Connection Status","DefaultConnectionStatus","The connection status to use when creating a new person",9,@"368DD475-242C-49C4-A42C-7278BE690CC2","203FA19A-50E0-449D-BB45-F12FC4ADB600"); // Rock.Workflow.Action.GetPersonFromFields:Default Connection Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","59D5A94C-94A0-4630-B80A-BB25697D74C7","Default Record Status","DefaultRecordStatus","The record status to use when creating a new person",8,@"283999EC-7346-42E3-B807-BCE9B2BABB49","35B6603A-20B3-4BC5-8320-52DF3D527754"); // Rock.Workflow.Action.GetPersonFromFields:Default Record Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("E5E7CA24-7030-4D48-9C39-04B5809E71A8","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","1CFE3B8B-7F1E-4498-8345-50133E4FDFDF"); // Rock.Workflow.Action.GetPersonFromFields:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Status|Status Attribute","Status","The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>",0,@"Completed","07CB7DBC-236D-4D38-92A4-47EE448BA89A"); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The person or group attribute value to assign this activity to.",0,@"","FBADD25F-D309-4512-8430-3CC8615DD60E"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","50B01639-4938-40D2-A791-AA0EB4F86847"); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Persist Immediately","PersistImmediately","This action will normally cause the workflow to be persisted (saved) once all the current activities/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.",0,@"False","E22BE348-18B1-4420-83A8-6319B35416D2"); // Rock.Workflow.Action.PersistWorkflow:Persist Immediately
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","86F795B0-0CB6-4DA4-9CE4-B11D0922F361"); // Rock.Workflow.Action.PersistWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","Requests","fa fa-question-circle","","78E38655-D951-41DB-A0FF-D6474775CFA1",0); // Requests

            #endregion

            #region General Contact

            RockMigrationHelper.UpdateWorkflowType(false,true,"General Contact","Used on public website when visitor selects to \"Contact Us\"","78E38655-D951-41DB-A0FF-D6474775CFA1","Inquiry","fa fa-phone",0,false,0,"9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",0); // General Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requester","Requester","The person who made the request. This is only available if the user was logged into the website when they made the inquiry.",0,@"","C6C6A13C-8ABC-4C14-97C1-61E1D4093125", false); // General Contact:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","9C204CD0-1233-41C5-818A-C5DA439445AA","First Name","FirstName","Your First Name",1,@"","E58F6294-52DE-4993-AD00-8332D5FCC908", false); // General Contact:First Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","9C204CD0-1233-41C5-818A-C5DA439445AA","Last Name","LastName","Your Last Name",2,@"","A105DC11-A1D1-4E10-A582-342F508F4D58", false); // General Contact:Last Name
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","9C204CD0-1233-41C5-818A-C5DA439445AA","Email Address","EmailAddress","Your email address.",3,@"","648FDC57-ECAE-4C76-98FD-8C25F5EEC09E", false); // General Contact:Email Address
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","9C204CD0-1233-41C5-818A-C5DA439445AA","Phone (optional)","Phone","Your phone number",4,@"","98F50ADB-F37F-4E4E-A4C2-F3C19D5DCAB3", false); // General Contact:Phone (optional)
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Inquiry","Inquiry","The details of your inquiry",5,@"","5FC49A8A-B7F9-4443-A460-ABE2CE33B5D1", false); // General Contact:Inquiry
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","1B71FEF4-201F-4D53-8C60-2DF21F1985ED","Campus","Campus","The campus (if any) that this inquiry is related to",6,@"","30DB30CB-0890-4212-A5EE-E0BC4E42BAFB", false); // General Contact:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Worker","Worker","The person responsible to follow up on the inquiry",7,@"","B5A5D6B4-592B-4CF0-8C20-CF488805608B", true); // General Contact:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Assign New Worker","NewWorker","If this inquiry needs to be re-assigned to a different person, select that person here.",8,@"","01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3", false); // General Contact:Assign New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Notes","Notes","Staff notes about this inquiry",9,@"","0DC37C7D-075F-4067-B276-B4CCC24E1562", false); // General Contact:Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","F4399CEF-827B-48B2-A735-F7806FCFE8E8","Worker Group","WorkerGroup","",10,@"CDA9CAE4-0674-4CD2-A275-8104F953D5CB","6E9BA6A0-E7BD-47D0-ADFF-3883BE6C8614", false); // General Contact:Worker Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D","9C204CD0-1233-41C5-818A-C5DA439445AA","WorkerGuid","WorkerGuid","",11,@"","59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B", false); // General Contact:WorkerGuid
            RockMigrationHelper.AddAttributeQualifier("59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B","ispassword",@"False","6B18CB80-6F8F-49AE-B3D6-58442C40751A"); // General Contact:WorkerGuid:ispassword
            RockMigrationHelper.AddAttributeQualifier("59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B","maxcharacters",@"","5528C5D2-8497-4A52-990F-1C45881EE1B7"); // General Contact:WorkerGuid:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B","showcountdown",@"False","D3EECC56-3FBF-496A-8F41-6D5A2249E846"); // General Contact:WorkerGuid:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",true,"Request","Prompt the user for the information about their request",true,0,"CDAB149A-11EA-4E50-A02A-343E4414810C"); // General Contact:Request
            RockMigrationHelper.UpdateWorkflowActivityType("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",true,"Open","Activity used to process the inquiry.",false,1,"FC5EED23-D70D-403A-BD2D-A8924B6BCBC8"); // General Contact:Open
            RockMigrationHelper.UpdateWorkflowActivityType("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",true,"Complete","Complete the workflow",false,2,"8BEF8579-3281-4DEE-85E2-DE912135534C"); // General Contact:Complete
            RockMigrationHelper.UpdateWorkflowActivityType("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",true,"Re-assign Worker","Assigns the inquiry to a new worker",false,3,"2FAC1A24-DC00-42C2-BF13-A62174F42D50"); // General Contact:Re-assign Worker
            RockMigrationHelper.UpdateWorkflowActivityType("9A5541BD-A914-4EE2-9CC0-DC6258D7D17D",true,"Create Prayer Request","Creates a Prayer Request from the General Contact's Inquiry text, modified as necessary.",false,4,"20430220-25CB-47C2-ACCB-044037A52A4F"); // General Contact:Create Prayer Request
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("20430220-25CB-47C2-ACCB-044037A52A4F","9C204CD0-1233-41C5-818A-C5DA439445AA","Prayer Command Text","PrayerCommandText","",0,@"","E17C1327-7BA3-40E9-94D0-885FB61C08DC"); // General Contact:Create Prayer Request:Prayer Command Text
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("20430220-25CB-47C2-ACCB-044037A52A4F","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Prayer Details","PrayerDetails","",1,@"","90D14FD3-1607-4379-87D4-891CE8A43B07"); // General Contact:Create Prayer Request:Prayer Details
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("20430220-25CB-47C2-ACCB-044037A52A4F","775899FB-AC17-4C2C-B809-CF3A1D2AA4E1","Prayer Category","PrayerCategory","",2,@"4b2d88f5-6e45-4b4b-8776-11118c8e8269","525E99F6-BCAF-40D9-B1DE-928707BA257C"); // General Contact:Create Prayer Request:Prayer Category
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("20430220-25CB-47C2-ACCB-044037A52A4F","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Is Prayer Request Public?","IsPrayerRequestPublic","",3,@"1","B2C5798B-9C7B-40E9-B55B-68F703FA5E3D"); // General Contact:Create Prayer Request:Is Prayer Request Public?
            RockMigrationHelper.AddAttributeQualifier("E17C1327-7BA3-40E9-94D0-885FB61C08DC","ispassword",@"False","04AD84F8-5C93-429C-862F-BF424F255B0E"); // General Contact:Prayer Command Text:ispassword
            RockMigrationHelper.AddAttributeQualifier("E17C1327-7BA3-40E9-94D0-885FB61C08DC","maxcharacters",@"","137D1ED7-F46F-4445-B709-895F7E95C30C"); // General Contact:Prayer Command Text:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("E17C1327-7BA3-40E9-94D0-885FB61C08DC","showcountdown",@"False","049762F8-07E9-47FC-AEE4-546D19E6FB29"); // General Contact:Prayer Command Text:showcountdown
            RockMigrationHelper.AddAttributeQualifier("90D14FD3-1607-4379-87D4-891CE8A43B07","allowhtml",@"False","5329C98D-1249-40CF-9F81-91AD3A9579A4"); // General Contact:Prayer Details:allowhtml
            RockMigrationHelper.AddAttributeQualifier("90D14FD3-1607-4379-87D4-891CE8A43B07","maxcharacters",@"","AA1C0653-E7DB-4762-98ED-5DC8514EBA31"); // General Contact:Prayer Details:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("90D14FD3-1607-4379-87D4-891CE8A43B07","numberofrows",@"3","8DE33115-4CD7-40E0-B456-DD2618E6C5DE"); // General Contact:Prayer Details:numberofrows
            RockMigrationHelper.AddAttributeQualifier("90D14FD3-1607-4379-87D4-891CE8A43B07","showcountdown",@"False","4CF8BDDA-63B9-4AB8-963A-E3F4E79A1897"); // General Contact:Prayer Details:showcountdown
            RockMigrationHelper.AddAttributeQualifier("525E99F6-BCAF-40D9-B1DE-928707BA257C","entityTypeName",@"Rock.Model.PrayerRequest","24CA64E6-2F57-4153-9764-CBD44313832A"); // General Contact:Prayer Category:entityTypeName
            RockMigrationHelper.AddAttributeQualifier("525E99F6-BCAF-40D9-B1DE-928707BA257C","qualifierColumn",@"","84F92BA0-398E-4514-A4EE-C1B87B3E305A"); // General Contact:Prayer Category:qualifierColumn
            RockMigrationHelper.AddAttributeQualifier("525E99F6-BCAF-40D9-B1DE-928707BA257C","qualifierValue",@"","85992B48-F23B-4576-B950-020E0EAC37F2"); // General Contact:Prayer Category:qualifierValue
            RockMigrationHelper.AddAttributeQualifier("B2C5798B-9C7B-40E9-B55B-68F703FA5E3D","fieldtype",@"ddl","A9DB457E-B82F-41DE-B64B-4324823B7B4D"); // General Contact:Is Prayer Request Public?:fieldtype
            RockMigrationHelper.AddAttributeQualifier("B2C5798B-9C7B-40E9-B55B-68F703FA5E3D","values",@"1^Yes,0^No","8D9565E2-372E-47EC-86AA-1E3A290D3F2A"); // General Contact:Is Prayer Request Public?:values
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h2>How Can We Help?</h2> <p> Complete the form below and we'll forward your inquiry to the appropriate team member. Please allow 2-3  business days for a response. {% if 'Global' | Attribute:'OrganizationPhone' != Empty %}If you need assistance right  away, please call <strong>{{ 'Global' | Attribute:'OrganizationPhone' }}</strong> to speak with someone.{% endif %} </p> <br/>",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Thank You. Your request has been forwarded to a staff member who will be following up with you soon.","",true,"","BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8"); // General Contact:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h4>{{ Workflow | Attribute:'Topic' }} Inquiry from {{ Workflow | Attribute:'FirstName' }} {{ Workflow | Attribute:'LastName' }}</h4> <p>The following inquiry has been submitted by a visitor to our website.</p>",@"","Update^638beee0-2f8f-4706-b9a4-5bab70386697^FC5EED23-D70D-403A-BD2D-A8924B6BCBC8^The information you entered has been saved.|Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^8BEF8579-3281-4DEE-85E2-DE912135534C^|Create Prayer Request^3c026b37-29d4-47cb-bb6e-da43afe779fe^20430220-25CB-47C2-ACCB-044037A52A4F^|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","5FB12E48-0F07-4B31-B17E-0609C93DC5E8"); // General Contact:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionForm(@"",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your prayer request was created successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^^|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"E17C1327-7BA3-40E9-94D0-885FB61C08DC","0082B677-1060-48DA-B739-48F8DE05395F"); // General Contact:Create Prayer Request:Worker User Entry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","C6C6A13C-8ABC-4C14-97C1-61E1D4093125",0,false,true,false,false,@"",@"","44F7F66F-EFE8-48B6-A755-5B7AD1314353"); // General Contact:Request:Prompt User:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","E58F6294-52DE-4993-AD00-8332D5FCC908",1,true,false,true,false,@"",@"","673E01F9-F8BD-46BC-BD51-99B10DB25F2C"); // General Contact:Request:Prompt User:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","A105DC11-A1D1-4E10-A582-342F508F4D58",2,true,false,true,false,@"",@"","25D4033F-6737-49E0-BF32-054294FF5154"); // General Contact:Request:Prompt User:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","648FDC57-ECAE-4C76-98FD-8C25F5EEC09E",3,true,false,true,false,@"",@"","DEE71147-4FDD-47A9-B192-88CB772ADCD1"); // General Contact:Request:Prompt User:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","98F50ADB-F37F-4E4E-A4C2-F3C19D5DCAB3",4,true,false,false,false,@"",@"","3B0FF399-A908-4F82-A2AB-CC7A8A6D1D29"); // General Contact:Request:Prompt User:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","5FC49A8A-B7F9-4443-A460-ABE2CE33B5D1",5,true,false,true,false,@"",@"","30584212-F144-4DE2-9B9C-7DE0E55C134E"); // General Contact:Request:Prompt User:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","30DB30CB-0890-4212-A5EE-E0BC4E42BAFB",6,true,false,false,false,@"",@"","BD0E9547-5621-472C-8C5D-99B314DFC770"); // General Contact:Request:Prompt User:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","B5A5D6B4-592B-4CF0-8C20-CF488805608B",7,false,true,false,false,@"",@"","FDBA0D12-65E7-488C-A227-8F45D8627D90"); // General Contact:Request:Prompt User:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3",9,false,true,false,false,@"",@"","72B2E7B6-79FE-4F4F-BF49-A35874A08305"); // General Contact:Request:Prompt User:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","0DC37C7D-075F-4067-B276-B4CCC24E1562",8,false,true,false,false,@"",@"","401090E3-9915-479B-917D-57D391056040"); // General Contact:Request:Prompt User:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","6E9BA6A0-E7BD-47D0-ADFF-3883BE6C8614",10,false,true,false,false,@"",@"","6048C019-3C98-486E-8794-81F2356AB507"); // General Contact:Request:Prompt User:Worker Group
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B",11,false,true,false,false,@"",@"","F84AA094-1880-42A9-B287-1848415FB6F0"); // General Contact:Request:Prompt User:WorkerGuid
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","C6C6A13C-8ABC-4C14-97C1-61E1D4093125",0,true,true,false,false,@"",@"","0912694A-B693-4B09-B19B-C61AD2D8BF2B"); // General Contact:Open:Capture Notes:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","E58F6294-52DE-4993-AD00-8332D5FCC908",1,false,true,false,false,@"",@"","9302B1E1-751E-46C3-BF7C-BA3EAE75B213"); // General Contact:Open:Capture Notes:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","A105DC11-A1D1-4E10-A582-342F508F4D58",2,false,true,false,false,@"",@"","027DC21E-CA37-4FDD-A2C8-202E44A10A4C"); // General Contact:Open:Capture Notes:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","648FDC57-ECAE-4C76-98FD-8C25F5EEC09E",3,true,true,false,false,@"",@"","864FD88D-9679-448D-81FF-11C51391300E"); // General Contact:Open:Capture Notes:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","98F50ADB-F37F-4E4E-A4C2-F3C19D5DCAB3",4,true,true,false,false,@"",@"","4DF40669-555E-4B18-B858-DE30E6898EC3"); // General Contact:Open:Capture Notes:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","5FC49A8A-B7F9-4443-A460-ABE2CE33B5D1",5,true,true,false,false,@"",@"","2953EFC0-7C8C-4440-B4E0-20C65444473C"); // General Contact:Open:Capture Notes:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","30DB30CB-0890-4212-A5EE-E0BC4E42BAFB",6,true,false,true,false,@"",@"","3B2C350D-72B0-4334-9D56-A8D974818C4A"); // General Contact:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","B5A5D6B4-592B-4CF0-8C20-CF488805608B",7,false,true,false,false,@"",@"","161211A3-B659-4ABD-AE1F-9FA8FCABE8F4"); // General Contact:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3",8,true,false,false,false,@"",@"","A9B12036-631D-4C3C-8520-A8B6EE2AC6F4"); // General Contact:Open:Capture Notes:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","0DC37C7D-075F-4067-B276-B4CCC24E1562",9,true,false,false,false,@"",@"","203709FC-8D7B-41DE-92DE-04A365D6BAB9"); // General Contact:Open:Capture Notes:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","6E9BA6A0-E7BD-47D0-ADFF-3883BE6C8614",10,false,true,false,false,@"",@"","12E03678-8B9E-4AC5-9ED2-A83E8D61BD0B"); // General Contact:Open:Capture Notes:Worker Group
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("5FB12E48-0F07-4B31-B17E-0609C93DC5E8","59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B",11,false,true,false,false,@"",@"","171554AD-9793-4D2E-95CC-91BFA9719F10"); // General Contact:Open:Capture Notes:WorkerGuid
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","C6C6A13C-8ABC-4C14-97C1-61E1D4093125",0,true,true,false,false,@"",@"","6FA0B462-DE5B-4D7E-9064-C23500F72B59"); // General Contact:Create Prayer Request:Worker User Entry:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","E17C1327-7BA3-40E9-94D0-885FB61C08DC",15,false,true,false,false,@"",@"","335EA175-9F47-4928-8F4D-91BAED1E4061"); // General Contact:Create Prayer Request:Worker User Entry:Prayer Command Text
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","90D14FD3-1607-4379-87D4-891CE8A43B07",13,true,false,true,false,@"",@"","FCAB7FE8-E841-478F-A2EA-B1BDB105DFBD"); // General Contact:Create Prayer Request:Worker User Entry:Prayer Details
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","E58F6294-52DE-4993-AD00-8332D5FCC908",1,true,true,false,false,@"",@"","6EA7FC35-5D52-494A-A28C-A1F9238B5923"); // General Contact:Create Prayer Request:Worker User Entry:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","A105DC11-A1D1-4E10-A582-342F508F4D58",2,true,true,false,false,@"",@"","FCC82FD6-BDF5-4F8A-BBA9-E5314D270321"); // General Contact:Create Prayer Request:Worker User Entry:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","525E99F6-BCAF-40D9-B1DE-928707BA257C",12,true,false,true,false,@"",@"","20BBA14C-2E37-4709-8A06-5DC891E9F871"); // General Contact:Create Prayer Request:Worker User Entry:Prayer Category
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","B2C5798B-9C7B-40E9-B55B-68F703FA5E3D",14,true,false,true,false,@"",@"","E053F15C-F881-4F3F-9506-C2175110A2F5"); // General Contact:Create Prayer Request:Worker User Entry:Is Prayer Request Public?
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","648FDC57-ECAE-4C76-98FD-8C25F5EEC09E",3,false,true,false,false,@"",@"","A83BFE32-AE21-4E29-9A7E-74A1A3B5D6A4"); // General Contact:Create Prayer Request:Worker User Entry:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","98F50ADB-F37F-4E4E-A4C2-F3C19D5DCAB3",4,false,true,false,false,@"",@"","F25B5C4B-1D81-4CBB-A58C-2EE09AA52BA2"); // General Contact:Create Prayer Request:Worker User Entry:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","5FC49A8A-B7F9-4443-A460-ABE2CE33B5D1",5,false,true,false,false,@"",@"","F8F0AC01-A909-4A05-B690-9AD81E4C0F6A"); // General Contact:Create Prayer Request:Worker User Entry:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","30DB30CB-0890-4212-A5EE-E0BC4E42BAFB",6,true,true,false,false,@"",@"","579D466B-D28C-4DF5-A495-2869F0CA6F56"); // General Contact:Create Prayer Request:Worker User Entry:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","B5A5D6B4-592B-4CF0-8C20-CF488805608B",7,false,true,false,false,@"",@"","3F2FBD96-DB48-4A2C-973A-42D7BED63298"); // General Contact:Create Prayer Request:Worker User Entry:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3",8,false,true,false,false,@"",@"","31A04179-6B86-4390-A1E7-8581CAB62537"); // General Contact:Create Prayer Request:Worker User Entry:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","0DC37C7D-075F-4067-B276-B4CCC24E1562",9,true,true,false,false,@"",@"","0879FCE7-6B98-47F2-816E-4F3738072E85"); // General Contact:Create Prayer Request:Worker User Entry:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","6E9BA6A0-E7BD-47D0-ADFF-3883BE6C8614",10,false,true,false,false,@"",@"","021E4FAB-DD46-4F5B-AE79-B611EC71478B"); // General Contact:Create Prayer Request:Worker User Entry:Worker Group
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("0082B677-1060-48DA-B739-48F8DE05395F","59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B",11,false,true,false,false,@"",@"","F5A85E53-C473-47AF-A432-5D595020CCB7"); // General Contact:Create Prayer Request:Worker User Entry:WorkerGuid
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Prompt User",0,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"BE95DB03-7967-4FD7-A831-0ADA6BF8FFE8","",1,"","9CE9BE24-853A-4B10-8A35-AB1C5473A015"); // General Contact:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Set Name",1,"36005473-BD5D-470B-B28D-98E6D7ED808D",true,false,"","",1,"","9718742D-5416-49E0-B05B-ADEEB10A46DB"); // General Contact:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Set Requester",2,"E5E7CA24-7030-4D48-9C39-04B5809E71A8",true,false,"","",1,"","F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D"); // General Contact:Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Persist the Workflow",3,"F1A39347-6FE0-43D4-89FB-544195088ECF",true,false,"","",1,"","EED11036-BC0F-480C-9CBE-F922536B8DE5"); // General Contact:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Get Initial Worker Guid",4,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",1,"","448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0"); // General Contact:Request:Get Initial Worker Guid
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Assign Initial Worker",5,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","8DE81D47-91FC-478D-8E81-6E7C9CF28726"); // General Contact:Request:Assign Initial Worker
            RockMigrationHelper.UpdateWorkflowActionType("CDAB149A-11EA-4E50-A02A-343E4414810C","Activate Open Activity",6,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","",32,"","00E5D268-53B5-49A9-B1E3-7903AAEAF2D1"); // General Contact:Request:Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType("FC5EED23-D70D-403A-BD2D-A8924B6BCBC8","Assign New Worker",0,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3",64,"","0DBA0176-D784-44C3-8FEB-5CC2DC9DE416"); // General Contact:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType("FC5EED23-D70D-403A-BD2D-A8924B6BCBC8","Assign Activity to Worker",1,"F100A31F-E93A-4C7A-9E55-0FAF41A101C4",true,false,"","",1,"","B6EE0C6C-B999-47B4-9A51-AC2D11B6B4EC"); // General Contact:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType("FC5EED23-D70D-403A-BD2D-A8924B6BCBC8","Capture Notes",2,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,true,"5FB12E48-0F07-4B31-B17E-0609C93DC5E8","",1,"","5176ED2B-6538-432E-BD72-26AD8A3E5487"); // General Contact:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType("8BEF8579-3281-4DEE-85E2-DE912135534C","Complete Workflow",0,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,false,"","",1,"","FA213435-AC9B-469B-A22B-8CB8E7038259"); // General Contact:Complete:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType("2FAC1A24-DC00-42C2-BF13-A62174F42D50","Set Worker",0,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","D72546E1-8BC0-45F8-AC34-1F840C5E9C8F"); // General Contact:Re-assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType("2FAC1A24-DC00-42C2-BF13-A62174F42D50","Clear New Worker Value",1,"17962C23-2E94-4E06-8461-0FB8B94E2FEA",true,false,"","",1,"","EF5E8C89-3159-4132-9BAB-EB7767C903DB"); // General Contact:Re-assign Worker:Clear New Worker Value
            RockMigrationHelper.UpdateWorkflowActionType("2FAC1A24-DC00-42C2-BF13-A62174F42D50","Re-Activate Open Activity",2,"38907A90-1634-4A93-8017-619326A4A582",true,false,"","",1,"","1DBD663E-8EAB-45AF-BB9A-59E1DF104E0C"); // General Contact:Re-assign Worker:Re-Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType("20430220-25CB-47C2-ACCB-044037A52A4F","Create Prayer Details from Inquiry",0,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","D6388186-DAE6-4725-BDB6-DFEEE1CBEC3A"); // General Contact:Create Prayer Request:Create Prayer Details from Inquiry
            RockMigrationHelper.UpdateWorkflowActionType("20430220-25CB-47C2-ACCB-044037A52A4F","Worker User Entry",1,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"0082B677-1060-48DA-B739-48F8DE05395F","",1,"","EF23F4A2-A321-4384-AB43-C72D5383D07C"); // General Contact:Create Prayer Request:Worker User Entry
            RockMigrationHelper.UpdateWorkflowActionType("20430220-25CB-47C2-ACCB-044037A52A4F","Create Prayer Request",2,"A41216D6-6FB0-4019-B222-2C29B4519CF4",true,false,"","E17C1327-7BA3-40E9-94D0-885FB61C08DC",8,"Submit","983B2D5C-9374-4CB2-99DC-AB0B12DC0999"); // General Contact:Create Prayer Request:Create Prayer Request
            RockMigrationHelper.UpdateWorkflowActionType("20430220-25CB-47C2-ACCB-044037A52A4F","Return To Open",3,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","",1,"","CF5395B9-5C35-4F38-9A01-E70E3D08446F"); // General Contact:Create Prayer Request:Return To Open
            RockMigrationHelper.AddActionTypeAttributeValue("9CE9BE24-853A-4B10-8A35-AB1C5473A015","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // General Contact:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue("9CE9BE24-853A-4B10-8A35-AB1C5473A015","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // General Contact:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue("9718742D-5416-49E0-B05B-ADEEB10A46DB","0A800013-51F7-4902-885A-5BE215D67D3D",@"False"); // General Contact:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue("9718742D-5416-49E0-B05B-ADEEB10A46DB","5D95C15A-CCAE-40AD-A9DD-F929DA587115",@""); // General Contact:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue("9718742D-5416-49E0-B05B-ADEEB10A46DB","93852244-A667-4749-961A-D47F88675BE4",@"{{ Workflow | Attribute:'FirstName' }} {{ Workflow | Attribute:'LastName' }}"); // General Contact:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","FE0EA0F6-9612-4E7C-A1EF-ADF0724F00BF",@"False"); // General Contact:Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","1CFE3B8B-7F1E-4498-8345-50133E4FDFDF",@""); // General Contact:Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","02A1EA9F-AB3F-4D1A-91C0-173FBE974BDC",@"e58f6294-52de-4993-ad00-8332d5fcc908"); // General Contact:Request:Set Requester:First Name|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","94A570D3-EC23-4EBA-A412-F43F91D91E3F",@"a105dc11-a1d1-4e10-a582-342f508f4d58"); // General Contact:Request:Set Requester:Last Name|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","42B3DAD7-307A-4453-A7EA-674945DA72B4",@"648fdc57-ecae-4c76-98fd-8c25f5eec09e"); // General Contact:Request:Set Requester:Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","00EF16C9-4F52-4982-956D-8C8CFFC012D5",@"98f50adb-f37f-4e4e-a4c2-f3c19d5dcab3"); // General Contact:Request:Set Requester:Mobile Number|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","577AF9C7-95FB-4B68-9A59-D74C87C71841",@""); // General Contact:Request:Set Requester:Birth Day|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","388BF556-5D3E-4058-AF83-5AC1B1AE1486",@""); // General Contact:Request:Set Requester:Birth Month|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","3DA7C481-0929-45D8-A6BD-1F3D7851F3AB",@""); // General Contact:Request:Set Requester:Birth Year|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","16307EEA-9646-42F7-9A31-0B5933B3C53C",@"c6c6a13c-8abc-4c14-97c1-61e1d4093125"); // General Contact:Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","35B6603A-20B3-4BC5-8320-52DF3D527754",@"283999ec-7346-42e3-b807-bce9b2babb49"); // General Contact:Request:Set Requester:Default Record Status
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","203FA19A-50E0-449D-BB45-F12FC4ADB600",@"368dd475-242c-49c4-a42c-7278be690cc2"); // General Contact:Request:Set Requester:Default Connection Status
            RockMigrationHelper.AddActionTypeAttributeValue("F11E2B39-B9C9-4C0A-8BB5-0C04318DA06D","CB3D18DB-E19C-48C4-B9ED-0764373E2598",@"30db30cb-0890-4212-a5ee-e0bc4e42bafb"); // General Contact:Request:Set Requester:Default Campus
            RockMigrationHelper.AddActionTypeAttributeValue("EED11036-BC0F-480C-9CBE-F922536B8DE5","50B01639-4938-40D2-A791-AA0EB4F86847",@"False"); // General Contact:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("EED11036-BC0F-480C-9CBE-F922536B8DE5","86F795B0-0CB6-4DA4-9CE4-B11D0922F361",@""); // General Contact:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue("EED11036-BC0F-480C-9CBE-F922536B8DE5","E22BE348-18B1-4420-83A8-6319B35416D2",@"False"); // General Contact:Request:Persist the Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue("448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"
{% assign group = Workflow | Attribute:'WorkerGroup','Object' %}
{% assign personGuid = '' %}
{% for gm in group.Members %}
    {% if personGuid == '' %}
        {% assign memberCampuses = gm | Attribute:'Campus','RawValue' %}
        {% assign requestCampusGuid = Workflow | Attribute:'Campus','Guid' %}

        {% if requestCampusGuid != '' %}
            {% if memberCampuses contains requestCampusGuid %}
                {% assign personGuid = gm.Person.PrimaryAlias.Guid %}
            {% elseif memberCampuses == '' %}
                {% assign personGuid = gm.Person.PrimaryAlias.Guid %}
            {% endif %}
        {% else %}
            {% assign personGuid = gm.Person.PrimaryAlias.Guid %}
        {% endif %}
    {% endif %}
{% endfor %}
{{personGuid}} " ); // General Contact:Request:Get Initial Worker Guid:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // General Contact:Request:Get Initial Worker Guid:Active
            RockMigrationHelper.AddActionTypeAttributeValue("448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0","1B833F48-EFC2-4537-B1E3-7793F6863EAA",@""); // General Contact:Request:Get Initial Worker Guid:Order
            RockMigrationHelper.AddActionTypeAttributeValue("448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"59f2f9b3-ca30-4b77-8bb3-fdf035d3e63b"); // General Contact:Request:Get Initial Worker Guid:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("448A26ED-5AC0-4FA1-9124-ACDC3A3C7DA0","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // General Contact:Request:Get Initial Worker Guid:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("8DE81D47-91FC-478D-8E81-6E7C9CF28726","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // General Contact:Request:Assign Initial Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue("8DE81D47-91FC-478D-8E81-6E7C9CF28726","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // General Contact:Request:Assign Initial Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue("8DE81D47-91FC-478D-8E81-6E7C9CF28726","44A0B977-4730-4519-8FF6-B0A01A95B212",@"b5a5d6b4-592b-4cf0-8c20-cf488805608b"); // General Contact:Request:Assign Initial Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("8DE81D47-91FC-478D-8E81-6E7C9CF28726","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"{{ Workflow | Attribute:'WorkerGuid' | Trim }}"); // General Contact:Request:Assign Initial Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("00E5D268-53B5-49A9-B1E3-7903AAEAF2D1","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // General Contact:Request:Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("00E5D268-53B5-49A9-B1E3-7903AAEAF2D1","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // General Contact:Request:Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue("00E5D268-53B5-49A9-B1E3-7903AAEAF2D1","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"FC5EED23-D70D-403A-BD2D-A8924B6BCBC8"); // General Contact:Request:Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("0DBA0176-D784-44C3-8FEB-5CC2DC9DE416","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // General Contact:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue("0DBA0176-D784-44C3-8FEB-5CC2DC9DE416","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // General Contact:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue("0DBA0176-D784-44C3-8FEB-5CC2DC9DE416","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"2FAC1A24-DC00-42C2-BF13-A62174F42D50"); // General Contact:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("B6EE0C6C-B999-47B4-9A51-AC2D11B6B4EC","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF",@"False"); // General Contact:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B6EE0C6C-B999-47B4-9A51-AC2D11B6B4EC","FBADD25F-D309-4512-8430-3CC8615DD60E",@"b5a5d6b4-592b-4cf0-8c20-cf488805608b"); // General Contact:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("B6EE0C6C-B999-47B4-9A51-AC2D11B6B4EC","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA",@""); // General Contact:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue("5176ED2B-6538-432E-BD72-26AD8A3E5487","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // General Contact:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue("5176ED2B-6538-432E-BD72-26AD8A3E5487","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // General Contact:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue("FA213435-AC9B-469B-A22B-8CB8E7038259","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // General Contact:Complete:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("FA213435-AC9B-469B-A22B-8CB8E7038259","25CAD4BE-5A00-409D-9BAB-E32518D89956",@""); // General Contact:Complete:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue("FA213435-AC9B-469B-A22B-8CB8E7038259","07CB7DBC-236D-4D38-92A4-47EE448BA89A",@"Completed"); // General Contact:Complete:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("D72546E1-8BC0-45F8-AC34-1F840C5E9C8F","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // General Contact:Re-assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue("D72546E1-8BC0-45F8-AC34-1F840C5E9C8F","44A0B977-4730-4519-8FF6-B0A01A95B212",@"b5a5d6b4-592b-4cf0-8c20-cf488805608b"); // General Contact:Re-assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("D72546E1-8BC0-45F8-AC34-1F840C5E9C8F","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // General Contact:Re-assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue("D72546E1-8BC0-45F8-AC34-1F840C5E9C8F","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"01dd1cda-d3e2-49fb-9d1c-e945718c5eb3"); // General Contact:Re-assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("EF5E8C89-3159-4132-9BAB-EB7767C903DB","CE28B79D-FBC2-4894-9198-D923D0217549",@"False"); // General Contact:Re-assign Worker:Clear New Worker Value:Active
            RockMigrationHelper.AddActionTypeAttributeValue("EF5E8C89-3159-4132-9BAB-EB7767C903DB","7AC47975-71AC-4A2F-BF1F-115CF5578D6F",@"01dd1cda-d3e2-49fb-9d1c-e945718c5eb3"); // General Contact:Re-assign Worker:Clear New Worker Value:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("EF5E8C89-3159-4132-9BAB-EB7767C903DB","18EF907D-607E-4891-B034-7AA379D77854",@""); // General Contact:Re-assign Worker:Clear New Worker Value:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue("EF5E8C89-3159-4132-9BAB-EB7767C903DB","5C803BD1-40FA-49B1-AE7E-68F43D3687BB",@""); // General Contact:Re-assign Worker:Clear New Worker Value:Person
            RockMigrationHelper.AddActionTypeAttributeValue("1DBD663E-8EAB-45AF-BB9A-59E1DF104E0C","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // General Contact:Re-assign Worker:Re-Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("1DBD663E-8EAB-45AF-BB9A-59E1DF104E0C","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // General Contact:Re-assign Worker:Re-Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue("1DBD663E-8EAB-45AF-BB9A-59E1DF104E0C","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"FC5EED23-D70D-403A-BD2D-A8924B6BCBC8"); // General Contact:Re-assign Worker:Re-Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("D6388186-DAE6-4725-BDB6-DFEEE1CBEC3A","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // General Contact:Create Prayer Request:Create Prayer Details from Inquiry:Active
            RockMigrationHelper.AddActionTypeAttributeValue("D6388186-DAE6-4725-BDB6-DFEEE1CBEC3A","44A0B977-4730-4519-8FF6-B0A01A95B212",@"90d14fd3-1607-4379-87d4-891ce8a43b07"); // General Contact:Create Prayer Request:Create Prayer Details from Inquiry:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("D6388186-DAE6-4725-BDB6-DFEEE1CBEC3A","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // General Contact:Create Prayer Request:Create Prayer Details from Inquiry:Order
            RockMigrationHelper.AddActionTypeAttributeValue("D6388186-DAE6-4725-BDB6-DFEEE1CBEC3A","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"5fc49a8a-b7f9-4443-a460-abe2ce33b5d1"); // General Contact:Create Prayer Request:Create Prayer Details from Inquiry:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("EF23F4A2-A321-4384-AB43-C72D5383D07C","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // General Contact:Create Prayer Request:Worker User Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue("EF23F4A2-A321-4384-AB43-C72D5383D07C","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // General Contact:Create Prayer Request:Worker User Entry:Order
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","F3B9908B-096F-460B-8320-122CF046D1F9",@"{% assign aliasId = Workflow | Attribute:""Requestor"",""PrimaryAliasId"" %} {% if aliasId > 0 %}  INSERT INTO PrayerRequest ( FirstName, LastName, Email, Text, EnteredDateTime, ExpirationDate, AllowComments, IsUrgent, IsPublic, IsActive, IsApproved, [Guid], CreatedDateTime, RequestedByPersonAliasId, CategoryId, CampusId ) VALUES (     '{{ Workflow | Attribute:""FirstName"" }}'     , '{{ Workflow | Attribute:""LastName"" }}'     , '{{ Workflow | Attribute:""EmailAddress"" }}'     , '{{ Activity | Attribute:""PrayerDetails"" }}'     , '{{ ""Now"" | Date:""yyyy-MM-dd"" }}'     , '{{ ""Now"" | DateAdd:14,""d"" | Date:""yyyy-MM-dd"" }}'     , 1      , 0     , '{{ Activity | Attribute:""IsPrayerRequestPublic"",""RawValue"" }}'     , 1      , 1      , NewId()     , '{{ ""Now"" | Date:""yyyy-MM-dd"" }}'     , '{{ aliasId }}'     , '{{ Activity | Attribute:""PrayerCategory"",""Id"" }}'     , '{{ Workflow | Attribute:""Campus"",""Id"" }}' );  {% else %}  INSERT INTO PrayerRequest ( FirstName, LastName, Email, Text, EnteredDateTime, ExpirationDate, AllowComments, IsUrgent, IsPublic, IsActive, IsApproved, [Guid], CreatedDateTime, CategoryId, CampusId ) VALUES (     '{{ Workflow | Attribute:""FirstName"" }}'     , '{{ Workflow | Attribute:""LastName"" }}'     , '{{ Workflow | Attribute:""EmailAddress"" }}'     , '{{ Activity | Attribute:""PrayerDetails"" }}'     , '{{ ""Now"" | Date:""yyyy-MM-dd"" }}'     , '{{ ""Now"" | DateAdd:14,""d"" | Date:""yyyy-MM-dd"" }}'     , 1      , 0     , '{{ Activity | Attribute:""IsPrayerRequestPublic"",""RawValue"" }}'     , 1      , 1      , NewId()     , '{{ ""Now"" | Date:""yyyy-MM-dd"" }}'     , '{{ Activity | Attribute:""PrayerCategory"",""Id"" }}'     , '{{ Workflow | Attribute:""Campus"",""Id"" }}' );   {% endif %}  "); // General Contact:Create Prayer Request:Create Prayer Request:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","A18C3143-0586-4565-9F36-E603BC674B4E",@"False"); // General Contact:Create Prayer Request:Create Prayer Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","FA7C685D-8636-41EF-9998-90FFF3998F76",@""); // General Contact:Create Prayer Request:Create Prayer Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5",@""); // General Contact:Create Prayer Request:Create Prayer Request:Parameters
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","56997192-2545-4EA1-B5B2-313B04588984",@""); // General Contact:Create Prayer Request:Create Prayer Request:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("983B2D5C-9374-4CB2-99DC-AB0B12DC0999","423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC",@"True"); // General Contact:Create Prayer Request:Create Prayer Request:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue("CF5395B9-5C35-4F38-9A01-E70E3D08446F","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // General Contact:Create Prayer Request:Return To Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue("CF5395B9-5C35-4F38-9A01-E70E3D08446F","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // General Contact:Create Prayer Request:Return To Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue("CF5395B9-5C35-4F38-9A01-E70E3D08446F","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"FC5EED23-D70D-403A-BD2D-A8924B6BCBC8"); // General Contact:Create Prayer Request:Return To Open:Activity

            #endregion

            #region DefinedValue AttributeType qualifier helper


            Sql( @"     UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )     FROM [AttributeQualifier] [aq]     INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]     INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]     INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]     WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'     AND [aq].[key] = 'definedtypeguid'    " );


            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Contact Us, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "CA7D13BB-6781-4908-9198-CF89E915F9D7", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"9a5541bd-a914-4ee2-9cc0-dc6258d7d17d" );

            #endregion
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "CCC95489-99D9-48B5-BDD4-BE28F58C61FE" );
            RockMigrationHelper.DeleteGroup( "CDA9CAE4-0674-4CD2-A275-8104F953D5CB" );
        }

        private void AddGroupGroupMemberAttribute( string groupGuid, string fieldTypeGuid, string name, string description, int order, string defaultValue, bool isRequired, string guid, bool isGridColumn = false )
        {
            string defaultValueDbParam = ( defaultValue == null ) ? "NULL" : "'" + defaultValue + "'";
            string attributeKey = name.RemoveSpaces();

            Sql( $@"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupMember')

                DECLARE @GroupId int
                SET @GroupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{groupGuid}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{fieldTypeGuid}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'GroupId'
                    AND [EntityTypeQualifierValue] = @GroupId
                    AND [Key] = '{attributeKey}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [FieldTypeId] = @FieldTypeId,
                        [Name] = '{name}',
                        [Description] = '{description.Replace( "'", "''" )}',
                        [Order] = {order},
                        [DefaultValue] = {defaultValueDbParam},
                        [IsRequired]= {isRequired.Bit()},
                        [Guid] = '{guid}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'GroupId'
                    AND [EntityTypeQualifierValue] = @GroupId
                    AND [Key] = '{attributeKey}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute]
                        ([IsSystem]
                        ,[FieldTypeId]
                        ,[EntityTypeId]
                        ,[EntityTypeQualifierColumn]
                        ,[EntityTypeQualifierValue]
                        ,[Key]
                        ,[Name]
                        ,[Description]
                        ,[Order]
                        ,[IsGridColumn]
                        ,[DefaultValue]
                        ,[IsMultiValue]
                        ,[IsRequired]
                        ,[Guid])
                    VALUES
                        (1
                        ,@FieldTypeId
                        ,@EntityTypeId
                        ,'GroupId'
                        ,@GroupId
                        ,'{attributeKey}'
                        ,'{name}'
                        ,'{description.Replace( "'", "''" )}'
                        ,{order}
                        ,{isGridColumn.Bit()}
                        ,{defaultValueDbParam}
                        ,0
                        ,{isRequired.Bit()}
                        ,'{guid}')
                END
                " );
        }

        public void AddGroupTypeAssociation( string parentGroupTypeGuid, string childGroupTypeGuid )
        {
            Sql( string.Format( @"

                -- Insert a group type association...

                DECLARE @ParentGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{0}' )
                DECLARE @ChildGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{1}' )

                IF NOT EXISTS (
                    SELECT [GroupTypeId]
                    FROM [GroupTypeAssociation]
                    WHERE [GroupTypeId] = @ParentGroupTypeId
                    AND [ChildGroupTypeId] = @ChildGroupTypeId)
                BEGIN
                    INSERT INTO [GroupTypeAssociation] (
                        [GroupTypeId]
                        ,[ChildGroupTypeId])
                    VALUES(
                        @ParentGroupTypeId
                        ,@ChildGroupTypeId)
                END
            ",
                parentGroupTypeGuid,
                childGroupTypeGuid
           ) );
        }
    }
}
