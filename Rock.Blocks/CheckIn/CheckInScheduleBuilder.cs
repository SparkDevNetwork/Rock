using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.ViewModels.Blocks.Crm.PhotoVerifyList;
using Rock.ViewModels.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder;
using Rock.Web.Cache;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;
using DotLiquid;

namespace Rock.Blocks.CheckIn
{
    [DisplayName( "Schedule Builder" )]
    [Category( "Check-in" )]
    [Description( "Helps to build schedules used for check-in." )]
    [IconCssClass( "fa fa-clipboard" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    //[Rock.SystemGuid.EntityTypeGuid(  )]
    //[Rock.SystemGuid.BlockTypeGuid(  )]
    [CustomizedGrid]
    public class CheckInScheduleBuilder : RockListBlockType<CheckInGroupBag>
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
            var box = new ListBlockBox<CheckInScheduleBuilderOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
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

            // TODO: Hide picker on the Obsidian side if the Page Parameter has a value.
            var groupTypes = GetTopGroupTypes();
            foreach ( var groupType in groupTypes )
            {
                bag.GroupTypes.Add( groupType.Guid );
            }

            var groupTypeService = new GroupTypeService( RockContext );
            if ( _groupTypeId.HasValue )
            {
                bag.Areas = groupTypeService.GetCheckinAreaDescendants( _groupTypeId.Value ).Where( a => a.GroupTypePurposeValue == null || !a.GroupTypePurposeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid() ) ).Select( a => a.Guid).ToList();
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

            bag.Schedules = GetSchedules();

            if ( !bag.Schedules.Any() )
            {
                //TODO: Resolve URL on Client side? Format on Client.
                bag.WarningMessage = "<p><strong>Warning</strong></p>No schedules found. Consider <a class='alert-link' href='{0}'>adding a schedule</a> or a different schedule category.";
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override IQueryable<CheckInGroupBag> GetListQueryable( RockContext rockContext )
        {
            var groupLocationQry = GetGroupLocationQuery( out List<CheckinAreaPath> groupPaths );
            var groupService = new GroupService( rockContext );
            var bags = new List<CheckInGroupBag>();

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

            var locationService = new LocationService( rockContext );
            if ( SelectedParentLocation != null && Guid.TryParse( SelectedParentLocation.Value, out var parentLocationGuid ) )
            {
                var currentAndDescendantLocationIds = new List<int>();
                var parentLocationId = locationService.Get( parentLocationGuid ).Id;
                currentAndDescendantLocationIds.Add( parentLocationId );
                currentAndDescendantLocationIds.AddRange( locationService.GetAllDescendents( parentLocationId ).Select( a => a.Id ) );

                qryList = qryList.Where( a => currentAndDescendantLocationIds.Contains( a.Location.Id ) ).ToList();
            }

            if ( _schedules == null || _schedules.Count == 0 )
            {
                _schedules = GetSchedules();
            }

            var locationPaths = new Dictionary<int, string>();

            foreach ( var row in qryList )
            {
                var bag = new CheckInGroupBag
                {
                    GroupLocationId = row.GroupLocationId,
                    GroupName = groupService.GroupAncestorPathName( row.GroupId ),
                    GroupPath = groupPaths.Where( gt => gt.GroupTypeId == row.GroupTypeId ).Select( gt => gt.Path ).FirstOrDefault(),
                    LocationName = row.Location.Name,
                    IsScheduleSelected = new List<ListItemBag>()
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

                    foreach ( var schedule in _schedules )
                    {
                        bag.IsScheduleSelected.Add( new ListItemBag
                        {
                            Value = schedule.Value,
                            Text = row.ScheduleIdList.Any( a => a == schedule.Value.ToIntSafe() ).ToString()
                        } );
                    }
                }

                bags.Add( bag );
            }

            return bags.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<CheckInGroupBag> GetGridBuilder()
        {
            return new GridBuilder<CheckInGroupBag>()
                .WithBlock( this )
                .AddTextField( "groupLocationId", a => a.GroupLocationId.ToString() )
                .AddTextField( "groupId", a => a.GroupId.ToString() )
                .AddTextField( "groupName", a => a.GroupName )
                .AddTextField( "groupPath", a => a.GroupPath )
                .AddTextField( "locationName", a => a.LocationName )
                .AddTextField( "locationPath", a => a.LocationPath )
                .AddField( "isScheduleSelected", a => a.IsScheduleSelected );
        }

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
            var scheduleList = scheduleQry.OrderBy(a => a.Name).ToList();
            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

            foreach ( var item in sortedScheduleList )
            {
                schedules.Add( new ListItemBag
                {
                    Value = item.Id.ToString(),
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
                groupTypeId = GetIdFromPageParam( PageParameterKey.GroupTypeId )
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

            // populate the GroupType DropDownList only with GroupTypes with GroupTypePurpose of Check-in Template
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

        private int? GetIdFromPageParam( string pageParameterKey )
        {
            var pageParamId = PageParameter( pageParameterKey );
            _groupTypeId = Rock.Utility.IdHasher.Instance.GetId( pageParamId ) ?? pageParamId.AsIntegerOrNull();
            return _groupTypeId;
        }
    }
}
