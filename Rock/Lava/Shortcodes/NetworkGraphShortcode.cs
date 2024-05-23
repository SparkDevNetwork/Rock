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

using Rock.Web.UI;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Network Graph",
        TagName = "networkgraph",
        Description = "Displays a set of data as a diagram showing how different nodes are interconnected with each other.",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
    public class NetworkGraphShortcode : LavaShortcodeBase, ILavaBlock
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
            public const string BaseEdgeSize = "baseedgesize";

            /// <summary>
            /// This value will be added to all node sizes as a way to provide
            /// a common boost to the size of all nodes.
            /// </summary>
            public const string BaseNodeSize = "basenodesize";

            /// <summary>
            /// The default color for any edge that does not specify a color.
            /// </summary>
            public const string EdgeColor = "edgecolor";

            /// <summary>
            /// The height of the graph container. By default this will be
            /// 400px.
            /// </summary>
            public const string Height = "height";

            /// <summary>
            /// The color to use when highlighting nodes and edges.
            /// </summary>
            public const string HighlightColor = "highlightcolor";

            /// <summary>
            /// The scale factor to apply to highlighted edges and nodes. A value
            /// of 0.5 means half size, 2.0 means double size, 1.0 means leave
            /// at original size.
            /// </summary>
            public const string HighlightScaleFactor = "highlightscalefactor";

            /// <summary>
            /// The text color to use for labels.
            /// </summary>
            public const string LabelColor = "labelcolor";

            /// <summary>
            /// The maximum size a edge is allowed to be. Any edge larger than
            /// this will be set to this size.
            /// </summary>
            public const string MaximumEdgeSize = "maximumedgesize";

            /// <summary>
            /// The maximum size a node is allowed to be. Any node larger than
            /// this will be set to this size.
            /// </summary>
            public const string MaximumNodeSize = "maximumnodesize";

            /// <summary>
            /// The minimum size a edge is allowed to be. Any edge smaller than
            /// this will be set to this size.
            /// </summary>
            public const string MinimumEdgeSize = "minimumedgesize";

            /// <summary>
            /// The minimum size a node is allowed to be. Any node smaller than
            /// this will be set to this size.
            /// </summary>
            public const string MinimumNodeSize = "minimumnodesize";

            /// <summary>
            /// The default color for any node that does not specify a color.
            /// </summary>
            public const string NodeColor = "nodecolor";

            /// <summary>
            /// The width of the graph container.
            /// </summary>
            public const string Width = "width";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.BaseEdgeSize
            + "," + ParameterKeys.BaseNodeSize
            + "," + ParameterKeys.EdgeColor
            + "," + ParameterKeys.Height
            + "," + ParameterKeys.HighlightColor
            + "," + ParameterKeys.HighlightScaleFactor
            + "," + ParameterKeys.LabelColor
            + "," + ParameterKeys.MaximumEdgeSize
            + "," + ParameterKeys.MaximumNodeSize
            + "," + ParameterKeys.MinimumEdgeSize
            + "," + ParameterKeys.MinimumNodeSize
            + "," + ParameterKeys.NodeColor
            + "," + ParameterKeys.Width;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"
<p>Builds a graph that shows a set of items and how they are interconnected to each other. The graph consists of a set of nodes (the items) and edges (the lines connecting nodes).</p>

<pre>{[ networkgraph ]}
    [[ node id:'1' label:'Ted' ]][[ endnode ]]
    [[ node id:'2' label:'Cindy' ]][[ endnode ]]
    [[ node id:'3' label:'Noah' ]][[ endnode ]]
    [[ node id:'4' label:'Alex' ]][[ endnode ]]

    [[ edge source:'1' target:'2' ]][[ endedge ]]
    [[ edge source:'1' target:'3' ]][[ endedge ]]
    [[ edge source:'2' target:'4' ]][[ endedge ]]
{[ endnetworkgraph ]}</pre>

<p>The shortcode has a number of parameters that can customize the behavior.</p>

<ul>
    <li><strong>baseedgesize</strong> (0) - This value will be added to all edge sizes as a way to provide a common boost to the size of all edges.</li>
    <li><strong>basenodesize</strong> (0) - This value will be added to all node sizes as a way to provide a common boost to the size of all nodes.</li>
    <li><strong>edgecolor</strong> (default from theme) - The default color for any edge that does not specify a color.</li>
    <li><strong>height</strong> (400px) - The height of the graph container.</li>
    <li><strong>highlightcolor</strong> (default from theme) - The color to use when highlighting nodes and edges.</li>
    <li><strong>highlightscalefactor</strong> (1.1) - The scale factor to apply to highlighted edges and nodes. A value of 0.5 means half size, 2.0 means double size, 1.0 means leave at original size.</li>
    <li><strong>labelcolor</strong> - The text color to use for labels.
    <li><strong>maximumedgesize</strong> (10) - The maximum size a edge is allowed to be. Any edge larger than this will be set to this size.</li>
    <li><strong>maximumnodesize</strong> (25) - The maximum size a node is allowed to be. Any node larger than this will be set to this size.</li>
    <li><strong>minimumedgesize</strong> (1) - The minimum size a edge is allowed to be. Any edge smaller than this will be set to this size.</li>
    <li><strong>minimumnodesize</strong> (1) - The minimum size a node is allowed to be. Any node smaller than this will be set to this size.</li>
    <li><strong>nodecolor</strong> (default from theme) - The default color for any node that does not specify a color.</li>
    <li><strong>width</strong> (100%) - The width of the graph container.</li>
