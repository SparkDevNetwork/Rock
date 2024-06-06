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
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ScheduleList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of schedules.
    /// </summary>
    [DisplayName( "Schedule List" )]
    [Category( "Core" )]
    [Description( "Lists all the schedules." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        IsRequired = false,
        Order = 0 )]

    [BooleanField(
        "Filter Category From Query String",
        Key = AttributeKey.FilterCategoryFromQueryString,
        DefaultBooleanValue = false,
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "259b6074-eefa-4638-a7ed-c2169f450bee" )]
    [Rock.SystemGuid.BlockTypeGuid( "b6a17e77-e53d-4c96-bcb2-643123b8160c" )]
    [CustomizedGrid]
    public class ScheduleList : RockEntityListBlockType<Schedule>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string FilterCategoryFromQueryString = "FilterCategoryFromQueryString";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
            public const string CategoryGuid = "CategoryGuid";
        }

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region  Fields

        private Guid? _categoryGuid;
        private HashSet<int> _schedulesWithAttendance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the category guid to filter results by.
        /// </summary>
        /// <value>
        /// The category filter.
        /// </value>
        protected Guid? FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the active status to filter results by.
        /// </summary>
        /// <value>
        /// The active status filter.
        /// </value>
        protected string FilterActiveStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterActiveStatus );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ScheduleListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled && GetAttributeValue( AttributeKey.DetailPage ).IsNotNullOrWhiteSpace();
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ScheduleListOptionsBag GetBoxOptions()
        {
            var categoryGuid = GetCategoryGuid();
            var filterCategoryFromQueryString = GetAttributeValue( AttributeKey.FilterCategoryFromQueryString ).AsBoolean();

            var options = new ScheduleListOptionsBag()
            {
                IsGridVisible = !filterCategoryFromQueryString || categoryGuid.HasValue,
                IsCategoryColumnAndFilterVisible = !filterCategoryFromQueryString
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ScheduleId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Schedule> GetListQueryable( RockContext rockContext )
        {
            var queryable = new ScheduleService( rockContext ).Queryable().Where( a => !string.IsNullOrEmpty( a.Name ) );

            Guid? categoryGuid = this.GetAttributeValue( AttributeKey.FilterCategoryFromQueryString ).AsBoolean() ? GetCategoryGuid() : FilterCategory;

            // Filter by Category
            if ( categoryGuid.HasValue )
            {
                queryable = queryable.Where( a => a.Category.Guid == categoryGuid.Value );
            }

            // Filter by IsActive
            if ( !string.IsNullOrWhiteSpace( FilterActiveStatus ) )
            {
                var activeFilter = FilterActiveStatus.AsBoolean();
                queryable = queryable.Where( b => b.IsActive == activeFilter );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override List<Schedule> GetListItems( IQueryable<Schedule> queryable, RockContext rockContext )
        {
            var items = queryable.ToList().OrderByOrderAndNextScheduledDateTime();

            // Populate _schedulesWithAttendance so that a warning can be displayed if a schedule with attendances is deleted.
            var displayedScheduleIds = items.ConvertAll( a => a.Id );
            var schedulesWithAttendance = new AttendanceService( rockContext )
                .Queryable()
                .Where( a => a.Occurrence.ScheduleId.HasValue && displayedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
                .Select( a => a.Occurrence.ScheduleId.Value );

            _schedulesWithAttendance = new HashSet<int>( schedulesWithAttendance.Distinct().ToList() );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Schedule> GetGridBuilder()
        {
            return new GridBuilder<Schedule>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "category", a => a.Category?.Name )
                .AddField( "hasAttendance", a => _schedulesWithAttendance.Contains( a.Id ) )
                .AddField( "isActive", a => a.IsActive );
        }

        /// <summary>
        /// Gets the category unique identifier from the Page parameters.
        /// </summary>
        /// <returns></returns>
        private Guid? GetCategoryGuid()
        {
            if ( _categoryGuid == null )
            {
                var categoryId = this.PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();

                if ( !categoryId.HasValue )
                {
                    _categoryGuid = this.PageParameter( PageParameterKey.CategoryGuid ).AsGuidOrNull();
                }
                else
                {
                    _categoryGuid = CategoryCache.Get( categoryId.Value )?.Guid;
                }
            }

            return _categoryGuid;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ScheduleService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Schedule.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Schedule.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
