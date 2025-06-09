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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Updates the current HTTP request's response headers and status code.
    /// </summary>
    public class HttpResponse : LavaBlockBase
    {
        #region Fields

        /// <summary>
        /// The markup for the block describing the properties being passed
        /// to the block logic.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        /// <summary>
        /// The inner markup between the start and end tags of the block.
        /// </summary>
        private string _blockContent = string.Empty;

        #endregion

        /// <inheritdoc/>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _blockPropertiesMarkup = markup;

            // Get the internal Lava for the block. The last token will be the block's end tag.
            _blockContent = string.Join( " ", tokens.Take( tokens.Count - 1 ) );

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // Get the settings attributes from the Lava command
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );

            // Get body parameters
            ExtractBlockChildElements( context, _blockContent, out var responseParameters, out _ );

            // Process the status code
            var statusCode = settings["status"].AsIntegerOrNull() ?? 200;

            if ( HttpContext.Current != null )
            {
                HttpContext.Current.Response.StatusCode = statusCode;
            }

            // Add headers
            if ( responseParameters.ContainsKey( "headers" ) )
            {
                var headers = responseParameters["headers"];

                foreach ( var headerInfo in headers as List<object> )
                {
                    var header = headerInfo as Dictionary<string, object>;

                    var headerKey = string.Empty;
                    var headerValue = string.Empty;

                    if ( header.ContainsKey( "key" ) )
                    {
                        headerKey = header["key"].ToString();
                    }

                    if ( header.ContainsKey( "content" ) )
                    {
                        headerValue = header["content"].ToString();
                    }

                    if ( headerKey.IsNotNullOrWhiteSpace() && headerValue.IsNotNullOrWhiteSpace() )
                    {
                        HttpContext.Current?.Response.Headers.Add( headerKey, headerValue );
                    }
                }
            }

            // Abort the current rendering operation if the status code is not 2xx.
            if ( statusCode < 200 || statusCode > 299 )
            {
                throw new LavaInterruptException();
            }
        }

        /// <summary>
        /// Gets a list of parameter attributes from the block command.
        /// </summary>
        /// <param name="markup">The markup that describes the command parameters.</param>
        /// <param name="context">The rendering context used to parse the parameters.</param>
        /// <returns>An instance of <see cref="LavaElementAttributes"/> that contains the parameters.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            // Default status to 200
            if ( settings["status"] == null )
            {
                settings.AddOrIgnore( "status", "200" );
            }


            return settings;
        }

        /// <summary>
        /// Extracts a set of child elements from the content of the block.
        /// Child elements are grouped by tag name, and each item in the collection has a set of properties
        /// corresponding to the child element tag attributes and a "content" property representing the inner content of the child element.
        /// </summary>
        /// <param name="context">The rendering context currently handling rendering this block.</param>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childParameters">The child parameters.</param>
        /// <param name="residualBlockContent">The inner content of the block after the child tags have been extracted.</param>
        /// <returns><c>true</c> if the inner content was valid; otherwise <c>false</c>.</returns>
        private bool ExtractBlockChildElements( ILavaRenderContext context, string blockContent, out Dictionary<string, object> childParameters, out string residualBlockContent )
        {
            childParameters = new Dictionary<string, object>();

            var startTagStartExpress = new Regex( @"\[\[\s*" );

            var isValid = true;
            var matchExists = true;
            while ( matchExists )
            {
                var match = startTagStartExpress.Match( blockContent );
                if ( match.Success )
                {
                    int startTagStartIndex = match.Index;

                    // get the name of the parameter
                    var parmNameMatch = new Regex( @"[\w-]*" ).Match( blockContent, startTagStartIndex + match.Length );
                    if ( parmNameMatch.Success )
                    {
                        var parmNameStartIndex = parmNameMatch.Index;
                        var parmNameEndIndex = parmNameStartIndex + parmNameMatch.Length;
                        var parmName = blockContent.Substring( parmNameStartIndex, parmNameMatch.Length );

                        // get end of the tag index
                        var startTagEndIndex = blockContent.IndexOf( "]]", parmNameStartIndex ) + 2;

                        // get the tags parameters
                        var tagParms = blockContent.Substring( parmNameEndIndex, startTagEndIndex - parmNameEndIndex ).Trim();

                        // get the closing tag location
                        var endTagMatchExpression = String.Format( @"\[\[\s*end{0}\s*\]\]", parmName );
                        var endTagMatch = new Regex( endTagMatchExpression ).Match( blockContent, startTagStartIndex );

                        if ( endTagMatch.Success )
                        {
                            var endTagStartIndex = endTagMatch.Index;
                            var endTagEndIndex = endTagStartIndex + endTagMatch.Length;

                            // get the parm content (the string between the two parm tags)
                            var parmContent = blockContent.Substring( startTagEndIndex, endTagStartIndex - startTagEndIndex ).Trim();

                            // Run Lava across the content
                            if ( parmContent.IsNotNullOrWhiteSpace() )
                            {
                                var engine = context?.GetService<ILavaEngine>();
                                var renderParameters = new LavaRenderParameters { Context = context };
                                parmContent = engine.RenderTemplate( parmContent, renderParameters ).Text;
                            }

                            // create dynamic object from parms
                            var dynamicParm = new Dictionary<string, object>();
                            dynamicParm.Add( "content", parmContent );

                            var parmItems = Regex.Matches( tagParms, @"(\S*?:'[^']+')" )
                                .Cast<Match>()
                                .Select( m => m.Value )
                                .ToList();

                            foreach ( var item in parmItems )
                            {
                                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                                if ( itemParts.Length > 1 )
                                {
                                    dynamicParm.Add( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                                }
                            }

                            // add new parm to a collection of parms
                            var propertyCollectionName = parmName.Pluralize();

                            if ( childParameters.ContainsKey( propertyCollectionName ) )
                            {
                                var parmList = ( List<object> ) childParameters[propertyCollectionName];
                                parmList.Add( dynamicParm );
                            }
                            else
                            {
                                var parmList = new List<object>
                                {
                                    dynamicParm
                                };
                                childParameters.Add( propertyCollectionName, parmList );
                            }

                            // pull this tag out of the block content
                            blockContent = blockContent.Remove( startTagStartIndex, endTagEndIndex - startTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            isValid = false;
                            matchExists = false;
                            blockContent = blockContent + "Warning: Missing field end tag." + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        isValid = false;
                        matchExists = false;
                        blockContent += "Warning: Field definition does not have any parameters.";
                    }

                }
                else
                {
                    matchExists = false; // we're done here
                }
            }

            residualBlockContent = blockContent.Trim();

            return isValid;
        }
    }
}
