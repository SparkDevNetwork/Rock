using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.ccvonline.CommandCenter.Migrations
{
    [MigrationNumber( 1, "1.0.8" )]
    public class FixNamespaceCase : Migration
    {
        public override void Up()
        {
            Sql( @"
    UPDATE [BlockType] SET [Path] = REPLACE( [Path], 'org_CcvOnline', 'org_ccvonline' )
    UPDATE [EntityType] SET 
	    [Name] = REPLACE( [Name], 'ccvOnline', 'ccvonline' ),
	    [AssemblyName] = REPLACE( [AssemblyName], 'ccvOnline', 'ccvonline' )
" );
        }

        public override void Down()
        {
        }
    }
}
