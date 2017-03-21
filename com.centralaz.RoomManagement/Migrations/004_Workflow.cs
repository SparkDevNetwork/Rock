// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 4, "1.4.5" )]
    public class Workflow : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Reservation", "", "com.centralaz.RoomManagement", "com.centralaz.RoomManagement.Field.Types.ReservationFieldType", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );
            RockMigrationHelper.UpdateFieldType( "ReservationStatus", "", "com.centralaz.RoomManagement", "com.centralaz.RoomManagement.Field.Types.ReservationStatusFieldType", "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );

            #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 4, @"False", "2040B757-3012-4EAB-AB98-8222EC97AB7F" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 2, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "EFC517E9-8A53-4681-B16B-9D4DE89244BE" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "83E27C1E-1491-4AE2-93F1-909791D4B70A" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Room Reservation Approval Notification

            RockMigrationHelper.UpdateWorkflowType( false, true, "Room Reservation Approval Notification", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "543D4FCD-310B-4048-BFCB-BAE582CBB890", 0 ); // Room Reservation Approval Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Current Reservation Status", "CurrentReservationStatus", "", 0, @"", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", false ); // Room Reservation Approval Notification:Current Reservation Status
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "General Approval Group", "GeneralApprovalGroup", "", 1, @"fbe0324f-f29a-4acf-8ec3-5386c5562d70", "653CE164-554A-4B22-A830-3E760DA2023E", false ); // Room Reservation Approval Notification:General Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reservation Detail Page", "ReservationDetailPage", "", 2, @"4cbd2b96-e076-46df-a576-356bca5e577f", "73189729-A15A-4EAD-A17F-90E0A9538653", false ); // Room Reservation Approval Notification:Reservation Detail Page
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 3, @"", "AEACA577-F09F-4818-8C09-CA5490B0DD91", false ); // Room Reservation Approval Notification:Reservation
            RockMigrationHelper.AddAttributeQualifier( "9E1FD102-AA41-4F7A-A52D-601DB1852A44", "ispassword", @"False", "A7D1E684-D628-488B-997A-1EED71927314" ); // Room Reservation Approval Notification:Current Reservation Status:ispassword
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Set Attributes", "", true, 0, "6A396018-6CC1-4C41-8EF1-FB9779C0B04D" ); // Room Reservation Approval Notification:Set Attributes
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Send Notification Email", "", false, 1, "2C1387D7-3E8F-4702-A9B9-4C2E52684EEE" ); // Room Reservation Approval Notification:Send Notification Email
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Reservation From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "3628C256-C190-449F-B41A-0928DAE7615F" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Current Reservation Status From Entity", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Activate Activity", 2, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49" ); // Room Reservation Approval Notification:Set Attributes:Activate Activity
            RockMigrationHelper.UpdateWorkflowActionType( "2C1387D7-3E8F-4702-A9B9-4C2E52684EEE", "Request General Approval", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "Unapproved", "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"aeaca577-f09f-4818-8c09-ca5490b0dd91" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"{{ Entity.Guid }}" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"9e1fd102-aa41-4f7a-a52d-601db1852a44" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"{{ Entity.ReservationStatus.Name }}" ); // Room Reservation Approval Notification:Set Attributes:Set Current Reservation Status From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Set Attributes:Activate Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Set Attributes:Activate Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2C1387D7-3E8F-4702-A9B9-4C2E52684EEE" ); // Room Reservation Approval Notification:Set Attributes:Activate Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "0C4C13B8-7076-4872-925A-F950886B5E16", @"653ce164-554a-4b22-a830-3e760da2023e" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Room Reservation Requires Approval" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new room reservation requires your approval:<br/><br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>

{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "2040B757-3012-4EAB-AB98-8222EC97AB7F", @"False" ); // Room Reservation Approval Notification:Send Notification Email:Request General Approval:Save Communication History

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
            RockMigrationHelper.DeleteFieldType( "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );
            RockMigrationHelper.DeleteFieldType( "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );
        }
    }
}
