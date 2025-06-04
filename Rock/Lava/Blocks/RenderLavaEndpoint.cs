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

using Rock.Cms;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Renders a lava endpoint inline with the merged output for the Lava operation.
    /// </summary>
    public class RenderLavaEndpoint : LavaTagBase
    {
        #region Fields

        /// <summary>
        /// The markup text that represents the parameters for tag.
        /// </summary>
        private string _blockPropertiesMarkup = string.Empty;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _blockPropertiesMarkup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // Get the settings attributes from the Lava command
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );

            // No route no bother
            if ( settings["route"].IsNullOrWhiteSpace() )
            {
                return;
            }

            var route = settings["route"].TrimStart( "^/".ToCharArray() );
            var routeComponents = route.Split( new[] { '/' }, 2 );

            // Check for invalid route
            if ( routeComponents.Length != 2 )
            {
                return;
            }

            var applicationSlug = routeComponents[0];
            var endpointSlug = routeComponents[1];

            // Get Lava Application
            var lavaApplication = LavaApplicationCache.GetBySlug( applicationSlug );

            if ( lavaApplication == null || lavaApplication.IsActive == false )
            {
                return;
            }

            // Get Lava Endpoint
            var lavaEndpoint = lavaApplication.GetEndpoint( endpointSlug, LavaEndpoint.GetHttpMethodFromString( settings["method"] ) );

            if ( lavaEndpoint == null || lavaEndpoint.IsActive == false )
            {
                return;
            }

            var currentPerson = GetCurrentPerson( context );

            // Check if authorized
            if ( !lavaEndpoint.IsAuthorized( "Execute", currentPerson ) )
            {
                return;
            }

            result.Write( MergeRequest( lavaEndpoint, lavaApplication, currentPerson ) );
        }

        /// <summary>
        /// Get's the settings attributes from the Lava command.
        /// </summary>
        /// <param name="markup">The markup text that represents the parameters for the tag.</param>
        /// <param name="context">The Lava rendering context for this operation.</param>
        /// <returns>An instance of <see cref="LavaElementAttributes"/> that represents the parameters for this tag.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            // Default method is get
            if ( settings["method"].IsNullOrWhiteSpace() )
            {
                settings.AddOrIgnore( "method", "get" );
            }


            return settings;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The Lava context for the current render operation.</param>
        /// <returns>The <see cref="Person"/> designated as the current person for this operation.</returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = HttpContext.Current;
                if ( httpContext != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Runs the endpoint's logic and returns the results.
        /// </summary>
        /// <param name="lavaEndpoint">The lava endpoint.</param>
        /// <param name="lavaApplication">The lava application.</param>
        /// <param name="currentPerson">The current person related to this rendering request.</param>
        /// <returns>The output from the application endpoint.</returns>
        private string MergeRequest( LavaEndpointCache lavaEndpoint, LavaApplicationCache lavaApplication, Person currentPerson )
        {
            var mergeFields = LavaApplicationRequestHelpers.RequestToDictionary( HttpContext.Current.Request, currentPerson );
            mergeFields.Add( "ConfigurationRigging", lavaApplication.ConfigurationRigging );

            try
            {
                return lavaEndpoint.CodeTemplate.ResolveMergeFields(
                    mergeFields,
                    currentPerson,
                    lavaEndpoint.EnabledLavaCommands ).Trim();
            }
            catch ( Exception )
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
