using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.DoorControl.DSX.Migrations
{
    [MigrationNumber( 7, "1.8.0" )]
    public class DoorLockUpdates : Migration
    {
        public override void Up()
        {
            // Deletes the Door Lock if the reservation or location is removed
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Reservation]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Reservation] ([Id])
                ON DELETE CASCADE
                
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Location]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])
                ON DELETE CASCADE
                
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Location]
" );

            var addScheduleColumn = string.Format( @"
                -- Getting a few Ids
                DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
                DECLARE @ScheduleBuilderTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'com.bemaservices.WorkflowExtensions.Field.Types.ScheduleBuilderFieldType');

                -- Checking if AttributeMatrixTemplate exists
                DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('{0}' AS uniqueidentifier))
                
                DECLARE @CreatedId INT

                -- Checking if Start DateTime Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start DateTime Attribute
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @ScheduleBuilderTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'Schedule', 
		                'Schedule', 
		                'The Schedule of the Override', 
		                0,
		                0, 
		                0, 
		                1, 
		                '{1}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
                END",
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTEMATRIX,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_RESERVATION_SCHEDULE
            );

                        Sql( addScheduleColumn );

        }

        public override void Down()
        {

        }
    }
}
