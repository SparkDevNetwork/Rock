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
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Route List")]
    [Category("CMS")]
    [Description("Displays a list of page routes.")]
    [LinkedPage("Detail Page")]
    public partial class PageRouteList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPageRoutes.DataKeyNames = new string[] { "Id" };
            gPageRoutes.Actions.ShowAdd = true;
            gPageRoutes.Actions.AddClick += gPageRoutes_Add;
            gPageRoutes.GridRebind += gPageRoutes_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPageRoutes.Actions.ShowAdd = canAddEditDelete;
            gPageRoutes.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "pageRouteId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "pageRouteId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PageRouteService pageRouteService = new PageRouteService( rockContext );
            PageRoute pageRoute = pageRouteService.Get( e.RowKeyId );

            if ( pageRoute != null )
            {
                string errorMessage;
                if ( !pageRouteService.CanDelete( pageRoute, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                pageRouteService.Delete( pageRoute );

                rockContext.SaveChanges();

                RemovePageRoute( pageRoute );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPageRoutes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        /// <summary>
        /// Removes the page route.
        /// </summary>
        /// <param name="pageRoute">The page route.</param>
        private static void RemovePageRoute( PageRoute pageRoute )
        {
            var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteId() == pageRoute.Id );
            if ( existingRoute != null )
            {
                RouteTable.Routes.Remove( existingRoute );
            }
        }

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            PageRouteService pageRouteService = new PageRouteService( new RockContext() );
            SortProperty sortProperty = gPageRoutes.SortProperty;
            gPageRoutes.EntityTypeId = EntityTypeCache.Read<PageRoute>().Id;

            var qry = pageRouteService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Route,
                    PageName = a.Page.InternalName,
                    PageId = a.Page.Id,
                    a.IsSystem
                } );

            if ( sortProperty != null )
            {
                gPageRoutes.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gPageRoutes.DataSource = qry.OrderBy( p => p.Route ).ToList();
            }

            gPageRoutes.DataBind();
        }

        #endregion
    }
}