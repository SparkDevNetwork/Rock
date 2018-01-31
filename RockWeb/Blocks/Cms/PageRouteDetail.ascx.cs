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
using System.Web.Routing;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Route Detail")]
    [Category("CMS")]
    [Description("Displays the details of a page route.")]
    public partial class PageRouteDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbErrorMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "pageRouteId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PageRoute pageRoute;
            var rockContext = new RockContext();
            PageRouteService pageRouteService = new PageRouteService( rockContext );

            int pageRouteId = int.Parse( hfPageRouteId.Value );

            if ( pageRouteId == 0 )
            {
                pageRoute = new PageRoute();
                pageRouteService.Add( pageRoute );
            }
            else
            {
                pageRoute = pageRouteService.Get( pageRouteId );
            }

            pageRoute.Route = tbRoute.Text.Trim();
            int selectedPageId = int.Parse( ppPage.SelectedValue );
            pageRoute.PageId = selectedPageId;

            if ( !pageRoute.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            int? siteId = null;
            var pageCache = PageCache.Read( selectedPageId );
            if ( pageCache != null && pageCache.Layout != null )
            {
                siteId = pageCache.Layout.SiteId;
            }

            var duplicateRoutes = pageRouteService
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
                // Duplicate
                nbErrorMessage.Title = "Duplicate Route";
                nbErrorMessage.Text = "<p>There is already an existing route with this name for the selected page's site. Route names must be unique per site. Please choose a different route name.</p>";
                nbErrorMessage.Visible = true;
            }
            else
            {
                rockContext.SaveChanges();

                // Remove previous route
                var oldRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteIds().Contains( pageRoute.Id ) );
                if ( oldRoute != null )
                {
                    var pageAndRouteIds = oldRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                    pageAndRouteIds = pageAndRouteIds.Where( p => p.RouteId != pageRoute.Id ).ToList();
                    if ( pageAndRouteIds.Any() )
                    {
                        oldRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                    }
                    else
                    {
                        RouteTable.Routes.Remove( oldRoute );
                    }
                }

                // Remove the '{shortlink}' route (will be added back after specific routes)
                var shortLinkRoute = RouteTable.Routes.OfType<Route>().Where( r => r.Url == "{shortlink}" ).FirstOrDefault();
                if ( shortLinkRoute != null )
                {
                    RouteTable.Routes.Remove( shortLinkRoute );
                }

                // Add new route
                var pageAndRouteId = new Rock.Web.PageAndRouteId { PageId = pageRoute.PageId, RouteId = pageRoute.Id };
                var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( r => r.Url == pageRoute.Route );
                if ( existingRoute != null )
                {
                    var pageAndRouteIds = existingRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                    pageAndRouteIds.Add( pageAndRouteId );
                    existingRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                }
                else
                {
                    var pageAndRouteIds = new List<Rock.Web.PageAndRouteId>();
                    pageAndRouteIds.Add( pageAndRouteId );
                    RouteTable.Routes.AddPageRoute( pageRoute.Route, pageAndRouteIds );
                }

                RouteTable.Routes.Add( new Route( "{shortlink}", new Rock.Web.RockRouteHandler() ) );

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectItem event of the ppPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPage_SelectItem( object sender, EventArgs e )
        {
            ShowSite();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="routeId">The route identifier.</param>
        public void ShowDetail( int routeId )
        {
            pnlDetails.Visible = true;

            PageRoute pageRoute = null;

            if ( !routeId.Equals( 0 ) )
            {
                pageRoute = new PageRouteService( new RockContext() ).Get( routeId );
                lActionTitle.Text = ActionTitle.Edit( PageRoute.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( pageRoute, ResolveRockUrl( "~" ) );
            }

            if (pageRoute == null)
            {
                pageRoute = new PageRoute { Id = 0 };
                lActionTitle.Text = ActionTitle.Add(PageRoute.FriendlyTypeName).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfPageRouteId.Value = pageRoute.Id.ToString();
            ppPage.SetValue( pageRoute.Page );

            ShowSite();

            tbRoute.Text = pageRoute.Route;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PageRoute.FriendlyTypeName );
            }

            if ( pageRoute.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( PageRoute.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( PageRoute.FriendlyTypeName ).FormatAsHtmlTitle();
                btnCancel.Text = "Close";
            }

            ppPage.Enabled = !readOnly;
            tbRoute.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        private void ShowSite()
        {
            lSite.Text = string.Empty;

            int? pageId = ppPage.SelectedValueAsInt();
            if ( pageId.HasValue )
            {
                var page = PageCache.Read( pageId.Value );
                if ( page != null && page.Layout != null && page.Layout.Site != null )
                {
                    lSite.Text = page.Layout.Site.Name;
                }
            }
        }

        #endregion
    }
}