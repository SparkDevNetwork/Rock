using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.MailChimp.SystemGuid;

namespace com.bemaservices.MailChimp.Migrations
{
    [MigrationNumber( 2, "1.9.4" )]
    public class UpdateCronExpression : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Add Job
            Sql( string.Format( @"
			    UPDATE ServiceJob
			    SET CronExpression = '0 0 1 1/1 * ? *'
				WHERE Guid = 'A7EF4133-6616-4CF1-AD44-6F8DBE4EA46C'
            " ) );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
           
        }
    }
}
