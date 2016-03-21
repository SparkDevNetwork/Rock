using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Promotions.Migrations
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
CREATE TABLE [dbo].[_church_ccv_Promotions_PromotionRequest] (
    [Id] [int] NOT NULL IDENTITY,
    [EventItemOccurrenceId] [int] NOT NULL,
    [EventLastModifiedTime] [datetime] NOT NULL,
    [ContentChannelId] [int] NOT NULL,
    [IsActive] [bit] NOT NULL,

    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [int] NULL,
    [ForeignGuid] [uniqueidentifier] NULL,
    [ForeignKey] [nvarchar](100) NULL   
)

CREATE TABLE [dbo].[_church_ccv_Promotions_PromotionOccurrence] (
    [Id] [int] NOT NULL IDENTITY,
    [PromotionRequestId] [int] NULL,
    [ContentChannelItemId] [int] NOT NULL,

    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [int] NULL,
    [ForeignGuid] [uniqueidentifier] NULL,
    [ForeignKey] [nvarchar](100) NULL
)" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
