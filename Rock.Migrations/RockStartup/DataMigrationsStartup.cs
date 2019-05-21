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
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Utility;

namespace Rock.Migrations.RockStartup
{
    /// <summary>
    /// Starts any 'Run-Once' migration jobs that haven't been run yet
    /// </summary>
    /// <seealso cref="Rock.Utility.IRockStartup" />
    public class DataMigrationsStartup : IRockStartup
    {
        public int StartupOrder => 0;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            List<Guid> runOnceJobGuids = new List<Guid>
            {
                SystemGuid.ServiceJob.DATA_MIGRATIONS_74.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_80.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_84.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_90_DISC.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_90.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_HISTORY_SUMMARY_DATA.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_ATTENDANCE_OCCURRENCE.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_FAMILY_CHECKIN_IDS.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_INTERACTIONS_DATA.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_COMMUNICATION_MEDIUM_DATA.AsGuid()
            };

            // run any of the above jobs if they still exist (they haven't run and deleted themselves)
            var runOnceJobIds = new Model.ServiceJobService( new Rock.Data.RockContext() ).Queryable().Where( a => runOnceJobGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();

            foreach ( var runOnceJobId in runOnceJobIds )
            {
                new Transactions.RunJobNowTransaction( runOnceJobId ).Enqueue();
            }
        }
    }
}
