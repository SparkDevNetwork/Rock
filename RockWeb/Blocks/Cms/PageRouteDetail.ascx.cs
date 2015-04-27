// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;
using System.ComponentModel;
using Rock.Security;
using Rock.Data;

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
            }

            if (pageRoute == null)
            {
                pageRoute = new PageRoute { Id = 0 };
                lActionTitle.Text = ActionTitle.Add(PageRoute.FriendlyTypeName).FormatAsHtmlTitle();
            }

            hfPageRouteId.Value = pageRoute.Id.ToString();
            ppPage.SetValue( pageRoute.Page );
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

            if ( pageRouteService.Queryable().Any( r => r.Route == pageRoute.Route && r.Id != pageRoute.Id ) )
            {
                // Duplicate
                nbErrorMessage.Title = "Duplicate Route";
                nbErrorMessage.Text = "<p>There is already an existing route with this name and route names must be unique. Please choose a different route name.</p>";
                nbErrorMessage.Visible = true;
            }
            else
            {
                rockContext.SaveChanges();

                // new or updated route
                var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteId() == pageRoute.Id );
                if ( existingRoute != null )
                {
                    RouteTable.Routes.Remove( existingRoute );
                }

                RouteTable.Routes.AddPageRoute( pageRoute );

                NavigateToParentPage();
            }
        }

        #endregion
    }
}