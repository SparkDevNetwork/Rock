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
    [MigrationNumber( 4, "1.0.14" )]
    public class WeekendAttendanceCheckinConfig : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Weekend Gathering", "Holds all the campus weekend gathering groups for attendance purposes.", "Group", "Member", false, true, true, null, 0, null, 0, "4A406CB0-495B-4795-B788-52BDFDE00B01", "85FAEE00-42F3-415E-B921-86712E855B85" );
            RockMigrationHelper.AddGroupType( "Weekend Gathering", "", "Group", "Member", true, true, false, null, 0, "6E7AD783-7614-4721-ABC1-35842113EF59", 2, null, "531320FA-9BD1-494A-B512-EDBA1262B93D" );

            int parentGroupTypeId = SqlScalar( "Select Top 1 Id From GroupType Where Guid = '85FAEE00-42F3-415E-B921-86712E855B85'" ).ToString().AsInteger();
            int childGroupTypeId = SqlScalar( "Select Top 1 Id From GroupType Where Guid = '531320FA-9BD1-494A-B512-EDBA1262B93D'" ).ToString().AsInteger();
            Sql( String.Format( @"INSERT INTO [dbo].[GroupTypeAssociation]
                       ([GroupTypeId]
                       ,[ChildGroupTypeId] )
                 VALUES
                       ({0}
                       ,{1})", parentGroupTypeId, childGroupTypeId ) );

            Sql( @" Update GroupType
                    Set TakesAttendance = 1,
                        AttendancePrintTo = 1
                    Where Guid = '531320FA-9BD1-494A-B512-EDBA1262B93D'" );

            RockMigrationHelper.AddGroupTypeRole( "531320FA-9BD1-494A-B512-EDBA1262B93D", "Member", "", 0, null, null, "A1A6E1F5-3F13-42BC-9244-C267041E8968", false, false, true );

            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Saturday 4:30pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180616T173500
DTSTAMP:20180618T202547Z
DTSTART:20180616T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:d381f1c5-49e9-4031-a430-de6fd294f787
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "7883CAC8-6E30-482B-95A7-2F0DEE859BE1", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 9:00am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T100500
DTSTAMP:20180618T204008Z
DTSTART:20180617T090000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:688cb659-508c-48b1-8a5f-a8aac4f5517c
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "FF6FB240-0C32-4542-BE40-159C522F7E51", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 12:15pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T132000
DTSTAMP:20180618T205704Z
DTSTART:20180617T121500
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:9c0ada46-2623-46e7-98cd-49b8cb739021
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "4:30 (test)", "", @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR", 0, 1439, "2018-06-16", "", "A5C81078-EB8C-46AA-BB91-1E2BA8BA76AE", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "6:00 (test)", "", @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR", 0, 1439, "2018-06-16", "", "C8B7BEB4-54E2-4473-822F-F5D0F8CE19D7", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 4:00pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T170500
DTSTAMP:20180618T205725Z
DTSTART:20180617T160000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:4cb0e5d3-99ee-44f9-bf14-e6722051d054
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "6A4EF8A8-E57A-472D-A3A7-2488807BE9F5", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 10:45am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T115000
DTSTAMP:20180618T204025Z
DTSTART:20180617T104500
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:aef67a6e-30d5-46e1-a9cd-fb0238b6d6be
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "2D558B3F-98F6-4150-A986-3FDF5F5BFB77", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Monday 6:30pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180618T193500
DTSTAMP:20180618T210145Z
DTSTART:20180618T183000
RRULE:FREQ=WEEKLY;BYDAY=MO
SEQUENCE:0
UID:3dcb4806-5fb9-483a-8ba8-0ca8638c17d2
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "B22DCCD8-E97F-41DF-8997-CDE6856628EE", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Saturday 6:15pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180616T192000
DTSTAMP:20180618T203744Z
DTSTART:20180616T181500
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:652a42ec-6e7b-4e2d-827b-7750ce9fe789
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "BF9D66B9-F271-4D20-9BD4-253AE316F71C", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 9:30am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T103500
DTSTAMP:20180618T203844Z
DTSTART:20180617T093000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:3f0349ff-1515-4e73-913a-0853e43067b2
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "0C72F278-A517-4579-A03F-6797DC83584C", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 11:00am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T120500
DTSTAMP:20180618T210058Z
DTSTART:20180617T110000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:7a9dbca7-b2de-46d1-9bd9-3d3df6393364
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "EF3746FD-7C2D-4EF5-B710-FCA38A2D971B", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 8:30am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T093500
DTSTAMP:20180618T210119Z
DTSTART:20180617T083000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:22b22f2d-3d50-4aa4-b6ec-9eb6107ec6c6
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "43BACF08-8C76-4B18-8E19-479F78376C87", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 10:00am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T110500
DTSTAMP:20180618T210136Z
DTSTART:20180617T100000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:f705a1ba-9279-462e-b2cd-ee5193865b18
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "D48C45CC-3710-42E8-9ECE-A6DA7AA7B144", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 11:30am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T123500
DTSTAMP:20180618T205920Z
DTSTART:20180617T113000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:d7f4a1ad-0699-42bd-af9b-06799690d973
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "00FE463F-AED3-4EAB-8B8F-B0825E2FBC2F", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 1:00pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T140500
DTSTAMP:20180618T205932Z
DTSTART:20180617T130000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:12dfb5b9-103b-4bc4-a211-ea2f11c248e0
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "6060D0A0-BD38-4FCF-AB22-F3E3EDC08D98", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 5:30pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T183500
DTSTAMP:20180618T205816Z
DTSTART:20180617T173000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:7b4bb8f6-6ca9-4e86-9066-612bddf5fa9e
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "581BAA9E-164B-4708-90D5-E8A97A2BD222", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 9:15am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T102000
DTSTAMP:20180618T210040Z
DTSTART:20180617T091500
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:182387b1-f957-4c63-b424-2e8ae75e5517
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "B3AD30DE-397E-4E29-9D45-AF431E7FE4EA", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 12:30pm", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20180617T133500
DTSTAMP:20180618T210553Z
DTSTART:20180617T123000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:3bc9780d-0489-41ef-8d65-7d26a15857de
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "DE81EED0-B0D4-454E-93FB-9A16E540BE1B", true );
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 10:30am", "", @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20181209T103001
DTSTAMP:20181205T174949Z
DTSTART:20181209T103000
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:6095c35f-9750-4a2f-a921-e7a69d406cfd
END:VEVENT
END:VCALENDAR
", 20, 20, "2018-06-16", "2018-06-16", "8B182857-3529-4EA2-A375-B132132CB7F3", true );

        }
        public override void Down()
        {
        }

        public void UpdateSchedule( string categoryGuid, string name, string description, string iCalendarContent, int? checkInStartOffsetMinutes, int? checkInEndOffsetMinutes, string effectiveStartDate, string effectiveEndDate,
            string guid, bool isActive = true )
        {
            Sql( string.Format( @"

                -- Update or insert a group...

                DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = {1} )
                IF EXISTS (
                    SELECT [Id]
                    FROM [Schedule]
                    WHERE [Guid] = '{0}' )
                BEGIN
                    UPDATE [Schedule]
                        SET [Name] = '{2}'
                              ,[Description] = '{3}'
                              ,[iCalendarContent] = '{4}'
                              ,[CheckInStartOffsetMinutes] = {5}
                              ,[CheckInEndOffsetMinutes] = {6}
                              ,[EffectiveStartDate] = '{7}'
                              ,[EffectiveEndDate] = '{8}'
                              ,[CategoryId] = @CategoryId
                              ,[IsActive] = {9}
                    WHERE [Guid] = '{0}'
                END
                ELSE
                BEGIN
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
                       ,[IsActive])
                 VALUES
                       ('{2}'
                       ,'{3}'
                       ,'{4}'
                       ,{5}
                       ,{6}
                       ,'{7}'
                       ,'{8}'
                       ,@CategoryId
                       ,'{0}'
                       ,{9})
                END
",
                    guid,
                    ( categoryGuid == null ) ? "NULL" : "'" + categoryGuid + "'",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    iCalendarContent.Replace( "'", "''" ),
                    ( checkInStartOffsetMinutes == null ) ? "NULL" : checkInStartOffsetMinutes.ToString(),
                    ( checkInEndOffsetMinutes == null ) ? "NULL" : checkInEndOffsetMinutes.ToString(),
                    effectiveStartDate.Replace( "'", "''" ),
                    effectiveEndDate.Replace( "'", "''" ),
                    ( isActive ? "1" : "0" )
            ) );
        }
    }
}
