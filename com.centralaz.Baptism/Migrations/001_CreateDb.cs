using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_Baptism_Baptizee](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignId] [nvarchar](50) NULL,
	                [GroupId] [int] NOT NULL,
	                [PersonAliasId] [int] NOT NULL,
	                [Baptizer1AliasId] [int] NULL,
	                [Baptizer2AliasId] [int] NULL,
	                [ApproverAliasId] [int] NULL,
	                [isConfirmed] [bit] NULL,
	                [BaptismDateTime] [datetime] NULL,
                 CONSTRAINT [PK__com_centralaz_Baptism_Baptizee] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_Group] FOREIGN KEY([GroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] CHECK CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_Group]

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] CHECK CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias]

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias1] FOREIGN KEY([ApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] CHECK CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias1]

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias2] FOREIGN KEY([Baptizer1AliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] CHECK CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias2]

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias3] FOREIGN KEY([Baptizer2AliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] CHECK CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias3] " );
        }
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] DROP CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias3]
                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] DROP CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias2]
                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] DROP CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias1]
                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] DROP CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_PersonAlias]
                ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] DROP CONSTRAINT [FK__com_centralaz_Baptism_Baptizee_Group]
                DROP TABLE [dbo].[_com_centralaz_Baptism_Baptizee]" );
        }
    }
}
