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
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace rocks.pillars.Jobs
{
    /// <summary>
    /// Sends Group Scheduling Confirmation and Reminder emails to people that haven't been notified yet.
    /// Copy of the core job except that it will resovle lava merge fields for the From Email for each System communication
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
        private static class AttributeKey
        {
            /// <summary>
            /// The root group
            /// </summary>
            public const string RootGroup = "RootGroup";

            public const string FromPersonAttKey = "FromPersonAttKey";
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
                    var groupChildrenIds = groupService.GetAllDescendentGroupIds( parentGroup.Id, false );
                    groupIds.AddRange( groupChildrenIds );
                    sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                var currentDate = RockDateTime.Now.Date;

                // limit to confirmation offset window
                sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery
                    .Where( a => a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.HasValue )
                    .Where( a => System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate ) <= a.Occurrence.Group.GroupType.ScheduleConfirmationEmailOffsetDays.Value );

                List<string> errorMessages;
                _groupScheduleConfirmationsSent = SendScheduleConfirmationSystemEmails( sendConfirmationAttendancesQuery, rockContext, out errorMessages );
                rockContext.SaveChanges();

                if ( errorMessages.Any() )
                {
                    throw new Exception( "One or more errors occurred when sending confirmation emails: " + Environment.NewLine + errorMessages.AsDelimited( Environment.NewLine ) );
                }
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
                    var groupChildrenIds = groupService.GetAllDescendentGroupIds( parentGroup.Id, false );
                    groupIds.AddRange( groupChildrenIds );
                    sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => groupIds.Contains( a.Occurrence.GroupId.Value ) );
                }

                // limit to ones that have an offset window for either the GroupType or for the Person in the group
                sendReminderAttendancesQuery = sendReminderAttendancesQuery
                 .Where( a => a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays.HasValue
                     || ( a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId ).OrderBy( r => r.GroupRole.IsLeader ).FirstOrDefault().ScheduleReminderEmailOffsetDays.HasValue ) );

                // limit to ones within offset
                sendReminderAttendancesQuery = sendReminderAttendancesQuery.Where( a => System.Data.Entity.SqlServer.SqlFunctions.DateDiff( "day", currentDate, a.Occurrence.OccurrenceDate )
                    <= ( ( a.Occurrence.Group.Members.Where( m => m.PersonId == a.PersonAlias.PersonId ).OrderBy( r => r.GroupRole.IsLeader ).FirstOrDefault().ScheduleReminderEmailOffsetDays ?? a.Occurrence.Group.GroupType.ScheduleReminderEmailOffsetDays ) ) );

                _groupScheduleRemindersSent = SendScheduleReminderSystemEmails( sendReminderAttendancesQuery, rockContext );
                rockContext.SaveChanges();
            }
        }


        public int SendScheduleConfirmationSystemEmails(IQueryable<Attendance> sendConfirmationAttendancesQuery, RockContext rockContext, out List<string> errorMessages)
        {
            int emailsSent = 0;
            errorMessages = new List<string>();

            sendConfirmationAttendancesQuery = sendConfirmationAttendancesQuery.Where(a =>
               a.PersonAlias.Person.Email != null
               && a.PersonAlias.Person.Email != string.Empty
               && a.PersonAlias.Person.EmailPreference != EmailPreference.DoNotEmail
               && a.PersonAlias.Person.IsEmailActive);

            var sendConfirmationAttendancesQueryList = sendConfirmationAttendancesQuery.ToList();
            var attendancesBySystemEmailTypeList = sendConfirmationAttendancesQueryList.GroupBy(a => a.Occurrence.Group.GroupType.ScheduleConfirmationSystemCommunicationId).Where(a => a.Key.HasValue).Select(s => new
            {
                ScheduleConfirmationSystemCommunicationId = s.Key.Value,
                Attendances = s.ToList()
            }).ToList();

            List<Exception> exceptionList = new List<Exception>();

            foreach (var attendancesBySystemEmailType in attendancesBySystemEmailTypeList)
            {
                var scheduleConfirmationSystemEmail = new SystemCommunicationService(rockContext).GetNoTracking(attendancesBySystemEmailType.ScheduleConfirmationSystemCommunicationId);

                var attendancesByPersonList = attendancesBySystemEmailType.Attendances.GroupBy(a => a.PersonAlias.Person).Select(s => new
                {
                    Person = s.Key,
                    Attendances = s.ToList()
                });

                foreach (var attendancesByPerson in attendancesByPersonList)
                {
                    try
                    {
                        var emailMessage = new RockEmailMessage(scheduleConfirmationSystemEmail);
                        var recipient = attendancesByPerson.Person;
                        var attendances = attendancesByPerson.Attendances;

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null);
                        mergeFields.Add("Attendance", attendances.FirstOrDefault());
                        mergeFields.Add("Attendances", attendances);

                        emailMessage.FromEmail = emailMessage.FromEmail.ResolveMergeFields(mergeFields);
                        emailMessage.AddRecipient(new RockEmailMessageRecipient(recipient, mergeFields));
                        List<string> sendErrors;
                        bool sendSuccess = emailMessage.Send(out sendErrors);

                        if (sendSuccess)
                        {
                            emailsSent++;
                            foreach (var attendance in attendances)
                            {
                                attendance.ScheduleConfirmationSent = true;
                            }
                        }
                        else
                        {
                            errorMessages.AddRange(sendErrors);
                        }
                    }
                    catch (Exception ex)
                    {
                        var emailException = new Exception($"Exception occurred when trying to send Schedule Confirmation Email to { attendancesByPerson.Person }", ex);
                        errorMessages.Add(emailException.Message);
                        exceptionList.Add(emailException);
                    }
                }
            }

            // group messages that are exactly the same and put a count of those in the message
            errorMessages = errorMessages.GroupBy(a => a).Select(s => s.Count() > 1 ? $"{s.Key}  ({s.Count()})" : s.Key).ToList();

            if (exceptionList.Any())
            {
                ExceptionLogService.LogException(new AggregateException("Errors Occurred sending schedule confirmation emails", exceptionList));
            }

            return emailsSent;
        }


        public int SendScheduleReminderSystemEmails(IQueryable<Attendance> sendReminderAttendancesQuery, RockContext rockContext)
        {
            int emailsSent = 0;
            var sendReminderAttendancesQueryList = sendReminderAttendancesQuery.ToList();
            var attendancesBySystemEmailTypeList = sendReminderAttendancesQueryList.GroupBy(a => a.Occurrence.Group.GroupType.ScheduleReminderSystemCommunicationId).Where(a => a.Key.HasValue).Select(s => new
            {
                ScheduleReminderSystemCommunicationId = s.Key.Value,
                Attendances = s.ToList()
            }).ToList();

            foreach (var attendancesBySystemEmailType in attendancesBySystemEmailTypeList)
            {
                var scheduleReminderSystemEmail = new SystemCommunicationService(rockContext).GetNoTracking(attendancesBySystemEmailType.ScheduleReminderSystemCommunicationId);

                var attendancesByPersonList = attendancesBySystemEmailType.Attendances.GroupBy(a => a.PersonAlias.Person).Select(s => new
                {
                    Person = s.Key,
                    Attendances = s.ToList()
                });

                foreach (var attendancesByPerson in attendancesByPersonList)
                {
                    try
                    {
                        var emailMessage = new RockEmailMessage(scheduleReminderSystemEmail);
                        var recipient = attendancesByPerson.Person;
                        var attendances = attendancesByPerson.Attendances;

                        foreach (var attendance in attendances)
                        {
                            attendance.ScheduleReminderSent = true;
                        }

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null);
                        mergeFields.Add("Attendance", attendances.FirstOrDefault());
                        mergeFields.Add("Attendances", attendances);

                        emailMessage.FromEmail = emailMessage.FromEmail.ResolveMergeFields(mergeFields);
                        emailMessage.AddRecipient(new RockEmailMessageRecipient(recipient, mergeFields));
                        emailMessage.Send();
                        emailsSent++;
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogService.LogException(new Exception($"Exception occurred trying to send SendScheduleReminderSystemEmails to { attendancesByPerson.Person }", ex));
                    }
                }
            }

            return emailsSent;
        }
    }
}
