using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Controls;

namespace RockWeb.Blocks.Cms
{
    public partial class Pages : Rock.Cms.CmsBlock
	{
        #region Fields

        private Rock.Cms.Cached.Page _page = null;
        private Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
            _page = Rock.Cms.Cached.Page.Read( pageId );

            if ( _page.Authorized( "Configure", Membership.GetUser() ) )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.EnableAdd = true;
                rGrid.GridAdd += new GridAddEventHandler( rGrid_GridAdd );
                rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );

                string script = string.Format( @"
        $(document).ready(function() {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this page?');
                }});
        }});

        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this page?');
                }});
        }});
    ", rGrid.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _page.Authorized( "Configure", Membership.GetUser() ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                    LoadLayouts();
                }
            }
            else
            {
                rGrid.Visible = false;
                nbMessage.Text = "You are not authorized to edit these pages";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            pageService.Reorder( pageService.GetPagesByParentPageId( _page.Id ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Models.Cms.Page page = pageService.GetPage( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( page != null )
            {
                Rock.Cms.Cached.Page.Flush( page.Id );

                pageService.DeletePage( page );
                pageService.Save( page, CurrentPersonId );

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
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.Models.Cms.Page page;

            int pageId = 0;
            if ( !Int32.TryParse( hfPageId.Value, out pageId ) )
                pageId = 0;

            if ( pageId == 0 )
            {
                page = new Rock.Models.Cms.Page();
                page.ParentPageId = _page.Id;
                page.SiteId = _page.Site.Id;
                page.Title = tbPageName.Text;
                page.EnableViewState = true;
                page.IncludeAdminFooter = true;

                Rock.Models.Cms.Page lastPage =
                    pageService.GetPagesByParentPageId( _page.Id ).
                        OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastPage != null )
                    page.Order = lastPage.Order + 1;
                else
                    page.Order = 0;

                pageService.AddPage( page );
            }
            else
                page = pageService.GetPage( pageId );

            page.Layout = ddlLayout.Text;
            page.Name = tbPageName.Text;

            pageService.Save( page, CurrentPersonId );

            Rock.Cms.Security.Authorization.CopyAuthorization( _page, page, CurrentPersonId );
            _page.FlushChildPages();

            BindGrid();

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            rGrid.DataSource = pageService.GetPagesByParentPageId( _page.Id ).ToList();
            rGrid.DataBind();
        }

        private void LoadLayouts()
        {
            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.ThemePath ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx.cs" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx.cs" ) ) ) );
        }

        protected void ShowEdit( int pageId )
        {
            Rock.Models.Cms.Page page = pageService.GetPage( pageId );
            if ( page != null )
            {
                hfPageId.Value = page.Id.ToString();
                ddlLayout.Text = page.Layout;
                tbPageName.Text = page.Name;
            }
            else
            {
                hfPageId.Value = "0";
                ddlLayout.Text = _page.Layout;
                tbPageName.Text = string.Empty;
            }

            pnlDetails.Visible = true;
        }

        #endregion
    }
}