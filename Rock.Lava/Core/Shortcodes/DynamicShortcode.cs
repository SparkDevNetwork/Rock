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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rock.Lava
{
    /// <summary>
    /// A shortcode that uses a parameterized Lava template supplied at runtime to dynamically generate a block or tag element in a Lava source document.
    /// </summary>
    public class DynamicShortcode : LavaShortcodeBase
    {
        string _elementAttributesMarkup = string.Empty;
        string _tagName = string.Empty;
        DynamicShortcodeDefinition _shortcode = null;
        StringBuilder _blockMarkup = new StringBuilder();
        ILavaEngine _engine;

        const int _maxRecursionDepth = 10;

        #region Constructors

        /// <summary>
        /// Default constructor required for internal use.
        /// An instance created using this constructor must call the Initialize() method before use.
        /// </summary>
        public DynamicShortcode()
        {
            //
        }

        public DynamicShortcode( DynamicShortcodeDefinition definition, ILavaEngine engine )
        {
            Initialize( definition, engine );

            AssertShortcodeIsInitialized();
        }

        #endregion

        /// <summary>
        ///  Initialize this shortcode instance with metadata provided by a shortcode definition.
        /// </summary>
        /// <param name="definition"></param>
        public void Initialize( DynamicShortcodeDefinition definition, ILavaEngine engine )
        {
            if ( _shortcode != null )
            {
                throw new Exception();
            }

            _shortcode = definition;
            _engine = engine;
        }

        /// <summary>
        /// Gets the type of this shortcode element.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return _shortcode.ElementType;
            }
        }

        #region Overrides

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            // This code is only required for the DotLiquid implementation of Lava.
            if ( _engine.EngineName == "RockLiquid" )
            {
                InitializeRockLiquidShortcode( tagName, markup, tokens );
                return;
            }

            _elementAttributesMarkup = markup;
            _tagName = tagName;

            _blockMarkup = new StringBuilder();

            if ( tokens.Any() )
            {
                // To allow for backward-compatibility with custom blocks developed for the DotLiquid framework,
                // the set of tokens returned by the Lava block parser includes the closing tag of the block.
                // We remove the closing tag here because it is not needed for our internal dynamic shortcode implementation.
                tokens = tokens.Take( tokens.Count - 1 ).ToList();

                foreach ( var tokenText in tokens )
                {
                    _blockMarkup.Append( tokenText );
                }
            }

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        private void InitializeRockLiquidShortcode( string tagName, string markup, List<string> tokens )
        {
            _elementAttributesMarkup = markup;
            _tagName = tagName;

            _blockMarkup = new StringBuilder();

            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.
            // We must take into consideration nested tags of the same type.

            var endTagFound = false;

            // Create regular expressions for start and end tags.
            var startTag = $@"{{\[\s*{_tagName}\s*(.*?)\]}}";
            var endTag = $@"{{\[\s*end{_tagName}\s*\]}}";

            var startTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {
                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    startTags++; // increment the child tag counter
                    if ( startTags > 1 )
                    {
                        _blockMarkup.Append( token );
                    }
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if ( startTags > 1 )
                        {
                            startTags--; // decrement the child tag counter
                            _blockMarkup.Append( token );
                        }
                        else
                        {
                            endTagFound = true;
                            break;
                        }
                    }
                    else
                    {
                        _blockMarkup.Append( token );
                    }
                }

            }

            // If this is a block, we need a closing tag.
            if ( this.ElementType == LavaShortcodeTypeSpecifier.Block
                 && !endTagFound )
            {
                AssertMissingDelimitation();
            }

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            if ( _shortcode == null )
            {
                result.Write( $"An error occurred while processing the {0} shortcode.", _tagName );
            }

            // Get the parameters and default values defined by the shortcode, then apply the parameters that have been specified in the shortcode tag attributes
            // and those that are stored in the current render context.
            var parms = new Dictionary<string, object>();

            foreach ( var shortcodeParm in _shortcode.Parameters )
            {
                parms.AddOrReplace( shortcodeParm.Key, shortcodeParm.Value );
            }

            SetParametersFromElementAttributes( parms, _elementAttributesMarkup, context );

            // Set a unique id for the shortcode.
            parms.AddOrReplace( "uniqueid", "id-" + Guid.NewGuid().ToString() );

            // Apply the merge fields in the block context.
            var internalMergeFields = context.GetMergeFields();

            foreach ( var item in parms )
            {
                internalMergeFields.AddOrReplace( item.Key, item.Value );
            }

            // Add parameters for tracking the recursion depth.
            int currentRecursionDepth = 0;

            if ( parms.ContainsKey( "RecursionDepth" ) )
            {
                currentRecursionDepth = parms["RecursionDepth"].ToString().AsInteger() + 1;

                if ( currentRecursionDepth > _maxRecursionDepth )
                {
                    result.Write( "A recursive loop was detected and processing of this shortcode has stopped." );
                    return;
                }
            }

            parms.AddOrReplace( "RecursionDepth", currentRecursionDepth );

            // Resolve the merge fields in the shortcode template in a separate context, using the set of merge fields that have been modified by the shortcode parameters.
            // Apply the set of enabled commands specified by the shortcode definition, or those enabled for the current context if none are defined by the shortcode.
            // The resulting content is a shortcode template that is ready to be processed to resolve its child elements.
            var shortcodeCommands = _shortcode.EnabledLavaCommands ?? new List<string>();

            shortcodeCommands = shortcodeCommands.Where( x => !string.IsNullOrWhiteSpace( x ) ).ToList();

            if ( !shortcodeCommands.Any() )
            {
                shortcodeCommands = context.GetEnabledCommands();
            }

            var shortcodeTemplateContext = _engine.NewRenderContext( internalMergeFields, shortcodeCommands );
            var blockMarkupRenderResult = _engine.RenderTemplate( _blockMarkup.ToString(), LavaRenderParameters.WithContext( shortcodeTemplateContext ) );
            var shortcodeTemplateMarkup = blockMarkupRenderResult.Text;

            // Extract child elements from the shortcode template content.
            // One or more child elements can be added to a shortcode block using the syntax "[[ <childElementName> <paramName1>:value1 <paramName2>:value2 ... ]] ... [[ end<childElementName> ]]",
            // where <childElementName> is a shortcode-specific tag name, <param1> is a shortcode parameter name, and <value1> is the parameter value.
            // Child elements are grouped by <childElementName>, and each collection is passed as a separate parameter to the shortcode template
            // using the variable name "<childElementNameItems>". The first element of the array is also added using the variable name "<childElementName>".
            // Parameters declared on child elements can be referenced in the shortcode template as <childElementName>.<paramName>.
            Dictionary<string, object> childElements;

            string residualMarkup;
            var childElementsAreValid = ExtractShortcodeBlockChildElements( shortcodeTemplateMarkup, out childElements, out residualMarkup );

            if ( !childElementsAreValid )
            {
                // The residual block markup contains the error message, so write it to the output stream.
                result.Write( residualMarkup );
                return;
            }

            // Add the collections of child to the set of parameters that will be passed to the shortcode template.
            foreach ( var item in childElements )
            {
                parms.AddOrReplace( item.Key, item.Value );
            }

            // Set context variables related to the block content so they can be referenced by the shortcode template.
            var blockHasContent = residualMarkup.IsNotNullOrWhiteSpace();
            parms.AddOrReplace( "blockContentExists", blockHasContent );

            if ( blockHasContent )
            {
                parms.AddOrReplace( "blockContent", residualMarkup );

                // Render the shortcode template to check for security error messages.
                // This is necessary because the shortcode may be configured to permit access to more entities than the source block, template, or current action permits.
                // Note that in some situations, the template may fail to render for other reasons - for example, where the previous render operation has
                // introduced invalid Lava as the output of a {% raw %} tag.
                // This method of verifying security is unreliable and should be replaced with a more robust implementation in the future.
                // We need a render process that can replace merge fields while leaving tags and blocks intact.
                var securityRenderParameters = new LavaRenderParameters
                {
                    Context = context,
                    ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput
                };
                var securityCheckResult = _engine.RenderTemplate( residualMarkup, securityRenderParameters );

                var securityErrorPattern = new Regex( string.Format( Constants.Messages.NotAuthorizedMessage, ".*" ) );
                var securityErrorMatch = securityErrorPattern.Match( securityCheckResult.Text );

                // If the security check failed, return the error message.
                if ( securityErrorMatch.Success )
                {
                    result.Write( securityErrorMatch.Value );

                    return;
                }
            }

            // Render the shortcode template in a child scope that includes the shortcode parameters.
            context.EnterChildScope();

            LavaRenderResult results;
            try
            {
                context.SetMergeFields( parms );

                // If the shortcode specifies a set of Lava commands, add these to the context.
                // The set of permitted entity commands is the union of the parent scope and the specific shortcode settings.
                if ( _shortcode.EnabledLavaCommands != null )
                {
                    var enabledCommands = context.GetEnabledCommands();
                    foreach ( var commandName in _shortcode.EnabledLavaCommands )
                    {
                        if ( !enabledCommands.Contains(commandName) )
                        {
                            enabledCommands.Add( commandName );
                        }
                    }
                    context.SetEnabledCommands( enabledCommands );
                }

                results = _engine.RenderTemplate( _shortcode.TemplateMarkup, LavaRenderParameters.WithContext( context ) );
                result.Write( results.Text.Trim() );
            }
            finally
            {
                context.ExitChildScope();
            }
        }

        #endregion

        /// <summary>
        /// Extracts a set of child elements from the content of a shortcode block.
        /// Child elements are grouped by tag name, and each item in the collection has a set of properties
        /// corresponding to the child element tag attributes and a "content" property representing the inner content of the child element.
        /// </summary>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childParameters">The child parameters.</param>
        /// <returns></returns>
        private bool ExtractShortcodeBlockChildElements( string blockContent, out Dictionary<string, object> childParameters, out string residualBlockContent )
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

                            // create dynamic object from parms
                            var dynamicParm = new Dictionary<string, Object>();
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

                            // add new parm to a collection of parms and as a single parm if none exist
                            if ( childParameters.ContainsKey( parmName + "s" ) )
                            {
                                var parmList = (List<object>)childParameters[parmName + "s"];
                                parmList.Add( dynamicParm );
                            }
                            else
                            {
                                var parmList = new List<object>();
                                parmList.Add( dynamicParm );
                                childParameters.Add( parmName + "s", parmList );
                            }

                            if ( !childParameters.ContainsKey( parmName ) )
                            {
                                childParameters.Add( parmName, dynamicParm );
                            }

                            // pull this tag out of the block content
                            blockContent = blockContent.Remove( startTagStartIndex, endTagEndIndex - startTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            isValid = false;
                            matchExists = false;
                            blockContent = blockContent + "Warning: missing end tag end" + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        isValid = false;
                        matchExists = false;
                        blockContent = blockContent + "Warning: invalid child parameter definition.";
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
        /// Parses the element attributes markup to collate parameter settings for the shortcode.
        /// </summary>
        /// <param name="elementAttributesMarkup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private void SetParametersFromElementAttributes( Dictionary<string, object> parameters, string elementAttributesMarkup, ILavaRenderContext context )
        {
            // Resolve any Lava merge fields in the element attributes markup.
            var resolvedMarkup = _engine.RenderTemplate( elementAttributesMarkup, LavaRenderParameters.WithContext( context ) );

            var markupItems = Regex.Matches( resolvedMarkup.Text, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );

                if ( itemParts.Length > 1 )
                {
                    parameters.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }

            // OK, now let's look for any passed variables ala: name:variable
            var variableTokens = Regex.Matches( resolvedMarkup.Text, @"\w*:\w+(\[\d+\])?" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in variableTokens )
            {
                var itemParts = item.Trim().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    var scopeKey = itemParts[1].Trim();
                    object scopeObject = null;

                    // if scopeKey appears to be indexing a Collection
                    if ( Regex.Match( scopeKey, @"\w+\[\d+\]" ).Success )
                    {
                        // Get the Collection name and index.
                        string[] parts = scopeKey.Split( new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries );
                        string arrayName = parts[0];
                        int scopeObjectIndex = int.Parse( parts[1] );

                        // Get the collection and the value at the index.
                        var enumerable = context.GetMergeField( arrayName, null ) as IEnumerable;
                        scopeObject = enumerable?.Cast<object>().ElementAtOrDefault( scopeObjectIndex );
                    }
                    else
                    {
                        scopeObject = context.GetMergeField( scopeKey, null );
                    }

                    if ( scopeObject != null )
                    {
                        parameters.AddOrReplace( itemParts[0].Trim().ToLower(), scopeObject );
                    }
                }
            }
        }

        private void AssertShortcodeIsInitialized()
        {
            if ( _shortcode == null )
            {
                throw new Exception( $"Shortcode configuration error. \"{_tagName}\" is not initialized." );
            }
        }

    }
}