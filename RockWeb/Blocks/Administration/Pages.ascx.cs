// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel;
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
            pageService.Reorder( pageService.GetByParentPageId( _page.Id ).ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            Rock.Web.Cache.PageCache.Flush( _page.Id );
            _page.FlushChildPages();

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

                foreach ( var pageView in pageViewService.GetByPageId( page.Id ) )
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
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var pageViewService = new PageViewService( rockContext );
            var siteService = new SiteService( rockContext );

            var page = pageService.Get( e.RowKeyId );
            if ( page != null )
            {
                Dictionary<Guid, Guid> pageGuidDictionary = new Dictionary<Guid, Guid>();
                Dictionary<Guid, Guid> blockGuidDictionary = new Dictionary<Guid, Guid>();
                var newPage = GeneratePageCopy( page, pageGuidDictionary, blockGuidDictionary );

                pageService.Add( newPage );
                rockContext.SaveChanges();

                GenerateBlockAttributeValues( pageGuidDictionary, blockGuidDictionary, rockContext );
                GeneratePageBlockAuths( pageGuidDictionary, blockGuidDictionary, rockContext );
                CloneHtmlContent( blockGuidDictionary, rockContext );
            }

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

        #region Page Copy Methods

        /// <summary>
        /// This method generates a copy of the given page along with any descendant pages, as well as any blocks on
        /// any of those pages.
        /// </summary>
        /// <param name="sourcePage">The source page.</param>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <returns></returns>
        private Rock.Model.Page GeneratePageCopy( Rock.Model.Page sourcePage, Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary )
        {
            var targetPage = new Rock.Model.Page();
            targetPage = sourcePage.Clone( false );
            targetPage.CreatedByPersonAlias = null;
            targetPage.CreatedByPersonAliasId = CurrentPersonAliasId;
            targetPage.CreatedDateTime = RockDateTime.Now;
            targetPage.ModifiedByPersonAlias = null;
            targetPage.ModifiedByPersonAliasId = CurrentPersonAliasId;
            targetPage.ModifiedDateTime = RockDateTime.Now;
            targetPage.Id = 0;
            targetPage.Guid = Guid.NewGuid();
            targetPage.PageTitle = sourcePage.PageTitle + " - Copy";
            targetPage.InternalName = sourcePage.InternalName + " - Copy";
            targetPage.BrowserTitle = sourcePage.BrowserTitle + " - Copy";
            pageGuidDictionary.Add( sourcePage.Guid, targetPage.Guid );

            foreach ( var block in sourcePage.Blocks )
            {
                var newBlock = block.Clone( false );
                newBlock.CreatedByPersonAlias = null;
                newBlock.CreatedByPersonAliasId = CurrentPersonAliasId;
                newBlock.CreatedDateTime = RockDateTime.Now;
                newBlock.ModifiedByPersonAlias = null;
                newBlock.ModifiedByPersonAliasId = CurrentPersonAliasId;
                newBlock.ModifiedDateTime = RockDateTime.Now;
                newBlock.Id = 0;
                newBlock.Guid = Guid.NewGuid();
                newBlock.PageId = 0;

                blockGuidDictionary.Add( block.Guid, newBlock.Guid );
                targetPage.Blocks.Add( newBlock );
            }

            foreach ( var oldchildPage in sourcePage.Pages )
            {
                targetPage.Pages.Add( GeneratePageCopy( oldchildPage, pageGuidDictionary, blockGuidDictionary ) );
            }

            return targetPage;
        }

        /// <summary>
        /// Copies any auths for the original pages and blocks over to the copied pages and blocks.
        /// </summary>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void GeneratePageBlockAuths( Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext )
        {
            var authService = new AuthService( rockContext );
            var pageService = new PageService( rockContext );
            var blockService = new BlockService( rockContext );
            var pageGuid = "E104DCDF-247C-4CED-A119-8CC51632761F".AsGuid();
            var blockGuid = "D89555CA-9AE4-4D62-8AF1-E5E463C1EF65".AsGuid();

            Dictionary<Guid, int> pageIntDictionary = pageService.Queryable()
                .Where( p => pageGuidDictionary.Keys.Contains( p.Guid ) || pageGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var pageAuths = authService.Queryable().Where( a =>
                a.EntityType.Guid == pageGuid && pageIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            var blockAuths = authService.Queryable().Where( a =>
                a.EntityType.Guid == blockGuid && blockIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            foreach ( var pageAuth in pageAuths )
            {
                var newPageAuth = pageAuth.Clone( false );
                newPageAuth.CreatedByPersonAlias = null;
                newPageAuth.CreatedByPersonAliasId = CurrentPersonAliasId;
                newPageAuth.CreatedDateTime = RockDateTime.Now;
                newPageAuth.ModifiedByPersonAlias = null;
                newPageAuth.ModifiedByPersonAliasId = CurrentPersonAliasId;
                newPageAuth.ModifiedDateTime = RockDateTime.Now;
                newPageAuth.Id = 0;
                newPageAuth.Guid = Guid.NewGuid();
                newPageAuth.EntityId = pageIntDictionary[pageGuidDictionary[pageIntDictionary.Where( d => d.Value == pageAuth.EntityId.Value ).FirstOrDefault().Key]];
                authService.Add( newPageAuth );
            }

            foreach ( var blockAuth in blockAuths )
            {
                var newBlockAuth = blockAuth.Clone( false );
                newBlockAuth.CreatedByPersonAlias = null;
                newBlockAuth.CreatedByPersonAliasId = CurrentPersonAliasId;
                newBlockAuth.CreatedDateTime = RockDateTime.Now;
                newBlockAuth.ModifiedByPersonAlias = null;
                newBlockAuth.ModifiedByPersonAliasId = CurrentPersonAliasId;
                newBlockAuth.ModifiedDateTime = RockDateTime.Now;
                newBlockAuth.Id = 0;
                newBlockAuth.Guid = Guid.NewGuid();
                newBlockAuth.EntityId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == blockAuth.EntityId.Value ).FirstOrDefault().Key]];
                authService.Add( newBlockAuth );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// This method takes the attribute values of the original blocks, and creates copies of them that point to the copied blocks. 
        /// In addition, any block attribute value pointing to a page in the original page tree is now updated to point to the
        /// corresponding page in the copied page tree.
        /// </summary>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void GenerateBlockAttributeValues( Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext )
        {
            var attributeValueService = new AttributeValueService( rockContext );
            var pageService = new PageService( rockContext );
            var blockService = new BlockService( rockContext );
            var pageGuid = "E104DCDF-247C-4CED-A119-8CC51632761F".AsGuid();
            var blockGuid = "D89555CA-9AE4-4D62-8AF1-E5E463C1EF65".AsGuid();

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var attributeValues = attributeValueService.Queryable().Where( a =>
                a.Attribute.EntityType.Guid == blockGuid && blockIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            foreach ( var attributeValue in attributeValues )
            {
                var newAttributeValue = attributeValue.Clone( false );
                newAttributeValue.CreatedByPersonAlias = null;
                newAttributeValue.CreatedByPersonAliasId = CurrentPersonAliasId;
                newAttributeValue.CreatedDateTime = RockDateTime.Now;
                newAttributeValue.ModifiedByPersonAlias = null;
                newAttributeValue.ModifiedByPersonAliasId = CurrentPersonAliasId;
                newAttributeValue.ModifiedDateTime = RockDateTime.Now;
                newAttributeValue.Id = 0;
                newAttributeValue.Guid = Guid.NewGuid();
                newAttributeValue.EntityId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == attributeValue.EntityId.Value ).FirstOrDefault().Key]];

                if ( attributeValue.Attribute.FieldType.Guid == "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108".AsGuid() )
                {
                    if ( pageGuidDictionary.ContainsKey( attributeValue.Value.AsGuid() ) )
                    {
                        newAttributeValue.Value = pageGuidDictionary[attributeValue.Value.AsGuid()].ToString();
                    }
                }

                attributeValueService.Add( newAttributeValue );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Copies any HtmlContent in the original page tree over to the corresponding blocks on the copied page tree.
        /// </summary>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void CloneHtmlContent( Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext )
        {
            var htmlContentService = new HtmlContentService( rockContext );
            var blockService = new BlockService( rockContext );

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var htmlContents = htmlContentService.Queryable().Where( a =>
                blockIntDictionary.Values.Contains( a.BlockId ) )
                .ToList();

            foreach ( var htmlContent in htmlContents )
            {
                var newHtmlContent = htmlContent.Clone( false );
                newHtmlContent.CreatedByPersonAlias = null;
                newHtmlContent.CreatedByPersonAliasId = CurrentPersonAliasId;
                newHtmlContent.CreatedDateTime = RockDateTime.Now;
                newHtmlContent.ModifiedByPersonAlias = null;
                newHtmlContent.ModifiedByPersonAliasId = CurrentPersonAliasId;
                newHtmlContent.ModifiedDateTime = RockDateTime.Now;
                newHtmlContent.Id = 0;
                newHtmlContent.Guid = Guid.NewGuid();
                newHtmlContent.BlockId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == htmlContent.BlockId ).FirstOrDefault().Key]];

                htmlContentService.Add( newHtmlContent );
            }

            rockContext.SaveChanges();
        }

        #endregion

        #endregion
    }
}