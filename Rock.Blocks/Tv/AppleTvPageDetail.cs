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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tv;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.AppleTvAppDetail;
using Rock.ViewModels.Blocks.Tv.AppleTvPageDetail;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Allows a person to edit an Apple TV application.
    /// </summary>

    [DisplayName( "Apple TV Page Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV page." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d8419b3c-eda1-46fc-9810-b1d81fb37cb3" )]
    [Rock.SystemGuid.BlockTypeGuid( "adbf3377-a491-4016-9375-346496a25fb4" )]
    public class AppleTvPageDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string SitePageId = "SitePageId";
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
                var box = new DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Page>();

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
        private AppleTvPageDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AppleTvPageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Page for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="page">The page to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Page is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePage( Page page, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Page.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Page.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="AppleTvPageBag"/> that represents the entity.</returns>
        private AppleTvPageBag GetEntityBagForEdit( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var response = entity.GetAdditionalSettings<AppleTvPageSettings>();
            var cacheability = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();

            var bag = new AppleTvPageBag
            {
                Description = entity.Description,
                IdKey = entity.IdKey,
                Name = entity.InternalName,
                ShowInMenu = entity.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed,
                PageTVML = response.Content,
                IsSystem = entity.IsSystem,
                RockCacheability = cacheability.ToCacheabilityBag(),
                PageGuid = entity.Guid.ToString()
            };

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
        private bool UpdateEntityFromBox( Page entity, DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
                () =>
                {
                    entity.InternalName = box.Entity.Name;
                    entity.BrowserTitle = box.Entity.Name;
                    entity.PageTitle = box.Entity.Name;
                } );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.ShowInMenu ),
                () => entity.DisplayInNavWhen = box.Entity.ShowInMenu ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never );

            box.IfValidProperty( nameof( box.Entity.PageTVML ),
                () => UpdatePageResponseContent( box.Entity, entity ) );

            box.IfValidProperty( nameof( box.Entity.RockCacheability ),
                () => UpdateCacheability( box.Entity, entity ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Updates the <see cref="Page.CacheControlHeaderSettings"/> with the <see cref="AppleTvPageBag.RockCacheability"/>.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private static void UpdateCacheability( AppleTvPageBag bag, Page entity )
        {
            var cacheability = bag.RockCacheability.ToCacheability();
            entity.CacheControlHeaderSettings = cacheability?.ToJson();
        }

        /// <summary>
        /// Updates the <see cref="Page.AdditionalSettings"/> with the <see cref="AppleTvPageBag.PageTVML"/>.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private static void UpdatePageResponseContent( AppleTvPageBag bag, Page entity )
        {
            var pageResponse = entity.GetAdditionalSettings<AppleTvPageSettings>();
            pageResponse.Content = bag.PageTVML;
            entity.SetAdditionalSettings( pageResponse );
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Page"/> to be viewed or edited on the page.</returns>
        private Page GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Page, PageService>( rockContext, PageParameterKey.SitePageId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>()
            {
                { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
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
        private string GetSecurityGrantToken( Page entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Page entity, out BlockActionResult error )
        {
            var entityService = new PageService( rockContext );
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
                entity = new Page();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Page.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var key = pageReference.GetPageParameter( PageParameterKey.SitePageId );
                var breadCrumbs = new List<IBreadCrumb>();

                if ( key.IsNotNullOrWhiteSpace() )
                {
                    var pageParameters = new Dictionary<string, string>();
                    var detailBreadCrumb = pageReference.BreadCrumbs.Find( x => x.Name == "Application Screen Detail" );
                    if ( detailBreadCrumb != null )
                    {
                        pageReference.BreadCrumbs.Remove( detailBreadCrumb );
                    }

                    var page = PageCache.Get( key, true );

                    var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                    var breadCrumb = new BreadCrumbLink( page?.InternalName ?? "New Page", breadCrumbPageRef );

                    breadCrumbs.Add( breadCrumb );
                }

                return new BreadCrumbResult
                {
                    BreadCrumbs = breadCrumbs
                };
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box )
            {
                using ( var rockContext = new RockContext() )
                {
                    var applicationId = PageParameter( PageParameterKey.SiteId );
                    var entityService = new PageService( rockContext );
                    var site = SiteCache.Get( applicationId, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( site == null )
                    {
                        return ActionBadRequest( "Please provide the Apple Application this page belongs to." );
                    }

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
                    if ( !ValidatePage( entity, rockContext, out var validationMessage ) )
                    {
                        return ActionBadRequest( validationMessage );
                    }

                    var isNew = entity.Id == 0;

                    if ( isNew )
                    {
                        entity.ParentPageId = site.DefaultPageId;
                        entity.LayoutId = site.DefaultPage.LayoutId;

                        // Set the order of the new page to be the last one
                        var currentMaxOrder = entityService.GetByParentPageId( site.DefaultPageId )
                            .OrderByDescending( p => p.Order )
                            .Select( p => p.Order )
                            .FirstOrDefault();
                        entity.Order = currentMaxOrder + 1;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        entity.SaveAttributeValues( rockContext );

                        rockContext.SaveChanges();
                    } );

                    rockContext.SaveChanges();

                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    } ) );
                }
            }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box )
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

                    var refreshedBox = new DetailBlockBox<AppleTvPageBag, AppleTvAppDetailOptionsBag>
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

        #endregion
    }
}
