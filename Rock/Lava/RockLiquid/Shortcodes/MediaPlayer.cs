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
using Rock.Lava.DotLiquid;
using Rock.Model;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "Media Player",
        TagName = "mediaplayer",
        Description = "Media Player displays a single URL or a Media Element in a player that can also record metric data.",
        Documentation = MediaPlayerShortcode.DocumentationMetadata,
        Parameters = MediaPlayerShortcode.ParameterNamesMetadata,
        Categories = "C3270142-E72E-4FBF-BE94-9A2505DE7D54" )]
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
            int? visitorAliasId = null;
            var currentPerson = GetCurrentPerson( context );
            var settings = MediaPlayerShortcode.GetAttributesFromMarkup( this.Markup, new RockLiquidRenderContext( context ) );

            // Attempt to get the session guid
            Guid? sessionGuid;
            try
            {
                sessionGuid = ( HttpContext.Current?.Handler as Web.UI.RockPage )?.Session["RockSessionId"]?.ToString().AsGuidOrNull();
            }
            catch
            {
                sessionGuid = null;
            }

            if ( currentPerson == null )
            {
                visitorAliasId = GetVisitorAliasId( context );
            }

            MediaPlayerShortcode.RenderToWriter( settings.Attributes, currentPerson, visitorAliasId, sessionGuid, result );
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

                if ( context != null
                    && httpContext != null
                    && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Gets the visitor person alias identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static int? GetVisitorAliasId( Context context )
        {
            PersonAlias visitorAlias = null;

            // Check for a visitor value included in lava context
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "CurrentVisitor" ) )
                    {
                        visitorAlias = scopeHash["CurrentVisitor"] as PersonAlias;
                    }
                }
            }

            return visitorAlias?.Id;
        }

        #endregion
    }
}
