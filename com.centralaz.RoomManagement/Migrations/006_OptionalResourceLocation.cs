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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 6, "1.6.0" )]
    public class OptionalResourceLocation : Migration
    {
        public override void Up()
        {
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Resource]", "LocationId", c => c.Int() );
            //AddForeignKey( "[dbo].[_com_centralaz_RoomManagement_Resource]", "LocationId", "dbo.Location", "Id" );
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])
" );
        }
        public override void Down()
        {
            DropForeignKey( "[dbo].[_com_centralaz_RoomManagement_Resource]", "LocationId" );
            DropColumn( "[dbo].[_com_centralaz_RoomManagement_Resource]", "LocationId" );
        }
    }
}
