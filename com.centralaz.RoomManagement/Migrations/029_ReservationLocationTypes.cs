// <copyright>
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
using System;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 29, "1.9.4" )]
    public class ReservationLocationTypes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"CREATE TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReservationTypeId] [int] NOT NULL,
	[LocationTypeValueId] [int] NOT NULL,
	[CreatedDateTime] [datetime] NULL,
	[ModifiedDateTime] [datetime] NULL,
	[CreatedByPersonAliasId] [int] NULL,
	[ModifiedByPersonAliasId] [int] NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[ForeignKey] [nvarchar](100) NULL,
	[ForeignGuid] [uniqueidentifier] NULL,
	[ForeignId] [int] NULL,
 CONSTRAINT [PK__com_centralaz_RoomManagement_ReservationLocationType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_CreatedByPersonAliasId]

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId]

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
REFERENCES [dbo].[_com_centralaz_RoomManagement_ReservationType] ([Id])

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_ReservationTypeId]

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_LocationTypeValueId] FOREIGN KEY([LocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocationType_LocationTypeValueId]
" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.ReservationLocationType", "Reservation Location Type", "com.centralaz.RoomManagement.Model.ReservationLocationType, com.centralaz.RoomManagement, Version=1.2.2.0, Culture=neutral, PublicKeyToken=null", true, true, "834F278F-49E6-4BEA-B724-E7723F9EE4C9" );

            Sql( @"
INSERT INTO [dbo].[_com_centralaz_RoomManagement_ReservationLocationType]
           ([LocationTypeValueId]
           ,[Guid]
           ,[ReservationTypeId])
     Select dv.Id
           ,newId()
           ,rt.Id
		   From [dbo].[_com_centralaz_RoomManagement_ReservationType] rt
Outer Apply DefinedValue dv where dv.DefinedTypeId = (Select Top 1 Id From DefinedType Where Guid = '3285DCEF-FAA4-43B9-9338-983F4A384ABA')" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}