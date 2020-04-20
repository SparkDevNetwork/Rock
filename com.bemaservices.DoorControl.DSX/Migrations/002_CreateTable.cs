using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.DoorControl.DSX.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    public class CreateTable : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [StartDateTime] [datetime] NOT NULL,
                    [StartAction] [int] NOT NULL,
	                [EndDateTime] [datetime] NOT NULL,
	                [EndAction] [int] NOT NULL,
	                [ReservationId] [int] NULL,
                    [OverrideGroup] [int] NOT NULL,
	                [LocationId] [int] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_DoorLock] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_ModifiedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_DoorLock_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Reservation] ([Id])
            " );
        }

        public override void Down()
        {

        }
    }
}
