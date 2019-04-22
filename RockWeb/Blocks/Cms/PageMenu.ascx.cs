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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Page Menu" )]
    [Category( "CMS" )]
    [Description( "Renders a page menu based on a root page and lava template." )]
    [CodeEditorField( "Template", "The lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, @"{% include '~~/Assets/Lava/PageNav.lava' %}", order: 0)]
    [LavaCommandsField("Enabled Lava Commands", description: "The Lava commands that should be enabled for this content channel item block.", required: false, order: 1)]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]
    [TextField( "Number of Levels", "Number of parent-child page levels to display. Default 3.", false, "3" )]
    [TextField( "CSS File", "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the style sheet in the current theme's styles folder.", false, "" )]
    [BooleanField( "Include Current Parameters", "Flag indicating if current page's route parameters should be used when building URL for child pages", false )]
    [BooleanField( "Include Current QueryString", "Flag indicating if current page's QueryString should be used when building URL for child pages", false )]
    [BooleanField( "Is Secondary Block", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", false )]
    [KeyValueListField( "Include Page List", "List of pages to include in the Lava. Any ~/ will be resolved by Rock. Enable debug for assistance. Example 'Give Now' with '~/page/186' or 'Me' with '~/MyAccount'.", false, "", "Title", "Link" )]

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
            LavaTemplateCache.Remove( CacheKey() );
        }

        private void Render()
        {
            try
            {
                PageCache currentPage = PageCache.Get( RockPage.PageId );
                PageCache rootPage = null;

                var pageRouteValuePair = GetAttributeValue( ROOT_PAGE ).SplitDelimitedValues(false).AsGuidOrNullList();
                if ( pageRouteValuePair.Any() && pageRouteValuePair[0].HasValue && !pageRouteValuePair[0].Value.IsEmpty() )
                {
                    rootPage = PageCache.Get( pageRouteValuePair[0].Value );
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

                // Get list of pages in current page's hierarchy
                var pageHeirarchy = new List<int>();
                if ( currentPage != null )
                {
                    pageHeirarchy = currentPage.GetPageHierarchy().Select( p => p.Id ).ToList();
                }

                // Add context to merge fields
                var contextEntityTypes = RockPage.GetContextEntityTypes();
                var contextObjects = new Dictionary<string, object>();
                foreach ( var conextEntityType in contextEntityTypes )
                {
                    var contextObject = RockPage.GetCurrentContext( conextEntityType );
                    contextObjects.Add( conextEntityType.FriendlyName, contextObject );
                }

                var pageProperties = new Dictionary<string, object>();
                pageProperties.Add( "CurrentPerson", CurrentPerson );
                pageProperties.Add( "Context", contextObjects );
                pageProperties.Add( "Site", GetSiteProperties( RockPage.Site ) );
                pageProperties.Add( "IncludePageList", GetIncludePageList() );
                pageProperties.Add( "CurrentPage", this.PageCache );

                using ( var rockContext = new RockContext() )
                {
                    pageProperties.Add( "Page", rootPage.GetMenuProperties( levelsDeep, CurrentPerson, rockContext, pageHeirarchy, pageParameters, queryString ) );
                }

                var lavaTemplate = GetTemplate();

                // Apply Enabled Lava Commands
                var enabledCommands = GetAttributeValue( "EnabledLavaCommands" );
                lavaTemplate.Registers.AddOrReplace( "EnabledCommands", enabledCommands);

                string content = lavaTemplate.Render( Hash.FromDictionary( pageProperties ) );

                // check for errors
                if ( content.Contains( "error" ) )
                {
                    content = "<div class='alert alert-warning'><h4>Warning</h4>" + content + "</div>";
                }

                phContent.Controls.Clear();
                phContent.Controls.Add( new LiteralControl( content ) );

            }
            catch ( Exception ex )
            {
                LogException( ex );
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.Append( "<div class='alert alert-warning'>" );
                errorMessage.Append( "An error has occurred while generating the page menu. Error details:" );
                errorMessage.Append( ex.Message );
                errorMessage.Append( "</div>" );

                phContent.Controls.Add( new LiteralControl( errorMessage.ToString() ) );
            }
        }

        #region Methods

        private string CacheKey()
        {
            return string.Format( "Rock:PageMenu:{0}", BlockId );
        }

        private Template GetTemplate()
        {
            var cacheTemplate = LavaTemplateCache.Get( CacheKey(), GetAttributeValue( "Template" ) );
            return cacheTemplate != null ? cacheTemplate.Template : null;
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

        /// <summary>
        /// Gets the site *PageId properties.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A dictionary of various page ids for the site.</returns>
        private Dictionary<string, object> GetSiteProperties( SiteCache site )
        {
            var properties = new Dictionary<string, object>();
            properties.Add( "DefaultPageId", site.DefaultPageId );
            properties.Add( "LoginPageId", site.LoginPageId );
            properties.Add( "PageNotFoundPageId", site.PageNotFoundPageId );
            properties.Add( "CommunicationPageId", site.CommunicationPageId );
            properties.Add( "RegistrationPageId ", site.RegistrationPageId );
            properties.Add( "MobilePageId", site.MobilePageId );
            return properties;
        }

        /// <summary>
        /// Gets the include page list as a dictionary to be included in the Lava.
        /// </summary>
        /// <returns>A dictionary of Titles with their Links.</returns>
        private Dictionary<string, object> GetIncludePageList()
        {
            var properties = new Dictionary<string, object>();

            var navPagesString = GetAttributeValue( "IncludePageList" );

            if ( !string.IsNullOrWhiteSpace( navPagesString ) )
            {
                navPagesString = navPagesString.TrimEnd( '|' );
                var navPages = navPagesString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Select( p => new { Title = p[0], Link = p[1] } );

                StringBuilder sbPageMarkup = new StringBuilder();
                foreach ( var page in navPages )
                {
                    properties.Add( page.Title, Page.ResolveUrl( page.Link ) );
                }
            }
            return properties;
        }

        #endregion

    }
}