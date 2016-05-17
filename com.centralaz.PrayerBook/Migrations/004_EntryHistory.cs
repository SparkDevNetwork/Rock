using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Constants;

namespace com.centralaz.Prayerbook.Migrations
{
    [MigrationNumber( 4, "1.4.5" )]
    public class EntryHistory : Migration
    {
        public override void Up()
        {
            // Add history categories
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Prayer", "", "", com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_PRAYER );
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Up Team Entry", "", "", com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_UPTEAM_ENTRY );
        }

        public override void Down()
        {
            // Delete the categories
            RockMigrationHelper.DeleteCategory( com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_UPTEAM_ENTRY );
            RockMigrationHelper.DeleteCategory( com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_PRAYER );
        }
    }
}
