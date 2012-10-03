//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using Rock;
using Rock.Cms;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PageRoutes : Rock.Web.UI.RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
            {
                gPageRoutes.DataKeyNames = new string[] { "id" };
                gPageRoutes.Actions.IsAddEnabled = true;
                gPageRoutes.Actions.AddClick += gPageRoutes_Add;
                gPageRoutes.GridRebind += gPageRoutes_GridRebind;
            }

            string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this Page Route?');
                });
        });
    ";
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gPageRoutes.ClientID ), script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                    LoadDropDowns();
                }
            }
            else
            {
                gPageRoutes.Visible = false;
                nbMessage.Text = "You are not authorized to edit page routes";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)gPageRoutes.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Delete( object sender, RowEventArgs e )
        {
            Rock.Cms.PageRouteService pageRouteService = new Rock.Cms.PageRouteService();
            Rock.Cms.PageRoute pageRoute = pageRouteService.Get( (int)gPageRoutes.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                pageRouteService.Delete( pageRoute, CurrentPersonId );
                pageRouteService.Save( pageRoute, CurrentPersonId );
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

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PageRoute pageRoute;
            PageRouteService pageRouteService = new PageRouteService();

            int pageRouteId = 0;
            if ( !int.TryParse( hfPageRouteId.Value, out pageRouteId ) )
            {
                pageRouteId = 0;
            }

            if ( pageRouteId == 0 )
            {
                pageRoute = new PageRoute();
                pageRouteService.Add( pageRoute, CurrentPersonId );
            }
            else
            {
                pageRoute = pageRouteService.Get( pageRouteId );
            }

            pageRoute.Route = tbRoute.Text.Trim();
            int selectedPageId = int.Parse( ddlPageName.SelectedValue );
            pageRoute.PageId = selectedPageId;

            // Validate that this can be created as a real Route
            try
            {
                Route testRoute = new Route( tbRoute.Text, null );
            }
            catch ( Exception ex )
            {
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
                return;
            }

            // check for EmptyString
            if ( string.IsNullOrWhiteSpace( pageRoute.Route ) )
            {
                nbMessage.Text = "Route cannot be blank.";
                nbMessage.Visible = true;
                return;
            }

            // check for duplicates
            if ( pageRouteService.Queryable().Count( a => a.Route.Equals( pageRoute.Route, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( pageRoute.Id ) ) > 0 )
            {
                nbMessage.Text = "This route is already being used on another Page.";
                nbMessage.Visible = true;
                return;
            }

            pageRouteService.Save( pageRoute, CurrentPersonId );
            BindGrid();
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Rock.Cms.PageRouteService pageRouteService = new Rock.Cms.PageRouteService();
            List<PageRoute> pageRoutes; 

            SortProperty sortProperty = gPageRoutes.SortProperty;
            if ( sortProperty != null )
            {
                pageRoutes = pageRouteService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                pageRoutes = pageRouteService.Queryable().OrderBy( p => p.Route ).ToList();
            }

            gPageRoutes.DataSource = pageRoutes;
            gPageRoutes.DataBind();
        }

        /// <summary>
        /// 
        /// </summary>
        private class PageInfo
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string DropDownListText { get; set; }
            public int pageDepth { get; set; }
            public string pageHash { get; set; }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            Rock.Cms.PageService pageService = new Rock.Cms.PageService();
            List<Rock.Cms.Page> allPages = pageService.Queryable().ToList();

            List<PageInfo> pageNameList = new List<PageInfo>();

            foreach ( Rock.Cms.Page page in allPages )
            {
                PageInfo pageInfo = new PageInfo { Id = page.Id, Title = page.Title, pageDepth = 0, pageHash = page.Title.PadRight( 100, ' ' ) };
                pageNameList.Add( pageInfo );
                Rock.Cms.Page parentPage = page.ParentPage;
                while ( parentPage != null )
                {
                    pageInfo.pageDepth++;
                    pageInfo.pageHash = parentPage.Title.PadRight( 100, ' ' ) + pageInfo.pageHash;
                    parentPage = parentPage.ParentPage;
                }
                pageInfo.DropDownListText = new string( '-', pageInfo.pageDepth ) + pageInfo.Title;
            }

            ddlPageName.DataSource = pageNameList.OrderBy( a => a.pageHash );
            ddlPageName.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="pageRouteId">The page route id.</param>
        protected void ShowEdit( int pageRouteId )
        {
            pnlList.Visible = false;
            pnlDetails.Visible = true;

            Rock.Cms.PageRouteService pageRouteService = new Rock.Cms.PageRouteService();
            Rock.Cms.PageRoute pageRoute = pageRouteService.Get( pageRouteId );

            if ( pageRoute != null )
            {
                hfPageRouteId.Value = pageRoute.Id.ToString();

                ddlPageName.SelectedValue = pageRoute.PageId.ToString();
                tbRoute.Text = pageRoute.Route;
            }
            else
            {
                tbRoute.Text = string.Empty;
            }
        }

        #endregion
    }
}