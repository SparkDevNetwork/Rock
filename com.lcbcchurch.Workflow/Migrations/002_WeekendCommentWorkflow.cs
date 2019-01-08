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
    [MigrationNumber( 2, "1.0.14" )]
    public class WeekendCommentWorkflow : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateNoteType( "Weekend Comments", "Rock.Model.Person", false, "E9BEA615-EFA0-452A-981E-3AF51626F86A" );
            var noteTypeId = SqlScalar( "Select Top 1 Id From NoteType Where [Guid] = 'E9BEA615-EFA0-452A-981E-3AF51626F86A'" ).ToString();
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Note", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "NoteTypeId", noteTypeId, "Campus", null, "", 1003, "", "8B14B6F4-93DF-447A-9097-83200CECEE26", "Campus" );

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersonNoteAdd", "B2C8B951-C41E-4DFB-9F92-F183223448AA", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetEntityAttribute", "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "2778BEDA-ECB0-4057-8475-D624495BAEE4" ); // Rock.Workflow.Action.PersonNoteAdd:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Alert", "Alert", "Determines if the note should be flagged as an alert.", 5, @"False", "49C560A6-4347-4907-B7E5-38CAC697C86B" ); // Rock.Workflow.Action.PersonNoteAdd:Alert
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Author", "Author", "Workflow attribute that contains the person to use as the author of the note. While not required it is recommended.", 4, @"", "756DBFE4-4B36-49C3-A047-DB52274D5CF8" ); // Rock.Workflow.Action.PersonNoteAdd:Author
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person", "Person", "Workflow attribute that contains the person to add the note to.", 0, @"", "EE030DB7-2FB7-482B-98AF-7BD61035CAD1" ); // Rock.Workflow.Action.PersonNoteAdd:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "The title/caption of the note. If none is provided then the author's name will be displayed. <span class='tip tip-lava'></span>", 2, @"", "90416422-D244-4D73-A878-7C60350AB154" ); // Rock.Workflow.Action.PersonNoteAdd:Caption
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D5CD976E-E641-465A-AD08-A13F635353F4" ); // Rock.Workflow.Action.PersonNoteAdd:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Text", "Text", "The body of the note. <span class='tip tip-lava'></span>", 3, @"", "D45B1A38-94F0-4B4D-A895-D4E6E14F82C5" ); // Rock.Workflow.Action.PersonNoteAdd:Text
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B2C8B951-C41E-4DFB-9F92-F183223448AA", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "The type of note to add.", 1, @"66A1B9D7-7EFA-40F3-9415-E54437977D60", "4A120959-FB2D-49D2-A9A4-6D04B9DB53D2" ); // Rock.Workflow.Action.PersonNoteAdd:Note Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "BD0EA27D-4B17-4112-AAD6-66ED25D538D8" ); // Rock.Workflow.Action.SetEntityAttribute:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "205AD20A-8481-419F-89CA-EACF0094D9E0" ); // Rock.Workflow.Action.SetEntityAttribute:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Attribute Key|Attribute Key Attribute", "AttributeKey", "The key of the attribute to set. <span class='tip tip-lava'></span>", 2, @"", "E9C633AD-70FF-427D-B2FD-4B1391959E87" ); // Rock.Workflow.Action.SetEntityAttribute:Attribute Key|Attribute Key Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Attribute Value|Attribute Value Attribute", "AttributeValue", "The value to set. <span class='tip tip-lava'></span>", 3, @"", "50754983-F004-485B-9D06-B5EBE980805C" ); // Rock.Workflow.Action.SetEntityAttribute:Attribute Value|Attribute Value Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "D1AF1E79-B4B0-4E5C-AB98-347A486F15FB" ); // Rock.Workflow.Action.SetEntityAttribute:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "964E6708-84C3-4B20-B85F-6B355C9FE4BE" ); // Rock.Workflow.Action.SetEntityAttribute:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Web Forms", "fa fa-wpforms", "Forms that live externally on LCBCChurch.com", "2B52D211-E476-4592-9EDB-79A2EA622267", 0 ); // Web Forms

            #endregion

            #region Make a Comment

            RockMigrationHelper.UpdateWorkflowType( false, true, "Make a Comment", "", "2B52D211-E476-4592-9EDB-79A2EA622267", "Work", "fa fa-list-ol", 28800, true, 0, "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", 0 ); // Make a Comment
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "F89403DE-E186-493F-A11E-CE9D16C87D2D", false ); // Make a Comment:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "", 1, @"", "44872384-1807-4A35-8C94-830F8D13A55A", false ); // Make a Comment:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Comment", "Comment", "", 2, @"", "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", false ); // Make a Comment:Comment
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Guid", "NoteGuid", "", 3, @"", "5F2FB437-CB4F-4DB6-BB34-A256D40342AD", false ); // Make a Comment:Note Guid
            RockMigrationHelper.AddAttributeQualifier( "F89403DE-E186-493F-A11E-CE9D16C87D2D", "EnableSelfSelection", @"False", "3F703E07-7DEC-407F-914A-65FA9A40A141" ); // Make a Comment:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "44872384-1807-4A35-8C94-830F8D13A55A", "includeInactive", @"False", "20A77B02-08D9-4A3E-8F64-1F99DAEF3E76" ); // Make a Comment:Campus:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", "allowhtml", @"False", "5286B219-91C8-49D7-84A5-479D997758FA" ); // Make a Comment:Comment:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", "maxcharacters", @"", "15C6E534-299B-4CFC-B8F3-A7A5AC2451B8" ); // Make a Comment:Comment:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", "numberofrows", @"3", "A95AC28A-6900-4BCB-9825-0373E7DDC15A" ); // Make a Comment:Comment:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", "showcountdown", @"False", "BA179BBA-FF89-45B8-8D4B-A3CE67574384" ); // Make a Comment:Comment:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "5F2FB437-CB4F-4DB6-BB34-A256D40342AD", "ispassword", @"False", "45562CAD-0DC8-43AF-8FCC-AC97A437D264" ); // Make a Comment:Note Guid:ispassword
            RockMigrationHelper.AddAttributeQualifier( "5F2FB437-CB4F-4DB6-BB34-A256D40342AD", "maxcharacters", @"", "FE201B98-1AFF-4F65-93E7-BE88E1351070" ); // Make a Comment:Note Guid:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "5F2FB437-CB4F-4DB6-BB34-A256D40342AD", "showcountdown", @"False", "89477909-0648-4B28-A890-94B6EF2B451D" ); // Make a Comment:Note Guid:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "0CF2CA1C-60CF-46E3-B2C9-6437E7BA0FEE", true, "Start", "", true, 0, "08A1BECC-458C-4006-91C5-A5790A14ACC0" ); // Make a Comment:Start
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^^^Your information has been submitted successfully.", "", false, "", "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B" ); // Make a Comment:Start:User Entry Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B", "F89403DE-E186-493F-A11E-CE9D16C87D2D", 0, true, false, true, false, @"", @"", "887D394E-6F58-49FE-BBE6-7FFBD2A9E2FC" ); // Make a Comment:Start:User Entry Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B", "44872384-1807-4A35-8C94-830F8D13A55A", 1, true, false, true, false, @"", @"", "49E0C8F9-9B81-491F-B952-81752EB261B8" ); // Make a Comment:Start:User Entry Form:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B", "4048405C-57FD-4EA7-861E-0FEDFE2BC3D3", 2, true, false, true, false, @"", @"", "8BEC81DA-49F5-47A9-B0A0-ECA130F36660" ); // Make a Comment:Start:User Entry Form:Comment
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B", "5F2FB437-CB4F-4DB6-BB34-A256D40342AD", 3, false, true, false, false, @"", @"", "B4F415FA-17A2-4F8B-BDB5-F96F97699451" ); // Make a Comment:Start:User Entry Form:Note Guid
            RockMigrationHelper.UpdateWorkflowActionType( "08A1BECC-458C-4006-91C5-A5790A14ACC0", "User Entry Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "1F2BAFC2-4E6B-4B89-A3E7-1897355AB70B", "", 1, "", "18E1DE2D-D609-48B3-9E1F-7D5DAFA0CB54" ); // Make a Comment:Start:User Entry Form
            RockMigrationHelper.UpdateWorkflowActionType( "08A1BECC-458C-4006-91C5-A5790A14ACC0", "Add Note", 1, "B2C8B951-C41E-4DFB-9F92-F183223448AA", true, false, "", "", 1, "", "A5F95ACB-C388-420A-96CC-9681506604EE" ); // Make a Comment:Start:Add Note
            RockMigrationHelper.UpdateWorkflowActionType( "08A1BECC-458C-4006-91C5-A5790A14ACC0", "Get Note Guid", 2, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "8C41E000-95EC-4C43-90F1-CF88539484CF" ); // Make a Comment:Start:Get Note Guid
            RockMigrationHelper.UpdateWorkflowActionType( "08A1BECC-458C-4006-91C5-A5790A14ACC0", "Entity Attribute Set", 3, "E8E85D29-37B2-4B4B-BF4A-03D84390A74F", true, true, "", "", 1, "", "357FE45B-33F9-489C-B2D1-0643E9848364" ); // Make a Comment:Start:Entity Attribute Set
            RockMigrationHelper.AddActionTypeAttributeValue( "18E1DE2D-D609-48B3-9E1F-7D5DAFA0CB54", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Make a Comment:Start:User Entry Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "18E1DE2D-D609-48B3-9E1F-7D5DAFA0CB54", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Make a Comment:Start:User Entry Form:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "EE030DB7-2FB7-482B-98AF-7BD61035CAD1", @"f89403de-e186-493f-a11e-ce9d16c87d2d" ); // Make a Comment:Start:Add Note:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "D5CD976E-E641-465A-AD08-A13F635353F4", @"" ); // Make a Comment:Start:Add Note:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "2778BEDA-ECB0-4057-8475-D624495BAEE4", @"False" ); // Make a Comment:Start:Add Note:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "4A120959-FB2D-49D2-A9A4-6D04B9DB53D2", @"E9BEA615-EFA0-452A-981E-3AF51626F86A" ); // Make a Comment:Start:Add Note:Note Type
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "90416422-D244-4D73-A878-7C60350AB154", @"" ); // Make a Comment:Start:Add Note:Caption
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "D45B1A38-94F0-4B4D-A895-D4E6E14F82C5", @"{{Workflow | Attribute:'Comment'}}" ); // Make a Comment:Start:Add Note:Text
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "756DBFE4-4B36-49C3-A047-DB52274D5CF8", @"" ); // Make a Comment:Start:Add Note:Author
            RockMigrationHelper.AddActionTypeAttributeValue( "A5F95ACB-C388-420A-96CC-9681506604EE", "49C560A6-4347-4907-B7E5-38CAC697C86B", @"False" ); // Make a Comment:Start:Add Note:Alert
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "F3B9908B-096F-460B-8320-122CF046D1F9", @"SELECT TOP 1 [Guid] 
FROM Note 
WHERE EntityId = {{ Workflow | Attribute:'Person', 'Id' }} 
AND NoteTypeId = 63 
ORDER BY CreatedDateTime DESC" ); // Make a Comment:Start:Get Note Guid:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Make a Comment:Start:Get Note Guid:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // Make a Comment:Start:Get Note Guid:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "9868FF7B-5EA4-43DC-AE35-26C3DB91D3A5", @"" ); // Make a Comment:Start:Get Note Guid:Parameters
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "56997192-2545-4EA1-B5B2-313B04588984", @"5f2fb437-cb4f-4db6-bb34-a256d40342ad" ); // Make a Comment:Start:Get Note Guid:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "8C41E000-95EC-4C43-90F1-CF88539484CF", "423B33CB-85A7-40DC-B7C8-71C0EA4A0CDC", @"False" ); // Make a Comment:Start:Get Note Guid:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "964E6708-84C3-4B20-B85F-6B355C9FE4BE", @"" ); // Make a Comment:Start:Entity Attribute Set:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "BD0EA27D-4B17-4112-AAD6-66ED25D538D8", @"False" ); // Make a Comment:Start:Entity Attribute Set:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "205AD20A-8481-419F-89CA-EACF0094D9E0", @"53dc1e78-14a5-44de-903f-6a2cb02164e7" ); // Make a Comment:Start:Entity Attribute Set:Entity Type
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "D1AF1E79-B4B0-4E5C-AB98-347A486F15FB", @"{{ Workflow | Attribute:'NoteGuid' }}" ); // Make a Comment:Start:Entity Attribute Set:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "E9C633AD-70FF-427D-B2FD-4B1391959E87", @"Campus" ); // Make a Comment:Start:Entity Attribute Set:Attribute Key|Attribute Key Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "357FE45B-33F9-489C-B2D1-0643E9848364", "50754983-F004-485B-9D06-B5EBE980805C", @"44872384-1807-4a35-8c94-830f8d13a55a" ); // Make a Comment:Start:Entity Attribute Set:Attribute Value|Attribute Value Attribute

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

        }
    }
}
