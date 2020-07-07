using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.ClientPackage.WUMC.Migrations
{
    [MigrationNumber( 1, "1.9.4" )]
    public class Data : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [RequestorPersonAliasId] [int] NOT NULL,
	                [AssignedPersonAliasId] [int] NULL,
	                [AssignedSupervisorPersonAliasId] [int] NULL,
	                [LocationId] [int] NULL,
	                [StatusId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_StatusId] FOREIGN KEY([StatusId])
                REFERENCES [dbo].[DefinedValue] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_StatusId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_LocationId] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_LocationId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_RequestorPersonAliasId] FOREIGN KEY([RequestorPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_RequestorPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedPersonAliasId] FOREIGN KEY([AssignedPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedSupervisorPersonAliasId] FOREIGN KEY([AssignedSupervisorPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedSupervisorPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] CHECK CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_ModifiedByPersonAliasId]
" );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.ClientPackage.WUMC.Model.MaintenanceRequest", "FE9235F1-69B7-4187-A200-00A73BAD307A", true, true );

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "FE9235F1-69B7-4187-A200-00A73BAD307A" );

            Sql( @"              
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_StatusId]
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedSupervisorPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_AssignedPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_RequestorPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest] DROP CONSTRAINT [FK__com_bemaservices_ClientPackage_WUMC_MaintenanceRequest_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_ClientPackage_WUMC_MaintenanceRequest]
" );
        }
    }
}
