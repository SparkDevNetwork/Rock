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
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows a snippet of JavaScript to be executed in the browser.
    /// </summary>
    public class JavascriptBlock : LavaBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // Get the current page object if it is available.
            RockPage page = null;

            if ( HttpContext.Current != null )
            {
                page = HttpContext.Current.Handler as RockPage;
            }

            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;

            using ( TextWriter twJavascript = new StringWriter() )
            {
                base.OnRender( context, twJavascript );

                if ( parms["url"].IsNullOrWhiteSpace() )
                {
                    string javascript = "";

                    if ( !parms["disableanonymousfunction"].AsBoolean() )
                    {
                        javascript = $@"(function(){{
  {twJavascript.ToString()}
}})({parms["references"]});";
                    }
                    else
                    {
                        javascript = twJavascript.ToString();
                    }

                    var scriptText = $"{Environment.NewLine}<script>{javascript}</script>{Environment.NewLine}";

                    if ( parms.ContainsKey( "id" ) )
                    {
                        var identifier = parms["id"];
                        if ( identifier.IsNotNullOrWhiteSpace() )
                        {
                            var controlId = "js-" + identifier;
                            System.Web.UI.Control scriptControl = null;

                            if ( page != null )
                            {
                                scriptControl = page.Header.FindControl( controlId );

                                if ( scriptControl == null )
                                {
                                    scriptControl = new System.Web.UI.LiteralControl( scriptText );
                                    scriptControl.ID = controlId;
                                    page.Header.Controls.Add( scriptControl );
                                }
                            }
                            else
                            {
                                result.Write( scriptText );
                            }
                        }
                    }
                    else
                    {
                        if ( page != null )
                        {
                            page.Header.Controls.Add( new System.Web.UI.LiteralControl( scriptText ) );
                        }
                        else
                        {
                            result.Write( scriptText );
                        }
                    }
                }
                else
                {
                    var url = ResolveRockUrl( context, parms["url"] );

                    var scriptText = $"{Environment.NewLine}<script src='{url}' type='text/javascript'></script>{Environment.NewLine}";
                    if ( parms.ContainsKey( "id" ) )
                    {
                        var identifier = parms["id"];
                        if ( identifier.IsNotNullOrWhiteSpace() )
                        {
                            var controlId = "js-" + identifier;
                            System.Web.UI.Control scriptControl = null;

                            if ( page != null )
                            {
                                scriptControl = page.Header.FindControl( controlId );

                                if ( scriptControl == null )
                                {
                                    scriptControl = new System.Web.UI.LiteralControl( scriptText );
                                    scriptControl.ID = controlId;
                                    page.Header.Controls.Add( scriptControl );
                                }
                            }
                            else
                            {
                                result.Write( scriptText );
                            }
                        }
                    }
                    else
                    {
                        if ( page != null )
                        {
                            page.Header.Controls.Add( new System.Web.UI.LiteralControl( scriptText ) );
                        }
                        else
                        {
                            result.Write( scriptText );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="context">The lava render context.</param>
        /// <param name="url">The URL.</param>
        /// <returns>The resolved rock URL.</returns>
        private string ResolveRockUrl( ILavaRenderContext context, string url )
        {
            // Some requests (i.e. Obsidian block action requests) won't have a RockPage,
            // so try to resolve the URL using the RockRequestContext first.
            var rockRequestContext = context?.GetRockRequestContext();
            if ( rockRequestContext != null )
            {
                return rockRequestContext.ResolveRockUrl( url );
            }

            var page = HttpContext.Current?.Handler as RockPage;
            if ( page == null )
            {
                // If we are not operating in the context of a page, return the unresolved URL.
                return url;
            }

            if ( url.StartsWith( "~~" ) )
            {
                string theme = "Rock";
                if ( page.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Theme;
                }
                else if ( page.Site != null && page.Site.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Site.Theme;
                }

                url = "~/Themes/" + theme + ( url.Length > 2 ? url.Substring( 2 ) : string.Empty );
            }

            return page.ResolveUrl( url );
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "cacheduration", "0" );
            settings.AddOrIgnore( "references", string.Empty );
            settings.AddOrIgnore( "disableanonymousfunction", "false" );
            settings.AddOrIgnore( "url", string.Empty );

            return settings;
        }
    }
}