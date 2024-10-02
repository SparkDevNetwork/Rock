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
    public class CheckInScheduleBuilder : RockEntityListBlockType<CheckInGroupBag>
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
        }

        #endregion

        #region Properties

        protected Guid? SelectedGroupType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedGroupType )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected Guid? SelectedArea => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedArea )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

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
            return GetSchedules();
        }

        /// <inheritdoc/>
        protected override IQueryable<CheckInGroupBag> GetListQueryable( RockContext rockContext )
        {
            var groupLocationQry = GetGroupLocationQuery( out List<CheckinAreaPath> groupPaths );

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

        private CheckInScheduleBuilderOptionsBag GetSchedules()
        {
            ScheduleService scheduleService = new ScheduleService( RockContext );
            CheckInScheduleBuilderOptionsBag bag = new CheckInScheduleBuilderOptionsBag();

            // limit Schedules to ones that are Active and have a CheckInStartOffsetMinutes
            var scheduleQry = scheduleService.Queryable().Where( a => a.IsActive && a.CheckInStartOffsetMinutes != null );

            // TODO
            //// limit Schedules to the Category from the Filter
            //int scheduleCategoryId = pCategory.SelectedValueAsInt() ?? Rock.Constants.All.Id;
            //if ( scheduleCategoryId != Rock.Constants.All.Id )
            //{
            //    scheduleQry = scheduleQry.Where( a => a.CategoryId == scheduleCategoryId );
            //}
            //else
            //{
            //    // NULL (or 0) means Shared, so specifically filter so to show only Schedules with CategoryId NULL
            //    scheduleQry = scheduleQry.Where( a => a.CategoryId == null );
            //}

            // clear out any existing schedule columns and add the ones that match the current filter setting
            var scheduleList = scheduleQry.ToList().OrderBy( a => a.Name ).ToList();
            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

            if ( !scheduleList.Any() )
            {
                //TODO: Resolve URL on Client side? Format on Client.
                bag.WarningMessage = "<p><strong>Warning</strong></p>No schedules found. Consider <a class='alert-link' href='{0}'>adding a schedule</a> or a different schedule category.";
                return bag;
            }

            foreach ( var item in sortedScheduleList )
            {
                bag.Schedules.Add( item.Id, item.Name );
            }

            return bag;
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
            var groupTypeIdParam = PageParameter( PageParameterKey.GroupTypeId );
            var groupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupTypeIdParam ) ?? groupTypeIdParam.AsIntegerOrNull();


            // if this page has a PageParam for groupTypeId use that to limit which groupTypeId to see. Otherwise, use the groupTypeId specified in the filter
            if ( !groupTypeId.HasValue )
            {
                if ( SelectedGroupType.HasValue )
                {
                    groupTypeId = GroupTypeCache.Get( SelectedGroupType.Value ).Id;
                }
                else
                {
                    groupTypeId = Rock.Constants.All.Id;
                }
            }

            int? selectedAreaId = null;
            if ( SelectedArea.HasValue )
            {
                selectedAreaId = GroupTypeCache.Get(SelectedArea.Value).Id;
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
    }
}
