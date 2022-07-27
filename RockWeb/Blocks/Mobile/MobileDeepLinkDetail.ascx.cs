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

using Rock;
using Rock.Common.Mobile;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Mobile
{
    /// <summary>
    /// Add or edit a mobile deep link route for a particular application.
    /// </summary>
    [DisplayName( "Mobile Deep Link Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of mobile deep-link routes." )]
    [Rock.SystemGuid.BlockTypeGuid( "5C157EBD-2482-4393-A309-A872F774E19F" )]
    public partial class MobileDeepLinkDetail : RockBlock
    {
        #region Fields

        /// <summary>
        /// The state if we are modifying an existing deep link route.
        /// </summary>
        private bool _isModifying;

        #endregion

        #region Base Method Overrides

        ///<inheritdoc />
        protected override void OnLoad( EventArgs e )
        {
            // If we are modifying, there will be a page parameter passed containing the Guid of the route that we are modifying.
            Guid? deepLinkGuid = PageParameter( "DeepLinkRouteGuid" ).AsGuidOrNull();
            if ( deepLinkGuid != null )
            {
                _isModifying = true;
            }

            if ( !IsPostBack )
            {
                using ( var context = new RockContext() )
                {
                    var siteService = new SiteService( context );
                    var site = siteService.Get( PageParameter( "SiteId" ) );
                    ConfigureContent( site );
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Configures the content.
        /// </summary>
        /// <param name="site">The site.</param>
        private void ConfigureContent( Site site )
        {
            // Limit the pages in the PagePicker to only show the ones for this mobile application/site.
            ppMobilePage.ItemRestUrlExtraParams = $"?rootPageId={site.DefaultPageId}";

            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
            tbRoute.PrependText = $"/{additionalSettings.DeepLinkPathPrefix}/";
            tbFallbackUrl.Placeholder = "https://example.com/home";

            // If we are modifying a current route.
            if ( _isModifying )
            {
                Guid? deepLinkGuid = PageParameter( "DeepLinkRouteGuid" ).AsGuidOrNull();
                using ( var context = new RockContext() )
                {
                    var siteService = new SiteService( context );
                    var pageService = new PageService( context );

                    var routes = additionalSettings.DeepLinkRoutes;
                    var route = routes.First( r => r.Guid == deepLinkGuid ) ?? null;

                    if ( route == null )
                    {
                        NavigateToParentPage( new Dictionary<string, string>
                        {
                            { "SiteId", PageParameter( "SiteId" ) },
                            { "Tab", "Deep Links" }
                        } );

                        return;
                    }

                    var mobilePage = pageService.Get( route.MobilePageGuid );

                    tbRoute.Text = route.Route;
                    ppMobilePage.SetValue( mobilePage );
                    tglFallbackType.Checked = route.UsesUrlAsFallback;

                    if ( route.UsesUrlAsFallback )
                    {
                        tbFallbackUrl.Text = route.WebFallbackPageUrl;
                    }
                    else
                    {
                        ppFallbackPage.SetValue( pageService.Get( route.WebFallbackPageGuid.Value ) );
                    }
                }

            }

            ToggleFallbackContent();
        }


        /// <summary>
        /// Sets the content of the fallback.
        /// </summary>
        private void ToggleFallbackContent()
        {
            // If the fallback type is set to page.
            var fallbackTypeIsPage = !tglFallbackType.Checked;

            // Toggle the content.
            ppFallbackPage.Visible = fallbackTypeIsPage;
            ppFallbackPage.Required = fallbackTypeIsPage;
            tbFallbackUrl.Visible = !fallbackTypeIsPage;
            tbFallbackUrl.Required = !fallbackTypeIsPage;
        }

        /// <summary>
        /// Saves the deep link.
        /// </summary>
        private void SaveDeepLink()
        {
            using ( var context = new RockContext() )
            {
                var pageService = new PageService( context );
                var siteService = new SiteService( context );

                // Get the site settings for this specific site.
                var site = siteService.Get( PageParameter( "SiteId" ) );
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                // Generate the guid for our route, and get the guid for the mobile page corresponding to it.
                var mobilePageGuid = pageService.GetGuid( ppMobilePage.SelectedValue.AsInteger() );
                Guid guid = Guid.NewGuid();

                // If we are modifying, change our guid var to match the one we are modifying, then delete it from our route list.
                if ( _isModifying )
                {
                    // We actually checked whether this is null in our ConfigureContent(), so we know it exists.
                    guid = PageParameter( "DeepLinkRouteGuid" ).AsGuid();
                    var refRoute = additionalSettings.DeepLinkRoutes.Where( r => r.Guid == guid ).First();
                    additionalSettings.DeepLinkRoutes.Remove( refRoute );
                }

                var routeAlreadyExists = additionalSettings.DeepLinkRoutes.Any( r => r.Route == tbRoute.Text );
                if ( routeAlreadyExists )
                {
                    // TODO: Need to tighten up this check to include potential query parameters
                    nbError.Text = $"The route: '{tbRoute.Text}' already exists. Please choose a non-conflicting route.";
                    return;
                }

                var webFallbackPageGuid = pageService.GetGuid( ppFallbackPage.SelectedValue.AsInteger() );

                var usesUrlAsFallback = tglFallbackType.Checked;

                // Clear the selection of the fallback page picker. 
                if ( usesUrlAsFallback )
                {
                    webFallbackPageGuid = null;
                }
                // Clear the selection of the fallback URL textbox.
                else
                {
                    tbFallbackUrl.Text = string.Empty;
                }

                var route = new DeepLinkRoute
                {
                    Guid = guid,
                    Route = tbRoute.Text.Trim('/'),
                    UsesUrlAsFallback = tglFallbackType.Checked,
                    MobilePageGuid = mobilePageGuid.Value,
                    WebFallbackPageGuid = webFallbackPageGuid,
                    WebFallbackPageUrl = tbFallbackUrl.Text
                };

                // Add the route and save the settings.
                additionalSettings.DeepLinkRoutes.Add( route );
                site.AdditionalSettings = additionalSettings.ToJson();
                context.SaveChanges();

                NavigateToParentPage( new Dictionary<string, string>
                {
                    { "SiteId", site.Id.ToString() },
                    { "Tab", "Deep Links" }
                } );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SaveDeepLink();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage( new Dictionary<string, string>
                {
                    { "SiteId", PageParameter( "SiteId" ) },
                    { "Tab", "Deep Links" }
                } );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglFallbackType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglFallbackType_CheckedChanged( object sender, EventArgs e )
        {
            ToggleFallbackContent();
        }

        #endregion
    }
}