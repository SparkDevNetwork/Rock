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
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "En Route" )]
    [Category( "Check-in > Manager" )]
    [Description( "Lists the people who are checked-in but not yet marked present." )]

    [CustomDropdownListField(
        "Filter By",
        Description = "This controls which people appear in the list. For example, when set to 'Checked-in' people who are only checked-in (not yet marked 'Present') will be shown. For more information read about the 'Enable Presence' feature in the check-in documentation.",
        Key = AttributeKey.FilterBy,
        DefaultValue = "2",
        ListSource = "2^Checked-in,3^Present,4^Checked-out",
        IsRequired = true,
        Order = 1
        )]

    [BooleanField(
        "Show Only Parent Group",
        Description = "When enabled, the parent group and path for each check-in, instead of the actual group.",
        Key = AttributeKey.ShowOnlyParentGroup,
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Always Show Child Groups",
        Description = @"When enabled, all child groups of the selected group will be included in the filter. Otherwise, a 'Include Child Groups' option will 
 be displayed to include child groups.",
        Key = AttributeKey.AlwaysShowChildGroups,
        DefaultBooleanValue = false,
        Order = 3 )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CHECK_IN_MANAGER_EN_ROUTE )]
    public partial class EnRoute : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys for attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string FilterBy = "FilterBy";

            public const string ShowOnlyParentGroup = "ShowOnlyParentGroup";

            public const string AlwaysShowChildGroups = "AlwaysShowChildGroups";
        }

        #endregion

        #region Custom Settings Keys

        /// <summary>
        /// Keys to use for settings stored in the checkin-manager cookie.
        /// These are stored in a cookie vs user-preference since the same
        /// login is often used by multiple devices when running Checkin-Manager.
        /// </summary>
        private class CustomSettingKey
        {
            public const string EnRouteScheduleIdsFilter = "EnRouteScheduleIdsFilter";
            public const string EnRoutePickedGroupIdsFilter = "EnRoutePickedGroupIdsFilter";
            public const string EnRouteIncludeChildGroupsFilter = "EnRouteIncludeChildGroupsFilter";
        }

        #endregion Custom Settings Keys

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string ScheduleListItems = "ScheduleListItems";
            public const string LocationListItems = "LocationListItems";
            public const string GroupListItems = "GroupListItems";
        }

        #endregion ViewState Keys

        #region Fields

        private const string GroupListItemKeyDelimiter = "|";

        private List<ListItemBag> _scheduleListItems = new List<ListItemBag>();
        private Dictionary<string, List<ListItemBag>> _locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
        private Dictionary<string, List<ListItemBag>> _groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState[ViewStateKey.ScheduleListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _scheduleListItems = new List<ListItemBag>();
            }
            else
            {
                _scheduleListItems = JsonConvert.DeserializeObject<List<ListItemBag>>( json ) ?? new List<ListItemBag>();
            }

            json = ViewState[ViewStateKey.LocationListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
            }
            else
            {
                _locationListItemsBySchedule = JsonConvert.DeserializeObject<Dictionary<string, List<ListItemBag>>>( json ) ?? new Dictionary<string, List<ListItemBag>>();
            }

            json = ViewState[ViewStateKey.GroupListItems] as string;
            if ( json.IsNullOrWhiteSpace() )
            {
                _groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();
            }
            else
            {
                _groupListItemsByScheduleAndLocation = JsonConvert.DeserializeObject<Dictionary<string, List<ListItemBag>>>( json ) ?? new Dictionary<string, List<ListItemBag>>();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                HandleCustomPostback();
            }
            else
            {
                BindFilter();
                BindGrid();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.ScheduleListItems] = JsonConvert.SerializeObject( _scheduleListItems, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.LocationListItems] = JsonConvert.SerializeObject( _locationListItemsBySchedule, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.GroupListItems] = JsonConvert.SerializeObject( _groupListItemsByScheduleAndLocation, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the TextChanged event of the tbSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HandleCustomPostback()
        {
            var eventArg = this.Request.Params["__EVENTARGUMENT"];
            if ( eventArg == "search" && tbSearch.Text.Length > 2 )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Shows the filters.
        /// </summary>
        private void BindFilter()
        {
            ScheduleService scheduleService = new ScheduleService( new RockContext() );
            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;

            var scheduleIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteScheduleIdsFilter );
            var pickedGroupIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRoutePickedGroupIdsFilter );
            var includeChildGroupsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteIncludeChildGroupsFilter );

            var selectedScheduleIds = scheduleIdsFilter.SplitDelimitedValues().AsIntegerList();

            // limit Schedules to ones that are Active, have a CheckInStartOffsetMinutes, and are Named schedules
            var scheduleQry = scheduleService.Queryable().Where( a => a.IsActive && a.CheckInStartOffsetMinutes != null && a.Name != null && a.Name != string.Empty );

            var scheduleList = scheduleQry.ToList().OrderBy( a => a.Name ).ToList();

            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            lbSchedules.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                var listItem = new ListItem();
                if ( schedule.Name.IsNotNullOrWhiteSpace() )
                {
                    listItem.Text = schedule.Name;
                }
                else
                {
                    listItem.Text = schedule.FriendlyScheduleText;
                }

                listItem.Value = schedule.Id.ToString();
                listItem.Selected = selectedScheduleIds.Contains( schedule.Id );
                lbSchedules.Items.Add( listItem );
            }

            IEnumerable<CheckinAreaPath> checkinAreasPaths;
            var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
            if ( checkinAreaFilter != null )
            {
                checkinAreasPaths = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendantsPath( checkinAreaFilter.Id );
            }
            else
            {
                checkinAreasPaths = new GroupTypeService( new RockContext() ).GetAllCheckinAreaPaths();
            }

            var checkinGroupTypeIdsToShow = checkinAreasPaths.Select( a => a.GroupTypeId ).Distinct().ToList();

            gpGroups.IncludedGroupTypeIds = checkinGroupTypeIdsToShow;

            var selectedGroupIds = pickedGroupIdsFilter.SplitDelimitedValues().AsIntegerList();
            gpGroups.SetValues( selectedGroupIds );

            if ( GetAttributeValue( AttributeKey.AlwaysShowChildGroups ).AsBoolean() )
            {
                cbIncludeChildGroups.Visible = false;
                lblIncludeChildGroups.Visible = true;
                cbIncludeChildGroups.Checked = true;
            }
            else
            {
                cbIncludeChildGroups.Visible = true;
                lblIncludeChildGroups.Visible = false;
                cbIncludeChildGroups.Checked = includeChildGroupsFilter.AsBoolean();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApplyFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyFilter_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnClearFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearFilter_Click( object sender, EventArgs e )
        {
            lbSchedules.SetValues( new int[0] );
            gpGroups.SetValues( new int[0] );
            tbSearch.Text = string.Empty;
        }

        /// <summary>
        /// Gets the row attendance ids.
        /// </summary>
        /// <param name="rowEventArgs">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private int[] GetRowAttendanceIds( RowEventArgs rowEventArgs )
        {
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
            var attendanceIds = rowEventArgs.RowKeyValues[1] as int[];
            return attendanceIds;
        }

        #endregion

        #region Move Person

        /*  12-07-2021 MDP

        This Move Person code in this #region is nearly identical in both the RockWeb.Blocks.CheckIn.Manager
        EnRoute and AttendanceDetail Blocks. If changes are made to one, make sure to update the other.

        */

        /// <summary>
        /// Handles the Click event of the btnMovePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnMovePerson_Click( object sender, RowEventArgs e )
        {
            var attendanceIds = GetRowAttendanceIds( e ).ToList();
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            var attendances = attendanceService.GetByIds( attendanceIds ).OrderByDescending( a => a.StartDateTime ).ToList();
            var mostRecentAttendance = attendances.OrderByDescending( a => a.StartDateTime ).FirstOrDefault();
            if ( attendances.Count > 1 )
            {
                nbMovePersonInstructions.Text = $"{mostRecentAttendance.PersonAlias} is en-route to multiple services. Select the one to be moved.";
                pnlMovePersonMultipleAttendance.Visible = true;
            }
            else
            {
                pnlMovePersonMultipleAttendance.Visible = false;
            }

            ddlMovePersonSelectAttendance.Items.Clear();
            foreach ( var attendance in attendances )
            {
                var listItem = new ListItem();
                listItem.Text = $"{attendance.Occurrence.Group.Name} in {attendance.Occurrence.Location.Name} at {attendance.Occurrence.Schedule.Name}";
                listItem.Value = attendance.Id.ToString();
                ddlMovePersonSelectAttendance.Items.Add( listItem );
            }

            ddlMovePersonSelectAttendance.SetValue( mostRecentAttendance );

            var attendanceId = mostRecentAttendance?.Id;

            if ( !attendanceId.HasValue )
            {
                return;
            }

            UpdateMovePersonControls( mostRecentAttendance.Id );

            mdMovePerson.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMovePersonSelectAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void ddlMovePersonSelectAttendance_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateMovePersonControls( ddlMovePersonSelectAttendance.SelectedValueAsId() );
        }

        /// <summary>
        /// Populates the move person controls.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        private void UpdateMovePersonControls( int? attendanceId )
        {
            nbMovePersonLocationFull.Visible = false;
            nbMovePersonLocationFull.Text = string.Empty;

            if ( !attendanceId.HasValue )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var attendance = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( a => a.Occurrence.Group )
                    .FirstOrDefault( a => a.Id == attendanceId.Value );

                if ( attendance?.Occurrence?.Group == null )
                {
                    nbMovePersonLocationFull.Text = "Attendance Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                GetMovePersonOptions( rockContext, attendance );
                LoadMovePersonScheduleDropDown( attendance.Occurrence.ScheduleId );
                LoadMovePersonLocationDropDown( attendance.Occurrence.LocationId );
                LoadMovePersonGroupDropDown( attendance.Occurrence.GroupId );

                mdMovePerson.Show();
            }
        }

        /// <summary>
        /// Gets the available group, location and schedule combinations to which an individual may be moved.
        /// <para>
        /// Values will be added to private fields and subsequently saved to/loaded from ViewState for use
        /// within partial post backs.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attendance">The attendance instance.</param>
        private void GetMovePersonOptions( RockContext rockContext, Attendance attendance )
        {
            var groupLocationSchedules = CheckinManagerHelper.GetGroupLocationSchedulesForPersonMove( rockContext, attendance );
            if ( groupLocationSchedules?.Any() != true )
            {
                return;
            }

            CheckinManagerHelper.GroupAndSortMovePersonOptions(
                groupLocationSchedules,
                out _scheduleListItems,
                out _locationListItemsBySchedule,
                out _groupListItemsByScheduleAndLocation,
                GroupListItemKeyDelimiter
            );
        }

        /// <summary>
        /// Loads the list of schedules in the "Move Person" dialog based on active schedules for the current check-in configuration.
        /// </summary>
        /// <param name="currentScheduleId">The currently-selected schedule ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonScheduleDropDown( int? currentScheduleId )
        {
            ddlMovePersonSchedule.Items.Clear();

            if ( _scheduleListItems?.Any() == true )
            {
                foreach ( var scheduleListItem in _scheduleListItems )
                {
                    ddlMovePersonSchedule.Items.Add( new ListItem( scheduleListItem.Text, scheduleListItem.Value ) );
                }

                if ( currentScheduleId.HasValue && _scheduleListItems.Any( s => s.Value == currentScheduleId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonSchedule.SetValue( currentScheduleId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( _scheduleListItems.Count == 1 )
                    {
                        ddlMovePersonSchedule.SetValue( _scheduleListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonSchedule.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Loads the list of locations in the "Move Person" dialog based on the selected schedule drop down list value.
        /// </summary>
        /// <param name="currentLocationId">The currently-selected location ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonLocationDropDown( int? currentLocationId )
        {
            ddlMovePersonLocation.Items.Clear();

            var selectedScheduleId = ddlMovePersonSchedule.SelectedValue;

            List<ListItemBag> locationListItems = null;
            if ( _locationListItemsBySchedule?.TryGetValue( selectedScheduleId, out locationListItems ) == true && locationListItems?.Any() == true )
            {
                foreach ( var locationListItem in locationListItems )
                {
                    ddlMovePersonLocation.Items.Add( new ListItem( locationListItem.Text, locationListItem.Value ) );
                }

                if ( currentLocationId.HasValue && locationListItems.Any( l => l.Value == currentLocationId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonLocation.SetValue( currentLocationId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( locationListItems.Count == 1 )
                    {
                        ddlMovePersonLocation.SetValue( locationListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonLocation.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Loads the list of groups in the "Move Person" dialog based on the selected schedule and location drop down list values.
        /// </summary>
        /// <param name="currentGroupId">The currently-selected group ID. This value will be reselected if it's still an option.</param>
        private void LoadMovePersonGroupDropDown( int? currentGroupId )
        {
            ddlMovePersonGroup.Items.Clear();

            var groupListItemKey = $"{ddlMovePersonSchedule.SelectedValue}{GroupListItemKeyDelimiter}{ddlMovePersonLocation.SelectedValue}";

            List<ListItemBag> groupListItems = null;
            if ( _groupListItemsByScheduleAndLocation?.TryGetValue( groupListItemKey, out groupListItems ) == true && groupListItems?.Any() == true )
            {
                foreach ( var groupListItem in groupListItems )
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( groupListItem.Text, groupListItem.Value ) );
                }

                if ( currentGroupId.HasValue && groupListItems.Any( g => g.Value == currentGroupId.Value.ToString() ) )
                {
                    // If the currently-selected value is still an option, auto-select it.
                    ddlMovePersonGroup.SetValue( currentGroupId );
                }
                else
                {
                    // If there is only a single possible selection, auto-select that option.
                    if ( groupListItems.Count == 1 )
                    {
                        ddlMovePersonGroup.SetValue( groupListItems.First().Value );
                        return;
                    }

                    // Otherwise, blank out the selection and force the person to select a value.
                    ddlMovePersonGroup.Items.Insert( 0, new ListItem( string.Empty, Rock.Constants.None.Text ) );
                }
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;

            LoadMovePersonLocationDropDown( ddlMovePersonLocation.SelectedValueAsId() );
            LoadMovePersonGroupDropDown( ddlMovePersonGroup.SelectedValueAsId() );
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;

            LoadMovePersonGroupDropDown( ddlMovePersonGroup.SelectedValueAsId() );
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlMovePersonGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMovePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMovePerson_SaveClick( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;
            nbMovePersonLocationFull.Text = string.Empty;

            var attendanceId = ddlMovePersonSelectAttendance.SelectedValueAsId();
            if ( attendanceId == null )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendance = attendanceService.Get( attendanceId.Value );
                if ( attendance == null )
                {
                    nbMovePersonLocationFull.Text = "Attendance Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                var selectedScheduleId = ddlMovePersonSchedule.SelectedValueAsId();
                if ( !selectedScheduleId.HasValue )
                {
                    nbMovePersonLocationFull.Text = "Schedule Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                var selectedLocationId = ddlMovePersonLocation.SelectedValueAsId();
                if ( !selectedLocationId.HasValue )
                {
                    nbMovePersonLocationFull.Text = "Location Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                var selectedGroupId = ddlMovePersonGroup.SelectedValueAsId();
                if ( !selectedGroupId.HasValue )
                {
                    nbMovePersonLocationFull.Text = "Group Not Found";
                    nbMovePersonLocationFull.Visible = true;
                    return;
                }

                var selectedOccurrenceDate = attendance.Occurrence.OccurrenceDate;

                var location = NamedLocationCache.Get( selectedLocationId.Value );

                var locationFirmRoomThreshold = location?.FirmRoomThreshold;
                if ( locationFirmRoomThreshold.HasValue )
                {
                    // The totalAttended is the number of people still checked in (not people who have been checked-out)
                    // not counting the current person who may already be checked in,
                    // + the person we are trying to move
                    var locationCount = attendanceService
                        .GetByDateOnLocationAndSchedule( selectedOccurrenceDate, selectedLocationId.Value, selectedScheduleId.Value )
                        .Where( a => a.EndDateTime == null && a.PersonAlias.PersonId != attendance.PersonAlias.PersonId )
                        .Count();

                    if ( ( locationCount + 1 ) >= locationFirmRoomThreshold.Value )
                    {
                        nbMovePersonLocationFull.Text = $"The {location} has reached its hard threshold capacity and cannot be used for check-in.";
                        nbMovePersonLocationFull.Visible = true;
                        return;
                    }
                }

                var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd( selectedOccurrenceDate, selectedGroupId, selectedLocationId, selectedScheduleId );
                attendance.OccurrenceId = newRoomsOccurrence.Id;
                rockContext.SaveChanges();
            }

            mdMovePerson.Hide();
            BindGrid();
        }

        #endregion Move Person

        #region Methods

        /// <summary>
        /// Gets the campus from context.
        /// </summary>
        /// <returns></returns>
        private CampusCache GetCampusFromContext()
        {
            CampusCache campus = null;

            var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            if ( campusEntityType != null )
            {
                var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( campusContext != null )
                {
                    campus = CampusCache.Get( campusContext.Id );
                }
            }

            return campus;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            RosterAttendee attendee = e.Row.DataItem as RosterAttendee;

            var lPhoto = e.Row.FindControl( "lPhoto" ) as Literal;
            lPhoto.Text = attendee.GetPersonPhotoImageHtmlTag();
            var lName = e.Row.FindControl( "lName" ) as Literal;
            lName.Text = attendee.GetAttendeeNameHtml();

            var lGroupNameAndPath = e.Row.FindControl( "lGroupNameAndPath" ) as Literal;
            if ( lGroupNameAndPath != null && lGroupNameAndPath.Visible )
            {
                if ( _showOnlyParentGroup )
                {
                    lGroupNameAndPath.Text = lGroupNameAndPath.Text = attendee.GetParentGroupNameAndPathHtml();
                }
                else
                {
                    lGroupNameAndPath.Text = lGroupNameAndPath.Text = attendee.GetGroupNameAndPathHtml();
                }
            }

            var lLocation = e.Row.FindControl( "lLocation" ) as Literal;
            lLocation.Text = attendee.RoomName;
        }

        private bool _showOnlyParentGroup;

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            CampusCache campus = GetCampusFromContext();

            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList().Where( a => a > 0 ).ToList();

            List<int> selectedGroupIds = GetSelectedGroupIds();

            var rockContext = new RockContext();

            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;

            customSettings.AddOrReplace( CustomSettingKey.EnRouteScheduleIdsFilter, selectedScheduleIds.AsDelimited( "," ) );
            customSettings.AddOrReplace( CustomSettingKey.EnRoutePickedGroupIdsFilter, gpGroups.SelectedIds.ToList().AsDelimited( "," ) );
            customSettings.AddOrReplace( CustomSettingKey.EnRouteIncludeChildGroupsFilter, cbIncludeChildGroups.Checked.ToTrueFalse() );

            CheckinManagerHelper.SaveCustomSettingsToCookie( customSettings );

            IList<RosterAttendee> attendees = null;

            attendees = GetAttendees( rockContext );

            // sort by Name, and also by PersonGuid (so they stay in a consistent order in cases where Names are identical
            var attendeesSorted = attendees
                .OrderBy( a => a.NickName )
                .ThenBy( a => a.LastName )
                .ThenBy( a => a.PersonGuid )
                .ToList();

            _showOnlyParentGroup = this.GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            gAttendees.DataSource = attendeesSorted;
            gAttendees.DataBind();
        }

        /// <summary>
        /// Gets the selected group ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var groupPickerGroupIds = gpGroups.SelectedValues.AsIntegerList().Where( a => a > 0 ).ToList();
            List<int> selectedGroupIds;

            if ( cbIncludeChildGroups.Checked )
            {
                var childGroupIds = new GroupService( new RockContext() ).Queryable().Where( a => a.ParentGroupId.HasValue && groupPickerGroupIds.Contains( a.Id ) ).Select( a => a.Id );
                selectedGroupIds = groupPickerGroupIds.Union( childGroupIds ).ToList();
            }
            else
            {
                selectedGroupIds = groupPickerGroupIds;
            }

            return selectedGroupIds;
        }

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        private IList<RosterAttendee> GetAttendees( RockContext rockContext )
        {
            var startDateTime = RockDateTime.Today;

            CampusCache campusCache = GetCampusFromContext();
            DateTime currentDateTime;
            if ( campusCache != null )
            {
                currentDateTime = campusCache.CurrentDateTime;
            }
            else
            {
                currentDateTime = RockDateTime.Now;
            }

            // Get all Attendance records for the current day and location
            var attendanceQuery = new AttendanceService( rockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.ScheduleId.HasValue );

            if ( campusCache != null )
            {
                // limit locations (rooms) to locations within the selected campus
                var campusLocationIds = new LocationService( rockContext ).GetAllDescendentIds( campusCache.LocationId.Value ).ToList();
                attendanceQuery = attendanceQuery.Where( a => campusLocationIds.Contains( a.Occurrence.LocationId.Value ) );
            }

            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList().Where( a => a > 0 ).ToList();
            var selectedGroupIds = GetSelectedGroupIds();

            if ( selectedScheduleIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            if ( selectedGroupIds.Any() )
            {
                if ( cbIncludeChildGroups.Checked )
                {
                    var groupService = new GroupService( rockContext );
                    foreach ( var groupId in selectedGroupIds.ToList() )
                    {
                        var childGroupIds = groupService.GetAllDescendentGroupIds( groupId, false );
                        selectedGroupIds.AddRange( childGroupIds );
                    }
                }

                attendanceQuery = attendanceQuery.Where( a => selectedGroupIds.Contains( a.Occurrence.GroupId.Value ) );
            }
            else
            {
                var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );

                if ( checkinAreaFilter != null )
                {
                    // if there is a checkin area filter, limit to groups within the selected check-in area
                    var checkinAreaGroupTypeIds = new GroupTypeService( rockContext ).GetCheckinAreaDescendants( checkinAreaFilter.Id ).Select( a => a.Id ).ToList();
                    selectedGroupIds = new GroupService( rockContext ).Queryable().Where( a => checkinAreaGroupTypeIds.Contains( a.GroupTypeId ) ).Select( a => a.Id ).ToList();
                    attendanceQuery = attendanceQuery.Where( a => selectedGroupIds.Contains( a.Occurrence.GroupId.Value ) );
                }
            }

            RosterStatusFilter rosterStatusFilter = this.GetAttributeValue( AttributeKey.FilterBy ).ConvertToEnumOrNull<RosterStatusFilter>() ?? RosterStatusFilter.CheckedIn;

            attendanceQuery = CheckinManagerHelper.FilterByRosterStatusFilter( attendanceQuery, rosterStatusFilter );

            var attendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            attendanceList = CheckinManagerHelper.FilterByActiveCheckins( currentDateTime, attendanceList );

            attendanceList = attendanceList.Where( a => a.Person != null ).ToList();

            if ( tbSearch.Text.IsNotNullOrWhiteSpace() )
            {
                // search by name
                var searchValue = tbSearch.Text;

                // ignore the result of reversed (LastName, FirstName vs FirstName LastName
                bool reversed;

                var personIds = new PersonService( rockContext )
                    .GetByFullName( searchValue, false, false, false, out reversed )
                    .AsNoTracking()
                    .Select( a => a.Id )
                    .ToList();

                attendanceList = attendanceList.Where( a => personIds.Contains( a.PersonId ) ).ToList();
            }

            var attendees = RosterAttendee.GetFromAttendanceList( attendanceList );

            return attendees;
        }

        #endregion
    }
}