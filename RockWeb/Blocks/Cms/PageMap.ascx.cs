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
using System.Web;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Page Map" )]
    [Category( "CMS" )]
    [Description( "Displays a site map in a tree view." )]
    [LinkedPage( "Root Page", "Select the root page to use as a starting point for the tree view. Leaving empty will build a tree of all pages.", false )]
    public partial class PageMap : RockBlock
    {
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

            var allPages = pageService.Queryable().Include( a => a.PageContexts ).Include( a => a.PageRoutes ).AsNoTracking();

            foreach ( var page in allPages )
            {
                PageCache.Read( page );
            }

            foreach ( var block in new BlockService( rockContext ).Queryable().AsNoTracking() )
            {
                BlockCache.Read( block );
            }

            foreach ( var blockType in new BlockTypeService( rockContext ).Queryable().AsNoTracking() )
            {
                BlockTypeCache.Read( blockType );
            }

            if ( Page.IsPostBack )
            {
                if ( Request.Form["__EVENTTARGET"] == "CopyPage" )
                {
                    // Fire event
                    int? intPageId = Request.Form["__EVENTARGUMENT"].AsIntegerOrNull();
                    if ( intPageId.HasValue )
                    {
                        Guid? pageGuid = pageService.CopyPage( intPageId.Value, CurrentPersonAliasId );
                        if ( pageGuid.HasValue )
                        {
                            NavigateToPage( pageGuid.Value, null );
                        }
                        else
                        {
                            NavigateToCurrentPage();
                        }
                    }
                }

                foreach ( string expandedId in hfExpandedIds.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    int id = 0;
                    if ( expandedId.StartsWith( "p" ) && expandedId.Length > 1 )
                    {
                        if ( int.TryParse( expandedId.Substring( 1 ), out id ) )
                        {
                            expandedPageIds.Add( id );
                        }
                    }
                }
            }
            else
            {
                string pageSearch = this.PageParameter( "pageSearch" );

                // NOTE: using "Page" instead of "PageId" since PageId is already parameter of the current page
                int? selectedPageId = this.PageParameter( "Page" ).AsIntegerOrNull();
                int? selectedBlockId = this.PageParameter( "BlockId" ).AsIntegerOrNull();
                if ( !string.IsNullOrWhiteSpace( pageSearch ) )
                {
                    foreach ( Page page in pageService.Queryable().Where( a => a.InternalName.IndexOf( pageSearch ) >= 0 ) )
                    {
                        Page selectedPage = page;
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
                else if (selectedPageId.HasValue)
                {
                    expandedPageIds.Add( selectedPageId.Value );
                    hfSelectedItemId.Value = "p" + selectedPageId.ToString();
                }
                else if (selectedBlockId.HasValue)
                {
                    var selectedBlock = BlockCache.Read( selectedBlockId.Value );
                    if (selectedBlock != null )
                    {
                        if ( selectedBlock.PageId.HasValue )
                        {
                            expandedPageIds.Add( selectedBlock.PageId.Value );
                        }

                        hfSelectedItemId.Value = "b" + selectedBlock.Id.ToString();
                    }
                }
            }

            // also get any additional expanded nodes that were sent in the Post
            string postedExpandedIds = this.Request.Params["ExpandedIds"];
            if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
            {
                var postedExpandedIdList = postedExpandedIds.Split( ',' ).Select( a => a.Replace( "p", string.Empty ) ).ToList().AsIntegerList();
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

            string rootPage = GetAttributeValue( "RootPage" );
            if ( !string.IsNullOrEmpty( rootPage ) )
            {
                Guid pageGuid = rootPage.AsGuid();
                allPages = allPages.Where( a => a.ParentPage.Guid == pageGuid );
            }
            else
            {
                allPages = allPages.Where( a => a.ParentPageId == null );
            }

            foreach ( var page in allPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ).Include( a => a.Blocks ).ToList() )
            {
                sb.Append( PageNode( PageCache.Read( page ), expandedPageIds, rockContext ) );
            }

            sb.AppendLine( "</ul>" );

            lPages.Text = sb.ToString();
        }

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

            string pageSearch = this.PageParameter( "pageSearch" );

            // NOTE: using "Page" instead of "PageId" since PageId is already parameter of the current page
            int? selectedPageId = this.PageParameter( "Page" ).AsIntegerOrNull();
            int? selectedBlockId = this.PageParameter( "BlockId" ).AsIntegerOrNull();
            bool isPageSelected = false;
            if ( !string.IsNullOrWhiteSpace( pageSearch ) )
            {
                isPageSelected = page.InternalName.IndexOf( pageSearch, StringComparison.OrdinalIgnoreCase ) >= 0;
            }
            else if (selectedPageId.HasValue)
            {
                isPageSelected = page.Id == selectedPageId.Value;
            }

            bool isExpanded = expandedPageIdList.Contains( page.Id );

            sb.AppendFormat(
                @"
<li {0} data-id='p{1}'>
    <span><i class='fa fa-file-o'></i> {2}</span>
",
                isExpanded ? "data-expanded='true'" : string.Empty, // 0
                page.Id, // 1
                isPageSelected ? "<strong>" + page.InternalName + "</strong>" : page.InternalName // 2
            );

            var childPages = page.GetPages( rockContext );
            if ( childPages.Any() || page.Blocks.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var childPage in childPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
                {
                    sb.Append( PageNode( childPage, expandedPageIdList, rockContext ) );
                }

                bool includeBlocks = false;

                if ( includeBlocks )
                {
                    foreach ( var block in page.Blocks.OrderBy( b => b.Order ) )
                    {
                        string blockName = block.Name;
                        string blockCacheName = block.BlockType.Name;
                        if ( blockName != blockCacheName )
                        {
                            blockName = blockName + " (" + blockCacheName + ")";
                        }

                        var isBlockSelected = selectedBlockId.HasValue && selectedBlockId.Value == block.Id;


                        sb.AppendFormat( @"
<li data-id='b{0}'>
    <span><i class='fa {2}'></i> {1}</span> 
</li>
",
                            block.Id, // 0
                            isBlockSelected ? "<strong>" + blockName + "</strong>" : blockName, // 1
                            block.PageId.HasValue ? "fa-th-large" : "fa-th"
                        );
                    }
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        protected void lbAddPageRoot_Click( object sender, EventArgs e )
        {

        }

        protected void lbAddPageChild_Click( object sender, EventArgs e )
        {

        }

        protected void lbAddBlock_Click( object sender, EventArgs e )
        {

        }
    }
}