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
using System;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
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
            Sql( @"CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType](
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
 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLocationType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_CreatedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ModifiedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_ReservationTypeId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_LocationTypeValueId] FOREIGN KEY([LocationTypeValueId])
REFERENCES [dbo].[DefinedValue] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLocationType_LocationTypeValueId]
" );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationLocationType", "Reservation Location Type", "com.bemaservices.RoomManagement.Model.ReservationLocationType, com.bemaservices.RoomManagement, Version=1.2.2.0, Culture=neutral, PublicKeyToken=null", true, true, "834F278F-49E6-4BEA-B724-E7723F9EE4C9" );

            Sql( @"
INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationLocationType]
           ([LocationTypeValueId]
           ,[Guid]
           ,[ReservationTypeId])
     Select dv.Id
           ,newId()
           ,rt.Id
		   From [dbo].[_com_bemaservices_RoomManagement_ReservationType] rt
Outer Apply DefinedValue dv where dv.DefinedTypeId = (Select Top 1 Id From DefinedType Where Guid = '3285DCEF-FAA4-43B9-9338-983F4A384ABA')" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
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
    }
}