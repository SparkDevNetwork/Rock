using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace Rock.Migrations.HotFixMigrations
{
    [MigrationNumber( 7, "1.6.2" )]
    public class Fundraising : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // NOTE: This migration was added to core in v7.0 ( see 201704031947125_Fundraising )
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // NOTE: This migration was added to core in v7.0 ( see 201704031947125_Fundraising )
        }
    }
}
