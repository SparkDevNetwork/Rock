//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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

            gPageRoutes.DataKeyNames = new string[] { "id" };
            gPageRoutes.Actions.ShowAdd = true;
            gPageRoutes.Actions.AddClick += gPageRoutes_Add;
            gPageRoutes.GridRebind += gPageRoutes_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
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
            NavigateToLinkedPage( "DetailPage", "pageRouteId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                PageRouteService pageRouteService = new PageRouteService();
                PageRoute pageRoute = pageRouteService.Get( (int)e.RowKeyValue );

                if ( pageRoute != null )
                {
                    string errorMessage;
                    if ( !pageRouteService.CanDelete( pageRoute, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    pageRouteService.Delete( pageRoute, CurrentPersonId );
                    pageRouteService.Save( pageRoute, CurrentPersonId );

                    RemovePageRoute( pageRoute );
                }
            } );

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
            PageRouteService pageRouteService = new PageRouteService();
            SortProperty sortProperty = gPageRoutes.SortProperty;

            var qry = pageRouteService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Route,
                    PageName = a.Page.Name,
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