// <copyright>
// Copyright by BEMA Software Services
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

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 1, "1.9.4" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationStatus] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationMinistry] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_Reservation] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Schedule] FOREIGN KEY([ScheduleId])
                REFERENCES [dbo].[Schedule] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Schedule]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Campus] FOREIGN KEY([CampusId])
                REFERENCES [dbo].[Campus] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Campus]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationMinistry] FOREIGN KEY([ReservationMinistryId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationMinistry]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationStatus] FOREIGN KEY([ReservationStatusId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationStatus] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationStatus]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ApproverAliasId] FOREIGN KEY([ApproverAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ApproverAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_RequesterAliasId] FOREIGN KEY([RequesterAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_RequesterAliasId] 

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_Resource](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_Resource] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Category] FOREIGN KEY([CategoryId])
                REFERENCES [dbo].[Category] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Category]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Campus] FOREIGN KEY([CampusId])
                REFERENCES [dbo].[Campus] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Campus]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationResource] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Resource] FOREIGN KEY([ResourceId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Resource] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Resource]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ModifiedByPersonAliasId]

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLocation] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_ModifiedByPersonAliasId]
" );
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId] FOREIGN KEY([WorkflowTypeId])
                REFERENCES [dbo].[WorkflowType] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId] 

" );
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationWorkflow] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationId] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId] FOREIGN KEY([ReservationWorkflowTriggerId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ([Id])                

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_WorkflowId]
" );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.Reservation'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.Reservation", "839768A3-10D6-446C-A65B-B8F9EFD7808F", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationLocation'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationLocation", "07084E96-2907-4741-80DF-016AB5981D12", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationMinistry'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationMinistry", "5DFCA44E-7090-455C-8C7B-D02CF6331A0F", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationResource'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationResource", "A9A1F735-0298-4137-BCC1-A9117B6543C9", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationStatus'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationStatus", "5241B2B1-AEF2-4EB9-9737-55604069D93B", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationWorkflow'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationWorkflow", "3660E6A9-B3DA-4CCB-8FC8-B182BC1A2587", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationWorkflowTrigger'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationWorkflowTrigger", "CD0C935B-C3EF-465B-964E-A3AB686D8F51", true, true );

            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.Resource'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.Resource", "35584736-8FE2-48DA-9121-3AFD07A2DA8D", true, true );

            Sql( @"
                    Delete
                    From FieldType
                    Where Class = 'com.bemaservices.RoomManagement.Field.Types.ReservationStatusesFieldType'
                " );
            UpdateFieldTypeByGuid( "ReservationStatuses", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationStatusesFieldType", "335E190C-88FE-4BE2-BE36-3F8B85AF39F2" );
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
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_WorkflowId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationWorkflowTriggerId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflow_ReservationId]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_WorkflowTypeId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Reservation]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocation_Location]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Reservation]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationResource_Resource]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationResource]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Campus]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Resource] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Resource_Category]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_Resource]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_RequesterAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ApproverPersonId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationStatus]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationMinistry]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Campus]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_Schedule]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationStatus_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationStatus]
" );
        }

        public void UpdateEntityTypeByGuid( string name, string guid, bool isEntity, bool isSecured )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [IsEntity] = {1},
                        [IsSecured] = {2},
                        [Name] = '{0}'
                    WHERE [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}' )
                    BEGIN
                        UPDATE [EntityType] SET
                            [IsEntity] = {1},
                            [IsSecured] = {2},
                            [Guid] = '{3}'
                        WHERE [Name] = '{0}'
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [EntityType] ([Name], [IsEntity], [IsSecured], [IsCommon], [Guid])
                        VALUES ('{0}', {1}, {2}, 0, '{3}')
                    END
                END
