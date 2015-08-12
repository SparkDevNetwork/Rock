using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.Accountability.Migrations
{
    [MigrationNumber(1, "1.0.14")]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            Sql(@"
            CREATE TABLE [dbo].[_com_centralaz_Accountability_Question](
	            [Id] [int] IDENTITY(1,1) NOT NULL,
	            [Guid] [uniqueidentifier] NOT NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignId] [nvarchar](50) NULL,
	            [ShortForm] [nvarchar](50) NULL,
	            [LongForm] [nvarchar](400) NULL,
	            [GroupTypeId] [int] NULL,
             CONSTRAINT [PK__com_centralaz_Accountability_Question] PRIMARY KEY CLUSTERED 
            (
	            [Id] ASC
            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            ) ON [PRIMARY]

            CREATE TABLE [dbo].[_com_centralaz_Accountability_ResponseSet](
	            [Id] [int] IDENTITY(1,1) NOT NULL,
	            [Guid] [uniqueidentifier] NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignId] [nvarchar](50) NULL,
	            [Comment] [varchar](4000) NULL,
	            [SubmitForDate] [date] NULL,
	            [Score] [float] NULL,
	            [GroupId] [int] NULL,
	            [PersonId] [int] NULL,
             CONSTRAINT [PK__com_centralaz_Accountability_ResponseSet] PRIMARY KEY CLUSTERED 
            (
	            [Id] ASC
            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            ) ON [PRIMARY]

            CREATE TABLE [dbo].[_com_centralaz_Accountability_Response](
	            [Id] [int] IDENTITY(1,1) NOT NULL,
	            [Guid] [uniqueidentifier] NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignId] [nvarchar](50) NULL,
	            [IsResponseYes] [bit] NULL,
	            [Comment] [varchar](300) NULL,
	            [QuestionId] [int] NULL,
	            [ResponseSetId] [int] NULL,
             CONSTRAINT [PK__com_centralaz_Accountability_Response] PRIMARY KEY CLUSTERED 
            (
	            [Id] ASC
            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            ) ON [PRIMARY]

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Accountability_Question__com_centralaz_Accountability_Question] FOREIGN KEY([GroupTypeId])
            REFERENCES [dbo].[GroupType] ([Id])

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Question] CHECK CONSTRAINT [FK__com_centralaz_Accountability_Question__com_centralaz_Accountability_Question]

            ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Group] FOREIGN KEY([GroupId])
            REFERENCES [dbo].[Group] ([Id])

            ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet] CHECK CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Group]

            ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Person] FOREIGN KEY([PersonId])
            REFERENCES [dbo].[Person] ([Id])

            ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet] CHECK CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Person]

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Response]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_Question] FOREIGN KEY([QuestionId])
            REFERENCES [dbo].[_com_centralaz_Accountability_Question] ([Id])

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Response] CHECK CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_Question]

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Response]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_ResponseSet] FOREIGN KEY([ResponseSetId])
            REFERENCES [dbo].[_com_centralaz_Accountability_ResponseSet] ([Id])

            ALTER TABLE [dbo].[_com_centralaz_Accountability_Response] CHECK CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_ResponseSet]

");
        }
        public override void Down()
        {
            Sql(@"
                ALTER TABLE [dbo].[_com_centralaz_Accountability_Response] DROP CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_ResponseSet]
                ALTER TABLE [dbo].[_com_centralaz_Accountability_Response] DROP CONSTRAINT [FK__com_centralaz_Accountability_Response__com_centralaz_Accountability_Question]
                ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet] DROP CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Group]
                ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet] DROP CONSTRAINT [FK__com_centralaz_Accountability_ResponseSet_Person]
                ALTER TABLE [dbo].[_com_centralaz_Accountability_Question] DROP CONSTRAINT [FK__com_centralaz_Accountability_Question__com_centralaz_Accountability_Question]
                DROP TABLE [dbo].[_com_centralaz_Accountability_Response]
                DROP TABLE [dbo].[_com_centralaz_Accountability_ResponseSet]
                DROP TABLE [dbo].[_com_centralaz_Accountability_Question]");
        }
    }
}
