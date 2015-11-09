using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.SafetySecurity.Migrations
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
CREATE TABLE [dbo].[_church_ccv_SafetySecurity_DPSOffender](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LastName] [nvarchar](max) NULL,
	[FirstName] [nvarchar](max) NULL,
	[MiddleInitial] [nvarchar](max) NULL,
	[Age] [int] NULL,
	[Height] [int] NULL,
	[Weight] [int] NULL,
	[Race] [nvarchar](max) NULL,
	[Gender] [nvarchar](max) NULL,
	[Hair] [nvarchar](max) NULL,
	[Eyes] [nvarchar](max) NULL,
	[ResAddress] [nvarchar](max) NULL,
	[ResCity] [nvarchar](max) NULL,
	[ResZip] [nvarchar](max) NULL,
	[Offense] [nvarchar](max) NULL,
	[DateConvicted] [date] NOT NULL,
	[ConvictionState] [nvarchar](max) NULL,
	[Absconder] [bit] NOT NULL,
	[PersonAliasId] [int] NULL,
	[DpsLocationId] [int] NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[ForeignId] [int] NULL,
	[ForeignGuid] [uniqueidentifier] NULL,
	[ForeignKey] [nvarchar](100) NULL,
    CONSTRAINT [PK_dbo._church_ccv_SafetySecurity_DPSOffender] PRIMARY KEY CLUSTERED ([Id] ASC) 
)

ALTER TABLE [dbo].[_church_ccv_SafetySecurity_DPSOffender] ADD CONSTRAINT [FK_dbo._church_ccv_SafetySecurity_DPSOffender.Location_DpsLocationId] FOREIGN KEY([DpsLocationId]) REFERENCES [dbo].[Location] ([Id])

ALTER TABLE [dbo].[_church_ccv_SafetySecurity_DPSOffender] ADD CONSTRAINT [FK_dbo._church_ccv_SafetySecurity_DPSOffender.PersonAlias_PersonAliasId] FOREIGN KEY([PersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
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
