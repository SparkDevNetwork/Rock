using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.SampleProject.Migrations
{
    [MigrationNumber( 3, "1.2.0" )]
    class RenameDomain : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    sp_rename '_com_ccvonline_SampleProject_ReferralAgency', '_church_ccv_SampleProject_ReferralAgency'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.Campus_CampusId]', 'FK_dbo._church_ccv_SampleProject_ReferralAgency_dbo.Campus_CampusId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.DefinedValue_ReferralAgencyTypeValueId]', 'FK_dbo._church_ccv_SampleProject_ReferralAgency_dbo.DefinedValue_ReferralAgencyTypeValueId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_SampleProject_ReferralAgency_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_SampleProject_ReferralAgency_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_SampleProject_ReferralAgency]', 'PK_dbo._church_ccv_SampleProject_ReferralAgency'
" );

            Sql( @"
    DELETE [EntityType] 
    WHERE [Name] LIKE 'church.ccv.SampleProject%'

    UPDATE [EntityType] SET 
	    [Name] = REPLACE([Name],'com.ccvonline.SampleProject', 'church.ccv.SampleProject'),
	    [AssemblyName] = REPLACE([Name],'com.ccvonline.SampleProject', 'church.ccv.SampleProject')
    WHERE [Name] LIKE 'com.ccvonline.SampleProject%'

    DELETE [BlockType]
    WHERE [Path] LIKE '%church_ccv/SampleProject%'

    UPDATE [BlockType] SET 
	    [Path] = REPLACE([Path],'com_ccvonline/SampleProject', 'church_ccv/SampleProject')
    WHERE [Path] LIKE '%com_ccvonline/SampleProject%'
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
