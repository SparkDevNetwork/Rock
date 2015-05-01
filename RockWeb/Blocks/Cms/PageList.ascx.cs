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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Data;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Page List" )]
    [Category( "CMS" )]
    [Description( "Lists pages for a site." )]
    public partial class PageList : RockBlock, ISecondaryBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPages.DataKeyNames = new string[] { "Guid" };
            gPages.GridRebind += gPages_GridRebind;
            gPagesFilter.ApplyFilterClick += gPagesFilter_ApplyFilterClick;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPages.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindPagesGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void gPages_GridRebind( object sender, EventArgs e )
        {
            BindPagesGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gPagesFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gPagesFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlLayoutFilter.SelectedIndex > 0 )
            {
                gPagesFilter.SaveUserPreference( "Layout", ddlLayoutFilter.SelectedValue );
            }
            else
            {
                gPagesFilter.SaveUserPreference( "Layout", "" );
            }

            BindPagesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPages_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PageService pageService = new PageService( rockContext );
            var pageViewService = new PageViewService(rockContext);
            var siteService = new SiteService(rockContext);

            Rock.Model.Page page = pageService.Get( new Guid( e.RowKeyValue.ToString() ) );
            if ( page != null )
            {
                string errorMessage;
                if ( !pageService.CanDelete( page, out errorMessage, includeSecondLvl: true ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                foreach (var site in siteService.Queryable())
                {
                    if (site.DefaultPageId == page.Id)
                    {
                        site.DefaultPageId = null;
                        site.DefaultPageRouteId = null;
                    }
                    if (site.LoginPageId == page.Id)
                    {
                        site.LoginPageId = null;
                        site.LoginPageRouteId = null;
                    }
                    if (site.RegistrationPageId == page.Id)
                    {
                        site.RegistrationPageId = null;
                        site.RegistrationPageRouteId = null;
                    }
                }

                foreach (var pageView in pageViewService.GetByPageId(page.Id))
                {
                    pageView.Page = null;
                    pageView.PageId = null;
                }

                pageService.Delete( page );

                rockContext.SaveChanges();

                PageCache.Flush( page.Id );
            }

            BindPagesGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int siteId = PageParameter( "siteId" ).AsInteger();
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }
            LayoutService.RegisterLayouts( Request.MapPath( "~" ), SiteCache.Read( siteId ) );
            LayoutService layoutService = new LayoutService( new RockContext() );
            var layouts = layoutService.Queryable().Where( a => a.SiteId.Equals( siteId ) ).ToList();
            ddlLayoutFilter.DataSource = layouts;
            ddlLayoutFilter.DataBind();
            ddlLayoutFilter.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlLayoutFilter.Visible = layouts.Any();
            ddlLayoutFilter.SetValue( gPagesFilter.GetUserPreference( "Layout" ) );
        }

        /// <summary>
        /// Binds the pages grid.
        /// </summary>
        protected void BindPagesGrid()
        {
            pnlPages.Visible = false;
            int siteId = PageParameter( "siteId" ).AsInteger();
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }

            hfSiteId.SetValue( siteId );
            pnlPages.Visible = true;

            // Question: Is this RegisterLayouts necessary here?  Since if it's a new layout
            // there would not be any pages on them (which is our concern here).
            // It seems like it should be the concern of some other part of the puzzle.
            LayoutService.RegisterLayouts( Request.MapPath( "~" ), SiteCache.Read( siteId ) );
            //var layouts = layoutService.Queryable().Where( a => a.SiteId.Equals( siteId ) ).Select( a => a.Id ).ToList();

            // Find all the pages that are related to this site...
            // 1) pages used by one of this site's layouts and
            // 2) the site's 'special' pages used directly by the site.
            var rockContext = new RockContext();
            var siteService = new SiteService( rockContext );
            var pageService = new PageService( rockContext );

            var site = siteService.Get( siteId );
            if ( site != null )
            {
                var sitePages = new List<int> {
                    site.DefaultPageId ?? -1,
                    site.LoginPageId ?? -1,
                    site.RegistrationPageId ?? -1, 
                    site.PageNotFoundPageId ?? -1
                };

                var qry = pageService.Queryable("Layout")
                    .Where( t => 
                        t.Layout.SiteId == siteId ||
                        sitePages.Contains( t.Id ) );

                string layoutFilter = gPagesFilter.GetUserPreference( "Layout" );
                if ( !string.IsNullOrWhiteSpace( layoutFilter ) && layoutFilter != Rock.Constants.All.Text )
                {
                    qry = qry.Where( a => a.Layout.ToString() == layoutFilter );
                }

                SortProperty sortProperty = gPages.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry
                        .OrderBy( t => t.Layout.Name )
                        .ThenBy( t => t.InternalName );
                }

                gPages.DataSource = qry.ToList();
                gPages.DataBind();
            }
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}