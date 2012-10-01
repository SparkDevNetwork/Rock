//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Rock.Web.UI.Controls;
using Rock.Cms;


namespace RockWeb.Blocks.Administration
{
	/// <summary>
	/// 
	/// </summary>
	public partial class PageRoutes : Rock.Web.UI.Block
	{
		#region Fields

		Rock.Cms.PageRouteService pageRouteService = new Rock.Cms.PageRouteService();
		Rock.Cms.PageService pageService = new Rock.Cms.PageService();

		#endregion

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

				// Open Question

				//gPageRoutes.Actions.IsAddEnabled = true;
				//gPageRoutes.Actions.AddClick += gSites_Add;

				gPageRoutes.GridRebind += gPageRoutes_GridRebind;
			}

			string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this page route?');
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
			Rock.Cms.PageRoute pageRoute;

			int pageRouteId = 0;
			if ( !int.TryParse( hfPageRouteId.Value, out pageRouteId ) )
			{
				pageRouteId = 0;
			}

			if ( pageRouteId == 0 )
			{
				pageRoute = new Rock.Cms.PageRoute();
				pageRouteService.Add( pageRoute, CurrentPersonId );
			}
			else
			{
				pageRoute = pageRouteService.Get( pageRouteId );
			}

			pageRoute.Route = tbRoute.Text;

			int selectedPageId = int.Parse( ddlPageName.SelectedValue );
			pageRoute.PageId = selectedPageId;
			//pageRoute.

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
			List<PageRoute> pageRoutes = pageRouteService.Queryable().OrderBy( p => p.Route ).ToList();

			gPageRoutes.DataSource = pageRoutes;
			gPageRoutes.DataBind();
		}

		/// <summary>
		/// Loads the drop downs.
		/// </summary>
		private void LoadDropDowns()
		{
			List<Rock.Cms.Page> pages = pageService.Queryable().OrderBy( p => p.Name ).ToList();
			ddlPageName.DataSource = pages;
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

			Rock.Cms.PageRoute pageRoute = pageRouteService.Get( pageRouteId );

			if ( pageRoute != null )
			{
				hfPageRouteId.Value = pageRoute.Id.ToString();

				ddlPageName.SelectedValue = pageRoute.Page.Id.ToString();
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