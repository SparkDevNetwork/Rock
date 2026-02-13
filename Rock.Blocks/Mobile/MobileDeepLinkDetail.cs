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
using Rock.Common.Mobile;
using Rock.Constants;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Mobile.MobileDeepLinkDetail;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// Displays the details of a particular mobile deep-link.
    /// </summary>
    [DisplayName( "Mobile Deep Link Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of mobile deep-link routes." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "4C323181-AAEC-423F-8679-5280F9C9B168" )]
    // was [SystemGuid.BlockTypeGuid( "DB6CFD8E-9FC3-40AE-B570-4863853BBEB0" )]
    [Rock.SystemGuid.BlockTypeGuid( "5C157EBD-2482-4393-A309-A872F774E19F" )]
    public class MobileDeepLinkDetail : RockDetailBlockType
    {
        #region Fields

        private string _friendlyName = "Mobile Deep Link Detail";

        #endregion Fields

        #region Properties

        private SiteService SiteService => new SiteService( RockContext );

        #endregion Properties

        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string DeepLinkRouteGuid = "DeepLinkRouteGuid";
            public const string AutoEdit = "AutoEdit";
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
            int? siteId = SiteService
                .GetQueryableByKey( PageParameter( PageParameterKey.SiteId ), !PageCache.Layout.Site.DisablePredictableIds )
                .Select( p => ( int? ) p.Id )
                .FirstOrDefault();

            var deepLinkRouteGuid = PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuidOrNull();

            // Pull the site, settings, and route to ensure they exists and we have permission to view/edit them.
            var site = SiteService.Get( siteId ?? 0 );
            if ( site == null )
            {
                return new MobileDeepLinkDetailInitializationBox
                {
                    ErrorMessage = "Site not found."
                };
            }
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
            var route = additionalSettings?.DeepLinkRoutes?.FirstOrDefault( r => r.Guid == deepLinkRouteGuid ) ?? null;


            var box = new MobileDeepLinkDetailInitializationBox();

            SetBoxInitialEntityState( box, site, additionalSettings );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( MobileDeepLinkDetailInitializationBox box, Site site, AdditionalSiteSettings additionalSettings )
        {
            DeepLinkRoute mdlRoute;
            var mdlRoutes = additionalSettings.DeepLinkRoutes;
            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // If Adding new deep link, short circuit
            if ( PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuidOrNull() == null )
            {
                if ( box.IsEditable )
                {
                    box.Bag = new MobileDeepLinkDetailBag();
                    box.Bag.PathPrefix = additionalSettings.DeepLinkPathPrefix;
                    return;
                }
            }

            try
            {
                mdlRoute = mdlRoutes.First( route => route.Guid == PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuid() );
            }
            catch ( InvalidOperationException ) // If guid is invalid guid or not associated with any route for the site.
            {
                box.ErrorMessage = $"The {_friendlyName} with the specified Guid was not found.";
                return;
            }

            var mobileDeepLink = new MobileDeepLink
            {
                SiteId = site.Id,
                RouteGuid = mdlRoute.Guid,
                Route = mdlRoute.Route,
                MobilePageGuid = mdlRoute.MobilePageGuid,
                UsesUrlAsFallback = mdlRoute.UsesUrlAsFallback,
                WebFallbackPageGuid = mdlRoute.WebFallbackPageGuid,
                WebFallbackPageUrl = mdlRoute.WebFallbackPageUrl,
            };

            // New entity is being created, default to edit as only choice.
            if ( box.IsEditable && PageParameter( PageParameterKey.AutoEdit ).AsBooleanOrNull() == true )
            {
                box.Bag = GetDetailBagForEdit( mobileDeepLink, additionalSettings.DeepLinkPathPrefix );
            }
            else
            {
                if ( isViewable )
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( _friendlyName );
                    box.Bag = GetDetailBagForView( mobileDeepLink, additionalSettings.DeepLinkPathPrefix );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( _friendlyName );
                    return;
                }
            }

            if ( mobileDeepLink != null && box.ErrorMessage.IsNullOrWhiteSpace() )
            {
                var grant = new Rock.Security.SecurityGrant();

                if ( mobileDeepLink is IHasAttributes attributedEntity )
                {
                    grant.AddRulesForAttributes( attributedEntity, RequestContext.CurrentPerson );
                }

                box.SecurityGrantToken = grant.ToToken();
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    ["SiteId"] = PageParameter( PageParameterKey.SiteId ),
                    ["Tab"] = "Deep Links",
                }
                )
            };
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private MobileDeepLinkDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new MobileDeepLinkDetailOptionsBag();

            options.RootPageGuid = SiteService
                .Get( RequestContext.GetPageParameter( PageParameterKey.SiteId ).AsInteger() )
                .DefaultPage
                .Guid;

            return options;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="mobileDeepLink">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MobileDeepLinkDetailBag"/> that represents the entity.</returns>
        private MobileDeepLinkDetailBag GetCommonEntityBag( MobileDeepLink mobileDeepLink, string pathPrefix )
        {
            if ( mobileDeepLink == null )
            {
                return null;
            }

            PageRouteValueBag mobilePage = GetMobilePageRouteValueBag( mobileDeepLink );

            PageRouteValueBag webFallbackPage = null;
            var webFallbackUrl = string.Empty;

            if ( mobileDeepLink.UsesUrlAsFallback )
            {
                webFallbackPage = null;
                webFallbackUrl = mobileDeepLink.WebFallbackPageUrl;
            }
            else
            {
                webFallbackPage = GetWebFallbackPageRouteValueBag( mobileDeepLink );
                webFallbackUrl = null;
            }

            return new MobileDeepLinkDetailBag
            {
                SiteId = mobileDeepLink.SiteId,
                RouteGuid = mobileDeepLink.RouteGuid.Value,
                PathPrefix = pathPrefix,
                Route = mobileDeepLink.Route,
                MobilePage = mobilePage,
                UsesUrlAsFallback = mobileDeepLink.UsesUrlAsFallback,
                WebFallbackPage = webFallbackPage,
                WebFallbackPageUrl = webFallbackUrl,
            };
        }

        /// <summary>
        /// Gets the entity bag for view mode.
        /// </summary>
        /// <param name="mobileDeepLink">The mobile deep link.</param>
        /// <returns>
        /// A <see cref="MobileDeepLinkDetailBag"/> representing the mobile deep link for view mode, or <c>null</c> if the mobile deep link is <c>null</c>.
        /// </returns>
        protected MobileDeepLinkDetailBag GetDetailBagForView( MobileDeepLink mobileDeepLink, string pathPrefix )
        {
            if ( mobileDeepLink == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( mobileDeepLink, pathPrefix );

            return bag;
        }

        /// <summary>
        /// Gets the entity bag for edit mode.
        /// </summary>
        /// <param name="mobileDeepLink">The mobile deep link.</param>
        /// <returns>
        /// A <see cref="MobileDeepLinkDetailBag"/> representing the mobile deep link for edit mode, or <c>null</c> if the mobile deep link is <c>null</c>.
        /// </returns>
        protected MobileDeepLinkDetailBag GetDetailBagForEdit( MobileDeepLink mobileDeepLink, string pathPrefix )
        {
            if ( mobileDeepLink == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( mobileDeepLink, pathPrefix );

            return bag;
        }

        /// <summary>
        /// Validates the MobileDeepLink for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="MobileDeepLink">The MobileDeepLink to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the MobileDeepLink is valid, <c>false</c> otherwise.</returns>
        private bool ValidateMobileDeepLinkDetailBag( MobileDeepLinkDetailBag bag, out string errorMessage )
        {
            errorMessage = null;

            bool isUrlUsedAsFallback = bag.UsesUrlAsFallback;
            bool isFallbackPageUrlValid = bag.WebFallbackPageUrl != null
                                          && bag.WebFallbackPageUrl.IsNullOrWhiteSpace() == false
                                          && string.IsNullOrEmpty( bag.WebFallbackPageUrl ) == false;
            bool isFallbackPageGuidValid = bag.WebFallbackPage != null
                                           && bag.WebFallbackPage.Page != null
                                           && Guid.TryParse( bag.WebFallbackPage.Page.Value, out _ )
                                           && bag.WebFallbackPage.Page.Value.AsGuidOrNull() != null
                                           && bag.WebFallbackPage.Page.Value.AsGuidOrNull() != Guid.Empty;

            // Ensure everything is valid before saving.
            if ( isUrlUsedAsFallback && !isFallbackPageUrlValid )
            {
                errorMessage = "The fallback toggle was set to URL, but a Fallback URL was not provided.";
                return false;
            }

            if ( !isUrlUsedAsFallback && !isFallbackPageGuidValid )
            {
                errorMessage = "The fallback toggle was set to Page, but a Fallback Page was not selected.";
                return false;
            }

            if ( bag.Route.IsNullOrWhiteSpace() || string.IsNullOrEmpty( bag.Route ) )
            {
                errorMessage = "A Route is required.";
                return false;
            }

            if ( bag.MobilePage.Page.Value.AsGuidOrNull() == null || bag.MobilePage.Page.Value.AsGuidOrNull() == Guid.Empty )
            {
                errorMessage = "A Mobile Page is required.";
                return false;
            }

            return true;
        }

        private BlockActionResult EditDeepLink( string routeGuid )
        {
            var bag = new MobileDeepLinkDetailBag();

            var pageService = new PageService( RockContext );

            var site = SiteService.Get( PageParameter( PageParameterKey.SiteId ) );

            if ( site == null )
            {
                return ActionBadRequest( "Site not found." );
            }

            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( additionalSettings == null )
            {
                return ActionBadRequest( "Additional Settings for site not found." );
            }

            var deepLinkGuid = routeGuid.AsGuidOrNull();
            var routes = additionalSettings.DeepLinkRoutes;

            var route = ( DeepLinkRoute ) null;
            try
            {
                route = routes.First( r => r.Guid == deepLinkGuid ) ?? null;
            }
            catch ( InvalidOperationException )
            {
                return ActionBadRequest( "The Mobile Deep Link with specified GUID was not found." );
            }

            if ( route == null )
            {
                return ActionBadRequest( "The Mobile Deep Link with specified GUID was not found." );
            }

            var mobileDeepLink = new MobileDeepLink
            {
                SiteId = site.Id,
                RouteGuid = route.Guid,
                Route = route.Route,
                MobilePageGuid = route.MobilePageGuid,
                UsesUrlAsFallback = route.UsesUrlAsFallback,
                WebFallbackPageGuid = route.WebFallbackPageGuid,
                WebFallbackPageUrl = route.WebFallbackPageUrl,
            };

            bag.SiteId = site.Id;
            bag.RouteGuid = route.Guid;
            bag.PathPrefix = additionalSettings.DeepLinkPathPrefix;
            bag.Route = route.Route;
            bag.MobilePage = GetMobilePageRouteValueBag( mobileDeepLink );
            bag.UsesUrlAsFallback = route.UsesUrlAsFallback;

            if ( mobileDeepLink.UsesUrlAsFallback )
            {
                bag.WebFallbackPage = null;
                bag.WebFallbackPageUrl = route.WebFallbackPageUrl;
            }
            else
            {
                bag.WebFallbackPage = GetWebFallbackPageRouteValueBag( mobileDeepLink );
                bag.WebFallbackPageUrl = null;
            }

            return ActionOk( new ValidPropertiesBox<MobileDeepLinkDetailBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the deep link.
        /// </summary>
        private BlockActionResult SaveDeepLink( MobileDeepLinkDetailBag bag )
        {
            if ( ValidateMobileDeepLinkDetailBag( bag, out string errorMessage ) == false )
            {
                return ActionBadRequest( errorMessage );
            }

            var pageService = new PageService( RockContext );

            // Get the site settings for this specific site.
            var site = SiteService.Get( PageParameter( PageParameterKey.SiteId ) );
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            // Generate the guid for our route, and get the guid for the mobile page corresponding to it.
            Guid guid = Guid.NewGuid();

            // If page parameters contains Guid, change our guid var to match the one we are modifying,
            // then delete it from our route list, so we don't make a duplicate
            if ( PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuidOrNull() != null )
            {
                // We actually checked whether this is null in our ConfigureContent(), so we know it exists.
                guid = PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuid();
                var refRoute = additionalSettings.DeepLinkRoutes.Where( r => r.Guid == guid ).First();
                additionalSettings.DeepLinkRoutes.Remove( refRoute );
            }

            var routeAlreadyExists = additionalSettings.DeepLinkRoutes.Any( r => r.Route == bag.Route );
            if ( guid != PageParameter( PageParameterKey.DeepLinkRouteGuid ).AsGuid() && routeAlreadyExists )
            {
                return ActionBadRequest( $"The route: '{bag.Route}' already exists. Please choose a non-conflicting route." );
            }

            Guid? webFallbackPageGuid;
            string webFallbackPageUrl;

            // Clear the selection of the fallback page picker. 
            if ( bag.UsesUrlAsFallback )
            {
                webFallbackPageGuid = null;
                webFallbackPageUrl = bag.WebFallbackPageUrl;
            }
            // Clear the selection of the fallback URL textbox.
            else
            {
                webFallbackPageGuid = bag.WebFallbackPage.Page.Value.AsGuidOrNull();
                webFallbackPageUrl = string.Empty;
            }

            var route = new DeepLinkRoute
            {
                Guid = guid,
                Route = bag.Route.Trim( '/' ),
                UsesUrlAsFallback = bag.UsesUrlAsFallback,
                MobilePageGuid = bag.MobilePage.Page.Value.AsGuid(),
                WebFallbackPageGuid = webFallbackPageGuid,
                WebFallbackPageUrl = webFallbackPageUrl
            };

            // Add the route and save the settings.
            additionalSettings.DeepLinkRoutes.Add( route );
            site.AdditionalSettings = additionalSettings.ToJson();
            RockContext.SaveChanges();

            return ActionOk();
        }

        private static PageRouteValueBag GetMobilePageRouteValueBag( MobileDeepLink mobileDeepLink )
        {
            var mobilePageCache = PageCache.Get( mobileDeepLink.MobilePageGuid.Value );
            PageRouteValueBag mobilePage = new PageRouteValueBag
            {
                Page = new ListItemBag
                {
                    Value = mobileDeepLink.MobilePageGuid.ToString(),
                    Text = mobilePageCache.InternalName,
                },
                Route = new ListItemBag { },
            };
            return mobilePage;
        }

        private static PageRouteValueBag GetWebFallbackPageRouteValueBag( MobileDeepLink mobileDeepLink )
        {
            var webFallbackPageCache = PageCache.Get( mobileDeepLink.WebFallbackPageGuid.Value );
            PageRouteValueBag webFallbackPage = new PageRouteValueBag
            {
                Page = new ListItemBag
                {
                    Value = mobileDeepLink.WebFallbackPageGuid.ToString(),
                    Text = webFallbackPageCache.InternalName,
                },
                Route = new ListItemBag { },
            };
            return webFallbackPage;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="routeGuid">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string routeGuid )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var allowedAuthorizations = new[] { Authorization.EDIT, Authorization.ADMINISTRATE };

            if ( allowedAuthorizations.Any( auth => BlockCache.IsAuthorized( auth, currentPerson ) ) )
            {
                return EditDeepLink( routeGuid );
            }

            return ActionForbidden( "You do not have permission to modify this block." );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<MobileDeepLinkDetailBag> box )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var allowedAuthorizations = new[] { Authorization.EDIT, Authorization.ADMINISTRATE };

            if ( allowedAuthorizations.Any( auth => BlockCache.IsAuthorized( auth, currentPerson ) ) )
            {
                return SaveDeepLink( box.Bag );
            }

            return ActionForbidden( "You do not have permission to modify this block." );
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// Represents a mobile deep link entity with properties for site, route, and fallback options.
        /// </summary>
        public class MobileDeepLink
        {
            /// <summary>
            /// Gets or sets the identifier of the site.
            /// </summary>
            public int SiteId { get; set; }

            /// <summary>
            /// Gets or sets the GUID of the deep link route.
            /// </summary>
            public Guid? RouteGuid { get; set; }

            /// <summary>
            /// Gets or sets the route for the deep link.
            /// </summary>
            public string Route { get; set; }

            /// <summary>
            /// Gets or sets the GUID of the mobile page.
            /// </summary>
            public Guid? MobilePageGuid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the URL is used as a fallback.
            /// </summary>
            public bool UsesUrlAsFallback { get; set; }

            /// <summary>
            /// Gets or sets the GUID of the web fallback page.
            /// </summary>
            public Guid? WebFallbackPageGuid { get; set; }

            /// <summary>
            /// Gets or sets the URL of the web fallback page.
            /// </summary>
            public string WebFallbackPageUrl { get; set; }
        }

        #endregion Helper Classes
    }
}
