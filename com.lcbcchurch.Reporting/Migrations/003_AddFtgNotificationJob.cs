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
namespace com.lcbcchurch.Reporting.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class AddFtgNotificationJob : Migration
    {
        public override void Up()
        {
            #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
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
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Samples", "fa fa-star", "", "CB99421E-9ADC-488E-8C71-94BB14F27F56", 0 ); // Samples

            #endregion

            #region First Time Guest Notification Workflow

            RockMigrationHelper.UpdateWorkflowType( false, true, "First Time Guest Notification Workflow", "", "CB99421E-9ADC-488E-8C71-94BB14F27F56", "Work", "fa fa-list-ol", 28800, true, 0, "F9AF3CF6-B7B5-4EB3-800C-B1D88E65BE1E", 0 ); // First Time Guest Notification Workflow
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F9AF3CF6-B7B5-4EB3-800C-B1D88E65BE1E", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDate", "", 0, @"", "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", false ); // First Time Guest Notification Workflow:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F9AF3CF6-B7B5-4EB3-800C-B1D88E65BE1E", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDate", "", 1, @"", "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", false ); // First Time Guest Notification Workflow:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F9AF3CF6-B7B5-4EB3-800C-B1D88E65BE1E", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Admin Group", "AdminGroup", "", 2, @"", "E1D7119A-FA6A-497D-A5DC-E33F7B332DB9", false ); // First Time Guest Notification Workflow:Admin Group
            RockMigrationHelper.AddAttributeQualifier( "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", "datePickerControlType", @"Date Picker", "FABA3BB1-6810-4A58-954A-0F148F6987FB" ); // First Time Guest Notification Workflow:Start Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", "displayCurrentOption", @"False", "E9D70651-1F96-4A3E-A6B7-5382CC1EE376" ); // First Time Guest Notification Workflow:Start Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", "displayDiff", @"False", "EEE39C94-EB08-47FA-A771-82480B0A30C2" ); // First Time Guest Notification Workflow:Start Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", "format", @"", "3791E520-73BC-4C76-8114-755B6300D648" ); // First Time Guest Notification Workflow:Start Date:format
            RockMigrationHelper.AddAttributeQualifier( "ABD841F4-BA92-4F38-9492-E9A506CE9E6E", "futureYearCount", @"", "D3074618-B814-4BE7-85F0-FBB0C8A85CAD" ); // First Time Guest Notification Workflow:Start Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", "datePickerControlType", @"Date Picker", "4ABF1DCC-D6D0-487F-8FC2-64C816115767" ); // First Time Guest Notification Workflow:End Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", "displayCurrentOption", @"False", "DD6ED3B6-82F5-4078-A926-94E5E8CD1E8F" ); // First Time Guest Notification Workflow:End Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", "displayDiff", @"False", "DB6A4414-23AE-45F6-8A48-0812899D142D" ); // First Time Guest Notification Workflow:End Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", "format", @"", "9A39E6FE-9188-4C7A-890D-F13287368323" ); // First Time Guest Notification Workflow:End Date:format
            RockMigrationHelper.AddAttributeQualifier( "9FC6E8B1-7CC7-4E1F-9C30-626E5BF35AB6", "futureYearCount", @"", "D62EC907-D312-4181-81A5-04FE247CF592" ); // First Time Guest Notification Workflow:End Date:futureYearCount
            RockMigrationHelper.UpdateWorkflowActivityType( "F9AF3CF6-B7B5-4EB3-800C-B1D88E65BE1E", true, "Start", "", true, 0, "246CC179-1FBA-4A16-8992-341812ADF2BE" ); // First Time Guest Notification Workflow:Start
            RockMigrationHelper.UpdateWorkflowActionType( "246CC179-1FBA-4A16-8992-341812ADF2BE", "Set End Date", 0, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "6BB93175-7B60-4406-9BE9-1D84D35E2E88" ); // First Time Guest Notification Workflow:Start:Set End Date
            RockMigrationHelper.UpdateWorkflowActionType( "246CC179-1FBA-4A16-8992-341812ADF2BE", "Set Start Date", 1, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "065DE1CB-ECBE-4D63-96B4-D9F6D5D1F32E" ); // First Time Guest Notification Workflow:Start:Set Start Date
            RockMigrationHelper.UpdateWorkflowActionType( "246CC179-1FBA-4A16-8992-341812ADF2BE", "Email Link to Form", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "17100485-D8B0-418B-B58E-189AE881E68A" ); // First Time Guest Notification Workflow:Start:Email Link to Form
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "F3B9908B-096F-460B-8320-122CF046D1F9", @"Select DateAdd(day, 1, [dbo].ufnUtility_GetPreviousSundayDate())" ); // First Time Guest Notification Workflow:Start:Set End Date:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // First Time Guest Notification Workflow:Start:Set End Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // First Time Guest Notification Workflow:Start:Set End Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5", @"" ); // First Time Guest Notification Workflow:Start:Set End Date:Parameters
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "56997192-2545-4EA1-B5B2-313B04588984", @"9fc6e8b1-7cc7-4e1f-9c30-626e5bf35ab6" ); // First Time Guest Notification Workflow:Start:Set End Date:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6BB93175-7B60-4406-9BE9-1D84D35E2E88", "423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC", @"False" ); // First Time Guest Notification Workflow:Start:Set End Date:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "065DE1CB-ECBE-4D63-96B4-D9F6D5D1F32E", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // First Time Guest Notification Workflow:Start:Set Start Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "065DE1CB-ECBE-4D63-96B4-D9F6D5D1F32E", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"abd841f4-ba92-4f38-9492-e9a506ce9e6e" ); // First Time Guest Notification Workflow:Start:Set Start Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "065DE1CB-ECBE-4D63-96B4-D9F6D5D1F32E", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // First Time Guest Notification Workflow:Start:Set Start Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "065DE1CB-ECBE-4D63-96B4-D9F6D5D1F32E", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{{ Workflow | Attribute:'EndDate','Object' | DateAdd:-7 }}" ); // First Time Guest Notification Workflow:Start:Set Start Date:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "0C4C13B8-7076-4872-925A-F950886B5E16", @"e1d7119a-fa6a-497d-a5dc-e33f7b332db9" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Send to Group Role
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"New First Time Guest Report" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Subject

            // Need to query for Assimiltation Report Page ID since it won't be the same across Rock instances
            string assimilationReportPageId = SqlScalar( "SELECT TOP 1 P.Id FROM [Page] P WHERE P.[Guid] = '82A4A145-4E72-41DA-AA05-E4B31A3290FF'" ).ToString().Trim();
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture dateRange %}{{Workflow | Attribute:'StartDate'}},{{Workflow | Attribute:'EndDate'}}{% endcapture %}
<p>{{ Person.FirstName }},</p>
<p>A new First Time Guest report has been generated:<p>
<p>
    <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}/page/" + assimilationReportPageId + @"?DateRange={{ dateRange | EscapeDataString}}&Visitors=1&GroupRole=3%2c4&CampusIds=14'>First Time Guest Report</a>
