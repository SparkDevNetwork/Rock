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
        #region Control Methods

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

        #region Edit Events

        /// <summary>
        /// Handles the Edit event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPages_Edit( object sender, RowEventArgs e )
        {
            var queryString = new Dictionary<string, string>();
            NavigateToPage( new Guid( e.RowKeyValue.ToString() ), queryString );
        }

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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int siteId = PageParameter( "siteId" ).AsInteger() ?? 0;
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }
            LayoutService layoutService = new LayoutService();
            layoutService.RegisterLayouts( Request.MapPath( "~" ), SiteCache.Read( siteId ), CurrentPersonAlias );
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
            int siteId = PageParameter( "siteId" ).AsInteger() ?? 0;
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }

            hfSiteId.SetValue( siteId );
            pnlPages.Visible = true;

            LayoutService layoutService = new LayoutService();
            layoutService.RegisterLayouts( Request.MapPath( "~" ), SiteCache.Read( siteId ), CurrentPersonAlias );
            var layouts = layoutService.Queryable().Where( a => a.SiteId.Equals( siteId ) ).Select( a => a.Id ).ToList();

            var siteService = new SiteService();
            var pageId = siteService.Get( siteId ).DefaultPageId;

            var pageService = new PageService();
            var qry = pageService.GetAllDescendents( (int)pageId ).AsQueryable().Where( a => layouts.Contains( a.LayoutId ) )
                .Concat( pageService.GetByIds( new List<int>{ (int)pageId } ) );
       
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
                qry = qry.OrderBy( q => q.Id );
            }

            gPages.DataSource = qry.ToList();
            gPages.DataBind();
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