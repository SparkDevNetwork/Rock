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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
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

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [page updated].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page updated]; otherwise, <c>false</c>.
        /// </value>
        protected bool PageUpdated
        {
            get
            {
                DialogPage dialogPage = this.Page as DialogPage;
                if ( dialogPage != null )
                {
                    return dialogPage.CloseMessage == "PAGE_UPDATED";
                }
                return false;
            }
            set
            {
                DialogPage dialogPage = this.Page as DialogPage;
                if ( dialogPage != null )
                {
                    dialogPage.CloseMessage = value ? "PAGE_UPDATED" : "";
                }
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                    rGrid.Actions.ShowMergeTemplate = false;
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            if ( _page == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var childPages = pageService.GetByParentPageId( _page.Id ).ToList();
            pageService.Reorder( childPages, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            Rock.Web.Cache.PageCache.Flush( _page.Id );

            foreach ( var page in childPages )
            {
                // make sure the PageCache for all the re-ordered pages get flushed so the new Order is updated
                Rock.Web.Cache.PageCache.Flush( page.Id );
            }

            _page.FlushChildPages();
            PageUpdated = true;

            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
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

                pageService.Delete( page );

                rockContext.SaveChanges();

                Rock.Web.Cache.PageCache.Flush( page.Id );
                PageUpdated = true;

                if ( _page != null )
                {
                    _page.FlushChildPages();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridAdd event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Copy event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_Copy( object sender, RowEventArgs e )
        {
            hfPageIdToCopy.Value = e.RowKeyId.ToStringSafe();
            mdConfirmCopy.Show();
            mdConfirmCopy.Header.Visible = false;
        }

        /// <summary>
        /// Handles the Copy event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmCopy_Click( object sender, EventArgs e )
        {
            mdConfirmCopy.Hide();
            var pageService = new PageService( new RockContext() );

            // todo, prompt if childpages should be copied
            pageService.CopyPage( hfPageIdToCopy.Value.AsInteger(), true, CurrentPersonAliasId );

            PageUpdated = true;

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            rGrid.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                        page.AllowIndexing = _page.AllowIndexing;
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

                    Rock.Model.Page lastPage = pageService.GetByParentPageId( _page.Id ).OrderByDescending( b => b.Order ).FirstOrDefault();

                    if ( lastPage != null )
                    {
                        page.Order = lastPage.Order + 1;
                    }
                    else
                    {
                        page.Order = 0;
                    }

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
                        Rock.Security.Authorization.CopyAuthorization( _page, page, rockContext );
                        _page.FlushChildPages();
                    }

                    BindGrid();

                    PageUpdated = true;
                }

                rGrid.Visible = true;
                pnlDetails.Visible = false;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int? parentPageId = null;
            if ( _page != null )
            {
                parentPageId = _page.Id;
            }

            rGrid.DataSource = new PageService( new RockContext() ).GetByParentPageId( parentPageId ).ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Loads the layouts.
        /// </summary>
        private void LoadLayouts()
        {
            ddlLayout.Items.Clear();
            int siteId = _page != null ? _page.Layout.SiteId : RockPage.Layout.SiteId;
            foreach ( var layout in new LayoutService( new RockContext() ).GetBySiteId( siteId ) )
            {
                ddlLayout.Items.Add( new ListItem( layout.Name, layout.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        protected void ShowEdit( int pageId )
        {
            var page = new PageService( new RockContext() ).Get( pageId );
            if ( page != null )
            {
                hfPageId.Value = page.Id.ToString();
                ddlLayout.SetValue( page.LayoutId );

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
                    {
                        ddlLayout.SetValue( _page.LayoutId );
                    }
                    else
                    {
                        ddlLayout.Text = RockPage.Layout.Id.ToString();
                    }
                }
                catch
                {
                    // intentionally ignore error. todo: test if we really need to do this
                }

                dtbPageName.Text = string.Empty;

                lEditAction.Text = "Add";
                lbSave.Text = "Add";
            }

            rGrid.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
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