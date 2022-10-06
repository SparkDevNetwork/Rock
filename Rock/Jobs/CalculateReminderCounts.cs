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
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Logging;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to calculate metric values for metrics that are based on a schedule and have a database or sql datasource type
    /// Only Metrics that need to be populated (based on their Schedule) will be processed
    /// </summary>
    [DisplayName( "Calculate Reminder Counts" )]
    [Description( "A job that calculates the updates the reminder count value for people with active reminders." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for any SQL based operations to complete. Leave blank to use the default for this job (300). Note, some metrics do not use SQL so this timeout will only apply to metrics that are SQL based.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 5,
        Category = "General",
        Order = 1 )]

    [DisallowConcurrentExecution]
    public class CalculateReminderCounts : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateReminderCounts()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var currentDate = RockDateTime.Now;
            RockLogger.Log.Debug( RockLogDomains.Jobs, $"CalculateReminderCounts job started at {currentDate}." );

            var dataMap = context.JobDetail.JobDataMap;
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var reminderService = new ReminderService( rockContext );
                var personService = new PersonService( rockContext );

                var activeReminders = reminderService
                    .Queryable().AsNoTracking()                 // Get reminders that:
                    .Where( r => r.ReminderType.IsActive        // have active reminder types,
                            && !r.IsComplete                    // are not complete,
                            && r.ReminderDate <= currentDate ); // are active (reminder date has passed).

                // Locate people who currently have a reminder count greater than zero but shouldn't.
                var peopleWithNoReminders = personService.Queryable()
                    .Where( p => p.ReminderCount != null
                                && p.ReminderCount > 0
                                && !activeReminders.Select( r => r.PersonAlias.PersonId ).Contains( p.Id ) );

                int zeroedCount = peopleWithNoReminders.Count();
                RockLogger.Log.Debug( RockLogDomains.Jobs, $"CalculateReminderCounts job:  Resetting {zeroedCount} reminder counts to 0." );

                rockContext.BulkUpdate( peopleWithNoReminders, p => new Person { ReminderCount = 0 } );
                rockContext.SaveChanges();

                // Update individual reminder accounts with correct values.
                var reminderCounts = activeReminders.GroupBy( r => r.PersonAlias.PersonId )
                    .ToDictionary( a => a.Key, a => a.Count() );

                int updatedCount = 0;
                foreach ( var personId in reminderCounts.Keys )
                {
                    var person = personService.Get( personId );
                    if ( person.ReminderCount != reminderCounts[personId])
                    {
                        updatedCount++;
                        person.ReminderCount = reminderCounts[personId];
                        rockContext.SaveChanges();
                    }
                }

                RockLogger.Log.Debug( RockLogDomains.Jobs, $"CalculateReminderCounts job:  Updated {updatedCount} reminder counts." );
            }
        }
    }
}
