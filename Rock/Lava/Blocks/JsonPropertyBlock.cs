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

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class JsonPropertyBlock : LavaBlockBase
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
            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;

            // If no name given then skip this
            if ( parms["name"].IsNullOrWhiteSpace() )
            {
                return;
            }

            // Make the format parameter case insensitive, because we're nice... or lazy... or both.
            parms["format"] = parms["format"].ToLower();

            var parameterMarkup = string.Empty;

            using ( TextWriter twBody = new StringWriter() )
            {
                base.OnRender( context, twBody );

                var body = twBody.ToString();

                switch ( parms["format"] )
                {
                    case "number":
                    case "boolean":
                        {
                            parameterMarkup = string.Format( "\"{0}\": {1}", parms["name"], body.Trim() );
                            break;
                        }
                    default:
                        {
                            parameterMarkup = string.Format( "\"{0}\": {1}", parms["name"], body.Trim().ToJson() );
                            break;
                        }
                }

                result.Write( parameterMarkup );
            }
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "name", string.Empty );
            settings.AddOrIgnore( "format", "string" );

            return settings;
        }
    }
}