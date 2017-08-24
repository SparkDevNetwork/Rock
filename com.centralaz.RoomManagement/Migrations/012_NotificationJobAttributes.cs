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
using Rock.Model;
using Rock.Plugin;
using Rock;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 12, "1.6.0" )]
    public class NotificationJobAttributes : Migration
    {
        public override void Up()
        {
            var serviceJob = new ServiceJobService( new Rock.Data.RockContext() ).Get( "6832E24B-5650-41D3-9EBA-1D2D213F768C".AsGuid() );

            RockMigrationHelper.AddAttributeValue( "8F3BEC15-A076-4C07-8047-D85C319F8DBF", serviceJob.Id, @"Upcoming|1|Day||", "8F3BEC15-A076-4C07-8047-D85C319F8DBF" ); // Reservation Reminder: Date Range

            RockMigrationHelper.AddAttributeValue( "3E394836-2175-4C5B-9247-063BCB6CD6D2", serviceJob.Id, @"True", "3E394836-2175-4C5B-9247-063BCB6CD6D2" ); // Reservation Reminder: Include only reservations that start in date range

            RockMigrationHelper.AddAttributeValue( "E14B7BA0-77D8-4553-B2E0-070FF3ECA34E", serviceJob.Id, @"a219357d-4992-415e-bf5f-33c242bb3bd2", "E14B7BA0-77D8-4553-B2E0-070FF3ECA34E" ); // Reservation Reminder: Workflow Type

            RockMigrationHelper.AddAttributeValue( "49D7EDB4-E2FB-4081-B4D5-43D8217BF105", serviceJob.Id, @"2", "49D7EDB4-E2FB-4081-B4D5-43D8217BF105" ); // Reservation Reminder: Reservation Statuses

        }

        public override void Down()
        {
        }
    }
}
