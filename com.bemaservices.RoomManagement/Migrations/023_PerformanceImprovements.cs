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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration to provide performance improvements to the Room Management's Reservations
    /// by running a nightly job to populate the FirstOccurrenceStartDateTime and LastOccurrenceEndDateTime
    /// fields on the Reservation table.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 23, "1.9.4" )]
    public class PerformanceImprovements : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [FirstOccurrenceStartDateTime] [datetime] NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [LastOccurrenceEndDateTime] [datetime] NULL
                " );

            // Job for Populating FirstOccurrenceStartDateTime and LastOccurrenceEndDateTime fields (schedule for 9pm to avoid conflict with AppPoolRecycle)
            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.bemaservices.RoomManagement.Jobs.PopulateFirstLastOccurrenceDateTimes' AND [Guid] = '{ SystemGuid.ServiceJob.POPULATE_FIRST_LAST_OCCURRENCE_DATETIMES }' )
              BEGIN
               IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.centralaz.RoomManagement.Jobs.PopulateFirstLastOccurrenceDateTimes' AND [Guid] = '{ SystemGuid.ServiceJob.POPULATE_FIRST_LAST_OCCURRENCE_DATETIMES }' )
                BEGIN
                    INSERT INTO [dbo].[ServiceJob]
                           ([IsSystem]
                           ,[IsActive]
                           ,[Name]
                           ,[Description]
                           ,[Class]
                           ,[CronExpression]
                           ,[NotificationStatus]
                           ,[Guid])
                     VALUES
                        (0	
                         ,1	
                         ,'Populate FirstOccurrenceStartDateTime and LastOccurrenceEndDateTime fields on the Reservation table.'
                         ,'Populates FirstOccurrenceStartDateTime and LastOccurrenceEndDateTime fields on the Reservation table. Once all data has been populated, the job will remove itself.'
                         ,'com.bemaservices.RoomManagement.Jobs.PopulateFirstLastOccurrenceDateTimes'
                         ,'0 0 21 1/1 * ? *'
                         ,3
                         ,'{ SystemGuid.ServiceJob.POPULATE_FIRST_LAST_OCCURRENCE_DATETIMES }')
                END
                ELSE
                BEGIN
                   UPDATE [ServiceJob]
                   SET [Class]= 'com.bemaservices.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange'
                   WHERE Guid = '6832E24B-5650-41D3-9EBA-1D2D213F768C'
                END
            END" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN [FirstOccurrenceStartDateTime]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN [LastOccurrenceEndDateTime]
                " );
        }
    }
}
