using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs
{


    /// <summary>
    /// Job to process communications
    /// </summary>
    [DisplayName( "Send Attendance Reminders Based on Data View" )]
	[Description( "Sends a reminder to group leaders about entering attendance for their group meeting if their group belongs to a selected data view (and the group type takes attendance and is configured to send reminders)." )]

    #region Job Attributes

    [DataViewField( "Data View", "The Group data view that includes groups to send attendance reminders for.", true, "", "Rock.Model.Group", "", 0, AttributeKey.DataView )]

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
        DefaultValue = "1",
        Order = 3 )]

    #endregion

    [DisallowConcurrentExecution]
    public class SendAttendanceReminderBasedOnDataView : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The data view setting.
            /// </summary>
            public const string DataView = "DataView";

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
        public SendAttendanceReminderBasedOnDataView() { }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var dataMap = context.JobDetail.JobDataMap;
            var dataViewGuid = dataMap.GetString( AttributeKey.DataView).AsGuid();

            context.Result = "0 attendance reminders sent.";

            var systemEmailGuid = dataMap.GetString( AttributeKey.SystemEmail ).AsGuid();
            var systemCommunication = new SystemCommunicationService( rockContext ).Get( systemEmailGuid );

            var jobPreferredCommunicationType = ( CommunicationType ) dataMap.GetString( AttributeKey.SendUsingConfiguration ).AsInteger();
            var isSmsEnabled = MediumContainer.HasActiveSmsTransport() && !string.IsNullOrWhiteSpace( systemCommunication.SMSMessage );

            if ( jobPreferredCommunicationType == CommunicationType.SMS && !isSmsEnabled )
            {
                // If sms selected but not usable default to email.
                var errorMessages = new List<string>
                {
                    $"The job is setup to send via SMS but either SMS isn't enabled or no SMS message was found in system communication {systemCommunication.Title}."
                };
                HandleErrorMessages( context, errorMessages );
            }

            var results = new StringBuilder();
            if ( jobPreferredCommunicationType != CommunicationType.Email && string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
            {
                var warning = $"No SMS message found in system communication {systemCommunication.Title}. All attendance reminders were sent via email.";
                results.AppendLine( warning );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                jobPreferredCommunicationType = CommunicationType.Email;
            }

            var occurrences = GetOccurenceDates( dataViewGuid, dataMap, rockContext );

            // Get the groups that have occurrences
            var groupIds = occurrences.Where( o => o.Value.Any() ).Select( o => o.Key ).ToList();

            // Get the leaders of those groups
            var leaders = GetGroupLeaders( groupIds, rockContext );

            var attendanceRemindersResults = SendAttendanceReminders( leaders, occurrences, systemCommunication, jobPreferredCommunicationType, isSmsEnabled );

            results.AppendLine( $"{attendanceRemindersResults.MessagesSent} attendance reminders sent." );

            results.Append( FormatWarningMessages( attendanceRemindersResults.Warnings ) );

            context.Result = results.ToString();
            HandleErrorMessages( context, attendanceRemindersResults.Errors );
        }

        /// <summary>
        /// Gets the occurrence dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="dataMap">The data map.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, List<DateTime>> GetOccurenceDates( Guid dataViewGuid, JobDataMap dataMap, RockContext rockContext )
        {
            var dates = GetSearchDates( dataMap );
            var startDate = dates.Min();
            var endDate = dates.Max().AddDays( 1 );

            var occurrences = GetAllOccurenceDates( dataViewGuid, dates, startDate, endDate, rockContext );

            // Remove any 'occurrences' that already have attendance data entered
            RemoveGroupsWithAttendenceData( occurrences, startDate, endDate, rockContext );

            return occurrences;
        }

        /// <summary>
        /// Gets the search dates.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private List<DateTime> GetSearchDates( JobDataMap dataMap )
        {

            // Get the occurrence dates that apply
            var dates = new List<DateTime>
            {
                RockDateTime.Today
            };

            try
            {
                var reminderDays = dataMap.GetString( AttributeKey.SendReminders ).Split( ',' );
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
            catch
            {
                // if an exception occurs just use today's date.
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
        private static Dictionary<int, List<DateTime>> GetAllOccurenceDates( Guid dataViewGuid, List<DateTime> dates, DateTime startDate, DateTime endDate, RockContext rockContext )
        {
            var groupService = new GroupService( rockContext );

            // Find all 'occurrences' for the groups that occur on the affected dates
            var occurrences = new Dictionary<int, List<DateTime>>();

            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( dataViewGuid );
            if ( dataView != null )
            {
                var dvArgs = new DataViewGetQueryArgs
                {
                    DbContext = rockContext
                };
                var groupIds = dataView
                    .GetQuery( dvArgs )
                    .Select( i => i.Id );

                var groupsToRemind = groupService
                    .Queryable( "Schedule" ).AsNoTracking()
                    .Where( g =>
                        groupIds.Contains( g.Id ) &&
                        g.GroupType.TakesAttendance &&
                        g.GroupType.SendAttendanceReminder )
                    .IsActive()
                    .HasSchedule()
                    .HasActiveLeader();

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

                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                    RemoveExclusionDates( groupType, occurrences[group.Id] );
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Removes the exclusion dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="occurrences">The occurrences.</param>
        private static void RemoveExclusionDates( GroupTypeCache groupType, List<DateTime> occurrences )
        {
            foreach ( var exclusion in groupType.GroupScheduleExclusions )
            {
                if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                {
                    foreach ( var occurrenceDate in occurrences.ToList() )
                    {
                        if ( occurrenceDate >= exclusion.Start.Value &&
                            occurrenceDate < exclusion.End.Value.AddDays( 1 ) )
                        {
                            occurrences.Remove( occurrenceDate );
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
                .HasAttendeesOrDidNotOccur()
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
                        .ToList();
        }

        /// <summary>
        /// Sends the attendance reminders.
        /// </summary>
        /// <param name="leaders">The leaders.</param>
        /// <param name="occurrences">The occurrences.</param>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="jobPreferredCommunicationType">Type of the job preferred communication.</param>
        /// <param name="isSmsEnabled">if set to <c>true</c> [is SMS enabled].</param>
        /// <returns></returns>
        private SendMessageResult SendAttendanceReminders( List<GroupMember> leaders,
                        Dictionary<int, List<DateTime>> occurrences,
                        SystemCommunication systemCommunication,
                        CommunicationType jobPreferredCommunicationType,
                        bool isSmsEnabled )
        {
            var result = new SendMessageResult();

            // Loop through the leaders
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
                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, leader.Person );
                    mergeObjects.Add( "Person", leader.Person );
                    mergeObjects.Add( "Group", leader.Group );
                    mergeObjects.Add( "Occurrence", leaderOccurrence.Value.Max() );

                    var sendResult = CommunicationHelper.SendMessage( leader.Person, mediumType, systemCommunication, mergeObjects );

                    result.MessagesSent += sendResult.MessagesSent;
                    result.Errors.AddRange( sendResult.Errors );
                    result.Warnings.AddRange( sendResult.Warnings );
                }
            }
            return result;
        }

        private StringBuilder FormatWarningMessages( List<string> warnings )
        {
            return FormatMessages( warnings, "Warning" );
        }

        /// <summary>
        /// Handles the error messages.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="errorMessages">The error messages.</param>
        private void HandleErrorMessages( IJobExecutionContext context, List<string> errorMessages )
        {
            if ( errorMessages.Any() )
            {
                StringBuilder sb = new StringBuilder( context.Result.ToString() );

                sb.Append( FormatMessages( errorMessages, "Error" ) );

                var resultMessage = sb.ToString();

                context.Result = resultMessage;

                var exception = new Exception( resultMessage );

                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
        }

        private StringBuilder FormatMessages( List<string> messages, string label )
        {
            StringBuilder sb = new StringBuilder();
            if ( messages.Any() )
            {
                var pluralizedLabel = label.PluralizeIf( messages.Count > 1 );

                sb.AppendLine( $"{messages.Count} {pluralizedLabel}:" );

                messages.ForEach( w => { sb.AppendLine( w ); } );
            }
            return sb;
        }
    }
}