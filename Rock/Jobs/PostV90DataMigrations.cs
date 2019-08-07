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
using System.ComponentModel;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V9.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Runs data updates that need to occur after updating to v9.0" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v9.0. After all the operations are done, this job will delete itself." )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete.", false, 60 * 60, "General", 7, "CommandTimeout" )]
    public class PostV90DataMigrations : IJob
    {
        private int? _commandTimeout = null;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                // MP: Populate AnalyticsSourceDate (if it isn't already)
                if ( !rockContext.AnalyticsSourceDates.AsQueryable().Any() )
                {
                    var analyticsStartDate = new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
                    var analyticsEndDate = new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
                    Rock.Model.AnalyticsSourceDate.GenerateAnalyticsSourceDateData( 1, false, analyticsStartDate, analyticsEndDate );
                }
            }

            // MP: Migrate any pre-v9 RegistrationTemplateFee.CostValue data to RegistrationTemplateFeeItems
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var registrationTemplateFeeService = new Rock.Model.RegistrationTemplateFeeService( rockContext );

                // disable Obsolete warning since we need to run this, but don't want anybody else to start using it
#pragma warning disable 612, 618
                registrationTemplateFeeService.MigrateFeeCostValueToFeeItems();
#pragma warning restore 612, 618
                rockContext.SaveChanges();
            }


            DeleteJob( context.GetJobId() );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                    return;
                }
            }
        }

    }
}
