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
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends Group Scheduling Confirmations and Reminders to people that haven't been notified yet.
    /// </summary>
    [DisplayName( "Send Group Schedule Confirmations and Reminders" )]
    [Description( "Sends Group Scheduling Confirmations and Reminders to people that haven't been notified yet. Only Email and SMS are supported. PUSH is not supported." )]

    [GroupField(
        "Group Filter",
        Key = AttributeKey.RootGroup,
        Description = "Only people in or under this group will receive the schedule notifications.",
        IsRequired = false,
        Order = 0 )]
    [DataViewField(
        "Groups Data View Filter",
        Description = "Only groups returned from this data view will be considered (child groups are not included).",
        EntityType = typeof( Rock.Model.Group ),
        Key = AttributeKey.GroupDataView,
        IsRequired = false,
        Order = 1 )]
    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the SQL operation to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Category = "General",
        Order = 2 )]
    public class SendGroupScheduleNotifications : RockJob
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

            /// <summary>
            /// The group data view
            /// </summary>
            public const string GroupDataView = "GroupDataView";

            /// <summary>
            /// The command timeout in seconds
            /// </summary>
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            var groupDataViewGuid = GetAttributeValue( AttributeKey.GroupDataView ).AsGuidOrNull();
            var commandTimeoutSeconds = GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            var confirmationSends = SendGroupScheduleConfirmationCommunications( rootGroupGuid, groupDataViewGuid, commandTimeoutSeconds );
            var reminderSends = SendGroupScheduleReminderCommunications( rootGroupGuid, groupDataViewGuid, commandTimeoutSeconds );

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

            this.Result = $@"{confirmationSends.MessagesSent} confirmation messages were sent.
                                {reminderSends.MessagesSent} reminder messages were sent.";
        }

        /// <summary>
        /// Sends the group schedule confirmations.
        /// </summary>
        /// <param name="rootGroupGuid">The root group unique identifier.</param>
        /// <param name="groupDataViewGuid">The guid for the group data view.</param>
        /// <param name="commandTimeoutSeconds">The command timeout seconds.</param>
        private SendMessageResult SendGroupScheduleConfirmationCommunications( System.Guid? rootGroupGuid, System.Guid? groupDataViewGuid, int commandTimeoutSeconds )
        {
            List<Person> personsScheduled = new List<Person>();
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeoutSeconds;
                List<int> groupIds = new List<int>();
                var groupService = new GroupService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                // Get all who have not already been notified( attendance.ScheduleConfirmationSent = false ) and who have been requested to attend.
                var sendConfirmationAttendancesQuery = new AttendanceService( rockContext )
                    .GetPendingAndAutoAcceptScheduledConfirmations()
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

                // if the group data view guid is configured on the Job then limit to selected groups in the data view not considering child groups
                if ( groupDataViewGuid.HasValue )
                {
                    var groupDataView = new DataViewService( rockContext ).Get( groupDataViewGuid.Value );
                    var groupsQuery = groupDataView.GetQuery( new DataViewGetQueryArgs { DatabaseTimeoutSeconds = commandTimeoutSeconds } ) as IQueryable<Group>;
                    groupIds.AddRange( groupsQuery.Select( a => a.Id ).ToList() );
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
        /// <param name="groupDataViewGuid">The guid for the group data view.</param>
        /// <param name="commandTimeoutSeconds">The command timeout seconds.</param>
        private SendMessageResult SendGroupScheduleReminderCommunications( System.Guid? rootGroupGuid, System.Guid? groupDataViewGuid, int commandTimeoutSeconds )
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

                // if the group data view guid is configured on the Job then limit to selected groups in the data view not considering child groups
                if ( groupDataViewGuid.HasValue )
                {
                    var groupDataView = new DataViewService( rockContext ).Get( groupDataViewGuid.Value );
                    var groupsQuery = groupDataView.GetQuery( new DataViewGetQueryArgs { DatabaseTimeoutSeconds = commandTimeoutSeconds } ) as IQueryable<Group>;
                    groupIds.AddRange( groupsQuery.Select( a => a.Id ).ToList() );
                    sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                // limit to ones that have an offset window for either the GroupType or for the Person in the group
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                    .Where( a => a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays.HasValue
                        || a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId )
                            .OrderBy( r => r.GroupRole.IsLeader )
                            .FirstOrDefault()
                            .ScheduleReminderEmailOffsetDays.HasValue );

                // filter out any Group Member that has selected "Do not send a reminder"
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                    .Where( a => a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId )
                            .OrderBy( r => r.GroupRole.IsLeader )
                            .FirstOrDefault()
                            .ScheduleReminderEmailOffsetDays != -1 );

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
