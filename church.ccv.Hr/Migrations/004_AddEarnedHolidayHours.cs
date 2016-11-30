using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Hr.Migrations
{
    [MigrationNumber( 4, "1.2.0" )]
    public class AddEarnedHolidayHours : Migration
    {
        public override void Up()
        {
            Sql( "ALTER TABLE [dbo].[_com_ccvonline_Hr_TimeCardDay] add [EarnedHolidayHours] [decimal](18, 2) null" );
            
            // changes TimeCardHistory notes to have max length
            Sql( "ALTER TABLE [dbo].[_com_ccvonline_Hr_TimeCardHistory] ALTER COLUMN [Notes] NVARCHAR(MAX)" );
        }

        public override void Down()
        {
            //
        }
    }
}
