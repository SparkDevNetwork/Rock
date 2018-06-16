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
    public partial class HomepageMetricSchedule : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180507T010001
DTSTAMP:20180509T164128Z
DTSTART:20180507T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:34d27dff-e110-47b0-9147-bebd312aae67
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-07' ,'2018-05-07' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , '9CDB85EC-CF0D-4AC2-81C0-266A7DBAFA06', NULL ,NULL ,NULL ,NULL ,NULL ,1 )" );

            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180508T010001
DTSTAMP:20180509T164716Z
DTSTART:20180508T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:72042074-e62b-4403-a999-2853d80aabba
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-08' ,'2018-05-08' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , '717D75F1-644F-45A4-B25E-64652A270AD9', NULL ,NULL ,NULL ,NULL ,NULL ,1 )
" );

            Sql( @"
INSERT INTO [dbo].[Schedule]
    ([Name]
    ,[Description]
    ,[iCalendarContent]
    ,[CheckInStartOffsetMinutes]
    ,[CheckInEndOffsetMinutes]
    ,[EffectiveStartDate]
    ,[EffectiveEndDate]
    ,[CategoryId]
    ,[Guid]
    ,[ForeignKey]
    ,[WeeklyDayOfWeek]
    ,[WeeklyTimeOfDay]
    ,[ForeignGuid]
    ,[ForeignId]
    ,[IsActive])
VALUES
('' ,NULL ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180508T010001
DTSTAMP:20180509T165611Z
DTSTART:20180508T010000
RRULE:FREQ=DAILY
SEQUENCE:0
UID:649969c3-118c-4021-a4b4-c47212d4f357
END:VEVENT
END:VCALENDAR
' ,NULL ,NULL ,'2018-05-08' ,'2018-05-08' ,( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '5A794741-5444-43F0-90D7-48E47276D426' ) , 'C376AF3C-5356-43BE-ACAC-3846D490CF08', NULL ,NULL ,NULL ,NULL ,NULL ,1 )
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = '9CDB85EC-CF0D-4AC2-81C0-266A7DBAFA06' )
WHERE [Guid] = 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = '717D75F1-644F-45A4-B25E-64652A270AD9' )
WHERE [Guid] = '491061b7-1834-44da-8ea1-bb73b2d52ad3'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET
[ScheduleId] = ( SELECT TOP 1 [Id] FROM [dbo].[Schedule] WHERE [Guid] = 'C376AF3C-5356-43BE-ACAC-3846D490CF08' )
WHERE [Guid] = 'f0a24208-f8ac-4e04-8309-1a276885f6a6'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

        }
    }
}
