using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.ccvonline.CommandCenter.Migrations
{
    [MigrationNumber( 3, "1.0.9" )]
    class AddVenueType : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                    ALTER TABLE dbo._com_ccvonline_CommandCenter_Recording
                    ADD VenueType VARCHAR(50) NULL
                " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
                    ALTER TABLE dbo._com_ccvonline_CommandCenter_Recording
                    DROP COLUMN VenueType 
                " );
        }
    }
}
