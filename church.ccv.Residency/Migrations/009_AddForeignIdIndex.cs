using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Residency.Migrations
{
    [MigrationNumber( 9, "1.3.1" )]
    class AddForeignIdIndex : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_Competency] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Competency] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPerson] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPerson] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProject] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProject] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessment] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_Period] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Period] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_Project] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Project] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_ProjectPointOfAssessment] ([ForeignId] ASC)" );

            Sql( "ALTER TABLE [dbo].[_church_ccv_Residency_Track] ALTER COLUMN [ForeignId] NVARCHAR(100)" );
            Sql( "CREATE NONCLUSTERED INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Residency_Track] ([ForeignId] ASC)" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}