using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 7, "1.0.14" )]
    public class BaptismCleanup : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteAttribute( "DC2EDBFD-15BD-4A1C-B380-F9361844A295" );
            RockMigrationHelper.DeleteAttribute( "CB638267-0065-4FFC-A665-BCE475BD022D" );
            RockMigrationHelper.AddBlockTypeAttribute( "697B2414-42CE-4093-9546-DAF26E9B34CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Blackout Date Page", "AddBlackoutDatePage", "", "", 0, @"", "CB638267-0065-4FFC-A665-BCE475BD022D" );


        }

        public override void Down()
        {

        }
    }
}