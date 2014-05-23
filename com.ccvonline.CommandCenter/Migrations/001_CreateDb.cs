using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.ccvonline.CommandCenter.Migrations
{
    [MigrationNumber( 1, "1.0.8" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    CREATE TABLE [dbo].[_com_ccvonline_CommandCenter_Recording](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
	    [CampusId] [int] NULL,
	    [Date] [datetime] NULL,
	    [Label] [nvarchar](100) NULL,
	    [App] [nvarchar](100) NULL,
	    [StreamName] [nvarchar](100) NULL,
	    [RecordingName] [nvarchar](100) NULL,
	    [StartTime] [datetime] NULL,
	    [StartResponse] [nvarchar](400) NULL,
	    [StopTime] [datetime] NULL,
	    [StopResponse] [nvarchar](400) NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._com_ccvonline_CommandCenterRecording] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_CommandCenterRecording_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
    REFERENCES [dbo].[Campus] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording] CHECK CONSTRAINT [FK_dbo._com_ccvonline_CommandCenterRecording_dbo.Campus_CampusId]

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording] CHECK CONSTRAINT [FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_CommandCenter_Recording] CHECK CONSTRAINT [FK_dbo._com_ccvonline_CommandCenter_Recording_dbo.PersonAlias_ModifiedByPersonAliasId]

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
