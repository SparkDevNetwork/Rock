using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.CommandCenter.Migrations
{
    [MigrationNumber( 4, "1.2.0" )]
    class RenameDomain : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    sp_rename '_com_ccvonline_CommandCenter_Recording', '_church_ccv_CommandCenter_Recording'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_CommandCenter_Recording_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_CommandCenter_Recording_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_CommandCenterRecording_dbo.Campus_CampusId]', 'FK_dbo._church_ccv_CommandCenterRecording_dbo.Campus_CampusId'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_CommandCenterRecording]', 'PK_dbo._church_ccv_CommandCenterRecording'
" );

            Sql( @"
    DELETE [EntityType] 
    WHERE [Name] LIKE 'church.ccv.CommandCenter%'

    UPDATE [EntityType] SET 
	    [Name] = REPLACE([Name],'com.ccvonline.CommandCenter', 'church.ccv.CommandCenter'),
	    [AssemblyName] = REPLACE([Name],'com.ccvonline.CommandCenter', 'church.ccv.CommandCenter')
    WHERE [Name] LIKE 'com.ccvonline.CommandCenter%'

    DELETE [BlockType]
    WHERE [Path] LIKE '%church_ccv/CommandCenter%'

    UPDATE [BlockType] SET 
	    [Path] = REPLACE([Path],'com_ccvonline/CommandCenter', 'church_ccv/CommandCenter')
    WHERE [Path] LIKE '%com_ccvonline/CommandCenter%'
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
