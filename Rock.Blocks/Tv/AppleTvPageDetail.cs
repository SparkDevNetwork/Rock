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
using Rock.ViewModels.Blocks.Tv.AppleTvPageDetail;
using Rock.ViewModels.Utility;
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
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d8419b3c-eda1-46fc-9810-b1d81fb37cb3" )]
    [Rock.SystemGuid.BlockTypeGuid( "adbf3377-a491-4016-9375-346496a25fb4" )]
    public class AppleTvPageDetail : RockEntityDetailBlockType<Page, AppleTvPageBag>, IBreadCrumbBlock
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
            var box = new DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AppleTvPageDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AppleTvPageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Page for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="page">The page to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Page is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePage( Page page, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Page.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Page.FriendlyTypeName );
            }

            PrepareDetailBox( box, entity );
        }

        /// <inheritdoc/>
        protected override AppleTvPageBag GetEntityBagForView( Page entity )
        {
            return GetEntityBagForEdit( entity );
        }

        /// <inheritdoc/>
        protected override AppleTvPageBag GetEntityBagForEdit( Page entity )
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

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Page entity, ValidPropertiesBox<AppleTvPageBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () =>
                {
                    entity.InternalName = box.Bag.Name;
                    entity.BrowserTitle = box.Bag.Name;
                    entity.PageTitle = box.Bag.Name;
                } );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.ShowInMenu ),
                () => entity.DisplayInNavWhen = box.Bag.ShowInMenu ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never );

            box.IfValidProperty( nameof( box.Bag.PageTVML ),
                () => UpdatePageResponseContent( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.RockCacheability ),
                () => UpdateCacheability( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
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

        /// <inheritdoc/>
        protected override Page GetInitialEntity()
        {
            return GetInitialEntity<Page, PageService>( RockContext, PageParameterKey.SitePageId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>()
                {
                        { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) }
                } )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Page entity, out BlockActionResult error )
        {
            var entityService = new PageService( RockContext );
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
        public BlockActionResult Save( ValidPropertiesBox<AppleTvPageBag> box )
        {
            var applicationId = PageParameter( PageParameterKey.SiteId );
            var entityService = new PageService( RockContext );
            var site = SiteCache.Get( applicationId, !PageCache.Layout.Site.DisablePredictableIds );

            if ( site == null )
            {
                return ActionBadRequest( "Please provide the Apple Application this page belongs to." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidatePage( entity, out var validationMessage ) )
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

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                RockContext.SaveChanges();
            } );

            RockContext.SaveChanges();

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<AppleTvPageBag>()
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        #endregion
    }
}
