// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdatePersonalizationSegments : Rock.Migrations.RockMigration
    {
        public const string SCHEDULE_PERSONALIZATION_SEGMENTS = "568A8E6C-DD49-4AC8-84BB-799099EFD9C6";
        public const string NIGHTLY_SCHEDULE_GUID = "26708BFB-B645-4C59-848B-E64A2C4BD5B8";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdatePersonalizationSegmentModelUp();
            AddPersonalizationSegmentScheduleCategoryUp();
            UpdatePersonalizationJobCronUp();
            SetAllPersonalizationSegmentsToNightlyScheduleUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SetAllPersonalizationSegmentsToNoScheduleDown();
            UpdatePersonalizationJobCronDown();
            AddPersonalizationSegmentScheduleCategoryDown();
            UpdatePersonalizationSegmentModelDown();
        }

        #region Up Methods
        private void UpdatePersonalizationSegmentModelUp()
        {
            AddColumn( "dbo.PersonalizationSegment", "PersistedScheduleId", c => c.Int() );
            AddColumn( "dbo.PersonalizationSegment", "PersistedScheduleIntervalMinutes", c => c.Int() );
            AddColumn( "dbo.PersonalizationSegment", "PersistedLastRefreshDateTime", c => c.DateTime() );
            AddColumn( "dbo.PersonalizationSegment", "PersistedLastRunDurationMilliseconds", c => c.Int() );
            AddForeignKey( "dbo.PersonalizationSegment", "PersistedScheduleId", "dbo.Schedule", "Id" );
        }

        private void AddPersonalizationSegmentScheduleCategoryUp()
        {
            // New category for Personalization Segment Schedules
            RockMigrationHelper.UpdateCategory(
                SystemGuid.EntityType.SCHEDULE,
                "Personalization Segment Schedules",
                "",
                "",
                SCHEDULE_PERSONALIZATION_SEGMENTS
            );

            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT TOP 1 [Id] 
    FROM [Schedule] 
    WHERE [Guid] = '{NIGHTLY_SCHEDULE_GUID}');

DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '{SCHEDULE_PERSONALIZATION_SEGMENTS}');
IF @ScheduleId IS NULL AND @CategoryId IS NOT NULL
BEGIN
    INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive], [IsPublic])
    VALUES (N'Nightly', NULL, N'BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20250716T010001
DTSTAMP:20250716T010100
DTSTART:20250716T010000
RRULE:FREQ=DAILY;BYHOUR=1;BYMINUTE=0;BYSECOND=0
SEQUENCE:0
UID:6DF642D5-A35A-4F60-9C89-9EA6B42BDD35
END:VEVENT
END:VCALENDAR',
        @CategoryId, N'{NIGHTLY_SCHEDULE_GUID}', 1, 0)
END" );
        }

        private void UpdatePersonalizationJobCronUp()
        {
            // Set the cron expression to run every minute
            Sql( @"UPDATE [ServiceJob]
SET [CronExpression] = '0 0/1 * 1/1 * ? *'
WHERE [Guid] = '67CFE1FE-7C64-4328-8576-F1A4BFD0EA8B'" );
        }

        private void SetAllPersonalizationSegmentsToNightlyScheduleUp()
        {
            Sql( $@"
                DECLARE @NightlyScheduleId INT = (SELECT TOP 1 [Id] FROM [Schedule] WHERE [Guid] = '{NIGHTLY_SCHEDULE_GUID}');
                UPDATE [PersonalizationSegment]
                SET [PersistedScheduleId] = @NightlyScheduleId,
                    [PersistedScheduleIntervalMinutes] = NULL
                WHERE [PersistedScheduleId] IS NULL
            " );
        }

        #endregion

        #region Down Methods
        private void UpdatePersonalizationSegmentModelDown()
        {
            DropForeignKey( "dbo.PersonalizationSegment", "PersistedScheduleId", "dbo.Schedule" );
            DropColumn( "dbo.PersonalizationSegment", "PersistedLastRunDurationMilliseconds" );
            DropColumn( "dbo.PersonalizationSegment", "PersistedLastRefreshDateTime" );
            DropColumn( "dbo.PersonalizationSegment", "PersistedScheduleIntervalMinutes" );
            DropColumn( "dbo.PersonalizationSegment", "PersistedScheduleId" );
        }

        private void AddPersonalizationSegmentScheduleCategoryDown()
        {
            Sql( $"DELETE FROM [Schedule] WHERE [Guid] = '{NIGHTLY_SCHEDULE_GUID}'" );
            RockMigrationHelper.DeleteCategory( SCHEDULE_PERSONALIZATION_SEGMENTS );
        }

        private void UpdatePersonalizationJobCronDown()
        {
            // Optionally, set it back to once per day at 1:20am (or leave as is)
            Sql( @"UPDATE [ServiceJob]
SET [CronExpression] = '0 20 1 1/1 * ? *'
WHERE [Guid] = '67CFE1FE-7C64-4328-8576-F1A4BFD0EA8B'" );
        }

        private void SetAllPersonalizationSegmentsToNoScheduleDown()
        {
            Sql( $@"
                DECLARE @NightlyScheduleId INT = (SELECT TOP 1 [Id] FROM [Schedule] WHERE [Guid] = '{NIGHTLY_SCHEDULE_GUID}');
                UPDATE [PersonalizationSegment]
                SET [PersistedScheduleId] = NULL
                WHERE [PersistedScheduleId] = @NightlyScheduleId
            " );
        }

        #endregion
    }
}