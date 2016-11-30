using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.SampleProject.Migrations
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
    CREATE TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
        [ContactName] [nvarchar](100) NULL,
        [PhoneNumber] [nvarchar](100) NULL,
        [Website] [nvarchar](100) NULL,
	    [CampusId] [int] NULL,
	    [AgencyTypeValueId] [int] NULL,
	    [Name] [nvarchar](100) NOT NULL,
	    [Description] [nvarchar](max) NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
     CONSTRAINT [PK_dbo._com_ccvonline_SampleProject_ReferralAgency] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.DefinedValue_ReferralAgencyTypeValueId] FOREIGN KEY([AgencyTypeValueId])
    REFERENCES [dbo].[DefinedValue] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] CHECK CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.DefinedValue_ReferralAgencyTypeValueId]

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
    REFERENCES [dbo].[Campus] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] CHECK CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.Campus_CampusId]

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] CHECK CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] CHECK CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
        
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql(@"
    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] DROP CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_ModifiedByPersonAliasId]
    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] DROP CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.PersonAlias_CreatedByPersonAliasId]
    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] DROP CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.Campus_CampusId]
    ALTER TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency] DROP CONSTRAINT [FK_dbo._com_ccvonline_SampleProject_ReferralAgency_dbo.DefinedValue_ReferralAgencyTypeValueId]
    DROP TABLE [dbo].[_com_ccvonline_SampleProject_ReferralAgency]
" );
        }
    }
}
