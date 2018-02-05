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
    [MigrationNumber( 41, "1.7.0" )]
    public class MigrationRollupsForV7_3 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Update 'Wizard' Communication Detail block to have the same Approvers as 'Simple' communication detail block
            Sql( HotFixMigrationResource._041_MigrationRollupsForV7_3_UpdateWizardCommunicationDetailApprovers );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
