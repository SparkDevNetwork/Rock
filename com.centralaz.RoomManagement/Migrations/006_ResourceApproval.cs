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
    [MigrationNumber( 6, "1.4.5" )]
    public class ResourceApproval : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteSecurityAuth( "0AD21D60-C750-4FD7-9D89-106692854BA4" );
            RockMigrationHelper.DeleteSecurityAuth( "CCBC6C0C-EEDB-4B55-9D07-528253624FD0" );

            RockMigrationHelper.DeleteEntityType( "5241B2B1-AEF2-4EB9-9737-55604069D93B" );
            RockMigrationHelper.DeleteFieldType( "335E190C-88FE-4BE2-BE36-3F8B85AF39F2" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationStatus]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus]
                " );


            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] ADD ApprovalGroupId [int] NULL;

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup] FOREIGN KEY([ApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
                " );

            Sql( @"
            ALTER TABLE [_com_centralaz_RoomManagement_ReservationResource] DROP COLUMN IsApproved;
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationResource] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationLocation] DROP COLUMN IsApproved;
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationLocation] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Reservation] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Reservation] DROP COLUMN IsApproved;
                " );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Location", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "", "", "Approval Group", "If this resource requires special approval, select the group in charge of approving it here.", 100, null, "96C07909-E34A-4379-854F-C05E79F772E4" );
        }
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] DROP COLUMN ApprovalGroupId
            " );
        }
    }
}
