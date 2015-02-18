using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Residency.Migrations
{
    [MigrationNumber( 8, "1.2.0" )]
    class RenameDomain : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    sp_rename '_com_ccvonline_Residency_Competency', '_church_ccv_Residency_Competency'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_CompetencyPerson', '_church_ccv_Residency_CompetencyPerson'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_CompetencyPersonProject', '_church_ccv_Residency_CompetencyPersonProject'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_CompetencyPersonProjectAssessment', '_church_ccv_Residency_CompetencyPersonProjectAssessment'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment', '_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_Period', '_church_ccv_Residency_Period'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_Project', '_church_ccv_Residency_Project'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_ProjectPointOfAssessment', '_church_ccv_Residency_ProjectPointOfAssessment'
" );
            Sql( @"
    sp_rename '_com_ccvonline_Residency_Track', '_church_ccv_Residency_Track'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Competency_dbo._com_ccvonline_Residency_Track_TrackId]', 'FK_dbo._church_ccv_Residency_Competency_dbo._church_ccv_Residency_Track_TrackId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Competency_dbo.Person_FacilitatorPersonId]', 'FK_dbo._church_ccv_Residency_Competency_dbo.Person_FacilitatorPersonId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Competency_dbo.Person_TeacherOfRecordPersonId]', 'FK_dbo._church_ccv_Residency_Competency_dbo.Person_TeacherOfRecordPersonId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Competency_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Competency_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Competency_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Competency_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo._com_ccvonline_Residency_Competency_CompetencyId]', 'FK_dbo._church_ccv_Residency_CompetencyPerson_dbo._church_ccv_Residency_Competency_CompetencyId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.Person_PersonId]', 'FK_dbo._church_ccv_Residency_CompetencyPerson_dbo.Person_PersonId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPerson_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPerson_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo._com_ccvonline_Residency_CompetencyPerson_CompetencyPersonId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProject_dbo._church_ccv_Residency_CompetencyPerson_CompetencyPersonId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo._com_ccvonline_Residency_Project_ProjectId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProject_dbo._church_ccv_Residency_Project_ProjectId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProject_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProject_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo._com_ccvonline_Residency_CompetencyPersonProject_Competenc]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessment_dbo._church_ccv_Residency_CompetencyPersonProject_Competenc'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.Person_AssessorPersonId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessment_dbo.Person_AssessorPersonId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._com_ccvonline_Residency_CompetencyPerson]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._church_ccv_Residency_CompetencyPerson'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._com_ccvonline_Residency_ProjectPointOfAs]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._church_ccv_Residency_ProjectPointOfAs'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Period_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Period_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Period_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Period_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Project_dbo._com_ccvonline_Residency_Competency_CompetencyId]', 'FK_dbo._church_ccv_Residency_Project_dbo._church_ccv_Residency_Competency_CompetencyId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Project_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Project_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Project_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Project_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo._com_ccvonline_Residency_Project_ProjectId]', 'FK_dbo._church_ccv_Residency_ProjectPointOfAssessment_dbo._church_ccv_Residency_Project_ProjectId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.DefinedValue_PointOfAssessmentTypeValueId]', 'FK_dbo._church_ccv_Residency_ProjectPointOfAssessment_dbo.DefinedValue_PointOfAssessmentTypeValueId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_ProjectPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_ProjectPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Track_dbo._com_ccvonline_Residency_Period_PeriodId]', 'FK_dbo._church_ccv_Residency_Track_dbo._church_ccv_Residency_Period_PeriodId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Track_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Track_dbo.PersonAlias_CreatedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[FK_dbo._com_ccvonline_Residency_Track_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo._church_ccv_Residency_Track_dbo.PersonAlias_ModifiedByPersonAliasId'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_Competency]', 'PK_dbo._church_ccv_Residency_Competency'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_CompetencyPerson]', 'PK_dbo._church_ccv_Residency_CompetencyPerson'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_CompetencyPersonProject]', 'PK_dbo._church_ccv_Residency_CompetencyPersonProject'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment]', 'PK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessment'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]', 'PK_dbo._church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_Period]', 'PK_dbo._church_ccv_Residency_Period'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_Project]', 'PK_dbo._church_ccv_Residency_Project'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment]', 'PK_dbo._church_ccv_Residency_ProjectPointOfAssessment'
" );
            Sql( @"
    sp_rename 'dbo.[PK_dbo._com_ccvonline_Residency_Track]', 'PK_dbo._church_ccv_Residency_Track'
" );

            Sql( @"
    DELETE [EntityType] 
    WHERE [Name] LIKE 'church.ccv.Residency%'

    UPDATE [EntityType] SET 
	    [Name] = REPLACE([Name],'com.ccvonline.Residency', 'church.ccv.Residency'),
	    [AssemblyName] = REPLACE([Name],'com.ccvonline.Residency', 'church.ccv.Residency')
    WHERE [Name] LIKE 'com.ccvonline.Residency%'

    DELETE [BlockType]
    WHERE [Path] LIKE '%church_ccv/Residency%'

    UPDATE [BlockType] SET 
	    [Path] = REPLACE([Path],'com_ccvonline/Residency', 'church_ccv/Residency')
    WHERE [Path] LIKE '%com_ccvonline/Residency%'
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
