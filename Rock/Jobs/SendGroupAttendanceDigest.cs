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
using Rock.Model;
using Rock.Utility;

namespace Rock.Jobs
{
    /// <summary>
    /// Send Group Attendance Digest
    /// </summary>
    [DisplayName( "Send Group Attendance Digest" )]
    [Description( "This job will send a single digest email to all active Leaders of the Sectional/Regional (middle) group layer for all active child groups of that middle layer group. The digest will contain an attendance summary for all these child groups, and the digest is based on the configured System Communication template. That template should contain the following merge objects: AttendanceSummary, GroupAttendance." )]

    #region Job Attributes

    [GroupField(
        "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "The parent group that contains all of the leadership groups who have section leaders who will receive the digest emails containing a summary of the attendance entry of the groups under them.",
        IsRequired = true,
        Order = 1 )]

    [SystemCommunicationField(
        "System Communication",
        Key = AttributeKey.SystemCommunication,
        Description = "The system communication that contains the email template to use for the email.",
        IsRequired = true,
        Order = 2 )]

    [CustomDropdownListField(
        "Date Range",
        "The Monday-Sunday date range that should be used when reporting attendance status.",
        "1^Current Week,2^Previous Week",
        Key = AttributeKey.DateRange,
        IsRequired = true,
        DefaultValue = "1",
        Order = 3 )]

    #endregion

