using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 44, "1.7.0" )]
    public class EnsureCommunicationMigration : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // There was a bug in one of the v7.0 Rock Updates that might have prevented UpdateCommunicationRecords, so make sure that UpdateCommunicationRecords ran.  
            Rock.Jobs.MigrateCommunicationMediumData.UpdateCommunicationRecords( true, 50, null );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
