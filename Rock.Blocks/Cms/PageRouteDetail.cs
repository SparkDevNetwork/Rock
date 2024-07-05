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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageRouteDetail;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular page route.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Route Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular page route." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3ed3c7c7-70b8-416b-819c-353011204f51" )]
    [Rock.SystemGuid.BlockTypeGuid( "e4302549-0bb8-4dde-9b9a-70f0b665e76f" )]
    public class PageRouteDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PageRouteId = "PageRouteId";
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
                var box = new DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<PageRoute>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageRouteDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new PageRouteDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the PageRoute for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="pageRoute">The PageRoute to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PageRoute is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePageRoute( PageRoute pageRoute, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( pageRoute.PageId == 0 )
            {
                errorMessage = "Page is required";
                return false;
            }

            int? siteId = null;
            var entityService = new PageRouteService( rockContext );
            var pageCache = Web.Cache.PageCache.Get( pageRoute.PageId );
            if ( pageCache != null && pageCache.Layout != null )
            {
                siteId = pageCache.Layout.SiteId;
            }

            var duplicateRoutes = entityService
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.Route == pageRoute.Route &&
                    r.Id != pageRoute.Id );

            if ( siteId.HasValue )
            {
                duplicateRoutes = duplicateRoutes
                    .Where( r =>
                        r.Page != null &&
                        r.Page.Layout != null &&
                        r.Page.Layout.SiteId == siteId.Value );
            }

            if ( duplicateRoutes.Any() )
            {
                errorMessage = "There is already an existing route with this name for the selected page's site. Route names must be unique per site. Please choose a different route name.";
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
        private void SetBoxInitialEntityState( DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {PageRoute.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized(Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PageRoute.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PageRoute.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PageRouteBag"/> that represents the entity.</returns>
        private PageRouteBag GetCommonEntityBag( PageRoute entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PageRouteBag
            {
                IdKey = entity.IdKey,
                IsGlobal = entity.IsGlobal,
                IsSystem = entity.IsSystem,
                Page = entity.Page.ToListItemBag(),
                Route = entity.Route,
                Site = GetPageSite( entity.PageId )
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="PageRouteBag"/> that represents the entity.</returns>
        private PageRouteBag GetEntityBagForView( PageRoute entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the page site for the Page
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        private string GetPageSite( int pageId )
        {
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Get( pageId );

            if ( page != null && page.Layout != null && page.Layout.Site != null )
            {
                return page.Layout.Site.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="PageRouteBag"/> that represents the entity.</returns>
        private PageRouteBag GetEntityBagForEdit( PageRoute entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( PageRoute entity, DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.IsGlobal ),
                () => entity.IsGlobal = box.Entity.IsGlobal );

            box.IfValidProperty( nameof( box.Entity.Page ),
                () => entity.PageId = box.Entity.Page.GetEntityId<Page>( rockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Entity.Route ),
                () => entity.Route = box.Entity.Route );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="PageRoute"/> to be viewed or edited on the page.</returns>
        private PageRoute GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<PageRoute, PageRouteService>( rockContext, PageParameterKey.PageRouteId );
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

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( PageRoute entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out PageRoute entity, out BlockActionResult error )
        {
            var entityService = new PageRouteService( rockContext );
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
                entity = new PageRoute();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PageRoute.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${PageRoute.FriendlyTypeName}." );
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

                // Ensure everything is valid before saving.
                if ( !ValidatePageRoute( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
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
                if ( !ValidatePageRoute( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                var pageCache = Web.Cache.PageCache.Get( entity.PageId );

                if ( pageCache != null )
                {
                    PageCache.FlushPage( pageCache.Id );
                }

                Rock.Web.RockRouteHandler.ReregisterRoutes();

                return ActionOk( this.GetParentPageUrl() );
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
                var entityService = new PageRouteService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<PageRouteBag, PageRouteDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        /// <summary>
        /// Gets the name of the site the page is found on.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetSiteName( Guid? guid )
        {
            string siteName = string.Empty;

            if ( guid.HasValue )
            {
                Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Get( guid.Value );

                if ( page != null && page.Layout != null && page.Layout.Site != null )
                {
                    siteName = page.Layout.Site.Name;
                }
            }

            return ActionOk( new {
                siteName = siteName
            } );
        }

        #endregion
    }
}
