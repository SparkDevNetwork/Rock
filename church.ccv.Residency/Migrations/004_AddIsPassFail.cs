using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Residency.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 4, "1.0.8" )]
    public class AddIsPassFail : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"alter table [_com_ccvonline_Residency_ProjectPointOfAssessment] add [IsPassFail] bit not null default 0" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
