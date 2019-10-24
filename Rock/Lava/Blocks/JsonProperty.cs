﻿// <copyright>
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
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DotLiquid;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using RestSharp;
using RestSharp.Authenticators;
using Rock.Utility;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class JsonProperty : DotLiquid.Block, IRockStartup
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

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
            var parms = ParseMarkup( _markup, context );

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

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            foreach( var environment in context.Environments )
            {
                foreach ( var item in environment )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            var parms = new Dictionary<string, string>();
            parms.Add( "name", string.Empty );
            parms.Add( "format", "string" );

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