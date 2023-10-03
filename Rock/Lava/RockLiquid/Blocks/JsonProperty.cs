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
using DotLiquid;
using Rock.Lava.Blocks;
using Rock.Lava.DotLiquid;
using Rock.Utility;
using Block = DotLiquid.Block;

namespace Rock.Lava.RockLiquid.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class JsonProperty : Block, IRockStartup
    {
        string _markup = string.Empty;

        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            Template.RegisterTag<JsonProperty>( "jsonproperty" );
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
            var settings = JsonPropertyBlock.GetAttributesFromMarkup( _markup, new RockLiquidRenderContext( context ) );
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
                base.Render( context, twBody );

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
    }
}