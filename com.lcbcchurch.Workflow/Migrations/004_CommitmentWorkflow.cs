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
namespace com.lcbcchurch.Workflow.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class CommitmentWorkflow : Migration
    {
        public override void Up()
        {
            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Web Forms", "fa fa-wpforms", "Forms that live externally on LCBCChurch.com", "2B52D211-E476-4592-9EDB-79A2EA622267", 0 ); // Web Forms

            #endregion

            #region Commitment Choice

            RockMigrationHelper.UpdateWorkflowType( false, true, "Commitment Choice", "", "2B52D211-E476-4592-9EDB-79A2EA622267", "Commitment", "fa fa-phone", 0, true, 0, "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", 0 ); // Commitment Choice
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person who made the request. This is only available if the user was logged into the website when they made the inquiry.", 0, @"", "34814E97-227A-4BCD-B628-3350FC0CB380", false ); // Commitment Choice:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Commitment", "Commitment", "", 1, @"", "422408DF-F0B1-46FD-9AE3-CF88E611398D", false ); // Commitment Choice:Commitment
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Info", "ContactInfo", "", 2, @"", "377E487E-D6A2-4D65-9F34-6980D6038100", false ); // Commitment Choice:Contact Info
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "The campus (if any) that this inquiry is related to", 3, @"", "CA74CF37-FB13-4A1C-9230-5BE05D229E1C", false ); // Commitment Choice:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person responsible to follow up on the inquiry", 4, @"", "1193D684-0C9E-4D86-9C23-0F9E4940A0E9", true ); // Commitment Choice:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Assign New Worker", "NewWorker", "If this inquiry needs to be re-assigned to a different person, select that person here.", 5, @"", "BC3CA97D-AD69-4368-92A8-A44C0554EA1E", false ); // Commitment Choice:Assign New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Notes", "Notes", "Staff notes about this inquiry", 6, @"", "14D38C07-101C-4B24-94B5-A31F990F0F2F", false ); // Commitment Choice:Notes
            RockMigrationHelper.AddAttributeQualifier( "34814E97-227A-4BCD-B628-3350FC0CB380", "EnableSelfSelection", @"False", "7CC2D344-DDDE-4AB5-985F-06214B8259F6" ); // Commitment Choice:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "422408DF-F0B1-46FD-9AE3-CF88E611398D", "fieldtype", @"rb", "69FCADA4-17A7-4572-A415-A85CAB539C44" ); // Commitment Choice:Commitment:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "422408DF-F0B1-46FD-9AE3-CF88E611398D", "values", @"Committed^I've made the decision to start trusting Jesus, Questioning^I still have questions about what it means to trust Jesus", "7811257B-62D3-43B4-B45E-C28DB3A29B83" ); // Commitment Choice:Commitment:values
            RockMigrationHelper.AddAttributeQualifier( "377E487E-D6A2-4D65-9F34-6980D6038100", "ispassword", @"False", "BC724604-9566-4931-81DC-75484AAEF915" ); // Commitment Choice:Contact Info:ispassword
            RockMigrationHelper.AddAttributeQualifier( "377E487E-D6A2-4D65-9F34-6980D6038100", "maxcharacters", @"", "1C087D70-0242-42B3-8CAF-84FA5438D67B" ); // Commitment Choice:Contact Info:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "377E487E-D6A2-4D65-9F34-6980D6038100", "showcountdown", @"False", "0986AA59-1A75-46C2-A99C-39D9BB7D2E58" ); // Commitment Choice:Contact Info:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", true, "Request", "Prompt the user for the information about their request", true, 0, "92EDD2DA-4C1A-4404-8FED-05192B06194D" ); // Commitment Choice:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", true, "Open", "Activity used to process the inquiry.", false, 1, "97845D92-C631-4CFA-BB7F-F611903EAFF5" ); // Commitment Choice:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", true, "Complete", "Complete the workflow", false, 2, "D6B82D91-5DA5-480D-80AA-8E4178767B83" ); // Commitment Choice:Complete
            RockMigrationHelper.UpdateWorkflowActivityType( "54FE40B1-3885-4E65-B15B-6E5C64AA59B9", true, "Re-assign Worker", "Assigns the inquiry to a new worker", false, 3, "C53DB987-31D5-40A8-9570-0301A83035FA" ); // Commitment Choice:Re-assign Worker
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow | Attribute:'Topic' }} Inquiry from {{ Workflow | Attribute:'FirstName' }} {{ Workflow | Attribute:'LastName' }}</h4> <p>The following inquiry has been submitted by a visitor to our website.</p>", @"", "Update^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^d6b82d91-5da5-480d-80aa-8e4178767b83^", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "656800C4-D478-4427-923F-583F093761D5" ); // Commitment Choice:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "34814E97-227A-4BCD-B628-3350FC0CB380", 0, true, true, false, false, @"", @"", "04F3B154-AEB4-458C-B0DD-64EBF61E1729" ); // Commitment Choice:Open:Capture Notes:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "422408DF-F0B1-46FD-9AE3-CF88E611398D", 1, true, true, false, false, @"", @"", "0FEFE9BE-F349-4BB3-90F6-FD78A1B7A197" ); // Commitment Choice:Open:Capture Notes:Commitment
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "377E487E-D6A2-4D65-9F34-6980D6038100", 6, false, true, false, false, @"", @"", "101FBEC5-58B0-46C7-8CA7-57AF4EF2BDCE" ); // Commitment Choice:Open:Capture Notes:Contact Info
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "CA74CF37-FB13-4A1C-9230-5BE05D229E1C", 2, true, true, false, false, @"", @"", "454A679B-5F50-4F44-B344-CAC2B2EEF709" ); // Commitment Choice:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "1193D684-0C9E-4D86-9C23-0F9E4940A0E9", 3, false, true, false, false, @"", @"", "2DF7504F-2A99-4E3A-AFC0-C388482AD377" ); // Commitment Choice:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "BC3CA97D-AD69-4368-92A8-A44C0554EA1E", 4, true, false, false, false, @"", @"", "165F9FCE-738A-44C4-9029-1A3B47CB98E9" ); // Commitment Choice:Open:Capture Notes:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "656800C4-D478-4427-923F-583F093761D5", "14D38C07-101C-4B24-94B5-A31F990F0F2F", 5, true, false, false, false, @"", @"", "B4D53987-997B-45D7-99B0-EB26BB913328" ); // Commitment Choice:Open:Capture Notes:Notes
            RockMigrationHelper.UpdateWorkflowActionType( "92EDD2DA-4C1A-4404-8FED-05192B06194D", "Set Name", 0, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "82D3B8DD-FCBD-4CE7-B7EE-2E929E18B024" ); // Commitment Choice:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "92EDD2DA-4C1A-4404-8FED-05192B06194D", "Assign Worker", 1, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, true, "", "", 8, "Serving", "6EE874F7-38C1-482F-B460-18E6970549C0" ); // Commitment Choice:Request:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "92EDD2DA-4C1A-4404-8FED-05192B06194D", "Open", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 8, "LifeGroups", "6DEEB608-0C28-4542-9C48-F78444971DD3" ); // Commitment Choice:Request:Open
            RockMigrationHelper.UpdateWorkflowActionType( "97845D92-C631-4CFA-BB7F-F611903EAFF5", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "C4D37429-58D9-495C-8739-428CB1EBAE6D" ); // Commitment Choice:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "97845D92-C631-4CFA-BB7F-F611903EAFF5", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "656800C4-D478-4427-923F-583F093761D5", "", 1, "", "DC8C2F6E-E4FF-41FE-8C40-793E0469033D" ); // Commitment Choice:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "97845D92-C631-4CFA-BB7F-F611903EAFF5", "Assign New Worker", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "BC3CA97D-AD69-4368-92A8-A44C0554EA1E", 64, "", "FA1CFCA0-8BD5-4D64-A5B6-D1FF00415A9B" ); // Commitment Choice:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "97845D92-C631-4CFA-BB7F-F611903EAFF5", "Re-Activate These Actions", 3, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "", 1, "", "719AA9E3-4E17-4A97-AB3E-0F657833C35B" ); // Commitment Choice:Open:Re-Activate These Actions
            RockMigrationHelper.UpdateWorkflowActionType( "D6B82D91-5DA5-480D-80AA-8E4178767B83", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "61C938CD-6BDB-43F6-893C-9901B2E4660D" ); // Commitment Choice:Complete:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "C53DB987-31D5-40A8-9570-0301A83035FA", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "10309548-F125-40EA-BEEF-C43AD742380F" ); // Commitment Choice:Re-assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "C53DB987-31D5-40A8-9570-0301A83035FA", "Clear New Worker Value", 1, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "2E61464E-89A7-474B-B5E4-9183F3F840CC" ); // Commitment Choice:Re-assign Worker:Clear New Worker Value
            RockMigrationHelper.UpdateWorkflowActionType( "C53DB987-31D5-40A8-9570-0301A83035FA", "Re-Activate Open Activity", 2, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "1B32077E-E022-439A-BFB7-1F5A6C5C2E77" ); // Commitment Choice:Re-assign Worker:Re-Activate Open Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "82D3B8DD-FCBD-4CE7-B7EE-2E929E18B024", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Commitment Choice:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "82D3B8DD-FCBD-4CE7-B7EE-2E929E18B024", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Commitment Choice:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "82D3B8DD-FCBD-4CE7-B7EE-2E929E18B024", "93852244-A667-4749-961A-D47F88675BE4", @"{% assign person = Workflow | Attribute:'Person','Object'%}{{person.FullName}} ( {{ Workflow | Attribute:'Interests' }} )" ); // Commitment Choice:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "6EE874F7-38C1-482F-B460-18E6970549C0", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Commitment Choice:Request:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6EE874F7-38C1-482F-B460-18E6970549C0", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1193d684-0c9e-4d86-9c23-0f9e4940a0e9" ); // Commitment Choice:Request:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6EE874F7-38C1-482F-B460-18E6970549C0", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Commitment Choice:Request:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "6EE874F7-38C1-482F-B460-18E6970549C0", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"36c2aba2-27e5-4652-9c96-ce504139e8f6" ); // Commitment Choice:Request:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "6DEEB608-0C28-4542-9C48-F78444971DD3", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Commitment Choice:Request:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6DEEB608-0C28-4542-9C48-F78444971DD3", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Commitment Choice:Request:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6DEEB608-0C28-4542-9C48-F78444971DD3", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"97845D92-C631-4CFA-BB7F-F611903EAFF5" ); // Commitment Choice:Request:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C4D37429-58D9-495C-8739-428CB1EBAE6D", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Commitment Choice:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C4D37429-58D9-495C-8739-428CB1EBAE6D", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"1193d684-0c9e-4d86-9c23-0f9e4940a0e9" ); // Commitment Choice:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C4D37429-58D9-495C-8739-428CB1EBAE6D", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Commitment Choice:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "DC8C2F6E-E4FF-41FE-8C40-793E0469033D", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Commitment Choice:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DC8C2F6E-E4FF-41FE-8C40-793E0469033D", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Commitment Choice:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "FA1CFCA0-8BD5-4D64-A5B6-D1FF00415A9B", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Commitment Choice:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FA1CFCA0-8BD5-4D64-A5B6-D1FF00415A9B", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Commitment Choice:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "FA1CFCA0-8BD5-4D64-A5B6-D1FF00415A9B", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"C53DB987-31D5-40A8-9570-0301A83035FA" ); // Commitment Choice:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "719AA9E3-4E17-4A97-AB3E-0F657833C35B", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // Commitment Choice:Open:Re-Activate These Actions:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "719AA9E3-4E17-4A97-AB3E-0F657833C35B", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // Commitment Choice:Open:Re-Activate These Actions:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "61C938CD-6BDB-43F6-893C-9901B2E4660D", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Commitment Choice:Complete:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "61C938CD-6BDB-43F6-893C-9901B2E4660D", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Commitment Choice:Complete:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "61C938CD-6BDB-43F6-893C-9901B2E4660D", "07CB7DBC-236D-4D38-92A4-47EE448BA89A", @"Completed" ); // Commitment Choice:Complete:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "10309548-F125-40EA-BEEF-C43AD742380F", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Commitment Choice:Re-assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "10309548-F125-40EA-BEEF-C43AD742380F", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"1193d684-0c9e-4d86-9c23-0f9e4940a0e9" ); // Commitment Choice:Re-assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "10309548-F125-40EA-BEEF-C43AD742380F", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Commitment Choice:Re-assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "10309548-F125-40EA-BEEF-C43AD742380F", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"bc3ca97d-ad69-4368-92a8-a44c0554ea1e" ); // Commitment Choice:Re-assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "2E61464E-89A7-474B-B5E4-9183F3F840CC", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Commitment Choice:Re-assign Worker:Clear New Worker Value:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2E61464E-89A7-474B-B5E4-9183F3F840CC", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"bc3ca97d-ad69-4368-92a8-a44c0554ea1e" ); // Commitment Choice:Re-assign Worker:Clear New Worker Value:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2E61464E-89A7-474B-B5E4-9183F3F840CC", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Commitment Choice:Re-assign Worker:Clear New Worker Value:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "2E61464E-89A7-474B-B5E4-9183F3F840CC", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"" ); // Commitment Choice:Re-assign Worker:Clear New Worker Value:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "1B32077E-E022-439A-BFB7-1F5A6C5C2E77", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Commitment Choice:Re-assign Worker:Re-Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1B32077E-E022-439A-BFB7-1F5A6C5C2E77", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Commitment Choice:Re-assign Worker:Re-Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1B32077E-E022-439A-BFB7-1F5A6C5C2E77", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"97845D92-C631-4CFA-BB7F-F611903EAFF5" ); // Commitment Choice:Re-assign Worker:Re-Activate Open Activity:Activity

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"     UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )     FROM [AttributeQualifier] [aq]     INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]     INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]     INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]     WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'     AND [aq].[key] = 'definedtypeguid'    " );

            #endregion
        }
        public override void Down()
        {

        }
    }
}
