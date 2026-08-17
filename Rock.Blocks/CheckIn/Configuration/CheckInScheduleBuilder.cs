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
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInScheduleBuilder;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Helps to build schedules used for check-in.
    /// </summary>
    /// <seealso cref="RockBlockType" />

    [DisplayName( "Check-in Schedule Builder" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Helps to build schedules used for check-in." )]
    [IconCssClass( "ti ti-clipboard" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    [SystemGuid.EntityTypeGuid( "28B9DAB2-C58A-4459-9EE7-8D1895C09592" )]
    [SystemGuid.BlockTypeGuid( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" )]
    public class CheckInScheduleBuilder : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CheckInConfiguration = "CheckInConfiguration";
        }

        private static class PreferenceKey
        {
            /// <summary>
            /// The selected group type user preference key
            /// </summary>
            public const string SelectedGroupType = "selected-group-type";

            /// <summary>
            /// Scoped to the check-in configuration GroupType entity (not the block) and shared with other check-in
            /// configuration blocks, so the area slicer selection persists across all blocks for the same configuration.
            /// Value is the area's Guid; empty means "All Areas".
            /// </summary>
            public const string SelectedArea = "checkin-config-selected-area";

            /// <summary>
            /// The selected category user preference key
            /// </summary>
            public const string SelectedCategory = "selected-category";

            /// <summary>
            /// The selected parent location user preference key
            /// </summary>
            public const string SelectedParentLocation = "selected-parent-location";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string AreasAndGroupsPage = "AreasAndGroupsPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="GroupTypeIdFromPageParameter"/> property.
        /// </summary>
        private int? _groupTypeIdFromPageParameter;

        private List<ListItemBag> _schedules;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the check-in configuration <see cref="GroupType"/> entity key passed to the
        /// <see cref="PageParameterKey.CheckInConfiguration"/> page parameter.
        /// </summary>
        private string GroupTypeKeyFromPageParameter => PageParameter( PageParameterKey.CheckInConfiguration );

        /// <summary>
        /// Gets the check-in configuration <see cref="GroupType"/> Id resolved from the
        /// <see cref="PageParameterKey.CheckInConfiguration"/> page parameter.
        /// </summary>
        private int? GroupTypeIdFromPageParameter
        {
            get
            {
                if ( !_groupTypeIdFromPageParameter.HasValue )
                {
                    if ( GroupTypeKeyFromPageParameter.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    var groupType = GroupTypeCache.Get( GroupTypeKeyFromPageParameter, !PageCache.Layout.Site.DisablePredictableIds );
                    _groupTypeIdFromPageParameter = groupType?.Id;
                }

                return _groupTypeIdFromPageParameter;
            }
        }

        /// <summary>
        /// Gets the selected group type from person preferences.
        /// </summary>
        protected ListItemBag SelectedGroupType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedGroupType )
            .FromJsonOrNull<ListItemBag>();

        /// <summary>
        /// Gets the person preferences scoped to the current check-in configuration GroupType, or <c>null</c> when
        /// no configuration is resolved. Scoping to the configuration entity (rather than the block) is what allows
        /// preferences to be shared with other check-in configuration blocks.
        /// </summary>
        private PersonPreferenceCollection ConfigurationPersonPreferences
        {
            get
            {
                if ( !GroupTypeIdFromPageParameter.HasValue )
                {
                    return null;
                }

                var configuration = GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value );
                return configuration != null ? GetScopedPersonPreferences( configuration ) : null;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the currently-selected area from person preferences, or null if none is
        /// selected (i.e. the user has "All Areas" selected in the slicer). Reads from the configuration-scoped
        /// preference shared with other check-in configuration blocks when a check-in configuration is in scope;
        /// falls back to the block person preference when this block is rendered without one, since there's nothing
        /// for the area selection to share with in that case.
        /// </summary>
        protected Guid? SelectedAreaGuid => ( ConfigurationPersonPreferences ?? GetBlockPersonPreferences() )
            .GetValue( PreferenceKey.SelectedArea )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the selected category from person preferences.
        /// </summary>
        protected ListItemBag SelectedCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedCategory )
            .FromJsonOrNull<ListItemBag>();

        /// <summary>
        /// Gets the selected parent location from person preferences.
        /// </summary>
        protected ListItemBag SelectedParentLocation => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedParentLocation )
            .FromJsonOrNull<ListItemBag>();

        /// <summary>
        /// Gets the campus identifier from the request context, if defined.
        /// </summary>
        private int? ContextCampusId => RequestContext.GetContextEntity<Campus>()?.Id;

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBoxOptions();
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Saves the newly added or removed schedules to their designated group locations.
        /// </summary>
        /// <param name="scheduledLocations">The scheduled group locations</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Save( List<GroupLocationsBag> scheduledLocations )
        {
            // Load all the group locations in a single query, along with the
            // schedule information.
            var groupLocationIds = scheduledLocations
                .Select( sl => IdHasher.Instance.GetId( sl.GroupLocationId ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
            var groupLocations = new GroupLocationService( RockContext )
                .Queryable()
                .Include( gl => gl.Schedules )
                .Where( gl => groupLocationIds.Contains( gl.Id ) )
                .ToList();

            // Get the schedule IdKey values that are valid. This is used so we
            // don't delete a schedule that wasn't available for selection.
            var validScheduleIds = GetSchedules().Select( s => s.Value ).ToList();
            var scheduleService = new ScheduleService( RockContext );

            foreach ( var scheduledLocation in scheduledLocations )
            {
                var groupLocation = groupLocations.FirstOrDefault( gl => gl.IdKey == scheduledLocation.GroupLocationId );

                if ( groupLocation == null )
                {
                    return ActionBadRequest( "Group or Location was not valid." );
                }

                // Add any schedules that are new.
                foreach ( var scheduleIdKey in scheduledLocation.ScheduleIds )
                {
                    var scheduleId = IdHasher.Instance.GetId( scheduleIdKey );

                    if ( !scheduleId.HasValue )
                    {
                        continue;
                    }

                    if ( !groupLocation.Schedules.Any( s => s.Id == scheduleId ) )
                    {
                        groupLocation.Schedules.Add( scheduleService.Get( scheduleId.Value ) );
                    }
                }

                // Remove any schedules that are old.
                foreach ( var schedule in groupLocation.Schedules.ToList() )
                {
                    if ( !scheduledLocation.ScheduleIds.Contains( schedule.IdKey ) && validScheduleIds.Contains( schedule.IdKey ) )
                    {
                        groupLocation.Schedules.Remove( schedule );
                    }
                }
            }

            if ( RockContext.SaveChanges() > 0 )
            {
                RefreshConnectedKiosks();
            }

            return ActionOk();
        }

        /// <summary>
        /// Processes the cloned schedules
        /// </summary>
        /// <param name="bag">The clone schedule bag that contains the source and destination schedules</param>
        /// <returns>The updated group locations list to the client.</returns>
        [BlockAction]
        public BlockActionResult ProcessClonedSchedule( CloneScheduleBag bag )
        {
            var groupLocationQuery = GetGroupLocationQuery( out List<CheckinAreaPath> groupPaths ).ToList();

            if ( bag.SourceSchedule != null
                && Guid.TryParse( bag.SourceSchedule.Value, out var sourceScheduleGuid )
                && bag.DestinationSchedule != null
                && Guid.TryParse( bag.DestinationSchedule.Value, out var destinationScheduleGuid ) )
            {
                var sourceSchedule = NamedScheduleCache.Get( sourceScheduleGuid );
                var destinationSchedule = NamedScheduleCache.Get( destinationScheduleGuid );

                if ( sourceSchedule != null && destinationSchedule != null )
                {
                    if ( !destinationSchedule.CheckInStartOffsetMinutes.HasValue || !sourceSchedule.CheckInStartOffsetMinutes.HasValue )
                    {
                        string messagePrefix;
                        if ( !destinationSchedule.CheckInStartOffsetMinutes.HasValue && !sourceSchedule.CheckInStartOffsetMinutes.HasValue )
                        {
                            messagePrefix = "The Destination and Source schedules are";
                        }
                        else if ( !destinationSchedule.CheckInStartOffsetMinutes.HasValue )
                        {
                            messagePrefix = "The Destination schedule is";
                        }
                        else
                        {
                            messagePrefix = "The Source schedule is";
                        }

                        return ActionBadRequest( $"{messagePrefix} not enabled for check-in. You can enable check-in for a schedule by providing a value for the 'Enable Check-in' field of that schedule." );
                    }

                    var destinationScheduleId = IdHasher.Instance.GetHash( destinationSchedule.Id );
                    var sourceScheduleId = IdHasher.Instance.GetHash( sourceSchedule.Id );

                    var groupLocationsToClear = bag.CurrentScheduleConfiguration.Where( l => l.ScheduleIds.Contains( destinationScheduleId.ToString() ) ).ToList();
                    foreach ( var groupLocationToClear in groupLocationsToClear )
                    {
                        groupLocationToClear.ScheduleIds.Remove( destinationScheduleId.ToString() );
                    }

                    var groupLocationsToAdd = bag.CurrentScheduleConfiguration.Where( l => l.ScheduleIds.Contains( sourceScheduleId.ToString() ) ).ToList();
                    foreach ( var groupLocationToAdd in groupLocationsToAdd )
                    {
                        groupLocationToAdd.ScheduleIds.Add( destinationScheduleId.ToString() );
                    }

                    return ActionOk( bag.CurrentScheduleConfiguration );
                }
            }

            return ActionBadRequest( "The source and destination schedules must be defined." );
        }

        /// <summary>
        /// Loads the group schedule location data
        /// </summary>
        /// <returns>The group schedule location data bag</returns>
        [BlockAction]
        public BlockActionResult LoadGroupScheduleLocationData()
        {
            CheckInScheduleBuilderDataBag bag = new CheckInScheduleBuilderDataBag();
            var groupLocationQry = GetGroupLocationQuery( out List<CheckinAreaPath> groupPaths );
            bag.GroupLocations = GetGroupLocationSchedules( groupLocationQry, groupPaths );
            bag.Schedules = GetSchedules();

            return ActionOk( bag );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CheckInScheduleBuilderOptionsBag GetBoxOptions()
        {
            CheckInScheduleBuilderOptionsBag bag = new CheckInScheduleBuilderOptionsBag
            {
                HasValidCheckInConfigurationPageParam = GroupTypeIdFromPageParameter.HasValue,
                GroupTypes = new List<Guid>(),
                Areas = new List<ListItemBag>()
            };

            var groupTypes = GetTopGroupTypes();
            foreach ( var groupType in groupTypes )
            {
                bag.GroupTypes.Add( groupType.Guid );
            }

            var groupTypeService = new GroupTypeService( RockContext );
            var groupTypeId = GroupTypeIdFromPageParameter;
            if ( groupTypeId.GetValueOrDefault() > 0 )
            {
                bag.Areas = groupTypeService
                    .GetCheckinAreaDescendants( groupTypeId.Value )
                    .Where( a =>
                        a.GroupTypePurposeValue == null
                        || !a.GroupTypePurposeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid() )
                    )
                    .ToListItemBagList();
            }
            else
            {
                List<GroupTypeCache> allAreas = new List<GroupTypeCache>();
                foreach ( var groupType in groupTypes )
                {
                    var areas = groupTypeService
                        .GetCheckinAreaDescendants( groupType.Id )
                        .Where( a =>
                            a.GroupTypePurposeValue == null
                            || !a.GroupTypePurposeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid() )
                        );

                    allAreas.AddRange( areas );
                }

                bag.Areas = allAreas.ToListItemBagList();
            }

            var defaultCategoryId = CategoryCache.GetId( Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES.AsGuid() );
            if ( defaultCategoryId.HasValue )
            {
                bag.DefaultScheduleCategory = new ListItemBag
                {
                    Text = CategoryCache.Get( defaultCategoryId.Value ).Name,
                    Value = Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES,
                };
            }

            bag.NavigationUrls = GetBoxNavigationUrls();

            bag.ConfigurationIdKey = groupTypeId?.AsIdKey();
            bag.ConfigurationName = groupTypeId.HasValue ? GroupTypeCache.Get( groupTypeId.Value )?.Name : null;
            bag.SelectedAreaGuid = SelectedAreaGuid;

            bag.CampusRootLocations = CampusCache.All()
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusGuid = c.Guid.ToString(),
                    LocationGuid = NamedLocationCache.Get( c.LocationId.Value, RockContext )?.Guid.ToString()
                } )
                .Where( c => c.LocationGuid != null )
                .ToDictionary( c => c.CampusGuid, c => c.LocationGuid );

            return bag;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };

            // The Areas and Groups page route requires a configuration, so only deep-link when one is in scope.
            if ( GroupTypeIdFromPageParameter.HasValue )
            {
                urls[NavigationUrlKey.AreasAndGroupsPage] = $"/admin/checkin/configuration-areas-groups/{GroupTypeIdFromPageParameter.Value.AsIdKey()}";
            }

            return urls;
        }

        /// <summary>
        /// Gets the list of schedules to display and applies the category filter against the schedules.
        /// </summary>
        /// <returns>A list of schedules</returns>
        private List<ListItemBag> GetSchedules()
        {
            if ( _schedules?.Count > 0 )
            {
                return _schedules;
            }

            ScheduleService scheduleService = new ScheduleService( RockContext );
            var schedules = new List<ListItemBag>();

            // limit Schedules to ones that are Active and have a CheckInStartOffsetMinutes
            var scheduleQry = scheduleService.Queryable().Where( a => a.IsActive && a.CheckInStartOffsetMinutes != null );

            // Resolve the category used to filter Schedules. A missing preference means the individual
            // hasn't chosen a category yet, so default to Service Times to match the slicer's seeded default.
            // An explicit selection whose value isn't a category Guid means "Shared" (Schedules with no category).
            var categoryGuid = SelectedCategory != null
                ? SelectedCategory.Value.AsGuidOrNull()
                : Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES.AsGuid();

            var categoryId = categoryGuid.HasValue ? CategoryCache.GetId( categoryGuid.Value ) : null;

            scheduleQry = scheduleQry.Where( a => a.CategoryId == categoryId );

            // clear out any existing schedule columns and add the ones that match the current filter setting
            var scheduleList = scheduleQry.OrderBy( a => a.Name ).ToList();
            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

            foreach ( var item in sortedScheduleList )
            {
                schedules.Add( new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( item.Id ),
                    Text = item.Name ?? $"(unnamed {item.Id})"
                } );
            }

            _schedules = schedules;
            return _schedules;
        }

        /// <summary>
        /// Generates the GroupLocation query using the selected filters.
        /// </summary>
        /// <param name="groupPaths">The group paths.</param>
        /// <returns></returns>
        private IQueryable<GroupLocation> GetGroupLocationQuery( out List<CheckinAreaPath> groupPaths )
        {
            var groupLocationService = new GroupLocationService( RockContext );
            var groupTypeService = new GroupTypeService( RockContext );
            groupPaths = new List<CheckinAreaPath>();
            var groupLocationQry = groupLocationService.Queryable().Where( gl => gl.Group.IsActive && !gl.Group.IsArchived );

            if ( ContextCampusId.HasValue )
            {
                var campus = CampusCache.Get( ContextCampusId.Value );
                if ( campus?.LocationId != null )
                {
                    var locationService = new LocationService( RockContext );
                    var campusLocationIds = locationService.GetAllDescendents( campus.LocationId.Value )
                        .Select( l => l.Id )
                        .ToList();
                    campusLocationIds.Add( campus.LocationId.Value );

                    groupLocationQry = groupLocationQry.Where( gl => campusLocationIds.Contains( gl.LocationId ) );
                }
            }

            // Determine the groupTypeId to use: first from the page parameter, then from the selected group type, or default to "All".
            int? groupTypeId = GroupTypeIdFromPageParameter
                ?? ( SelectedGroupType != null && Guid.TryParse( SelectedGroupType.Value, out var groupTypeGuid )
                    ? GroupTypeCache.Get( groupTypeGuid ).Id
                    : Rock.Constants.All.Id );

            int? selectedAreaId = null;
            if ( SelectedAreaGuid.HasValue )
            {
                selectedAreaId = GroupTypeCache.Get( SelectedAreaGuid.Value ).Id;
            }
            if ( groupTypeId != Rock.Constants.All.Id )
            {
                var descendantGroupTypeIds = groupTypeService.GetCheckinAreaDescendants( groupTypeId.Value ).Select( a => a.Id );

                if ( selectedAreaId.HasValue )
                {
                    descendantGroupTypeIds = descendantGroupTypeIds.Where( a => a == selectedAreaId.Value );
                }

                // filter to groups that either are of the GroupType or are of a GroupType that has the selected GroupType as a parent (ancestor)
                groupLocationQry = groupLocationQry.Where( a => a.Group.GroupType.Id == groupTypeId || descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) );

                groupPaths = groupTypeService.GetCheckinAreaDescendantsPath( groupTypeId.Value ).ToList();
            }
            else
            {
                List<int> descendantGroupTypeIds = new List<int>();
                foreach ( GroupType groupType in GetTopGroupTypes() )
                {
                    descendantGroupTypeIds.Add( groupType.Id );

                    groupPaths.AddRange( groupTypeService.GetCheckinAreaDescendantsPath( groupType.Id ).ToList() );
                    foreach ( var childGroupType in groupTypeService.GetChildGroupTypes( groupType.Id ) )
                    {
                        descendantGroupTypeIds.Add( childGroupType.Id );
                        descendantGroupTypeIds.AddRange( groupTypeService.GetCheckinAreaDescendants( childGroupType.Id ).Select( a => a.Id ).ToList() );
                    }
                }

                if ( selectedAreaId.HasValue )
                {
                    descendantGroupTypeIds = descendantGroupTypeIds.Where( a => a == selectedAreaId.Value ).ToList();
                }

                groupLocationQry = groupLocationQry.Where( a => descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) );
            }

            groupLocationQry = groupLocationQry.OrderBy( a => a.Group.Name ).ThenBy( a => a.Location.Name );

            return groupLocationQry;
        }

        private List<GroupType> GetTopGroupTypes()
        {
            var groupTypes = new List<GroupType>();

            // Populate the GroupType DropDownList only with GroupTypes with GroupTypePurpose of Check-in Template
            // or with group types that allow multiple locations/schedules and support named locations
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
            GroupTypeService groupTypeService = new GroupTypeService( RockContext );

            // First find all the group types that have a purpose of 'Check-in Template'
            var checkInGroupTypeIds = groupTypeService.Queryable()
                .Where( t =>
                    t.GroupTypePurposeValueId.HasValue &&
                    t.GroupTypePurposeValueId.Value == groupTypePurposeCheckInTemplateId )
                .Select( t => t.Id )
                .ToList();

            // Now find all their descendants (so we can exclude them in a sec)
            var descendentGroupTypeIds = new List<int>();
            foreach ( int id in checkInGroupTypeIds )
            {
                descendentGroupTypeIds.AddRange( groupTypeService.GetCheckinAreaDescendants( id ).Select( a => a.Id ).ToList() );
            }

            // Now query again for all the types that have a purpose of 'Check-in Template' or support check-in outside of being a descendant of the template
            var groupTypeList = groupTypeService.Queryable()
                .Where( a =>
                    checkInGroupTypeIds.Contains( a.Id ) ||
                    (
                        !descendentGroupTypeIds.Contains( a.Id ) &&
                        a.AllowMultipleLocations &&
                        a.EnableLocationSchedules.HasValue &&
                        a.EnableLocationSchedules.Value &&
                        a.LocationTypes.Any()
                    ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            foreach ( var groupType in groupTypeList )
            {
                // Make sure the group type supports named locations (we can't query on this in the above qry)
                if ( groupType.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId ||
                    ( groupType.LocationSelectionMode & GroupLocationPickerMode.Named ) == GroupLocationPickerMode.Named )
                {
                    groupTypes.Add( groupType );
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Gets the group location schedules
        /// </summary>
        /// <param name="groupLocationQry">The group location queryable</param>
        /// <param name="groupPaths">The list of group paths</param>
        /// <returns>A list of group locations</returns>
        private List<GroupLocationsBag> GetGroupLocationSchedules( IQueryable<GroupLocation> groupLocationQry, List<CheckinAreaPath> groupPaths )
        {
            var groupService = new GroupService( RockContext );
            var bags = new List<GroupLocationsBag>();

            var qryList = groupLocationQry
                .Where( a => a.Location != null )
                .Select( a =>
                new
                {
                    GroupLocationId = a.Id,
                    a.Location,
                    GroupId = a.GroupId,
                    GroupName = a.Group.Name,
                    ScheduleIdList = a.Schedules.Select( s => s.Id ),
                    GroupTypeId = a.Group.GroupTypeId
                } ).ToList();

            var locationService = new LocationService( RockContext );
            if ( SelectedParentLocation != null && Guid.TryParse( SelectedParentLocation.Value, out var parentLocationGuid ) )
            {
                var currentAndDescendantLocationIds = new List<int>();
                var parentLocationId = locationService.Get( parentLocationGuid ).Id;
                currentAndDescendantLocationIds.Add( parentLocationId );
                currentAndDescendantLocationIds.AddRange( locationService.GetAllDescendents( parentLocationId ).Select( a => a.Id ) );

                qryList = qryList.Where( a => currentAndDescendantLocationIds.Contains( a.Location.Id ) ).ToList();
            }

            var locationPaths = new Dictionary<int, string>();

            foreach ( var row in qryList )
            {
                var bag = new GroupLocationsBag
                {
                    GroupLocationId = IdHasher.Instance.GetHash( row.GroupLocationId ),
                    GroupPath = groupService.GroupAncestorPathName( row.GroupId ),
                    AreaPath = groupPaths.Where( gt => gt.GroupTypeId == row.GroupTypeId ).Select( gt => gt.Path ).FirstOrDefault(),
                    LocationName = row.Location.Name,
                    ScheduleIds = row.ScheduleIdList
                            .Select( id => IdHasher.Instance.GetHash( id ) )
                            .ToList()
                };

                if ( row.Location.ParentLocationId.HasValue )
                {
                    int locationId = row.Location.ParentLocationId.Value;

                    if ( !locationPaths.ContainsKey( locationId ) )
                    {
                        var locationNames = new List<string>();
                        var parentLocation = locationService.Get( locationId );
                        while ( parentLocation != null )
                        {
                            locationNames.Add( parentLocation.Name );
                            parentLocation = parentLocation.ParentLocation;
                        }
                        if ( locationNames.Any() )
                        {
                            locationNames.Reverse();
                            locationPaths.Add( locationId, locationNames.AsDelimited( " > " ) );
                        }
                        else
                        {
                            locationPaths.Add( locationId, string.Empty );
                        }
                    }

                    bag.LocationPath = locationPaths[locationId];
                }

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Clears the kiosk device cache and pushes a refresh notification to all connected kiosks so configuration
        /// changes propagate without waiting for an app recycle.
        /// </summary>
        private void RefreshConnectedKiosks()
        {
#if NET472_OR_GREATER
            // Temporary until legacy check-in is removed.
            KioskDevice.Clear();
#endif

            // I know, this is a terrible hack. But we need to force the
            // kiosks to refresh and we don't want to make this public yet. -dsh
            typeof( GroupType ).Assembly.GetType( "Rock.CheckIn.v2.CheckInDirector" )
                ?.GetMethod( "SendRefreshKioskConfiguration" )
                ?.Invoke( null, new object[0] );
        }

        #endregion Private Methods
    }
}
