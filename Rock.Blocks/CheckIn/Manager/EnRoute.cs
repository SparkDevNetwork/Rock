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

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;

using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Manager.EnRoute;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Lists the people who are checked-in but not yet marked present (or
    /// another roster status, depending on the block's "Filter By" setting).
    /// Provides filters for schedule, group, and name search, and a "Move
    /// Person" action to relocate an attendee to a different
    /// schedule/location/group combination.
    /// </summary>

    [DisplayName( "En Route" )]
    [Category( "Check-in > Manager" )]
    [Description( "Lists the people who are checked-in but not yet marked present." )]
    [IconCssClass( "ti ti-map-route" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [Rock.Web.UI.ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [CustomDropdownListField(
        "Filter By",
        Description = "This controls which people appear in the list. For example, when set to 'Checked-in' people who are only checked-in (not yet marked 'Present') will be shown. For more information read about the 'Enable Presence' feature in the check-in documentation.",
        Key = AttributeKey.FilterBy,
        DefaultValue = "2",
        ListSource = "2^Checked-in,3^Present,4^Checked-out",
        IsRequired = true,
        Order = 1 )]

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

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "8D2F4A6E-1B3C-4D5E-9F7A-0E2C4B6D8A1F" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CHECK_IN_MANAGER_EN_ROUTE )]
    public class EnRoute : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string FilterBy = "FilterBy";
            public const string ShowOnlyParentGroup = "ShowOnlyParentGroup";
            public const string AlwaysShowChildGroups = "AlwaysShowChildGroups";
        }

        /// <summary>
        /// Keys for settings stored in the CheckinManager cookie.
        /// These are stored in a cookie (not user-preference) since the same
        /// login is often used by multiple devices when running Checkin-Manager.
        /// </summary>
        private static class CustomSettingKey
        {
            public const string EnRouteScheduleIdsFilter = "EnRouteScheduleIdsFilter";
            public const string EnRoutePickedGroupIdsFilter = "EnRoutePickedGroupIdsFilter";
            public const string EnRouteIncludeChildGroupsFilter = "EnRouteIncludeChildGroupsFilter";
        }

        private const string GroupListItemKeyDelimiter = "|";

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            var options = BuildOptionsBag();

            return new ListBlockBox<EnRouteOptionsBag>
            {
                Options = options,
                GridDefinition = GetGridBuilder().BuildDefinition()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Returns the grid data for the En Route list based on the
        /// specified filter criteria. Persists the filter selection to the
        /// shared CheckinManager cookie so it survives the next visit.
        /// </summary>
        /// <param name="selectedScheduleIds">The selected schedule integer ids.</param>
        /// <param name="selectedGroupGuids">The selected group Guids from the GroupPicker.</param>
        /// <param name="includeChildGroups">Whether to include child groups of the selected groups.</param>
        /// <param name="searchText">The name search text.</param>
        [BlockAction]
        public BlockActionResult GetGridData( List<int> selectedScheduleIds, List<Guid> selectedGroupGuids, bool includeChildGroups, string searchText )
        {
            selectedScheduleIds = selectedScheduleIds ?? new List<int>();
            selectedGroupGuids = selectedGroupGuids ?? new List<Guid>();

            // Resolve group Guids to integer ids for cookie storage and
            // downstream query filtering.
            var selectedGroupIds = new List<int>();
            if ( selectedGroupGuids.Any() )
            {
                selectedGroupIds = new GroupService( RockContext ).Queryable()
                    .Where( g => selectedGroupGuids.Contains( g.Guid ) )
                    .Select( g => g.Id )
                    .ToList();
            }

            // Save the filter state to the shared CheckinManager cookie.
            SaveFilterToCookie( selectedScheduleIds, selectedGroupIds, includeChildGroups );

            var attendees = GetAttendees( RockContext, selectedScheduleIds, selectedGroupIds, includeChildGroups, searchText );

            var gridData = GetGridBuilder().Build( attendees );

            return ActionOk( new EnRouteGridDataBag
            {
                GridData = gridData
            } );
        }

        /// <summary>
        /// Returns the move-person options for the given attendance ids.
        /// Populates the attendance selector (when a person is checked into
        /// multiple services) and the cascading Schedule / Location / Group
        /// dropdowns for the most recent attendance.
        /// </summary>
        /// <param name="attendanceIds">The attendance ids from the grid row.</param>
        [BlockAction]
        public BlockActionResult GetMovePersonOptions( List<int> attendanceIds )
        {
            if ( attendanceIds == null || !attendanceIds.Any() )
            {
                return ActionOk( new EnRouteMovePersonOptionsBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            var attendanceService = new AttendanceService( RockContext );
            var attendances = attendanceService.Queryable()
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Location )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.PersonAlias )
                .Where( a => attendanceIds.Contains( a.Id ) )
                .OrderByDescending( a => a.StartDateTime )
                .ToList();

            if ( !attendances.Any() )
            {
                return ActionOk( new EnRouteMovePersonOptionsBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            var mostRecentAttendance = attendances.First();

            // Build the attendance selector items.
            var attendanceListItems = attendances.Select( a => new ListItemBag
            {
                Value = a.Id.ToString(),
                Text = $"{a.Occurrence.Group?.Name} in {a.Occurrence.Location?.Name} at {a.Occurrence.Schedule?.Name}"
            } ).ToList();

            var hasMultiple = attendances.Count > 1;
            var instructionText = hasMultiple
                ? $"{mostRecentAttendance.PersonAlias} is en-route to multiple services. Select the one to be moved."
                : null;

            // Build cascading dropdown options for the most recent attendance.
            var optionsBag = BuildMovePersonOptions( mostRecentAttendance );

            optionsBag.Attendances = attendanceListItems;
            optionsBag.HasMultipleAttendances = hasMultiple;
            optionsBag.InstructionText = instructionText;

            return ActionOk( optionsBag );
        }

        /// <summary>
        /// Refreshes the move-person cascading dropdown options for a
        /// specific attendance when the user changes the attendance selector.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [BlockAction]
        public BlockActionResult RefreshMovePersonOptions( int attendanceId )
        {
            var attendance = new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( a => a.Occurrence.Group )
                .FirstOrDefault( a => a.Id == attendanceId );

            if ( attendance?.Occurrence?.Group == null )
            {
                return ActionOk( new EnRouteMovePersonOptionsBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            return ActionOk( BuildMovePersonOptions( attendance ) );
        }

        /// <summary>
        /// Moves the specified attendance record to the selected
        /// schedule/location/group combination. Validates the firm room
        /// threshold before completing the move.
        /// </summary>
        /// <param name="request">The move request details.</param>
        [BlockAction]
        public BlockActionResult MovePerson( EnRouteMovePersonRequestBag request )
        {
            if ( request == null )
            {
                return ActionOk( new EnRouteMovePersonResponseBag
                {
                    ErrorMessage = "No move data was supplied."
                } );
            }

            var attendanceService = new AttendanceService( RockContext );
            var attendanceOccurrenceService = new AttendanceOccurrenceService( RockContext );
            var attendance = attendanceService.Get( request.AttendanceId );

            if ( attendance == null )
            {
                return ActionOk( new EnRouteMovePersonResponseBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            if ( !request.ScheduleId.HasValue )
            {
                return ActionOk( new EnRouteMovePersonResponseBag { ErrorMessage = "Schedule Not Found" } );
            }

            if ( !request.LocationId.HasValue )
            {
                return ActionOk( new EnRouteMovePersonResponseBag { ErrorMessage = "Location Not Found" } );
            }

            if ( !request.GroupId.HasValue )
            {
                return ActionOk( new EnRouteMovePersonResponseBag { ErrorMessage = "Group Not Found" } );
            }

            var selectedOccurrenceDate = attendance.Occurrence.OccurrenceDate;
            var location = NamedLocationCache.Get( request.LocationId.Value );
            var locationFirmRoomThreshold = location?.FirmRoomThreshold;

            if ( locationFirmRoomThreshold.HasValue )
            {
                // Count people still checked in (not checked out), excluding
                // the current person who may already be in that location, then
                // add one for the person we are trying to move.
                var locationCount = attendanceService
                    .GetByDateOnLocationAndSchedule( selectedOccurrenceDate, request.LocationId.Value, request.ScheduleId.Value )
                    .Where( a => a.EndDateTime == null && a.PersonAlias.PersonId != attendance.PersonAlias.PersonId )
                    .Count();

                if ( ( locationCount + 1 ) >= locationFirmRoomThreshold.Value )
                {
                    return ActionOk( new EnRouteMovePersonResponseBag
                    {
                        WarningMessage = $"The {location} has reached its hard threshold capacity and cannot be used for check-in."
                    } );
                }
            }

            var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd(
                selectedOccurrenceDate,
                request.GroupId,
                request.LocationId,
                request.ScheduleId );

            attendance.OccurrenceId = newRoomsOccurrence.Id;
            RockContext.SaveChanges();

            return ActionOk( new EnRouteMovePersonResponseBag
            {
                IsSuccess = true
            } );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Builds the options bag for the block's initial render. Reads the
        /// filter state from the shared CheckinManager cookie and the
        /// block-level settings.
        /// </summary>
        private EnRouteOptionsBag BuildOptionsBag()
        {
            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;
            var scheduleIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteScheduleIdsFilter );
            var pickedGroupIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRoutePickedGroupIdsFilter );
            var includeChildGroupsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteIncludeChildGroupsFilter );

            var selectedScheduleIds = scheduleIdsFilter.SplitDelimitedValues().AsIntegerList();
            var selectedGroupIds = pickedGroupIdsFilter.SplitDelimitedValues().AsIntegerList();
            var isIncludeChildGroups = includeChildGroupsFilter.AsBoolean();

            var alwaysShowChildGroups = GetAttributeValue( AttributeKey.AlwaysShowChildGroups ).AsBoolean();
            if ( alwaysShowChildGroups )
            {
                isIncludeChildGroups = true;
            }

            // Build the available schedule list (active named schedules with
            // check-in start offset).
            var scheduleService = new ScheduleService( RockContext );
            var scheduleQry = scheduleService.Queryable()
                .Where( a => a.IsActive && a.CheckInStartOffsetMinutes != null && a.Name != null && a.Name != string.Empty );

            var scheduleList = scheduleQry.ToList()
                .OrderByOrderAndNextScheduledDateTime()
                .ToList();

            var availableSchedules = scheduleList.Select( s => new ListItemBag
            {
                Value = s.Id.ToString(),
                Text = s.Name.IsNotNullOrWhiteSpace() ? s.Name : s.FriendlyScheduleText
            } ).ToList();

            // Resolve the check-in area filter to determine which group types
            // the GroupPicker should be limited to.
            var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter(
                PageParameter( CheckinManagerHelper.PageParameterKey.Area ).AsGuidOrNull(),
                false,
                null );

            IEnumerable<CheckinAreaPath> checkinAreaPaths;
            if ( checkinAreaFilter != null )
            {
                checkinAreaPaths = new GroupTypeService( RockContext ).GetCheckinAreaDescendantsPath( checkinAreaFilter.Id );
            }
            else
            {
                checkinAreaPaths = new GroupTypeService( RockContext ).GetAllCheckinAreaPaths();
            }

            var checkinGroupTypeIds = checkinAreaPaths.Select( a => a.GroupTypeId ).Distinct().ToList();

            // Convert group type IDs to GUIDs for the GroupPicker.
            var checkinGroupTypeGuids = checkinGroupTypeIds
                .Select( id => GroupTypeCache.Get( id ) )
                .Where( gt => gt != null )
                .Select( gt => gt.Guid )
                .ToList();

            // Hydrate the previously-selected group IDs into ListItemBag
            // format for the GroupPicker.
            var selectedGroups = new List<ListItemBag>();
            if ( selectedGroupIds.Any() )
            {
                var groups = new GroupService( RockContext ).Queryable()
                    .Where( g => selectedGroupIds.Contains( g.Id ) )
                    .Select( g => new { g.Id, g.Guid, g.Name } )
                    .ToList();

                selectedGroups = groups.Select( g => new ListItemBag
                {
                    Value = g.Guid.ToString(),
                    Text = g.Name
                } ).ToList();
            }

            return new EnRouteOptionsBag
            {
                AvailableSchedules = availableSchedules,
                SelectedScheduleIds = selectedScheduleIds,
                SelectedGroups = selectedGroups,
                CheckinAreaGroupTypeGuids = checkinGroupTypeGuids,
                IsIncludeChildGroups = isIncludeChildGroups,
                IsAlwaysShowChildGroups = alwaysShowChildGroups,
                IsShowOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean()
            };
        }

        /// <summary>
        /// Saves the current filter selection to the shared CheckinManager
        /// cookie.
        /// </summary>
        /// <param name="selectedScheduleIds">The selected schedule ids.</param>
        /// <param name="selectedGroupIds">The selected group ids.</param>
        /// <param name="includeChildGroups">Whether child groups are included.</param>
        private void SaveFilterToCookie( List<int> selectedScheduleIds, List<int> selectedGroupIds, bool includeChildGroups )
        {
            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;

            customSettings.AddOrReplace( CustomSettingKey.EnRouteScheduleIdsFilter, selectedScheduleIds.AsDelimited( "," ) );
            customSettings.AddOrReplace( CustomSettingKey.EnRoutePickedGroupIdsFilter, selectedGroupIds.AsDelimited( "," ) );
            customSettings.AddOrReplace( CustomSettingKey.EnRouteIncludeChildGroupsFilter, includeChildGroups.ToTrueFalse() );

            CheckinManagerHelper.SaveCustomSettingsToCookie( customSettings );
        }

        /// <summary>
        /// Gets the campus from the block's context entity.
        /// </summary>
        private CampusCache GetCampusFromContext()
        {
            var campus = RequestContext.GetContextEntity<Campus>();
            return campus == null ? null : CampusCache.Get( campus.Id );
        }

        /// <summary>
        /// Queries the attendance records for today, applying all of the
        /// specified filter criteria, and maps the results to
        /// <see cref="EnRouteAttendeeBag"/> rows for the grid.
        /// </summary>
        private List<EnRouteAttendeeBag> GetAttendees( RockContext rockContext, List<int> selectedScheduleIds, List<int> selectedGroupIds, bool includeChildGroups, string searchText )
        {
            var startDateTime = RockDateTime.Today;
            var campusCache = GetCampusFromContext();
            var currentDateTime = campusCache != null ? campusCache.CurrentDateTime : RockDateTime.Now;
            var showOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            // Base attendance query: today, did attend, has required occurrence fields.
            var attendanceQuery = new AttendanceService( rockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && a.Occurrence.LocationId.HasValue );

            // Campus filter: limit to locations within the selected campus.
            if ( campusCache != null && campusCache.LocationId.HasValue )
            {
                var campusLocationIds = new LocationService( rockContext ).GetAllDescendentIds( campusCache.LocationId.Value ).ToList();
                attendanceQuery = attendanceQuery.Where( a => campusLocationIds.Contains( a.Occurrence.LocationId.Value ) );
            }

            // Schedule filter.
            if ( selectedScheduleIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            // Group filter (with optional child groups).
            if ( selectedGroupIds.Any() )
            {
                var effectiveGroupIds = new List<int>( selectedGroupIds );

                if ( includeChildGroups )
                {
                    var groupService = new GroupService( rockContext );
                    foreach ( var groupId in selectedGroupIds )
                    {
                        var childGroupIds = groupService.GetAllDescendentGroupIds( groupId, false );
                        effectiveGroupIds.AddRange( childGroupIds );
                    }
                }

                attendanceQuery = attendanceQuery.Where( a => effectiveGroupIds.Contains( a.Occurrence.GroupId.Value ) );
            }
            else
            {
                // When no groups are selected, limit by the check-in area filter.
                var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter(
                    PageParameter( CheckinManagerHelper.PageParameterKey.Area ).AsGuidOrNull(),
                    false,
                    null );

                if ( checkinAreaFilter != null )
                {
                    var checkinAreaGroupTypeIds = new GroupTypeService( rockContext )
                        .GetCheckinAreaDescendants( checkinAreaFilter.Id )
                        .Select( a => a.Id )
                        .ToList();

                    var areaGroupIds = new GroupService( rockContext ).Queryable()
                        .Where( a => checkinAreaGroupTypeIds.Contains( a.GroupTypeId ) )
                        .Select( a => a.Id )
                        .ToList();

                    attendanceQuery = attendanceQuery.Where( a => areaGroupIds.Contains( a.Occurrence.GroupId.Value ) );
                }
            }

            // Roster status filter (Checked-in, Present, or Checked-out).
            var rosterStatusFilter = GetAttributeValue( AttributeKey.FilterBy ).ConvertToEnumOrNull<RosterStatusFilter>() ?? RosterStatusFilter.CheckedIn;
            attendanceQuery = CheckinManagerHelper.FilterByRosterStatusFilter( attendanceQuery, rosterStatusFilter );

            // Materialize the attendance list and apply active-checkin filtering.
            var attendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();
            attendanceList = CheckinManagerHelper.FilterByActiveCheckins( currentDateTime, attendanceList );
            attendanceList = attendanceList.Where( a => a.Person != null ).ToList();

            // Name search filter.
            if ( searchText.IsNotNullOrWhiteSpace() && searchText.Length > 2 )
            {
                // Constrain the name search to only the people already in the
                // en-route list so the query does not scan/return every person
                // in the database that matches the search text (e.g. thousands
                // of "John" records). The en-route list is small, so this is a
                // short IN clause.
                var attendeePersonIds = attendanceList.Select( a => a.PersonId ).Distinct().ToList();

                bool reversed;
                var personIds = new PersonService( rockContext )
                    .GetByFullName( searchText, false, false, true, out reversed )
                    .Where( p => attendeePersonIds.Contains( p.Id ) )
                    .AsNoTracking()
                    .Select( a => a.Id )
                    .ToList();

                attendanceList = attendanceList.Where( a => personIds.Contains( a.PersonId ) ).ToList();
            }

            // Aggregate into RosterAttendee objects (groups multiple attendances
            // for the same person into one row).
            var attendees = RosterAttendee.GetFromAttendanceList( attendanceList );

            // Sort by name, then by PersonGuid for consistency when names match.
            var sorted = attendees
                .OrderBy( a => a.NickName )
                .ThenBy( a => a.LastName )
                .ThenBy( a => a.PersonGuid )
                .ToList();

            // Map to bag objects for the grid.
            return sorted.Select( a => MapAttendeeToBag( a, showOnlyParentGroup ) ).ToList();
        }

        /// <summary>
        /// Maps a <see cref="RosterAttendee"/> to an
        /// <see cref="EnRouteAttendeeBag"/> for the grid.
        /// </summary>
        private EnRouteAttendeeBag MapAttendeeToBag( RosterAttendee attendee, bool showOnlyParentGroup )
        {
            string groupName;
            string groupPath;

            if ( showOnlyParentGroup )
            {
                groupName = attendee.ParentGroupName;
                groupPath = attendee.ParentGroupGroupTypePath;
            }
            else
            {
                groupName = attendee.GroupName;
                groupPath = attendee.GroupTypePath;
            }

            return new EnRouteAttendeeBag
            {
                PersonGuid = attendee.PersonGuid,
                AttendanceIds = attendee.AttendanceIds.ToList(),
                PhotoImageTag = attendee.Person != null
                    ? Person.GetPersonPhotoImageTag( attendee.Person, 50, 50, className: "avatar avatar-lg" )
                    : null,
                NickName = attendee.NickName,
                LastName = attendee.LastName,
                FullName = attendee.FullName,
                ParentNames = attendee.ParentNames,
                GroupName = groupName,
                GroupPath = groupPath,
                ServiceTimes = attendee.ServiceTimes,
                RoomName = attendee.RoomName
            };
        }

        /// <summary>
        /// Builds the cascading Schedule / Location / Group dropdown options
        /// for the Move Person modal from the given attendance record.
        /// </summary>
        private EnRouteMovePersonOptionsBag BuildMovePersonOptions( Attendance attendance )
        {
            var groupLocationSchedules = CheckinManagerHelper.GetGroupLocationSchedulesForPersonMove( RockContext, attendance );

            var scheduleListItems = new List<ListItemBag>();
            var locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
            var groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();

            if ( groupLocationSchedules?.Any() == true )
            {
                CheckinManagerHelper.GroupAndSortMovePersonOptions(
                    groupLocationSchedules,
                    out scheduleListItems,
                    out locationListItemsBySchedule,
                    out groupListItemsByScheduleAndLocation,
                    GroupListItemKeyDelimiter );
            }

            return new EnRouteMovePersonOptionsBag
            {
                Schedules = scheduleListItems,
                LocationsBySchedule = locationListItemsBySchedule,
                GroupsByScheduleAndLocation = groupListItemsByScheduleAndLocation,
                GroupListItemKeyDelimiter = GroupListItemKeyDelimiter,
                CurrentScheduleId = attendance.Occurrence?.ScheduleId,
                CurrentLocationId = attendance.Occurrence?.LocationId,
                CurrentGroupId = attendance.Occurrence?.GroupId
            };
        }

        /// <summary>
        /// Builds the grid builder used for the block's grid definition and
        /// data serialization.
        /// </summary>
        private GridBuilder<EnRouteAttendeeBag> GetGridBuilder()
        {
            return new GridBuilder<EnRouteAttendeeBag>()
                .WithBlock( this )
                .AddTextField( "personGuid", a => a.PersonGuid.ToString() )
                .AddField( "attendanceIds", a => a.AttendanceIds )
                .AddTextField( "photoImageTag", a => a.PhotoImageTag )
                .AddTextField( "nickName", a => a.NickName )
                .AddTextField( "lastName", a => a.LastName )
                .AddTextField( "fullName", a => a.FullName )
                .AddTextField( "parentNames", a => a.ParentNames )
                .AddTextField( "groupName", a => a.GroupName )
                .AddTextField( "groupPath", a => a.GroupPath )
                .AddTextField( "serviceTimes", a => a.ServiceTimes )
                .AddTextField( "roomName", a => a.RoomName );
        }

        #endregion Private Methods
    }
}
