using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.MailChimp.SystemGuid;

namespace com.bemaservices.MailChimp.Migrations
{
    [MigrationNumber( 3, "1.9.4" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_MailChimp_Member](
                    [Id] [int] IDENTITY(1,1) NOT NULL,
                    [MemberId] [varchar](50) NOT NULL,
	                [ListId] [varchar](50) NULL,
                    [PersonAliasId] [int] NULL,
	                [LastUpdated] [datetime] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_MailChimp_Member] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]              

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_MailChimp_Member_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member] CHECK CONSTRAINT [FK__com_bemaservices_MailChimp_Member_PersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MailChimp_Member_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member] CHECK CONSTRAINT [FK_dbo.MailChimp_Member_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member]  WITH CHECK ADD  CONSTRAINT [FK_dbo.MailChimp_Member_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member] CHECK CONSTRAINT [FK_dbo.MailChimp_Member_dbo.PersonAlias_ModifiedByPersonAliasId]
" );

            RockMigrationHelper.UpdateEntityType( "com.bemaservices.MailChimp.Model.MailChimpMember", "415dffd1-8b17-4fd1-9d50-43d44444dfc7", true, true );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "415dffd1-8b17-4fd1-9d50-43d44444dfc7" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_MailChimp_Member] DROP CONSTRAINT [FK__com_bemaservices_MailChimp_Member_PersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_MailChimp_Member]
                " );
        }
    }
}
