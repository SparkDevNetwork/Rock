using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.DpsMatch.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_DpsMatch_Offender](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [KeyString] [nvarchar](100) NOT NULL,
	                [LastName] [nvarchar](50) NOT NULL,
	                [FirstName] [nvarchar](50) NOT NULL,
	                [MiddleInitial] [char](1) NULL,
	                [Age] [int] NULL,
	                [Height] [int] NULL,
	                [Weight] [int] NULL,
	                [Race] [nvarchar](50) NOT NULL,
	                [Sex] [nvarchar](50) NOT NULL,
	                [Hair] [nvarchar](50) NOT NULL,
	                [Eyes] [nvarchar](50) NOT NULL,
	                [ResidentialAddress] [nvarchar](100) NULL,
	                [ResidentialCity] [nvarchar](50) NULL,
	                [ResidentialState] [nvarchar](50) NULL,
	                [ResidentialZip] [int] NULL,
	                [VerificationDate] [datetime] NULL,
	                [Offense] [nvarchar](500) NULL,
	                [OffenseLevel] [int] NULL,
	                [Absconder] [bit] NULL,
	                [ConvictingJurisdiction] [nvarchar](100) NULL,
	                [Unverified] [bit] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignId] [nvarchar](50) NULL,
                 CONSTRAINT [PK__com_centralaz_DpsMatch_Offender] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                CREATE TABLE [dbo].[_com_centralaz_DpsMatch_Match](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [PersonAliasId] [int] NOT NULL,
	                [OffenderId] [int] NOT NULL,
	                [MatchPercentage] [int] NULL,
	                [IsMatch] [bit] NULL,
	                [VerifiedDate] [datetime] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignId] [nvarchar](50) NULL,
                 CONSTRAINT [PK__com_centralaz_DpsMatch_Match] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_DpsMatch_Match__com_centralaz_DpsMatch_Offender] FOREIGN KEY([OffenderId])
                REFERENCES [dbo].[_com_centralaz_DpsMatch_Offender] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match] CHECK CONSTRAINT [FK__com_centralaz_DpsMatch_Match__com_centralaz_DpsMatch_Offender]

                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_DpsMatch_Match_PersonAlias] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match] CHECK CONSTRAINT [FK__com_centralaz_DpsMatch_Match_PersonAlias]
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match] DROP CONSTRAINT [FK__com_centralaz_DpsMatch_Match_PersonAlias]
                ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match] DROP CONSTRAINT [FK__com_centralaz_DpsMatch_Match__com_centralaz_DpsMatch_Offender]
                DROP TABLE [dbo].[_com_centralaz_DpsMatch_Match]

                DROP TABLE [dbo].[_com_centralaz_DpsMatch_Offender]
" );
        }
    }
}
