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
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Group Schedule Status Board" )]
    [Category( "Group Scheduling" )]
    [Description( "Scheduler can see overview of current schedules by groups and dates." )]

    [IntegerField(
        "Number Of Weeks (Max 16)",
        Key = AttributeKey.NumberOfWeeks,
        Description = "How many weeks into the future should be displayed.",
        IsRequired = false,
        DefaultIntegerValue = 2,
        Order = 0 )]

    [GroupField(
        "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "A parent group to start from when allowing someone to pick one or more groups to view.",
        IsRequired = false,
        Order = 0 )]

    public partial class GroupScheduleStatusBoard : RockBlock
    {
        #region Fields

        protected static class AttributeKey
        {
            public const string ParentGroup = "ParentGroup";
            public const string NumberOfWeeks = "NumberOfWeeks";
        }

        protected static class UserPreferenceKey
        {
            public const string GroupIds = "GroupIds";
            public const string NumberOfWeeks = "NumberOfWeeks";
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-scheduler.css", true );

            if ( !this.IsPostBack )
            {
                var rootGroupGuid = this.GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();
                if ( rootGroupGuid.HasValue )
                {
                    gpGroups.RootGroupId = new GroupService( new RockContext() ).GetId( rootGroupGuid.Value );
                }

                BuildStatusBoard();
            }
        }

        /// <summary>
        /// Builds the status board.
        /// </summary>
        private void BuildStatusBoard()
        {
            lGroupStatusTableHTML.Text = string.Empty;

            int numberOfWeeks = GetSelectedNumberOfWeeks();

            List<int> selectedGroupIds = GetSelectedGroupIds();
            var rockContext = new RockContext();
            var groupsQuery = new GroupService( rockContext ).GetByIds( selectedGroupIds ).Where( a => a.GroupType.IsSchedulingEnabled == true );

            nbGroupsWarning.Visible = false;
            if ( !groupsQuery.Any() )
            {
                nbGroupsWarning.Text = "Please select at least one group.";
                nbGroupsWarning.NotificationBoxType = NotificationBoxType.Warning;
                nbGroupsWarning.Visible = true;
                return;
            }

            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocationsQuery = groupLocationService.Queryable().Where( a => selectedGroupIds.Contains( a.GroupId ) && a.Group.GroupType.IsSchedulingEnabled == true );

            // get all the schedules that are in use by at least one of the GroupLocations
            var groupsScheduleList = groupLocationsQuery.SelectMany( a => a.Schedules ).Distinct().AsNoTracking().ToList();
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
                    var scheduledDateTime = schedule.GetNextStartDateTime( sundayWeekStart );
                    if ( scheduledDateTime.HasValue && scheduledDateTime.Value >= currentDate )
                    {
                        scheduleOccurrenceDateList.Add( new ScheduleOccurrenceDate { Schedule = schedule, ScheduledDateTime = scheduledDateTime.Value } );
                    }
                }
            }

            scheduleOccurrenceDateList = scheduleOccurrenceDateList.OrderBy( a => a.ScheduledDateTime ).ToList();

            var latestOccurrenceDate = sundayDateList.Max();

            var scheduledOccurrencesQuery = new AttendanceOccurrenceService( rockContext ).Queryable().Where( a => a.GroupId.HasValue && a.LocationId.HasValue && a.ScheduleId.HasValue && selectedGroupIds.Contains( a.GroupId.Value ) );
            scheduledOccurrencesQuery = scheduledOccurrencesQuery.Where( a => a.OccurrenceDate >= currentDate && a.OccurrenceDate <= latestOccurrenceDate );

            var occurrenceScheduledAttendancesList = scheduledOccurrencesQuery.Select( ao => new
            {
                Occurrence = ao,
                ScheduledAttendees = ao.Attendees.Where( a => a.RequestedToAttend == true || a.ScheduledToAttend == true ).Select( a => new
                {
                    ScheduledPerson = a.PersonAlias.Person,
                    a.RequestedToAttend,
                    a.ScheduledToAttend,
                    a.RSVP
                } )
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
        <span class='date'>{{ scheduleOccurrenceDate.ScheduledDateTime | Date:'MMM d, yyyy'  }}</span>
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

            var groupLocationsList = groupsQuery.Where( g => g.GroupLocations.Any() ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( g => new
            {
                Group = g,
                LocationScheduleCapacitiesList = g.GroupLocations.OrderBy( gl => gl.Order ).ThenBy( gl => gl.Location.Name ).Select( a => new
                {
                    ScheduleCapacitiesList = a.GroupLocationScheduleConfigs.Select( sc =>
                         new ScheduleCapacities
                         {
                             ScheduleId = sc.ScheduleId,
                             MinimumCapacity = sc.MinimumCapacity,
                             DesiredCapacity = sc.DesiredCapacity,
                             MaximumCapacity = sc.MaximumCapacity
                         } ),
                    a.Location
                } ).ToList()
            } ).ToList();

            var columnsCount = scheduleOccurrenceDateList.Count() + 1;
            foreach ( var groupLocations in groupLocationsList )
            {
                var group = groupLocations.Group;
                StringBuilder sbGroupLocations = new StringBuilder();
                sbGroupLocations.AppendLine( string.Format( "<tbody class='group-locations js-group-locations' data-group-id='{0}' data-locations-expanded='1'>", group.Id ) );

                // group header row
                sbGroupLocations.AppendLine( "<tr class='group-heading js-group-header thead-dark clickable' >" );
                sbGroupLocations.AppendLine( string.Format( "<th></th><th colspan='{0}'><i class='fa fa-chevron-down'></i> {1}</th>", columnsCount - 1, group.Name ) );
                sbGroupLocations.AppendLine( "</tr>" );

                // group/schedule+locations
                var locationScheduleCapacitiesList = groupLocations.LocationScheduleCapacitiesList;
                foreach ( var locationScheduleCapacities in locationScheduleCapacitiesList )
                {
                    var location = locationScheduleCapacities.Location;
                    var scheduleCapacitiesLookup = locationScheduleCapacities.ScheduleCapacitiesList.ToDictionary( k => k.ScheduleId, v => v );
                    sbGroupLocations.AppendLine( "<tr class='location-row js-location-row'>" );

                    sbGroupLocations.AppendLine( string.Format( "<td class='location' scope='row' data-location-id='{0}'><div>{1}</div></td>", location.Id, location.Name ) );

                    foreach ( var scheduleOccurrenceDate in scheduleOccurrenceDateList )
                    {
                        var capacities = scheduleCapacitiesLookup.GetValueOrNull( scheduleOccurrenceDate.Schedule.Id ) ?? new ScheduleCapacities();

                        var scheduleLocationStatusHtmlFormat =
    @"<ul class='location-scheduled-list' data-capacity-min='{1}' data-capacity-desired='{2}' data-capacity-max='{3}' data-scheduled-count='{4}'>
    {0}
</ul>";
                        StringBuilder sbScheduledListHtml = new StringBuilder();
                        var occurrenceScheduledAttendances = occurrenceScheduledAttendancesList
                            .FirstOrDefault( ao =>
                                 ao.Occurrence.OccurrenceDate == scheduleOccurrenceDate.OccurrenceDate
                                 && ao.Occurrence.GroupId == groupLocations.Group.Id
                                 && ao.Occurrence.ScheduleId == scheduleOccurrenceDate.Schedule.Id
                                 && ao.Occurrence.LocationId == location.Id );

                        int scheduledCount = 0;

                        if ( occurrenceScheduledAttendances != null && occurrenceScheduledAttendances.ScheduledAttendees.Any() )
                        {
                            // sort so that it is Yes, then Pending, then Denied
                            var scheduledPersonList = occurrenceScheduledAttendances
                                .ScheduledAttendees
                                .OrderBy( a => a.RSVP == RSVP.Yes ? 0 : 1 )
                                .ThenBy( a => ( a.RSVP == RSVP.Maybe || a.RSVP == RSVP.Unknown ) ? 0 : 1 )
                                .ThenBy( a => a.RSVP == RSVP.No ? 0 : 1 )
                                .ToList();

                            foreach ( var scheduledPerson in scheduledPersonList )
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

                                sbScheduledListHtml.AppendLine( string.Format( "<li class='slot person {0}' data-status='{0}'><i class='status-icon'></i><span class='person-name'>{1}</span></li>", status.ConvertToString( false ).ToLower(), scheduledPerson.ScheduledPerson ) );
                            }

                            scheduledCount = scheduledPersonList.Where( a => a.RSVP != RSVP.No ).Count();
                        }

                        if ( capacities.DesiredCapacity.HasValue && scheduledCount < capacities.DesiredCapacity.Value )
                        {
                            var countNeeded = capacities.DesiredCapacity.Value - scheduledCount;
                            sbScheduledListHtml.AppendLine( string.Format( "<li class='slot persons-needed empty-slot'>{0} {1} needed</li>", countNeeded, "Person".PluralizeIf( countNeeded != 0 ) ) );

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
        /// Gets the sunday date list.
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
        ///
        /// </summary>
        private class ScheduleOccurrenceDate : DotLiquid.Drop
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

        /// <summary>
        /// Gets the selected groups from UserPreferences
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            return this.GetBlockUserPreference( UserPreferenceKey.GroupIds ).SplitDelimitedValues().AsIntegerList();
        }

        /// <summary>
        /// Gets the number of weeks from UserPreferences or Block Settings
        /// </summary>
        /// <returns></returns>
        private int GetSelectedNumberOfWeeks()
        {
            // if there is a stored user preference, use that, otherwise use the value from block attributes
            int? numberOfWeeks = this.GetBlockUserPreference( UserPreferenceKey.NumberOfWeeks ).AsIntegerOrNull();
            if ( !numberOfWeeks.HasValue )
            {
                numberOfWeeks = this.GetAttributeValue( AttributeKey.NumberOfWeeks ).AsIntegerOrNull() ?? 2;
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
            rsDateRange.SelectedValue = this.GetBlockUserPreference( UserPreferenceKey.NumberOfWeeks ).AsIntegerOrNull() ?? 2;
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
            this.SetBlockUserPreference( UserPreferenceKey.GroupIds, selectedGroupIds.AsDelimited( "," ) );
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
            this.SetBlockUserPreference( UserPreferenceKey.NumberOfWeeks, rsDateRange.SelectedValue.ToString() );
            BuildStatusBoard();
        }

        /// <summary>
        ///
        /// </summary>
        public class ScheduleCapacities
        {
            public int ScheduleId { get; set; }

            public int? MinimumCapacity { get; set; }

            public int? DesiredCapacity { get; set; }

            public int? MaximumCapacity { get; set; }
        }
    }
}