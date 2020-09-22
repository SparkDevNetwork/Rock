// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 29, "1.9.4" )]
    public class ReservationLocationTypes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReservationTypeId] [int] NOT NULL,
	[LocationTypeValueId] [int] NOT NULL,
	[CreatedDateTime] [datetime] NULL,
	[ModifiedDateTime] [datetime] NULL,
	[CreatedByPersonAliasId] [int] NULL,
	[ModifiedByPersonAliasId] [int] NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[ForeignKey] [nvarchar](100) NULL,
	[ForeignGuid] [uniqueidentifier] NULL,
	[ForeignId] [int] NULL,
 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLocationType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_CreatedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ReservationTypeId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_LocationTypeValueId] FOREIGN KEY([LocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_LocationTypeValueId]
" );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationLocationType", "Reservation Location Type", "com.bemaservices.RoomManagement.Model.ReservationLocationType, com.bemaservices.RoomManagement, Version=1.2.2.0, Culture=neutral, PublicKeyToken=null", true, true, "834F278F-49E6-4BEA-B724-E7723F9EE4C9" );

            Sql( @"
INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
           ([LocationTypeValueId]
           ,[Guid]
           ,[ReservationTypeId])
     Select dv.Id
           ,newId()
           ,rt.Id
		   From [dbo].[_com_bemaservices_RoomManagement_ReservationType] rt
Outer Apply DefinedValue dv where dv.DefinedTypeId = (Select Top 1 Id From DefinedType Where Guid = '3285DCEF-FAA4-43B9-9338-983F4A384ABA')" );

                        #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);
            RockMigrationHelper.UpdateEntityType("com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState","3894452A-E763-41AC-8260-10373646D8A0",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.ActivateActivity","38907A90-1634-4A93-8017-619326A4A582",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersistWorkflow","F1A39347-6FE0-43D4-89FB-544195088ECF",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunLava","BC21E57A-1477-44B3-A7C2-61A806118945",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunSQL","A41216D6-6FB0-4019-B222-2C29B4519CF4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmail","66197B01-D1F0-4924-A315-47AD54E030DE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeFromEntity","972F19B9-598B-474B-97A4-50E56E7B59D2",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeValue","C789E457-0783-44B3-9D8F-2EBAB5F11110",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E8ABD802-372C-47BE-82B1-96F50DB5169E"); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","739FD425-5B8C-4605-B775-7E4D9D4C11DB","Activity","Activity","The activity type to activate",0,@"","02D5A7A5-8781-46B4-B9FC-AF816829D240"); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3809A78C-B773-440C-8E3F-A8E81D0DAE08"); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3894452A-E763-41AC-8260-10373646D8A0","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","ACA008E2-2406-457E-8E4C-6922E03757A4"); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3894452A-E763-41AC-8260-10373646D8A0","33E6DF69-BDFA-407A-9744-C175B60643AE","Approval State Attribute","ApprovalStateAttribute","The attribute that contains the reservation approval state.",1,@"","2E185FB5-FC8E-41BE-B7FE-702F74B47539"); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3894452A-E763-41AC-8260-10373646D8A0","33E6DF69-BDFA-407A-9744-C175B60643AE","Reservation Attribute","ReservationAttribute","The attribute that contains the reservation.",0,@"","1D4F819F-145D-4A7F-AB4E-AD7C06759042"); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3894452A-E763-41AC-8260-10373646D8A0","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25954FDC-F486-417D-ABBB-E2DF2C67B186"); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3894452A-E763-41AC-8260-10373646D8A0","F4ACC5B8-98BB-4611-B6B7-065BBC47503B","Approval State","ApprovalState","The connection state to use (if Connection State Attribute is not specified).",2,@"","C32C481E-3123-4347-A5FA-E3C79FE3D4A2"); // com.bemaservices.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Body","Body","The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",4,@"","4D245B9E-6B03-46E7-8482-A51FBA190E4D"); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36197160-7D3D-490D-AB42-7E29105AFE91"); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Save Communication History","SaveCommunicationHistory","Should a record of this communication be saved to the recipient's profile",8,@"False","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3"); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment One","AttachmentOne","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",5,@"","C2C7DA55-3018-4645-B9EE-4BCD11855F2C"); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment Three","AttachmentThree","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",7,@"","A059767A-5592-4926-948A-1065AF4E9748"); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment Two","AttachmentTwo","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",6,@"","FFD9193A-451F-40E6-9776-74D5DCAC1450"); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Send to Group Role","GroupRole","An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.",2,@"","E3667110-339F-4FE3-B6B7-084CF9633580"); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","From Email Address|Attribute Value","From","The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",0,@"","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC"); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Addresses|Attribute Value","To","The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>",1,@"","0C4C13B8-7076-4872-925A-F950886B5E16"); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Subject","Subject","The subject that should be used when sending email. <span class='tip tip-lava'></span>",3,@"","5D9B13B6-CD96-4C7C-86FA-4512B9D28386"); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D1269254-C15A-40BD-B784-ADCC231D3950"); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>",4,@"","7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199"); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","9392E3D7-A28B-4CD8-8B03-5E147B102EF1"); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Entity Is Required","EntityIsRequired","Should an error be returned if the entity is missing or not a valid entity type?",2,@"True","B524B00C-29CB-49E9-9896-8BB60F209783"); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Use Id instead of Guid","UseId","Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).",3,@"False","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B"); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",1,@"","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7"); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","AD4EFAC4-E687-43DF-832F-0DC3856ABABB"); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","SQLQuery","SQLQuery","The SQL query to run. <span class='tip tip-lava'></span>",0,@"","F3B9908B-096F-460B-8320-122CF046D1F9"); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","A18C3143-0586-4565-9F36-E603BC674B4E"); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Continue On Error","ContinueOnError","Should processing continue even if SQL Error occurs?",3,@"False","9BCD10A8-FDCE-44A9-9CC8-72D935B5E974"); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","33E6DF69-BDFA-407A-9744-C175B60643AE","Result Attribute","ResultAttribute","An optional attribute to set to the scaler result of SQL query.",2,@"","56997192-2545-4EA1-B5B2-313B04588984"); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","73B02051-0D38-4AD9-BF81-A2D477DE4F70","Parameters","Parameters","The parameters to supply to the SQL query. <span class='tip tip-lava'></span>",1,@"","EA9A026A-934F-4920-97B1-9734795127ED"); // Rock.Workflow.Action.RunSQL:Parameters
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
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Status|Status Attribute","Status","The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>",0,@"Completed","385A255B-9F48-4625-862B-26231DBAC53A"); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","50B01639-4938-40D2-A791-AA0EB4F86847"); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Persist Immediately","PersistImmediately","This action will normally cause the workflow to be persisted (saved) once all the current activities/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.",0,@"False","82744A46-0110-4728-BD3D-66C85C5FCB2F"); // Rock.Workflow.Action.PersistWorkflow:Persist Immediately
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","86F795B0-0CB6-4DA4-9CE4-B11D0922F361"); // Rock.Workflow.Action.PersistWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","Room Management","fa fa-building-o","","B8E4B3B0-B543-48B6-93BE-604D4F368559",0); // Room Management

            #endregion

            #region Room Reservation Approval Update

            RockMigrationHelper.UpdateWorkflowType(false,true,"Room Reservation Approval Update","A workflow that changes the reservation's approval status if it was modified by someone not in the Final Approval Group after being approved.","B8E4B3B0-B543-48B6-93BE-604D4F368559","Approval Update","fa fa-list-ol",28800,false,0,"13D0361C-0552-43CA-8F27-D47DB120608D",0); // Room Reservation Approval Update
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","9C204CD0-1233-41C5-818A-C5DA439445AA","Approval State","ApprovalState","",0,@"","2D709B9B-C75C-4B47-8649-3D05A593BD21", false); // Room Reservation Approval Update:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","7BD25DC9-F34A-478D-BEF9-0C787F5D39B8","Final Approval Group","FinalApprovalGroup","An optional group that gives final approval for reservations.",1,@"","0BBB10DD-4702-4374-B294-9656A2E17440", false); // Room Reservation Approval Update:Final Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","66739D2C-1F39-44C4-BDBB-9AB181DA4ED7","Reservation","Reservation","",2,@"","58001844-1FCF-4ADD-96C4-37D4FF68AF88", false); // Room Reservation Approval Update:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requester","Requester","",3,@"","6764C3C7-0E95-45C7-BF91-247A7F3E3AA2", false); // Room Reservation Approval Update:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Admin Contact","AdminContact","",4,@"","6160000B-A3FF-4563-BCD6-1B3E12B30601", false); // Room Reservation Approval Update:Admin Contact
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","FE95430C-322D-4B67-9C77-DFD1D4408725","Previous Modified Date Time","PreviousModifiedDateTime","",5,@"","4570202A-D5C6-49D0-960D-9F3EAC1BBD57", false); // Room Reservation Approval Update:Previous Modified Date Time
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","9C204CD0-1233-41C5-818A-C5DA439445AA","Reservation Changes","ReservationChanges","",6,@"","B943F376-3CF6-42F8-8B99-2207AC7BBE2A", false); // Room Reservation Approval Update:Reservation Changes
            RockMigrationHelper.UpdateWorkflowTypeAttribute("13D0361C-0552-43CA-8F27-D47DB120608D","9C204CD0-1233-41C5-818A-C5DA439445AA","Is Approval State Change Needed","IsApprovalStateChangeNeeded","",7,@"","52A9B3E2-0DA0-4972-9172-F5C62ECC8076", false); // Room Reservation Approval Update:Is Approval State Change Needed
            RockMigrationHelper.AddAttributeQualifier("2D709B9B-C75C-4B47-8649-3D05A593BD21","ispassword",@"False","1E77B359-2741-4916-8F84-03E981CA968D"); // Room Reservation Approval Update:Approval State:ispassword
            RockMigrationHelper.AddAttributeQualifier("6764C3C7-0E95-45C7-BF91-247A7F3E3AA2","EnableSelfSelection",@"False","94AED7D2-C950-4E56-92D8-33CC6A71D103"); // Room Reservation Approval Update:Requester:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("6160000B-A3FF-4563-BCD6-1B3E12B30601","EnableSelfSelection",@"False","6FE040E2-4BBE-47AE-835B-0FE4B1A6E67C"); // Room Reservation Approval Update:Admin Contact:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("4570202A-D5C6-49D0-960D-9F3EAC1BBD57","datePickerControlType",@"Date Picker","71DC4B14-B9E6-4ABF-85B3-30874D1883F1"); // Room Reservation Approval Update:Previous Modified Date Time:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier("4570202A-D5C6-49D0-960D-9F3EAC1BBD57","displayCurrentOption",@"False","73B2EF69-2931-4EC4-A5F8-44E4D4B61F85"); // Room Reservation Approval Update:Previous Modified Date Time:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier("4570202A-D5C6-49D0-960D-9F3EAC1BBD57","displayDiff",@"False","2D228BA7-5805-474C-BE93-926593B521A6"); // Room Reservation Approval Update:Previous Modified Date Time:displayDiff
            RockMigrationHelper.AddAttributeQualifier("4570202A-D5C6-49D0-960D-9F3EAC1BBD57","format",@"","65D89F86-8698-4CA0-92DD-E51E12EAB81A"); // Room Reservation Approval Update:Previous Modified Date Time:format
            RockMigrationHelper.AddAttributeQualifier("4570202A-D5C6-49D0-960D-9F3EAC1BBD57","futureYearCount",@"","2BDA0D2D-4424-4233-A874-85BFA4CE42D3"); // Room Reservation Approval Update:Previous Modified Date Time:futureYearCount
            RockMigrationHelper.AddAttributeQualifier("B943F376-3CF6-42F8-8B99-2207AC7BBE2A","ispassword",@"False","10CA1A4B-C79A-4EB9-84ED-561341458D15"); // Room Reservation Approval Update:Reservation Changes:ispassword
            RockMigrationHelper.AddAttributeQualifier("B943F376-3CF6-42F8-8B99-2207AC7BBE2A","maxcharacters",@"","9A3A0074-1B16-43F3-B7E2-935D61042043"); // Room Reservation Approval Update:Reservation Changes:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("B943F376-3CF6-42F8-8B99-2207AC7BBE2A","showcountdown",@"False","941B6C23-40B2-4BE0-9E57-7630977F376C"); // Room Reservation Approval Update:Reservation Changes:showcountdown
            RockMigrationHelper.AddAttributeQualifier("52A9B3E2-0DA0-4972-9172-F5C62ECC8076","ispassword",@"False","C55770B9-0D67-4A17-8171-471FD7F773CE"); // Room Reservation Approval Update:Is Approval State Change Needed:ispassword
            RockMigrationHelper.AddAttributeQualifier("52A9B3E2-0DA0-4972-9172-F5C62ECC8076","maxcharacters",@"","102028C3-05D9-43BB-9610-0DE8BF41EDB5"); // Room Reservation Approval Update:Is Approval State Change Needed:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("52A9B3E2-0DA0-4972-9172-F5C62ECC8076","showcountdown",@"False","D930520B-3667-4B4A-9604-46F545C9605F"); // Room Reservation Approval Update:Is Approval State Change Needed:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType("13D0361C-0552-43CA-8F27-D47DB120608D",true,"Set Attributes","",true,0,"9D366F65-51B6-47C7-BACB-E0F7DC8B1072"); // Room Reservation Approval Update:Set Attributes
            RockMigrationHelper.UpdateWorkflowActivityType("13D0361C-0552-43CA-8F27-D47DB120608D",true,"Notify Approval group that the Reservation is Pending Review","",false,1,"4CBF518F-AB76-4C67-9B70-597CCDABEEBA"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","9C204CD0-1233-41C5-818A-C5DA439445AA","Delay Activated","8167ed7e-35f9-4ab4-85d3-e929cf965f44","",0,@"","9897855E-07F4-433B-8284-75E709CB909B"); // Room Reservation Approval Update:Set Attributes:Delay Activated
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Final Approval Group from Entity",0,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","1F7907CB-26FD-4153-AE61-036F0B0FE764"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Reservation From Entity",1,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","B85B9A95-7998-4275-A23B-019CA7BC8819"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Admin Contact From Entity",2,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","8F878A72-B797-4A80-8BE1-98C7A4BC5CB5"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Approval State From Entity",3,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","E27AA360-6C27-44B4-A61E-5B479A5D860D"); // Room Reservation Approval Update:Set Attributes:Set Approval State From Entity
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Reservation Changes",4,"A41216D6-6FB0-4019-B222-2C29B4519CF4",true,false,"","58001844-1FCF-4ADD-96C4-37D4FF68AF88",64,"","6E5028B9-D657-4B2E-B70E-9EAB0565AFCD"); // Room Reservation Approval Update:Set Attributes:Set Reservation Changes
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Is Approval State Change Needed",5,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",1,"","13BBE5D5-9E8E-41AA-A03D-A0D609DEE384"); // Room Reservation Approval Update:Set Attributes:Set Is Approval State Change Needed
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Persist Workflow",6,"F1A39347-6FE0-43D4-89FB-544195088ECF",true,false,"","52A9B3E2-0DA0-4972-9172-F5C62ECC8076",8,"True","AF9160BF-892D-4DCC-9113-71FAB4FB5B7C"); // Room Reservation Approval Update:Set Attributes:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Set Reservation Status",7,"3894452A-E763-41AC-8260-10373646D8A0",true,false,"","52A9B3E2-0DA0-4972-9172-F5C62ECC8076",8,"True","194D86C6-5581-4944-8EAB-972997DBC93A"); // Room Reservation Approval Update:Set Attributes:Set Reservation Status
            RockMigrationHelper.UpdateWorkflowActionType("9D366F65-51B6-47C7-BACB-E0F7DC8B1072","Activate Pending Review Activity",8,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","",1,"","F7A6042A-0B86-4585-BBC3-0D06416E3D69"); // Room Reservation Approval Update:Set Attributes:Activate Pending Review Activity
            RockMigrationHelper.UpdateWorkflowActionType("4CBF518F-AB76-4C67-9B70-597CCDABEEBA","Send Email",0,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","0BBB10DD-4702-4374-B294-9656A2E17440",64,"","40190F7C-DDC6-4F2D-8E6F-E604B5CA273C"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email
            RockMigrationHelper.UpdateWorkflowActionType("4CBF518F-AB76-4C67-9B70-597CCDABEEBA","Complete Workflow",1,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,false,"","",32,"","BFCBA1F7-5872-4766-B986-78DEA404DF8C"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue("1F7907CB-26FD-4153-AE61-036F0B0FE764","9392E3D7-A28B-4CD8-8B03-5E147B102EF1",@"False"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("1F7907CB-26FD-4153-AE61-036F0B0FE764","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"0bbb10dd-4702-4374-b294-9656a2e17440"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("1F7907CB-26FD-4153-AE61-036F0B0FE764","B524B00C-29CB-49E9-9896-8BB60F209783",@"True"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue("1F7907CB-26FD-4153-AE61-036F0B0FE764","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B",@"False"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue("1F7907CB-26FD-4153-AE61-036F0B0FE764","7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199",@"{{ Entity.ReservationType.FinalApprovalGroup.Guid }}"); // Room Reservation Approval Update:Set Attributes:Set Final Approval Group from Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue("B85B9A95-7998-4275-A23B-019CA7BC8819","9392E3D7-A28B-4CD8-8B03-5E147B102EF1",@"False"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B85B9A95-7998-4275-A23B-019CA7BC8819","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"58001844-1fcf-4add-96c4-37d4ff68af88"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("B85B9A95-7998-4275-A23B-019CA7BC8819","B524B00C-29CB-49E9-9896-8BB60F209783",@"True"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue("B85B9A95-7998-4275-A23B-019CA7BC8819","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B",@"False"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue("B85B9A95-7998-4275-A23B-019CA7BC8819","7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199",@"{{ Entity.Guid }}"); // Room Reservation Approval Update:Set Attributes:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue("8F878A72-B797-4A80-8BE1-98C7A4BC5CB5","9392E3D7-A28B-4CD8-8B03-5E147B102EF1",@"False"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("8F878A72-B797-4A80-8BE1-98C7A4BC5CB5","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"6160000b-a3ff-4563-bcd6-1b3e12b30601"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("8F878A72-B797-4A80-8BE1-98C7A4BC5CB5","B524B00C-29CB-49E9-9896-8BB60F209783",@"True"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue("8F878A72-B797-4A80-8BE1-98C7A4BC5CB5","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B",@"False"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue("8F878A72-B797-4A80-8BE1-98C7A4BC5CB5","7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199",@"{{ Entity.RequesterAlias.Guid }}"); // Room Reservation Approval Update:Set Attributes:Set Admin Contact From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue("E27AA360-6C27-44B4-A61E-5B479A5D860D","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Room Reservation Approval Update:Set Attributes:Set Approval State From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("E27AA360-6C27-44B4-A61E-5B479A5D860D","44A0B977-4730-4519-8FF6-B0A01A95B212",@"2d709b9b-c75c-4b47-8649-3d05a593bd21"); // Room Reservation Approval Update:Set Attributes:Set Approval State From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("E27AA360-6C27-44B4-A61E-5B479A5D860D","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"{% assign reservation =  Workflow | Attribute:'Reservation', 'object' %}{{ reservation.ApprovalState }}"); // Room Reservation Approval Update:Set Attributes:Set Approval State From Entity:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("6E5028B9-D657-4B2E-B70E-9EAB0565AFCD","F3B9908B-096F-460B-8320-122CF046D1F9",@"SELECT Verb+' '+ValueName+' From '+IsNull(OldValue,'N/A')+' to '+IsNull(NewValue,'N/A')
  FROM [History] h
  Join Category c on c.Id = h.CategoryId and c.Guid = '806E96DE-3744-4F56-B12F-787F36A1CEB5'
  Join [dbo].[_com_bemaservices_RoomManagement_Reservation] r on r.Id = h.EntityId
  Where h.Verb in ('MODIFY','ADD','REMOVE','DELETE')
  And h.ValueName not like '%Approval State%'
  And h.ValueName not like '%Updated by the%'
  And h.ValueName not in (
			'Reservation',
			'Name',
			'Event Contact',
			'Event Contact Phone Number',
			'Event Contact Email',
			'Administrative Contact',
			'Administrative Contact Phone Number',
			'Administrative Contact Email',
			'Approval State',
			'Note'
			)
And h.CreatedDateTime > '{{Workflow | Attribute:'PreviousModifiedDateTime','RawValue' | DateAdd:2,'s'}}'
And r.Id = {{Workflow | Attribute:'Reservation','Id'}}"); // Room Reservation Approval Update:Set Attributes:Set Reservation Changes:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue("6E5028B9-D657-4B2E-B70E-9EAB0565AFCD","A18C3143-0586-4565-9F36-E603BC674B4E",@"False"); // Room Reservation Approval Update:Set Attributes:Set Reservation Changes:Active
            RockMigrationHelper.AddActionTypeAttributeValue("6E5028B9-D657-4B2E-B70E-9EAB0565AFCD","56997192-2545-4EA1-B5B2-313B04588984",@"b943f376-3cf6-42f8-8b99-2207ac7bbe2a"); // Room Reservation Approval Update:Set Attributes:Set Reservation Changes:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("6E5028B9-D657-4B2E-B70E-9EAB0565AFCD","9BCD10A8-FDCE-44A9-9CC8-72D935B5E974",@"False"); // Room Reservation Approval Update:Set Attributes:Set Reservation Changes:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue("13BBE5D5-9E8E-41AA-A03D-A0D609DEE384","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign reservation = Workflow | Attribute:'Reservation','Object' %}
{% assign finalApprovalGroupGuid = Workflow | Attribute:'FinalApprovalGroup','RawValue' %}
{% sql %}
Select top 1 *
From [Group]
Where Guid = '{{finalApprovalGroupGuid}}'
{% endsql %}
{% for item in results %}
    {% assign finalApprovalGroup = item %}
{% endfor %}
{% assign modifier = reservation.ModifiedByPersonAliasId | PersonByAliasId %}
{% assign finalApprovalGroupIdString = finalApprovalGroup.Id | AsString %}
{% assign groupMembers = modifier | Group:finalApprovalGroupIdString %}
{% assign groupMemberCount = groupMembers | Size %}
{% assign approvalState = Workflow | Attribute:'ApprovalState' %}
{% assign changes = Workflow | Attribute:'ReservationChanges' %}

{% if finalApprovalGroup.Id == null || groupMemberCount > 0 || approvalState != 'Approved' || changes == '' %}
    False
{% else %}
    True
{% endif %}"); // Room Reservation Approval Update:Set Attributes:Set Is Approval State Change Needed:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("13BBE5D5-9E8E-41AA-A03D-A0D609DEE384","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // Room Reservation Approval Update:Set Attributes:Set Is Approval State Change Needed:Active
            RockMigrationHelper.AddActionTypeAttributeValue("13BBE5D5-9E8E-41AA-A03D-A0D609DEE384","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"52a9b3e2-0da0-4972-9172-f5c62ecc8076"); // Room Reservation Approval Update:Set Attributes:Set Is Approval State Change Needed:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("13BBE5D5-9E8E-41AA-A03D-A0D609DEE384","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"Sql"); // Room Reservation Approval Update:Set Attributes:Set Is Approval State Change Needed:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("AF9160BF-892D-4DCC-9113-71FAB4FB5B7C","50B01639-4938-40D2-A791-AA0EB4F86847",@"False"); // Room Reservation Approval Update:Set Attributes:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("AF9160BF-892D-4DCC-9113-71FAB4FB5B7C","82744A46-0110-4728-BD3D-66C85C5FCB2F",@"True"); // Room Reservation Approval Update:Set Attributes:Persist Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue("194D86C6-5581-4944-8EAB-972997DBC93A","ACA008E2-2406-457E-8E4C-6922E03757A4",@"False"); // Room Reservation Approval Update:Set Attributes:Set Reservation Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("194D86C6-5581-4944-8EAB-972997DBC93A","1D4F819F-145D-4A7F-AB4E-AD7C06759042",@"58001844-1fcf-4add-96c4-37d4ff68af88"); // Room Reservation Approval Update:Set Attributes:Set Reservation Status:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("194D86C6-5581-4944-8EAB-972997DBC93A","C32C481E-3123-4347-A5FA-E3C79FE3D4A2",@"5"); // Room Reservation Approval Update:Set Attributes:Set Reservation Status:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue("F7A6042A-0B86-4585-BBC3-0D06416E3D69","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Room Reservation Approval Update:Set Attributes:Activate Pending Review Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("F7A6042A-0B86-4585-BBC3-0D06416E3D69","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"4CBF518F-AB76-4C67-9B70-597CCDABEEBA"); // Room Reservation Approval Update:Set Attributes:Activate Pending Review Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("40190F7C-DDC6-4F2D-8E6F-E604B5CA273C","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue("40190F7C-DDC6-4F2D-8E6F-E604B5CA273C","0C4C13B8-7076-4872-925A-F950886B5E16",@"0bbb10dd-4702-4374-b294-9656a2e17440"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("40190F7C-DDC6-4F2D-8E6F-E604B5CA273C","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"Approval Needed: {{Workflow | Attribute:'Reservation'}}"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("40190F7C-DDC6-4F2D-8E6F-E604B5CA273C","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> A new reservation requires your approval:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.bemaservices.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("40190F7C-DDC6-4F2D-8E6F-E604B5CA273C","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3",@"False"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue("BFCBA1F7-5872-4766-B986-78DEA404DF8C","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("BFCBA1F7-5872-4766-B986-78DEA404DF8C","385A255B-9F48-4625-862B-26231DBAC53A",@"Completed"); // Room Reservation Approval Update:Notify Approval group that the Reservation is Pending Review:Complete Workflow:Status|Status Attribute

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

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }

         public void UpdateEntityTypeByGuid( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{5}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [FriendlyName] = '{1}',
                        [AssemblyName] = '{2}',
                        [IsEntity] = {3},
                        [IsSecured] = {4},
                        [Name] = '{0}'
                    WHERE [Guid] = '{5}'
                END
                ELSE
                BEGIN
                    DECLARE @Guid uniqueidentifier
                    SET @Guid = (SELECT [Guid] FROM [EntityType] WHERE [Name] = '{0}')
                    IF @Guid IS NULL
                    BEGIN
                        INSERT INTO [EntityType] (
                            [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[IsCommon],[Guid])
                        VALUES(
                            '{0}','{1}','{2}',{3},{4},0,'{5}')
                    END
                    ELSE
                    BEGIN

                        UPDATE [EntityType] SET
                            [FriendlyName] = '{1}',
                            [AssemblyName] = '{2}',
                            [IsEntity] = {3},
                            [IsSecured] = {4},
                            [Guid] = '{5}'
                        WHERE [Name] = '{0}'

                        -- Update any attribute values that might have been using entity's old guid value
	                    DECLARE @EntityTypeFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.EntityTypeFieldType' )
	                    DECLARE @ComponentFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentFieldType' )
	                    DECLARE @ComponentsFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentsFieldType' )

                        UPDATE V SET [Value] = REPLACE( LOWER([Value]), LOWER(CAST(@Guid AS varchar(50))), LOWER('{5}') )
	                    FROM [AttributeValue] V
	                    INNER JOIN [Attribute] A ON A.[Id] = V.[AttributeId]
	                    WHERE ( A.[FieldTypeId] = @EntityTypeFieldTypeId OR A.[FieldTypeId] = @ComponentFieldTypeId	OR A.[FieldTypeId] = @ComponentsFieldTypeId )
                        OPTION (RECOMPILE)

                    END
                END
",
                    name.Replace( "'", "''" ),
                    friendlyName.Replace( "'", "''" ),
                    assemblyName.Replace( "'", "''" ),
                    isEntity ? "1" : "0",
                    isSecured ? "1" : "0",
                    guid ) );
        }
    }
}