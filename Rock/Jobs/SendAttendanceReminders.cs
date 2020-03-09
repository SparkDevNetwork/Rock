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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process communications
    /// </summary>
    [GroupTypeField( "Group Type", "The Group type to send attendance reminders for.", true, Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP, "", 0, AttributeKey.GroupType )]

    #region Job Attributes
    [SystemCommunicationField( "System Communication",
        "The system communication to use when sending reminder.",
        true,
        Rock.SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER,
        "",
        1,
        AttributeKey.SystemEmail )] // NOTE: This key is different than the label!

    [TextField( "Send Reminders", "Comma delimited list of days after a group meets to send an additional reminder. For example, a value of '2,4' would result in an additional reminder getting sent two and four days after group meets if attendance was not entered.", false, "", "", 2, AttributeKey.SendReminders )]

    [CustomDropdownListField(
        "Send Using",
        "Specifies how the reminder will be sent.",
        "1^Email,2^SMS,0^Recipient Preference",
        Key = AttributeKey.SendUsingConfiguration,
        IsRequired = true,
        DefaultValue = "0",
        Order = 3 )]
    #endregion

    [DisallowConcurrentExecution]
    public class SendAttendanceReminder : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The group type setting.
            /// </summary>
            public const string GroupType = "GroupType";

            /// <summary>
            /// The system communication attribute setting-key.
            /// </summary>
            public const string SystemEmail = "SystemEmail";

            /// <summary>
            /// The send reminders 'days before' comma delimited setting
            /// </summary>
            public const string SendReminders = "SendReminders";

            /// <summary>
            /// The method to use when determining how the notice should be sent.
            /// </summary>
            public const string SendUsingConfiguration = "SendUsingConfiguration";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendAttendanceReminder()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupType = GroupTypeCache.Get( dataMap.GetString( AttributeKey.GroupType ).AsGuid() );
            var sendUsingConfiguration = ( CommunicationType ) dataMap.GetString( AttributeKey.SendUsingConfiguration ).AsInteger();


            int attendanceRemindersSent = 0;
            int errorCount = 0;
            var errorMessages = new List<string>();

            if ( groupType.TakesAttendance && groupType.SendAttendanceReminder )
            {

                // Get the occurrence dates that apply
                var dates = new List<DateTime>();
                dates.Add( RockDateTime.Today );
                try
                {
                    string[] reminderDays = dataMap.GetString( AttributeKey.SendReminders ).Split( ',' );
                    foreach ( string reminderDay in reminderDays )
                    {
                        if ( reminderDay.Trim().IsNotNullOrWhiteSpace() )
                        {
                            var reminderDate = RockDateTime.Today.AddDays( 0 - Convert.ToInt32( reminderDay ) );
                            if ( !dates.Contains( reminderDate ) )
                            {
                                dates.Add( reminderDate );
                            }
                        }
                    }
                }
                catch { }

                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var scheduleService = new ScheduleService( rockContext );
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                var startDate = dates.Min();
                var endDate = dates.Max().AddDays( 1 );

                // Find all 'occurrences' for the groups that occur on the affected dates
                var occurrences = new Dictionary<int, List<DateTime>>();
                foreach ( var group in groupService
                    .Queryable( "Schedule" ).AsNoTracking()
                    .Where( g =>
                        g.GroupTypeId == groupType.Id &&
                        g.IsActive &&
                        g.Schedule != null &&
                        g.Members.Any( m =>
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.GroupRole.IsLeader &&
                            m.Person.Email != null &&
                            m.Person.Email != String.Empty ) ) )
                {
                    // Add the group 
                    occurrences.Add( group.Id, new List<DateTime>() );

                    // Check for a iCal schedule
                    if ( !string.IsNullOrWhiteSpace( group.Schedule.iCalendarContent ) )
                    {
                        // If schedule has an iCal schedule, get occurrences between first and last dates
                        foreach ( var occurrence in group.Schedule.GetOccurrences( startDate, endDate ) )
                        {
                            var startTime = occurrence.Period.StartTime.Value;
                            if ( dates.Contains( startTime.Date ) )
                            {
                                occurrences[group.Id].Add( startTime );
                            }
                        }
                    }
                    else
                    {
                        // if schedule does not have an iCal, then check for weekly schedule and calculate occurrences starting with first attendance or current week
                        if ( group.Schedule.WeeklyDayOfWeek.HasValue )
                        {
                            foreach ( var date in dates )
                            {
                                if ( date.DayOfWeek == group.Schedule.WeeklyDayOfWeek.Value )
                                {
                                    var startTime = date;
                                    if ( group.Schedule.WeeklyTimeOfDay.HasValue )
                                    {
                                        startTime = startTime.Add( group.Schedule.WeeklyTimeOfDay.Value );
                                    }
                                    occurrences[group.Id].Add( startTime );
                                }
                            }
                        }
                    }
                }

                // Remove any occurrences during group type exclusion date ranges
                foreach ( var exclusion in groupType.GroupScheduleExclusions )
                {
                    if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                    {
                        foreach ( var keyVal in occurrences )
                        {
                            foreach ( var occurrenceDate in keyVal.Value.ToList() )
                            {
                                if ( occurrenceDate >= exclusion.Start.Value &&
                                    occurrenceDate < exclusion.End.Value.AddDays( 1 ) )
                                {
                                    keyVal.Value.Remove( occurrenceDate );
                                }
                            }
                        }
                    }
                }

                // Remove any 'occurrences' that already have attendance data entered
                foreach ( var occurrence in attendanceOccurrenceService
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.OccurrenceDate >= startDate &&
                        a.OccurrenceDate < endDate &&
                        a.GroupId.HasValue &&
                        occurrences.Keys.Contains( a.GroupId.Value ) &&
                        a.ScheduleId.HasValue &&
                        ( a.Attendees.Any() || ( a.DidNotOccur.HasValue && a.DidNotOccur.Value ) ) )
                    .Select( a => new
                    {
                        GroupId = a.GroupId.Value,
                        a.OccurrenceDate
                    } )
                    .Distinct()
                    .ToList() )
                {
                    occurrences[occurrence.GroupId].RemoveAll( d => d.Date == occurrence.OccurrenceDate.Date );
                }

                // Get the groups that have occurrences
                var groupIds = occurrences.Where( o => o.Value.Any() ).Select( o => o.Key ).ToList();

                // Get the leaders of those groups
                var leaders = groupMemberService
                    .Queryable( "Group,Person,Person.PhoneNumbers" ).AsNoTracking()
                    .Where( m =>
                        groupIds.Contains( m.GroupId ) &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.GroupRole.IsLeader &&
                        m.Person.Email != null &&
                        m.Person.Email != string.Empty )
                    .ToList();

                var systemEmailGuid = dataMap.GetString( AttributeKey.SystemEmail ).AsGuid();
                var systemCommunication = new SystemCommunicationService( rockContext ).Get( systemEmailGuid );

                var isSmsEnabled = MediumContainer.HasActiveSmsTransport();
                var alwaysSendEmail = !isSmsEnabled || sendUsingConfiguration == CommunicationType.Email || string.IsNullOrWhiteSpace( systemCommunication.SMSMessage );
                var alwaysSendSms = sendUsingConfiguration == CommunicationType.SMS;

                if ( sendUsingConfiguration != CommunicationType.Email && string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
                {
                    errorMessages.Add( string.Format( "No SMS message found in system communication {0}.", systemCommunication.Title ) );
                    errorCount++;
                }

                // Loop through the leaders
                foreach ( var leader in leaders )
                {
                    var groupMemberSendSms = !alwaysSendEmail && leader.CommunicationPreference == CommunicationType.SMS;
                    var personSendSms = !alwaysSendEmail
                                            && leader.CommunicationPreference == CommunicationType.RecipientPreference
                                            && leader.Person.CommunicationPreference == CommunicationType.SMS;

                    var sendSms = alwaysSendSms || groupMemberSendSms || personSendSms;

                    foreach ( var group in occurrences.Where( o => o.Key == leader.GroupId ) )
                    {
                        var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, leader.Person );
                        mergeObjects.Add( "Person", leader.Person );
                        mergeObjects.Add( "Group", leader.Group );
                        mergeObjects.Add( "Occurrence", group.Value.Max() );

                        RockMessage message = null;
                        var recipients = new List<RockMessageRecipient>();

                        if ( sendSms )
                        {
                            var phoneNumber = leader.Person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
                            var smsNumber = phoneNumber == null ? string.Empty : phoneNumber.ToSmsNumber();

                            if ( string.IsNullOrWhiteSpace( smsNumber ) || !isSmsEnabled )
                            {
                                string smsErrorMessage = GenerateSmsErrorMessage( isSmsEnabled, alwaysSendSms, leader.Person, groupMemberSendSms, personSendSms );
                                errorMessages.Add( smsErrorMessage );
                                errorCount++;
                                continue;
                            }

                            recipients.Add( new RockSMSMessageRecipient( leader.Person, smsNumber, mergeObjects ) );

                            message = new RockSMSMessage( systemCommunication );
                            message.SetRecipients( recipients );
                        }
                        else
                        {
                            recipients.Add( new RockEmailMessageRecipient( leader.Person, mergeObjects ) );

                            message = new RockEmailMessage( systemCommunication );
                            message.SetRecipients( recipients );
                        }

                        if ( message != null )
                        {
                            var errors = new List<string>();
                            if ( message.Send( out errors ) )
                            {
                                attendanceRemindersSent++;
                            }
                            else
                            {
                                errorCount += errors.Count;
                                errorMessages.AddRange( errors );
                            }
                        }
                    }
                }
            }

            context.Result = string.Format( "{0} attendance reminders sent", attendanceRemindersSent );
            if ( errorMessages.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( string.Format( "{0} Errors: ", errorCount ) );
                errorMessages.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errors = sb.ToString();
                context.Result += errors;
                var exception = new Exception( context.Result.ToString() );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
        }

        private string GenerateSmsErrorMessage( bool isSmsEnabled, bool alwaysSendSms, Person person, bool groupMemberSendSms, bool personSendSms )
        {
            var smsErrorMessage = "{0} calls for SMS, but no SMS number could be found for {1}.";
            if ( !isSmsEnabled )
            {
                smsErrorMessage = "{0} calls for SMS, but SMS is not enabled. {1} did not receive a notification.";
            }

            if ( alwaysSendSms )
            {
                smsErrorMessage = string.Format( smsErrorMessage, "Job Settings", person );
            }
            else if ( groupMemberSendSms )
            {
                smsErrorMessage = string.Format( smsErrorMessage, "Group Member Communications Settings", person );
            }
            else if ( personSendSms )
            {
                smsErrorMessage = string.Format( smsErrorMessage, "The Person's Communications Settings", person );
            }

            return smsErrorMessage;
        }
    }
}