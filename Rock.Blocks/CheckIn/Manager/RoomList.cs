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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Manager.RoomList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Shows all locations of type "Room" for the campus (context) and
    /// selected schedules, together with the current Checked-in / Present /
    /// Checked-out counts for each room. Clicking a row navigates to the
    /// configured Roster page for that Location.
    /// </summary>

    [DisplayName( "Room List" )]
    [Category( "Check-in > Manager" )]
    [Description( "Shows all locations of the type room for the campus (context) and selected schedules." )]
    [IconCssClass( "ti ti-building" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [Rock.Web.UI.ContextAware( typeof( Campus ) )]

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
        Description = "When enabled, only the actual parent group for each check-in group-location will be shown and groups under the same parent group in the same location will be combined into one row.",
        Key = AttributeKey.ShowOnlyParentGroup,
        DefaultBooleanValue = false,
        Order = 5 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "81BF7C1B-880A-44E7-ADD9-8ED89261890F" )]
    [Rock.SystemGuid.BlockTypeGuid( "2DEA7808-9AC1-4913-BF58-1CDC7922C901" )]
    public class RoomList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowAllAreas = CheckinManagerHelper.BlockAttributeKey.ShowAllAreas;
            public const string AreaSelectPage = "AreaSelectPage";

            /// <summary>
            /// The 'Check-in Configuration' Guid (a <see cref="Rock.Model.GroupType" /> Guid) that limits the rooms this block manages.
            /// </summary>
            public const string CheckInAreaGuid = CheckinManagerHelper.BlockAttributeKey.CheckInAreaGuid;

            public const string RosterPage = "RosterPage";

            /// <summary>
            /// When enabled, only the actual parent group for the check-in group-location is shown
            /// in the Room Name grid column, and groups under the same parent group in the same
            /// location are combined into a single row.
            /// </summary>
            public const string ShowOnlyParentGroup = "ShowOnlyParentGroup";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// The 'Check-in Configuration' Guid (a <see cref="Rock.Model.GroupType" /> Guid), which overrides
            /// the block-level Area selection when supplied.
            /// </summary>
            public const string Area = CheckinManagerHelper.PageParameterKey.Area;

            /// <summary>
            /// When present, only direct (first-level) child locations of the specified location are shown.
            /// The panel title becomes "{Parent Location Name} Child Locations". Takes precedence over the
            /// campus context locations.
            /// </summary>
            public const string ParentLocationId = "ParentLocationId";

            /// <summary>
            /// When present, the block is scoped to a single location: the Room column is hidden and the
            /// panel title is set to the location's name. Takes precedence over both the campus context
            /// locations and <see cref="ParentLocationId"/>.
            /// </summary>
            public const string LocationId = "LocationId";
        }

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            var showOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            var box = new ListBlockBox<RoomListOptionsBag>
            {
                Options = BuildOptionsBag( GetInitiallySelectedScheduleIds() ),
                GridDefinition = GetGridBuilder( showOnlyParentGroup ).BuildDefinition(),
                NavigationUrls = new Dictionary<string, string>
                {
                    ["RosterPage"] = this.GetLinkedPageUrl( AttributeKey.RosterPage, "LocationId", "((Key))" )
                }
            };

            return box;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Persists the current schedule filter selection to the shared
        /// CheckinManager cookie and returns fresh grid data + refreshed
        /// column-visibility flags and panel title for the current request.
        /// </summary>
        /// <param name="selectedScheduleGuids">
        /// The schedule Guids the individual has selected in the filter modal.
        /// The Guids are resolved to integer identifiers server-side so the
        /// shared CheckinManager cookie schema can stay int-based. May be null
        /// or empty when the filter is cleared.
        /// </param>
        [BlockAction]
        public BlockActionResult GetGridData( List<Guid> selectedScheduleGuids )
        {
            var effectiveScheduleGuids = ( selectedScheduleGuids ?? new List<Guid>() )
                .Where( g => g != Guid.Empty )
                .Distinct()
                .ToList();

            // Resolve the picker's Guids to integer identifiers in a single
            // query so the cookie schema (int[]) and the downstream WHERE-IN
            // clauses stay Id-based.
            var effectiveScheduleIds = effectiveScheduleGuids.Any()
                ? new ScheduleService( RockContext ).Queryable()
                    .Where( s => effectiveScheduleGuids.Contains( s.Guid ) )
                    .Select( s => s.Id )
                    .ToArray()
                : new int[0];

            // Save the filter selection into the shared CheckinManager cookie
            // so it round-trips with the WebForms Room List block during A-B
            // testing and survives the individual's next visit.
            CheckinManagerHelper.SaveRoomListFilterToCookie( effectiveScheduleIds );

            var context = ResolveContext();

            if ( context.RedirectUrl.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( new RoomListGridDataBag
                {
                    RedirectUrl = context.RedirectUrl,
                    PanelTitle = context.PanelTitle,
                    PresentColumnHeader = "Present"
                } );
            }

            if ( context.WarningMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( new RoomListGridDataBag
                {
                    WarningMessage = context.WarningMessage,
                    PanelTitle = context.PanelTitle,
                    PresentColumnHeader = "Present"
                } );
            }

            var rows = BuildRoomRows( context, effectiveScheduleIds );
            var showOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();
            var gridData = GetGridBuilder( showOnlyParentGroup ).Build( rows );

            return ActionOk( new RoomListGridDataBag
            {
                GridData = gridData,
                PanelTitle = context.PanelTitle,
                ShowRoomColumn = context.ShowRoomColumn,
                ShowCheckedInCount = context.ShowCheckedInCount,
                ShowCheckedOutCount = context.ShowCheckedOutCount,
                PresentColumnHeader = context.PresentColumnHeader
            } );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Builds the options bag that seeds the block's first render. Reuses
        /// the shared <see cref="ResolveContext"/> helper so warning messages,
        /// redirect URLs, and column-visibility flags all follow the same
        /// resolution rules as <see cref="GetGridData"/>.
        /// </summary>
        private RoomListOptionsBag BuildOptionsBag( int[] initialSelectedScheduleIds )
        {
            var context = ResolveContext();

            return new RoomListOptionsBag
            {
                WarningMessage = context.WarningMessage,
                RedirectUrl = context.RedirectUrl,
                PanelTitle = context.PanelTitle,
                SelectedSchedules = GetSelectedScheduleBags( initialSelectedScheduleIds ),
                ShowRoomColumn = context.ShowRoomColumn,
                ShowCheckedInCount = context.ShowCheckedInCount,
                ShowCheckedOutCount = context.ShowCheckedOutCount,
                PresentColumnHeader = context.PresentColumnHeader,
                IsShowOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean()
            };
        }

        /// <summary>
        /// Reads the initially-selected schedule identifiers from the shared
        /// CheckinManager cookie.
        /// </summary>
        private int[] GetInitiallySelectedScheduleIds()
        {
            return CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie()?.RoomListScheduleIdsFilter ?? new int[0];
        }

        /// <summary>
        /// Resolves the cookie's stored integer schedule identifiers into
        /// <see cref="ListItemBag"/> values (Guid + display text) so the
        /// Schedule Picker can hydrate its initial selection. Missing or
        /// invalid schedule ids are dropped silently — a schedule that has
        /// been deleted since the cookie was written should not surface as an
        /// empty chip.
        /// </summary>
        private List<ListItemBag> GetSelectedScheduleBags( int[] scheduleIds )
        {
            if ( scheduleIds == null || scheduleIds.Length == 0 )
            {
                return new List<ListItemBag>();
            }

            var schedules = new ScheduleService( RockContext ).Queryable()
                .Where( s => scheduleIds.Contains( s.Id ) )
                .ToList();

            return schedules
                .Select( s => new ListItemBag
                {
                    Value = s.Guid.ToString(),
                    Text = s.Name.IsNotNullOrWhiteSpace() ? s.Name : s.FriendlyScheduleText
                } )
                .ToList();
        }

        /// <summary>
        /// Resolves the campus, area, target locations, and column-visibility
        /// flags for the current request. Both <see cref="GetObsidianBlockInitialization"/>
        /// and <see cref="GetGridData"/> call through here so the two entry
        /// points cannot disagree about what should be shown.
        /// </summary>
        private RoomListContext ResolveContext()
        {
            var context = new RoomListContext
            {
                PanelTitle = "Room List",
                ShowRoomColumn = true,
                PresentColumnHeader = "Present"
            };

            var campus = GetCampusFromContext();

            if ( campus == null )
            {
                context.WarningMessage = "Please select a Campus.";
                return context;
            }

            if ( !campus.LocationId.HasValue )
            {
                context.WarningMessage = "This campus does not have any locations.";
                return context;
            }

            var showAllAreas = GetAttributeValue( AttributeKey.ShowAllAreas ).AsBoolean();
            var areaPageParameterGuid = PageParameter( PageParameterKey.Area ).AsGuidOrNull();
            var blockAttributeCheckinAreaGuid = GetAttributeValue( AttributeKey.CheckInAreaGuid ).AsGuidOrNull();

            var areaFilter = CheckinManagerHelper.GetCheckinAreaFilter( areaPageParameterGuid, showAllAreas, blockAttributeCheckinAreaGuid );

            if ( areaFilter == null && !showAllAreas )
            {
                // The Check-in Area cannot be determined and Show All Areas is
                // disabled. Match the WebForms behavior: redirect the person
                // to the Area Select Page if configured, otherwise surface a
                // warning that the block is not configured correctly.
                var areaSelectPageUrl = this.GetLinkedPageUrl( AttributeKey.AreaSelectPage );

                if ( areaSelectPageUrl.IsNotNullOrWhiteSpace() )
                {
                    context.RedirectUrl = areaSelectPageUrl;
                    return context;
                }

                context.WarningMessage = "The 'Area Select Page' Block Attribute must be defined.";
                return context;
            }

            context.Campus = campus;
            context.AreaFilter = areaFilter;

            var groupTypeService = new GroupTypeService( RockContext );
            IEnumerable<CheckinAreaPath> checkinAreaPaths = areaFilter != null
                ? groupTypeService.GetCheckinAreaDescendantsPath( areaFilter.Id )
                : groupTypeService.GetAllCheckinAreaPaths();

            context.CheckinAreaPaths = checkinAreaPaths.ToList();
            context.SelectedGroupTypeIds = context.CheckinAreaPaths.Select( a => a.GroupTypeId ).Distinct().ToArray();

            // Resolve the target locations. Precedence (matching WebForms):
            //   1. LocationId page parameter (single location; Room column hidden; panel title = location name).
            //   2. ParentLocationId page parameter (immediate children of that location; panel title = "{Name} Child Locations").
            //   3. All descendants of the campus's location.
            //
            // The LocationId and ParentLocationId parameters accept an Id,
            // IdKey, or Guid so the block can be linked to from an IdKey-only
            // page without breaking existing integer-Id URLs.
            var locationService = new LocationService( RockContext );
            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;

            var locationIdParameter = PageParameter( PageParameterKey.LocationId );
            var parentLocationIdParameter = PageParameter( PageParameterKey.ParentLocationId );

            if ( locationIdParameter.IsNotNullOrWhiteSpace() )
            {
                var location = locationService.Get( locationIdParameter, allowIntegerId );
                if ( location != null )
                {
                    context.LocationIds = new List<int> { location.Id };
                    context.PanelTitle = location.Name;
                    context.ShowRoomColumn = false;
                }
            }
            else if ( parentLocationIdParameter.IsNotNullOrWhiteSpace() )
            {
                var parentLocation = locationService.Get( parentLocationIdParameter, allowIntegerId );
                if ( parentLocation != null )
                {
                    context.LocationIds = locationService.Queryable()
                        .Where( a => a.ParentLocationId == parentLocation.Id )
                        .Select( a => a.Id )
                        .ToList();

                    context.PanelTitle = $"{parentLocation.Name} Child Locations";
                }
            }

            if ( context.LocationIds == null )
            {
                context.LocationIds = locationService.GetAllDescendentIds( campus.LocationId.Value ).ToList();
                context.LocationIds.Add( campus.LocationId.Value, true );
            }

            // Compute column visibility from the resolved group types in a
            // single scan (the WebForms block does two scans of the same list).
            var enablePresenceIds = new HashSet<int>();
            var allowCheckoutIds = new HashSet<int>();

            foreach ( var groupTypeId in context.SelectedGroupTypeIds )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                if ( groupType == null )
                {
                    continue;
                }

                if ( groupType.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean() )
                {
                    enablePresenceIds.Add( groupTypeId );
                }

                if ( groupType.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean() )
                {
                    allowCheckoutIds.Add( groupTypeId );
                }
            }

            context.EnablePresenceGroupTypeIds = enablePresenceIds;
            context.AllowCheckoutManagerGroupTypeIds = allowCheckoutIds;

            context.ShowCheckedInCount = enablePresenceIds.Any();
            context.ShowCheckedOutCount = allowCheckoutIds.Any();

            // When Enable Presence is off for every resolved group type, an
            // attendee is automatically marked present at check-in — so the
            // "Present" column is really counting Checked-In records and the
            // header should read that way.
            context.PresentColumnHeader = enablePresenceIds.Any() ? "Present" : "Checked-In";

            return context;
        }

        /// <summary>
        /// Gets the campus supplied by the block's context, resolved through
        /// the cache so callers hold a lightweight <see cref="CampusCache" />
        /// instead of a materialized entity.
        /// </summary>
        private CampusCache GetCampusFromContext()
        {
            var campus = RequestContext.GetContextEntity<Campus>();
            return campus == null ? null : CampusCache.Get( campus.Id );
        }

        /// <summary>
        /// Runs the compound GroupLocation + Attendance query and produces the
        /// list of <see cref="RoomInfoBag"/> rows for the grid. Assumes the
        /// context has already resolved a valid campus, area, and location
        /// set.
        /// </summary>
        private List<RoomInfoBag> BuildRoomRows( RoomListContext context, int[] selectedScheduleIds )
        {
            var showOnlyParentGroup = GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            // Room-eligible GroupLocation query, scoped by active/non-archived
            // group, the resolved GroupType ids and Location ids, and (if a
            // schedule filter is applied) the selected schedules.
            var groupLocationQuery = new GroupLocationService( RockContext ).Queryable()
                .Where( gl => context.SelectedGroupTypeIds.Contains( gl.Group.GroupTypeId )
                    && gl.Group.IsActive
                    && !gl.Group.IsArchived
                    && context.LocationIds.Contains( gl.LocationId ) );

            if ( selectedScheduleIds.Any() )
            {
                groupLocationQuery = groupLocationQuery.Where( gl => gl.Schedules.Any( s =>
                    s.IsActive
                    && s.CheckInStartOffsetMinutes.HasValue
                    && selectedScheduleIds.Contains( s.Id ) ) );
            }
            else
            {
                groupLocationQuery = groupLocationQuery.Where( gl => gl.Schedules.Any( s =>
                    s.IsActive
                    && s.CheckInStartOffsetMinutes.HasValue ) );
            }

            var groupLocationList = groupLocationQuery.Select( a => new GroupLocationInfo
            {
                LocationId = a.LocationId,
                LocationName = a.Location.Name,
                ParentGroupId = a.Group.ParentGroupId,
                ParentGroupName = a.Group.ParentGroup.Name,
                GroupId = a.Group.Id,
                GroupName = a.Group.Name,
                GroupTypeId = a.Group.GroupTypeId
            } ).ToList();

            if ( !groupLocationList.Any() )
            {
                return new List<RoomInfoBag>();
            }

            // Attendance query for today, up to the campus's current date-time.
            var startDateTime = RockDateTime.Today;
            var currentDateTime = context.Campus.CurrentDateTime;

            var groupLocationLocationIds = groupLocationList.Select( a => a.LocationId ).Distinct().ToList();
            var groupLocationGroupIds = groupLocationList.Select( a => a.GroupId ).Distinct().ToList();

            var attendanceQuery = new AttendanceService( RockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && groupLocationLocationIds.Contains( a.Occurrence.LocationId.Value )
                && groupLocationGroupIds.Contains( a.Occurrence.GroupId.Value )
                && context.SelectedGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );

            if ( selectedScheduleIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var rosterAttendeeAttendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            // For any resolved GroupType that does NOT allow the manager to
            // check people out, drop attendees whose schedule is no longer
            // active for check-out. This keeps stale rows out of the counts
            // when the volunteers cannot manually mark people checked-out.
            var scheduleIds = rosterAttendeeAttendanceList.Select( a => a.ScheduleId.Value ).Distinct().ToList();
            var scheduleList = new ScheduleService( RockContext ).GetByIds( scheduleIds ).ToList();
            var scheduleIdsWasScheduleOrCheckInActiveForCheckOut = new HashSet<int>(
                scheduleList.Where( a => a.WasScheduleOrCheckInActiveForCheckOut( currentDateTime ) ).Select( a => a.Id ) );

            rosterAttendeeAttendanceList = rosterAttendeeAttendanceList.Where( a =>
            {
                var allowCheckout = context.AllowCheckoutManagerGroupTypeIds.Contains( a.GroupTypeId );
                if ( !allowCheckout )
                {
                    return scheduleIdsWasScheduleOrCheckInActiveForCheckOut.Contains( a.ScheduleId.Value );
                }
                return true;
            } ).ToList();

            var attendancesByLocationAndGroupId = rosterAttendeeAttendanceList
                .GroupBy( a => a.LocationId.Value )
                .ToDictionary(
                    k => k.Key,
                    v => v.GroupBy( x => x.GroupId.Value ).ToDictionary( x => x.Key, xx => xx.ToList() ) );

            var checkinAreaPathsByGroupTypeId = context.CheckinAreaPaths.ToDictionary( k => k.GroupTypeId, v => v );

            // Aggregate into the two RoomInfo shapes.
            if ( showOnlyParentGroup )
            {
                return BuildRoomInfoByParentGroups( groupLocationList, attendancesByLocationAndGroupId );
            }

            return BuildRoomInfoByGroup( groupLocationList, attendancesByLocationAndGroupId, checkinAreaPathsByGroupTypeId );
        }

        /// <summary>
        /// Aggregates group-locations into one row per Location+Group, with
        /// the group name + group-type path shown side by side.
        /// </summary>
        private static List<RoomInfoBag> BuildRoomInfoByGroup(
            List<GroupLocationInfo> groupLocationList,
            Dictionary<int, Dictionary<int, List<RosterAttendeeAttendance>>> attendancesByLocationAndGroupId,
            Dictionary<int, CheckinAreaPath> checkinAreaPathsByGroupTypeId )
        {
            var rows = new List<RoomInfoBag>();

            foreach ( var groupLocation in groupLocationList )
            {
                var attendees = GetRosterAttendees( attendancesByLocationAndGroupId, groupLocation.LocationId, groupLocation.GroupId );
                var counts = GetRoomCounts( attendees );

                rows.Add( new RoomInfoBag
                {
                    RowKey = $"{groupLocation.LocationId}|{groupLocation.GroupId}",
                    LocationIdKey = groupLocation.LocationId.AsIdKey(),
                    LocationName = groupLocation.LocationName,
                    GroupName = groupLocation.GroupName,
                    GroupTypePath = checkinAreaPathsByGroupTypeId.GetValueOrNull( groupLocation.GroupTypeId )?.Path,
                    CheckedInCount = counts.CheckedInCount,
                    PresentCount = counts.PresentCount,
                    CheckedOutCount = counts.CheckedOutCount
                } );
            }

            return rows
                .OrderBy( r => r.LocationName )
                .ThenBy( r => r.GroupName )
                .ToList();
        }

        /// <summary>
        /// Aggregates group-locations into one row per Location, joining all
        /// parent group names (alphabetically) into a single Group column
        /// value. Counts across every group under that location are merged.
        /// </summary>
        private static List<RoomInfoBag> BuildRoomInfoByParentGroups(
            List<GroupLocationInfo> groupLocationList,
            Dictionary<int, Dictionary<int, List<RosterAttendeeAttendance>>> attendancesByLocationAndGroupId )
        {
            var byLocation = new Dictionary<int, RoomInfoByLocationAccumulator>();

            foreach ( var groupLocation in groupLocationList )
            {
                var attendees = GetRosterAttendees( attendancesByLocationAndGroupId, groupLocation.LocationId, groupLocation.GroupId );

                if ( !byLocation.TryGetValue( groupLocation.LocationId, out var accumulator ) )
                {
                    accumulator = new RoomInfoByLocationAccumulator
                    {
                        LocationId = groupLocation.LocationId,
                        LocationName = groupLocation.LocationName
                    };
                    byLocation[groupLocation.LocationId] = accumulator;
                }

                accumulator.CheckedInAttendees.AddRange( attendees.Where( a => a.Status == RosterAttendeeStatus.CheckedIn ) );
                accumulator.PresentAttendees.AddRange( attendees.Where( a => a.Status == RosterAttendeeStatus.Present ) );
                accumulator.CheckedOutAttendees.AddRange( attendees.Where( a => a.Status == RosterAttendeeStatus.CheckedOut ) );

                if ( groupLocation.ParentGroupId.HasValue && !accumulator.ParentGroupNames.ContainsKey( groupLocation.ParentGroupId.Value ) )
                {
                    accumulator.ParentGroupNames[groupLocation.ParentGroupId.Value] = groupLocation.ParentGroupName;
                }
            }

            return byLocation.Values
                .OrderBy( a => a.LocationName )
                .Select( a => new RoomInfoBag
                {
                    RowKey = a.LocationId.ToString(),
                    LocationIdKey = a.LocationId.AsIdKey(),
                    LocationName = a.LocationName,
                    GroupName = string.Join( ", ", a.ParentGroupNames.Values.OrderBy( n => n ) ),
                    GroupTypePath = null,
                    CheckedInCount = a.CheckedInAttendees.DistinctBy( x => x.PersonId ).Count(),
                    PresentCount = a.PresentAttendees.DistinctBy( x => x.PersonId ).Count(),
                    CheckedOutCount = a.CheckedOutAttendees.DistinctBy( x => x.PersonId ).Count()
                } )
                .ToList();
        }

        /// <summary>
        /// Materializes <see cref="RosterAttendee"/> instances for the
        /// attendance records that belong to the given location + group. Empty
        /// list when the location or the group has no attendance.
        /// </summary>
        private static List<RosterAttendee> GetRosterAttendees(
            Dictionary<int, Dictionary<int, List<RosterAttendeeAttendance>>> attendancesByLocationAndGroupId,
            int locationId,
            int groupId )
        {
            if ( !attendancesByLocationAndGroupId.TryGetValue( locationId, out var byGroup ) )
            {
                return new List<RosterAttendee>();
            }

            if ( !byGroup.TryGetValue( groupId, out var attendances ) )
            {
                return new List<RosterAttendee>();
            }

            return RosterAttendee.GetFromAttendanceList( attendances ).ToList();
        }

        /// <summary>
        /// Computes deduplicated (by PersonId) counts across the three roster
        /// statuses for the given attendee list.
        /// </summary>
        private static RoomCounts GetRoomCounts( List<RosterAttendee> attendees )
        {
            return new RoomCounts
            {
                CheckedInCount = attendees.Where( a => a.Status == RosterAttendeeStatus.CheckedIn ).DistinctBy( a => a.PersonId ).Count(),
                PresentCount = attendees.Where( a => a.Status == RosterAttendeeStatus.Present ).DistinctBy( a => a.PersonId ).Count(),
                CheckedOutCount = attendees.Where( a => a.Status == RosterAttendeeStatus.CheckedOut ).DistinctBy( a => a.PersonId ).Count()
            };
        }

        /// <summary>
        /// Builds the grid builder used for the block's grid. The Room and
        /// Group columns are always emitted; the block controls visibility on
        /// the frontend through <see cref="RoomListGridDataBag" /> flags so
        /// the response can hide columns without changing the grid schema.
        /// </summary>
        private GridBuilder<RoomInfoBag> GetGridBuilder( bool isShowOnlyParentGroup )
        {
            return new GridBuilder<RoomInfoBag>()
                .WithBlock( this )
                .AddTextField( "rowKey", a => a.RowKey )
                .AddTextField( "locationIdKey", a => a.LocationIdKey )
                .AddTextField( "locationName", a => a.LocationName )
                .AddTextField( "groupName", a => a.GroupName )
                .AddTextField( "groupTypePath", a => a.GroupTypePath )
                .AddField( "checkedInCount", a => a.CheckedInCount )
                .AddField( "presentCount", a => a.PresentCount )
                .AddField( "checkedOutCount", a => a.CheckedOutCount );
        }

        #endregion Private Methods

        #region Private Types

        /// <summary>
        /// Aggregated per-request context: campus, area, target locations,
        /// column-visibility flags, and any warning / redirect state.
        /// </summary>
        private class RoomListContext
        {
            public CampusCache Campus { get; set; }

            public GroupTypeCache AreaFilter { get; set; }

            public int[] SelectedGroupTypeIds { get; set; } = new int[0];

            public List<CheckinAreaPath> CheckinAreaPaths { get; set; } = new List<CheckinAreaPath>();

            public List<int> LocationIds { get; set; }

            public HashSet<int> AllowCheckoutManagerGroupTypeIds { get; set; } = new HashSet<int>();

            public HashSet<int> EnablePresenceGroupTypeIds { get; set; } = new HashSet<int>();

            public string PanelTitle { get; set; }

            public string WarningMessage { get; set; }

            public string RedirectUrl { get; set; }

            public bool ShowRoomColumn { get; set; }

            public bool ShowCheckedInCount { get; set; }

            public bool ShowCheckedOutCount { get; set; }

            public string PresentColumnHeader { get; set; }
        }

        private class GroupLocationInfo
        {
            public int LocationId { get; set; }

            public string LocationName { get; set; }

            public int? ParentGroupId { get; set; }

            public string ParentGroupName { get; set; }

            public int GroupId { get; set; }

            public string GroupName { get; set; }

            public int GroupTypeId { get; set; }
        }

        private class RoomCounts
        {
            public int CheckedInCount { get; set; }

            public int PresentCount { get; set; }

            public int CheckedOutCount { get; set; }
        }

        private class RoomInfoByLocationAccumulator
        {
            public int LocationId { get; set; }

            public string LocationName { get; set; }

            public Dictionary<int, string> ParentGroupNames { get; } = new Dictionary<int, string>();

            public List<RosterAttendee> CheckedInAttendees { get; } = new List<RosterAttendee>();

            public List<RosterAttendee> PresentAttendees { get; } = new List<RosterAttendee>();

            public List<RosterAttendee> CheckedOutAttendees { get; } = new List<RosterAttendee>();
        }

        #endregion Private Types
    }
}
