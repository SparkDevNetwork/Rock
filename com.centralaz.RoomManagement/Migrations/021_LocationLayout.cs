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

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 21, "1.6.0" )]
    public class LocationLayout : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [IsSystem] [bit] NOT NULL,
                    [LocationId] [int] NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [Description] [nvarchar](max) NULL,
	                [IsActive] [bit] NOT NULL,
	                [IsDefault] [bit] NOT NULL,
                    [LayoutPhotoId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_LocationLayout] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_LocationId] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_LocationId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_ModifiedByPersonAliasId]
" );
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] ADD [LocationLayoutId] INT NULL

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_LocationLayoutId] FOREIGN KEY([LocationLayoutId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_LocationLayout] ([Id])
" );
            // Page: Named Locations
            RockMigrationHelper.UpdateBlockType( "Location Layout List", "A list of layouts tied to a location", "~/Plugins/com_centralaz/RoomManagement/LocationLayoutList.ascx", "com_centralaz > Room Management", "AA41242C-DF95-40E2-B184-0E024A07FDFF" );
            // Add Block to Page: Named Locations, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2BECFB85-D566-464F-B6AC-0BE90189A418","","AA41242C-DF95-40E2-B184-0E024A07FDFF","Location Layout List","Main","","",1,"BA2F9650-C9E9-4819-8293-445AC14DAD81");   
            // Attrib for BlockType: Location Layout List:Layout Image Height
            RockMigrationHelper.UpdateBlockTypeAttribute("AA41242C-DF95-40E2-B184-0E024A07FDFF","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Layout Image Height","LayoutImageHeight","","",0,@"150","CB24F528-929E-45D5-BDB2-96DADD53BDEE");  
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "CB24F528-929E-45D5-BDB2-96DADD53BDEE" );
            RockMigrationHelper.DeleteBlock( "BA2F9650-C9E9-4819-8293-445AC14DAD81" );
            RockMigrationHelper.DeleteBlockType( "AA41242C-DF95-40E2-B184-0E024A07FDFF" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationLocation_LocationLayoutId]
                ALTER TABLE[dbo].[_com_centralaz_RoomManagement_ReservationLocation] DROP COLUMN[LocationLayoutId]" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_LocationId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_LocationLayout_ModifiedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_LocationLayout]" );
        }
    }
}