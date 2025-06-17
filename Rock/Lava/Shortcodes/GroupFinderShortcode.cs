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
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using csscript;

using Nest;

using Rock.Address;
using Rock.Address.Classes;
using Rock.Data;
using Rock.Lava.Filters.Internal;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Group Finder",
        TagName = "groupfinder",
        Description = "Provides the group finder logic in a simple to use shortcode..",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class GroupFinderShortcode : LavaShortcodeBase, ILavaBlock
    {
        #region Constants

        /// <summary>
        /// The parameter names that are used in the shortcode.
        /// </summary>
        internal static class ParameterKeys
        {
            /// <summary>
            /// This value will be added to all edge sizes as a way to provide
            /// a common boost to the size of all edges.
            /// </summary>
            public const string GroupTypeIds = "grouptypeid";

            /// <summary>
            /// This value will be added to all node sizes as a way to provide
            /// a common boost to the size of all nodes.
            /// </summary>
            public const string MaxResults = "maxresults";

            /// <summary>
            /// If true, only the closest location for each group will be returned. Otherwise, all locations for a group will be considered.
            /// </summary>
            public const string ReturnOnlyClosestLocationPerGroup = "returnonlyclosestlocationpergroup";

            /// <summary>
            /// The maximum distance to search for groups in meters.
            /// </summary>
            public const string MaxDistance = "maxdistance";

            /// <summary>
            /// The lat/long of the location to use for the search.
            /// </summary>
            public const string Origin = "origin";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.GroupTypeIds
            + "," + ParameterKeys.MaxResults
            + "," + ParameterKeys.ReturnOnlyClosestLocationPerGroup
            + "," + ParameterKeys.MaxDistance
            + "," + ParameterKeys.Origin;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"";

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the type of Lava element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType => LavaShortcodeTypeSpecifier.Block;

        #endregion

        #region Fields

        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        /// <summary>
        /// The markup that was inside the shortcode block.
        /// </summary>
        private string _internalMarkup = string.Empty;

        #endregion

        #region Methods

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

            // Get Rock Context
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            var mergedMarkup = engine.RenderTemplate( _internalMarkup, LavaRenderParameters.WithContext( context ) );
            var childElementsAreValid = ExtractBlockChildElements( context, mergedMarkup.Text, out var childElements, out var residualBlockContent );

            if ( !childElementsAreValid )
            {
                return;
            }

            // Get the origin point
            

            // Get the options for the shortcode
            var options = new Options
            {
                GroupTypeIds = settings[ParameterKeys.GroupTypeIds] ?? "",
                MaxResults = settings[ParameterKeys.MaxResults].AsIntegerOrNull() ?? 10,
                ReturnOnlyClosestLocationPerGroup = settings[ParameterKeys.ReturnOnlyClosestLocationPerGroup].AsBooleanOrNull() ?? true,
                MaxDistance = settings[ParameterKeys.MaxDistance].AsIntegerOrNull(),
                Origin = settings[ParameterKeys.Origin].ToString(),
                OriginPoint = GetOriginPoint( settings[ParameterKeys.Origin].ToString() )
            };

            //var (nodes, edges) = GetNodesAndEdges( childElements );

            // Create the initial query context.
            var results = new GroupService( rockContext )
                .GetNearestGroups( options.OriginPoint, options.GroupTypeIdList , options.MaxResults, options.ReturnOnlyClosestLocationPerGroup, options.MaxDistance )
                .Select( g => new GroupProximityResult
                {
                    StraightLineDistance = g.Location.GeoPoint.Distance( options.OriginPoint.ToDbGeography() ),
                    Group = g.Group,
                    Location = g.Location
                } ).ToList();

            

            result.Write( "output here" );
        }

        /// <summary>
        /// Gets the origin point from the settings. If the origin is not a lat/long, it will be geocoded.
        /// </summary>
        /// <param name="originString"></param>
        /// <returns></returns>
        private static GeographyPoint GetOriginPoint( string originString )
        {
            // Check if it's a lat/long
            if ( GeographyPoint.TryParse( "40.7128,-74.0060", out var point ) )
            {
                return point;
            }

            // Run it through the geocoder
            return Task.Run( () => ( LocationHelpers.Geocode( originString ) ).Result;
        }

        /// <summary>
        /// Gets the attributes and values from the markup. This ensures that
        /// all parameters exist in the settings.
        /// </summary>
        /// <param name="markup">The markup to be parsed.</param>
        /// <param name="context">The lava render context.</param>
        /// <returns>LavaElementAttributes.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Parse attributes string.
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            // Add default settings.
            settings.AddOrIgnore( settings[ParameterKeys.MaxResults], "" );
            
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

        /// <summary>
        /// Gets the nodes and edges from the child elements.
        /// </summary>
        /// <param name="elements">The elements to be parsed.</param>
        /// <returns>A tuple that contains a list of nodes and a list of edges.</returns>
        private (List<object>, List<object>) GetNodesAndEdges( List<ChildBlockElement> elements )
        {
            //var nodes = new List<GraphNode>();
            //var edges = new List<GraphEdge>();

            //foreach ( var element in elements )
            //{
            //    if ( element.Name == "node" )
            //    {
            //        if ( !element.Parameters.ContainsKey( "label" ) )
            //        {
            //            continue;
            //        }

            //        var node = new GraphNode
            //        {
            //            Id = element.Parameters.GetValueOrNull( "id" ) ?? element.Parameters["label"],
            //            Label = element.Parameters["label"],
            //            Color = element.Parameters.GetValueOrNull( "color" ),
            //            Size = element.Parameters.GetValueOrNull( "size" ).AsIntegerOrNull()
            //        };

            //        nodes.Add( node );
            //    }
            //    else if ( element.Name == "edge" )
            //    {
            //        if ( !element.Parameters.ContainsKey( "source" ) || !element.Parameters.ContainsKey( "target" ) )
            //        {
            //            continue;
            //        }

            //        var edge = new GraphEdge
            //        {
            //            Id = element.Parameters.GetValueOrNull( "id" ) ?? $"e_{edges.Count + 1}",
            //            Source = element.Parameters["source"],
            //            Target = element.Parameters["target"],
            //            Label = element.Parameters.GetValueOrNull( "label" ),
            //            Color = element.Parameters.GetValueOrNull( "color" ),
            //            Size = element.Parameters.GetValueOrNull( "size" ).AsIntegerOrNull()
            //        };

            //        edges.Add( edge );
            //    }
            //}

            //return (nodes, edges);

            return (null, null);
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The options that can be passed into the network graph.
        /// </summary>
        private class Options
        {
            /// <summary>
            /// The group type id to use for the search.
            /// </summary>
            public string GroupTypeIds { get; set; }

            /// <summary>
            /// The group type ids in a list to use for the search.
            /// </summary>
            public List<int> GroupTypeIdList
            {
                get
                {
                    if ( GroupTypeIds.IsNullOrWhiteSpace() )
                    {
                        return new List<int>();
                    }
                    return GroupTypeIds.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsInteger() ).ToList();
                }
            }

            /// <summary>
            /// The maximum number of results to return.
            /// </summary>
            public int MaxResults { get; set; }

            /// <summary>
            /// The maximum distance to search for groups in meters.
            /// </summary>
            public int? MaxDistance { get; set; }

            /// <summary>
            /// If true, only the closest location for each group will be returned. Otherwise, all locations for a group will be considered.
            /// </summary>
            public bool ReturnOnlyClosestLocationPerGroup { get; set; }

            /// <summary>
            /// The origin to use for the search.
            /// </summary>
            public string Origin { get; set; }

            /// <summary>
            /// The origin point to use for the search.
            /// </summary>
            public GeographyPoint OriginPoint { get; set; }
        }

        private class ChildBlockElement
        {
            public string Name { get; set; }

            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            public string Content { get; set; }
        }

        #endregion
    }
}
