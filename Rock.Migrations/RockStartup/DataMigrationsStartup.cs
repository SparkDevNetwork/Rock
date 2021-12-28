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
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using Rock.Model;
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
            /* 10-01-2021 MDP
              
            In multi-server/cluster/web-farm Rock environments, only one of the servers should be running
            these DataMigrationsStartups. To make sure that multiple servers don't run these, we'll
            check if this is the server with RunJobsInIISContext enabled.

            If RunJobsInIISContext isn't enabled on this server, don't run these. However, if this 
            is a developer environment, we'll want to run these regardless of the RunJobsInIISContext setting.
            
            */

            bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
            if ( !runJobsInContext )
            {
                // RunJobsInIISContext isn't enabled on this server, so don't run these DataMigrationsStartups unless
                // this is a developer environment
                if ( !System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    // RunJobsInIISContext isn't enabled, and this isn't a developer environment so exit without running DataMigrationsStartups.
                    return;
                }
            }

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
                SystemGuid.ServiceJob.DATA_MIGRATIONS_90_SCHEDULEDTRANSACTIONNOTESTOHISTORY.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_100_ATTRIBUTEVALUE_VALUEASNUMERIC.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_100_SUNDAYDATE.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_103_SPIRITUAL_GIFTS.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_DATE_KEYS.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_110_COMMUNICATIONRECIPIENT_RESPONSECODE_INDEX.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_RELATED_DATAVIEW_ID.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_120_UPDATE_INTERACTION_INDEXES.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATION_GET_QUEUED_INDEX.AsGuid(),
                
                /* MDP 07-22-2021
                
                NOTE: We intentionally are excluding SystemGuid.ServiceJob.DATA_MIGRATIONS_122_INTERACTION_PERSONAL_DEVICE_ID
                from DataMigrationStartup and will just wait for it to run at 2am.
                See https://app.asana.com/0/0/1199506067368201/f

                */
                
                SystemGuid.ServiceJob.DATA_MIGRATIONS_124_UPDATE_GROUP_SALUTATIONS.AsGuid(),
                SystemGuid.ServiceJob.POST_INSTALL_DATA_MIGRATIONS.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX.AsGuid(),
                SystemGuid.ServiceJob.DATA_MIGRATIONS_127_REBUILD_GROUP_SALUTATIONS.AsGuid(),                
                SystemGuid.ServiceJob.DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX.AsGuid()
            };

            // run any of the above jobs if they still exist (they haven't run and deleted themselves)
            var runOnceJobIds = new Model.ServiceJobService( new Rock.Data.RockContext() ).Queryable().Where( a => runOnceJobGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();

            // start a task that will run any incomplete RunOneJobs (one at a time)
            Task.Run( () =>
             {
                 var rockContext = new Rock.Data.RockContext();
                 var jobService = new Rock.Model.ServiceJobService( rockContext );
                 foreach ( var runOnceJobId in runOnceJobIds )
                 {
                     try
                     {
                         var job = jobService.Get( runOnceJobId );
                         jobService.RunNow( job, out _ );
                     }
                     catch ( Exception ex )
                     {
                         // this shouldn't happen since the jobService.RunNow catches and logs errors, but just in case
                         ExceptionLogService.LogException( ex );
                     }
                 }
             } );
        }
    }
}
