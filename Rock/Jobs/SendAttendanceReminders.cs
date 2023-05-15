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
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process communications
    /// </summary>
    [DisplayName( "Send Attendance Reminders for Group Type" )]
    [Description( "Sends a reminder to group leaders about entering attendance for their group meeting." )]

    #region Job Attributes

    [GroupTypeField( "Group Type",
        Description = "The Group type to send attendance reminders for.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP,
        Order = 0,
        Key = AttributeKey.GroupType )]

    [SystemCommunicationField( "System Communication",
        Description = "The system communication to use when sending reminder.",
        Key = AttributeKey.SystemEmail,
        IsRequired = true,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER,
        Order = 1 )]

    [TextField( "Send Reminders",
        Description = "Comma delimited list of days after a group meets to send an additional reminder. For example, a value of '2,4' would result in an additional reminder getting sent two and four days after group meets if attendance was not entered.",
        Key = AttributeKey.SendReminders,
        IsRequired = false,
        Order = 2 )]

    [CustomDropdownListField( "Send Using",
        Description = "Specifies how the reminder will be sent.",
        Key = AttributeKey.SendUsingConfiguration,
        ListSource = "1^Email,2^SMS,0^Recipient Preference",
        IsRequired = true,
        DefaultValue = "1",
        Order = 3 )]

    [CampusesField( name:"Campuses",
        description: "When set will filter groups by the campuses selected. This requires that groups have a campus set to work.",
        required: false,
        includeInactive: false,
        order: 4,
        key: AttributeKey.Campuses )]

    [GroupField( "Parent Group",
        Description = "When set only groups under this parent (at any level in the hierarchy) will be considered.",
        Key = AttributeKey.ParentGroup,
        IsRequired = false,
        Order = 4 )]

    #endregion Job Attributes

    public class SendAttendanceReminder : RockJob
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

            /// <summary>
            /// The campuses the groups should belong to.
            /// </summary>
            public const string Campuses = "Campuses";

            /// <summary>
            /// The parent group of the group.
            /// </summary>
            public const string ParentGroup = "ParentGroup";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendAttendanceReminder() { }

        /// <inheritdoc cref="RockJob.Execute()" />
        public override void Execute()
        {
            var rockContext = new RockContext();
            var groupType = GroupTypeCache.Get( GetAttributeValue( AttributeKey.GroupType ).AsGuid() );
            var results = new StringBuilder();
            
            this.Result = "0 attendance reminders sent.";

            if ( groupType.SendAttendanceReminder )
            {
                var warning = $"Group Type {groupType.Name} has been configured to send reminders through the \"Send Group Attendance Reminders\" job and this legacy job is no longer required.";
                results.Append( FormatWarningMessage( warning ) );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                this.Result = results.ToString();
                throw new RockJobWarningException( warning );
            }

            if ( !groupType.TakesAttendance)
            {
                var warning = $"Group Type {groupType.Name} isn't setup to take attendance.";
                results.Append( FormatWarningMessage( warning ) );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                this.Result = results.ToString();
                throw new RockJobWarningException( warning );
            }

            var systemEmailGuid = GetAttributeValue( AttributeKey.SystemEmail ).AsGuid();
            var systemCommunication = new SystemCommunicationService( rockContext ).Get( systemEmailGuid );

            var jobPreferredCommunicationType = ( CommunicationType ) GetAttributeValue( AttributeKey.SendUsingConfiguration ).AsInteger();
            var isSmsEnabled = MediumContainer.HasActiveSmsTransport() && !string.IsNullOrWhiteSpace( systemCommunication.SMSMessage );

            if ( jobPreferredCommunicationType == CommunicationType.SMS && !isSmsEnabled )
            {
                // If sms selected but not usable default to email.
                var errorMessage = $"The job is setup to send via SMS but either SMS isn't enabled or no SMS message was found in system communication {systemCommunication.Title}.";
                HandleErrorMessage( errorMessage );
            }

            if ( jobPreferredCommunicationType != CommunicationType.Email && string.IsNullOrWhiteSpace( systemCommunication.SMSMessage ) )
            {
                var warning = $"No SMS message found in system communication {systemCommunication.Title}. All attendance reminders were sent via email.";
                results.Append( FormatWarningMessage( warning ) );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                jobPreferredCommunicationType = CommunicationType.Email;
            }

            var occurrences = GetOccurenceDates( groupType, rockContext );
            var groupIds = occurrences.Where( o => o.Value.Any() ).Select( o => o.Key ).ToList();
            var leaders = GetGroupLeaders( groupIds, rockContext );
            var attendanceRemindersResults = SendAttendanceReminders( leaders, occurrences, systemCommunication, jobPreferredCommunicationType, isSmsEnabled );

            results.AppendLine( $"{attendanceRemindersResults.MessagesSent} attendance reminders sent." );
            results.Append( FormatWarningMessages( attendanceRemindersResults.Warnings ) );
            this.Result = results.ToString();

            HandleErrorMessages( attendanceRemindersResults.Errors );
        }

        /// <summary>
        /// Gets the occurrence dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, List<DateTime>> GetOccurenceDates( GroupTypeCache groupType, RockContext rockContext )
        {
            var dates = GetSearchDates();
            var startDate = dates.Min();
            var endDate = dates.Max().AddDays( 1 );
            var campuses = GetAttributeValue( AttributeKey.Campuses ).SplitDelimitedValues().AsGuidList();
            var parentGroup = GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();

            var occurrences = GetAllOccurenceDates( groupType, dates, startDate, endDate, campuses, parentGroup, rockContext );

            // Remove any occurrences during group type exclusion date ranges
            RemoveExclusionDates( groupType, occurrences );

            // Remove any 'occurrences' that already have attendance data entered
            RemoveGroupsWithAttendenceData( occurrences, startDate, endDate, rockContext );

            return occurrences;
        }

        /// <summary>
        /// Gets the search dates.
        /// </summary>
        /// <returns></returns>
        private List<DateTime> GetSearchDates()
        {

            // Get the occurrence dates that apply
            var dates = new List<DateTime>
            {
                RockDateTime.Today
            };

            try
            {
                var reminderDays = GetAttributeValue( AttributeKey.SendReminders ).Split( ',' );
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
        /// <param name="campuses">The campuses to filter by</param>
        /// <param name="parentGroupGuid">The parent group of the group</param>
        /// <returns></returns>
        private static Dictionary<int, List<DateTime>> GetAllOccurenceDates( GroupTypeCache groupType, List<DateTime> dates, DateTime startDate, DateTime endDate, List<Guid> campuses, Guid? parentGroupGuid, RockContext rockContext )
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

            if ( campuses.Count > 0 )
            {
                groupsToRemind = groupsToRemind.Where( g => g.CampusId.HasValue && campuses.Contains( g.Campus.Guid ) );
            }

            if ( parentGroupGuid.HasValue )
            {
                var parentGroup = groupService.Get( parentGroupGuid.Value );
                var descendantIds = groupService.GetAllDescendentGroupIds( parentGroup.Id, false ).ToList();
                groupsToRemind = groupsToRemind.Where( g => descendantIds.Contains( g.Id ) );
            }

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
            }

            return occurrences;
        }

        /// <summary>
        /// Removes the exclusion dates.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="occurrences">The occurrences.</param>
        private void RemoveExclusionDates( GroupTypeCache groupType, Dictionary<int, List<DateTime>> occurrences )
        {
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

        private StringBuilder FormatWarningMessage( string warning )
        {
            var errorMessages = new List<string> { warning };
            return FormatMessages( errorMessages, "Warning" );
        }

        private StringBuilder FormatWarningMessages( List<string> warnings )
        {
            return FormatMessages( warnings, "Warning" );
        }

        private void HandleErrorMessage( string errorMessage )
        {
            if ( errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            var errorMessages = new List<string> { errorMessage };
            HandleErrorMessages( errorMessages );
        }

        /// <summary>
        /// Handles the error messages. Throws an exception if there are any items in the errorMessages parameter
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        private void HandleErrorMessages( List<string> errorMessages )
        {
            if ( errorMessages.Any() )
            {
                StringBuilder sb = new StringBuilder( this.Result.ToString() );
                sb.Append( FormatMessages( errorMessages, "Error" ) );

                var resultMessage = sb.ToString();
                this.Result = resultMessage;
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