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
using Rock.ViewModels.Blocks.Core.EntitySearchDetail;
using Rock.ViewModels.Blocks.Core.EntitySearchList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of entity searches.
    /// </summary>

    [DisplayName( "Entity Search List" )]
    [Category( "Core" )]
    [Description( "Displays a list of entity searches." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the entity search details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "d6e8c3ce-8981-4086-b1c2-111819634bb4" )]
    [Rock.SystemGuid.BlockTypeGuid( "618265a6-1738-4b12-a9a8-153e260b8a79" )]
    [CustomizedGrid]
    public class EntitySearchList : RockEntityListBlockType<EntitySearch>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<EntitySearchListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
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
        private EntitySearchListOptionsBag GetBoxOptions()
        {
            var options = new EntitySearchListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new EntitySearch();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "EntitySearchId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<EntitySearch> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( a => a.EntityType );
        }

        /// <inheritdoc/>
        protected override GridBuilder<EntitySearch> GetGridBuilder()
        {
            return new GridBuilder<EntitySearch>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "entityType", a => a.EntityType?.FriendlyName )
                .AddTextField( "key", a => a.Key )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "disablePreview", a => a.GroupByExpression == string.Empty
                    && a.SortExpression == string.Empty
                    && a.SelectExpression == string.Empty
                    && a.SelectManyExpression == string.Empty
                    && a.WhereExpression == string.Empty )
                .AddTextField( "description", a => a.Description )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntity( string idKey, RockContext rockContext, out EntitySearch entity, out BlockActionResult error )
        {
            var entityService = new EntitySearchService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new EntitySearch();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{EntitySearch.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${EntitySearch.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the bag for viewing or editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="EntitySearchBag"/> that represents the entity.</returns>
        private EntitySearchBag GetEntityBag( EntitySearch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new EntitySearchBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                IsActive = entity.IsActive,
                IsEntitySecurityEnabled = entity.IsEntitySecurityEnabled,
                IsRefinementAllowed = entity.IsRefinementAllowed,
                Key = entity.Key,
                MaximumResultsPerQuery = entity.MaximumResultsPerQuery,
                Name = entity.Name,
                IncludePaths = entity.IncludePaths,
                GroupByExpression = entity.GroupByExpression,
                SortExpression = entity.SortExpression,
                SelectExpression = entity.SelectExpression,
                SelectManyExpression = entity.SelectManyExpression,
                WhereExpression = entity.WhereExpression
            };

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( EntitySearch entity, DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.GroupByExpression ),
                () => entity.GroupByExpression = box.Entity.GroupByExpression );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsEntitySecurityEnabled ),
                () => entity.IsEntitySecurityEnabled = box.Entity.IsEntitySecurityEnabled );

            box.IfValidProperty( nameof( box.Entity.IncludePaths ), () =>
            {
                var paths = ( box.Entity.IncludePaths ?? string.Empty ).Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                if ( paths.Any() )
                {
                    entity.IncludePaths = string.Join( ",", paths );
                }
                else
                {
                    entity.IncludePaths = null;
                }
            } );

            box.IfValidProperty( nameof( box.Entity.IsRefinementAllowed ),
                () => entity.IsRefinementAllowed = box.Entity.IsRefinementAllowed );

            box.IfValidProperty( nameof( box.Entity.Key ),
                () => entity.Key = box.Entity.Key );

            box.IfValidProperty( nameof( box.Entity.MaximumResultsPerQuery ),
                () => entity.MaximumResultsPerQuery = box.Entity.MaximumResultsPerQuery );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.SortExpression ),
                () => entity.SortExpression = box.Entity.SortExpression );

            box.IfValidProperty( nameof( box.Entity.SelectExpression ),
                () => entity.SelectExpression = box.Entity.SelectExpression );

            box.IfValidProperty( nameof( box.Entity.SelectManyExpression ),
                () => entity.SelectManyExpression = box.Entity.SelectManyExpression );

            box.IfValidProperty( nameof( box.Entity.WhereExpression ),
                () => entity.WhereExpression = box.Entity.WhereExpression );

            return true;
        }

        /// <summary>
        /// Validates the EntitySearch for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="entitySearch">The EntitySearch to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the EntitySearch is valid, <c>false</c> otherwise.</returns>
        private bool ValidateEntitySearch( EntitySearch entitySearch, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( !entitySearch.IsValid )
            {
                errorMessage = entitySearch.ValidationResults?.FirstOrDefault()?.ErrorMessage ?? "Entity Search contains invalid data and can't be saved.";
                return false;
            }

            return true;
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EntitySearchService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{EntitySearch.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${EntitySearch.FriendlyTypeName}." );
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

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult GetEntityForPreview( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntity( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                return ActionOk( GetEntityBag( entity ) );
            }
        }

        /// <summary>
        /// Generates a preview of the search query. Copied from the EntitySearchDetail block. Keep both of these up-to-date.
        /// </summary>
        /// <param name="box">The box that contains all the information required.</param>
        /// <returns>The results of the query.</returns>
        [BlockAction]
        public BlockActionResult Preview( DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EntitySearchService( rockContext );

                if ( !TryGetEntity( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateEntitySearch( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                // Never show more than 10 items for a preview.
                if ( !entity.MaximumResultsPerQuery.HasValue || entity.MaximumResultsPerQuery.Value > 10 )
                {
                    entity.MaximumResultsPerQuery = 10;
                }

                var resultsBag = new PreviewResultsBag();

                // Enable query count tracking so we can display it in the UI.
                rockContext.QueryMetricDetailLevel = QueryMetricDetailLevel.Count;

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var results = EntitySearchService.GetSearchResults( entity, null, RequestContext.CurrentPerson, rockContext );
#if REVIEW_WEBFORMS
                resultsBag.Data = Rock.Rest.Utility.BlockUtilities.ToV2ResponseJson( results.Items, true );
#else
                throw new NotImplementedException();
#endif
                sw.Stop();

                resultsBag.Duration = sw.Elapsed.TotalMilliseconds;
                resultsBag.QueryCount = rockContext.QueryCount;

                return ActionOk( resultsBag );
            }
        }

        #endregion
    }
}
