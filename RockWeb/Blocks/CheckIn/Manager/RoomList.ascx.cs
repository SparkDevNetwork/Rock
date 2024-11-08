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
using System.Linq;
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
    [DisplayName( "Room List" )]
    [Category( "Check-in > Manager" )]
    [Description( "Shows all locations of the type room for the campus (context) and selected schedules." )]

    #region Block Attributes

    [BooleanField(
        "Show All Areas",
        Key = AttributeKey.ShowAllAreas,
        Description = "If enabled, all Check-in Areas will be shown. This setting will be ignored if a specific area is specified in the URL.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [LinkedPage(
        "Area Select Page",
        Key = AttributeKey.AreaSelectPage,
        Description = "If Show All Areas is not enabled, the page to redirect user to if a Check-in Area has not been configured or selected.",
        IsRequired = false,
        Order = 2 )]

    [GroupTypeField(
        "Check-in Area",
        Key = AttributeKey.CheckInAreaGuid,
        Description = "If Show All Areas is not enabled, the Check-in Area for the rooms to be managed by this Block.",
        IsRequired = false,
        GroupTypePurposeValueGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE,
        Order = 3 )]

    [LinkedPage(
        "Roster Page",
        Key = AttributeKey.RosterPage,
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Show Only Parent Group",
        "When enabled, only the actual parent group for each check-in group-location will be shown and groups under the same parent group in the same location will be combined into one row.",
        Key = AttributeKey.ShowOnlyParentGroup,
        DefaultBooleanValue = false,
        Order = 5 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "2DEA7808-9AC1-4913-BF58-1CDC7922C901" )]
    public partial class RoomList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for block attributes.
        /// </summary>
        private class AttributeKey
        {
            public const string PersonPage = "PersonPage";
            public const string ShowAllAreas = CheckinManagerHelper.BlockAttributeKey.ShowAllAreas;
            public const string AreaSelectPage = "AreaSelectPage";

            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string CheckInAreaGuid = CheckinManagerHelper.BlockAttributeKey.CheckInAreaGuid;

            public const string RosterPage = "RosterPage";

            /// <summary>
            /// When enabled, only the actual parent group for the check-in group-location will be shown
            /// in the Room Name grid column and groups under the same parent group in the same
            /// location will be combined into one row.
            /// Otherwise, the actual group will be shown along with the parent group/area hierarchy under
            /// it (as per typical Rock check-in notation).
            /// </summary>
            public const string ShowOnlyParentGroup = "ShowOnlyParentGroup";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private class PageParameterKey
        {
            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string Area = CheckinManagerHelper.PageParameterKey.Area;

            /// <summary>
            /// If this is specified in the URL, show the direct (first level) child locations of the specified ParentLocationId.
            /// Also, set the PanelTitle in the format of "{{ParentLocation.Name}} Child Locations"
            /// This will take precedence over the selected campus+locations.
            /// </summary>
            public const string ParentLocationId = "ParentLocationId";

            /// <summary>
            /// If LocationId is specified in the URL, list only items for the specified location.
            /// Also, hide the Location Grid Column and set the PanelTitle as the location's name
            /// This will take precedence over the selected campus+locations and/or <seealso cref="ParentLocationId"/>
            /// </summary>
            public const string LocationId = "LocationId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gRoomList.GridRebind += gList_GridRebind;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload if block settings change
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the Filter
        /// </summary>
        /// <returns></returns>
        private bool HasFilterErrors()
        {
            CampusCache campus = GetCampusFromContext();
            if ( campus == null )
            {
                nbWarning.Text = "Please select a Campus.";
                nbWarning.NotificationBoxType = NotificationBoxType.Warning;
                nbWarning.Visible = true;
                return true;
            }

            if ( !campus.LocationId.HasValue )
            {
                nbWarning.Text = "This campus does not have any locations.";
                nbWarning.NotificationBoxType = NotificationBoxType.Warning;
                nbWarning.Visible = true;
                return true;
            }

            // If ShowAllAreas is false, the CheckinAreaFilter is required.
            if ( this.GetAttributeValue( AttributeKey.ShowAllAreas ).AsBoolean() == false )
            {
                var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
                if ( checkinAreaFilter == null )
                {
                    if ( NavigateToLinkedPage( AttributeKey.AreaSelectPage ) )
                    {
                        // We are navigating to get the Area Filter which will get the Area cookie.
                        return true;
                    }
                    else
                    {
                        nbWarning.Text = "The 'Area Select Page' Block Attribute must be defined.";
                        nbWarning.NotificationBoxType = NotificationBoxType.Warning;
                        nbWarning.Visible = true;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( HasFilterErrors() )
            {
                return;
            }

            var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
            CampusCache campus = GetCampusFromContext();

            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList();
            if ( selectedScheduleIds.Any() )
            {
                btnShowFilter.AddCssClass( "criteria-exists bg-warning" );
            }
            else
            {
                btnShowFilter.RemoveCssClass( "criteria-exists bg-warning" );
            }

            CheckinManagerHelper.SaveRoomListFilterToCookie( selectedScheduleIds.ToArray() );

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );
            IEnumerable<CheckinAreaPath> checkinAreaPaths;
            if ( checkinAreaFilter != null )
            {
                checkinAreaPaths = groupTypeService.GetCheckinAreaDescendantsPath( checkinAreaFilter.Id );
            }
            else
            {
                checkinAreaPaths = groupTypeService.GetAllCheckinAreaPaths();
            }

            var selectedGroupTypeIds = checkinAreaPaths.Select( a => a.GroupTypeId ).Distinct().ToArray();

            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocationsQuery = groupLocationService.Queryable()
                    .Where( gl => selectedGroupTypeIds.Contains( gl.Group.GroupTypeId ) && gl.Group.IsActive && ( !gl.Group.IsArchived ) );

            var parentLocationIdParameter = PageParameter( PageParameterKey.ParentLocationId ).AsIntegerOrNull();
            var locationIdParameter = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
            var locationGridField = gRoomList.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lRoomName" );
            if ( locationGridField != null && !locationGridField.Visible )
            {
                locationGridField.Visible = true;
            }

            List<int> locationIds;
            if ( locationIdParameter.HasValue )
            {
                // If LocationId is specified in the URL, list only items for the specified location.
                // Also, hide the Location Grid Column and set the PanelTitle as the location's name
                // This will take precedence over the selected campus+locations and/or <seealso cref="ParentLocationId"/>
                var locationService = new LocationService( rockContext );
                lPanelTitle.Text = locationService.GetSelect( locationIdParameter.Value, s => s.Name );

                locationIds = new List<int>();
                locationIds.Add( locationIdParameter.Value );

                if ( locationGridField != null )
                {
                    // since a LocationId parameter was specified, the LocationGrid field doesn't need to be shown
                    locationGridField.Visible = false;
                }
            }
            else if ( parentLocationIdParameter.HasValue )
            {
                // If parentLocationId is specified, show the direct (first level) child locations of the specified ParentLocationId.
                // This will take precedence over the selected campus+locations.
                var locationService = new LocationService( rockContext );
                locationIds = locationService.Queryable()
                    .Where( a => a.ParentLocationId.HasValue && a.ParentLocationId.Value == parentLocationIdParameter.Value )
                    .Select( a => a.Id ).ToList();

                lPanelTitle.Text = string.Format( "{0} Child Locations", locationService.GetSelect( parentLocationIdParameter.Value, s => s.Name ) );
            }
            else
            {
                // Limit locations (rooms) to locations within the selected campus.
                locationIds = new LocationService( rockContext ).GetAllDescendentIds( campus.LocationId.Value ).ToList();
                locationIds.Add( campus.LocationId.Value, true );

                lPanelTitle.Text = "Room List";
            }

            groupLocationsQuery = groupLocationsQuery.Where( a => locationIds.Contains( a.LocationId ) );

            if ( selectedScheduleIds.Any() )
            {
                groupLocationsQuery = groupLocationsQuery.Where( a => a.Schedules.Any( s => s.IsActive && s.CheckInStartOffsetMinutes.HasValue && selectedScheduleIds.Contains( s.Id ) ) );
            }
            else
            {
                groupLocationsQuery = groupLocationsQuery.Where( a => a.Schedules.Any( s => s.IsActive && s.CheckInStartOffsetMinutes.HasValue ) );
            }

            var groupLocationList = groupLocationsQuery.Select( a => new GroupLocationInfo
            {
                LocationId = a.LocationId,
                LocationName = a.Location.Name,
                ParentGroupId = a.Group.ParentGroupId,
                ParentGroupName = a.Group.ParentGroup.Name,
                GroupId = a.Group.Id,
                GroupName = a.Group.Name,
                GroupTypeId = a.Group.GroupTypeId
            } ).ToList();

            var startDateTime = RockDateTime.Today;
            DateTime currentDateTime;
            if ( campus != null )
            {
                currentDateTime = campus.CurrentDateTime;
            }
            else
            {
                currentDateTime = RockDateTime.Now;
            }

            // Get all Attendance records for the current day and location.
            var attendanceQuery = new AttendanceService( rockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.ScheduleId.HasValue );

            // Limit attendances (rooms) to the groupLocations' LocationId and GroupIds that we'll be showing
            var groupLocationLocationIds = groupLocationList.Select( a => a.LocationId ).Distinct().ToList();
            var groupLocationGroupsIds = groupLocationList.Select( a => a.GroupId ).Distinct().ToList();

            attendanceQuery = attendanceQuery.Where( a =>
                groupLocationLocationIds.Contains( a.Occurrence.LocationId.Value )
                && groupLocationGroupsIds.Contains( a.Occurrence.GroupId.Value ) );

            attendanceQuery = attendanceQuery.Where( a => selectedGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );

            if ( selectedScheduleIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var rosterAttendeeAttendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            var groupTypeIdsWithAllowCheckout = selectedGroupTypeIds
                .Select( a => GroupTypeCache.Get( a ) )
                .Where( a => a != null )
                .Where( gt => gt.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean() )
                .Select( a => a.Id )
                .Distinct().ToList();

            var groupTypeIdsWithEnablePresence = selectedGroupTypeIds
                .Select( a => GroupTypeCache.Get( a ) )
                .Where( a => a != null )
                .Where( gt => gt.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean() )
                .Select( a => a.Id )
                .Distinct();

            var scheduleIds = rosterAttendeeAttendanceList.Select( a => a.ScheduleId.Value ).Distinct().ToList();
            var scheduleList = new ScheduleService( rockContext ).GetByIds( scheduleIds ).ToList();
            var scheduleIdsWasScheduleOrCheckInActiveForCheckOut = new HashSet<int>( scheduleList.Where( a => a.WasScheduleOrCheckInActiveForCheckOut( currentDateTime ) ).Select( a => a.Id ).ToList() );

            rosterAttendeeAttendanceList = rosterAttendeeAttendanceList.Where( a =>
            {
                var allowCheckout = groupTypeIdsWithAllowCheckout.Contains( a.GroupTypeId );
                if ( !allowCheckout )
                {
                    /*
                        If AllowCheckout is false, remove all Attendees whose schedules are not currently active. Per the 'WasSchedule...ActiveForCheckOut()'
                        method below: "Check-out can happen while check-in is active or until the event ends (start time + duration)." This will help to keep
                        the list of 'Present' attendees cleaned up and accurate, based on the room schedules, since the volunteers have no way to manually mark
                        an Attendee as 'Checked-out'.

                        If, on the other hand, AllowCheckout is true, it will be the volunteers' responsibility to click the [Check-out] button when an
                        Attendee leaves the room, in order to keep the list of 'Present' Attendees in order. This will also allow the volunteers to continue
                        'Checking-out' Attendees in the case that the parents are running late in picking them up.
                    */

                    return scheduleIdsWasScheduleOrCheckInActiveForCheckOut.Contains( a.ScheduleId.Value );
                }
                else
                {
                    return true;
                }
            } ).ToList();

            var attendancesByLocationId = rosterAttendeeAttendanceList
                .GroupBy( a => a.LocationId.Value ).ToDictionary( k => k.Key, v => v.ToList() );

            _attendancesByLocationIdAndGroupId = attendancesByLocationId.ToDictionary(
                   k => k.Key,
                   v => v.Value.GroupBy( x => x.GroupId.Value ).ToDictionary( x => x.Key, xx => xx.ToList() ) );

            _checkinAreaPathsLookupByGroupTypeId = checkinAreaPaths.ToDictionary( k => k.GroupTypeId, v => v );

            _showOnlyParentGroup = this.GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            var roomList = new List<RoomInfo>();

            foreach ( var groupLocation in groupLocationList )
            {
                AddToRoomList( roomList, groupLocation );
            }

            List<RoomInfo> sortedRoomList;
            if ( _showOnlyParentGroup )
            {
                sortedRoomList = roomList.OrderBy( a => a.LocationName ).ToList();
            }
            else
            {
                sortedRoomList = new List<RoomInfo>();
                sortedRoomList.AddRange( roomList.OfType<RoomInfoByGroup>().OrderBy( a => a.LocationName ).ThenBy( a => a.GroupName ).ToList() );
            }

            var checkedInCountField = gRoomList.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lCheckedInCount" );
            var presentCountField = gRoomList.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lPresentCount" );
            var checkedOutCountField = gRoomList.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lCheckedOutCount" );

            checkedOutCountField.Visible = groupTypeIdsWithAllowCheckout.Any();

            // Always show Present Count regardless of the 'Enable Presence' setting. (A person gets automatically marked present if 'Enable Presence' is disabled.)
            presentCountField.Visible = true;
            if ( groupTypeIdsWithEnablePresence.Any() )
            {
                // Presence is enabled, so records could be in the 'Checked-in' state
                // and Present column should be labeled 'Present'.
                checkedInCountField.Visible = true;
                presentCountField.HeaderText = "Present";
            }
            else
            {
                // https://app.asana.com/0/0/1199637795718017/f
                // 'Enable Presence' is disabled, so a person automatically gets marked present.
                // So, no records will be in the 'Checked-In (but no present)' state.
                // Also, a user thinks of 'Present' as 'Checked-In' if they don't use the 'Enable Presence' feature
                checkedInCountField.Visible = false;
                presentCountField.HeaderText = "Checked-In";
            }

            if ( _showOnlyParentGroup )
            {
                gRoomList.DataKeyNames = new string[1] { "LocationId" };
            }
            else
            {
                gRoomList.DataKeyNames = new string[2] { "LocationId", "GroupId" };
            }

            gRoomList.DataSource = sortedRoomList;
            gRoomList.DataBind();
        }

        // lookups for adding groupLocationInfo to the RoomList
        private Dictionary<int, Dictionary<int, List<RosterAttendeeAttendance>>> _attendancesByLocationIdAndGroupId;
        private Dictionary<int, CheckinAreaPath> _checkinAreaPathsLookupByGroupTypeId;

        private bool _showOnlyParentGroup;

        /// <summary>
        /// Adds to room list.
        /// </summary>
        /// <param name="roomList">The room list.</param>
        /// <param name="groupLocation">The group location.</param>
        private void AddToRoomList( List<RoomInfo> roomList, GroupLocationInfo groupLocation )
        {
            Dictionary<int, List<RosterAttendee>> rosterAttendeesForLocationByGroupId = _attendancesByLocationIdAndGroupId
                .GetValueOrNull( groupLocation.LocationId )
                ?.ToDictionary( k => k.Key, v => RosterAttendee.GetFromAttendanceList( v.Value.ToList() ).ToList() );

            List<RosterAttendee> rosterAttendeesForLocationAndGroup;
            if ( rosterAttendeesForLocationByGroupId != null )
            {
                rosterAttendeesForLocationAndGroup = rosterAttendeesForLocationByGroupId.GetValueOrNull( groupLocation.GroupId );

                if ( rosterAttendeesForLocationAndGroup == null )
                {
                    // no attendances for this Location (Room) and Group
                    rosterAttendeesForLocationAndGroup = new List<RosterAttendee>();
                }
            }
            else
            {
                // no attendances for this Location (Room)
                rosterAttendeesForLocationByGroupId = new Dictionary<int, List<RosterAttendee>>();
                rosterAttendeesForLocationAndGroup = new List<RosterAttendee>();
            }

            var roomCounts = new RoomCounts
            {
                CheckedInList = rosterAttendeesForLocationAndGroup.Where( a => a.Status == RosterAttendeeStatus.CheckedIn ).ToList(),
                PresentList = rosterAttendeesForLocationAndGroup.Where( a => a.Status == RosterAttendeeStatus.Present ).ToList(),
                CheckedOutList = rosterAttendeesForLocationAndGroup.Where( a => a.Status == RosterAttendeeStatus.CheckedOut ).ToList(),
            };

            if ( _showOnlyParentGroup )
            {
                // roll group/location counts into the parent group
                // for example, if the Group Structure is
                // - Babies
                // --- 101 Blue
                // --- 101 Green
                // --- 101 Orange
                // --- 101 Red
                // --- 102 Blue
                // combine each ChildGroup+Location into Babies.
                // also, the grid would be single location per row, instead of location+group per row
                RoomInfoByParentGroups groupInfoByParentGroups = roomList.OfType<RoomInfoByParentGroups>().Where( a => a.LocationId == groupLocation.LocationId ).FirstOrDefault();
                if ( groupInfoByParentGroups == null )
                {
                    // since we are rolling into ParentGroup, use the ParentGroup/ParentGroupId as the group
                    groupInfoByParentGroups = new RoomInfoByParentGroups
                    {
                        LocationId = groupLocation.LocationId,
                        LocationName = groupLocation.LocationName,
                        RoomCounts = roomCounts,
                        ParentGroupNames = new Dictionary<int, string>(),
                    };

                    roomList.Add( groupInfoByParentGroups );
                }
                else
                {
                    groupInfoByParentGroups.RoomCounts.CheckedInList.AddRange( roomCounts.CheckedInList );
                    groupInfoByParentGroups.RoomCounts.PresentList.AddRange( roomCounts.PresentList );
                    groupInfoByParentGroups.RoomCounts.CheckedOutList.AddRange( roomCounts.CheckedOutList );
                }

                if ( groupLocation.ParentGroupId.HasValue )
                {
                    groupInfoByParentGroups.ParentGroupNames.TryAdd( groupLocation.ParentGroupId.Value, groupLocation.ParentGroupName );
                }
            }
            else
            {
                var roomInfoByGroup = new RoomInfoByGroup
                {
                    LocationId = groupLocation.LocationId,
                    GroupId = groupLocation.GroupId,
                    GroupName = groupLocation.GroupName,
                    LocationName = groupLocation.LocationName,
                    RoomCounts = roomCounts,
                    GroupPath = new RoomGroupPathInfo
                    {
                        GroupId = groupLocation.GroupId,
                        GroupName = groupLocation.GroupName,
                        GroupTypePath = _checkinAreaPathsLookupByGroupTypeId.GetValueOrNull( groupLocation.GroupTypeId )
                    }
                };

                roomList.Add( roomInfoByGroup );
            }
        }

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

        #endregion

        /// <summary>
        /// Handles the RowDataBound event of the gRoomList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRoomList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            RoomInfo roomInfo = e.Row.DataItem as RoomInfo;
            if ( roomInfo == null )
            {
                return;
            }

            var lRoomName = e.Row.FindControl( "lRoomName" ) as Literal;
            var lGroupNameAndPath = e.Row.FindControl( "lGroupNameAndPath" ) as Literal;
            var lCheckedInCount = e.Row.FindControl( "lCheckedInCount" ) as Literal;
            var lPresentCount = e.Row.FindControl( "lPresentCount" ) as Literal;
            var lCheckedOutCount = e.Row.FindControl( "lCheckedOutCount" ) as Literal;

            lRoomName.Text = roomInfo.LocationName;

            if ( roomInfo is RoomInfoByGroup )
            {
                lGroupNameAndPath.Text = ( roomInfo as RoomInfoByGroup ).GroupsPathHTML;
            }
            else if ( roomInfo is RoomInfoByParentGroups )
            {
                lGroupNameAndPath.Text = ( roomInfo as RoomInfoByParentGroups ).ParentGroupNames.Select( a => a.Value )
                    .OrderBy( a => a ).ToList().AsDelimited( "," );
            }

            if ( roomInfo.RoomCounts != null )
            {
                lCheckedInCount.Text = roomInfo.RoomCounts.CheckedInCount.ToString();
                lPresentCount.Text = roomInfo.RoomCounts.PresentCount.ToString();
                lCheckedOutCount.Text = roomInfo.RoomCounts.CheckedOutCount.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowFilter_Click( object sender, EventArgs e )
        {
            pnlFilterCriteria.Visible = !pnlFilterCriteria.Visible;
        }

        /// <summary>
        /// Shows the filters.
        /// </summary>
        private void BindFilter()
        {
            ScheduleService scheduleService = new ScheduleService( new RockContext() );

            var selectedScheduleIds = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().RoomListScheduleIdsFilter;

            // Limit Schedules to ones that are Active, have a CheckInStartOffsetMinutes, and are Named schedules.
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
        }

        /// <summary>
        /// Handles the Click event of the btnApplyFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyFilter_Click( object sender, EventArgs e )
        {
            pnlFilterCriteria.Visible = false;
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
        }

        /// <summary>
        /// Handles the RowSelected event of the gRoomList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRoomList_RowSelected( object sender, RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>
            {
                { "LocationId", e.RowKeyId.ToString() }
            };

            NavigateToLinkedPage( AttributeKey.RosterPage, queryParams );
        }

        /// <summary>
        ///  RoomInfo for when <seealso cref="AttributeKey.ShowOnlyParentGroup"/> is enabled.
        ///  In this mode, each row is a location, instead of Location+Group
        ///  So, there could be more than one group in this location
        /// </summary>
        /// <seealso cref="RockWeb.Blocks.CheckIn.Manager.RoomList.RoomInfo" />
        private class RoomInfoByParentGroups : RoomInfo
        {
            public Dictionary<int, string> ParentGroupNames { get; set; }
        }

        /// <summary>
        ///  RoomInfo for when <seealso cref="AttributeKey.ShowOnlyParentGroup"/> is disable.
        ///  In this mode, each row is  Location+Group
        ///  So, there could is only one group and one location for this
        /// </summary>
        /// <seealso cref="RockWeb.Blocks.CheckIn.Manager.RoomList.RoomInfo" />
        private class RoomInfoByGroup : RoomInfo
        {
            public int GroupId { get; internal set; }

            public RoomGroupPathInfo GroupPath { get; internal set; }

            public string GroupsPathHTML
            {
                get
                {
                    var groupsHtmlFormat =
@"<div class='group-name'>{0}</div>
<div class='small text-muted text-wrap'>{1}</div>";

                    return string.Format( groupsHtmlFormat, GroupPath.GroupName, GroupPath.GroupTypePath );
                }
            }

            public string GroupName { get; internal set; }
        }

        /// <summary>
        ///
        /// </summary>
        private abstract class RoomInfo
        {
            public int LocationId { get; internal set; }

            public string LocationName { get; internal set; }

            public RoomCounts RoomCounts { get; internal set; }
        }

        /// <summary>
        ///
        /// </summary>
        private class RoomGroupPathInfo
        {
            public int GroupId { get; internal set; }

            public string GroupName { get; internal set; }

            public int GroupTypeId { get; internal set; }

            public CheckinAreaPath GroupTypePath { get; internal set; }
        }

        private class RoomCounts
        {
            public List<RosterAttendee> CheckedInList { get; internal set; }

            public int CheckedInCount => CheckedInList.DistinctBy( a => a.PersonId ).Count();

            public List<RosterAttendee> PresentList { get; internal set; }

            public int PresentCount => PresentList.DistinctBy( a => a.PersonId ).Count();

            public List<RosterAttendee> CheckedOutList { get; internal set; }

            public int CheckedOutCount => CheckedOutList.DistinctBy( a => a.PersonId ).Count();
        }

        private class GroupLocationInfo
        {
            public int LocationId { get; internal set; }

            public string LocationName { get; internal set; }

            public int? ParentGroupId { get; internal set; }

            public string ParentGroupName { get; internal set; }

            public int GroupId { get; internal set; }

            public string GroupName { get; internal set; }

            public int GroupTypeId { get; internal set; }
        }
    }
}