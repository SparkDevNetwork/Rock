
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

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "CCV SEO Map" )]
    [Category( "CCV > Cms" )]
    [Description( "Displays a site map designed to make it easy for Google to crawl" )]
    [LinkedPage( "Root Page", "Select the root page to use as a starting point for the tree view. A page must be specified, or nothing will render.", true )]
    public partial class CCVSEOMap: RockBlock
    {
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
            
            var sb = new StringBuilder();

            string rootPage = GetAttributeValue("RootPage");
            if( string.IsNullOrWhiteSpace( rootPage ) == false )
            {
                sb.AppendLine( "<ul id=\"treeview\">" );
                
                Guid pageGuid = rootPage.AsGuid();
                var rootPageObj = allPages.Where( a => a.Guid == pageGuid ).FirstOrDefault( );
                
                sb.Append( PageNode( PageCache.Read( rootPageObj ), rockContext ) );

                sb.AppendLine( "</ul>" );

                lPages.Text = sb.ToString();
            }
        }

        /// <summary>
        /// Adds the page nodes.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected string PageNode( PageCache page, RockContext rockContext )
        {
            var sb = new StringBuilder();

            // if this page has routes, we should render it. Otherwise, we'll skip it,
            // and only render the child pages (if there are any)
            bool renderPage = page.PageRoutes.Count > 0 ? true : false;
            
            if( renderPage )
            {
                sb.AppendFormat(
                    "<li data-expanded='true' data-model='Page' data-id='p{0}'><span><span class='rollover-container'><i class=\"fa fa-file-o\">&nbsp;</i><a href='{1}'>{2}</a> </span></span>{3}",
                    page.Id, // 0
                    new PageReference( page.Id ).BuildUrl( ), // 1
                    page.InternalName, // 2
                    Environment.NewLine // 3
                );
            }

            var childPages = page.GetPages( rockContext );
            if ( childPages.Any() )
            {
                // if this page IS rendering, then we need to create a new UL for all child pages.
                // Otherwise, we are already within a UL, and don't need another one.
                if( renderPage )
                {
                    sb.AppendLine( "<ul>" );
                }

                foreach ( var childPage in childPages.OrderBy( a => a.Order ).ThenBy( a => a.InternalName ) )
                {
                    sb.Append( PageNode( childPage, rockContext ) );
                }

                if( renderPage )
                {
                    sb.AppendLine( "</ul>" );
                }
            }

            if( renderPage )
            {
                sb.AppendLine( "</li>" );
            }

            return sb.ToString();
        }
    }
}