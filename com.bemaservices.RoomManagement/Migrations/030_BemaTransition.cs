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
    [MigrationNumber( 30, "1.9.4" )]
    public class BemaTransition : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            try
            {
                UpdateTablesV56();
            }
            catch ( Exception ex )
            {
                try
                {
                    UpdateTablesV55();
                }
                catch ( Exception ex1 )
                {
                    UpdateTablesV41();
                }

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
            }

            Sql( @"
                Update AttributeValue
                Set Value = Replace(Value, '_centralaz_RoomManagement', '_bemaservices_RoomManagement')
                Where Value like '%_centralaz_RoomManagement%'

                Update AttributeValue
                Set Value = Replace(Value, 'centralaz.RoomManagement', 'bemaservices.RoomManagement')
                Where Value like '%centralaz.RoomManagement%'

                Update Page
                Set [Order] = 2
                Where Guid = 'CFF84B6D-C852-4FC4-B602-9F045EDC8854'
            " );

        }

        private void UpdateTablesV56()
        {
            Sql( @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
BEGIN
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId])
	Select Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId]
	From [dbo].[_com_centralaz_RoomManagement_Resource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_Question]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime])
	Select Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime]
	From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType](Id,[ReservationTypeId]
           ,[LocationTypeValueId]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[ReservationTypeId]
           ,[LocationTypeValueId]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId])
	Select Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId]
	From [dbo].[_com_centralaz_RoomManagement_Reservation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState])
	Select Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
	From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId])
	Select Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
END


" );
        }

        private void UpdateTablesV55()
        {
            Sql( @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
BEGIN
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
    Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[IsSystem]
           ,[LocationId]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IsDefault]
           ,[LayoutPhotoId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId])
	Select Id,[Name]
           ,[CategoryId]
           ,[CampusId]
           ,[Quantity]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalGroupId]
           ,[LocationId]
	From [dbo].[_com_centralaz_RoomManagement_Resource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[LocationId]
           ,[ResourceId]
           ,[AttributeId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_Question]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime])
	Select Id,[IsSystem]
           ,[Name]
           ,[Description]
           ,[IsActive]
           ,[IconCssClass]
           ,[FinalApprovalGroupId]
           ,[SuperAdminGroupId]
           ,[NotificationEmailId]
           ,[DefaultSetupTime]
           ,[IsCommunicationHistorySaved]
           ,[IsNumberAttendingRequired]
           ,[IsContactDetailsRequired]
           ,[IsSetupTimeRequired]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[IsReservationBookedOnApproval]
           ,[DefaultCleanupTime]
	From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[WorkflowTypeId]
           ,[TriggerType]
           ,[QualifierValue]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId])
	Select Id,[Name]
           ,[Description]
           ,[Order]
           ,[IsActive]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ReservationTypeId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId])
	Select Id,[Name]
           ,[ScheduleId]
           ,[CampusId]
           ,[ReservationMinistryId]
           ,[ReservationStatusId]
           ,[RequesterAliasId]
           ,[ApproverAliasId]
           ,[SetupTime]
           ,[CleanupTime]
           ,[NumberAttending]
           ,[Note]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[SetupPhotoId]
           ,[EventContactPersonAliasId]
           ,[EventContactPhone]
           ,[EventContactEmail]
           ,[AdministrativeContactPersonAliasId]
           ,[AdministrativeContactPhone]
           ,[AdministrativeContactEmail]
           ,[ReservationTypeId]
           ,[FirstOccurrenceStartDateTime]
           ,[LastOccurrenceEndDateTime]
           ,[EventItemOccurrenceId]
	From [dbo].[_com_centralaz_RoomManagement_Reservation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState])
	Select Id,[ReservationId]
           ,[ResourceId]
           ,[Quantity]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
	From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId])
	Select Id,[ReservationId]
           ,[LocationId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[ApprovalState]
           ,[LocationLayoutId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	Insert
	Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId])
	Select Id,[ReservationId]
           ,[ReservationWorkflowTriggerId]
           ,[WorkflowId]
           ,[TriggerType]
           ,[TriggerQualifier]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[Guid]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
	From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
