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
using System.Text;
using System.Text.RegularExpressions;

using DotLiquid;
using DotLiquid.Util;

using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    ///
    /// </summary>
    public class DynamicShortcodeBlock : RockLavaShortcodeBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _tagName = string.Empty;
        LavaShortcodeCache _shortcode;
        Dictionary<string,object> _internalMergeFields;
        string _enabledSecurityCommands = "";

        StringBuilder _blockMarkup = new StringBuilder();

        const int _maxRecursionDepth = 10;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            // get all the block dynamic shortcodes and register them
            var blockShortCodes = LavaShortcodeCache.All().Where( s => s.TagType == TagType.Block );

            foreach(var shortcode in blockShortCodes )
            {
                // register this shortcode
                Template.RegisterShortcode<DynamicShortcodeBlock>( shortcode.TagName );
            }
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
            _tagName = tagName;
            _shortcode = LavaShortcodeCache.All().Where( c => c.TagName == tagName ).FirstOrDefault();

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        protected override void Parse( List<string> tokens )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type

            var endTagFound = false;

            var startTag = $@"{{\[\s*{ _tagName }\s*\]}}";
            var endTag = $@"{{\[\s*end{ _tagName }\s*\]}}";

            var childTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {

                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    childTags++; // increment the child tag counter
                    _blockMarkup.Append( token );
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if (childTags > 0 )
                        {
                            childTags--; // decrement the child tag counter
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

            if ( !endTagFound )
            {
                AssertMissingDelimitation();
            }
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            // get enabled security commands
            if ( context.Registers.ContainsKey( "EnabledCommands" ) )
            {
                _enabledSecurityCommands = context.Registers["EnabledCommands"].ToString();
            }

            var shortcode = LavaShortcodeCache.Get( _shortcode.Id );

            if ( shortcode != null )
            {
                var parms = ParseMarkup( _markup, context );

                // add a unique id so shortcodes have easy access to one
                parms.AddOrReplace( "uniqueid", "id-" + Guid.NewGuid().ToString() );

                // keep track of the recursion depth
                int currentRecurrsionDepth = 0;
                if ( parms.ContainsKey( "RecursionDepth" ) )
                {
                    currentRecurrsionDepth = parms["RecursionDepth"].ToString().AsInteger() + 1;

                    if (currentRecurrsionDepth > _maxRecursionDepth )
                    {
                        result.Write( "A recursive loop was detected and processing of this shortcode has stopped." );
                        return;
                    }
                }
                parms.AddOrReplace( "RecursionDepth", currentRecurrsionDepth );

                var lavaTemplate = shortcode.Markup;
                var blockMarkup = _blockMarkup.ToString().ResolveMergeFields( _internalMergeFields, _enabledSecurityCommands );

                // pull child parameters from block content
                Dictionary<string, object> childParamters;
                blockMarkup = GetChildParameters( blockMarkup, out childParamters );
                foreach(var item in childParamters )
                {
                    parms.AddOrReplace( item.Key, item.Value );
                }

                // merge the block markup in
                if ( blockMarkup.IsNotNullOrWhiteSpace() )
                {
                    Regex rgx = new Regex( @"{{\s*blockContent\s*}}", RegexOptions.IgnoreCase );
                    lavaTemplate = rgx.Replace( lavaTemplate, blockMarkup );

                    parms.AddOrReplace( "blockContentExists", true );
                }
                else
                {
                    parms.AddOrReplace( "blockContentExists", false );
                }

                // next ensure they did not use any entity commands in the block that are not allowed
                // this is needed as the shortcode it configured to allow entities for processing that
                // might allow more entities than the source block, template, action, etc allows
                var securityCheck = blockMarkup.ResolveMergeFields(new Dictionary<string, object>(), _enabledSecurityCommands);

                Regex securityPattern = new Regex( String.Format(RockLavaBlockBase.NotAuthorizedMessage, ".*" ) );
                Match securityMatch = securityPattern.Match( securityCheck );

                if ( securityMatch.Success )
                {
                    result.Write( securityMatch.Value ); // return security error message
                }
                else
                {
                    if ( shortcode.EnabledLavaCommands.IsNotNullOrWhiteSpace() )
                    {
                        _enabledSecurityCommands = shortcode.EnabledLavaCommands;
                    }

                    var results = lavaTemplate.ResolveMergeFields( parms, _enabledSecurityCommands );
                    result.Write( results.Trim() );
                    base.Render( context, result );
                }
            }
            else
            {
                result.Write( $"An error occurred while processing the {0} shortcode.", _tagName );
            }
        }

        /// <summary>
        /// Gets the child parameters.
        /// </summary>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childParameters">The child parameters.</param>
        /// <returns></returns>
        private string GetChildParameters(string blockContent, out Dictionary<string, object> childParameters )
        {
            childParameters = new Dictionary<string, object>();

            var startTagStartExpress = new Regex( @"\[\[\s*" );

            var matchExists = true;
            while ( matchExists )
            {
                var match = startTagStartExpress.Match( blockContent );
                if ( match.Success )
                {
                    int starTagStartIndex = match.Index;

                    // get the name of the parameter
                    var parmNameMatch = new Regex( @"[\w-]*" ).Match( blockContent, starTagStartIndex + match.Length );
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
                        var endTagMatch = new Regex( endTagMatchExpression ).Match( blockContent, starTagStartIndex );

                        if ( endTagMatch != null )
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

                            // add new parm to a collection of parms and as a single parm id none exist
                            if ( childParameters.ContainsKey(parmName + "s") )
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
                            blockContent = blockContent.Remove( starTagStartIndex, endTagEndIndex - starTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            matchExists = false;
                            blockContent = blockContent + "Warning: missing end tag end" + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        matchExists = false;
                        blockContent = blockContent + "Warning: invalid child parameter definition.";
                    }

                }
                else
                {
                    matchExists = false; // we're done here
                }
            }

            return blockContent.Trim();
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, object> ParseMarkup( string markup, Context context )
        {
            var parms = new Dictionary<string, object>();

            LoadBlockMergeFields( context, parms );

            // create all the parameters from the shortcode with their default values
            var shortcodeParms = _shortcode.Parameters.Split( '|' ).ToList();
            foreach ( var shortcodeParm in shortcodeParms )
            {
                var shortcodeParmKV = shortcodeParm.Split( '^' );
                if ( shortcodeParmKV.Length == 2 )
                {
                    parms.AddOrReplace( shortcodeParmKV[0], shortcodeParmKV[1] );
                }
            }

            // first run lava across the inputted markup
            var resolvedMarkup = markup.ResolveMergeFields( _internalMergeFields );

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

        /// <summary>
        /// Loads the block merge fields.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parms">The parms.</param>
        private void LoadBlockMergeFields( Context context, Dictionary<string, object> parms )
        {
            _internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    _internalMergeFields.AddOrReplace( item.Key, item.Value );
                    //parms.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    _internalMergeFields.AddOrReplace( item.Key, item.Value );
                    parms.AddOrReplace( item.Key, item.Value );
                }
            }
        }
    }
}