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
using Rock.Model;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Page Menu" )]
    [Category( "CMS" )]
    [Description( "Renders a page menu based on a root page and lava template." )]

    #region Block Attributes

    [CodeEditorField(
        "Template",
        Description = "The lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/PageNav.lava' %}",
        Order = 0,
        Key = AttributeKey.Template )]
    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this content channel item block.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.EnabledLavaCommands )]
    [LinkedPage(
        "Root Page",
        Description = "The root page to use for the page collection. Defaults to the current page instance if not set.",
        IsRequired = false,
        Key = AttributeKey.RootPage )]
    [TextField(
        "Number of Levels",
        Description = "Number of parent-child page levels to display. Default 3.",
        IsRequired = false,
        DefaultValue = "3",
        Key = AttributeKey.NumberofLevels )]
    [TextField(
        "CSS File",
        Description = "Optional CSS file to add to the page for styling. Example 'Styles/nav.css' would point the style sheet in the current theme's styles folder.",
        IsRequired = false,
        Key = AttributeKey.CSSFile )]
    [BooleanField(
        "Include Current Parameters",
        Description = "Flag indicating if current page's route parameters should be used when building URL for child pages",
        DefaultBooleanValue = false,
        Key = AttributeKey.IncludeCurrentParameters )]
    [BooleanField(
        "Include Current QueryString",
        Description = "Flag indicating if current page's QueryString should be used when building URL for child pages",
        DefaultBooleanValue = false,
        Key = AttributeKey.IncludeCurrentQueryString )]
    [BooleanField(
        "Is Secondary Block",
        Description = "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",
        DefaultBooleanValue = false,
        Key = AttributeKey.IsSecondaryBlock )]
    [KeyValueListField(
        "Include Page List",
        Description = "List of pages to include in the Lava. Any ~/ will be resolved by Rock. Enable debug for assistance. Example 'Give Now' with '~/page/186' or 'Me' with '~/MyAccount'.",
        IsRequired = false,
        KeyPrompt = "Title",
        ValuePrompt = "Link",
        Key = AttributeKey.IncludePageList )]

    #endregion
    public partial class PageMenu : RockBlock, ISecondaryBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Template = "Template";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string RootPage = "RootPage";
            public const string NumberofLevels = "NumberofLevels";
            public const string CSSFile = "CSSFile";
            public const string IncludeCurrentParameters = "IncludeCurrentParameters";
            public const string IncludeCurrentQueryString = "IncludeCurrentQueryString";
            public const string IsSecondaryBlock = "IsSecondaryBlock";
            public const string IncludePageList = "IncludePageList";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            this.EnableViewState = false;

            base.OnInit( e );

            this.BlockUpdated += PageMenu_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );

            // add css file to page
            if ( GetAttributeValue( AttributeKey.CSSFile ).Trim() != string.Empty )
            {
                RockPage.AddCSSLink( ResolveRockUrl( GetAttributeValue( AttributeKey.CSSFile ) ), false );
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
            string content = null;

            try
            {
                PageCache currentPage = PageCache.Get( RockPage.PageId );
                PageCache rootPage = null;

                var pageRouteValuePair = GetAttributeValue( AttributeKey.RootPage ).SplitDelimitedValues(false).AsGuidOrNullList();
                if ( pageRouteValuePair.Any() && pageRouteValuePair[0].HasValue && !pageRouteValuePair[0].Value.IsEmpty() )
                {
                    rootPage = PageCache.Get( pageRouteValuePair[0].Value );
                }

                // If a root page was not found, use current page
                if ( rootPage == null )
                {
                    rootPage = currentPage;
                }

                int levelsDeep = Convert.ToInt32( GetAttributeValue( AttributeKey.NumberofLevels ) );

                Dictionary<string, string> pageParameters = null;
                if ( GetAttributeValue( AttributeKey.IncludeCurrentParameters ).AsBoolean() )
                {
                    pageParameters = CurrentPageReference.Parameters;
                }

                NameValueCollection queryString = null;
                if ( GetAttributeValue( AttributeKey.IncludeCurrentQueryString ).AsBoolean() )
                {
                    queryString = CurrentPageReference.QueryString;
                }

                // Get list of pages in current page's hierarchy
                var pageHeirarchy = new List<int>();
                if ( currentPage != null )
                {
                    pageHeirarchy = currentPage.GetPageHierarchy().Select( p => p.Id ).ToList();
                }

                // Get default merge fields.
                var pageProperties = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                pageProperties.Add( "Site", GetSiteProperties( RockPage.Site ) );
                pageProperties.Add( "IncludePageList", GetIncludePageList() );
                pageProperties.Add( "CurrentPage", this.PageCache );

                using ( var rockContext = new RockContext() )
                {
                    pageProperties.Add( "Page", rootPage.GetMenuProperties( levelsDeep, CurrentPerson, rockContext, pageHeirarchy, pageParameters, queryString ) );
                }
           
                var lavaTemplate = GetTemplate();

                // Apply Enabled Lava Commands
                var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                lavaTemplate.Registers.AddOrReplace( "EnabledCommands", enabledCommands);

                content = lavaTemplate.Render( Hash.FromDictionary( pageProperties ) );

                // Check for Lava rendering errors.
                if ( lavaTemplate.Errors.Any() )
                {
                    throw lavaTemplate.Errors.First();
                }

                phContent.Controls.Clear();
                phContent.Controls.Add( new LiteralControl( content ) );

            }
            catch ( Exception ex )
            {
                LogException( ex );

                // Create a block showing the error and the attempted content render.
                // Show the error first to ensure that it is visible, because the rendered content may disrupt subsequent output if it is malformed.
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.Append( "<div class='alert alert-warning'>" );
                errorMessage.Append( "<h4>Warning</h4>" );
                errorMessage.Append( "An error has occurred while generating the page menu. Error details:<br/>" );
                errorMessage.Append( ex.Message );

                if ( !string.IsNullOrWhiteSpace( content ) )
                {
                    errorMessage.Append( "<h4>Rendered Content</h4>" );
                    errorMessage.Append( content );
                    errorMessage.Append( "</div>" );
                }

                phContent.Controls.Add( new LiteralControl( errorMessage.ToString() ) );
            }
        }

        #endregion Base Control Methods

        #region Methods

        private string CacheKey()
        {
            return string.Format( "Rock:PageMenu:{0}", BlockId );
        }

        private Template GetTemplate()
        {
            var cacheTemplate = LavaTemplateCache.Get( CacheKey(), GetAttributeValue( AttributeKey.Template ) );
            return cacheTemplate != null ? cacheTemplate.Template : null;
        }

        /// <summary>
        /// Will not display the block information if it is considered a secondary block and secondary blocks are being hidden.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            if ( GetAttributeValue( AttributeKey.IsSecondaryBlock ).AsBoolean() )
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

            var navPagesString = GetAttributeValue( AttributeKey.IncludePageList );

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