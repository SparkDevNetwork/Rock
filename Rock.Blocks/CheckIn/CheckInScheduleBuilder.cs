using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder;
using Rock.Web.Cache;
using Rock.ViewModels.Utility;
using Rock.Utility;
using Rock.CheckIn;
using System.Data.Entity;

namespace Rock.Blocks.CheckIn
{
    [DisplayName( "Schedule Builder" )]
    [Category( "Check-in" )]
    [Description( "Helps to build schedules used for check-in." )]
    [IconCssClass( "fa fa-clipboard" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "28B9DAB2-C58A-4459-9EE7-8D1895C09592" )]
    [Rock.SystemGuid.BlockTypeGuid( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" )]
    [CustomizedGrid]
    public class CheckInScheduleBuilder : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The group type id page parameter key
            /// </summary>
            public const string GroupTypeId = "GroupTypeId";
        }

        /// <summary>
        /// Keys for user preferences
        /// </summary>
        private static class PreferenceKey
        {
            /// <summary>
            /// The selected group type user preference key
            /// </summary>
            public const string SelectedGroupType = "selected-group-type";

            /// <summary>
            /// The selected area user preference key
            /// </summary>
            public const string SelectedArea = "selected-area";

            /// <summary>
            /// The selected category user preference key
            /// </summary>
            public const string SelectedCategory = "selected-category";

            /// <summary>
            /// The selected parent location user preference key
            /// </summary>
            public const string SelectedParentLocation = "selected-parent-location";
        }

        #endregion

        #region Properties

        protected ListItemBag SelectedGroupType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedGroupType )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag SelectedArea => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedArea )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag SelectedCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedCategory )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag SelectedParentLocation => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedParentLocation )
            .FromJsonOrNull<ListItemBag>();

        private List<ListItemBag> _schedules;
        private int? _groupTypeId;

        #endregion

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBoxOptions();
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CheckInScheduleBuilderOptionsBag GetBoxOptions()
        {
            CheckInScheduleBuilderOptionsBag bag = new CheckInScheduleBuilderOptionsBag
            {
                GroupTypes = new List<Guid>(),
                Areas = new List<Guid>()
            };

            var groupTypes = GetTopGroupTypes();
            foreach ( var groupType in groupTypes )
            {
                bag.GroupTypes.Add( groupType.Guid );
            }

            var groupTypeService = new GroupTypeService( RockContext );
            var groupTypeId = GetGroupTypeIdFromPageParam();
            if ( groupTypeId.HasValue )
            {
                bag.Areas = groupTypeService.GetCheckinAreaDescendants( groupTypeId.Value ).Where( a => a.GroupTypePurposeValue == null || !a.GroupTypePurposeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid() ) ).Select( a => a.Guid ).ToList();
            }
            else
            {
                List<GroupTypeCache> allAreas = new List<GroupTypeCache>();
                foreach ( var groupType in groupTypes )
                {
                    var areas = groupTypeService.GetCheckinAreaDescendants( groupType.Id ).Where( a => a.GroupTypePurposeValue == null || !a.GroupTypePurposeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid() ) );
                    allAreas.AddRange( areas );
                }

                bag.Areas = allAreas.Select( a => a.Guid ).ToList();
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

            return bag;
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

            // limit Schedules to the Category from the Filter
            if ( SelectedCategory != null && Guid.TryParse( SelectedCategory.Value, out var categoryGuid ) )
            {
                var categoryId = CategoryCache.Get( categoryGuid ).Id;
                scheduleQry = scheduleQry.Where( a => a.CategoryId == categoryId );
            }
            else
            {
                // NULL (or 0) means Shared, so specifically filter so to show only Schedules with CategoryId NULL
                scheduleQry = scheduleQry.Where( a => a.CategoryId == null );
            }

            // clear out any existing schedule columns and add the ones that match the current filter setting
            var scheduleList = scheduleQry.OrderBy( a => a.Name ).ToList();
            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

            foreach ( var item in sortedScheduleList )
            {
                schedules.Add( new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( item.Id ),
                    Text = item.Name
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

            // Determine the groupTypeId to use: first from the page parameter, then from the selected group type, or default to "All".
            int? groupTypeId = _groupTypeId;
            if ( !groupTypeId.HasValue )
            {
                groupTypeId = GetGroupTypeIdFromPageParam()
                               ?? ( SelectedGroupType != null && Guid.TryParse( SelectedGroupType.Value, out var groupTypeGuid )
                                   ? GroupTypeCache.Get( groupTypeGuid ).Id
                                   : Rock.Constants.All.Id );
            }

            int? selectedAreaId = null;
            if ( SelectedArea != null && Guid.TryParse( SelectedArea.Value, out var areaGuid ) )
            {
                selectedAreaId = GroupTypeCache.Get( areaGuid ).Id;
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
        /// Gets the Group Type Id found on the Page Parameter Key
        /// </summary>
        /// <returns>The Group Type Id </returns>
        private int? GetGroupTypeIdFromPageParam()
        {
            if ( _groupTypeId != null )
            {
                return _groupTypeId;
            }

            var pageParamId = PageParameter( PageParameterKey.GroupTypeId );
            _groupTypeId = Rock.Utility.IdHasher.Instance.GetId( pageParamId ) ?? pageParamId.AsIntegerOrNull();
            return _groupTypeId;
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
                // Temporary until legacy check-in is removed.
                KioskDevice.Clear();
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
                && Guid.TryParse( bag.SourceSchedule.Value, out var sourceScheduleId )
                && bag.DestinationSchedule != null
                && Guid.TryParse( bag.DestinationSchedule.Value, out var destinationScheduleId ) )
            {
                var sourceSchedule = NamedScheduleCache.Get( sourceScheduleId );
                var destinationSchedule = NamedScheduleCache.Get( destinationScheduleId );

                if ( sourceSchedule != null && destinationSchedule != null )
                {
                    var srcGroupLocations = groupLocationQuery.Where( gl => gl.Schedules.Any( s => s.Id == sourceSchedule.Id ) );
                    var destGroupLocations = groupLocationQuery.Where( gl => gl.Schedules.Any( s => s.Id == destinationSchedule.Id ) );

                    // Remove the target schedule from any existing GroupLocations, this should clear any existing enabled location for this configuration.
                    foreach ( var groupLocation in destGroupLocations )
                    {
                        var scheduleToRemove = groupLocation.Schedules.FirstOrDefault( s => s.Id == destinationSchedule.Id );
                        groupLocation.Schedules.Remove( scheduleToRemove );
                    }

                    // Add the target/destination schedule to any locations with the source schedule, this should enable the same locations as the source schedule.
                    var targetSchedule = new ScheduleService( RockContext ).Get( destinationSchedule.Id );
                    foreach ( var item in srcGroupLocations )
                    {
                        item.Schedules.Add( targetSchedule );
                    }

                    var groupScheduleLocationData = GetGroupLocationSchedules( groupLocationQuery.AsQueryable(), groupPaths );
                    return ActionOk( groupScheduleLocationData );
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
    }
}
