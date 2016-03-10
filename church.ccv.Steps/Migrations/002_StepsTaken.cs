using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Steps.Migrations
{
    [MigrationNumber( 2, "1.4.0" )]
    public class StepsTaken : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    CREATE TABLE [dbo].[_church_ccv_Steps_StepTaken](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [StepMeasureId] int NOT NULL,
	    [CampusId] int NULL,
	    [PersonAliasId] [int],
        [DateTaken] [datetime],
        [SundayDate]  AS ([dbo].[ufnUtility_GetSundayDate]([DateTaken])) PERSISTED,
        [Note] nvarchar(500) NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] int NULL,
        [ForeignKey] nvarchar(100) NULL,
        [ForeignGuid] [uniqueidentifier] NULL
     CONSTRAINT [PK_dbo._church_ccv_Steps_StepTaken] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_ModifiedByPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.PersonAlias_PersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo._church_ccv_Steps_StepMeasure_MeasureId] FOREIGN KEY([StepMeasureId])
    REFERENCES [dbo].[_church_ccv_Steps_StepMeasure] ([Id])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo._church_ccv_Steps_StepMeasure_MeasureId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
    REFERENCES [dbo].[Campus] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepTaken] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepTaken_dbo.Campus_CampusId]
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
