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
    [MigrationNumber( 52, "1.8.0" )]
    public class MigrationRollupsForV8_1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // MP - Fix for Inactive People showing up in Statement Generator Address when ExcludeInactive = True
            Sql( @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnCrm_GetFamilyTitleIncludeInactive]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ufnCrm_GetFamilyTitleIncludeInactive]
" );

            Sql( HotFixMigrationResource._051_MigrationRollupsForV8_1_ufnCrm_GetFamilyTitleIncludeInactive );
            Sql( HotFixMigrationResource._051_MigrationRollupsForV8_1_ufnCrm_GetFamilyTitle);
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
         
        }
    }
}
