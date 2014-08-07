using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.ccvonline.CommandCenter.Migrations
{
    [MigrationNumber( 3, "1.0.10" )]
    class AddVenue : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql(@"
                    ALTER TABLE dbo._com_ccvonline_CommandCenter_Recording
                    ADD Venue VARCHAR(50) NULL

                    UPDATE _com_ccvonline_CommandCenter_Recording
                    SET Venue = 'Command Center'
                    WHERE StreamName not like 'WR%'

                    UPDATE _com_ccvonline_CommandCenter_Recording
                    SET Venue = 'War Room'
                    WHERE StreamName like 'WR%'
                ");
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
                    ALTER TABLE dbo._com_ccvonline_CommandCenter_Recording
                    DROP COLUMN Venue
                " );
        }
    }
}
