﻿// <copyright>
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
using System.Runtime.Serialization.Formatters;
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
        /// A GUID list of data migration jobs that run automatically run once after an update and then delete themselves from the ServiceJob table.
        /// The jobs would be executed in the same order as they are specified in the list.
        /// </summary>
        public static List<Guid> startupRunOnceJobGuids = new List<Guid>
        {
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_SHORTENED_LINKS_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_120_UPDATE_INTERACTION_INDEXES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_120_ADD_COMMUNICATION_GET_QUEUED_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_124_UPDATE_GROUP_SALUTATIONS.AsGuid(),
            SystemGuid.ServiceJob.POST_INSTALL_DATA_MIGRATIONS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_127_REBUILD_GROUP_SALUTATIONS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_136_FIX_INCORRECT_ERA_START_DATE.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_140_ADD_MISSING_MEDIA_ELEMENT_INTERACTIONS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_140_UPDATE_CURRENT_SESSIONS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_140_CREATE_FK_INDEXES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_CURRENT_SESSIONS_1900.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_141_ADD_MISSING_INDEXES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_VALUEAS_ATTRIBUTE_VALUE_COLUMNS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_SLIDING_DATE_RANGE_VALUE.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_141_RECREATE_METRIC_ANALYTICS_VIEWS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_150_SYSTEM_PHONE_NUMBERS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_TRANSACTION_ENTRY_BLOCKS_WITH_UTILITY_PAYMENT_ENTRY_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_150_MOBILE_APPLICATION_USERS_REST_GROUP.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_151_DUPLICATE_MOBILE_INTERACTIONS_CLEANUP.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_152_IX_VALUE_AS_PERSON_ID.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_MOVE_PERSON_PREFERENCES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_SESSION_START_DATE_KEY.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_INTERACTION_CHANNEL_ID.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_AND_INTERACTION_INDICES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_POPULATE_INTERACTION_SESSION_DATA.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_ID.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_WORKFLOWID_COLUMNS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_NOTE_DATA.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_SWAP_NOTES_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_CHOP_BLOCKS_GROUP_1.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_CHOP_BLOCKS_GROUP_REGISTRATION.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_MEDIA_ELEMENT_DEFAULT_URLS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_161_SWAP_FINANCIAL_BATCH_LIST.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_161_REMOVE_OBSIDIAN_GROUP_SCHEDULE_TOOLBOX_BACK_BUTTONS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_161_CHOP_ACCOUNTENTRY_AND_LOGIN.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_166_UPDATE_ACHIEVEMENTTYPE_TARGETCOUNT_COLUMN.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_INTERACTION_CREATED_DATE_TIME_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_166_ADD_COMMUNICATION_RECIPIENT_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_166_CHOP_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_167_POPULATE_ENTITY_INTENTS_FROM_ADDITIONAL_SETTINGS_JSON.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_167_CHOP_ACCOUNT_EDIT_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_167_CHOP_PLEDGE_ENTRY_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_COMMUNICATION_RECIPIENT_LIST_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_LEGACY_PREFERENCES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_DISC_BLOCK.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_WEBFORMS_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_ADD_AND_UPDATE_PEER_NETWORK_INDEXES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_GUID.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_INTERACTION_INDEX_POST_MIGRATION_JOB.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_168_UPDATE_INDEXES.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_171_MIGRATE_LOGIN_HISTORY.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_171_UPDATE_COMMUNICATIONRECIPIENT_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_171_ADD_COMMUNICATIONRECIPIENT_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_171_POPULATE_ATTENDANCE_ROOT_GROUP_TYPE.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_171_CHOP_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_180_UPDATE_COMMUNICATIONRECIPIENT_INDEX.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_180_DELETE_GROUPLOCATIONHISTORICALSCHEDULE.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_180_POPULATE_COMMUNICATIONRECIPIENT_DELIVEREDDATETIME.AsGuid(),
        };


        /// <summary>
        /// A GUID list of data migration jobs that are scheduled to run at 2 AM when usage is low because they would affect performance
        /// </summary>
        public static List<Guid> scheduledRunOnceJobGuids = new List<Guid>
        {
            SystemGuid.ServiceJob.DATA_MIGRATIONS_122_INTERACTION_PERSONAL_DEVICE_ID.AsGuid(),
            SystemGuid.ServiceJob.DATA_MIGRATIONS_133_ADD_INTERACTION_SESSION_INTERACTION_SESSION_LOCATION_ID_INDEX.AsGuid(),
            SystemGuid.ServiceJob.POST_170_UPDATE_HISTORY_ENTITYTYPEID_INDEX.AsGuid(),
        };

        /// <summary>
        /// Method that will be run at Rock startup to run post update service jobs.
        /// These jobs are run asynchronously so Post update jobs must not be order dependent
        /// and must be able to run at the same time as other jobs.
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
                // RunJobsInIISContext isn't enabled on this server, so don't run these DataMigrationsStartups unless this is a developer environment
                if ( !System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    // RunJobsInIISContext isn't enabled, and this isn't a developer environment so exit without running DataMigrationsStartups.
                    return;
                }
            }

            // run any of the above jobs if they still exist (they haven't run and deleted themselves)
            var runOnceJobIds = GetRunOnceJobIds();

            // start a task that will run any incomplete RunOneJobs (one at a time)
            Task.Run( () =>
             {
                 ExecuteRunOnceJobs( runOnceJobIds );
             } );
        }

        /// <summary>
        /// Gets all <see cref="ServiceJob"/> identifiers for data migration
        /// jobs that still need to be run.
        /// </summary>
        /// <returns>A list of <see cref="ServiceJob"/> identifiers.</returns>
        internal static List<int> GetRunOnceJobIds()
        {
            return new Model.ServiceJobService( new Rock.Data.RockContext() ).Queryable()
                .Where( a => startupRunOnceJobGuids.Contains( a.Guid ) )
                .Select( a => new
                {
                    a.Id,
                    a.Guid
                } )
                .ToList()
                .OrderBy( j => startupRunOnceJobGuids.IndexOf( j.Guid ) )
                .Select( j => j.Id )
                .ToList();
        }

        /// <summary>
        /// Executes each of the run once data migration jobs in order.
        /// </summary>
        /// <param name="runOnceJobIds">The <see cref="ServiceJob"/> identifiers to be executed.</param>
        internal static void ExecuteRunOnceJobs( List<int> runOnceJobIds )
        {
            var rockContext = new Rock.Data.RockContext();
            var jobService = new Rock.Model.ServiceJobService( rockContext );
            foreach ( var runOnceJobId in runOnceJobIds )
            {
                try
                {
                    var job = jobService.Get( runOnceJobId );
                    jobService.RunNow( job );
                }
                catch ( Exception ex )
                {
                    // this shouldn't happen since the jobService.RunNow catches and logs errors, but just in case
                    ExceptionLogService.LogException( ex );
                }
            }
        }
    }
}