</ul>

<p>Nodes have the following options.</p>

<ul>
    <li><strong>color</strong> - The optional color for the node.</li>
    <li><strong>id</strong> - The required identifier for the node.</li>
    <li><strong>label</strong> - The required text label to display with the node.</li>
    <li><strong>size</strong> - The optional size of the node. If not specified then it will be calculated automatically based on the number of edges connected to the node.</li>
</ul>

<p>Edges have the following options.</p>

<ul>
    <li><strong>color</strong> - The optional color for the edge.</li>
    <li><strong>id</strong> - The optional identifier for the edge. If not specified one will be generated.</li>
    <li><strong>label</strong> - The required text label to display with the edge.</li>
    <li><strong>size</strong> (1) - The size of the edge.</li>
    <li><strong>source</strong> - The required source node identifier.</li>
    <li><strong>target</strong> - The required target node identifier.</li>
</ul>
";

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
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            var mergedMarkup = engine.RenderTemplate( _internalMarkup, LavaRenderParameters.WithContext( context ) );
            var childElementsAreValid = ExtractBlockChildElements( context, mergedMarkup.Text, out var childElements, out var residualBlockContent );

            if ( !childElementsAreValid )
            {
                return;
            }

            var (nodes, edges) = GetNodesAndEdges( childElements );
            var elementId = $"networkgraph_{Guid.NewGuid()}";

            var options = new Options
            {
                BaseEdgeSize = settings[ParameterKeys.BaseEdgeSize].AsIntegerOrNull() ?? 0,
                BaseNodeSize = settings[ParameterKeys.BaseNodeSize].AsIntegerOrNull() ?? 0,
                EdgeColor = settings[ParameterKeys.EdgeColor],
                Edges = edges,
                HighlightColor = settings[ParameterKeys.HighlightColor],
                HighlightScaleFactor = settings[ParameterKeys.HighlightScaleFactor].AsDoubleOrNull() ?? 1.1,
                LabelColor = settings[ParameterKeys.LabelColor],
                MaximumEdgeSize = settings[ParameterKeys.MaximumEdgeSize].AsIntegerOrNull() ?? 10,
                MaximumNodeSize = settings[ParameterKeys.MaximumNodeSize].AsIntegerOrNull() ?? 25,
                MinimumEdgeSize = settings[ParameterKeys.MinimumEdgeSize].AsIntegerOrNull() ?? 0,
                MinimumNodeSize = settings[ParameterKeys.MinimumNodeSize].AsIntegerOrNull() ?? 0,
                NodeColor = settings[ParameterKeys.NodeColor],
                Nodes = nodes
            };

            // Construct the CSS style for this media player.
            var style = $"width: {settings.Attributes[ParameterKeys.Width]}; height: {settings.Attributes[ParameterKeys.Height]};";

            // Construct the JavaScript to initialize the graph.
            var script = $@"<script>
