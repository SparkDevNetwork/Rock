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

using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility.ExtensionMethods;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Page Menu" )]
    [Category( "CMS" )]
    [Description( "Renders a page menu based on a root page and lava template." )]

    #region Block Attributes

    [CodeEditorField(
        "Template",
        Description = "The lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Web.UI.Controls.CodeEditorTheme.Rock,
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

    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [Rock.Cms.DefaultBlockRole( BlockRole.Navigation )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.PAGE_MENU )]
    [Rock.SystemGuid.EntityTypeGuid( "7c797c18-b632-4fa7-a088-04e583c2a8c7" )]
    public class PageMenu : RockBlockType
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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var cssFile = GetAttributeValue( AttributeKey.CSSFile );

            // add css file to page
            if ( cssFile.IsNotNullOrWhiteSpace() )
            {
                RequestContext.Response.AddCssLink( RequestContext.ResolveRockUrl( cssFile ), false );
            }

            return base.GetObsidianBlockInitialization();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return Render();
        }

        private string Render()
        {
            string content = null;

            try
            {
                var currentPage = PageCache;
                PageCache rootPage = null;

                var pageRouteValuePair = GetAttributeValue( AttributeKey.RootPage ).SplitDelimitedValues( false ).AsGuidOrNullList();
                if ( pageRouteValuePair.Any() && pageRouteValuePair[0].HasValue && !pageRouteValuePair[0].Value.IsEmpty() )
                {
                    rootPage = PageCache.Get( pageRouteValuePair[0].Value );
                }

                // If a root page was not found, use current page
                if ( rootPage == null )
                {
                    rootPage = currentPage;
                }

                int levelsDeep = GetAttributeValue( AttributeKey.NumberofLevels ).AsInteger();

                Dictionary<string, string> pageParameters = null;
                if ( GetAttributeValue( AttributeKey.IncludeCurrentParameters ).AsBoolean() )
                {
                    pageParameters = new Dictionary<string, string>( RequestContext.PageParameters );
                }

                NameValueCollection queryString = null;
                if ( GetAttributeValue( AttributeKey.IncludeCurrentQueryString ).AsBoolean() )
                {
                    queryString = RequestContext.QueryString;
                }

                // Get list of pages in current page's hierarchy
                var pageHeirarchy = new List<int>();
                if ( currentPage != null )
                {
                    pageHeirarchy = currentPage.GetPageHierarchy().Select( p => p.Id ).ToList();
                }

                // Get default merge fields.
                var pageProperties = RequestContext.GetCommonMergeFields();
                pageProperties.Add( "Site", GetSiteProperties( PageCache.Layout.Site ) );
                pageProperties.Add( "IncludePageList", GetIncludePageList() );
                pageProperties.Add( "CurrentPage", this.PageCache );
                pageProperties.Add( "Page", rootPage.GetMenuProperties( levelsDeep, RequestContext.CurrentPerson, RockContext, pageHeirarchy, pageParameters, queryString ) );

                var templateText = GetAttributeValue( AttributeKey.Template );
                var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

                /**
                 * 2025-011-06 - DSH
                 * 
                 * This originally used custom template parsing code with a custom cache
                 * key. However, that added lots of complexity and didn't seem to provide
                 * any benefit over the standard Lava template caching that is already in
                 * place.
                 * 
                 * If weird errors start cropping up, consider revisiting this decision.
                 */
                return templateText.ResolveMergeFields( pageProperties, enabledCommands, throwExceptionOnErrors: true );

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null, PageCache.Id, PageCache.Layout.SiteId, RequestContext.CurrentPerson?.PrimaryAlias );

                // Create a block showing the error and the attempted content render.
                // Show the error first to ensure that it is visible, because the rendered content may disrupt subsequent output if it is malformed.
                var errorMessage = new StringBuilder();
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

                return errorMessage.ToString();
            }
        }

        /// <summary>
        /// Gets the site *PageId properties.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A dictionary of various page ids for the site.</returns>
        private Dictionary<string, object> GetSiteProperties( SiteCache site )
        {
            return new Dictionary<string, object>
            {
                { "DefaultPageId", site.DefaultPageId },
                { "LoginPageId", site.LoginPageId },
                { "PageNotFoundPageId", site.PageNotFoundPageId },
                { "CommunicationPageId", site.CommunicationPageId },
                { "RegistrationPageId ", site.RegistrationPageId },
                { "MobilePageId", site.MobilePageId }
            };
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
                    properties.Add( page.Title, RequestContext.ResolveRockUrl( page.Link ) );
                }
            }
            return properties;
        }

        #endregion
    }
}
