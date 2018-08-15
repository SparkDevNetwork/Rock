using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace org.newpointe.Stars.Migrations
{
    [MigrationNumber( 1, "1.0.5" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql(@"
    CREATE TABLE [dbo].[_org_newpointe_Stars_Transactions](
	    [Id] [int] IDENTITY(1,1) NOT NULL,
        [PersonAliasId] [int] NOT NULL,
        [Value] decimal(18,0) NOT NULL,
        [TransactionDateTime] [datetime] NOT NULL,
        [Note] [nvarchar](100) NULL,
	    [CampusId] [int] NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
        [ForeignGuid] [nvarchar](50) NULL,
        [ForeignKey] [int] NULL,
     CONSTRAINT [PK_dbo._org_newpointe_Stars_Transactions] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    )

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] CHECK CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_PersonAliasId]

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.Campus_CampusId] FOREIGN KEY([CampusId])
    REFERENCES [dbo].[Campus] ([Id])

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] CHECK CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.Campus_CampusId]

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] CHECK CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_CreatedByPersonAliasId]

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
    REFERENCES [dbo].[PersonAlias] ([Id])

    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] CHECK CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_ModifiedByPersonAliasId]
");
        
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql(@"
    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] DROP CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_ModifiedByPersonAliasId]
    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] DROP CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_CreatedByPersonAliasId]
    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] DROP CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.Campus_CampusId]
    ALTER TABLE [dbo].[_org_newpointe_Stars_Transactions] DROP CONSTRAINT [FK_dbo._org_newpointe_Stars_Transactions_dbo.PersonAlias_PersonAliasId]
    DROP TABLE [dbo].[_org_newpointe_Stars_Transactions]
");
        }
    }
}