END
" );
        }

        private void UpdateTablesV41()
        {
            Sql( @"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME like '_com_centralaz_RoomManagement%'))
            BEGIN
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationResource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Reservation]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
                Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_ReservationType]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Question]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_Resource]
	            Delete From [dbo].[_com_bemaservices_RoomManagement_LocationLayout]

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_LocationLayout](Id,[IsSystem]
                       ,[LocationId]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IsDefault]
                       ,[LayoutPhotoId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[IsSystem]
                       ,[LocationId]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IsDefault]
                       ,[LayoutPhotoId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_LocationLayout]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_LocationLayout] OFF;

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Resource](Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId])
	            Select Id,[Name]
                       ,[CategoryId]
                       ,[CampusId]
                       ,[Quantity]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalGroupId]
                       ,[LocationId]
	            From [dbo].[_com_centralaz_RoomManagement_Resource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Resource] OFF;

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Question](Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[LocationId]
                       ,[ResourceId]
                       ,[AttributeId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_Question]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Question] OFF;  	

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationType](Id,[IsSystem]
                       ,[Name]
                       ,[Description]
                       ,[IsActive]
                       ,[IconCssClass]
                       ,[FinalApprovalGroupId]
                       ,[SuperAdminGroupId]
                       ,[NotificationEmailId]
                       ,[DefaultSetupTime]
                       ,[IsCommunicationHistorySaved]
                       ,[IsNumberAttendingRequired]
                       ,[IsContactDetailsRequired]
                       ,[IsSetupTimeRequired]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[IsSystem]
                           ,[Name]
                           ,[Description]
                           ,[IsActive]
                           ,[IconCssClass]
                           ,[FinalApprovalGroupId]
                           ,[SuperAdminGroupId]
                           ,[NotificationEmailId]
                           ,[DefaultSetupTime]
                           ,[IsCommunicationHistorySaved]
                           ,[IsNumberAttendingRequired]
                           ,[IsContactDetailsRequired]
                           ,[IsSetupTimeRequired]
                           ,[Guid]
                           ,[CreatedDateTime]
                           ,[ModifiedDateTime]
                           ,[CreatedByPersonAliasId]
                           ,[ModifiedByPersonAliasId]
                           ,[ForeignKey]
                           ,[ForeignGuid]
                           ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationType]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationType] OFF;  

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId])
	            Select Id,[WorkflowTypeId]
                       ,[TriggerType]
                       ,[QualifierValue]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                       ,[Description]
                       ,[Order]
                       ,[IsActive]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_Reservation](Id,[Name]
                       ,[ScheduleId]
                       ,[CampusId]
                       ,[ReservationMinistryId]
                       ,[ReservationStatusId]
                       ,[RequesterAliasId]
                       ,[ApproverAliasId]
                       ,[SetupTime]
                       ,[CleanupTime]
                       ,[NumberAttending]
                       ,[Note]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[SetupPhotoId]
                       ,[EventContactPersonAliasId]
                       ,[EventContactPhone]
                       ,[EventContactEmail]
                       ,[AdministrativeContactPersonAliasId]
                       ,[AdministrativeContactPhone]
                       ,[AdministrativeContactEmail]
                       ,[ReservationTypeId])
	            Select Id,[Name]
                        ,[ScheduleId]
                        ,[CampusId]
                        ,[ReservationMinistryId]
                        ,[ReservationStatusId]
                        ,[RequesterAliasId]
                        ,[ApproverAliasId]
                        ,[SetupTime]
                        ,[CleanupTime]
                        ,[NumberAttending]
                        ,[Note]
                        ,[Guid]
                        ,[CreatedDateTime]
                        ,[ModifiedDateTime]
                        ,[CreatedByPersonAliasId]
                        ,[ModifiedByPersonAliasId]
                        ,[ForeignKey]
                        ,[ForeignGuid]
                        ,[ForeignId]
                        ,[ApprovalState]
                        ,[SetupPhotoId]
                        ,[EventContactPersonAliasId]
                        ,[EventContactPhone]
                        ,[EventContactEmail]
                        ,[AdministrativeContactPersonAliasId]
                        ,[AdministrativeContactPhone]
                        ,[AdministrativeContactEmail]
                        ,[ReservationTypeId]
	            From [dbo].[_com_centralaz_RoomManagement_Reservation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_Reservation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationResource](Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState])
	            Select Id,[ReservationId]
                       ,[ResourceId]
                       ,[Quantity]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationResource]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationResource] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[LocationLayoutId])
	            Select Id,[ReservationId]
                       ,[LocationId]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
                       ,[ApprovalState]
                       ,[LocationLayoutId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationLocation]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] OFF; 

	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ON;  
	            Insert
	            Into [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId])
	            Select Id,[ReservationId]
                       ,[ReservationWorkflowTriggerId]
                       ,[WorkflowId]
                       ,[TriggerType]
                       ,[TriggerQualifier]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[Guid]
                       ,[ForeignKey]
                       ,[ForeignGuid]
                       ,[ForeignId]
	            From [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]
	            SET IDENTITY_INSERT [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] OFF; 
	
            END
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}