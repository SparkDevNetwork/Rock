//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;

using DotLiquid;
using System.Text;
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Cms
{
    [CodeEditorField( "Template", "The liquid template to use for rendering. This template should be in the theme's 'Assets/Liquid' folder and should have an underscore prepended to the filename. ", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"
{% include 'PageNav' %}
" )]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]
    [TextField( "Number of Levels", "Number of parent-child page levels to display. Default 3.", false, "3" )]
    [TextField( "CSS File", "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the stylesheet in the current theme's styles folder.", false, "" )]
    [BooleanField( "Include Current Parameters", "Flag indicating if current page's parameters should be used when building url for child pages", false )]
    [BooleanField( "Include Current QueryString", "Flag indicating if current page's QueryString should be used when building url for child pages", false )]
    [BooleanField( "Enable Debug", "Flag indicating that the control should output the page data that will be passed to Liquid for parsing.", false )]

    public partial class PageLiquid : Rock.Web.UI.RockBlock
    {
        private static readonly string ROOT_PAGE = "RootPage";
        private static readonly string NUM_LEVELS = "NumberofLevels";

        protected override void OnInit( EventArgs e )
        {
            this.EnableViewState = false;

            base.OnInit( e );

            this.BlockUpdated += PageLiquid_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );

            // add css file to page
            if ( GetAttributeValue( "CSSFile" ).Trim() != string.Empty )
                RockPage.AddCSSLink( ResolveRockUrl( GetAttributeValue( "CSSFile" ) ) );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
            Render();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PageLiquid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void PageLiquid_BlockUpdated( object sender, EventArgs e )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( CacheKey() );
        }

        private void Render()
        {
            Rock.Web.Cache.PageCache rootPage = null;

            Guid pageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( ROOT_PAGE ), out pageGuid ) )
            {
                rootPage = Rock.Web.Cache.PageCache.Read( pageGuid );
            }

            if ( rootPage == null )
            {
                rootPage = Rock.Web.Cache.PageCache.Read( RockPage.Guid );
            }

            int levelsDeep = Convert.ToInt32( GetAttributeValue( NUM_LEVELS ) );

            Dictionary<string, string> pageParameters = null;
            if ( GetAttributeValue( "IncludeCurrentParameters" ).AsBoolean() )
            {
                pageParameters = CurrentPageReference.Parameters;
            }

            NameValueCollection queryString = null;
            if ( GetAttributeValue( "IncludeCurrentQueryString" ).AsBoolean() )
            {
                queryString = CurrentPageReference.QueryString;
            }

            var pageHeirarchy = RockPage.PageHierarchy.Select( p => p.Id ).ToList();

            var pageProperties = new Dictionary<string, object>();
            pageProperties.Add( "page", rootPage.GetMenuProperties( levelsDeep, CurrentPerson, pageHeirarchy, pageParameters, queryString ) );
            string content = GetTemplate().Render( Hash.FromDictionary( pageProperties ) );

            // check for errors
            if ( content.Contains( "No such template" ) )
            {
                // get template name
                Match match = Regex.Match( GetAttributeValue( "Template" ), @"'([^']*)" );
                if ( match.Success )
                {
                    content = String.Format( "<div class='alert alert-warning'><h4>Warning</h4>Could not find the template _{1}.liquid in {0}.</div>", ResolveRockUrl( "~~/Assets/Liquid" ), match.Groups[1].Value );
                }
                else
                {
                    content = "<div class='alert alert-warning'><h4>Warning</h4>Unable to parse the template name from settings.</div>";
                }
            }

            if ( content.Contains( "error" ) )
            {
                content = "<div class='alert alert-warning'><h4>Warning</h4>" + content + "</div>";
            }

            phContent.Controls.Clear();
            phContent.Controls.Add( new LiteralControl( content ) );

            // add debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() )
            {
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.Append( "<p /><div class='alert alert-info'><h4>Debug Info</h4>" );

                debugInfo.Append("<p><em>Note:</em> If a page or group of pages is not in the data above check the following: <ul>");
                debugInfo.Append("<li>The parent page has 'Show Child Pages' enabled in the 'Page Properties' > 'Display Settings'</li>");
                debugInfo.Append("<li>Check the 'Display Settings' on the child pages</li>");
                debugInfo.Append("<li>Check the security of the child pages</li>");
                debugInfo.Append("</ul><br /></p>");

                debugInfo.Append( "<pre>" );
                debugInfo.Append( "<p /><strong>Page Data</strong> (referenced as 'page.' in Liquid)<br>" );
                debugInfo.Append( rootPage.GetMenuProperties( levelsDeep, CurrentPerson, pageHeirarchy, pageParameters, queryString ).ToJson() + "</pre>" );

                debugInfo.Append( "</div>" );
                phContent.Controls.Add( new LiteralControl( debugInfo.ToString() ) );
            }

        }

        private string CacheKey()
        {
            return string.Format( "Rock:PageLiquid:{0}", BlockId );
        }

        private Template GetTemplate()
        {
            string liquidFolder = System.Web.HttpContext.Current.Server.MapPath( ResolveRockUrl( "~~/Assets/Liquid" ) );
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem( liquidFolder );

            string cacheKey = CacheKey();

            ObjectCache cache = MemoryCache.Default;
            Template template = cache[cacheKey] as Template;

            if ( template != null )
            {
                return template;
            }
            else
            {
                template = Template.Parse( GetAttributeValue( "Template" ) );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, template, cachePolicy );

                return template;
            }
        }
    }
}