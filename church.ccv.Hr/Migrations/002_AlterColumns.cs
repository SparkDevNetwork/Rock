using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Hr.Migrations
{
    [MigrationNumber( 2, "1.2.0" )]
    public class AlterColumns : Migration
    {
        public override void Up()
        {
            Sql( @"
alter table [_com_ccvonline_Hr_TimeCardDay] drop column [TotalWorkedDuration]
alter table [_com_ccvonline_Hr_TimeCardDay] add [TotalWorkedDuration]
as
(
case when (EndDateTime is null)
then 
 case when (LunchStartDateTime is null)
 then 
    -- No EndTime and no LunchStart entered yet    
    null
 else
    -- No EndTime, but they did punch out for lunch
    DATEDIFF(MINUTE, StartDateTime, LunchStartDateTime) / 60.00
 end
else
 case when (LunchStartDateTime is null or LunchEndDateTime is null)
 then 
    --They entered an EndDateTime, but didn't fill out lunch, so don't subtract lunch
    DATEDIFF(MINUTE, StartDateTime, EndDateTime) / 60.00
 else
   -- The entered an EndDateTime, and punched Out and In for Lunch, so subtract lunch
    (DATEDIFF(MINUTE, StartDateTime, EndDateTime) / 60.00) - (DATEDIFF(MINUTE, LunchStartDateTime, LunchEndDateTime) / 60.00)
 end
end
) persisted" );
        }

        public override void Down()
        {
            //throw new NotImplementedException();
        }
    }
}