</p>

{{ 'Global' | Attribute:'EmailFooter' }}" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Body

            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Attachment One
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "FFD9193A-451F-40E6-9776-74D5DCAC1450", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Attachment Two
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "A059767A-5592-4926-948A-1065AF4E9748", @"" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Attachment Three
            RockMigrationHelper.AddActionTypeAttributeValue( "17100485-D8B0-418B-B58E-189AE881E68A", "65E69B78-37D8-4A88-B8AC-71893D2F75EF", @"False" ); // First Time Guest Notification Workflow:Start:Email Link to Form:Save Communication History

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

            // add ServiceJob: Reservation Reminder
            Sql( @"
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Notify Admins of First Time Guest Report'
                  ,''
                  ,'Rock.Jobs.LaunchWorkflow'
                  ,'0 30 9 ? * MON *'
                  ,1
                  ,'312DC659-66E8-439E-BB6B-E979DA3ED4D4'
                  );" );

            var serviceJobId = SqlScalar( "Select top 1 Id From [ServiceJob] Where [Guid] = '312DC659-66E8-439E-BB6B-E979DA3ED4D4'" ).ToString();

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Class", "Rock.Jobs.LaunchWorkflow", "Workflow", "The workflow this job should activate.", 0, @"", "CC960FFE-0A28-4771-8023-06B3E1A95BF6", "Workflow" );

            RockMigrationHelper.AddAttributeValue( "CC960FFE-0A28-4771-8023-06B3E1A95BF6", serviceJobId.AsInteger(), @"f9af3cf6-b7b5-4eb3-800c-b1d88e65be1e", "2AE8F3D6-256C-451F-B1F1-1C58EAC525E2" );

        }
        public override void Down()
        {
            // remove ServiceJob: Reservation Reminder
            Sql( @"DELETE [ServiceJob]  WHERE [Guid] = '312DC659-66E8-439E-BB6B-E979DA3ED4D4'" );
        }
    }
}
