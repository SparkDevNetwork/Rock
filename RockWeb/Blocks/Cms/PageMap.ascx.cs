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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Page Map" )]
    [Category( "CMS" )]
    [Description( "Displays a page map in a tree view." )]

    #region Block Attributes

    [LinkedPage(
        "Root Page",
        Description = "Select the root page to use as a starting point for the tree view. Leaving empty will build a tree of all pages.",
        IsRequired = false,
        Key = AttributeKey.RootPage)]
    [EnumsField(
        "Site Type",
        Description = "Select the Site Types of the root-level pages shown in the page map. If no items are selected, all root-level pages will be shown.",
        IsRequired = false,
        EnumSourceType = typeof( SiteType ),
        Key = AttributeKey.SiteType )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "2700A1B8-BD1A-40F1-A660-476DA86D0432" )]
    public partial class PageMap : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RootPage = "RootPage";
            public const string SiteType = "SiteType";
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string PageSearch = "PageSearch";
            public const string Page = "Page";
        }

        #endregion

        #region Fields

        private int? _pageId = null;
        private string _pageSearch = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _pageId = PageParameter( PageParameterKey.Page ).AsIntegerOrNull();
            _pageSearch = PageParameter( PageParameterKey.PageSearch );

            var detailPageReference = new Rock.Web.PageReference( GetAttributeValue( AttributeKey.DetailPage ) );

            // NOTE: if the detail page is the current page, use the current route instead of route specified in the DetailPage (to preserve old behavior)
            if ( detailPageReference == null || detailPageReference.PageId == this.RockPage.PageId )
            {
                hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
                hfDetailPageUrl.Value = new Rock.Web.PageReference( this.RockPage.PageId ).BuildUrl();
            }
            else
            {
                hfPageRouteTemplate.Value = string.Empty;
                var pageCache = PageCache.Get( detailPageReference.PageId );
                if ( pageCache != null )
                {
                    var route = pageCache.PageRoutes.FirstOrDefault( a => a.Id == detailPageReference.RouteId );
                    if ( route != null )
                    {
                        hfPageRouteTemplate.Value = route.Route;
                    }
                }

                hfDetailPageUrl.Value = detailPageReference.BuildUrl();
            }

            InitializeSettingsNotification( upPanel );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

            if ( _pageId.HasValue )
            {
                var key = string.Format( "Page:{0}", _pageId );
                var rockContext = new RockContext();
                var selectedPage = RockPage.GetSharedItem( key ) as Rock.Model.Page;
                var pageService = new PageService( rockContext );
                var parentIdList = new List<string>();

                // NOTE: using "Page" instead of "PageId" since PageId is already parameter of the current page
                if ( !string.IsNullOrWhiteSpace( _pageSearch ) )
                {
                    foreach ( var pageItem in pageService.Queryable().Where( a => a.InternalName.IndexOf( _pageSearch ) >= 0 ) )
                    {
                        selectedPage = pageItem;
                        while ( selectedPage != null )
                        {
                            selectedPage = selectedPage.ParentPage;
                            if ( selectedPage != null )
                            {
                                parentIdList.Add( selectedPage.Id.ToString() );
                            }
                        }
                    }
                }
                else
                {
                    parentIdList.Add( _pageId.ToString() );
                    hfSelectedPageId.Value = _pageId.ToString();
                }

                if ( selectedPage == null )
                {
                    selectedPage = new PageService( rockContext ).Queryable()
                        .Where( p => p.Id == _pageId )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, selectedPage );
                }

                // Apply the Site Type filter to the root page list.
                hfSiteTypeList.Value = GetAttributeValue( AttributeKey.SiteType );

                // get the parents of the selected item so we can tell the treeview to expand those
                int? rootPageId = hfRootPageId.Value.AsIntegerOrNull();
                var page = selectedPage;
                while ( page != null )
                {
                    if ( page.Id == rootPageId )
                    {
                        // stop if we are at the root group
                        page = null;
                    }
                    else
                    {
                        page = page.ParentPage;
                    }

                    if ( page != null )
                    {
                        if ( !parentIdList.Contains( page.Id.ToString() ) )
                        {
                            parentIdList.Insert( 0, page.Id.ToString() );
                        }
                        else
                        {
                            // The parent list already contains this node, so we have encountered a recursive loop.
                            // Stop here and make the current node the root of the tree.
                            page = null;
                        }
                    }
                }

                // also get any additional expanded nodes that were sent in the Post
                string postedExpandedIds = this.Request.Params["ExpandedIds"];
                if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
                {
                    var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList();
                    foreach ( var id in postedExpandedIdList )
                    {
                        if ( !parentIdList.Contains( id ) )
                        {
                            parentIdList.Add( id );
                        }
                    }
                }

                if ( null != selectedPage )
                {
                    hfInitialPageId.Value = selectedPage.Id.ToString();
                    hfSelectedPageId.Value = selectedPage.Id.ToString();
                }

                hfInitialPageParentIds.Value = parentIdList.AsDelimited( "," );
            }
            else
            {
                // let the Add button be visible if there is nothing selected (if authorized)
                lbAddPageChild.Enabled = canEditBlock;
            }

            // NOTE that canEditBlock just controls if the button is shown.
            // The page detail block will take care of enforcing auth when they attempt to save a group.
            divAddPage.Visible = canEditBlock;
            lbAddPageRoot.Enabled = canEditBlock;
            lbAddPageChild.Enabled = canEditBlock;

            // disable add child page if no page is selected
            if ( hfSelectedPageId.ValueAsInt() == 0 )
            {
                lbAddPageChild.Enabled = false;
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddPageRoot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPageRoot_Click( object sender, EventArgs e )
        {
            // if a rootPage is set, set that as the parentCategory when they select "add top-level"
            var rootPage = new PageService( new RockContext() ).Get( this.GetAttributeValue(  AttributeKey.RootPage ).AsGuid() );
            int parentPageId = rootPage != null ? rootPage.Id : 0;

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "Page", 0.ToString() );
            if ( parentPageId > 0 )
            {
                qryParams.Add( "ParentPageId", parentPageId.ToString() );
            }

            qryParams.Add( "ExpandedIds", hfInitialPageParentIds.Value );

            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddPageChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPageChild_Click( object sender, EventArgs e )
        {
            int? selectedPageId = this.PageParameter( PageParameterKey.Page ).AsIntegerOrNull();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "Page", 0.ToString() );
            if ( selectedPageId > 0 )
            {
                qryParams.Add( "ParentPageId", selectedPageId.ToString() );
            }

            qryParams.Add( "ExpandedIds", hfInitialPageParentIds.Value );

            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the BlockUpdated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            // Reload the page to refresh the map.
            NavigateToCurrentPageReference();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( triggerPanel );
        }

        #endregion
    }
}