",
                name,
                isEntity ? "1" : "0",
                isSecured ? "1" : "0",
                guid ) );
        }

        public void UpdateEntityTypeByGuid( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{5}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [FriendlyName] = '{1}',
                        [AssemblyName] = '{2}',
                        [IsEntity] = {3},
                        [IsSecured] = {4},
                        [Name] = '{0}'
                    WHERE [Guid] = '{5}'
                END
                ELSE
                BEGIN
                    DECLARE @Guid uniqueidentifier
                    SET @Guid = (SELECT [Guid] FROM [EntityType] WHERE [Name] = '{0}')
                    IF @Guid IS NULL
                    BEGIN
                        INSERT INTO [EntityType] (
                            [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[IsCommon],[Guid])
                        VALUES(
                            '{0}','{1}','{2}',{3},{4},0,'{5}')
                    END
                    ELSE
                    BEGIN

                        UPDATE [EntityType] SET
                            [FriendlyName] = '{1}',
                            [AssemblyName] = '{2}',
                            [IsEntity] = {3},
                            [IsSecured] = {4},
                            [Guid] = '{5}'
                        WHERE [Name] = '{0}'

                        -- Update any attribute values that might have been using entity's old guid value
	                    DECLARE @EntityTypeFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.EntityTypeFieldType' )
	                    DECLARE @ComponentFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentFieldType' )
	                    DECLARE @ComponentsFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentsFieldType' )

                        UPDATE V SET [Value] = REPLACE( LOWER([Value]), LOWER(CAST(@Guid AS varchar(50))), LOWER('{5}') )
	                    FROM [AttributeValue] V
	                    INNER JOIN [Attribute] A ON A.[Id] = V.[AttributeId]
	                    WHERE ( A.[FieldTypeId] = @EntityTypeFieldTypeId OR A.[FieldTypeId] = @ComponentFieldTypeId	OR A.[FieldTypeId] = @ComponentsFieldTypeId )
                        OPTION (RECOMPILE)

                    END
                END
",
                    name.Replace( "'", "''" ),
                    friendlyName.Replace( "'", "''" ),
                    assemblyName.Replace( "'", "''" ),
                    isEntity ? "1" : "0",
                    isSecured ? "1" : "0",
                    guid ) );
        }

        public void UpdateFieldTypeByGuid( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [FieldType] SET
                        [Name] = '{0}',
                        [Description] = '{1}',
                        [Guid] = '{4}',
                        [IsSystem] = {5},
                        [Assembly] = '{2}',
                        [Class] = '{3}'
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    DECLARE @Id int
                    SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = '{2}' AND [Class] = '{3}')
                    IF @Id IS NULL
                    BEGIN
                        INSERT INTO [FieldType] (
                            [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                        VALUES(
                            '{0}','{1}','{2}','{3}','{4}',{5})
                    END
                    ELSE
                    BEGIN
                        UPDATE [FieldType] SET
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [Guid] = '{4}',
                            [IsSystem] = {5}
                        WHERE [Assembly] = '{2}'
                        AND [Class] = '{3}'
                    END
                END
",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    assembly,
                    className,
                    guid,
                    IsSystem ? "1" : "0" ) );
        }

        public void UpdateEntityAttributeByGuid( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description, int order, string defaultValue, string guid, string key = null )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                key = name.Replace( " ", string.Empty );
            }

            Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [Guid] = '{7}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [EntityTypeId] = @EntityTypeId,
                        [EntityTypeQualifierColumn] = '{8}',
                        [EntityTypeQualifierValue] = '{9}',
                        [Key] = '{2}'
                    WHERE [Guid] = '{7}'
                END
                ELSE
                BEGIN
                    IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = '{9}'
                    AND [Key] = '{2}' )
                    BEGIN
                        UPDATE [Attribute] SET
                            [Name] = '{3}',
                            [Description] = '{4}',
                            [Order] = {5},
                            [DefaultValue] = '{6}',
                            [Guid] = '{7}'
                        WHERE [EntityTypeId] = @EntityTypeId
                        AND [EntityTypeQualifierColumn] = '{8}'
                        AND [EntityTypeQualifierValue] = '{9}'
                        AND [Key] = '{2}'
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [Attribute] (
                            [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                            [Key],[Name],[Description],
                            [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                            [Guid])
                        VALUES(
                            1,@FieldTypeId,@EntityTypeid,'{8}','{9}',
                            '{2}','{3}','{4}',
                            {5},0,'{6}',0,0,
                            '{7}')
                    END
                END
",
                    entityTypeName,
                    fieldTypeGuid,
                    key,
                    name,
                    description?.Replace( "'", "''" ) ?? string.Empty,
                    order,
                    defaultValue?.Replace( "'", "''" ) ?? string.Empty,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
        }

    }
}
