using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.Reporting.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    class RemoveExtraServiceTimeSchedules : Migration
    {
        public override void Up()
        {
            // Remove built-in Saturday 6:00pm Schedule
            RockMigrationHelper.DeleteByGuid( "33FF69E9-059B-4702-B1E5-4D499CB7B07A", "Schedule" );
            // Remove built-in Sunday 10:30am Schedule
            RockMigrationHelper.DeleteByGuid( "4628D917-EC9C-4269-B39C-5BCBEF3658E7", "Schedule" );
        }

        public override void Down()
        {
            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Saturday 6:00pm", "", @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T190000
DTSTART:20130501T180000
RRULE:FREQ=WEEKLY;BYDAY=SA
END:VEVENT
END:VCALENDAR
", 30, 30, "2013-05-01", null, "33FF69E9-059B-4702-B1E5-4D499CB7B07A", true );

            UpdateSchedule( "4FECC91B-83F9-4269-AE03-A006F401C47E", "Sunday 10:30am", "", @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T113000
DTSTART:20130501T103000
RRULE:FREQ=WEEKLY;BYDAY=SU
END:VEVENT
END:VCALENDAR
", 30, 30, "2013-05-01", null, "4628D917-EC9C-4269-B39C-5BCBEF3658E7", true );
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
