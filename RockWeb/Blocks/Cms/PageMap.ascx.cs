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

            var allPages = pageService.Queryable( "PageContexts, PageRoutes" );

            foreach ( var page in allPages )
            {
                PageCache.Read( page );
            }

            foreach ( var block in new BlockService(rockContext).Queryable() )
            {
                BlockCache.Read( block );
            }

            foreach ( var blockType in new BlockTypeService( rockContext ).Queryable() )
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
            }

            var sb = new StringBuilder();

            sb.AppendLine( "<ul id=\"treeview\">" );
            
            string rootPage = GetAttributeValue("RootPage");
            if ( ! string.IsNullOrEmpty( rootPage ) )
            {
                Guid pageGuid = rootPage.AsGuid();
                allPages = allPages.Where( a => a.ParentPage.Guid == pageGuid );
            }
            else
            {
                allPages = allPages.Where( a => a.ParentPageId == null );
            }

            foreach ( var page in allPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ).Include(a => a.Blocks).ToList() )
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
            bool isSelected = false;
            if ( !string.IsNullOrWhiteSpace( pageSearch ) )
            {
                isSelected = page.InternalName.IndexOf( pageSearch, StringComparison.OrdinalIgnoreCase ) >= 0;
            }

            bool isExpanded = expandedPageIdList.Contains( page.Id );

            var authRoles = Authorization.AuthRules( EntityTypeCache.Read<Rock.Model.Page>().Id, page.Id, Authorization.VIEW );
            string authHtml = string.Empty;
            if (authRoles.Any())
            {
                authHtml += string.Format(
                    "&nbsp<i class=\"fa fa-lock\">&nbsp;</i>{0}",
                    authRoles.Select( a => "<span class=\"badge badge-" + (a.AllowOrDeny == 'A' ? "success" : "danger")  + "\">" + a.DisplayName + "</span>"  ).ToList().AsDelimited( " " ) );
            }

            int pageEntityTypeId = EntityTypeCache.Read( "Rock.Model.Page" ).Id;

            sb.AppendFormat(
                "<li data-expanded='{4}' data-model='Page' data-id='p{0}'><span><span class='rollover-container'><i class=\"fa fa-file-o\">&nbsp;</i><a href='{1}'>{2}</a> {6} {7}<span class='js-auth-roles hidden'>{5}</span></span></span>{3}", 
                page.Id, // 0
                new PageReference( page.Id ).BuildUrl(), // 1
                isSelected ? "<strong>" + page.InternalName + "</strong>" : page.InternalName, // 2
                Environment.NewLine, // 3
                isExpanded.ToString().ToLower(), // 4
                authHtml, // 5
                CreatePageCopyIcon( page ), // 6
                CreatePageConfigIcon( page ), // 7
                CreateSecurityIcon( pageEntityTypeId, page.Id, page.InternalName ) // 8
            );

            var childPages = page.GetPages( rockContext );
            if ( childPages.Any() || page.Blocks.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var childPage in childPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
                {
                    sb.Append( PageNode( childPage, expandedPageIdList, rockContext ) );
                }

                foreach ( var block in page.Blocks.OrderBy( b => b.Order ) )
                {
                    string blockName = block.Name;
                    string blockCacheName = BlockTypeCache.Read( block.BlockTypeId, rockContext ).Name;
                    if (blockName != blockCacheName)
                    {
                        blockName = blockName + " (" + blockCacheName + ")";
                    }

                    int blockEntityTypeId = EntityTypeCache.Read( "Rock.Model.Block" ).Id;

                    sb.AppendFormat( "<li data-expanded='false' data-model='Block' data-id='b{0}'><span><span class='rollover-container'> <i class='fa fa-th-large'>&nbsp;</i> {2} {1} {4}</span></span></li>{3}", 
                        block.Id, // 0
                        CreateConfigIcon( block ), // 1
                        blockName,  // 2
                        Environment.NewLine, //3
                        CreateSecurityIcon( blockEntityTypeId, block.Id, block.Name ) // 4
                    );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        /// <summary>
        /// Creates the copy icon.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        protected string CreatePageCopyIcon( PageCache page )
        {
            return string.Format(
                "&nbsp;<span class='rollover-item' onclick=\"javascript: __doPostBack('CopyPage', '{0}'); event.stopImmediatePropagation();\" title=\"Clone Page and Descendants\"><i class=\"fa fa-clone\"></i>&nbsp;</span>",
                page.Id );
        }

        /// <summary>
        /// Creates the block config icon.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        protected string CreateConfigIcon( BlockCache block )
        {
            var blockPropertyUrl = ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", block.Id ) );

            return string.Format(
                "<a class='rollover-item' href=\"javascript: Rock.controls.modal.show($(this), '{0}')\" title=\"Block Properties\"><i class=\"fa fa-cog\"></i>&nbsp;</a>",
                blockPropertyUrl );
        }

        protected string CreateSecurityIcon(int entityTypeId, int entityId, string title )
        {
            string url = this.Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done", entityTypeId, entityId, HttpUtility.JavaScriptStringEncode( title ) ) );
            return string.Format( "<span class='rollover-item' onclick=\"javascript: Rock.controls.modal.show($(this), '{0}'); event.stopImmediatePropagation();\" title=\"Security\"><i class=\"fa fa-lock\">&nbsp;</i></span>", url );
        }

        /// <summary>
        /// Creates the page configuration icon.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected string CreatePageConfigIcon( PageCache page )
        {
            var pagePropertyUrl = ResolveUrl( string.Format( "~/PageProperties/{0}?t=Page Properties", page.Id ) );

            return string.Format(
                "&nbsp;<span class='rollover-item' onclick=\"javascript: Rock.controls.modal.show($(this), '{0}'); event.stopImmediatePropagation();\" title=\"Page Properties\"><i class=\"fa fa-cog\"></i>&nbsp;</span>",
                pagePropertyUrl );
        }
    }
}