(function() {{
    new Rock.UI.NetworkGraph(""#{elementId}"", {options.ToCamelCaseJson( false, false )});
}})();
</script>";

            result.WriteLine( $"<div id=\"{elementId}\" style=\"{style}\"></div>" );
            result.WriteLine( script );

            // If we have a RockPage related to the current request then
            // register all the JS links we need.
            if ( HttpContext.Current?.Handler is RockPage page )
            {
                // Don't fingerprint as we are loading external resources at specific versions.
                page.AddScriptLink( "https://cdn.jsdelivr.net/npm/sigma@2.4.0/build/sigma.min.js", false );
                page.AddScriptLink( "https://cdn.jsdelivr.net/npm/graphology@0.25.4/dist/graphology.umd.min.js", false );
                page.AddScriptLink( "https://cdn.jsdelivr.net/npm/graphology-library@0.8.0/dist/graphology-library.min.js", false );

                page.AddScriptLink( "~/Scripts/Rock/UI/NetworkGraph/networkgraph.js", true );
            }

            result.Write( residualBlockContent.Trim() );
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
            settings.AddOrIgnore( ParameterKeys.BaseEdgeSize, "0" );
            settings.AddOrIgnore( ParameterKeys.BaseNodeSize, "0" );
            settings.AddOrIgnore( ParameterKeys.EdgeColor, string.Empty );
            settings.AddOrIgnore( ParameterKeys.Height, "400px" );
            settings.AddOrIgnore( ParameterKeys.HighlightColor, "" );
            settings.AddOrIgnore( ParameterKeys.HighlightScaleFactor, "1.1" );
            settings.AddOrIgnore( ParameterKeys.LabelColor, string.Empty );
            settings.AddOrIgnore( ParameterKeys.MaximumEdgeSize, "10" );
            settings.AddOrIgnore( ParameterKeys.MaximumNodeSize, "25" );
            settings.AddOrIgnore( ParameterKeys.MinimumEdgeSize, "0" );
            settings.AddOrIgnore( ParameterKeys.MinimumNodeSize, "0" );
            settings.AddOrIgnore( ParameterKeys.NodeColor, string.Empty );
            settings.AddOrIgnore( ParameterKeys.Width, "100%" );

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

                            var parmItems = Regex.Matches( tagParms, @"(\S*?:'[^']+')" )
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
        private (List<GraphNode>, List<GraphEdge>) GetNodesAndEdges( List<ChildBlockElement> elements )
        {
            var nodes = new List<GraphNode>();
            var edges = new List<GraphEdge>();

            foreach ( var element in elements )
            {
                if ( element.Name == "node" )
                {
                    if ( !element.Parameters.ContainsKey( "label" ) )
                    {
                        continue;
                    }

                    var node = new GraphNode
                    {
                        Id = element.Parameters.GetValueOrNull( "id" ) ?? element.Parameters["label"],
                        Label = element.Parameters["label"],
                        Color = element.Parameters.GetValueOrNull( "color" )
                    };

                    nodes.Add( node );
                }
                else if ( element.Name == "edge" )
                {
                    if ( !element.Parameters.ContainsKey( "source" ) || !element.Parameters.ContainsKey( "target" ) )
                    {
                        continue;
                    }

                    var edge = new GraphEdge
                    {
                        Id = element.Parameters.GetValueOrNull( "id" ) ?? $"e_{edges.Count + 1}",
                        Source = element.Parameters["source"],
                        Target = element.Parameters["target"],
                        Label = element.Parameters.GetValueOrNull( "label" ),
                        Color = element.Parameters.GetValueOrNull( "color" ),
                        Size = element.Parameters.GetValueOrNull( "size" ).AsIntegerOrNull() ?? 1
                    };

                    edges.Add( edge );
                }
            }

            return (nodes, edges);
        }

        #endregion

        #region Support Classes

        private class ChildBlockElement
        {
            public string Name { get; set; }

            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            public string Content { get; set; }
        }

        /// <summary>
        /// The options that can be passed into the network graph.
        /// </summary>
        private class Options
        {
            /// <summary>
            /// This value will be added to all edge sizes as a way to provide
            /// a common boost to the size of all edges.
            /// </summary>
            public int BaseEdgeSize { get; set; }

            /// <summary>
            /// This value will be added to all node sizes as a way to provide
            /// a common boost to the size of all nodes.
            /// </summary>
            public int BaseNodeSize { get; set; }

            /// <summary>
            /// The default color for any edge that does not specify a color.
            /// </summary>
            public string EdgeColor { get; set; }

            /// <summary>
            /// The list of edges in the graph.
            /// </summary>
            public List<GraphEdge> Edges { get; set; }

            /// <summary>
            /// The color to use when highlighting nodes and edges.
            /// </summary>
            public string HighlightColor { get; set; }

            /// <summary>
            /// The scale factor to apply to highlighted edges and nodes. A value
            /// of 0.5 means half size, 2.0 means double size, 1.0 means leave
            /// at original size.
            /// </summary>
            public double HighlightScaleFactor { get; set; }

            /// <summary>
            /// The text color to use for labels.
            /// </summary>
            public string LabelColor { get; set; }

            /// <summary>
            /// The maximum size a edge is allowed to be. Any edge larger than
            /// this will be set to this size.
            /// </summary>
            public int MaximumEdgeSize { get; set; }

            /// <summary>
            /// The maximum size a node is allowed to be. Any node larger than
            /// this will be set to this size.
            /// </summary>
            public int MaximumNodeSize { get; set; }

            /// <summary>
            /// The minimum size a edge is allowed to be. Any edge smaller than
            /// this will be set to this size.
            /// </summary>
            public int MinimumEdgeSize { get; set; }

            /// <summary>
            /// The minimum size a edge is allowed to be. Any edge smaller than
            /// this will be set to this size.
            /// </summary>
            public int MinimumNodeSize { get; set; }

            /// <summary>
            /// The default color for any node that does not specify a color.
            /// </summary>
            public string NodeColor { get; set; }

            /// <summary>
            /// The list of nodes in the graph.
            /// </summary>
            public List<GraphNode> Nodes { get; set; }
        }

        /// <summary>
        /// Defines the structure of a single node on the graph.
        /// </summary>
        private class GraphNode
        {
            /// <summary>
            /// Gets or sets the optional color for this node.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of this node.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the label to display on this node.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the optional size for this node.
            /// </summary>
            public int? Size { get; set; }
        }

        /// <summary>
        /// Defines the structure of a single edge on the graph.
        /// </summary>
        private class GraphEdge
        {
            /// <summary>
            /// Gets or sets the optional color for this edge.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of this edge.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the label to display on this edge.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the optional size for this edge.
            /// </summary>
            public int? Size { get; set; }

            /// <summary>
            /// Gets or sets the source node identifier.
            /// </summary>
            public string Source { get; set; }

            /// <summary>
            /// Gets or sets the target node identifier.
            /// </summary>
            public string Target { get; set; }
        }

        #endregion
    }
}
