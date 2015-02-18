using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Hr.Migrations
{
    [MigrationNumber( 3, "1.2.0" )]
    public class AddHolidaySchedule : Migration
    {
        public override void Up()
        {
            Sql( @"
INSERT INTO [dbo].[Schedule]
           ([Name]
           ,[Description]
           ,[iCalendarContent]
           ,[EffectiveStartDate]
           ,[EffectiveEndDate]
           ,[CategoryId]
           ,[Guid]
            )
     VALUES
           ('TimeCard Holidays'
           ,'The Dates that should be considered as Holidays for TimeCards. Hours worked on these days will treated as holiday overtime hours.'
           ,'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20140101T010000
DTSTART:20140101T000000
END:VEVENT
END:VCALENDAR
'
           ,'2014-01-01'
           ,null
           ,null
           ,'C0893061-75D7-4A4E-BB8F-39348B90B84F'
           )
" );
        }

        public override void Down()
        {
            //
        }
    }
}
