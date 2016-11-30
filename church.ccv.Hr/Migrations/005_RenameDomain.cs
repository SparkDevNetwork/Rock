using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Hr.Migrations
{
    [MigrationNumber( 5, "1.2.0" )]
    class RenameDomain : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    sp_rename '_com_ccvonline_Hr_TimeCard', '_church_ccv_Hr_TimeCard'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_ApprovedByPersonAlias]', '_church_ccv_Hr_TimeCard_ApprovedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_CreatedByPersonAlias]', '_church_ccv_Hr_TimeCard_CreatedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_ModifiedByPersonAlias]', '_church_ccv_Hr_TimeCard_ModifiedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_PersonAlias]', '_church_ccv_Hr_TimeCard_PersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_SubmittedToPersonAlias]', '_church_ccv_Hr_TimeCard_SubmittedToPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCard_TimeCardPayPeriod]', '_church_ccv_Hr_TimeCard_TimeCardPayPeriod'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Hr_TimeCardDay', '_church_ccv_Hr_TimeCardDay'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardDay_CreatedByPersonAlias]', '_church_ccv_Hr_TimeCardDay_CreatedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardDay_ModifiedByPersonAlias]', '_church_ccv_Hr_TimeCardDay_ModifiedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardDay_TimeCard]', '_church_ccv_Hr_TimeCardDay_TimeCard'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Hr_TimeCardHistory', '_church_ccv_Hr_TimeCardHistory'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardHistory_CreatedByPersonAlias]', '_church_ccv_Hr_TimeCardHistory_CreatedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardHistory_ModifiedByPersonAlias]', '_church_ccv_Hr_TimeCardHistory_ModifiedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardHistory_StatusPersonAlias]', '_church_ccv_Hr_TimeCardHistory_StatusPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardHistory_TimeCard]', '_church_ccv_Hr_TimeCardHistory_TimeCard'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Hr_TimeCardPayPeriod', '_church_ccv_Hr_TimeCardPayPeriod'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardPayPeriod_CreatedByPersonAlias]', '_church_ccv_Hr_TimeCardPayPeriod_CreatedByPersonAlias'
" );
            Sql( @"
    sp_rename 'dbo.[_com_ccvonline_Hr_TimeCardPayPeriod_ModifiedByPersonAlias]', '_church_ccv_Hr_TimeCardPayPeriod_ModifiedByPersonAlias'
" );


            Sql( @"
    DELETE [EntityType] 
    WHERE [Name] LIKE 'church.ccv.Hr%'

    UPDATE [EntityType] SET 
	    [Name] = REPLACE([Name],'com.ccvonline.Hr', 'church.ccv.Hr'),
	    [AssemblyName] = REPLACE([Name],'com.ccvonline.Hr', 'church.ccv.Hr')
    WHERE [Name] LIKE 'com.ccvonline.Hr%'

    DELETE [BlockType]
    WHERE [Path] LIKE '%church_ccv/Hr%'

    UPDATE [BlockType] SET 
	    [Path] = REPLACE([Path],'com_ccvonline/Hr', 'church_ccv/Hr')
    WHERE [Path] LIKE '%com_ccvonline/Hr%'
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
