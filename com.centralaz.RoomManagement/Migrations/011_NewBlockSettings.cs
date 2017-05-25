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
    [MigrationNumber( 11, "1.6.0" )]
    public class NewBlockSettings : Migration
    {
        public override void Up()
        {
            #region New Reservation Detail block settings

            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Setup & Cleanup Time", "RequireSetupCleanupTime", "", "Should the setup and cleanup time be required to be supplied?", 3, @"True", "A184337B-BB99-4261-A295-0F54447CF0C6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Defatult Setup & Cleanup Time", "DefaultSetupCleanupTime", "", "If you wish to default to a particular setup and cleanup time, you can supply a value here. (Use -1 to indicate no default value)", 4, @"-1", "2FA0C64D-9511-4278-9445-BD0A847EA299" );

            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "2FA0C64D-9511-4278-9445-BD0A847EA299", @"-1" ); // Defatult Setup & Cleanup Time
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "A184337B-BB99-4261-A295-0F54447CF0C6", @"True" ); // Require Setup & Cleanup Time

            DropForeignKey( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "FK__com_centralaz_RoomManagement_Reservation_ContactPersonAliasId" );
            DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPersonAliasId" );
            DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPhone" );
            DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactEmail" );

            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactPersonAliasId", c => c.Int() );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactPhone", c => c.String( maxLength: 50 ) );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactEmail", c => c.String( maxLength: 400 ) );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_EventContactPersonAliasId] FOREIGN KEY([EventContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
" );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactPersonAliasId", c => c.Int() );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactPhone", c => c.String( maxLength: 50 ) );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactEmail", c => c.String( maxLength: 400 ) );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_AdministrativeContactPersonAliasId] FOREIGN KEY([AdministrativeContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
" );

            #endregion

        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "A184337B-BB99-4261-A295-0F54447CF0C6" );
            RockMigrationHelper.DeleteAttribute( "2FA0C64D-9511-4278-9445-BD0A847EA299" );
        }
    }
}
