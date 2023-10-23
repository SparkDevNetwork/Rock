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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
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

        #region Base Control Methods

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
        /// Handles the SelectedIndexChanged event of the ddlMovePersonSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            // retain the selected location if it is available for the selected schedule
            var previouslySelectedLocationId = ddlMovePersonLocation.SelectedValueAsInt();
            Load_ddlMovePersonLocations( previouslySelectedLocationId, null );

            // retain the selected group if it is available for the selected schedule/location
            var previouslySelectedGroupId = ddlMovePersonGroup.SelectedValueAsInt();
            Load_ddlMovePersonGroups( previouslySelectedGroupId );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMovePersonLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMovePersonLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            nbMovePersonLocationFull.Visible = false;

            // retain the selected group if it is available for the selected schedule/location
            var previouslySelectedGroupId = ddlMovePersonGroup.SelectedValueAsInt();
            Load_ddlMovePersonGroups( previouslySelectedGroupId );
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

            var attendanceInfo = new AttendanceService( new RockContext() ).GetSelect( attendanceId.Value, s => new { s.Occurrence, s.CampusId } );
            var occurrence = attendanceInfo?.Occurrence;
            if ( occurrence == null )
            {
                nbMovePersonLocationFull.Text = "Attendance Not Found";
                nbMovePersonLocationFull.Visible = true;
                return;
            }

            var attendanceCampusId = attendanceInfo?.CampusId;

            Load_ddlMovePersonSchedule( occurrence.ScheduleId, attendanceCampusId );
            Load_ddlMovePersonLocations( occurrence.LocationId, attendanceCampusId );
            Load_ddlMovePersonGroups( occurrence.GroupId );
        }

        /// <summary>
        /// Loads ddlMovePersonSchedule with the available schedules. Filters by campus if provided and will select the provided schedule if it is valid.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        private void Load_ddlMovePersonSchedule( int? scheduleId, int? campusId )
        {
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var activeSchedules = groupLocationService.Queryable().SelectMany( gl => gl.Schedules ).Distinct().ToList();
            var currentDateTime = CampusCache.Get( campusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
            List<Schedule> scheduleList = new List<Schedule>();

            foreach ( var schedule in activeSchedules )
            {
                var isAvailable = schedule.GetCheckInTimes( currentDateTime ).Any( t => t.CheckInStart <= currentDateTime && t.CheckInEnd > currentDateTime );
                if ( isAvailable )
                {
                    scheduleList.Add( schedule );
                }
            }

            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            ddlMovePersonSchedule.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                ddlMovePersonSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }

            if ( sortedScheduleList.Where( s => s.Id == scheduleId ).Any() )
            {
                ddlMovePersonSchedule.SelectedValue = scheduleId.ToString();
            }
        }

        /// <summary>
        /// Loads ddlMovePersonLocations with the available locations based on the selected schedule and will select the provided location if it is valid.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        private void Load_ddlMovePersonLocations( int? locationId, int? campusId )
        {
            int? scheduleId = ddlMovePersonSchedule.SelectedValue.AsIntegerOrNull();
            if ( scheduleId.IsNullOrZero() )
            {
                // Clear the locations and disable selection until a schedule is provided.
                ddlMovePersonLocation.Items.Clear();
                ddlMovePersonLocation.Enabled = false;

                return;
            }

            // Get a list of GroupLocations for the schedule
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocations = groupLocationService.Queryable().Where( gl => gl.Schedules.Any( s => s.Id == scheduleId ) );

            // See if we can get a campus if one was not provided
            if ( campusId.IsNullOrZero() )
            {
                //var attendanceId = GetAttendanceId();
                var attendanceId = ddlMovePersonSelectAttendance.SelectedValueAsId();
                campusId = new AttendanceService( rockContext ).Queryable().Where( a => a.Id == attendanceId.Value ).Select( a => a.CampusId ).FirstOrDefault();
            }

            // Load the Locations filtered by schedule
            var campusLocationId = CampusCache.Get( campusId.Value )?.LocationId;
            var locationService = new LocationService( rockContext );
            List<Location> locations = null;
            if ( campusLocationId != null )
            {
                // Get a list of locations that are children of the campus then JOIN it to the list of groupLocations to filter it.
                locations = locationService
                    .GetAllDescendents( campusLocationId.Value )
                    .Join( groupLocations, l => l.Id, gl => gl.LocationId, ( l, gl ) => l )
                    .Where( l => l.IsActive == true )
                    .Distinct()
                    .OrderBy( l => l.Name )
                    .ToList();
            }
            else
            {
                locations = locationService
                    .Queryable()
                    .Join( groupLocations, l => l.Id, gl => gl.LocationId, ( l, gl ) => l )
                    .Where( l => l.IsActive == true )
                    .Distinct()
                    .OrderBy( l => l.Name )
                    .ToList();
            }

            
            ddlMovePersonLocation.Enabled = true;
            ddlMovePersonLocation.Items.Clear();

            // If there is more than one option add a blank option. This blank option will be auto selected if there is more than one available location and the previous location is not available for the selected schedule.
            if ( locations.Count > 1 )
            {
                ddlMovePersonLocation.Items.Add( new ListItem( string.Empty, string.Empty ) );
            }

            foreach ( var location in locations )
            {
                ddlMovePersonLocation.Items.Add( new ListItem( location.Name, location.Id.ToString() ) );
            }

            if ( locationId.IsNotNullOrZero() && locations.Where( l => l.Id == locationId ).Any() )
            {
                ddlMovePersonLocation.SelectedValue = locationId.ToString();
            }
        }

        /// <summary>
        /// Loads ddlMovePersonGroups with the available groups based on the selected schedule/location and will selected the provided group if it is valid.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        private void Load_ddlMovePersonGroups( int? groupId )
        {
            int? scheduleId = ddlMovePersonSchedule.SelectedValue.AsIntegerOrNull();
            int? locationId = ddlMovePersonLocation.SelectedValue.AsIntegerOrNull();

            if ( scheduleId.IsNullOrZero() || locationId.IsNullOrZero() )
            {
                // Clear the groups and disable selection until a location is provided.
                ddlMovePersonGroup.Items.Clear();
                ddlMovePersonGroup.Enabled = false;
                return;
            }

            // Get a list of GroupLocations for the schedule
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var groups = groupLocationService
                .Queryable()
                .Where( gl => gl.Schedules.Any( s => s.Id == scheduleId ) && gl.LocationId == locationId && gl.Group.IsActive )
                .Select( gl => gl.Group )
                .Include( a => a.ParentGroup )
                .ToList();

            // Load the Groups filtered by schedule and location
            ddlMovePersonGroup.Enabled = true;
            ddlMovePersonGroup.Items.Clear();

            // If there is more than one group add a blank option. This blank option will be auto selected if there is more than one available group and the previous group is not available for the selected schedule/location.
            if ( groups.Count > 1 )
            {
                ddlMovePersonGroup.Items.Add( new ListItem( string.Empty, string.Empty ) );
            }

            foreach ( var group in groups )
            {
                if ( group.ParentGroup != null )
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( $"{group.ParentGroup.Name} > {group.Name}", group.Id.ToString() ) );
                }
                else
                {
                    ddlMovePersonGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                }
            }

            if ( groupId != null && groups.Where( g => g.Id == groupId ).Any() )
            {
                ddlMovePersonGroup.SelectedValue = groupId.ToString();
            }
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

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.Get( attendanceId.Value );
            if ( attendance == null )
            {
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

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd( selectedOccurrenceDate, selectedGroupId, selectedLocationId, selectedScheduleId );
            attendance.OccurrenceId = newRoomsOccurrence.Id;
            rockContext.SaveChanges();

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