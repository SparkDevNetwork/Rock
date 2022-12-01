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
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A job which processes reminders, including creating appropriate notifications and updating the reminder count value
    /// for people with active reminders.
    /// </summary>
    [DisplayName( "Process Reminders" )]
    [Description( "A job which processes reminders, including creating appropriate notifications and updating the reminder count value for people with active reminders." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for any SQL based operations to complete. Leave blank to use the default for this job (300). Note, some metrics do not use SQL so this timeout will only apply to metrics that are SQL based.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 5,
        Category = "General",
        Order = 1 )]

    [SystemCommunicationField(
        "Reminder Notification",
        Key = AttributeKey.ReminderNotification,
        Description = "The System Communication template used to generate reminder notifications.  If not provided, the job will not generate reminder communications.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.REMINDER_NOTIFICATION,
        Category = "General",
        Order = 2 )]

    [DisallowConcurrentExecution]
    public class ProcessReminders : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The SQL command timeout.
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";

            /// <summary>
            /// The reminder notification.
            /// </summary>
            public const string ReminderNotification = "ReminderNotification";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessReminders()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var currentDate = RockDateTime.Now;
            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job started at {currentDate}." );

            //var dataMap = context.JobDetail.JobDataMap;
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300;
            var notificationSystemCommunicationGuid = GetAttributeValue( AttributeKey.ReminderNotification ).AsGuidOrNull();
            SystemCommunication notificationSystemCommunication = null;

            using ( var rockContext = new RockContext() )
            {
                if ( notificationSystemCommunicationGuid.HasValue )
                {
                    notificationSystemCommunication = new SystemCommunicationService( rockContext ).Get( notificationSystemCommunicationGuid.Value );
                }

                rockContext.Database.CommandTimeout = commandTimeout;

                var reminderService = new ReminderService( rockContext );
                var activeReminders = reminderService.GetActiveReminders( currentDate );
                ProcessNotifications( notificationSystemCommunication, activeReminders, rockContext );

                // Refresh active reminders, some of them may have been auto-completed by ProcessNotifications().
                activeReminders = reminderService.GetActiveReminders( currentDate );
                UpdateReminderCounts( activeReminders, rockContext );
            }

            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job completed at {RockDateTime.Now}." );
        }

        /// <summary>
        /// Processes notifications for active reminders.
        /// </summary>
        /// <param name="notificationSystemCommunication"></param>
        /// <param name="activeReminders"></param>
        /// <param name="rockContext"></param>
        private void ProcessNotifications( SystemCommunication notificationSystemCommunication, IQueryable<Reminder> activeReminders, RockContext rockContext )
        {
            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Initiated notification processing." );

            var activeReminderList = activeReminders.ToList();

            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Processing {activeReminderList.Count} reminders for notifications." );

            foreach ( var activeReminder in activeReminderList )
            {
                RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Processing Reminder {activeReminder.Id} for notifications." );

                bool notificationSent;
                if ( activeReminder.ReminderType.NotificationType == ReminderNotificationType.Workflow )
                {
                    // Create a notification workflow.
                    if ( !activeReminder.ReminderType.NotificationWorkflowTypeId.HasValue )
                    {
                        RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Notification workflow for reminder {activeReminder.Id} aborted:  The reminder type is incorrectly configured." );
                        continue;
                    }

                    notificationSent = InitiateNotificationWorkflow( activeReminder );
                }
                else
                {
                    // Default to communication.
                    var reminderEntity = new EntityTypeService( rockContext )
                        .GetEntity( activeReminder.ReminderType.EntityTypeId, activeReminder.EntityId );
                    var result = SendReminderCommunication( activeReminder, notificationSystemCommunication, reminderEntity );
                    notificationSent = ( result.MessagesSent > 0 );
                }

                if ( notificationSent && activeReminder.ReminderType.ShouldAutoCompleteWhenNotified )
                {
                    // Mark the reminder as complete.
                    activeReminder.CompleteReminder();
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Creates the notification workflow for a specific reminder.
        /// </summary>
        /// <param name="reminder"></param>
        /// <returns></returns>
        private bool InitiateNotificationWorkflow( Reminder reminder )
        {
            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Creating notification workflow for reminder {reminder.Id}." );

            try
            {
                var workflowParameters = new Dictionary<string, string>
                {
                    { "Reminder", reminder.Guid.ToString() },
                    { "ReminderType", reminder.ReminderType.Guid.ToString() },
                    { "Person", reminder.PersonAlias.Person.Guid.ToString() },
                    { "EntityTypeId", reminder.ReminderType.EntityTypeId.ToString() },
                    { "EntityId", reminder.EntityId.ToString() },
                };

                reminder.LaunchWorkflow( reminder.ReminderType.NotificationWorkflowTypeId, reminder.ToString(), workflowParameters, null );

                return true;
            }
            catch ( Exception ex )
            {
                RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Failed to create notification workflow for reminder {reminder.Id}: {ex.Message}" );
                ExceptionLogService.LogException( ex );
                return false;
            }
        }

        /// <summary>
        /// Creates a SystemCommunication notification for a specific reminder.
        /// </summary>
        /// <param name="reminder"></param>
        /// <param name="notificationSystemCommunication"></param>
        /// <param name="reminderEntity"></param>
        /// <returns></returns>
        private SendMessageResult SendReminderCommunication( Reminder reminder, SystemCommunication notificationSystemCommunication, IEntity reminderEntity )
        {
            if ( notificationSystemCommunication == null )
            {
                RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Aborted SystemCommunication notification for Reminder {reminder.Id}.  No SystemCommunication was specified." );
                return null;
            }

            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Creating SystemCommunication for reminder {reminder.Id}." );

            try
            {
                var person = reminder.PersonAlias.Person;
                var mergeFields = LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Reminder", reminder );
                mergeFields.Add( "ReminderType", reminder.ReminderType );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "EntityName", reminderEntity.ToString() );

                var mediumType = Model.Communication.DetermineMediumEntityTypeId(
                    ( int ) CommunicationType.Email,
                    ( int ) CommunicationType.SMS,
                    ( int ) CommunicationType.PushNotification,
                    person.CommunicationPreference );

                return CommunicationHelper.SendMessage( person, mediumType, notificationSystemCommunication, mergeFields );
            }
            catch ( Exception ex )
            {
                RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Failed to create SystemCommunication for reminder {reminder.Id}: {ex.Message}" );
                ExceptionLogService.LogException( ex );

                return new SendMessageResult()
                {
                    Errors = new List<string> { ex.Message },
                    Exceptions = new List<Exception> { ex },
                    MessagesSent = 0,
                    Warnings = new List<string>(),
                };
            }
        }

        /// <summary>
        /// Updates the reminder counts for people with reminders.
        /// </summary>
        /// <param name="activeReminders"></param>
        /// <param name="rockContext"></param>
        private void UpdateReminderCounts( IQueryable<Reminder> activeReminders, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );

            // Locate people who currently have a reminder count greater than zero but shouldn't.
            var peopleWithNoReminders = personService.Queryable()
                .Where( p => p.ReminderCount != null
                            && p.ReminderCount > 0
                            && !activeReminders.Select( r => r.PersonAlias.PersonId ).Contains( p.Id ) );

            int zeroedCount = peopleWithNoReminders.Count();
            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Resetting reminder counts to 0 for {zeroedCount} people." );

            rockContext.BulkUpdate( peopleWithNoReminders, p => new Person { ReminderCount = 0 } );
            rockContext.SaveChanges();

            // Update individual reminder counts with correct values.
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

            RockLogger.Log.Debug( RockLogDomains.Jobs, $"ProcessReminders job:  Updated reminder counts for {updatedCount} people." );
        }
    }
}
