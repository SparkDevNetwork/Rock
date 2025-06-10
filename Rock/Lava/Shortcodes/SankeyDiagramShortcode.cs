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
using Rock.ViewModels.Controls;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    ///
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Sankey Diagram",
        TagName = "sankeydiagram",
        Description = "Displays a set of data as a sankey diagram, showing the flow from nodes to other nodes.",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = "A5503FF2-01A2-49CB-8C22-E57C3D7FDC29" )]
    public class SankeyDiagramShortcode : LavaShortcodeBase, ILavaBlock
    {
        #region Constants

        /// <summary>
        /// The parameter names that are used in the shortcode.
        /// </summary>
        internal static class ParameterKeys
        {
            /// <summary>
            /// The width of the rectangular node
            /// </summary>
            public const string NodeWidth = "nodewidth";

            /// <summary>
            /// The vertical gap space between nodes
            /// </summary>
            public const string NodeVerticalSpacing = "nodeverticalspacing";

            /// <summary>
            /// The width of the chart
            /// </summary>
            public const string ChartWidth = "chartwidth";

            /// <summary>
            /// The height of the chart
            /// </summary>
            public const string ChartHeight = "chartheight";

            /// <summary>
            /// When hovering a node, a tooltip shows. If this is specified, this text will be prepended before the unit count.
            /// </summary>
            public const string NodeTooltipActionLabel = "nodetooltipactionlabel";

            /// <summary>
            /// Whether or not the legend should be shown
            /// </summary>
            public const string ShowLegend = "showlegend";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.NodeWidth
            + "," + ParameterKeys.NodeVerticalSpacing
            + "," + ParameterKeys.ChartWidth
            + "," + ParameterKeys.ChartHeight
            + "," + ParameterKeys.ShowLegend;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"
<p>Builds a diagram that emphasizes flow/movement/change (edge) from one state (node) to another or one time to another, in which the width of the flow is proportional to the number flowing in/out from the total.</p>

<pre>{[ sankeydiagram nodetooltipactionlabel:'Total Steps Taken' ]}
    [[ node id:'1' name:'Baptism' color:'cyan' ]][[ endnode ]]
    [[ node id:'2' name:'Confirmation' color:'rebeccapurple' ]][[ endnode ]]
    [[ node id:'3' name:'Eucharist' ]][[ endnode ]]
    [[ node id:'4' name:'Confession' ]][[ endnode ]]

    [[ edge level:'1' sourceid:'0' targetid:'1' units:'48' ]][[ endedge ]]
    [[ edge level:'1' sourceid:'0' targetid:'2' units:'21' ]][[ endedge ]]
    [[ edge level:'1' sourceid:'0' targetid:'3' units:'11' ]][[ endedge ]]
    [[ edge level:'1' sourceid:'0' targetid:'4' units:'10' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'1' targetid:'2' units:'32', tooltip:'&lt;strong&gt;Baptism &gt; Confirmation&lt;/strong&gt;&lt;br&gt;Steps Taken: 32' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'2' targetid:'1' units:'11', tooltip:'&lt;strong&gt;Confirmation &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 11' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'2' targetid:'3' units:'3', tooltip:'&lt;strong&gt;Confirmation &gt; Eucharist&lt;/strong&gt;&lt;br&gt;Steps Taken: 3' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'2' targetid:'4' units:'5', tooltip:'&lt;strong&gt;Confirmation &gt; Confession&lt;/strong&gt;&lt;br&gt;Steps Taken: 5' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'3' targetid:'1' units:'5', tooltip:'&lt;strong&gt;Eucharist &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 5' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'3' targetid:'4' units:'3', tooltip:'&lt;strong&gt;Eucharist &gt; Confession&lt;/strong&gt;&lt;br&gt;Steps Taken: 3' ]][[ endedge ]]
    [[ edge level:'2' sourceid:'4' targetid:'1' units:'7', tooltip:'&lt;strong&gt;Confession &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 7' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'1' targetid:'2' units:'10', tooltip:'&lt;strong&gt;Baptism &gt; Confirmation&lt;/strong&gt;&lt;br&gt;Steps Taken: 10' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'1' targetid:'3' units:'9', tooltip:'&lt;strong&gt;Baptism &gt; Eucharist&lt;/strong&gt;&lt;br&gt;Steps Taken: 9' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'2' targetid:'1' units:'10', tooltip:'&lt;strong&gt;Confirmation &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 10' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'2' targetid:'3' units:'20', tooltip:'&lt;strong&gt;Confirmation &gt; Eucharist&lt;/strong&gt;&lt;br&gt;Steps Taken: 20' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'3' targetid:'1' units:'3', tooltip:'&lt;strong&gt;Eucharist &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 3' ]][[ endedge ]]
    [[ edge level:'3' sourceid:'4' targetid:'1' units:'8', tooltip:'&lt;strong&gt;Confession &gt; Baptism&lt;/strong&gt;&lt;br&gt;Steps Taken: 8' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'1' targetid:'2' units:'11', tooltip:'&lt;strong&gt;Baptism &gt; Confirmation&lt;/strong&gt;&lt;br&gt;Steps Taken: 11' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'1' targetid:'3' units:'7', tooltip:'&lt;strong&gt;Baptism &gt; Eucharist&lt;/strong&gt;&lt;br&gt;Steps Taken: 7' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'1' targetid:'4' units:'2', tooltip:'&lt;strong&gt;Baptism &gt; Confession&lt;/strong&gt;&lt;br&gt;Steps Taken: 2' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'2' targetid:'3' units:'5', tooltip:'&lt;strong&gt;Confirmation &gt; Eucharist&lt;/strong&gt;&lt;br&gt;Steps Taken: 5' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'2' targetid:'4' units:'3', tooltip:'&lt;strong&gt;Confirmation &gt; Confession&lt;/strong&gt;&lt;br&gt;Steps Taken: 3' ]][[ endedge ]]
    [[ edge level:'4' sourceid:'3' targetid:'4' units:'10', tooltip:'&lt;strong&gt;Eucharist &gt; Confession&lt;/strong&gt;&lt;br&gt;Steps Taken: 10' ]][[ endedge ]]
{[ endsankeydiagram ]}</pre>

<p>The shortcode has a number of parameters that can customize the behavior.</p>

<ul>
    <li><strong>nodewidth</strong> (12) - The width of the rectangular node.</li>
    <li><strong>nodeverticalspacing</strong> (12) - The vertical gap space between nodes.</li>
    <li><strong>chartwidth</strong> (800) - The width of the chart.</li>
    <li><strong>chartheight</strong> (400) - The height of the chart.</li>
    <li><strong>showlegend</strong> (true) - Whether or not the legend should be shown.</li>
    <li><strong>nodetooltipactionlabel</strong> (none) - When hovering a node, a tooltip shows. If this is specified, this text will be prepended before the unit count.</li>
</ul>

<p>Nodes have the following options.</p>

<ul>
    <li><strong>id</strong> - The required identifier for the node.</li>
    <li><strong>name</strong> - The required text label for the node.</li>
    <li><strong>color</strong> (generated) - The optional color that represents this node.</li>
</ul>

<p>Edges have the following options.</p>

<ul>
    <li><strong>level</strong> - The required level for the edge. Level 1 represents the initial states and therefore should have empty source IDs. Level 2 is the flow from the initial state to the next state and so on.</li>
    <li><strong>sourceid</strong> - The required identifier of where the units are coming from. The identifier needs to match the ID of one of the nodes. Should be '0' for Level 1</li>
    <li><strong>targetid</strong> - The required identifier of where the units are going to. The identifier needs to match the ID of one of the nodes.</li>
    <li><strong>units</strong> - The required number of units or items flowing from the source to the target.</li>
    <li><strong>tooltip</strong> (generated) - The optional tooltip you'll see when hovering over this flow between the nodes. If not provided, the number of units will be shown.</li>
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

        /// <summary>
        /// Used to track which color should be used next.
        /// </summary>
        private int currentColorIndex = 0;

        /// <summary>
        /// Node colors if some nodes don't have a color specified. These are pulled from StepFlow block.
        /// </summary>
        private string[] defaultColors = { "#ea5545", "#f46a9b", "#ef9b20", "#edbf33", "#ede15b", "#bdcf32", "#87bc45", "#27aeef", "#b33dc6" };

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
            var elementId = "sankey_" + Guid.NewGuid();

            var engine = context.GetService<ILavaEngine>();
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );
            var mergedMarkup = engine.RenderTemplate( _internalMarkup, LavaRenderParameters.WithContext( context ) );
            var childElementsAreValid = ExtractBlockChildElements( context, mergedMarkup.Text, out var childElements, out var residualBlockContent );

            if ( !childElementsAreValid )
            {
                return;
            }

            var (nodes, edges) = GetNodesAndEdges( childElements );

            var nodeTooltipActionLabel = settings[ParameterKeys.NodeTooltipActionLabel];
            var legend = "";
            var showLegend = settings[ParameterKeys.ShowLegend].AsBooleanOrNull() ?? true;

            if ( showLegend && nodes.Count > 1 )
            {
                // TODO: Generate Legend
                legend += "<div class=\"flow-legend\">";

                foreach ( var node in nodes )
                {
                    legend += "<div class=\"flow-key\">";
                    legend += $@"<span class=""color"" style=""background-color:{node.Color}""></span>";
                    legend += $@"<span>{node.Order}. {node.Name}</span>";
                    legend += "</div>";
                }

                legend += "</div>";
            }

            var options = new SankeyDiagramSettingsBag
            {
                NodeWidth = settings[ParameterKeys.NodeWidth].AsIntegerOrNull() ?? 0,
                NodeVerticalSpacing = settings[ParameterKeys.NodeVerticalSpacing].AsIntegerOrNull() ?? 0,
                ChartWidth = settings[ParameterKeys.ChartWidth].AsIntegerOrNull() ?? 0,
                ChartHeight = settings[ParameterKeys.ChartHeight].AsIntegerOrNull() ?? 0,
                LegendHtml = legend
            };

            result.Write( $@"
<div id='{elementId}'></div>
<script>

Rock.Lava.Shortcode.SankeyDiagram.init(
    ""#{elementId}"",
    {edges.ToCamelCaseJson( true, true )},
    {nodes.ToCamelCaseJson( true, true )},
    {options.ToCamelCaseJson( true, true )},
    ""{nodeTooltipActionLabel}""
)

</script>
            " );

            // If we have a RockPage related to the current request then
            // register all the JS links we need.
#if REVIEW_WEBFORMS
            if ( HttpContext.Current?.Handler is RockPage page )
            {
                page.AddScriptLink( "~/Scripts/sankey-diagram-shortcode.js", true );
            }
#else
            throw new NotImplementedException();
#endif

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
            settings.AddOrIgnore( ParameterKeys.NodeWidth, "12" );
            settings.AddOrIgnore( ParameterKeys.NodeVerticalSpacing, "12" );
            settings.AddOrIgnore( ParameterKeys.ChartWidth, "800" );
            settings.AddOrIgnore( ParameterKeys.ChartHeight, "400" );
            settings.AddOrIgnore( ParameterKeys.ShowLegend, "true" );
            settings.AddOrIgnore( ParameterKeys.NodeTooltipActionLabel, "" );

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
        private (List<GraphNode>, List<GraphEdge>) GetNodesAndEdges( List<ChildBlockElement> elements )
        {
            var nodes = new List<GraphNode>();
            var edges = new List<GraphEdge>();
            var currentNodeOrder = 0;

            foreach ( var element in elements )
            {
                if ( element.Name == "node" )
                {
                    if ( !element.Parameters.ContainsKey( "id" ) || !element.Parameters.ContainsKey( "name" ) )
                    {
                        continue;
                    }

                    var node = new GraphNode
                    {
                        Id = element.Parameters.GetValueOrNull( "id" ).AsIntegerOrNull() ?? 0,
                        Name = element.Parameters.GetValueOrNull( "name" ),
                        Color = element.Parameters.GetValueOrNull( "color" ),
                        Order = ++currentNodeOrder,
                    };

                    if ( node.Color == null )
                    {
                        node.Color = GetNextDefaultColor();
                    }

                    nodes.Add( node );
                }
                else if ( element.Name == "edge" )
                {
                    if ( !element.Parameters.ContainsKey( "level" ) || !element.Parameters.ContainsKey( "sourceid" ) || !element.Parameters.ContainsKey( "targetid" ) || !element.Parameters.ContainsKey( "units" ) )
                    {
                        continue;
                    }

                    var edge = new GraphEdge
                    {
                        Level = element.Parameters.GetValueOrNull( "level" ).AsInteger(),
                        SourceId = element.Parameters.GetValueOrNull( "sourceid" ).AsInteger(),
                        TargetId = element.Parameters.GetValueOrNull( "targetid" ).AsInteger(),
                        Units = element.Parameters.GetValueOrNull( "units" ).AsInteger(),
                        Tooltip = element.Parameters.GetValueOrNull( "tooltip" )
                    };

                    edges.Add( edge );
                }
            }

            return (nodes, edges);
        }

        /// <summary>
        /// Choose a color from the list of default colors serially.
        /// </summary>
        /// <returns>A hexidecimal color string.</returns>
        private string GetNextDefaultColor()
        {
            if ( currentColorIndex >= defaultColors.Length )
            {
                currentColorIndex = 0;
            }

            return defaultColors[currentColorIndex++];
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
        /// Defines the structure of a single node on the diagram.
        /// </summary>
        private class GraphNode
        {
            /// <summary>
            /// The required identifier for the node.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The order, relative to other nodes, that it should be shown in.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// The required text label for the node.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The optional color that represents this node.
            /// </summary>
            public string Color { get; set; }
        }

        /// <summary>
        /// Defines the structure of an edge, or a flow from one node to another.
        /// </summary>
        private class GraphEdge
        {
            /// <summary>
            /// The required level for the edge. Level 1 represents the initial states and therefore should have empty source IDs.
            /// Level 2 is the flow from the initial state to the next state and so on.
            /// </summary>
            public int Level { get; set; }

            /// <summary>
            /// The required identifier of where the units are coming from. The identifier needs to match the ID of one of the nodes.
            /// Should be '0' for Level 1.
            /// </summary>
            public int SourceId { get; set; }

            /// <summary>
            /// The required identifier of where the units are going to. The identifier needs to match the ID of one of the nodes.
            /// </summary>
            public int TargetId { get; set; }

            /// <summary>
            /// The required number of units or items flowing from the source to the target.
            /// </summary>
            public int Units { get; set; }

            /// <summary>
            /// The optional tooltip you'll see when hovering over this flow between the nodes.
            /// If not provided, the number of units will be shown.
            /// </summary>
            public string Tooltip { get; set; }
        }

        #endregion
    }
}