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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Send Group Schedule Confirmation and Reminder Emails" )]
    [Description( "Sends Group Scheduling Confirmation and Reminder emails to people that haven't been notified yet." )]

    [GroupField(
        "Group",
        Key = AttributeKey.RootGroup,
        Description = "Only people in or under this group will receive the schedule notifications emails.",
        IsRequired = false,
        Order = 0 )]
    public class SendGroupScheduleNotifications : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Job Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The root group
            /// </summary>
            public const string RootGroup = "RootGroup";
        }

        #endregion Attribute Keys

        #region Fields

        private int _groupScheduleConfirmationsSent = 0;
        private int _groupScheduleRemindersSent = 0;

        #endregion Fields

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

            SendGroupScheduleConfirmationEmails( rootGroupGuid );
            SendGroupScheduleReminderEmails( rootGroupGuid );
        }

        /// <summary>
        /// Sends the group schedule confirmation emails.
        /// </summary>
        /// <param name="rootGroupGuid">The root group unique identifier.</param>
        private void SendGroupScheduleConfirmationEmails( System.Guid? rootGroupGuid )
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
                    var groupChildrenIds = groupService.GetAllDescendents( parentGroup.Id ).Select( g => g.Id ).ToArray();
                    groupIds.AddRange( groupChildrenIds );
                    sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                var currentDate = RockDateTime.Now.Date;

                // limit to confirmation offset window
                sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery
                    .Where( a => a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.HasValue )
                    .Where( a => System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate ) <= a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.Value );

                _groupScheduleConfirmationsSent = attendanceService.SendScheduleConfirmationSystemEmails( sendConfirmationAttendancesQuery );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Sends the group schedule reminder emails.
        /// </summary>
        /// <param name="rootGroupGuid">The root group unique identifier.</param>
        private void SendGroupScheduleReminderEmails( System.Guid? rootGroupGuid )
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
                    var groupChildrenIds = groupService.GetAllDescendents( parentGroup.Id ).Select( g => g.Id ).ToArray();
                    groupIds.AddRange( groupChildrenIds );
                    sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                // limit to ones that have an offset window for either the GroupType or for the Person in the group
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                 .Where( a => a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays.HasValue
                     || ( a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId ).OrderBy(r => r.GroupRole.IsLeader).FirstOrDefault().ScheduleReminderEmailOffsetDays.HasValue ) );

                // limit to ones within offset
                sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate )
                    <= ( ( a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId ).OrderBy( r => r.GroupRole.IsLeader ).FirstOrDefault().ScheduleReminderEmailOffsetDays ?? a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays) ) );

                _groupScheduleRemindersSent = attendanceService.SendScheduleReminderSystemEmails( sendReminderAttendancesQuery );
                rockContext.SaveChanges();
            }
        }
    }
}
