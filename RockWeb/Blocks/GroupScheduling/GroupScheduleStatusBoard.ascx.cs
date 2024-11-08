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
using System.Data.Entity;
using System.Linq;
using System.Text;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    /// Group Schedule Status Board block.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Group Schedule Status Board" )]
    [Category( "Group Scheduling" )]
    [Description( "Scheduler can see overview of current schedules by groups and dates." )]

    [IntegerField(
        "Number of Weeks to Show (Max 16)",
        Key = AttributeKey.FutureWeeksToShow,
        Description = "How many weeks into the future should be displayed.",
        IsRequired = false,
        DefaultIntegerValue = 2,
        Order = 0 )]

    [GroupField(
        "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "A parent group to start from when allowing someone to pick one or more groups to view.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Roster Page",
        Key = AttributeKey.RostersPage,
        Description = "The page to use to view and print a rosters.",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Communications Page",
        Key = AttributeKey.CommunicationsPage,
        Description = "The page to use to send group scheduling communications.",
        IsRequired = false,
        Order = 3 )]

    [BooleanField(
        "Hide Group Picker",
        Key = AttributeKey.HideGroupPicker,
        Description = "When enabled, the group picker will be hidden.",
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField(
        "Hide Date Setting",
        Key = AttributeKey.HideDateSetting,
        Description = "When enabled, the Dates setting button will be hidden.",
        DefaultBooleanValue = false,
        Order = 5 )]

    [Rock.SystemGuid.BlockTypeGuid( "1BFB72CC-A224-4A0B-B291-21733597738A" )]
    public partial class GroupScheduleStatusBoard : RockBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ParentGroup = "ParentGroup";
            public const string FutureWeeksToShow = "FutureWeeksToShow";
            public const string RostersPage = "RostersPage";
            public const string CommunicationsPage = "CommunicationsPage";
            public const string HideGroupPicker = "HideGroupPicker";
            public const string HideDateSetting = "HideDateSetting";
        }

        private static class UserPreferenceKey
        {
            public const string GroupIds = "GroupIds";
            public const string FutureWeeksToShow = "FutureWeeksToShow";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupGuid = "GroupGuid";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += GroupScheduleStatusBoard_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upScheduleStatusBoard );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-scheduler.css", true );

            if ( !this.IsPostBack )
            {
                ApplyBlockSettings();
                BuildStatusBoard();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Private Methods

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            var rootGroupGuid = this.GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();
            if ( rootGroupGuid.HasValue )
            {
                gpGroups.RootGroupId = new GroupService( new RockContext() ).GetId( rootGroupGuid.Value );
            }

            btnSendCommunications.Visible = this.GetAttributeValue( AttributeKey.CommunicationsPage ).IsNotNullOrWhiteSpace();
            btnRosters.Visible = this.GetAttributeValue( AttributeKey.RostersPage ).IsNotNullOrWhiteSpace();

            btnGroups.Visible = ( this.GetAttributeValue( AttributeKey.HideGroupPicker ).AsBoolean() == false );
            btnDates.Visible = ( this.GetAttributeValue( AttributeKey.HideDateSetting ).AsBoolean() == false );
        }

        /// <summary>
        /// Get the <see cref="Group"/> from the Page Parameter.
        /// </summary>
        /// <param name="groupService">The <see cref="GroupService"/>.</param>
        /// <returns>A group if specified in GroupGuid or GroupId Page Parameters; otherwise, null.</returns>
        private Group GetGroupFromParameter( GroupService groupService )
        {
            var groupGuid = PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();
            var groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupGuid.HasValue )
            {
                return groupService.Get( groupGuid.Value );
            }
            else if ( groupId.HasValue )
            {
                return groupService.Get( groupId.Value );
            }

            return null;
        }

        /// <summary>
        /// Builds the status board.
        /// </summary>
        private void BuildStatusBoard()
        {
            lGroupStatusTableHTML.Text = string.Empty;

            int numberOfWeeks = GetSelectedNumberOfWeeks();

            var selectedGroupIds = new List<int>();
            var groupsWarningText = "Please select at least one group.";

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var group = GetGroupFromParameter( groupService );
            if ( group != null )
            {
                // If a Group Guid was passed in, make sure the user has permission to schedule the
                // group (and that scheduling is enabled), and put only that group in the
                // selectedGroupIds list.
                bool isAuthorized = group.IsAuthorized( Authorization.SCHEDULE, CurrentPerson );
                if ( !isAuthorized )
                {
                    groupsWarningText = $"You are not authorized to schedule this group.";
                }
                else if ( !group.GroupType.IsSchedulingEnabled)
                {
                    groupsWarningText = $"Scheduling is not enabled for this group type ({group.GroupType.Name}).";
                }
                else if ( group.DisableScheduling )
                {
                    groupsWarningText = $"Scheduling is disabled for this group ({group.Name}).";
                }
                else
                {
                    selectedGroupIds = new List<int> { group.Id };
                }
            }
            else
            {
                selectedGroupIds = GetSelectedGroupIds();
            }

            var groupsQuery = groupService
                .GetByIds( selectedGroupIds )
                .Where( a => a.GroupType.IsSchedulingEnabled == true && a.DisableScheduling == false );

            nbGroupsWarning.Visible = false;
            if ( !groupsQuery.Any() )
            {
                nbGroupsWarning.Text = groupsWarningText;
                nbGroupsWarning.NotificationBoxType = NotificationBoxType.Warning;
                nbGroupsWarning.Visible = true;
                return;
            }

            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocationsQuery = groupLocationService.Queryable().Where( a => selectedGroupIds.Contains( a.GroupId ) && a.Group.GroupType.IsSchedulingEnabled == true );

            // get all the schedules that are in use by at least one of the GroupLocations
            var groupsScheduleList = groupLocationsQuery.SelectMany( a => a.Schedules ).Where( s => s.IsActive ).Distinct().AsNoTracking().ToList();
            if ( !groupsScheduleList.Any() )
            {
                nbGroupsWarning.Text = "The selected groups don't have any location schedules configured.";
                nbGroupsWarning.NotificationBoxType = NotificationBoxType.Warning;
                nbGroupsWarning.Visible = true;
                return;
            }

            // get the next N sundayDates
            List<DateTime> sundayDateList = GetSundayDateList( numberOfWeeks );

            // build the list of scheduled times for the next n weeks
            List<ScheduleOccurrenceDate> scheduleOccurrenceDateList = new List<ScheduleOccurrenceDate>();
            var currentDate = RockDateTime.Today;
            foreach ( var sundayDate in sundayDateList )
            {
                foreach ( var schedule in groupsScheduleList )
                {
                    var sundayWeekStart = sundayDate.AddDays( -6 );

                    // get all the occurrences for the selected week for this scheduled (It could be more than once a week if it is a daily scheduled, or it might not be in the selected week if it is every 2 weeks, etc)
                    var scheduledDateTimeList = schedule.GetScheduledStartTimes( sundayWeekStart, sundayDate.AddDays( 1 ) );
                    foreach ( var scheduledDateTime in scheduledDateTimeList )
                    {
                        if ( scheduledDateTime >= currentDate )
                        {
                            scheduleOccurrenceDateList.Add( new ScheduleOccurrenceDate { Schedule = schedule, ScheduledDateTime = scheduledDateTime } );
                        }
                    }
                }
            }

            scheduleOccurrenceDateList = scheduleOccurrenceDateList.OrderBy( a => a.ScheduledDateTime ).ToList();

            var latestOccurrenceDate = sundayDateList.Max();

            var scheduledOccurrencesQuery = new AttendanceOccurrenceService( rockContext ).Queryable().Where( a => a.GroupId.HasValue && a.LocationId.HasValue && a.ScheduleId.HasValue && selectedGroupIds.Contains( a.GroupId.Value ) );
            scheduledOccurrencesQuery = scheduledOccurrencesQuery.Where( a => a.OccurrenceDate >= currentDate && a.OccurrenceDate <= latestOccurrenceDate );

            var occurrenceScheduledAttendancesList = scheduledOccurrencesQuery.Select( ao => new ScheduledAttendanceInfo
            {
                Occurrence = ao,
                ScheduledAttendees = ao.Attendees.Where( a => a.RequestedToAttend == true || a.ScheduledToAttend == true ).Select( a => new ScheduledPersonInfo
                {
                    ScheduledPerson = a.PersonAlias.Person,
                    RequestedToAttend = a.RequestedToAttend,
                    ScheduledToAttend = a.ScheduledToAttend,
                    RSVP = a.RSVP,
                    DeclineReasonValueId = a.DeclineReasonValueId
                } ).ToList()
            } ).ToList();

            StringBuilder sbTable = new StringBuilder();

            // start of table
            sbTable.AppendLine( "<table class='table schedule-status-board js-schedule-status-board'>" );

            sbTable.AppendLine( "<thead class='schedule-status-board-header js-schedule-status-board-header'>" );
            sbTable.AppendLine( "<tr>" );

            var tableHeaderLavaTemplate = @"
{%- comment -%}empty column for group/location names{%- endcomment -%}
<th scope='col'></th>
{% for scheduleOccurrenceDate in ScheduleOccurrenceDateList %}
    <th scope='col'>
        <span class='date'>{{ scheduleOccurrenceDate.ScheduledDateTime | Date:'MMM d, yyyy' }}</span>
        <br />
        <span class='day-time'>{{ scheduleOccurrenceDate.Schedule.Name }}</span>
    </th>
{% endfor %}
";

            var headerMergeFields = new Dictionary<string, object> { { "ScheduleOccurrenceDateList", scheduleOccurrenceDateList } };
            string tableHeaderHtml = tableHeaderLavaTemplate.ResolveMergeFields( headerMergeFields );
            sbTable.Append( tableHeaderHtml );
            sbTable.AppendLine( "</tr>" );
            sbTable.AppendLine( "</thead>" );

            var groupLocationsList = groupsQuery.Where( g => g.GroupLocations.Any() ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( g => new GroupInfo
            {
                Group = g,
                MemberList = g.Members.Select( m =>
                    new MemberInfo
                    {
                        PersonId = m.PersonId,
                        GroupRoleId = m.GroupRoleId
                    } ).ToList(),
                // We are currently showing active and inactive locations (not filtering by gl => gl.Location.IsActive).
                // A room may be closed due to capacity but it will continue to show on the status board.
                LocationScheduleCapacitiesList = g.GroupLocations.OrderBy( gl => gl.Order ).ThenBy( gl => gl.Location.Name ).Select( a => new LocationScheduleCapacityInfo
                {
                    ScheduleCapacitiesList = a.GroupLocationScheduleConfigs.Select( sc =>
                         new ScheduleCapacities
                         {
                             ScheduleId = sc.ScheduleId,
                             MinimumCapacity = sc.MinimumCapacity,
                             DesiredCapacity = sc.DesiredCapacity,
                             MaximumCapacity = sc.MaximumCapacity
                         } ),
                    Location = a.Location
                } ).ToList()
            } ).ToList();

            var columnsCount = scheduleOccurrenceDateList.Count() + 1;
            foreach ( var groupLocations in groupLocationsList )
            {
                var locationGroup = groupLocations.Group;
                var groupType = GroupTypeCache.Get( groupLocations.Group.GroupTypeId );
                StringBuilder sbGroupLocations = new StringBuilder();
                sbGroupLocations.AppendLine( string.Format( "<tbody class='group-locations js-group-locations' data-group-id='{0}' data-locations-expanded='1'>", locationGroup.Id ) );

                var groupSchedulingUrl = ResolveRockUrl( string.Format( "~/GroupScheduler/{0}", locationGroup.Id ) );

                // group header row
                sbGroupLocations.AppendLine( "<tr class='group-heading js-group-header thead-dark clickable'>" );
                sbGroupLocations.AppendLine(
                    string.Format(
                        @"
<th></th>
<th colspan='{0}' class='position-relative'><div class='sticky-cell'>
    <i class='fa fa-chevron-down js-toggle-panel'></i> {1}
    <a href='{2}' class='ml-1 text-color js-group-scheduler-link'><i class='{3}'></i></a></div>
</th>",
                        columnsCount - 1, // {0}
                        locationGroup.Name, // {1}
                        groupSchedulingUrl,  // {2}
                        "fa fa-calendar-check-o" )  // {3}
                    );

                sbGroupLocations.AppendLine( "</tr>" );

                // group/schedule+locations
                var locationScheduleCapacitiesList = groupLocations.LocationScheduleCapacitiesList;
                foreach ( var locationScheduleCapacities in locationScheduleCapacitiesList )
                {
                    var location = locationScheduleCapacities.Location;
                    var scheduleCapacitiesLookup = locationScheduleCapacities.ScheduleCapacitiesList.ToDictionary( k => k.ScheduleId, v => v );
                    sbGroupLocations.AppendLine( "<tr class='location-row js-location-row'>" );

                    sbGroupLocations.AppendLine( string.Format( "<th class='location' scope='row' data-location-id='{0}'><div>{1}</div></th>", location.Id, location.Name ) );

                    foreach ( var scheduleOccurrenceDate in scheduleOccurrenceDateList )
                    {
                        var capacities = scheduleCapacitiesLookup.GetValueOrNull( scheduleOccurrenceDate.Schedule.Id ) ?? new ScheduleCapacities();

                        var scheduleLocationStatusHtmlFormat =
    @"<ul class='location-scheduled-list' data-capacity-min='{1}' data-capacity-desired='{2}' data-capacity-max='{3}' data-scheduled-count='{4}'>
    {0}
</ul>";
                        StringBuilder sbScheduledListHtml = new StringBuilder();
                        ScheduledAttendanceInfo occurrenceScheduledAttendances = occurrenceScheduledAttendancesList
                            .FirstOrDefault( ao =>
                                 ao.Occurrence.OccurrenceDate == scheduleOccurrenceDate.OccurrenceDate
                                 && ao.Occurrence.GroupId == groupLocations.Group.Id
                                 && ao.Occurrence.ScheduleId == scheduleOccurrenceDate.Schedule.Id
                                 && ao.Occurrence.LocationId == location.Id );

                        int scheduledCount = 0;


                        var groupTypeRoleLookup = groupType.Roles.ToDictionary( k => k.Id, v => v );

                        if ( occurrenceScheduledAttendances != null && occurrenceScheduledAttendances.ScheduledAttendees.Any() )
                        {
                            foreach ( ScheduledPersonInfo scheduledPersonInfo in occurrenceScheduledAttendances.ScheduledAttendees )
                            {
                                var personRolesInGroup = groupLocations
                                    .MemberList
                                    .Where( m => m.PersonId == scheduledPersonInfo.ScheduledPerson.Id )
                                    .Select( m => groupTypeRoleLookup.GetValueOrNull( m.GroupRoleId ) )
                                    .Where( r => r != null )
                                    .ToList();

                                var personRoleInGroup = personRolesInGroup
                                    .OrderBy( r => r.Order )
                                    .FirstOrDefault();

                                scheduledPersonInfo.PersonRoleInGroup = personRoleInGroup;
                            }

                            // sort so that it is Yes, then Pending, then Denied
                            var attendanceScheduledPersonList = occurrenceScheduledAttendances
                                .ScheduledAttendees
                                .OrderBy( a => a.RSVP == RSVP.Yes ? 0 : 1 )
                                .ThenBy( a => ( a.RSVP == RSVP.Maybe || a.RSVP == RSVP.Unknown ) ? 0 : 1 )
                                .ThenBy( a => a.RSVP == RSVP.No ? 0 : 1 )
                                .ThenBy( a => a.GroupTypeRoleOrder )
                                .ThenBy( a => a.ScheduledPerson.LastName )
                                .ToList();

                            foreach ( ScheduledPersonInfo scheduledPerson in attendanceScheduledPersonList )
                            {
                                ScheduledAttendanceItemStatus status = ScheduledAttendanceItemStatus.Pending;
                                if ( scheduledPerson.RSVP == RSVP.No )
                                {
                                    status = ScheduledAttendanceItemStatus.Declined;
                                }
                                else if ( scheduledPerson.ScheduledToAttend == true )
                                {
                                    status = ScheduledAttendanceItemStatus.Confirmed;
                                }

                                var statusAttributes = " class='status-icon'";
                                if ( !string.IsNullOrWhiteSpace( scheduledPerson.DeclineReason ) )
                                {
                                    statusAttributes = string.Format( " data-original-title='{0}' class='status-icon js-declined-tooltip'", scheduledPerson.DeclineReason);
                                }
                                sbScheduledListHtml.AppendLine(
                                    string.Format(
                                        "<li class='slot person {0}' data-status='{0}'><i{3}></i><span class='person-name'>{1}</span><span class='person-group-role pull-right'>{2}</span></li>",
                                        status.ConvertToString( false ).ToLower(),
                                        scheduledPerson.ScheduledPerson,
                                        scheduledPerson.PersonRoleInGroup,
                                        statusAttributes ) );
                            }

                            scheduledCount = attendanceScheduledPersonList.Where( a => a.RSVP != RSVP.No ).Count();
                        }

                        if ( capacities.DesiredCapacity.HasValue && scheduledCount < capacities.DesiredCapacity.Value )
                        {
                            var countNeeded = capacities.DesiredCapacity.Value - scheduledCount;
                            sbScheduledListHtml.AppendLine( string.Format( "<li class='slot persons-needed empty-slot'>{0} {1} needed</li>", countNeeded, "person".PluralizeIf( countNeeded != 1 ) ) );

                            // add empty slots if we are under the desired count (not including the slot for the 'persons-needed' li)
                            var emptySlotsToAdd = countNeeded - 1;
                            while ( emptySlotsToAdd > 0 )
                            {
                                sbScheduledListHtml.AppendLine( "<li class='slot empty-slot'></li>" );
                                emptySlotsToAdd--;
                            }
                        }

                        var scheduledLocationsStatusHtml = string.Format( scheduleLocationStatusHtmlFormat, sbScheduledListHtml, capacities.MinimumCapacity, capacities.DesiredCapacity, capacities.MaximumCapacity, scheduledCount );

                        sbGroupLocations.AppendLine( string.Format( "<td class='schedule-location js-schedule-location' data-schedule-id='{0}'><div>{1}</div></td>", scheduleOccurrenceDate.Schedule.Id, scheduledLocationsStatusHtml ) );
                    }

                    sbGroupLocations.AppendLine( "</tr>" );
                }

                sbGroupLocations.AppendLine( "</tbody>" );

                sbTable.Append( sbGroupLocations.ToString() );
            }

            // closing divs for main header
            sbTable.AppendLine( "</tr>" );
            sbTable.AppendLine( "</thead>" );

            // closing divs for table
            sbTable.AppendLine( "</table>" );

            lGroupStatusTableHTML.Text = sbTable.ToString();
        }

        /// <summary>
        /// Gets the Sunday date list.
        /// </summary>
        /// <param name="numberOfWeeks">The number of weeks.</param>
        /// <returns></returns>
        private static List<DateTime> GetSundayDateList( int numberOfWeeks )
        {
            var sundayDateList = new List<DateTime>();

            // start with the current weeks Sunday date
            var sundayDate = RockDateTime.Now.Date.SundayDate();
            while ( sundayDateList.Count < numberOfWeeks )
            {
                sundayDateList.Add( sundayDate );
                sundayDate = sundayDate.AddDays( 7 );
            }

            return sundayDateList;
        }

        /// <summary>
        /// Gets the selected groups from UserPreferences
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var preferences = GetBlockPersonPreferences();

            return preferences.GetValue( UserPreferenceKey.GroupIds ).SplitDelimitedValues().AsIntegerList();
        }

        /// <summary>
        /// Gets the number of weeks from UserPreferences or Block Settings
        /// </summary>
        /// <returns></returns>
        private int GetSelectedNumberOfWeeks()
        {
            // if "Hide Date Setting" is enabled, use the block setting (ignore user preference).
            if ( GetAttributeValue( AttributeKey.HideDateSetting ).AsBoolean() )
            {
                return this.GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 2;
            }

            // if there is a stored user preference, use that, otherwise use the value from block attributes
            var preferences = GetBlockPersonPreferences();
            int? numberOfWeeks = preferences.GetValue( UserPreferenceKey.FutureWeeksToShow ).AsIntegerOrNull();
            if ( !numberOfWeeks.HasValue )
            {
                numberOfWeeks = this.GetAttributeValue( AttributeKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 2;
            }

            // limit number of weeks to 1-16, just in case it got outside of that
            if ( numberOfWeeks > 16 )
            {
                numberOfWeeks = 16;
            }

            if ( numberOfWeeks < 1 )
            {
                numberOfWeeks = 1;
            }

            return numberOfWeeks ?? 2;
        }

        #endregion Private Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the GroupScheduleStatusBoard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GroupScheduleStatusBoard_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGroups_Click( object sender, EventArgs e )
        {
            gpGroups.SetValues( GetSelectedGroupIds() );
            dlgGroups.Show();
        }

        /// <summary>
        /// Handles the Click event of the btnDates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDates_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            rsDateRange.SelectedValue = preferences.GetValue( UserPreferenceKey.FutureWeeksToShow ).AsIntegerOrNull() ?? 2;
            dlgDateRangeSlider.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroups_SaveClick( object sender, EventArgs e )
        {
            dlgGroups.Hide();
            var selectedGroupIds = gpGroups.SelectedValues.ToList().AsIntegerList();
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.GroupIds, selectedGroupIds.AsDelimited( "," ) );
            preferences.Save();

            BuildStatusBoard();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgDateRangeSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgDateRangeSlider_SaveClick( object sender, EventArgs e )
        {
            dlgDateRangeSlider.Hide();

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.FutureWeeksToShow, rsDateRange.SelectedValue.ToString() );
            preferences.Save();

            BuildStatusBoard();
        }

        /// <summary>
        /// Handles the Click event of the btnSendCommunications control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendCommunications_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupIds", GetSelectedGroupIds().AsDelimited( "," ) );
            NavigateToLinkedPage( AttributeKey.CommunicationsPage, queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnRosters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRosters_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupIds", GetSelectedGroupIds().AsDelimited( "," ) );
            NavigateToLinkedPage( AttributeKey.RostersPage, queryParams );
        }

        #endregion Events

        #region Helper Classes

        private class ScheduleOccurrenceDate : RockDynamic
        {
            public Schedule Schedule { get; set; }

            public DateTime ScheduledDateTime { get; set; }

            public DateTime OccurrenceDate
            {
                get
                {
                    return ScheduledDateTime.Date;
                }
            }
        }

        public class ScheduleCapacities
        {
            public int ScheduleId { get; set; }

            public int? MinimumCapacity { get; set; }

            public int? DesiredCapacity { get; set; }

            public int? MaximumCapacity { get; set; }
        }

        private class ScheduledAttendanceInfo
        {
            public AttendanceOccurrence Occurrence { get; set; }

            public List<ScheduledPersonInfo> ScheduledAttendees { get; set; }
        }

        private class ScheduledPersonInfo
        {
            public Person ScheduledPerson { get; set; }

            public GroupTypeRoleCache PersonRoleInGroup { get; set; }

            public int GroupTypeRoleOrder
            {
                get
                {
                    if ( PersonRoleInGroup != null )
                    {
                        return PersonRoleInGroup.Order;
                    }
                    else
                    {
                        // put non-members after members
                        return int.MaxValue;
                    }
                }
            }

            public bool? RequestedToAttend { get; set; }

            public bool? ScheduledToAttend { get; set; }

            public RSVP RSVP { get; set; }

            public int? DeclineReasonValueId { get; set; }

            public string DeclineReason
            {
                get
                {
                    if( RSVP != RSVP.No )
                    {
                        return string.Empty;
                    }

                    var declinedReason = DefinedValueCache.GetValue( DeclineReasonValueId ).EncodeHtml();
                    if ( declinedReason.IsNullOrWhiteSpace())
                    {
                        declinedReason = "No reason given.";
                    }
                    return declinedReason;
                }
            }
        }

        private class MemberInfo
        {
            public int PersonId { get; internal set; }
            public int GroupRoleId { get; internal set; }
        }

        private class GroupInfo
        {
            public Group Group { get; set; }
            public List<MemberInfo> MemberList { get; set; }
            public List<LocationScheduleCapacityInfo> LocationScheduleCapacitiesList { get; set; }
        }

        private class LocationScheduleCapacityInfo
        {
            public IEnumerable<ScheduleCapacities> ScheduleCapacitiesList { get; set; }
            public Location Location { get; set; }
        }

        #endregion Helper Classes
    }
}