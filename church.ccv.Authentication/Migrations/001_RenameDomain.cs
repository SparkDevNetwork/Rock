using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Authentication.Migrations
{
    [MigrationNumber( 1, "1.2.0" )]
    class RenameDomain : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [EntityType] 
    WHERE [Name] LIKE 'church.ccv.Authentication%'

    UPDATE [EntityType] SET 
	    [Name] = REPLACE([Name],'com.ccvonline.Authentication', 'church.ccv.Authentication'),
	    [AssemblyName] = REPLACE([Name],'com.ccvonline.Authentication', 'church.ccv.Authentication')
    WHERE [Name] LIKE 'com.ccvonline.Authentication%'
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
