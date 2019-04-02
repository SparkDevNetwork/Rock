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
    [MigrationNumber( 3, "1.0.14" )]
    public class InterestWorkflow : Migration
    {
        public override void Up()
        {
            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Web Forms", "fa fa-wpforms", "Forms that live externally on LCBCChurch.com", "2B52D211-E476-4592-9EDB-79A2EA622267", 0 ); // Web Forms

            #endregion

            #region Ministry Interest

            RockMigrationHelper.UpdateWorkflowType( false, true, "Ministry Interest", "", "2B52D211-E476-4592-9EDB-79A2EA622267", "Interest", "fa fa-phone", 0, true, 0, "63B0641B-346B-4774-AE4A-B3F49FBD7708", 0 ); // Ministry Interest
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person who made the request. This is only available if the user was logged into the website when they made the inquiry.", 0, @"", "E33F3584-79CF-485F-9F4F-CBF73CEADB21", false ); // Ministry Interest:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Interests", "Interests", "", 1, @"", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", false ); // Ministry Interest:Interests
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "The campus (if any) that this inquiry is related to", 2, @"", "F1F1A311-0486-4177-8090-D55630E78AA8", false ); // Ministry Interest:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person responsible to follow up on the inquiry", 3, @"", "1D733975-C1BC-4D07-8591-6E87F1630EA1", true ); // Ministry Interest:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Assign New Worker", "NewWorker", "If this inquiry needs to be re-assigned to a different person, select that person here.", 4, @"", "3951D068-2474-4FD4-B036-FD08FDEDB2D7", false ); // Ministry Interest:Assign New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Notes", "Notes", "Staff notes about this inquiry", 5, @"", "B6993F30-EB7D-4E00-A6C7-5EDD387D2C8B", false ); // Ministry Interest:Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63B0641B-346B-4774-AE4A-B3F49FBD7708", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Worker Group", "WorkerGroup", "", 6, @"5579B44A-AA02-4E9B-BAB7-29E702E773BF", "AC59FA19-6EB3-42C6-B96F-9C4C77C37C86", false ); // Ministry Interest:Worker Group
            RockMigrationHelper.AddAttributeQualifier( "E33F3584-79CF-485F-9F4F-CBF73CEADB21", "EnableSelfSelection", @"False", "6DA747AF-1D99-47D4-8F97-8A54FF161510" ); // Ministry Interest:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", "enhancedselection", @"False", "4B644AAA-CEA5-4092-8CA2-EBF000635073" ); // Ministry Interest:Interests:enhancedselection
            RockMigrationHelper.AddAttributeQualifier( "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", "values", @"FirstSteps^First Steps, Journey^Growth Track: Journey, Discovery^Growth Track: Discovery, Story^Growth Track: Story, Baptism^Baptism, Serving^Serving, LifeGroups^LIFE Groups", "99631A9C-F4C0-4576-A8FF-564B7DB9138F" ); // Ministry Interest:Interests:values
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Request", "Prompt the user for the information about their request", true, 0, "7637F475-7693-44B4-9FCF-3CF73EAD790B" ); // Ministry Interest:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "First Steps", "Assign the worker to the person responsible for 'First Steps' topic inquiries.", false, 1, "81A9F9EA-EA69-4D95-AA26-CA5CE32A9DEB" ); // Ministry Interest:First Steps
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Growth Track: Journey", "Assign the worker to the person responsible for 'Journey' topic inquiries", false, 2, "03F13140-0DCE-4D67-AE63-64C591910CE3" ); // Ministry Interest:Growth Track: Journey
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Growth Track: Bible Discovery", "Assign the worker to the person responsible for 'Bible Discovery' topic inquiries", false, 3, "2D1B295A-7F1A-424E-B84E-1DEF1A67CC39" ); // Ministry Interest:Growth Track: Bible Discovery
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Growth Track: Story", "Assign the worker to the person responsible for 'Story' topic inquiries", false, 4, "E5EA075C-1C8C-412B-BEAC-B96AB12CC43E" ); // Ministry Interest:Growth Track: Story
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Baptism", "Assign the worker to the person responsible for 'Baptism' topic inquiries", false, 5, "1136AECD-8723-41F2-AD61-00A056BEB557" ); // Ministry Interest:Baptism
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Serving", "Assign the worker to the person responsible for 'Serving' topic inquiries", false, 6, "BCFB4A2A-1E0E-43B7-8461-293C74F255C9" ); // Ministry Interest:Serving
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "LIFE Groups", "Assign the worker to the person responsible for 'LIFE Groups' topic inquiries", false, 7, "47C4E4DA-B1CD-47F6-9FFD-0207BECA7ECD" ); // Ministry Interest:LIFE Groups
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Open", "Activity used to process the inquiry.", false, 8, "53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Complete", "Complete the workflow", false, 9, "C6A50AE8-DC47-4DE6-9578-BDCC92C5578C" ); // Ministry Interest:Complete
            RockMigrationHelper.UpdateWorkflowActivityType( "63B0641B-346B-4774-AE4A-B3F49FBD7708", true, "Re-assign Worker", "Assigns the inquiry to a new worker", false, 10, "8A232D27-3564-4B07-9FF8-D2C01484AB53" ); // Ministry Interest:Re-assign Worker
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow | Attribute:'Topic' }} Inquiry from {{ Workflow | Attribute:'FirstName' }} {{ Workflow | Attribute:'LastName' }}</h4> <p>The following inquiry has been submitted by a visitor to our website.</p>", @"", "Update^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^c6a50ae8-dc47-4de6-9578-bdcc92c5578c^", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "27F85727-519A-479A-9C87-B2F0289BDEAD" ); // Ministry Interest:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "E33F3584-79CF-485F-9F4F-CBF73CEADB21", 0, true, true, false, false, @"", @"", "DDB7AA18-2ACA-4017-A5C0-1507C0502CE3" ); // Ministry Interest:Open:Capture Notes:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 1, true, true, false, false, @"", @"", "395227E2-91B4-498A-A03F-0776924CBEFF" ); // Ministry Interest:Open:Capture Notes:Interests
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "F1F1A311-0486-4177-8090-D55630E78AA8", 2, true, true, false, false, @"", @"", "7F4DCF92-A820-4BF5-850F-01708E084684" ); // Ministry Interest:Open:Capture Notes:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "1D733975-C1BC-4D07-8591-6E87F1630EA1", 3, false, true, false, false, @"", @"", "0D460916-E063-4E38-B4E8-E12C4E277748" ); // Ministry Interest:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "3951D068-2474-4FD4-B036-FD08FDEDB2D7", 4, true, false, false, false, @"", @"", "61D9BA0D-EFB7-41D2-80AB-70C6ECE2675F" ); // Ministry Interest:Open:Capture Notes:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "B6993F30-EB7D-4E00-A6C7-5EDD387D2C8B", 5, true, false, false, false, @"", @"", "86650AFD-2AD2-430D-AAB5-5209D5CBD864" ); // Ministry Interest:Open:Capture Notes:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "27F85727-519A-479A-9C87-B2F0289BDEAD", "AC59FA19-6EB3-42C6-B96F-9C4C77C37C86", 6, false, true, false, false, @"", @"", "D834996D-AB71-4DC2-87C3-BDFB3A468322" ); // Ministry Interest:Open:Capture Notes:Worker Group
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Set Name", 0, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "A8B0A1E2-B505-4811-A509-2C5BE7712873" ); // Ministry Interest:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Assign Initial Worker", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "D5182816-1949-4BD0-A3BE-51A8A568EDBB" ); // Ministry Interest:Request:Assign Initial Worker
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Activate Open Activity", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 1, "", "7828B386-7F8F-42C6-9F2C-BC1754F1EA87" ); // Ministry Interest:Request:Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for First Steps", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "FirstSteps", "508BC653-3A91-4CDC-84CC-63F0BA55FB65" ); // Ministry Interest:Request:Test for First Steps
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for Journey", 4, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "Journey", "C3179102-C552-4CAB-B188-3D0F47EEC134" ); // Ministry Interest:Request:Test for Journey
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for Discovery", 5, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "Discovery", "5E289A4F-B85E-47BA-A5B5-32FDF45723DD" ); // Ministry Interest:Request:Test for Discovery
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for Story", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "Story", "98D8D786-5C98-4A3B-997E-39A4A3AD0838" ); // Ministry Interest:Request:Test for Story
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for Baptism", 7, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "Baptism", "C42552AC-903C-443F-A558-547BFB111E21" ); // Ministry Interest:Request:Test for Baptism
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for Serving", 8, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "Serving", "7EB5DF56-FFA7-4DC8-9F52-50AA8BB2FD55" ); // Ministry Interest:Request:Test for Serving
            RockMigrationHelper.UpdateWorkflowActionType( "7637F475-7693-44B4-9FCF-3CF73EAD790B", "Test for LIFE Groups", 9, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "7023A115-4AEC-4C34-8CEA-0E0C69149FCF", 8, "LifeGroups", "D76DAFC6-710F-463A-9A96-E8877C5EA7B3" ); // Ministry Interest:Request:Test for LIFE Groups
            RockMigrationHelper.UpdateWorkflowActionType( "81A9F9EA-EA69-4D95-AA26-CA5CE32A9DEB", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "AB0609C5-3D7D-470A-921D-07FFA66D8D38" ); // Ministry Interest:First Steps:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "81A9F9EA-EA69-4D95-AA26-CA5CE32A9DEB", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "093F0BBD-24B2-4BD1-B597-1E8E18E3326E" ); // Ministry Interest:First Steps:Open
            RockMigrationHelper.UpdateWorkflowActionType( "03F13140-0DCE-4D67-AE63-64C591910CE3", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "2156B090-E72C-48D7-B262-4BCCDC29F07B" ); // Ministry Interest:Growth Track: Journey:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "03F13140-0DCE-4D67-AE63-64C591910CE3", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "F0CB1F52-08F7-4BE4-AE99-A3920ED32870" ); // Ministry Interest:Growth Track: Journey:Open
            RockMigrationHelper.UpdateWorkflowActionType( "2D1B295A-7F1A-424E-B84E-1DEF1A67CC39", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "3BCA3F89-4D34-4682-B494-AA6361BF5DF8" ); // Ministry Interest:Growth Track: Bible Discovery:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "2D1B295A-7F1A-424E-B84E-1DEF1A67CC39", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "0D74CE72-3F97-4078-B6DB-0F14DE023407" ); // Ministry Interest:Growth Track: Bible Discovery:Open
            RockMigrationHelper.UpdateWorkflowActionType( "E5EA075C-1C8C-412B-BEAC-B96AB12CC43E", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "0B875C73-729F-40E2-BFA6-77555A73E11A" ); // Ministry Interest:Growth Track: Story:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "E5EA075C-1C8C-412B-BEAC-B96AB12CC43E", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "C8D62F92-AA79-412F-A6D0-F323EE5C8A60" ); // Ministry Interest:Growth Track: Story:Open
            RockMigrationHelper.UpdateWorkflowActionType( "1136AECD-8723-41F2-AD61-00A056BEB557", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "FBA8D1C3-C899-483C-98CB-0DB618A9B171" ); // Ministry Interest:Baptism:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "1136AECD-8723-41F2-AD61-00A056BEB557", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "A5F092E8-8F6C-409A-967B-608E1CE333D8" ); // Ministry Interest:Baptism:Open
            RockMigrationHelper.UpdateWorkflowActionType( "BCFB4A2A-1E0E-43B7-8461-293C74F255C9", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "A2CE1D4F-A398-4D13-9E1E-84741A54390A" ); // Ministry Interest:Serving:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "BCFB4A2A-1E0E-43B7-8461-293C74F255C9", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "46C1457E-52B7-4A8E-BFCA-D8779B0D26B8" ); // Ministry Interest:Serving:Open
            RockMigrationHelper.UpdateWorkflowActionType( "47C4E4DA-B1CD-47F6-9FFD-0207BECA7ECD", "Assign Worker", 0, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "DED53947-A2A3-4071-B160-C96A0F3B3E8C" ); // Ministry Interest:LIFE Groups:Assign Worker
            RockMigrationHelper.UpdateWorkflowActionType( "47C4E4DA-B1CD-47F6-9FFD-0207BECA7ECD", "Open", 1, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "6AF98D98-329C-4F54-A8D4-D9A783C1C4AC" ); // Ministry Interest:LIFE Groups:Open
            RockMigrationHelper.UpdateWorkflowActionType( "53186CE1-9A4F-41E2-84E6-DFA8C51BC580", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "F955F606-44DC-4E62-A8EC-24587E52059E" ); // Ministry Interest:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "53186CE1-9A4F-41E2-84E6-DFA8C51BC580", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "27F85727-519A-479A-9C87-B2F0289BDEAD", "", 1, "", "7BAE7B65-78AB-4CEA-8A8B-231CBE8F22F5" ); // Ministry Interest:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "53186CE1-9A4F-41E2-84E6-DFA8C51BC580", "Assign New Worker", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "3951D068-2474-4FD4-B036-FD08FDEDB2D7", 64, "", "A8B79A4D-4CCE-4F7F-8298-B9B251630734" ); // Ministry Interest:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "53186CE1-9A4F-41E2-84E6-DFA8C51BC580", "Re-Activate These Actions", 3, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "", 1, "", "39B3300E-FA40-419C-AAA4-53E3E2DB37B9" ); // Ministry Interest:Open:Re-Activate These Actions
            RockMigrationHelper.UpdateWorkflowActionType( "C6A50AE8-DC47-4DE6-9578-BDCC92C5578C", "Complete Workflow", 0, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "16A4B37A-701D-4EDD-8851-C9C1534E367A" ); // Ministry Interest:Complete:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "8A232D27-3564-4B07-9FF8-D2C01484AB53", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "9BBCAE00-8147-420F-B493-2ADC5AEA9941" ); // Ministry Interest:Re-assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "8A232D27-3564-4B07-9FF8-D2C01484AB53", "Clear New Worker Value", 1, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "1AE534F1-BB31-403F-8A94-1CD8683336CB" ); // Ministry Interest:Re-assign Worker:Clear New Worker Value
            RockMigrationHelper.UpdateWorkflowActionType( "8A232D27-3564-4B07-9FF8-D2C01484AB53", "Re-Activate Open Activity", 2, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "1B61E486-1D21-4F22-A08B-82FC69256DC8" ); // Ministry Interest:Re-assign Worker:Re-Activate Open Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B0A1E2-B505-4811-A509-2C5BE7712873", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Ministry Interest:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B0A1E2-B505-4811-A509-2C5BE7712873", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Ministry Interest:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B0A1E2-B505-4811-A509-2C5BE7712873", "93852244-A667-4749-961A-D47F88675BE4", @"{% assign person = Workflow | Attribute:'Person','Object'%}{{person.FullName}} ( {{ Workflow | Attribute:'Interests' }} )" ); // Ministry Interest:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "D5182816-1949-4BD0-A3BE-51A8A568EDBB", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"
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
{{personGuid | Trim }}" ); // Ministry Interest:Request:Assign Initial Worker:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "D5182816-1949-4BD0-A3BE-51A8A568EDBB", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Ministry Interest:Request:Assign Initial Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D5182816-1949-4BD0-A3BE-51A8A568EDBB", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Request:Assign Initial Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D5182816-1949-4BD0-A3BE-51A8A568EDBB", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // Ministry Interest:Request:Assign Initial Worker:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "7828B386-7F8F-42C6-9F2C-BC1754F1EA87", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7828B386-7F8F-42C6-9F2C-BC1754F1EA87", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Request:Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "508BC653-3A91-4CDC-84CC-63F0BA55FB65", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for First Steps:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "508BC653-3A91-4CDC-84CC-63F0BA55FB65", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for First Steps:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "508BC653-3A91-4CDC-84CC-63F0BA55FB65", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"81A9F9EA-EA69-4D95-AA26-CA5CE32A9DEB" ); // Ministry Interest:Request:Test for First Steps:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C3179102-C552-4CAB-B188-3D0F47EEC134", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for Journey:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C3179102-C552-4CAB-B188-3D0F47EEC134", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for Journey:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C3179102-C552-4CAB-B188-3D0F47EEC134", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"03F13140-0DCE-4D67-AE63-64C591910CE3" ); // Ministry Interest:Request:Test for Journey:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "5E289A4F-B85E-47BA-A5B5-32FDF45723DD", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for Discovery:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5E289A4F-B85E-47BA-A5B5-32FDF45723DD", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for Discovery:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5E289A4F-B85E-47BA-A5B5-32FDF45723DD", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2D1B295A-7F1A-424E-B84E-1DEF1A67CC39" ); // Ministry Interest:Request:Test for Discovery:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "98D8D786-5C98-4A3B-997E-39A4A3AD0838", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for Story:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "98D8D786-5C98-4A3B-997E-39A4A3AD0838", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for Story:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "98D8D786-5C98-4A3B-997E-39A4A3AD0838", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"E5EA075C-1C8C-412B-BEAC-B96AB12CC43E" ); // Ministry Interest:Request:Test for Story:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C42552AC-903C-443F-A558-547BFB111E21", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for Baptism:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C42552AC-903C-443F-A558-547BFB111E21", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for Baptism:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C42552AC-903C-443F-A558-547BFB111E21", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"1136AECD-8723-41F2-AD61-00A056BEB557" ); // Ministry Interest:Request:Test for Baptism:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "7EB5DF56-FFA7-4DC8-9F52-50AA8BB2FD55", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for Serving:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7EB5DF56-FFA7-4DC8-9F52-50AA8BB2FD55", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for Serving:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7EB5DF56-FFA7-4DC8-9F52-50AA8BB2FD55", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BCFB4A2A-1E0E-43B7-8461-293C74F255C9" ); // Ministry Interest:Request:Test for Serving:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "D76DAFC6-710F-463A-9A96-E8877C5EA7B3", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Request:Test for LIFE Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D76DAFC6-710F-463A-9A96-E8877C5EA7B3", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Request:Test for LIFE Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D76DAFC6-710F-463A-9A96-E8877C5EA7B3", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"47C4E4DA-B1CD-47F6-9FFD-0207BECA7ECD" ); // Ministry Interest:Request:Test for LIFE Groups:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "AB0609C5-3D7D-470A-921D-07FFA66D8D38", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:First Steps:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AB0609C5-3D7D-470A-921D-07FFA66D8D38", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:First Steps:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AB0609C5-3D7D-470A-921D-07FFA66D8D38", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:First Steps:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "AB0609C5-3D7D-470A-921D-07FFA66D8D38", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:First Steps:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "093F0BBD-24B2-4BD1-B597-1E8E18E3326E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:First Steps:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "093F0BBD-24B2-4BD1-B597-1E8E18E3326E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:First Steps:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "093F0BBD-24B2-4BD1-B597-1E8E18E3326E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:First Steps:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "2156B090-E72C-48D7-B262-4BCCDC29F07B", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Growth Track: Journey:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2156B090-E72C-48D7-B262-4BCCDC29F07B", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Growth Track: Journey:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "2156B090-E72C-48D7-B262-4BCCDC29F07B", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Growth Track: Journey:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "2156B090-E72C-48D7-B262-4BCCDC29F07B", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:Growth Track: Journey:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "F0CB1F52-08F7-4BE4-AE99-A3920ED32870", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Growth Track: Journey:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F0CB1F52-08F7-4BE4-AE99-A3920ED32870", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Growth Track: Journey:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F0CB1F52-08F7-4BE4-AE99-A3920ED32870", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BCFB4A2A-1E0E-43B7-8461-293C74F255C9" ); // Ministry Interest:Growth Track: Journey:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3BCA3F89-4D34-4682-B494-AA6361BF5DF8", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Growth Track: Bible Discovery:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3BCA3F89-4D34-4682-B494-AA6361BF5DF8", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Growth Track: Bible Discovery:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3BCA3F89-4D34-4682-B494-AA6361BF5DF8", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Growth Track: Bible Discovery:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "3BCA3F89-4D34-4682-B494-AA6361BF5DF8", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:Growth Track: Bible Discovery:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "0D74CE72-3F97-4078-B6DB-0F14DE023407", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Growth Track: Bible Discovery:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0D74CE72-3F97-4078-B6DB-0F14DE023407", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Growth Track: Bible Discovery:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0D74CE72-3F97-4078-B6DB-0F14DE023407", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Growth Track: Bible Discovery:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "0B875C73-729F-40E2-BFA6-77555A73E11A", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Growth Track: Story:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0B875C73-729F-40E2-BFA6-77555A73E11A", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Growth Track: Story:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0B875C73-729F-40E2-BFA6-77555A73E11A", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Growth Track: Story:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "0B875C73-729F-40E2-BFA6-77555A73E11A", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:Growth Track: Story:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "C8D62F92-AA79-412F-A6D0-F323EE5C8A60", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Growth Track: Story:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C8D62F92-AA79-412F-A6D0-F323EE5C8A60", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Growth Track: Story:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C8D62F92-AA79-412F-A6D0-F323EE5C8A60", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Growth Track: Story:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "FBA8D1C3-C899-483C-98CB-0DB618A9B171", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Baptism:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FBA8D1C3-C899-483C-98CB-0DB618A9B171", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Baptism:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FBA8D1C3-C899-483C-98CB-0DB618A9B171", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Baptism:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "FBA8D1C3-C899-483C-98CB-0DB618A9B171", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:Baptism:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F092E8-8F6C-409A-967B-608E1CE333D8", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Baptism:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F092E8-8F6C-409A-967B-608E1CE333D8", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Baptism:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F092E8-8F6C-409A-967B-608E1CE333D8", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Baptism:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "A2CE1D4F-A398-4D13-9E1E-84741A54390A", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Serving:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A2CE1D4F-A398-4D13-9E1E-84741A54390A", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Serving:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A2CE1D4F-A398-4D13-9E1E-84741A54390A", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Serving:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "A2CE1D4F-A398-4D13-9E1E-84741A54390A", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:Serving:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "46C1457E-52B7-4A8E-BFCA-D8779B0D26B8", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Serving:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "46C1457E-52B7-4A8E-BFCA-D8779B0D26B8", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Serving:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "46C1457E-52B7-4A8E-BFCA-D8779B0D26B8", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Serving:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "DED53947-A2A3-4071-B160-C96A0F3B3E8C", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:LIFE Groups:Assign Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "DED53947-A2A3-4071-B160-C96A0F3B3E8C", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:LIFE Groups:Assign Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "DED53947-A2A3-4071-B160-C96A0F3B3E8C", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:LIFE Groups:Assign Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "DED53947-A2A3-4071-B160-C96A0F3B3E8C", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"11b2869b-2420-4f16-8262-14b2f5c0dd1c" ); // Ministry Interest:LIFE Groups:Assign Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF98D98-329C-4F54-A8D4-D9A783C1C4AC", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:LIFE Groups:Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF98D98-329C-4F54-A8D4-D9A783C1C4AC", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:LIFE Groups:Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6AF98D98-329C-4F54-A8D4-D9A783C1C4AC", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:LIFE Groups:Open:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "F955F606-44DC-4E62-A8EC-24587E52059E", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Ministry Interest:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F955F606-44DC-4E62-A8EC-24587E52059E", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F955F606-44DC-4E62-A8EC-24587E52059E", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Ministry Interest:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7BAE7B65-78AB-4CEA-8A8B-231CBE8F22F5", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Ministry Interest:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7BAE7B65-78AB-4CEA-8A8B-231CBE8F22F5", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Ministry Interest:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B79A4D-4CCE-4F7F-8298-B9B251630734", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B79A4D-4CCE-4F7F-8298-B9B251630734", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A8B79A4D-4CCE-4F7F-8298-B9B251630734", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"8A232D27-3564-4B07-9FF8-D2C01484AB53" ); // Ministry Interest:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "39B3300E-FA40-419C-AAA4-53E3E2DB37B9", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // Ministry Interest:Open:Re-Activate These Actions:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "39B3300E-FA40-419C-AAA4-53E3E2DB37B9", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // Ministry Interest:Open:Re-Activate These Actions:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "16A4B37A-701D-4EDD-8851-C9C1534E367A", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Ministry Interest:Complete:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "16A4B37A-701D-4EDD-8851-C9C1534E367A", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Ministry Interest:Complete:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "16A4B37A-701D-4EDD-8851-C9C1534E367A", "07CB7DBC-236D-4D38-92A4-47EE448BA89A", @"Completed" ); // Ministry Interest:Complete:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9BBCAE00-8147-420F-B493-2ADC5AEA9941", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Ministry Interest:Re-assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9BBCAE00-8147-420F-B493-2ADC5AEA9941", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"1d733975-c1bc-4d07-8591-6e87f1630ea1" ); // Ministry Interest:Re-assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9BBCAE00-8147-420F-B493-2ADC5AEA9941", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Ministry Interest:Re-assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9BBCAE00-8147-420F-B493-2ADC5AEA9941", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"3951d068-2474-4fd4-b036-fd08fdedb2d7" ); // Ministry Interest:Re-assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1AE534F1-BB31-403F-8A94-1CD8683336CB", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Ministry Interest:Re-assign Worker:Clear New Worker Value:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1AE534F1-BB31-403F-8A94-1CD8683336CB", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"3951d068-2474-4fd4-b036-fd08fdedb2d7" ); // Ministry Interest:Re-assign Worker:Clear New Worker Value:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "1AE534F1-BB31-403F-8A94-1CD8683336CB", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Ministry Interest:Re-assign Worker:Clear New Worker Value:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "1AE534F1-BB31-403F-8A94-1CD8683336CB", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"" ); // Ministry Interest:Re-assign Worker:Clear New Worker Value:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "1B61E486-1D21-4F22-A08B-82FC69256DC8", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Ministry Interest:Re-assign Worker:Re-Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1B61E486-1D21-4F22-A08B-82FC69256DC8", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Ministry Interest:Re-assign Worker:Re-Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1B61E486-1D21-4F22-A08B-82FC69256DC8", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"53186CE1-9A4F-41E2-84E6-DFA8C51BC580" ); // Ministry Interest:Re-assign Worker:Re-Activate Open Activity:Activity

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
FROM [AttributeQualifier] [aq]
INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
AND [aq].[key] = 'definedtypeguid'" );

            #endregion
        }
        public override void Down()
        {

        }
    }
}
