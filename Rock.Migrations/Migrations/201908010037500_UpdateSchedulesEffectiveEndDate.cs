// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Rock;
    using Rock.Data;
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateSchedulesEffectiveEndDate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var rockContext = new RockContext();
            var recurring = new ScheduleService( rockContext ).Queryable().Where( s => s.IsActive ).Where( s => s.iCalendarContent.Contains( "RRULE" ) || s.iCalendarContent.Contains( "RDATE" ) ).ToList();
            foreach ( var schedule in recurring )
            {
                schedule.PreSaveChanges( rockContext, EntityState.Modified );
            }

            rockContext.SaveChanges();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
