using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.Utility.Migrations
{
    [MigrationNumber( 1, "1.0.8" )]
    public class AddAutoAssignNumberBlock : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Auto Assign Number", "Automatically increments and assigns the next number for a configured person attribute.", "~/Plugins/com_centralaz/Utility/AutoAssignNumber.ascx", "com_centralaz > Utility", "84D8D97A-8FC3-43CD-8AA0-E56A064B760E " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType( "84D8D97A-8FC3-43CD-8AA0-E56A064B760E" ); // Auto Assign Number
        }
    }
}
