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
using System.Text.RegularExpressions;

using DotLiquid;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    ///
    /// </summary>
    public class DynamicShortcodeInline : RockLavaShortcodeBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _tagName = string.Empty;
        LavaShortcodeCache _shortcode;

        Dictionary<string, object> _internalMergeFields;

        const int _maxRecursionDepth = 10;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            // get all the inline dynamic shortcodes and register them
            var inlineShortCodes = LavaShortcodeCache.All().Where( s => s.TagType == TagType.Inline );

            foreach(var shortcode in inlineShortCodes )
            {
                // register this shortcode
                Template.RegisterShortcode<DynamicShortcodeInline>( shortcode.TagName );
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
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            if (_shortcode != null )
            {
                var parms = ParseMarkup( _markup, context );

                // add a unique id so shortcodes have easy access to one
                parms.AddOrReplace( "uniqueid", "id-" + Guid.NewGuid().ToString() );

                // keep track of the recursion depth
                int currentRecurrsionDepth = 0;
                if ( parms.ContainsKey( "RecursionDepth" ) )
                {
                    currentRecurrsionDepth = parms["RecursionDepth"].ToString().AsInteger() + 1;

                    if ( currentRecurrsionDepth > _maxRecursionDepth )
                    {
                        result.Write( "A recursive loop was detected and processing of this shortcode has stopped." );
                        return;
                    }
                }
                parms.AddOrReplace( "RecursionDepth", currentRecurrsionDepth );

                var results = _shortcode.Markup.ResolveMergeFields( parms, _shortcode.EnabledLavaCommands );
                result.Write( results );
            }
            else
            {
                result.Write( $"An error occurred while processing the {0} shortcode.", _tagName );
            }

            base.Render( context, result );
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

            // first run lava across the inputted markup
            _internalMergeFields = new Dictionary<string, object>();

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    _internalMergeFields.AddOrReplace( item.Key, item.Value );
                    parms.AddOrReplace( item.Key, item.Value );
                }
            }

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    _internalMergeFields.AddOrReplace( item.Key, item.Value );
                    parms.AddOrReplace( item.Key, item.Value );
                }
            }

            var resolvedMarkup = markup.ResolveMergeFields( _internalMergeFields );

            // create all the parameters from the shortcode with their default values
            var shortcodeParms = _shortcode.Parameters.Split( '|' ).ToList();
            foreach (var shortcodeParm in shortcodeParms )
            {
                var shortcodeParmKV = shortcodeParm.Split( '^' );
                if (shortcodeParmKV.Length == 2 )
                {
                    parms.AddOrReplace( shortcodeParmKV[0], shortcodeParmKV[1] );
                }
            }

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
    }
}