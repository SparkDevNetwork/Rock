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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.ComponentModel;
using System.Linq;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v17.3 to update Nameless Schedules.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.3 - Update Nameless Schedules Job" )]
    [Description( "This job will update Nameless Schedules to store a Friendly Name in their Description Column." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV173UpdateNamelessSchedules : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var scheduleService = new ScheduleService( rockContext );
                var namelessSchedules = scheduleService.Queryable()
                    .Where( s => ( s.Name == null || s.Name == string.Empty ) && ( s.Description == null || s.Description == string.Empty ) )
                    .ToList();

                foreach ( var schedule in namelessSchedules )
                {
                    // Set the Description to a friendly text representation of the schedule.
                    schedule.Description = schedule.ToFriendlyScheduleText( true );
                }

                rockContext.SaveChanges();
            }

            DeleteJob();
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
