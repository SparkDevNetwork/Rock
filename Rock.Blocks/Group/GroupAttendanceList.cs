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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupAttendanceList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Lists all the scheduled occurrences for a given group.
    /// </summary>

    [DisplayName( "Group Attendance List" )]
    [Category( "Groups" )]
    [Description( "Lists all the scheduled occurrences for a given group." )]
    [IconCssClass( "ti ti-square-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the attendance detail.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.DetailPage )]

    [BooleanField( "Allow Add",
        Description = "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.AllowAdd )]

    [BooleanField( "Allow Campus Filter",
        Description = "Should block add an option to allow filtering attendance counts and percentage by campus?",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.AllowCampusFilter )]

    [BooleanField( "Display Notes",
        Description = "Should the Notes column be displayed?",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.DisplayNotes )]

    [BooleanField( "Display Attendance Type",
        Description = "Should the Attendance Type column be displayed?",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.DisplayAttendanceType )]

    [CustomizedGrid]
    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "CC29106F-BD80-4F54-A2D7-1B256412A84A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "0F68A497-F210-4189-876F-6EC5E16455B0" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.GROUP_ATTENDANCE_LIST )]
    public class GroupAttendanceList : RockListBlockType<GroupAttendanceList.AttendanceOccurrenceRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string AllowAdd = "AllowAdd";
            public const string AllowCampusFilter = "AllowCampusFilter";
            public const string DisplayNotes = "DisplayNotes";
            public const string DisplayAttendanceType = "DisplayAttendanceType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string ReturnUrl = "ReturnUrl";
            public const string OccurrenceId = "OccurrenceId";
            public const string Date = "Date";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterLocation = "filter-location";
            public const string FilterSchedule = "filter-schedule";
            public const string FilterCampus = "filter-campus";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The cached group, loaded via <see cref="GetGroup"/>.
        /// </summary>
        private Model.Group _group;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the filter sliding date range from person preferences, or <c>null</c> if none is saved.
        /// </summary>
        protected SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToGroup( PreferenceKey.FilterDateRange ) )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the filter location value from person preferences.
        /// </summary>
        protected string FilterLocation => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToGroup( PreferenceKey.FilterLocation ) );

        /// <summary>
        /// Gets the filter schedule identifier from person preferences.
        /// </summary>
        protected int? FilterScheduleId => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToGroup( PreferenceKey.FilterSchedule ) )
            .AsIntegerOrNull();

        /// <summary>
        /// Gets the filter campus identifier from person preferences. The campus
        /// preference is intentionally stored globally (not group-scoped) to match
        /// the WebForms behavior, which persists the user's campus selection across
        /// every group they view.
        /// </summary>
        protected int? FilterCampusId => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus )
            .AsIntegerOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GroupAttendanceListOptionsBag>();
            var group = GetGroup();
            var isAuthorized = group != null && group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );

            box.Options = GetBoxOptions( group, isAuthorized );

            if ( !isAuthorized )
            {
                return box;
            }

            var builder = GetGridBuilder();
            var canEdit = GetCanEdit( group );

            box.IsAddEnabled = canEdit && GetAttributeValue( AttributeKey.AllowAdd ).AsBoolean();
            box.IsDeleteEnabled = canEdit;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the options bag for the block.
        /// </summary>
        /// <param name="group">The group entity (may be <c>null</c> or unauthorized).</param>
        /// <param name="isAuthorized">
        /// Whether the current person is authorized to view <paramref name="group"/>.
        /// When <c>false</c>, the bag is populated with the authorization flag only.
        /// </param>
        /// <returns>The options bag populated with filter data and display settings.</returns>
        private GroupAttendanceListOptionsBag GetBoxOptions( Model.Group group, bool isAuthorized )
        {
            if ( !isAuthorized )
            {
                return new GroupAttendanceListOptionsBag
                {
                    IsAuthorized = false
                };
            }

            var isCampusFilterEnabled = GetAttributeValue( AttributeKey.AllowCampusFilter ).AsBoolean();
            var locationItems = GetLocationItems( group );
            var scheduleItems = GetScheduleItems( group );

            return new GroupAttendanceListOptionsBag
            {
                IsAuthorized = true,
                GroupName = group.Name,
                GroupIdKey = group.IdKey,
                IsNotesColumnVisible = GetAttributeValue( AttributeKey.DisplayNotes ).AsBoolean(),
                IsAttendanceTypeColumnVisible = GetAttributeValue( AttributeKey.DisplayAttendanceType ).AsBoolean(),
                IsCampusFilterEnabled = isCampusFilterEnabled,
                IsLocationColumnVisible = locationItems.Count > 0,
                IsScheduleColumnVisible = scheduleItems.Count > 0,
                LocationItems = locationItems,
                ScheduleItems = scheduleItems,
                CampusItems = isCampusFilterEnabled ? CampusCache.All().ToListItemBagList() : new List<ListItemBag>()
            };
        }

        /// <summary>
        /// Gets the location items for the filter dropdown, including parent locations.
        /// </summary>
        /// <param name="group">The group entity.</param>
        /// <returns>A list of location items sorted by path.</returns>
        private List<ListItemBag> GetLocationItems( Model.Group group )
        {
            var items = new List<ListItemBag>();
            var locationService = new LocationService( RockContext );
            var addedKeys = new HashSet<string>();

            var groupLocations = group.GroupLocations
                .Where( l => l.Location != null
                    && l.Location.Name != null
                    && l.Location.Name != string.Empty )
                .ToList();

            foreach ( var location in groupLocations.Select( l => l.Location ) )
            {
                var key = location.Id.ToString();
                if ( !addedKeys.Contains( key ) )
                {
                    items.Add( new ListItemBag { Value = key, Text = locationService.GetPath( location.Id ) } );
                    addedKeys.Add( key );
                }

                // Walk up the parent chain to add ancestor locations as filter options.
                var parentLocation = location.ParentLocation;
                while ( parentLocation != null )
                {
                    var parentKey = $"P{parentLocation.Id}";
                    if ( !addedKeys.Contains( parentKey ) )
                    {
                        items.Add( new ListItemBag { Value = parentKey, Text = locationService.GetPath( parentLocation.Id ) } );
                        addedKeys.Add( parentKey );
                    }

                    parentLocation = parentLocation.ParentLocation;
                }
            }

            return items.OrderBy( i => i.Text ).ToList();
        }

        /// <summary>
        /// Gets the schedule items for the filter dropdown. Only includes schedules
        /// from group locations that have a named location, matching WebForms behavior.
        /// </summary>
        /// <param name="group">The group entity.</param>
        /// <returns>A list of schedule items.</returns>
        private List<ListItemBag> GetScheduleItems( Model.Group group )
        {
            var items = new List<ListItemBag>();
            var addedIds = new HashSet<int>();

            var schedules = group.GroupLocations
                .Where( l => l.Location != null
                    && l.Location.Name != null
                    && l.Location.Name != string.Empty )
                .SelectMany( l => l.Schedules )
                .OrderBy( s => s.Name )
                .ToList();

            foreach ( var schedule in schedules )
            {
                if ( !addedIds.Contains( schedule.Id ) )
                {
                    items.Add( new ListItemBag { Value = schedule.Id.ToString(), Text = schedule.Name } );
                    addedIds.Add( schedule.Id );
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the navigation URLs for the block.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var group = GetGroup();

            // Forward page parameters to the detail page, excluding per-row keys that
            // are appended client-side.
            var perRowKeys = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
            {
                PageParameterKey.OccurrenceId,
                PageParameterKey.Date,
                PageParameterKey.LocationId,
                PageParameterKey.ScheduleId
            };

            var detailPageParams = new Dictionary<string, string>();
            foreach ( var kvp in RequestContext.PageParameters )
            {
                if ( perRowKeys.Contains( kvp.Key ) )
                {
                    continue;
                }

                if ( kvp.Value.IsNotNullOrWhiteSpace() )
                {
                    detailPageParams[kvp.Key] = kvp.Value;
                }
            }

            detailPageParams[PageParameterKey.ReturnUrl] = this.GetCurrentPageUrl();
            detailPageParams[PageParameterKey.GroupId] = group?.IdKey;

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, detailPageParams )
            };
        }

        /// <summary>
        /// Determines if the current person can edit attendance for the group.
        /// </summary>
        /// <param name="group">The group entity.</param>
        /// <returns><c>true</c> if the person can edit; otherwise <c>false</c>.</returns>
        private bool GetCanEdit( Model.Group group )
        {
            if ( group == null )
            {
                return false;
            }

            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || group.IsAuthorized( Authorization.TAKE_ATTENDANCE, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<AttendanceOccurrenceRow> GetListQueryable( RockContext rockContext )
        {
            var group = GetGroup();

            if ( group == null || !group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return Enumerable.Empty<AttendanceOccurrenceRow>().AsQueryable();
            }

            // Default to the last 3 months if no saved preference. This matches the WebForms
            // block's default window and keeps the initial load small.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 3
            };

            var dateRange = FilterDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var fromDateTime = dateRange.Start;
            var toDateTime = dateRange.End;

            // Build location filter IDs from the preference value.
            var locationIds = new List<int>();
            var filterLocation = FilterLocation;
            if ( filterLocation.IsNotNullOrWhiteSpace() )
            {
                if ( filterLocation.StartsWith( "P" ) )
                {
                    var parentLocationId = filterLocation.Substring( 1 ).AsIntegerOrNull();
                    if ( parentLocationId.HasValue )
                    {
                        locationIds = new LocationService( rockContext )
                            .GetAllDescendents( parentLocationId.Value )
                            .Select( l => l.Id )
                            .ToList();
                    }
                }
                else
                {
                    var locationId = filterLocation.AsIntegerOrNull();
                    if ( locationId.HasValue )
                    {
                        locationIds.Add( locationId.Value );
                    }
                }
            }

            // Build schedule filter IDs from the preference value.
            var scheduleIds = new List<int>();
            var filterScheduleId = FilterScheduleId;
            if ( filterScheduleId.HasValue && filterScheduleId.Value > 0 )
            {
                scheduleIds.Add( filterScheduleId.Value );
            }

            // Get occurrences including both persisted DB records and virtual ones from the schedule.
            var occurrences = new AttendanceOccurrenceService( rockContext )
                .GetGroupOccurrences( group, fromDateTime, toDateTime, locationIds, scheduleIds );

            // Apply campus filter only when the admin has enabled it AND a campus is selected.
            // Gating by the attribute value prevents a stale preference from continuing to filter
            // results after an admin turns off AllowCampusFilter.
            var isCampusFilterEnabled = GetAttributeValue( AttributeKey.AllowCampusFilter ).AsBoolean();
            var campusId = FilterCampusId;
            if ( isCampusFilterEnabled && campusId.HasValue && campusId.Value > 0 )
            {
                var locCampus = new Dictionary<int, int>();
                var locationService = new LocationService( rockContext );

                foreach ( var campus in CampusCache.All().Where( c => c.LocationId.HasValue ) )
                {
                    locCampus.TryAdd( campus.LocationId.Value, campus.Id );
                    foreach ( var locId in locationService.GetAllDescendentIds( campus.LocationId.Value ) )
                    {
                        locCampus.TryAdd( locId, campus.Id );
                    }
                }

                occurrences = occurrences
                    .Where( o =>
                    {
                        if ( o.LocationId.HasValue && locCampus.TryGetValue( o.LocationId.Value, out var occCampusId ) )
                        {
                            return occCampusId == campusId.Value;
                        }

                        // Include occurrences not associated with any campus.
                        return true;
                    } )
                    .ToList();
            }

            // Pre-fetch parent location paths for all distinct parent locations.
            var parentLocationPaths = new Dictionary<int, string>();
            var pathService = new LocationService( rockContext );
            foreach ( var parentLocationId in occurrences
                .Where( o => o.Location?.ParentLocationId.HasValue == true )
                .Select( o => o.Location.ParentLocationId.Value )
                .Distinct() )
            {
                parentLocationPaths[parentLocationId] = pathService.GetPath( parentLocationId );
            }

            // Map entity occurrences to grid row POCOs.
            var rows = occurrences.Select( o => new AttendanceOccurrenceRow
            {
                OccurrenceId = o.Id,
                OccurrenceDate = o.OccurrenceDate,
                LocationId = o.LocationId,
                LocationName = o.Location?.Name ?? string.Empty,
                ParentLocationId = o.Location?.ParentLocationId,
                ParentLocationPath = o.Location?.ParentLocationId.HasValue == true
                    ? parentLocationPaths.GetValueOrDefault( o.Location.ParentLocationId.Value, string.Empty )
                    : string.Empty,
                ScheduleId = o.ScheduleId,
                ScheduleName = GetScheduleName( o.Schedule ),
                StartTime = o.Schedule?.StartTimeOfDay ?? TimeSpan.Zero,
                AttendanceEntered = o.AttendanceEntered,
                DidNotOccur = o.DidNotOccur ?? false,
                DidAttendCount = o.DidAttendCount,
                AttendanceRate = o.AttendanceRate,
                Notes = o.Notes,
                AttendanceType = o.AttendanceTypeValueId.HasValue
                    ? DefinedValueCache.Get( o.AttendanceTypeValueId.Value )?.Value
                    : null
            } ).ToList();

            return rows.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<AttendanceOccurrenceRow> GetOrderedListQueryable( IQueryable<AttendanceOccurrenceRow> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( a => a.OccurrenceDate )
                .ThenByDescending( a => a.StartTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AttendanceOccurrenceRow> GetGridBuilder()
        {
            return new GridBuilder<AttendanceOccurrenceRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => GetRowKey( a ) )
                .AddDateTimeField( "occurrenceDate", a => a.OccurrenceDate )
                .AddTextField( "locationName", a => a.LocationName )
                .AddField( "parentLocationPath", a => a.ParentLocationPath )
                // Composite sort field. Rows without a parent location path are prefixed with a
                // leading space so they sort BEFORE rows with a parent — matching the WebForms
                // LINQ OrderBy( ParentLocationPath ).ThenBy( LocationName ) null-first behavior.
                .AddTextField( "locationSort", a => a.ParentLocationPath.IsNullOrWhiteSpace()
                    ? $" |{a.LocationName}"
                    : $"{a.ParentLocationPath}|{a.LocationName}" )
                .AddTextField( "scheduleName", a => a.ScheduleName )
                .AddField( "attendanceEntered", a => a.AttendanceEntered )
                .AddField( "didNotOccur", a => a.DidNotOccur )
                .AddField( "didAttendCount", a => a.DidAttendCount )
                .AddField( "attendanceRate", a => a.AttendanceRate )
                .AddTextField( "notes", a => a.Notes )
                .AddTextField( "notesPlain", a => a.Notes.StripHtml() )
                .AddTextField( "attendanceType", a => a.AttendanceType )
                .AddField( "locationId", a => a.LocationId )
                .AddField( "scheduleId", a => a.ScheduleId )
                .AddField( "isDeleteDisabled", a => a.OccurrenceId == 0 );
        }

        /// <summary>
        /// Gets a unique row key for the grid. Real occurrences use a hashed ID;
        /// virtual occurrences use a composite key.
        /// </summary>
        /// <param name="row">The occurrence row.</param>
        /// <returns>A unique key string.</returns>
        private string GetRowKey( AttendanceOccurrenceRow row )
        {
            if ( row.OccurrenceId > 0 )
            {
                return IdHasher.Instance.GetHash( row.OccurrenceId );
            }

            return $"0|{row.OccurrenceDate:yyyy-MM-dd}|{row.ScheduleId}|{row.LocationId}";
        }

        /// <summary>
        /// Gets the display name for a schedule.
        /// </summary>
        /// <param name="schedule">The schedule entity.</param>
        /// <returns>The schedule name, or a plain-text friendly representation when unnamed.</returns>
        private static string GetScheduleName( Schedule schedule )
        {
            if ( schedule == null )
            {
                return string.Empty;
            }

            // Use the condensed friendly text for unnamed schedules. The non-condensed variant
            // of Schedule.ToString() can return HTML (e.g., a <ul> of dates) which the grid's
            // TextColumn would render as literal tag text after HTML-encoding.
            return schedule.Name.IsNotNullOrWhiteSpace()
                ? schedule.Name
                : schedule.ToString( true );
        }

        /// <summary>
        /// Gets the group from the page parameter, caching the result for the request. The
        /// query eagerly includes <see cref="Model.Group.GroupLocations"/> with their
        /// <see cref="GroupLocation.Location"/> (+ immediate parent) and
        /// <see cref="GroupLocation.Schedules"/> so the filter-item builders and occurrence
        /// query don't lazy-load per row.
        /// </summary>
        /// <returns>The group entity or null if not found.</returns>
        private Model.Group GetGroup()
        {
            if ( _group != null )
            {
                return _group;
            }

            var groupKey = PageParameter( PageParameterKey.GroupId );
            if ( groupKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var queryable = new GroupService( RockContext ).Queryable().AsNoTracking()
                .Include( g => g.GroupLocations.Select( gl => gl.Location.ParentLocation ) )
                .Include( g => g.GroupLocations.Select( gl => gl.Schedules ) );

            if ( Guid.TryParse( groupKey, out var groupGuid ) )
            {
                _group = queryable.FirstOrDefault( g => g.Guid == groupGuid );
                return _group;
            }

            var allowIntegerIdentifier = !PageCache.Layout.Site.DisablePredictableIds;
            int? groupId = allowIntegerIdentifier && int.TryParse( groupKey, out var idInt )
                ? idInt
                : IdHasher.Instance.GetId( groupKey );

            if ( groupId.HasValue )
            {
                _group = queryable.FirstOrDefault( g => g.Id == groupId.Value );
            }

            return _group;
        }

        /// <summary>
        /// Makes the preference key unique to the current group.
        /// </summary>
        /// <param name="key">The base preference key.</param>
        /// <returns>A group-scoped preference key.</returns>
        private string MakeKeyUniqueToGroup( string key )
        {
            var group = GetGroup();
            if ( group != null )
            {
                return $"{group.IdKey}-{key}";
            }

            return key;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified attendance occurrence and its associated attendance records.
        /// </summary>
        /// <param name="key">The identifier of the occurrence to delete.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            // Virtual occurrences (composite key starting with "0|") cannot be deleted.
            if ( key.IsNullOrWhiteSpace() || key.StartsWith( "0|" ) )
            {
                return ActionBadRequest( "Virtual occurrences cannot be deleted." );
            }

            var group = GetGroup();
            if ( !GetCanEdit( group ) )
            {
                return ActionForbidden( "You are not authorized to delete attendance occurrences." );
            }

            var occurrenceService = new AttendanceOccurrenceService( RockContext );
            var occurrence = occurrenceService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( occurrence == null )
            {
                return ActionBadRequest( "Attendance occurrence not found." );
            }

            if ( !occurrenceService.CanDelete( occurrence, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var locationId = occurrence.LocationId;

            // Delete attendance records first since it is not a cascading delete.
            var attendanceService = new AttendanceService( RockContext );
            var attendees = attendanceService.Queryable().Where( a => a.OccurrenceId == occurrence.Id );
            RockContext.BulkDelete( attendees );

            occurrenceService.Delete( occurrence );
            RockContext.SaveChanges();

            if ( locationId.HasValue )
            {
                Rock.CheckIn.KioskLocationAttendance.Remove( locationId.Value );
            }

            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// A POCO representing a single attendance occurrence row in the grid.
        /// </summary>
        public class AttendanceOccurrenceRow
        {
            /// <summary>
            /// Gets or sets the attendance occurrence identifier. Zero for virtual occurrences
            /// that are generated from the group's schedule but not yet persisted.
            /// </summary>
            public int OccurrenceId { get; set; }

            /// <summary>
            /// Gets or sets the occurrence date.
            /// </summary>
            public DateTime OccurrenceDate { get; set; }

            /// <summary>
            /// Gets or sets the location identifier.
            /// </summary>
            public int? LocationId { get; set; }

            /// <summary>
            /// Gets or sets the location name.
            /// </summary>
            public string LocationName { get; set; }

            /// <summary>
            /// Gets or sets the parent location identifier.
            /// </summary>
            public int? ParentLocationId { get; set; }

            /// <summary>
            /// Gets or sets the parent location path.
            /// </summary>
            public string ParentLocationPath { get; set; }

            /// <summary>
            /// Gets or sets the schedule identifier.
            /// </summary>
            public int? ScheduleId { get; set; }

            /// <summary>
            /// Gets or sets the schedule name.
            /// </summary>
            public string ScheduleName { get; set; }

            /// <summary>
            /// Gets or sets the schedule start time of day, used for sorting.
            /// </summary>
            public TimeSpan StartTime { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether attendance was entered for this occurrence.
            /// </summary>
            public bool AttendanceEntered { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the group did not meet.
            /// </summary>
            public bool DidNotOccur { get; set; }

            /// <summary>
            /// Gets or sets the count of individuals who attended.
            /// </summary>
            public int DidAttendCount { get; set; }

            /// <summary>
            /// Gets or sets the attendance rate as a decimal value (0.0 to 1.0).
            /// </summary>
            public double AttendanceRate { get; set; }

            /// <summary>
            /// Gets or sets the occurrence notes.
            /// </summary>
            public string Notes { get; set; }

            /// <summary>
            /// Gets or sets the attendance type name resolved from the defined value.
            /// </summary>
            public string AttendanceType { get; set; }
        }

        #endregion
    }
}
