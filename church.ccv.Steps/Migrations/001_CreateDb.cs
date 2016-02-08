using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Steps.Migrations
{
    [MigrationNumber( 1, "1.4.0" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    CREATE TABLE [dbo].[_church_ccv_Steps_StepMeasure](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [Title] [nvarchar](75) NULL,
	    [Description] [nvarchar](500) NULL,
	    [IconCssClass] nvarchar(100) NULL,
	    [Color] [nvarchar](100) NULL,
        [Order] int NULL,
	    [IsActive] [bit] NULL,
	    [IsTbd] [bit] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._church_ccv_Steps_StepMeasure] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasure]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasure_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasure] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasure_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasure]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasure_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasure] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasure_dbo.PersonAlias_ModifiedByPersonAliasId]

" );

            Sql( @"
    CREATE TABLE [dbo].[_church_ccv_Steps_StepMeasureValue](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [StepMeasureId] int NOT NULL,
	    [Value] [int] NULL,
	    [CampusId] int NULL,
	    [PastorPersonAliasId] [int] NULL,
        [SundayDate] [datetime] NULL,
	    [WeekendAttendance] [int] NULL,
	    [ActiveAdults] [int] NULL,
        [Note] nvarchar(500) NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._church_ccv_Steps_StepMeasureValue] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_ModifiedByPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_PastorPersonAliasId] FOREIGN KEY([PastorPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.PersonAlias_PastorPersonAliasId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo._church_ccv_Steps_StepMeasure_MeasureId] FOREIGN KEY([StepMeasureId])
    REFERENCES [dbo].[_church_ccv_Steps_StepMeasure] ([Id])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo._church_ccv_Steps_StepMeasure_MeasureId]

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue]  WITH CHECK ADD  CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
    REFERENCES [dbo].[Campus] ([Id])

    ALTER TABLE [dbo].[_church_ccv_Steps_StepMeasureValue] CHECK CONSTRAINT [FK_dbo._church_ccv_Steps_StepMeasureValue_dbo.Campus_CampusId]
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
