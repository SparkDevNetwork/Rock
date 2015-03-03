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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Cms
{
    [DisplayName("Page Menu")]
    [Category("CMS")]
    [Description("Renders a page menu based on a root page and liquid template.")]
    [CodeEditorField( "Template", "The liquid template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include '~~/Assets/Lava/PageNav.lava' %}" )]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]
    [TextField( "Number of Levels", "Number of parent-child page levels to display. Default 3.", false, "3" )]
    [TextField( "CSS File", "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the stylesheet in the current theme's styles folder.", false, "" )]
    [BooleanField( "Include Current Parameters", "Flag indicating if current page's parameters should be used when building url for child pages", false )]
    [BooleanField( "Include Current QueryString", "Flag indicating if current page's QueryString should be used when building url for child pages", false )]
    [BooleanField( "Enable Debug", "Flag indicating that the control should output the page data that will be passed to Liquid for parsing.", false )]
    [BooleanField( "Is Secondary Block", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", false )]
    public partial class PageMenu : RockBlock, ISecondaryBlock
    {
        private static readonly string ROOT_PAGE = "RootPage";
        private static readonly string NUM_LEVELS = "NumberofLevels";

        protected override void OnInit( EventArgs e )
        {
            this.EnableViewState = false;

            base.OnInit( e );

            this.BlockUpdated += PageMenu_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );

            // add css file to page
            if ( GetAttributeValue( "CSSFile" ).Trim() != string.Empty )
            {
                RockPage.AddCSSLink( ResolveRockUrl( GetAttributeValue( "CSSFile" ) ), false );
            }
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
        /// Handles the BlockUpdated event of the PageMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void PageMenu_BlockUpdated( object sender, EventArgs e )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( CacheKey() );
        }

        private void Render()
        {
            try
            {
                RockContext rockContext = new RockContext();
                PageCache currentPage = PageCache.Read( RockPage.PageId, rockContext );
                PageCache rootPage = null;

                Guid pageGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( ROOT_PAGE ), out pageGuid ) )
                {
                    rootPage = PageCache.Read( pageGuid );
                }

                // If a root page was not found, use current page
                if ( rootPage == null )
                {
                    rootPage = currentPage;
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

                // Get list of pages in curren't page's heirarchy
                var pageHeirarchy = new List<int>();
                if ( currentPage != null )
                {
                    pageHeirarchy = currentPage.GetPageHierarchy().Select( p => p.Id ).ToList();
                }

                var pageProperties = new Dictionary<string, object>();
                pageProperties.Add( "Page", rootPage.GetMenuProperties( levelsDeep, CurrentPerson, rockContext, pageHeirarchy, pageParameters, queryString ) );
                string content = GetTemplate().Render( Hash.FromDictionary( pageProperties ) );

                // check for errors
                if ( content.Contains( "error" ) )
                {
                    content = "<div class='alert alert-warning'><h4>Warning</h4>" + content + "</div>";
                }

                phContent.Controls.Clear();
                phContent.Controls.Add( new LiteralControl( content ) );

                // add debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    StringBuilder tipInfo = new StringBuilder();
                    tipInfo.Append( "<p /><div class='alert alert-success' style='clear: both;'><h4>Page Menu Tips</h4>" );

                    tipInfo.Append( "<p><em>Note:</em> If a page or group of pages is not in the data above check the following: <ul>" );
                    tipInfo.Append( "<li>The parent page has 'Show Child Pages' enabled in the 'Page Properties' > 'Display Settings'</li>" );
                    tipInfo.Append( "<li>Check the 'Display Settings' on the child pages</li>" );
                    tipInfo.Append( "<li>Check the security of the child pages</li>" );
                    tipInfo.Append( "</ul><br /></p>" );
                    tipInfo.Append( "</div>" );

                    phContent.Controls.Add( new LiteralControl( tipInfo.ToString() + pageProperties.lavaDebugInfo() ) );
                }
            }
            catch ( Exception ex )
            {
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.Append( "<div class='alert alert-warning'>");
                errorMessage.Append( "An error has occurred while generating the page menu. Error details:" );
                errorMessage.Append( ex.Message );
                errorMessage.Append( "</div>" );

                phContent.Controls.Add( new LiteralControl( errorMessage.ToString()) );
                
            }

        }

        private string CacheKey()
        {
            return string.Format( "Rock:PageMenu:{0}", BlockId );
        }

        private Template GetTemplate()
        {
            string cacheKey = CacheKey();

            ObjectCache cache = RockMemoryCache.Default;
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

        /// <summary>
        /// Will not display the block information if it is considered a secondary block and secondary blocks are being hidden.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            if ( GetAttributeValue( "IsSecondaryBlock" ).AsBoolean() )
            {
                phContent.Visible = visible;
            }
        }
    }
}