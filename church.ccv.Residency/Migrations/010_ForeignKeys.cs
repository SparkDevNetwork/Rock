using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Residency.Migrations
{
    [MigrationNumber( 10, "1.3.4" )]
    class ForeignKeys : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Competency]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_Competency].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_Competency] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_Competency] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Competency] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_Competency] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPerson]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_CompetencyPerson].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPerson] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_CompetencyPerson] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPerson] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_CompetencyPerson] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProject]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_CompetencyPersonProject].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProject] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_CompetencyPersonProject] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProject] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_CompetencyPersonProject] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Period]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_Period].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_Period] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_Period] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Period] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_Period] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Project]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_Project].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_Project] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_Project] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Project] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_Project] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_ProjectPointOfAssessment]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_ProjectPointOfAssessment].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Track]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Residency_Track].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Residency_Track] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Residency_Track] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Track] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Residency_Track] (ForeignGuid)
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
