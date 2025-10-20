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
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Configuration;
using Rock.Net;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class SiteMasterBlock : LavaBlockBase
    {
        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        /// <summary>
        /// The markup that was inside the shortcode block.
        /// </summary>
        private string _internalMarkup = string.Empty;


        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _blockPropertiesMarkup = markup;

            // Get the internal Lava for the block. The last token will be the block's end tag.
            _internalMarkup = string.Join( string.Empty, tokens.Take( tokens.Count - 1 ) );

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var engine = context.GetService<ILavaEngine>();
            var requestContext = context.GetInternalField( "RockRequestContext" ) as RockRequestContext;
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            var mergedMarkup = engine.RenderTemplate( _internalMarkup, LavaRenderParameters.WithContext( context ) );
            var childElementsAreValid = ExtractBlockChildElements( context, mergedMarkup.Text, out var childElements, out var residualBlockContent );

            // If no src filename is given then skip this.
            if ( settings["src"].IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( !childElementsAreValid )
            {
                return;
            }

            var templatePath = RockApp.Current.MapPath( settings["src"], requestContext.Page?.Layout?.Site?.Theme ?? "Rock" );
            var template = File.ReadAllText( templatePath );

            var content = new Dictionary<string, string>();

            foreach ( var childElement in childElements )
            {
                if ( childElement.Parameters.TryGetValue( "id", out var contentId ) )
                {
                    content.Add( contentId, childElement.Content );
                }
            }

            context.ExecuteInChildScope( childContext =>
            {
                childContext.SetMergeField( "Content", content );

                var renderParameters = LavaRenderParameters.WithContext( childContext );
                var renderedTemplate = engine.RenderTemplate( template, renderParameters );

                result.Write( renderedTemplate.Text );
            } );
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "src", string.Empty );

            return settings;
        }

        /// <summary>
        /// Extracts a set of child elements from the content of the block.
        /// Child elements are grouped by tag name, and each item in the collection has a set of properties
        /// corresponding to the child element tag attributes and a "content" property representing the inner content of the child element.
        /// </summary>
        /// <param name="context">The current lava render context.</param>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childElements">The child parameters.</param>
        /// <param name="residualBlockContent">The block content that is left over after parsing.</param>
        /// <returns><c>true</c> if the child elements were valid, otherwise <c>false</c>.</returns>
        private bool ExtractBlockChildElements( ILavaRenderContext context, string blockContent, out List<ChildBlockElement> childElements, out string residualBlockContent )
        {
            childElements = new List<ChildBlockElement>();

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
                        var endTagMatchExpression = $@"\[\[\s*end{parmName}\s*\]\]";
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
                                var engine = context.GetService<ILavaEngine>();
                                var renderParameters = new LavaRenderParameters { Context = context };
                                parmContent = engine.RenderTemplate( parmContent, renderParameters ).Text;
                            }

                            var childElement = new ChildBlockElement
                            {
                                Name = parmName,
                                Content = parmContent
                            };

                            // Regex pattern explanation:
                            //
                            //  \S*? Matches any non-whitespace characters (non-greedy) before the colon.
                            //  : Matches the colon character.
                            //  (['"]) Capturing group that matches either a single ' or double " quote. This group is captured as \2 for backreference.
                            //  (.*?): Non-greedy match of any character, capturing as few characters as needed.
                            //  \2: Backreference to the matched quote in (['"]), ensuring the string is closed with the same type of quote.
                            //
                            // This allows for network graph labels that include single quotes, and will match either:
                            //  label:'A/V Team'
                            //  label:"Pete's Group'
                            var parmItems = Regex.Matches( tagParms, @"(\S*?:(['""])(.*?)\2)" )
                                .Cast<Match>()
                                .Select( m => m.Value )
                                .ToList();

                            foreach ( var item in parmItems )
                            {
                                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                                if ( itemParts.Length > 1 )
                                {
                                    childElement.Parameters.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                                }
                            }

                            childElements.Add( childElement );

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

        private class ChildBlockElement
        {
            public string Name { get; set; }

            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            public string Content { get; set; }
        }
    }
}