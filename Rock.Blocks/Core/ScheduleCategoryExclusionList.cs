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
using Rock.ViewModels.Blocks.Core.ScheduleCategoryExclusionList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of schedule category exclusions.
    /// </summary>
    [DisplayName( "Schedule Category Exclusion List" )]
    [Category( "Core" )]
    [Description( "List of dates that schedules are not active for an entire category." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [CategoryField( "Category",
        Description = "Optional Category to use (if not specified, query will be determined by query string).",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.Category )]

    [Rock.SystemGuid.EntityTypeGuid( "c08129e7-d22a-4213-8703-0f0c1511ebdd" )]
    [Rock.SystemGuid.BlockTypeGuid( "6bc7da76-1a19-4685-b50a-dfd7eaa5ce33" )]
    [CustomizedGrid]
    public class ScheduleCategoryExclusionList : RockEntityListBlockType<ScheduleCategoryExclusion>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Category = "Category";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
        }

        #endregion Keys

        #region Fields

        private int? _categoryId = null;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ScheduleCategoryExclusionListOptionsBag>();
            var builder = GetGridBuilder();

            var canConfigure = GetCanAdministrateAuthorization();
            box.IsAddEnabled = canConfigure;
            box.IsDeleteEnabled = canConfigure;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ScheduleCategoryExclusionListOptionsBag GetBoxOptions()
        {
            var options = new ScheduleCategoryExclusionListOptionsBag()
            {
                CanAdministrate = GetCanAdministrateAuthorization(),
                IsBlockVisible = GetCategoryId().HasValue,
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetCanAdministrateAuthorization()
        {
            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<ScheduleCategoryExclusion> GetListQueryable( RockContext rockContext )
        {
            IEnumerable<ScheduleCategoryExclusion> exclusions = Enumerable.Empty<ScheduleCategoryExclusion>();

            var categoryId = GetCategoryId();

            if ( categoryId.HasValue )
            {
                exclusions = new ScheduleCategoryExclusionService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( e => e.CategoryId == categoryId.Value );
            }

            return exclusions.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<ScheduleCategoryExclusion> GetOrderedListQueryable( IQueryable<ScheduleCategoryExclusion> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( s => s.StartDate );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ScheduleCategoryExclusion> GetGridBuilder()
        {
            return new GridBuilder<ScheduleCategoryExclusion>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddDateTimeField( "start", a => a.StartDate )
                .AddDateTimeField( "end", a => a.EndDate );
        }

        private int? GetCategoryId()
        {
            if ( !_categoryId.HasValue )
            {
                var categoryGuid = GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
                if ( categoryGuid.HasValue )
                {
                    var category = CategoryCache.Get( categoryGuid.Value );
                    if ( category != null )
                    {
                        _categoryId = category.Id;
                    }
                }

                if ( !_categoryId.HasValue )
                {
                    var categoryIdParam = PageParameter( PageParameterKey.CategoryId );
                    _categoryId = Rock.Utility.IdHasher.Instance.GetId( categoryIdParam ) ?? PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();
                }
            }

            return _categoryId;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
                            var entityService = new ScheduleCategoryExclusionService( RockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{ScheduleCategoryExclusion.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${ScheduleCategoryExclusion.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                RockContext.SaveChanges();

                Rock.CheckIn.KioskDevice.Clear();

                return ActionOk();
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="bag">The specified entity.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( ScheduleCategoryExclusionBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${ScheduleCategoryExclusion.FriendlyTypeName}." );
            }

            var entityService = new ScheduleCategoryExclusionService( RockContext );
            var entity = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                entity = new ScheduleCategoryExclusion();
                entityService.Add( entity );
            }

            var categoryId = GetCategoryId();

            if ( !categoryId.HasValue )
            {
                return ActionBadRequest( "Ensure a category is configured for the block." );
            }

            entity.CategoryId = categoryId.Value;
            entity.Title = bag.Title;
            entity.StartDate = bag.StartDate.Date;
            entity.EndDate = bag.EndDate.Date.AddDays( 1 ).AddSeconds( -1 );

            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults[0]?.ErrorMessage );
            }

            RockContext.SaveChanges();
            Rock.CheckIn.KioskDevice.Clear();

            return ActionOk();
        }

        #endregion
    }
}
