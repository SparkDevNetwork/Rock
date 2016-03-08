// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AttendanceAnalyticsQuery : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_Indexes );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_GroupTypeAttendance );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_AttendeeDates );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_AttendeeFirstDates );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_Attendees );
            Sql( MigrationSQL._201602161036322_AttendanceAnalyticsQuery_NonAttendees );

            //DT: Hide unnamed schedules from check-in schedule configuration
            RockMigrationHelper.AddBlockAttributeValue( "0AEF4F84-4365-484A-8B0F-EAA20E992EC4", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
