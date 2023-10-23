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
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using DotLiquid;
using Block = DotLiquid.Block;
using Rock.Utility;
using Rock.Web.UI;
using Rock.Lava.Blocks;
using Rock.Lava.DotLiquid;

namespace Rock.Lava.RockLiquid.Blocks
{
    /// <summary>
    /// Tag which allows a snippet of JavaScript to be executed in the browser.
    /// </summary>
    public class Javascript : Block, IRockStartup
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            Template.RegisterTag<Javascript>( "javascript" );
        }

        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            // Get the current page object if it is available.
            RockPage page = null;

            if ( HttpContext.Current != null )
            {
                page = HttpContext.Current.Handler as RockPage;
            }

            var settings = JavascriptBlock.GetAttributesFromMarkup( _markup, new RockLiquidRenderContext( context ) );
            var parms = settings.Attributes;

            using ( TextWriter twJavascript = new StringWriter() )
            {
                base.Render( context, twJavascript );

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
                    var url = ResolveRockUrl( parms["url"] );

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
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string ResolveRockUrl( string url )
        {
            // If we are not operating in the context of a page, return the unresolved URL.
            if ( HttpContext.Current == null )
            {
                return url;
            }

            RockPage page = HttpContext.Current.Handler as RockPage;

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
    }
}