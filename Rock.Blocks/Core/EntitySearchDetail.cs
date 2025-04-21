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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.EntitySearchDetail;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular entity search.
    /// </summary>

    [DisplayName( "Entity Search Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular entity search." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "db9f0335-91cb-4f89-a3bd-c084829798c6" )]
    [Rock.SystemGuid.BlockTypeGuid( "eb07313e-a0f6-4eb7-bdd1-6e5a22d456ff" )]
    public class EntitySearchDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EntitySearchId = "EntitySearchId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );

                return box;
            }
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var key = pageReference.GetPageParameter( PageParameterKey.EntitySearchId );
                var pageParameters = new Dictionary<string, string>();

                var name = EntitySearchCache.Get( key, true )?.Name;

                if ( name != null )
                {
                    pageParameters.Add( PageParameterKey.EntitySearchId, key );
                }

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                var breadCrumb = new BreadCrumbLink( name ?? "New Entity Search", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
                };
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private EntitySearchDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new EntitySearchDetailOptionsBag();

            return options;
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

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {EntitySearch.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( EntitySearch.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( EntitySearch.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="EntitySearchBag"/> that represents the entity.</returns>
        private EntitySearchBag GetCommonEntityBag( EntitySearch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new EntitySearchBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                IsActive = entity.IsActive,
                IsEntitySecurityEnabled = entity.IsEntitySecurityEnabled,
                IsRefinementAllowed = entity.IsRefinementAllowed,
                Key = entity.Key,
                MaximumResultsPerQuery = entity.MaximumResultsPerQuery,
                Name = entity.Name
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="EntitySearchBag"/> that represents the entity.</returns>
        private EntitySearchBag GetEntityBagForView( EntitySearch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="EntitySearchBag"/> that represents the entity.</returns>
        private EntitySearchBag GetEntityBagForEdit( EntitySearch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.IncludePaths = entity.IncludePaths;
            bag.GroupByExpression = entity.GroupByExpression;
            bag.SortExpression = entity.SortExpression;
            bag.SelectExpression = entity.SelectExpression;
            bag.SelectManyExpression = entity.SelectManyExpression;
            bag.WhereExpression = entity.WhereExpression;

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

            box.IfValidProperty( nameof(box.Entity.IncludePaths ), () =>
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
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="EntitySearch"/> to be viewed or edited on the page.</returns>
        private EntitySearch GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<EntitySearch, EntitySearchService>( rockContext, PageParameterKey.EntitySearchId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( EntitySearch entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out EntitySearch entity, out BlockActionResult error )
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

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var box = new DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EntitySearchService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
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

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.EntitySearchId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EntitySearchService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        /// <summary>
        /// Generates a preview of the search query.
        /// </summary>
        /// <param name="box">The box that contains all the information required.</param>
        /// <returns>The results of the query.</returns>
        [BlockAction]
        public BlockActionResult Preview( DetailBlockBox<EntitySearchBag, EntitySearchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EntitySearchService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
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
                resultsBag.Data = Rock.Rest.Utility.BlockUtilities.ToV2ResponseJson( results.Items, true );
                sw.Stop();

                resultsBag.Duration = sw.Elapsed.TotalMilliseconds;
                resultsBag.QueryCount = rockContext.QueryCount;

                return ActionOk( resultsBag );
            }
        }

        #endregion
    }
}
