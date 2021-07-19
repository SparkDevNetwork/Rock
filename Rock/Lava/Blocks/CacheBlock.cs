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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Cache allows you to cache the results of a Lava template.
    ///
    /// {% cache key:'my-content' %}
    ///     My Lava is now fast!
    /// {% endcache %}
    /// </summary>
    public class CacheBlock : LavaBlockBase, ILavaSecured
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _tagName = string.Empty;

        string _blockMarkup = null;

        const int _maxRecursionDepth = 10;

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
            _tagName = tagName;

            // Get the internal content of the block. The list of tokens passed in to custom blocks includes the block closing tag,
            // We need to remove the unmatched closing tag to get the valid internal markup for the block.
            if ( tokens.Any() )
            {
                var lastToken = tokens.Last().Replace( " ", "" );

                if ( lastToken.StartsWith( "{%end" ) || lastToken.StartsWith( "{%-end" ) )
                {
                    _blockMarkup = tokens.Take( tokens.Count - 1 ).JoinStrings( string.Empty );
                }
                else
                {
                    // If the final tag is not an (unmatched) closing tag, include it.
                    _blockMarkup = tokens.JoinStrings( string.Empty );
                }
            }
            else
            {
                _blockMarkup = string.Empty;
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
            // First ensure that cached commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                return;
            }

            var parms = ParseMarkup( _markup, context );

            var twoPassEnabled = parms["twopass"].AsBoolean();

            var cacheKey = "lavacache-" + parms["key"];
            if ( cacheKey == string.Empty )
            {
                result.Write( "* No cache key provided. *" );
                base.OnRender( context, result );
                return;
            }

            // Get content from cache
            var cachedResult = RockCache.Get( cacheKey, true ) as CacheLavaTag;

            // Check that the cached value is current
            if ( cachedResult != null )
            {
                var currentHash = CalculateContentHash( _blockMarkup.ToString() );
                if ( currentHash != cachedResult.Hash )
                {
                    cachedResult = null;
                }
            }

            // Use the cached value
            if ( cachedResult != null )
            {
                if ( twoPassEnabled )
                {
                    result.Write( MergeLava( cachedResult.Content, context ) );
                }
                else
                {
                    result.Write( cachedResult.Content );
                }

                base.OnRender( context, result );
                return;
            }

            // Cached value not available so render the template and cache it
            var lavaResults = MergeLava( _blockMarkup.ToString(), context );

            if ( lavaResults != null )
            {
                var cacheDuration = parms["duration"].AsInteger();

                if ( cacheDuration > 0 )
                {
                    // Don't cache if it's too large
                    var maxCacheSize = parms["maxcachesize"].AsInteger();

                    if ( lavaResults.Length < maxCacheSize )
                    {
                        var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
                        var cachedHash = CalculateContentHash( _blockMarkup.ToString() );
                        RockCache.AddOrUpdate( cacheKey, string.Empty, new CacheLavaTag { Hash = cachedHash, Content = lavaResults }, expiration, parms["tags"] );
                    }
                }

                // If twopass is enabled run the lava again
                if ( twoPassEnabled )
                {
                    lavaResults = MergeLava( lavaResults, context );
                }
            }

            result.Write( lavaResults );
        }

        /// <summary>
        /// Calculates the content hash.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private int CalculateContentHash( string content )
        {
            return ( content + _markup ).GetHashCode();
        }

        /// <summary>
        /// Cache Tag POCO
        /// </summary>
		[Serializable]
        [DataContract]
        private class CacheLavaTag
        {
            /// <summary>
            /// Gets or sets the hash.
            /// </summary>
            /// <value>
            /// The hash.
            /// </value>
			[DataMember]
            public int Hash { get; set; }

            /// <summary>
            /// Gets or sets the lava.
            /// </summary>
            /// <value>
            /// The lava.
            /// </value>
			[DataMember]
            public string Content { get; set; }
        }

        /// <summary>
        /// Merges the lava.
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string MergeLava( string lavaTemplate, ILavaRenderContext context )
        {
            // Resolve the Lava template contained in this block in a new context.
            var engine = context.GetService<ILavaEngine>();

            var newContext = engine.NewRenderContext();

            newContext.SetMergeFields( context.GetMergeFields() );
            newContext.SetInternalFields( context.GetInternalFields() );

            // Resolve the inner template.
            var result = engine.RenderTemplate( lavaTemplate, LavaRenderParameters.WithContext( newContext ) );

            return result.Text;
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, ILavaRenderContext context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = context.GetMergeFields();

            var parms = new Dictionary<string, string>();
            parms.Add( "key", string.Empty );
            parms.Add( "tags", string.Empty );
            parms.Add( "twopass", "false" );
            parms.Add( "duration", "3600" );
            parms.Add( "maxcachesize", "200000" );

            var markupItems = Regex.Matches( markup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    var value = itemParts[1];

                    if ( value.HasMergeFields() )
                    {
                        value = value.ResolveMergeFields( internalMergeFields );
                    }

                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), value.Substring( 1, value.Length - 2 ).Trim() );
                }
            }
            return parms;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "Cache";
            }
        }

        #endregion
    }
}