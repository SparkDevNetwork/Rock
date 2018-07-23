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
                SystemGuid.ServiceJob.MIGRATE_HISTORY_SUMMARY_DATA.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_ATTENDANCE_OCCURRENCE.AsGuid(),
                SystemGuid.ServiceJob.MIGRATE_FAMILY_CHECKIN_IDS.AsGuid(),
            };

            var runOnceJobIds = new Model.ServiceJobService( new Rock.Data.RockContext() ).Queryable().Where( a => runOnceJobGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();

            foreach ( var runOnceJobId in runOnceJobIds )
            {
                new Transactions.RunJobNowTransaction( runOnceJobId ).Enqueue();
            }
        }
    }
}
