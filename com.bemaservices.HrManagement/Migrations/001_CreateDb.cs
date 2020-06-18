using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 1, "1.9.4" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            // Update Field Types
            RockMigrationHelper.UpdateFieldType( "Pto Tier", "", "com.bemaservices.HrManagement", "com.bemaservices.HrManagement.Field.Types.PtoTierFieldType", SystemGuid.FieldType.PTO_TIER );

            // Add Global Attributes
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.DATE, "", "", "Fiscal Year Start Date", "Used to determine when the Fiscal Year Starts and Ends.", 0, "2020-1-1", SystemGuid.Attribute.FISCAL_YEAR_START_DATE_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Minimum Time Requirement", "The Minimum amout of time someone can request off in hours in a day.",0,"4" , SystemGuid.Attribute.MINIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Maximum Time Requirement", "The Maximum amout of time someone can request off in hours in a day.", 0, "8", SystemGuid.Attribute.MAXIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Time Increments", "The Increment for Time Off in hours.", 0, "4", SystemGuid.Attribute.TIME_INCREMENTS_ATTRIBUTE );

            // Add Person Attributes
            RockMigrationHelper.AddEntityAttribute( "Person", Rock.SystemGuid.FieldType.PERSON, "", "", "Supervisor", "Hr", "The Person that this person reports to for the purpose of Time Off Requests", 0, "", SystemGuid.Attribute.SUPERVISOR_PERSON_ATTRIBUTE );
            RockMigrationHelper.AddEntityAttribute( "Person", SystemGuid.FieldType.PTO_TIER , "", "", "PTO Tier", "Hr", "The PTO Tier that this person's PTO is accured based upon.", 1, "", SystemGuid.Attribute.PTO_TIER_PERSON_ATTRIBUTE );

            // Create Tables
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](100) NULL,
	                [Description] [nvarchar] NULL,
                    [Color] [nvarchar](100) NULL,
	                [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PtoTierId] int NOT NULL,
                    [MinimumYear] int NOT NULL,
                    [MaximumYear] int NULL,
	                [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_PtoTeirId] FOREIGN KEY([PtoTierId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoTier] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_PtoTeirId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PtoTierId] int NOT NULL,
                    [PtoTypeId] int NOT NULL,
	                [IsActive] [bit] NOT NULL,
                    [DefaultHours] int NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTeirId] FOREIGN KEY([PtoTierId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoTier] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTeirId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTypeId] FOREIGN KEY([PtoTypeId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTypeId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](100) NULL,
	                [Description] [nvarchar] NULL,
                    [Color] [nvarchar](100) NULL,
	                [IsActive] [bit] NOT NULL,
                    [IsNegativeTimeBalanceAllowed] [bit] NOT NULL,
                    [WorkflowTypeId] int NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
                REFERENCES [dbo].[WorkflowType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_WorkflowTypeId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [WorkflowId] int NOT NULL,
	                [PersonAliasId] int NOT NULL,
                    [StartDate] date NOT NULL,
                    [EndDate] date NULL,
                    [HoursPerDay] int NOT NULL,
                    [PtoTypeId] int NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_WorkflowId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PtoTypeId] FOREIGN KEY([PtoTypeId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PtoTypeId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PersonAliasId]
            " );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.MAXIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.MINIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.TIME_INCREMENTS_ATTRIBUTE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PTO_TIER_PERSON_ATTRIBUTE );

            RockMigrationHelper.DeleteFieldType( SystemGuid.FieldType.PTO_TIER );
        }
    }
}
