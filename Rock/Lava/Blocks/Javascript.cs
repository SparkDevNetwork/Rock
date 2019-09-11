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

using Rock.Utility;
using Rock.Web.UI;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Sql stores the result of provided SQL query into a variable.
    ///
    /// {% sql results %}
    /// SELECT [FirstName], [LastName] FROM [Person]
    /// {% endsql %}
    /// </summary>
    public class Javascript : DotLiquid.Block, IRockStartup
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
            RockPage page = HttpContext.Current.Handler as RockPage;

            var parms = ParseMarkup( _markup, context );

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

                    if ( parms.ContainsKey( "id" ) )
                    {
                        var identifier = parms["id"];
                        if ( identifier.IsNotNullOrWhiteSpace() )
                        {
                            var controlId = "js-" + identifier;

                            var scriptControl = page.Header.FindControl( controlId );
                            if ( scriptControl == null )
                            {
                                scriptControl = new System.Web.UI.LiteralControl( $"{Environment.NewLine}<script>{javascript}</script>{Environment.NewLine}" );
                                scriptControl.ID = controlId;
                                page.Header.Controls.Add( scriptControl );
                            }
                        }
                    }
                    else
                    {
                        page.Header.Controls.Add( new System.Web.UI.LiteralControl( $"{Environment.NewLine}<script>{javascript}</script>{Environment.NewLine}" ) );
                    }
                }
                else
                {
                    var url = ResolveRockUrl( parms["url"] );

                    if ( parms.ContainsKey( "id" ) )
                    {
                        var identifier = parms["id"];
                        if ( identifier.IsNotNullOrWhiteSpace() )
                        {
                            var controlId = "js-" + identifier;

                            var scriptControl = page.Header.FindControl( controlId );
                            if ( scriptControl == null )
                            {
                                scriptControl = new System.Web.UI.LiteralControl( $"{Environment.NewLine}<script src='{url}' type='text/javascript'></script>{Environment.NewLine}" );
                                scriptControl.ID = controlId;
                                page.Header.Controls.Add( scriptControl );
                            }
                        }
                    }
                    else
                    {
                        page.Header.Controls.Add( new System.Web.UI.LiteralControl( $"{Environment.NewLine}<script src='{url}' type='text/javascript'></script>{Environment.NewLine}" ) );
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string ResolveRockUrl(string url )
        {
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

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }
            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            var parms = new Dictionary<string, string>();
            parms.Add( "cacheduration", "0" );
            parms.Add( "references", string.Empty );
            parms.Add( "disableanonymousfunction", "false" );
            parms.Add( "url", string.Empty );

            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }
            return parms;
        }
    }
}