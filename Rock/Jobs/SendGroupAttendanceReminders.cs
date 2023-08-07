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
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to process communications
    /// </summary>
    [DisplayName( "Send Group Attendance Reminders" )]
    [Description( "Sends a reminder to group leaders about entering attendance for their group meeting.  This job is meant to run many times per day and will only send reminders to groups when the configured time has passed.  By default, this job runs every 15 minutes, and it will send reminders to any group whose time threshold has passed since the last time it ran." )]

    #region Job Attributes

    [CustomDropdownListField( "Send Using",
        Description = "Specifies how the reminder will be sent.",
        Key = AttributeKey.SendUsing,
        ListSource = "1^Email,2^SMS,0^Recipient Preference",
        IsRequired = true,
        DefaultValue = "1",
        Order = 1 )]

    #endregion Job Attributes

    public class SendGroupAttendanceReminders : RockJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Job Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The method to use when determining how the notice should be sent.
            /// </summary>
            public const string SendUsing = "SendUsing";
        }

        #endregion Attribute Keys

        private List<string> _jobWarnings = new List<string>();
        private List<string> _jobErrors = new List<string>();
        private List<string> _jobCompletions = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupAttendanceReminders"/> class.
        /// </summary>
        public SendGroupAttendanceReminders() { }

        /// <inheritdoc cref="RockJob.Execute()" />
        public override void Execute()
        {
            var rockContext = new RockContext();

            var jobPreferredCommunicationType = ( CommunicationType ) GetAttributeValue( AttributeKey.SendUsing ).AsInteger();
            if ( jobPreferredCommunicationType != CommunicationType.Email )
            {
                var smsTransportEnabled = MediumContainer.HasActiveSmsTransport();
                if ( !smsTransportEnabled )
                {
                    var message = "The job is configured to send reminders via SMS, but SMS transport is not enabled.";

                    if ( jobPreferredCommunicationType == CommunicationType.SMS )
                    {
                        Log( RockLogLevel.Error, message );

                        // Halt job execution until the system is correctly configured.
                        throw new Exception( message );
                    }
                    else
                    {
                        _jobWarnings.Add( message );
                        Log( RockLogLevel.Warning, message );

                        // Force the job to use email, since SMS is not available.
                        jobPreferredCommunicationType = CommunicationType.Email;
                    }
                }
            }

            var groupTypeQry = new GroupTypeService( rockContext ).Queryable()
                .Where( t => t.TakesAttendance && t.SendAttendanceReminder )
                .Include( t => t.AttendanceReminderSystemCommunication );

            var groupTypeList = groupTypeQry.ToList();
            foreach ( var groupType in groupTypeList )
            {
                SendRemindersForGroupType( groupType, jobPreferredCommunicationType );
            }

            this.Result = GetJobResultMessages();
        }

        /// <summary>
        /// Gets the job result messages.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetJobResultMessages()
        {
            var results = new StringBuilder();
            int errorCount = _jobErrors.Count;
            int warningCount = _jobWarnings.Count;

            if ( errorCount > 0 && warningCount > 0 )
            {
                results.AppendLine(
                    $"The job completed with {errorCount} {"error".PluralizeIf( errorCount != 1 )}"
                    + $"and {warningCount} {"warning".PluralizeIf( warningCount != 1 )}." );
            }
            else if ( errorCount > 0 )
            {
                results.AppendLine( $"The job completed with {errorCount} {"error".PluralizeIf( errorCount != 1 )}." );
                results.AppendLine();
            }
            else if ( warningCount > 0 )
            {
                results.AppendLine( $"The job completed with {warningCount} {"warning".PluralizeIf( warningCount != 1 )}." );
                results.AppendLine();
            }
            else if ( _jobCompletions.Any() )
            {
                results.AppendLine( "The job completed successfully." );
                results.AppendLine();
            }
            else
            {
                results.AppendLine( "No attendance reminders were scheduled to be sent." );
            }

            if ( errorCount > 0 )
            {
                _jobErrors.Take( 5 ).ToList().ForEach( e => results.AppendLine( $"Error: {e}" ) );

                if ( errorCount > 5 )
                {
                    results.AppendLine( "Additional errors have been truncated.  To see the full list, enable error level logging for jobs ." );
                }

                results.AppendLine();
            }

            if ( warningCount > 0 )
            {
                _jobWarnings.Take( 5 ).ToList().ForEach( w => results.AppendLine( $"Warning: {w}" ) );

                if ( warningCount == 5 )
                {
                    results.AppendLine( "Additional warnings have been truncated.  To see the full list, enable warning level logging for jobs ." );
                }

                results.AppendLine();
            }

            _jobCompletions.ForEach( c => results.AppendLine( c ) );

            return results.ToString();
        }

        /// <summary>
        /// Sends the type of the reminders for group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="jobPreferredCommunicationType">Type of the job preferred communication.</param>
        private void SendRemindersForGroupType( GroupType groupType, CommunicationType jobPreferredCommunicationType )
        {
            var rockContext = new RockContext();
            var isGroupTypeValid = groupType.AttendanceReminderSystemCommunicationId.HasValue;

            if ( !isGroupTypeValid )
            {
                var error = $"Cannot send attendance reminders for group type {groupType.Name}.  The group type does not have an attendance reminder system communication.";
                _jobErrors.Add( error );
                Log( RockLogLevel.Error, error );
                return;
            }

            var systemCommunication = new SystemCommunicationService( rockContext ).Get( groupType.AttendanceReminderSystemCommunicationId.Value );
            if ( jobPreferredCommunicationType != CommunicationType.Email && string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
            {
                var warning = $"The job is configured to send reminders via SMS, but no SMS message "
                    + $"was found in system communication {systemCommunication.Title}.  Reminders for "
                    + $"group type {groupType.Name} will be sent by email.";
                _jobWarnings.Add( warning );
                Log( RockLogLevel.Warning, warning );
                jobPreferredCommunicationType = CommunicationType.Email;
            }

            var occurrences = GetOccurenceDates( groupType, rockContext );
            var groupIds = occurrences.Where( o => o.Value.Any() ).Select( o => o.Key ).ToList();
            var leaders = GetGroupLeaders( groupIds, rockContext );
            var attendanceRemindersResults = SendAttendanceReminders( leaders, occurrences, systemCommunication, jobPreferredCommunicationType );

            // If we got any warnings or errors while trying to send the email, we'll report those as
            // job warnings.  These aren't reported as errors, because they probably aren't critical.
            attendanceRemindersResults.Errors.ForEach( error => {
                var warningText = $"Error sending reminders for group type {groupType.Name}: " + error;
                _jobWarnings.Add( warningText );
                Log( RockLogLevel.Warning, warningText );
            } );

            attendanceRemindersResults.Warnings.ForEach( warning => {
                var warningText = $"Warning sending reminders for group type {groupType.Name}: " + warning;
                _jobWarnings.Add( warningText );
                Log( RockLogLevel.Warning, warningText );
            } );

            var reminderCount = attendanceRemindersResults.MessagesSent;
            if ( reminderCount > 0 )
            {
                _jobCompletions.Add( $"{reminderCount} attendance {"reminder".PluralizeIf( reminderCount != 1 )} sent for group type {groupType.Name}." );
            }
        }

        /// <summary>
        /// Gets the occurrence dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, List<DateTime>> GetOccurenceDates( GroupType groupType, RockContext rockContext )
        {
            var dates = GetSearchDates( groupType );
            var startDate = dates.Min();
            var endDate = dates.Max().AddDays( 1 );

            var occurrences = GetAllOccurenceDates( groupType, dates, startDate, endDate, rockContext );

            // Remove any occurrences during group type exclusion date ranges
            RemoveExclusionDates( groupType, occurrences );

            // Remove any 'occurrences' that already have attendance data entered
            RemoveGroupsWithAttendenceData( occurrences, startDate, endDate, rockContext );

            return occurrences;
        }

        /// <summary>
        /// Gets the search dates for a <see cref="GroupType"/>.
        /// </summary>
        /// <param name="groupType">The <see cref="GroupType"/>.</param>
        /// <returns></returns>
        private List<DateTime> GetSearchDates( GroupType groupType )
        {
            // Get the occurrence dates that apply
            var dates = new List<DateTime>
            {
                RockDateTime.Today // Include today by default.
            };

            foreach ( var reminderDay in groupType.AttendanceReminderFollowupDaysList )
            {
                var reminderDate = RockDateTime.Today.AddDays( 0 - reminderDay );
                if ( !dates.Contains( reminderDate ) )
                {
                    dates.Add( reminderDate );
                }
            }

            return dates;
        }

        /// <summary>
        /// Gets all occurence dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="dates">The dates.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static Dictionary<int, List<DateTime>> GetAllOccurenceDates(
            GroupType groupType,
            List<DateTime> dates,
            DateTime startDate,
            DateTime endDate,
            RockContext rockContext )
        {
            var groupService = new GroupService( rockContext );

            // Find all 'occurrences' for the groups that occur on the affected dates
            var occurrences = new Dictionary<int, List<DateTime>>();
            var groupsToRemind = groupService
                .Queryable( "Schedule" ).AsNoTracking()
                .IsGroupType( groupType.Id )
                .IsActive()
                .HasSchedule()
                .HasActiveLeader();

            var currentTime = RockDateTime.Now.TimeOfDay;
            int groupTypeOffSetMinutes = groupType.AttendanceReminderSendStartOffsetMinutes ?? 0;
            var reminderTimeOffset = new TimeSpan( 0, groupTypeOffSetMinutes, 0 );

            foreach ( var group in groupsToRemind )
            {
                // Add the group 
                occurrences.Add( group.Id, new List<DateTime>() );

                // Check for a iCal schedule
                if ( !string.IsNullOrWhiteSpace( group.Schedule.iCalendarContent ) )
                {
                    // If schedule has an iCal schedule, get occurrences between first and last dates
                    foreach ( var occurrence in group.Schedule.GetICalOccurrences( startDate, endDate ) )
                    {
                        var startTime = occurrence.Period.StartTime.Value;
                        var sendRemindersStartingAt = startTime.TimeOfDay.Subtract( reminderTimeOffset );
                        if ( dates.Contains( startTime.Date ) && currentTime >= sendRemindersStartingAt )
                        {
                            // Only include this occurrence if the current time is later than the time reminders
                            // should be sent for the group.
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

                                var sendRemindersStartingAt = startTime.TimeOfDay.Subtract( reminderTimeOffset );
                                if ( currentTime >= sendRemindersStartingAt )
                                {
                                    // Only include this occurrence if the current time is later than the time reminders
                                    // should be sent for the group.
                                    occurrences[group.Id].Add( startTime );
                                }
                            }
                        }
                    }
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Removes the exclusion dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="occurrences">The occurrences.</param>
        private void RemoveExclusionDates( GroupType groupType, Dictionary<int, List<DateTime>> occurrences )
        {
            foreach ( var exclusion in groupType.GroupScheduleExclusions )
            {
                if ( exclusion.StartDate.HasValue && exclusion.EndDate.HasValue )
                {
                    foreach ( var keyVal in occurrences )
                    {
                        foreach ( var occurrenceDate in keyVal.Value.ToList() )
                        {
                            if ( occurrenceDate >= exclusion.StartDate.Value &&
                                occurrenceDate < exclusion.EndDate.Value.AddDays( 1 ) )
                            {
                                keyVal.Value.Remove( occurrenceDate );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the groups with attendence data.
        /// </summary>
        /// <param name="occurrences">The occurrences.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="rockContext">The rock context.</param>
        private void RemoveGroupsWithAttendenceData( Dictionary<int, List<DateTime>> occurrences, DateTime startDate, DateTime endDate, RockContext rockContext )
        {
            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            var occurrencesWithAttendence = attendanceOccurrenceService
                .Queryable()
                .AsNoTracking()
                .DateInRange( startDate, endDate )
                .GroupIdsInList( occurrences.Keys.ToList() )
                .HasScheduleId()
                .HasAttendeesOrDidNotOccurOrRemindersAlreadySentToday()
                .Select( a => new
                {
                    GroupId = a.GroupId.Value,
                    a.OccurrenceDate
                } )
                .Distinct()
                .ToList();

            foreach ( var occurrence in occurrencesWithAttendence )
            {
                occurrences[occurrence.GroupId].RemoveAll( d => d.Date == occurrence.OccurrenceDate.Date );
            }
        }

        /// <summary>
        /// Gets the group leaders.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<GroupMember> GetGroupLeaders( List<int> groupIds, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            return groupMemberService
                        .Queryable( "Group,Person,Person.PhoneNumbers" )
                        .AsNoTracking()
                        .InGroupList( groupIds )
                        .AreActive()
                        .AreLeaders()
                        .Include( gm => gm.Group )
                        .Include( gm => gm.Group.Schedule )
                        .ToList();
        }

        /// <summary>
        /// Sends the attendance reminders.
        /// </summary>
        /// <param name="leaders">The leaders.</param>
        /// <param name="occurrences">The occurrences.</param>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="jobPreferredCommunicationType">Type of the job preferred communication.</param>
        /// <returns></returns>
        private SendMessageResult SendAttendanceReminders( List<GroupMember> leaders,
                        Dictionary<int, List<DateTime>> occurrences,
                        SystemCommunication systemCommunication,
                        CommunicationType jobPreferredCommunicationType )
        {
            var result = new SendMessageResult();

            foreach ( var leader in leaders )
            {
                var mediumType = Rock.Model.Communication.DetermineMediumEntityTypeId(
                    ( int ) CommunicationType.Email,
                    ( int ) CommunicationType.SMS,
                    ( int ) CommunicationType.PushNotification,
                    jobPreferredCommunicationType,
                    leader.CommunicationPreference,
                    leader.Person.CommunicationPreference );

                var leaderOccurrences = occurrences.Where( o => o.Key == leader.GroupId );
                foreach ( var leaderOccurrence in leaderOccurrences )
                {
                    var occurrenceDate = leaderOccurrence.Value.Max();
                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, leader.Person );
                    mergeObjects.Add( "Person", leader.Person );
                    mergeObjects.Add( "Group", leader.Group );
                    mergeObjects.Add( "Occurrence", occurrenceDate );

                    var sendResult = CommunicationHelper.SendMessage( leader.Person, mediumType, systemCommunication, mergeObjects );

                    result.MessagesSent += sendResult.MessagesSent;
                    result.Errors.AddRange( sendResult.Errors );
                    result.Warnings.AddRange( sendResult.Warnings );

                    if ( sendResult.MessagesSent > 0 )
                    {
                        // Set the AttendanceOccurrence.AttendanceReminderLastSentDateTime field so future runs
                        // know we already sent reminders for this ocurrence today.
                        CreateAttendanceOccurenceForSentReminders( occurrenceDate, leader );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the attendance occurence for sent reminders (so that we know not to send the same reminder again today).
        /// </summary>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="leader">The leader.</param>
        private void CreateAttendanceOccurenceForSentReminders( DateTime occurrenceDate, GroupMember leader )
        {
            /*
                3/16/2023 - SMC
                The purpose of this method is to store the current time on an AttendanceOccurrence in the
                AttendanceReminderLastSentDateTime field, so that we know attendance reminders have already
                been sent for this occurrence today (so that when the job runs again on the same day, it does
                not re-send the same notifications it already sent.

                This method will first look for an existing AttendanceOccurrence record before creating one.
                Note that we cannot use the AttendanceOccurrenceService.GetOrAdd() methods because the job is
                unaware of the location, but we may need to read existing occurrences that have a location.

                Reason: Tracking sent attendance reminders.
            */

            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                // Check for an existing attendance occurrence record.
                var attendanceOccurrence = attendanceOccurrenceService.Queryable()
                    .Where(
                        o => o.OccurrenceDate == occurrenceDate.Date
                        && o.GroupId == leader.GroupId
                        && o.ScheduleId == leader.Group.ScheduleId
                    ).FirstOrDefault();

                if ( attendanceOccurrence == null )
                {
                    // Create the AttendanceOccurrence record for this occurrence.
                    attendanceOccurrence = new AttendanceOccurrence
                    {
                        OccurrenceDate = occurrenceDate,
                        GroupId = leader.GroupId,
                        ScheduleId = leader.Group.ScheduleId,
                    };
                    attendanceOccurrenceService.Add( attendanceOccurrence );
                }

                attendanceOccurrence.AttendanceReminderLastSentDateTime = RockDateTime.Now;

                rockContext.SaveChanges();
            }
        }
    }
}