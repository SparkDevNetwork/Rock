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
    [MigrationNumber( 7, "1.9.4" )]
    public class OptionalSetupPhoto : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // AddColumn is currently only available in Rock v7
            //AddColumn( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "SetupPhotoId", c => c.Int() );
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [SetupPhotoId] INT
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_SetupPhoto] FOREIGN KEY([SetupPhotoId])
                REFERENCES [dbo].[BinaryFile] ([Id])
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            // DropColumn & DropForeignKey are currently only available in Rock v7
            //DropForeignKey( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "SetupPhotoId" );
            //DropColumn( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "SetupPhotoId" );
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT  [FK__com_bemaservices_RoomManagement_Reservation_SetupPhoto]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN [SetupPhotoId]
" );
        }
    }
}
