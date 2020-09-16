﻿// <copyright>
// Copyright by the Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 1, "1.4.5" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [IsSystem] [bit] NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [Description] [nvarchar] NULL,
	                [IsCritical] [bit] NOT NULL,
	                [IsDefault] [bit] NOT NULL,
	                [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationStatus] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [Description] [nvarchar] NULL,
	                [Order] [int] NOT NULL,
	                [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationMinistry] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_Reservation](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [ScheduleId] [int] NOT NULL,
	                [CampusId] [int] NULL,
	                [ReservationMinistryId] [int] NULL,
	                [ReservationStatusId] [int] NULL,
                    [RequesterAliasId] [int] NULL,
	                [ApproverAliasId] [int] NULL,
	                [SetupTime] [int] NULL,
	                [CleanupTime] [int] NULL,
	                [NumberAttending] [int] NULL,
	                [IsApproved] [bit] NULL,
	                [Note] [nvarchar](50) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_Reservation] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Schedule] FOREIGN KEY([ScheduleId])
                REFERENCES [dbo].[Schedule] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Schedule]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Campus] FOREIGN KEY([CampusId])
                REFERENCES [dbo].[Campus] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Campus]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationMinistry] FOREIGN KEY([ReservationMinistryId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationMinistry]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationStatus] FOREIGN KEY([ReservationStatusId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_ReservationStatus] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationStatus]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ApproverAliasId] FOREIGN KEY([ApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ApproverAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_RequesterAliasId] FOREIGN KEY([RequesterAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_RequesterAliasId] 

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reservation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK_dbo.Reservation_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Reservation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] CHECK CONSTRAINT [FK_dbo.Reservation_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_Resource](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [CategoryId] [int] NULL,
	                [CampusId] [int] NULL,
                    [Quantity] [int] NOT NULL,
	                [Note] [nvarchar](50) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_Resource] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Category] FOREIGN KEY([CategoryId])
                REFERENCES [dbo].[Category] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Category]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Campus] FOREIGN KEY([CampusId])
                REFERENCES [dbo].[Campus] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Campus]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Resource_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK_dbo.Resource_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Resource_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK_dbo.Resource_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [ResourceId] [int] NOT NULL,
                    [Quantity] [int] NOT NULL,
	                [IsApproved] [bit] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationResource] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Reservation]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Resource] FOREIGN KEY([ResourceId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Resource] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Resource]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ModifiedByPersonAliasId]

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [LocationId] [int] NOT NULL,
	                [IsApproved] [bit] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationLocation] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Reservation]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Location]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReservationLocation_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK_dbo.ReservationLocation_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ReservationLocation_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK_dbo.ReservationLocation_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [WorkflowTypeId] [int] NOT NULL,
	                [TriggerType] [int] NOT NULL,
	                [QualifierValue] [nvarchar](max) NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [ForeignKey] [nvarchar](100) NULL,
	                [ForeignGuid] [uniqueidentifier] NULL,
	                [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationWorkflowTrigger] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
                REFERENCES [dbo].[WorkflowType] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId] 

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [ReservationWorkflowTriggerId] [int] NOT NULL,
	                [WorkflowId] [int] NOT NULL,
	                [TriggerType] [int] NOT NULL,
	                [TriggerQualifier] [nvarchar](max) NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [ForeignKey] [nvarchar](100) NULL,
	                [ForeignGuid] [uniqueidentifier] NULL,
	                [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationWorkflow] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationId] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Reservation] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId] FOREIGN KEY([ReservationWorkflowTriggerId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] ([Id])                

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId]
" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.Reservation", "839768A3-10D6-446C-A65B-B8F9EFD7808F", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationLocation", "07084E96-2907-4741-80DF-016AB5981D12", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationMinistry", "5DFCA44E-7090-455C-8C7B-D02CF6331A0F", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationResource", "A9A1F735-0298-4137-BCC1-A9117B6543C9", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationStatus", "5241B2B1-AEF2-4EB9-9737-55604069D93B", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationWorkflow", "3660E6A9-B3DA-4CCB-8FC8-B182BC1A2587", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationWorkflowTrigger", "CD0C935B-C3EF-465B-964E-A3AB686D8F51", true, true );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.Resource", "35584736-8FE2-48DA-9121-3AFD07A2DA8D", true, true );

            RockMigrationHelper.UpdateFieldType( "ReservationStatuses", "", "com.centralaz.RoomManagement", "com.centralaz.RoomManagement.Field.Types.ReservationStatusesFieldType", "335E190C-88FE-4BE2-BE36-3F8B85AF39F2" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteFieldType( "335E190C-88FE-4BE2-BE36-3F8B85AF39F2" );

            RockMigrationHelper.DeleteEntityType( "839768A3-10D6-446C-A65B-B8F9EFD7808F" );
            RockMigrationHelper.DeleteEntityType( "07084E96-2907-4741-80DF-016AB5981D12" );
            RockMigrationHelper.DeleteEntityType( "5DFCA44E-7090-455C-8C7B-D02CF6331A0F" );
            RockMigrationHelper.DeleteEntityType( "A9A1F735-0298-4137-BCC1-A9117B6543C9" );
            RockMigrationHelper.DeleteEntityType( "5241B2B1-AEF2-4EB9-9737-55604069D93B" );
            RockMigrationHelper.DeleteEntityType( "3660E6A9-B3DA-4CCB-8FC8-B182BC1A2587" );
            RockMigrationHelper.DeleteEntityType( "CD0C935B-C3EF-465B-964E-A3AB686D8F51" );
            RockMigrationHelper.DeleteEntityType( "35584736-8FE2-48DA-9121-3AFD07A2DA8D" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_ReservationId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Reservation]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_Location]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Reservation]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationResource_Resource]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationResource]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Campus]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Category]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_Resource]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_RequesterAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ApproverPersonId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationStatus]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationMinistry]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Campus]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_Schedule]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_Reservation]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationMinistry_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationMinistry]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus]
" );
        }
    }
}
