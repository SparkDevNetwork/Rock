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
using Rock;

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
            RockMigrationHelper.UpdateFieldType( "Pto Type", "", "com.bemaservices.HrManagement", "com.bemaservices.HrManagement.Field.Types.PtoTypeFieldType", SystemGuid.FieldType.PTO_TYPE );
            RockMigrationHelper.UpdateFieldType( "Pto Request", "", "com.bemaservices.HrManagement", "com.bemaservices.HrManagement.Field.Types.PtoRequestFieldType", SystemGuid.FieldType.PTO_REQUEST );
            RockMigrationHelper.UpdateFieldType( "Pto Allocation", "", "com.bemaservices.HrManagement", "com.bemaservices.HrManagement.Field.Types.PtoAllocationFieldType", SystemGuid.FieldType.PTO_ALLOCATION );

            // Update Entity Types
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoType", "Pto Type", "com.bemaservices.HrManagement.Model.PtoType, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "B1F9D79A-0B0D-49E2-AE3D-D1492425FD38" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoTier", "Pto Tier", "com.bemaservices.HrManagement.Model.PtoTier, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "9C7F0A71-23B9-4F8D-A104-7BB521F3EB0E" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoBracket", "Pto Bracket", "com.bemaservices.HrManagement.Model.PtoBracket, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "079275BF-4E79-4038-91E9-389A172DCA71" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoBracketType", "Pto Bracket Type", "com.bemaservices.HrManagement.Model.PtoBracketType, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "1632165E-246B-420C-AFE2-79BEE19BB3D0" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoAllocation", "Pto Allocation", "com.bemaservices.HrManagement.Model.PtoAllocation, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "198A76A9-9EA0-4030-B8B2-526ADE69C268" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Model.PtoRequest", "Pto Request", "com.bemaservices.HrManagement.Model.PtoRequest, com.bemaservices.HrManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true, true, "F38B7BA2-1289-4CFE-AFD4-74DF942280D8" );

            // Add Global Attributes
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.DATE, "", "", "Fiscal Year Start Date", "Used to determine when the Fiscal Year Starts and Ends.", 0, "2020-1-1", SystemGuid.Attribute.FISCAL_YEAR_START_DATE_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Minimum Time Requirement", "The Minimum amout of time someone can request off in hours in a day.", 0, "4", SystemGuid.Attribute.MINIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Maximum Time Requirement", "The Maximum amout of time someone can request off in hours in a day.", 0, "8", SystemGuid.Attribute.MAXIMUM_TIME_REQUIREMENT_ATTRIBUTE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Time Increments", "The Increment for Time Off in hours.", 0, "4", SystemGuid.Attribute.TIME_INCREMENTS_ATTRIBUTE );

            // Add Person Attributes
            RockMigrationHelper.UpdatePersonAttributeCategory( "Human Resources", "fa fa-compass", "", SystemGuid.Category.HR_ATTRIBUTE_CATEGORY );

            var categoryGuids = new List<string>();

            categoryGuids.Add( SystemGuid.Category.HR_ATTRIBUTE_CATEGORY );

            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( Rock.SystemGuid.FieldType.PERSON, categoryGuids, "Supervisor", "Supervisor", "Supervisor", "", "", 1, "", SystemGuid.Attribute.SUPERVISOR_PERSON_ATTRIBUTE );
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( SystemGuid.FieldType.PTO_TIER, categoryGuids, "PTO Tier", "PTO Tier", "PTOTier", "", "", 2, "", SystemGuid.Attribute.PTO_TIER_PERSON_ATTRIBUTE );
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( Rock.SystemGuid.FieldType.DATE, categoryGuids, "Hire Date", "Hire Date", "HireDate", "", "", 3, "", SystemGuid.Attribute.HIRE_DATE_PERSON_ATTRIBUTE );
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( Rock.SystemGuid.FieldType.DATE, categoryGuids, "Fire Date", "Fire Date", "FireDate", "", "", 3, "", SystemGuid.Attribute.FIRE_DATE_PERSON_ATTRIBUTE );

            // Add Security Role
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Human Resources Administration", "Group of individuals who can administrate the various parts of the human resources functionality.", "6F8AABA3-5BC8-468B-90DD-F0686F38E373" );

            // Create Tables
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](100) NULL,
	                [Description] [nvarchar](max) NULL,
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

                ALTER TABLE [_com_bemaservices_HrManagement_PtoTier] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoTier_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoTier] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoTier_ModifiedByPersonAliasId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](100) NULL,
	                [Description] [nvarchar](max) NULL,
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

                ALTER TABLE [_com_bemaservices_HrManagement_PtoType] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoType_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
                REFERENCES [dbo].[WorkflowType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoType_WorkflowTypeId]
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

                ALTER TABLE [_com_bemaservices_HrManagement_PtoBracket] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoBracket_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_PtoTierId] FOREIGN KEY([PtoTierId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoTier] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracket] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracket_PtoTierId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PtoBracketId] int NOT NULL,
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

                ALTER TABLE [_com_bemaservices_HrManagement_PtoBracketType] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoBracketType_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoBracketId] FOREIGN KEY([PtoBracketId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoBracket] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoBracketId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTypeId] FOREIGN KEY([PtoTypeId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoBracketType] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoBracketType_PtoTypeId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PtoTypeId] int NOT NULL,
	                [StartDate] datetime NOT NULL,
	                [EndDate] datetime NULL,
                    [Hours] decimal NOT NULL,
                    [PtoAccrualSchedule] int NULL,
                    [PtoAllocationSourceType] int NOT NULL,
	                [LastProcessedDate] datetime NULL,
                    [PtoAllocationStatus] int NOT NULL,
                    [PersonAliasId] int NOT NULL,
                    [Note] [nvarchar](max) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [_com_bemaservices_HrManagement_PtoAllocation] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoAllocation_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_PtoTypeId] FOREIGN KEY([PtoTypeId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoType] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_PtoTypeId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoAllocation] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoAllocation_PersonAliasId]
            " );

            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ApproverPersonAliasId] int NULL,
                    [RequestDate] date NOT NULL,
                    [Hours] [decimal](18, 1) NOT NULL,
                    [PtoAllocationId] int NOT NULL,
                    [Reason] [nvarchar](max) NULL,
                    [PtoRequestApprovalState] int NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                ) ON [PRIMARY]

                ALTER TABLE [_com_bemaservices_HrManagement_PtoRequest] ADD CONSTRAINT PK__com_bemaservices_HrManagement_PtoRequest_Id PRIMARY KEY CLUSTERED ( Id );

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PtoAllocationId] FOREIGN KEY([PtoAllocationId])
                REFERENCES [dbo].[_com_bemaservices_HrManagement_PtoAllocation] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_PtoAllocationId]

                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ApproverPersonAliasId] FOREIGN KEY([ApproverPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                ALTER TABLE [dbo].[_com_bemaservices_HrManagement_PtoRequest] CHECK CONSTRAINT [FK__com_bemaservices_HrManagement_PtoRequest_ApproverPersonAliasId]
            " );

            // add ServiceJob: Process PTO Allocations
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations' AND [Guid] = '48AB813A-0038-4F4F-B3F7-9CDB975B19A1' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Process PTO Allocations'
                  ,''
                  ,'com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations'
                  ,'0 0 12 1 1/1 ? *'
                  ,1
                  ,'48AB813A-0038-4F4F-B3F7-9CDB975B19A1'
                  );
            END" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Person Hired Date Attribute", "The Person Attribute that contains the Person's Hired Date.  This will be used to determine if the person is currently staff or not.", 0, @"", "2533F72C-57EB-4022-BD9E-8492B4516DF7", "HireDate" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Person Fired Date Attribute", "The Person Attribute that contains the Person's Fired Date.  This will be used to determine if the person is currently staff or not.", 0, @"", "6C8A6197-42C1-40A4-A752-8900EB2FCF46", "FireDate" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "New Allocations Status", "What should newly created allocations have as a status", 2, @"2", "6D67B840-6EC4-451A-8743-6D48CF2BAC39", "NewAllocationsStatus" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Pto Types", "The Pto Types to Create Allocations For", 4, @"", "2495F715-709D-49F4-B68D-B0C186FC64FE", "PtoTypes" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Days Back", "The Number of Days prior to the Fiscal Start Date to create new Allocations", 5, @"30", "C97F8E2A-FF03-443C-AF6E-427EFD81884D", "DaysBack" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Update Allocation Status", "Whether the job should set a status of 'Inactive' on allocations whose End Date has past or Activate Allocations that have a Start Date in the past.", 6, @"True", "57F6E158-2AF4-459C-A663-748CBF024BD2", "UpdateAllocationStatus" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Year Offset", "Whether you need a custom offset in for your staff's Years Worked calculations", 7, @"0", "203CF1E3-F20B-4FC7-8D5E-ECFCA4C3CB47", "YearOffset" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Person Hours Per Week Worked Attribute", "The Person Attribute that contains the Number of Hours a person works in a week.  This will be used to calculate how many hours of PTO some gets allocated.  If someone only works 20hrs a week they'll only be allocated 50% of what their bracket calls for.  If left blank, everyone will recieve full allocations", 8, @"", "D4F2C07A-17C0-47A3-B5E8-B5D1173C36FE", "HoursWorked" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "com.bemaservices.HrManagement.Jobs.ProcessPtoAllocations", "Hours Per Week", "This is used in conjuntion with the Hours a person works.  This is the number of hours someone should work to recieve the full PTO Allocation", 9, @"40", "6AB8FD2A-C03F-4A4F-BDD9-3844A2A04188", "HoursPerWeek" );

            var jobId = SqlScalar( "Select Top 1 Id from ServiceJob Where Guid = '48AB813A-0038-4F4F-B3F7-9CDB975B19A1'" );
            if ( jobId != null )
            {
                var jobIdInt = jobId.ToString().AsIntegerOrNull();
                if ( jobIdInt.HasValue )
                {
                    RockMigrationHelper.AddAttributeValue( "2533F72C-57EB-4022-BD9E-8492B4516DF7", jobIdInt.Value, SystemGuid.Attribute.HIRE_DATE_PERSON_ATTRIBUTE.ToLower(), "2533F72C-57EB-4022-BD9E-8492B4516DF7" ); // Process PTO Allocations: Person Hired Date Attribute
                    RockMigrationHelper.AddAttributeValue( "6C8A6197-42C1-40A4-A752-8900EB2FCF46", jobIdInt.Value, SystemGuid.Attribute.FIRE_DATE_PERSON_ATTRIBUTE.ToLower(), "6C8A6197-42C1-40A4-A752-8900EB2FCF46" ); ; // Process PTO Allocations: Person Fired Date Attribute
                    RockMigrationHelper.AddAttributeValue( "6D67B840-6EC4-451A-8743-6D48CF2BAC39", jobIdInt.Value, @"2", "6D67B840-6EC4-451A-8743-6D48CF2BAC39" ); // Process PTO Allocations: New Allocations Status
                    RockMigrationHelper.AddAttributeValue( "C97F8E2A-FF03-443C-AF6E-427EFD81884D", jobIdInt.Value, @"30", "C97F8E2A-FF03-443C-AF6E-427EFD81884D" ); // Process PTO Allocations: Days Back
                    RockMigrationHelper.AddAttributeValue( "57F6E158-2AF4-459C-A663-748CBF024BD2", jobIdInt.Value, @"True", "57F6E158-2AF4-459C-A663-748CBF024BD2" ); // Process PTO Allocations: Update Allocation Status
                    RockMigrationHelper.AddAttributeValue( "203CF1E3-F20B-4FC7-8D5E-ECFCA4C3CB47", jobIdInt.Value, @"0", "203CF1E3-F20B-4FC7-8D5E-ECFCA4C3CB47" ); // Process PTO Allocations: Year Offset
                    RockMigrationHelper.AddAttributeValue( "6AB8FD2A-C03F-4A4F-BDD9-3844A2A04188", jobIdInt.Value, @"40", "6AB8FD2A-C03F-4A4F-BDD9-3844A2A04188" ); // Process PTO Allocations: Hours Per Week
                }
            }
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
