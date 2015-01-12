using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Hr.Migrations
{
    [MigrationNumber( 2, "1.2.0" )]
    public class AlterColumns : Migration
    {
        public override void Up()
        {
            Sql(@"
alter table [_com_ccvonline_TimeCard_TimeCardDay] drop column [TotalWorkedDuration]
alter table [_com_ccvonline_TimeCard_TimeCardDay] add [TotalWorkedDuration]
as (DATEDIFF(MINUTE, StartDateTime, isnull(EndDateTime, LunchStartDateTime)) / 60.00) - isnull((DATEDIFF(MINUTE, LunchStartDateTime, LunchEndDateTime) / 60.00), 0) persisted");
        }

        public override void Down()
        {
            //throw new NotImplementedException();
        }
    }
}
