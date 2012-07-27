﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Pages : Rock.Web.UI.Block
	{
        #region Fields

        private bool canConfigure = false;
        private Rock.Web.Cache.Page _page = null;
        private Rock.CMS.PageService pageService = new Rock.CMS.PageService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            try
            {
                int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
                _page = Rock.Web.Cache.Page.Read( pageId );

                if ( _page != null )
                    canConfigure = _page.IsAuthorized( "Configure", CurrentUser );
                else
                    canConfigure = PageInstance.IsAuthorized( "Configure", CurrentUser );

                if ( canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;
                    rGrid.Actions.AddClick += rGrid_GridAdd;
                    rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                    rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this page?');
                }});
        }});
    ", rGrid.ClientID );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && canConfigure )
            {
                BindGrid();
                LoadLayouts();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            int? parentPageId = null;
            if (_page != null)
                parentPageId = _page.Id;

            pageService.Reorder( pageService.GetByParentPageId( parentPageId ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.Page page = pageService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( page != null )
            {
                Rock.Web.Cache.Page.Flush( page.Id );

                pageService.Delete( page, CurrentPersonId );
                pageService.Save( page, CurrentPersonId );

                if (_page != null)
                    _page.FlushChildPages();
            }

            BindGrid();
        }

        void rGrid_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            rGrid.Visible = true;
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.CMS.Page page;

            int pageId = 0;
            if ( !Int32.TryParse( hfPageId.Value, out pageId ) )
                pageId = 0;

            if ( pageId == 0 )
            {
                page = new Rock.CMS.Page();

                if ( _page != null )
                {
                    page.ParentPageId = _page.Id;
                    page.SiteId = _page.Site.Id;
                }
                else
                {
                    page.ParentPageId = null;
                    page.SiteId = PageInstance.Site.Id;
                }

                page.Title = tbPageName.Text;
                page.EnableViewState = true;
                page.IncludeAdminFooter = true;

                Rock.CMS.Page lastPage =
                    pageService.GetByParentPageId( _page.Id ).
                        OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastPage != null )
                    page.Order = lastPage.Order + 1;
                else
                    page.Order = 0;

                pageService.Add( page, CurrentPersonId );

            }
            else
                page = pageService.Get( pageId );

            page.Layout = ddlLayout.Text;
            page.Name = tbPageName.Text;

            pageService.Save( page, CurrentPersonId );

            if ( _page != null )
            {
                Rock.Security.Authorization.CopyAuthorization( _page, page, CurrentPersonId );
                _page.FlushChildPages();
            }

            BindGrid();

            rGrid.Visible = true; 
            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            int? parentPageId = null;
            if ( _page != null )
                parentPageId = _page.Id;

            rGrid.DataSource = pageService.GetByParentPageId( parentPageId ).ToList();
            rGrid.DataBind();
        }

        private void LoadLayouts()
        {
            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.ThemePath ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx" ) ) ) );
        }

        protected void ShowEdit( int pageId )
        {
            Rock.CMS.Page page = pageService.Get( pageId );
            if ( page != null )
            {
                hfPageId.Value = page.Id.ToString();
                try { ddlLayout.Text = page.Layout; }
                catch { }
                tbPageName.Text = page.Name;

                lEditAction.Text = "Edit";
                btnSave.Text = "Save";
            }
            else
            {
                hfPageId.Value = "0";

                try
                {
                    if ( _page != null )
                        ddlLayout.Text = _page.Layout;
                    else
                        ddlLayout.Text = PageInstance.Layout;
                }
                catch { }

                tbPageName.Text = string.Empty;

                lEditAction.Text = "Add";
                btnSave.Text = "Add";
            }

            rGrid.Visible = false;
            pnlDetails.Visible = true;
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phContent.Visible = false;
        }

        #endregion
    }
}