    public class SendGroupAttendanceDigest : RockJob
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ParentGroup = "ParentGroup";
            public const string SystemCommunication = "SystemCommunication";
            public const string DateRange = "DateRange";
        }

        #endregion

        private SystemCommunicationService _systemCommunicationService;
        private GroupService _groupService;
        private GroupMemberService _groupMemberService;
        private AttendanceOccurrenceService _attendanceOccurrenceService;

        private IList<string> _warnings;
        private IList<string> _errors;
        private int _digestsSentCount;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendGroupAttendanceDigest()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            try
            {
                InitializeResultsCounters();
                ProcessJob();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                throw;
            }

            //// ***********************
            ////  Final count and report
            //// ***********************

            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine();

            if ( _digestsSentCount > 0 )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {_digestsSentCount} Digests sent" );
            }

            foreach ( var warning in _warnings )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-warning'></i> {warning}" );
            }

            foreach ( var error in _errors )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }

            this.Result = jobSummaryBuilder.ToString();

            if ( _errors.Any() )
            {
                // This Exception class does not exist in the 10.3 branch.
                //throw new RockJobWarningException( ... );
            }
        }

        /// <summary>
        /// Processes the job.
        /// </summary>
        private void ProcessJob()
        {
            
            // Make sure we have valid entity Guids.
            var parentGroupGuid = this.GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();
            if ( parentGroupGuid == null )
            {
                _errors.Add( "The selected parent group is not valid." );
                return;
            }

            var systemCommunicationGuid = this.GetAttributeValue( AttributeKey.SystemCommunication ).ToString().AsGuidOrNull();
            if ( systemCommunicationGuid == null )
            {
                _errors.Add( "The selected system communication is not valid." );
                return;
            }

            // Make sure we have a valid date range.
            int.TryParse( this.GetAttributeValue( AttributeKey.DateRange ).ToString(), out int dateRangeInt );
            if ( dateRangeInt <= 0 || dateRangeInt > 2 )
            {
                dateRangeInt = 1;
            }

            DateRange dateRange = ( DateRange ) dateRangeInt;
            GetStartAndEndDates( dateRange, out DateTime startDate, out DateTime endDate );

            // Create an object to hold all of the attendance data.
            var attendanceSummary = new AttendanceSummary
            {
                StartDate = startDate,
                EndDate = endDate
            };

            SystemCommunication systemCommunication;

            // Retrieve all of the needed data from the database.
            using ( var rockContext = new RockContext() )
            {
                InitializeServices( rockContext );

                // Make sure the selected system communication exists.
                systemCommunication = _systemCommunicationService.GetNoTracking( systemCommunicationGuid.Value );
                if ( systemCommunication == null )
                {
                    _errors.Add( "Unable to retrieve the selected system communication." );
                    return;
                }

                attendanceSummary.RegionalGroupAttendances = GetRegionalGroupAttendances( parentGroupGuid.Value, startDate, endDate );
            }

            if ( attendanceSummary.RegionalGroupAttendances?.Any() != true )
            {
                // Warnings/errors will have been added explaining what failed.
                return;
            }

            // Send the emails.
            SendDigestEmails( systemCommunication, attendanceSummary );
        }

        /// <summary>
        /// The date range for which attendance should be reported.
        /// </summary>
        private enum DateRange
        {
            CurrentWeek = 1,
            PreviousWeek = 2
        }

        /// <summary>
        /// Gets the start and end dates for a given week, based on the current date/time, the Rock Sundate date/time and the supplied the <see cref="DateRange"/>.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        private void GetStartAndEndDates( DateRange dateRange, out DateTime startDate, out DateTime endDate )
        {
            /*
                10/25/2024 - SMC
                This job was originally intended to run Mon-Sun, as per the original specification, but this has been modified to run
                from the first day of the week (as specified in the System Settings) to the last day.  By default, this is Mon-Sun in
                Rock.

                The original engineering note about the appropriate date range below is no longer accurate, but has been left for
                explanatory reasons.

                Reason: Correcting date range for users with custom "Starting Day of Week" System Setting.

                --------

                Prior engineering note: 2020-05-26 - JH
                Per the SOW for this Job, the week reported should be Mon - Sun, so we need to add 1 day to the range.

                Reason: Veritas Church custom digest email job.
             */

            switch ( dateRange )
            {
                case DateRange.CurrentWeek:
                    startDate = RockDateTime.Today.StartOfWeek( Rock.Web.SystemSettings.StartDayOfWeek );
                    break;
                case DateRange.PreviousWeek:
                    startDate = RockDateTime.Today.StartOfWeek( Rock.Web.SystemSettings.StartDayOfWeek ).AddDays( -7 );
                    break;
                default:
                    throw new ApplicationException( $"The '{dateRange}' date range is not yet supported." );
            }

            endDate = startDate.AddDays( 7 );
        }

        /// <summary>
        /// A class to represent the attendance summary.
        /// </summary>
        private class AttendanceSummary : RockDynamic
        {
            /// <summary>
            /// The start date for this attendance summary.
            /// </summary>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// The end date for this attendance summary.
            /// </summary>
            public DateTime EndDate { get; set; }

            /// <summary>
            /// The regional group attendance summaries.
            /// </summary>
            public IList<RegionalGroupAttendance> RegionalGroupAttendances { get; set; }
        }

        /// <summary>
        /// A class to represent a regional group and attendance data for each of its child groups.
        /// </summary>
        private class RegionalGroupAttendance : RockDynamic
        {
            /// <summary>
            /// The regional group name.
            /// </summary>
            public Group Group { get; set; }

            /// <summary>
            /// The people who are leaders for this regional group.
            /// </summary>
            public IList<Person> Leaders { get; set; }

            /// <summary>
            /// The group attendance summaries for this regional group.
            /// </summary>
            public IList<GroupAttendance> GroupAttendances { get; set; }
        }

        /// <summary>
        /// A class to represent attendance data for a group.
        /// </summary>
        private class GroupAttendance : RockDynamic
        {
            /// <summary>
            /// The group name.
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// The meeting date.
            /// </summary>
            public DateTime MeetingDate { get; set; }

            /// <summary>
            /// The attendance summary.
            /// </summary>
            public string Attendance { get; set; }

            /// <summary>
            /// The notes for this attendance occurrence.
            /// </summary>
            public string Notes { get; set; }

            /// <summary>
            /// The email address for this group's leader.
            /// </summary>
            public string LeaderEmail { get; set; }
        }

        /// <summary>
        /// Initializes the results counters.
        /// </summary>
        private void InitializeResultsCounters()
        {
            _warnings = new List<string>();
            _errors = new List<string>();
            _digestsSentCount = 0;
        }

        /// <summary>
        /// Initializes the services.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void InitializeServices( RockContext rockContext )
        {
            _systemCommunicationService = new SystemCommunicationService( rockContext );
            _groupService = new GroupService( rockContext );
            _groupMemberService = new GroupMemberService( rockContext );
            _attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
        }

        /// <summary>
        /// Gets the regional group attendances.
        /// </summary>
        /// <param name="parentGroupGuid">The parent group unique identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The regional group attendances.</returns>
        private IList<RegionalGroupAttendance> GetRegionalGroupAttendances( Guid parentGroupGuid, DateTime startDate, DateTime endDate )
        {
            // Get the parent group.
            Group parentGroup = _groupService.GetNoTracking( parentGroupGuid );
            if ( parentGroup == null )
            {
                _errors.Add( "Unable to retrieve the selected parent group." );
                return null;
            }

            // Get the regional groups under the parent group.
            var regionalGroups = _groupService.Queryable()
                .AsNoTracking()
                .Where( g => g.ParentGroupId == parentGroup.Id && g.IsActive == true )
                .ToList();

            if ( !regionalGroups.Any() )
            {
                _warnings.Add( $"No regional groups found under the '{parentGroup.Name}' parent group." );
                return null;
            }

            var regionalGroupAttendances = new List<RegionalGroupAttendance>();

            foreach ( var regionalGroup in regionalGroups )
            {
                var leaders = _groupMemberService.GetLeadersWithActiveEmails( regionalGroup.Id )
                    .AsNoTracking()
                    .ToList();

                if ( !leaders.Any() )
                {
                    _warnings.Add( $"No leaders with active email addresses found for the '{regionalGroup.Name}' regional group." );
                    continue;
                }

                var regionalGroupAttendance = new RegionalGroupAttendance
                {
                    Group = regionalGroup,
                    Leaders = leaders.Select( l => l.Person ).ToList(),
                    GroupAttendances = GetChildrenGroupAttendances( regionalGroup, startDate, endDate )
                };

                // Only add this regional group if it has child group attendances to report.
                if ( regionalGroupAttendance.GroupAttendances?.Any() == true )
                {
                    regionalGroupAttendances.Add( regionalGroupAttendance );
                }
            }

            return regionalGroupAttendances;
        }

        /// <summary>
        /// Gets the group attendances for the active children groups of the specified regional group.
        /// </summary>
        /// <param name="regionalGroup">The regional group.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The group attendances for the children groups of the specified regional group.</returns>
        private IList<GroupAttendance> GetChildrenGroupAttendances( Group regionalGroup, DateTime startDate, DateTime endDate )
        {
            // Get the groups under this regional group.
            var groups = _groupService.Queryable( "Schedule" ).AsNoTracking()
                .Where( g => g.ParentGroupId == regionalGroup.Id
                    && g.IsActive
                    && g.GroupType.TakesAttendance 
                    && g.ScheduleId.HasValue )
                .ToList();

            if ( !groups.Any() )
            {
                _warnings.Add( $"No qualifying groups found under the '{regionalGroup.Name}' regional group." );
                return null;
            }

            var childrenGroupAttendances = new List<GroupAttendance>();

            foreach ( Group group in groups )
            {
                var groupAttendances = GetGroupAttendances( group, startDate, endDate );

                // Only add this group if it has attendances to report.
                if ( groupAttendances?.Any() == true )
                {
                    childrenGroupAttendances.AddRange( groupAttendances );
                }
            }

            return childrenGroupAttendances;
        }

        /// <summary>
        /// Gets the group attendances for the spedified individual group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        private IList<GroupAttendance> GetGroupAttendances( Group group, DateTime startDate, DateTime endDate )
        {
            // This service call will not only retrieve existing attendance occurances from the db, but will also create in-memory attendance occurance instances for any scheduled meetings within the specified date range that don't have a db representation.
            var attendanceOccurrences = _attendanceOccurrenceService.GetGroupOccurrences( group, startDate, endDate, new List<int>(), new List<int>() )
                .Where( a => a.OccurrenceDate >= startDate )
                .Where( a => a.OccurrenceDate < endDate )
                .ToList();

            if ( !attendanceOccurrences.Any() )
            {
                return null;
            }

            var leader = _groupMemberService.GetLeadersWithActiveEmails( group.Id )
                .AsNoTracking()
                .FirstOrDefault();

            return attendanceOccurrences.Select( a => new GroupAttendance
            {
                GroupName = group.Name,
                MeetingDate = a.OccurrenceDate,
                Attendance = GetAttendanceValue( a ),
                Notes = a.Notes,
                LeaderEmail = leader?.Person?.Email
            } )
            .ToList();
        }

        /// <summary>
        /// Gets the attendance value.
        /// </summary>
        /// <param name="attendanceOccurrence">The attendance occurrence.</param>
        /// <returns>The attendance value.</returns>
        private string GetAttendanceValue( AttendanceOccurrence attendanceOccurrence )
        {
            if ( attendanceOccurrence.DidNotOccur == true )
            {
                return "Did not meet";
            }

            if ( !attendanceOccurrence.AttendanceEntered )
            {
                return "No attendance";
            }

            return $"{attendanceOccurrence.DidAttendCount}/{attendanceOccurrence.Attendees.Count}";
        }

        /// <summary>
        /// Sends the digest emails for each regional group within the attendance summary.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="attendanceSummary">The attendance summary.</param>
        private void SendDigestEmails( SystemCommunication systemCommunication, AttendanceSummary attendanceSummary )
        {
            foreach ( var regionalGroupAttendance in attendanceSummary.RegionalGroupAttendances )
            {
                // Add the merge objects to support this email.
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeObjects.Add( "StartDate", attendanceSummary.StartDate );
                mergeObjects.Add( "EndDate", attendanceSummary.EndDate );
                mergeObjects.Add( "GroupAttendances", regionalGroupAttendance.GroupAttendances );

                // Send a separate email to each leader within this regional group.
                foreach ( Person leader in regionalGroupAttendance.Leaders )
                {
                    SendDigestEmail( systemCommunication, mergeObjects, leader, regionalGroupAttendance.Group );
                }
            }
        }

        /// <summary>
        /// Sends and individual digest email to the specified person.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="mergeObjects">The Lava merge objects.</param>
        /// <param name="person">The person who should receive the email.</param>
        /// <param name="regionalGroup">The regional group that this digest email represents.</param>
        private void SendDigestEmail( SystemCommunication systemCommunication, Dictionary<string, object> mergeObjects, Person person, Group regionalGroup )
        {
            mergeObjects.AddOrReplace( "Person", person );

            var recipient = new RockEmailMessageRecipient( person, mergeObjects );
            var message = new RockEmailMessage( systemCommunication );
            message.Subject = $"'{regionalGroup.Name}' Group Attendance Digest";
            message.SetRecipients( new List<RockEmailMessageRecipient> { recipient } );
            message.Send( out List<string> errorMessages );

            if ( !errorMessages.Any() )
            {
                _digestsSentCount++;
                return;
            }

            _errors.Add( $"Unable to send '{regionalGroup.Name}' digest to '{person.Email}'." );
        }
    }
}
