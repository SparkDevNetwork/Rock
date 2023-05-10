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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using DotLiquid;
using Rock.Lava.DotLiquid;
using Rock.Utility;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Scripturize ",
        TagName = "scripturize",
        Description = "Scripturize reads through text finding scripture references and converts them into links to popular Bible websites.",
        Documentation = ScripturizeShortcode.DocumentationMetadata,
        Parameters = "defaulttranslation,landingsite,cssclass",
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class Scripturize : RockLavaShortcodeBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterShortcode<Scripturize>( "scripturize" );
        }

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
            using ( TextWriter writer = new StringWriter() )
            {
                base.Render( context, writer );

                var settings = LavaElementAttributes.NewFromMarkup( _markup, new RockLiquidRenderContext( context ) );

                var landingSite = settings.GetEnumOrNull<LandingSite>( "landingsite" );
                if ( landingSite == null )
                {
                    if ( settings.HasValue( "landingsite" ) )
                    {
                        // If the specified value cannot be mapped, return an error message.
                        result.Write( "<!-- the landing site provided to the scripturize shortcode was not correct -->" + writer.ToString() );
                        return;
                    }
                    else
                    {
                        // If not specified, set the default.
                        landingSite = LandingSite.YouVersion;
                    }
                }

                var output = Rock.Utility.Scripturize.Parse( Rock.Utility.Scripturize.Parse( writer.ToString(),
                    settings.GetString( "defaulttranslation", "NLT" ),
                    landingSite.Value,
                    settings.GetString( "cssclass" ),
                    settings.GetBoolean( "openintab" ) ) );

                result.Write( output );
            }
        }
    }
}