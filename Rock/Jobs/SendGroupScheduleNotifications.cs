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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends Group Scheduling Confirmations and Reminders to people that haven't been notified yet.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Send Group Schedule Confirmations and Reminders" )]
    [Description( "Sends Group Scheduling Confirmations and Reminders to people that haven't been notified yet. Only Email and SMS are supported. PUSH is not supported." )]

    [GroupField(
        "Group",
        Key = AttributeKey.RootGroup,
        Description = "Only people in or under this group will receive the schedule notifications.",
        IsRequired = false,
        Order = 0 )]
    public class SendGroupScheduleNotifications : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Job Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The root group
            /// </summary>
            public const string RootGroup = "RootGroup";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendGroupScheduleNotifications()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rootGroupGuid = context.JobDetail.JobDataMap.GetString( AttributeKey.RootGroup ).AsGuidOrNull();

            var confirmationSends = SendGroupScheduleConfirmationCommunications( rootGroupGuid );
            var reminderSends = SendGroupScheduleReminderCommunications( rootGroupGuid );

            var exceptionMessage = string.Empty;
            if ( confirmationSends.Errors.Any() )
            {
                exceptionMessage = "One or more errors occurred when sending confirmations: " + Environment.NewLine + confirmationSends.Errors.AsDelimited( Environment.NewLine );
            }

            if ( reminderSends.Errors.Any() )
            {
                if ( exceptionMessage.IsNotNullOrWhiteSpace() )
                {
                    exceptionMessage += Environment.NewLine;
                }

                exceptionMessage += "One or more errors occurred when sending reminders: " + Environment.NewLine + reminderSends.Errors.AsDelimited( Environment.NewLine );
            }

            if ( exceptionMessage.IsNotNullOrWhiteSpace() )
            {
                throw new Exception( exceptionMessage );
            }

            context.Result = $@"{confirmationSends.MessagesSent} confirmation messages were sent.
                                {reminderSends.MessagesSent} reminder messages were sent.";
        }

        /// <summary>
        /// Sends the group schedule confirmations.
        /// </summary>
        /// <param name="rootGroupGuid">The root group unique identifier.</param>
        private SendMessageResult SendGroupScheduleConfirmationCommunications( System.Guid? rootGroupGuid )
        {
            List<Person> personsScheduled = new List<Person>();
            using ( var rockContext = new RockContext() )
            {
                List<int> groupIds = new List<int>();
                var groupService = new GroupService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                // Get all who have not already been notified( attendance.ScheduleConfirmationSent = false ) and who have been requested to attend.
                var sendConfirmationAttendancesQuery = new AttendanceService( rockContext )
                    .GetPendingScheduledConfirmations()
                    .Where( a => a.ScheduleConfirmationSent != true );

                // if the root group is configured on the Job then limit to the group and its child groups
                if ( rootGroupGuid.HasValue )
                {
                    var parentGroup = groupService.Get( rootGroupGuid.Value );
                    groupIds.Add( parentGroup.Id );
                    var groupChildrenIds = groupService.GetAllDescendentGroupIds( parentGroup.Id, false );
                    groupIds.AddRange( groupChildrenIds );
                    sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                var currentDate = RockDateTime.Now.Date;

                // limit to confirmation offset window
                sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery
                    .Where( a => a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.HasValue )
                    .Where( a => System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate ) <= a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.Value );

                var messageResult = attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery );
                rockContext.SaveChanges();

                return messageResult;
            }
        }

        /// <summary>
        /// Sends the group schedule reminders.
        /// </summary>
        /// <param name="rootGroupGuid">The root group unique identifier.</param>
        private SendMessageResult SendGroupScheduleReminderCommunications( System.Guid? rootGroupGuid )
        {
            List<Person> personsScheduled = new List<Person>();
            using ( var rockContext = new RockContext() )
            {
                List<int> groupIds = new List<int>();
                var groupService = new GroupService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                var currentDate = RockDateTime.Now.Date;

                // Get all who have not already been notified( attendance.ScheduleReminderSent = false ) and who have been requested to attend.
                var sendReminderAttendancesQuery = new AttendanceService( rockContext )
                    .GetConfirmedScheduled()
                    .Where( a => a.Occurrence.OccurrenceDate >= currentDate )
                    .Where( a => a.ScheduleReminderSent != true );

                // if the root group is configured on the Job then limit to the group and its child groups
                if ( rootGroupGuid.HasValue )
                {
                    var parentGroup = groupService.Get( rootGroupGuid.Value );
                    groupIds.Add( parentGroup.Id );
                    var groupChildrenIds = groupService.GetAllDescendentGroupIds( parentGroup.Id, false );
                    groupIds.AddRange( groupChildrenIds );
                    sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                // limit to ones that have an offset window for either the GroupType or for the Person in the group
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                    .Where( a => a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays.HasValue
                        || a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId )
                            .OrderBy( r => r.GroupRole.IsLeader )
                            .FirstOrDefault()
                            .ScheduleReminderEmailOffsetDays.HasValue );

                // limit to ones within offset
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                    .Where( a =>
                        System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate )
                            <= ( a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId )
                                .OrderBy( r => r.GroupRole.IsLeader )
                                .FirstOrDefault()
                                .ScheduleReminderEmailOffsetDays ?? a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays ) );

                var messageResult = attendanceService.SendScheduleReminderSystemCommunication( sendReminderAttendancesQuery );
                rockContext.SaveChanges();

                return messageResult;
            }
        }
    }
}
