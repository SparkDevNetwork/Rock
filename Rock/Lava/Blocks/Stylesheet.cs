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

using dotless.Core;
using dotless.Core.configuration;

using DotLiquid;

using Rock.Utility;
using Rock.Web.Cache;
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
    public class Stylesheet : DotLiquid.Block, IRockStartup
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            Template.RegisterTag<Stylesheet>( "stylesheet" );
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

            using ( TextWriter twStylesheet = new StringWriter() )
            {
                base.Render( context, twStylesheet );

                var stylesheet = twStylesheet.ToString();

                if ( parms.ContainsKey( "compile" ) )
                {
                    if ( parms["compile"] == "less" )
                    {
                        DotlessConfiguration dotLessConfiguration = new DotlessConfiguration();
                        dotLessConfiguration.MinifyOutput = true;

                        if ( parms.ContainsKey( "import" ) )
                        {
                            // import statements should go at the end to allow for default variable assignment in the beginning
                            // to help reduce the number of Less errors we automatically add the bootstrap and core rock variables files
                            var importStatements = string.Empty;

                            var importSource = "~/Styles/Bootstrap/variables.less,~/Styles/_rock-variables.less," + parms["import"];

                            var importFiles = importSource.Split( ',' );
                            foreach( var importFile in importFiles )
                            {
                                var filePath = string.Empty;
                                if ( !importFile.StartsWith( "~" ) )
                                {
                                    filePath = $"~~/Styles/{importFile}";
                                }
                                else
                                {
                                    filePath = importFile;
                                }

                                filePath = page.ResolveRockUrl( filePath );

                                var fullPath = page.MapPath( "~/" ) + filePath;

                                if (File.Exists( fullPath ) )
                                {
                                    importStatements = $"{importStatements}{Environment.NewLine}@import \"{fullPath}\";";
                                }
                            }

                            stylesheet = $"{stylesheet}{Environment.NewLine}{importStatements}";
                        }

                        // ok we have our less stylesheet let's see if it's been cached (less can take ~100ms to compile so let's try not to do that if necessary)
                        if ( parms.ContainsKey( "cacheduration" ) )
                        {
                            var cacheKey = stylesheet.GetHashCode().ToString();
                            var cachedStylesheet = RockCache.Get( cacheKey ) as string;

                            if ( cachedStylesheet.IsNotNullOrWhiteSpace() )
                            {
                                stylesheet = cachedStylesheet;
                            }
                            else
                            {
                                stylesheet = LessWeb.Parse( stylesheet, dotLessConfiguration );

                                // check if we should cache this
                                if ( parms.ContainsKey( "cacheduration" ) && stylesheet.IsNotNullOrWhiteSpace() )
                                {
                                    int cacheDuration = 0;
                                    Int32.TryParse( parms["cacheduration"], out cacheDuration );

                                    if ( cacheDuration > 0 )
                                    {
                                        RockCache.AddOrUpdate( cacheKey, null, stylesheet, RockDateTime.Now.AddSeconds( cacheDuration ) );
                                    }
                                }

                            }
                        }
                        else
                        {
                            stylesheet = LessWeb.Parse( stylesheet, dotLessConfiguration );
                        }
                    }

                    if (stylesheet == string.Empty )
                    {
                        if ( parms.ContainsKey( "id" ) )
                        {
                            result.Write( $"An error occurred compiling the Less for this stylesheet (id: {parms["id"]})." );
                        }
                        else
                        {
                            result.Write( "An error occurred compiling the Less for this stylesheet." );
                        }
                        return;
                    }
                }

                if ( parms.ContainsKey("id") )
                {
                    var identifier = parms["id"];
                    if ( identifier.IsNotNullOrWhiteSpace() )
                    {
                        var controlId = "css-" + identifier;

                        var cssControl = page.Header.FindControl( controlId );
                        if (cssControl == null )
                        {
                            cssControl = new System.Web.UI.LiteralControl( $"{Environment.NewLine}<style>{stylesheet}</style>{Environment.NewLine}" );
                            cssControl.ID = controlId;
                            page.Header.Controls.Add( cssControl );
                        }
                    }
                }
                else
                {
                    page.Header.Controls.Add( new System.Web.UI.LiteralControl( $"{Environment.NewLine}<style>{stylesheet}</style>{Environment.NewLine}" ) );
                }
            }
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