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

    public partial class PageMap : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RootPage = "RootPage";
            public const string SiteType = "SiteType";
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

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeSettingsNotification( upPanel );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            List<int> expandedPageIds = new List<int>();

            RockContext rockContext = new RockContext();
            PageService pageService = new PageService( rockContext );

            if ( Page.IsPostBack )
            {
                foreach ( int expandedId in hfExpandedIds.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList() )
                {
                    if ( expandedId != 0 )
                    {
                        expandedPageIds.Add( expandedId );
                    }
                }
            }
            else
            {
                string pageSearch = this.PageParameter( PageParameterKey.PageSearch );

                // NOTE: using "Page" instead of "PageId" since PageId is already parameter of the current page
                int? selectedPageId = this.PageParameter( PageParameterKey.Page ).AsIntegerOrNull();
                if ( !string.IsNullOrWhiteSpace( pageSearch ) )
                {
                    foreach ( var page in pageService.Queryable().Where( a => a.InternalName.IndexOf( pageSearch ) >= 0 ) )
                    {
                        var selectedPage = page;
                        while ( selectedPage != null )
                        {
                            selectedPage = selectedPage.ParentPage;
                            if ( selectedPage != null )
                            {
                                expandedPageIds.Add( selectedPage.Id );
                            }
                        }
                    }
                }
                else if ( selectedPageId.HasValue )
                {
                    expandedPageIds.Add( selectedPageId.Value );
                    hfSelectedItemId.Value = selectedPageId.ToString();
                }
            }

            // also get any additional expanded nodes that were sent in the Post
            string postedExpandedIds = this.Request.Params["ExpandedIds"];
            if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
            {
                var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList().AsIntegerList();
                foreach ( var postedId in postedExpandedIdList )
                {
                    if ( !expandedPageIds.Contains( postedId ) )
                    {
                        expandedPageIds.Add( postedId );
                    }
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine( "<ul id=\"treeview\">" );

            var rootPagesQry = pageService.Queryable().AsNoTracking();

            string rootPage = GetAttributeValue( AttributeKey.RootPage );
            if ( !string.IsNullOrEmpty( rootPage ) )
            {
                Guid pageGuid = rootPage.AsGuid();
                rootPagesQry = rootPagesQry.Where( a => a.ParentPage.Guid == pageGuid );
            }
            else
            {
                rootPagesQry = rootPagesQry.Where( a => a.ParentPageId == null );
            }

            // Apply the Site Type filter to the root page list.
            var siteTypeList = GetAttributeValue( AttributeKey.SiteType ).SplitDelimitedValues( "," ).AsIntegerList();

            if ( siteTypeList.Any() )
            {
                var siteService = new SiteService( rockContext );

                var siteIdList = siteService.Queryable()
                    .Where( x => siteTypeList.Contains( ( int ) x.SiteType ) )
                    .Select( x => x.Id )
                    .ToList();

                rootPagesQry = rootPagesQry.Where( a => siteIdList.Contains( a.Layout.Site.Id ) );
            }

            var rootPageList = rootPagesQry.OrderBy( a => a.Order ).Select( a => a.Id ).ToList();

            foreach ( var pageId in rootPageList )
            {
                sb.Append( PageNode( PageCache.Get( pageId ), expandedPageIds, rockContext ) );
            }

            sb.AppendLine( "</ul>" );

            lPages.Text = sb.ToString();
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Adds the page nodes.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="expandedPageIdList">The expanded page identifier list.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected string PageNode( PageCache page, List<int> expandedPageIdList, RockContext rockContext )
        {
            var sb = new StringBuilder();

            bool isExpanded = expandedPageIdList.Contains( page.Id );

            sb.AppendFormat(
                @"
<li {0} data-id='{1}'>
    <span><i class='fa fa-file-o'></i> {2}</span>
",
                isExpanded ? "data-expanded='true'" : string.Empty, // 0
                page.Id, // 1
                page.InternalName // 2
            );

            var childPages = page.GetPages( rockContext );
            if ( childPages.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var childPage in childPages.OrderBy( a => a.Order ) )
                {
                    sb.Append( PageNode( childPage, expandedPageIdList, rockContext ) );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

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

            qryParams.Add( "ExpandedIds", hfExpandedIds.Value );

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

            qryParams.Add( "ExpandedIds", hfExpandedIds.Value );

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