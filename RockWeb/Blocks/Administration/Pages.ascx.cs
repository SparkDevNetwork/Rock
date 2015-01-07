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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Pages" )]
    [Category( "Administration" )]
    [Description( "Lists pages in Rock." )]
    public partial class Pages : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool canConfigure = false;
        private Rock.Web.Cache.PageCache _page = null;

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            try
            {
                int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
                _page = Rock.Web.Cache.PageCache.Read( pageId );

                if ( _page != null )
                {
                    canConfigure = _page.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                }
                else
                {
                    canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );
                } 

                if ( canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "Id" };
                    rGrid.Actions.ShowAdd = true;
                    rGrid.Actions.AddClick += rGrid_GridAdd;
                    rGrid.Actions.ShowExcelExport = false;
                    rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                    rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
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

        #region Events

        #region Grid 

        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            int? parentPageId = null;
            if (_page != null)
                parentPageId = _page.Id;

            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            pageService.Reorder( pageService.GetByParentPageId( parentPageId ).ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var pageViewService = new PageViewService( rockContext );
            var siteService = new SiteService( rockContext );

            var page = pageService.Get( e.RowKeyId );
            if ( page != null )
            {
                string errorMessage = string.Empty;
                if ( !pageService.CanDelete( page, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                foreach ( var site in siteService.Queryable() )
                {
                    if ( site.DefaultPageId == page.Id )
                    {
                        site.DefaultPageId = null;
                        site.DefaultPageRouteId = null;
                    }
                    if ( site.LoginPageId == page.Id )
                    {
                        site.LoginPageId = null;
                        site.LoginPageRouteId = null;
                    }
                    if ( site.RegistrationPageId == page.Id )
                    {
                        site.RegistrationPageId = null;
                        site.RegistrationPageRouteId = null;
                    }
                }

                foreach( var pageView in pageViewService.GetByPageId(page.Id))
                {
                    pageView.Page = null;
                    pageView.PageId = null;
                }

                pageService.Delete( page );

                rockContext.SaveChanges();

                Rock.Web.Cache.PageCache.Flush( page.Id );

                if ( _page != null )
                {
                    _page.FlushChildPages();
                }
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

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            rGrid.Visible = true;
            pnlDetails.Visible = false;
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                Rock.Model.Page page;

                var rockContext = new RockContext();
                var pageService = new PageService( rockContext );

                int pageId = hfPageId.Value.AsInteger();
                if ( pageId == 0 )
                {
                    page = new Rock.Model.Page();

                    if ( _page != null )
                    {
                        page.ParentPageId = _page.Id;
                        page.LayoutId = _page.LayoutId;
                    }
                    else
                    {
                        page.ParentPageId = null;
                        page.LayoutId = PageCache.Read( RockPage.PageId ).LayoutId;
                    }

                    page.PageTitle = dtbPageName.Text;
                    page.BrowserTitle = page.PageTitle;
                    page.EnableViewState = true;
                    page.IncludeAdminFooter = true;
                    page.MenuDisplayChildPages = true;

                    Rock.Model.Page lastPage =
                        pageService.GetByParentPageId( _page.Id ).
                            OrderByDescending( b => b.Order ).FirstOrDefault();

                    if ( lastPage != null )
                        page.Order = lastPage.Order + 1;
                    else
                        page.Order = 0;

                    pageService.Add( page );

                }
                else
                {
                    page = pageService.Get( pageId );
                }

                page.LayoutId = ddlLayout.SelectedValueAsInt().Value;
                page.InternalName = dtbPageName.Text;

                if ( page.IsValid )
                {
                    rockContext.SaveChanges();

                    PageCache.Flush( page.Id );
                    if ( _page != null )
                    {
                        Rock.Security.Authorization.CopyAuthorization( _page, page );
                        _page.FlushChildPages();
                    }

                    BindGrid();
                }

                rGrid.Visible = true;
                pnlDetails.Visible = false;
            }
        }

        #endregion

        #endregion

        #region Methods

        private void BindGrid()
        {
            int? parentPageId = null;
            if ( _page != null )
                parentPageId = _page.Id;

            rGrid.DataSource = new PageService( new RockContext() ).GetByParentPageId( parentPageId ).ToList();
            rGrid.DataBind();
        }

        private void LoadLayouts()
        {
            ddlLayout.Items.Clear();
            int siteId = _page != null ? _page.Layout.SiteId : RockPage.Layout.SiteId;
            foreach ( var layout in new LayoutService( new RockContext() ).GetBySiteId( siteId ) )
            {
                ddlLayout.Items.Add( new ListItem( layout.Name, layout.Id.ToString() ) );
            }
        }

        protected void ShowEdit( int pageId )
        {
            var page = new PageService( new RockContext() ).Get( pageId );
            if ( page != null )
            {
                hfPageId.Value = page.Id.ToString();
                try { ddlLayout.SelectedValue = page.LayoutId.ToString(); }
                catch { }
                dtbPageName.Text = page.InternalName;

                lEditAction.Text = "Edit";
                lbSave.Text = "Save";
            }
            else
            {
                hfPageId.Value = "0";

                try
                {
                    if ( _page != null )
                        ddlLayout.SelectedValue = _page.LayoutId.ToString();
                    else
                        ddlLayout.Text = RockPage.Layout.Id.ToString();
                }
                catch { }

                dtbPageName.Text = string.Empty;

                lEditAction.Text = "Add";
                lbSave.Text = "Add";
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