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
using System.Web;

using DotLiquid;

using Rock.Model;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        name: "Media Player",
        tagName: "mediaplayer",
        description: "Media Player displays a single URL or a Media Element in a player that can also record metric data.",
        documentation: MediaPlayerShortcode.DocumentationMetadata,
        parameters: MediaPlayerShortcode.ParameterNamesMetadata,
        enabledCommands: "" )]
    public class MediaPlayer : RockLavaShortcodeBlockBase
    {
        #region Methods

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterShortcode<MediaPlayer>( "mediaplayer" );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            var currentPerson = GetCurrentPerson( context );
            var parms = ParseMarkup( Markup, context );
            Guid? sessionGuid;

            // Attempt to get the session guid
            try
            {
                sessionGuid = ( HttpContext.Current.Handler as Web.UI.RockPage )?.Session["RockSessionId"]?.ToString().AsGuidOrNull();
            }
            catch
            {
                sessionGuid = null;
            }

            MediaPlayerShortcode.RenderToWriter( parms, currentPerson, sessionGuid, result );
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( Context context )
        {
            Person currentPerson = null;

            // First check for a person override value included in lava context
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "CurrentPerson" ) )
                    {
                        currentPerson = scopeHash["CurrentPerson"] as Person;
                    }
                }
            }

            // Check the HttpContext.
            if ( currentPerson == null )
            {
                var httpContext = HttpContext.Current;

                if ( context != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
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
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            return Rock.Lava.Shortcodes.MediaPlayerShortcode.ParseResolvedMarkup( resolvedMarkup );
        }

        #endregion
    }
}
