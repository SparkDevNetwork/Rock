using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Accountability.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class AccountabilityRollup : Migration
    {
        //
        public override void Up()
        {
            RockMigrationHelper.DeleteAttribute( "4E1C2BA2-52E3-4777-88ED-255DF39D3DFE" );
            RockMigrationHelper.DeleteAttribute( "01B835A6-7040-4111-B7C7-5D396A08BCA7" );
            RockMigrationHelper.DeleteAttribute( "04B74B24-274E-43D4-9148-5E432390014A" );
        }

        public override void Down()
        {

        }
    }
}