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
using DotLiquid;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Block = DotLiquid.Block;

namespace Rock.Lava.RockLiquid.Blocks
{
    /// <summary>
    /// A Lava Block that provides a means of rendering content based on a set of personalization filters.
    /// Segments and filters can be used to determine the visibility of the block for the current user and request.
    /// </summary>
    public class Personalize : Block, IRockStartup
    {
        #region Filter Parameter Names

        /// <summary>
        /// Parameter name for specifying maximum occurrences. If not specified, the default value is 100.
        /// </summary>
        public static readonly string ParameterMatchType = "matchtype";
        /// <summary>
        /// Parameter name for specifying a filter for the intended audiences of the Event Occurrences. If not specified, all audiences are considered.
        /// </summary>
        public static readonly string ParameterSegments = "segment";
        /// <summary>
        /// Parameter name for specifying a filter for the campus of the Event Occurrences. If not specified, all campuses are considered.
        /// </summary>
        public static readonly string ParameterRequestFilters = "requestfilter";
        /// <summary>
        /// Parameter name for specifying the Person for whom the block should be evaluated.
        /// This parameter is most useful for testing purposes.
        /// </summary>
        public static readonly string ParameterPersonIdentifier = "person";

        #endregion

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public void OnStartup()
        {
            Template.RegisterTag<Personalize>( TagSourceName );
        }

        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "personalize";

        private string _attributesMarkup;
        private bool _renderErrors = true;

        LavaElementAttributes _settings = new LavaElementAttributes();

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            try
            {
                _settings.ParseFromMarkup( _attributesMarkup, context );

                var showContent = ShowContentForCurrentRequest( context );
                if ( showContent )
                {
                    base.Render( context, result );
                }
            }
            catch ( Exception ex )
            {
                var message = "Personalization block not available. " + ex.Message;

                if ( _renderErrors )
                {
                    result.Write( message );
                }
                else
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        private bool ShowContentForCurrentRequest( Context context )
        {
            var person = LavaHelper.GetCurrentPerson( context );
            var rockContext = new RockContext();

            var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForRequest( person,
                rockContext,
                System.Web.HttpContext.Current?.Request );

            var requestFilterIdList = LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList();

            var matchType = _settings.GetStringValue( ParameterMatchType, "any" ).ToLower();

            // Determine if the current block segments match the segments for the user in the current context.
            var specifiedSegmentIdList = PersonalizationSegmentCache.GetByKeys( _settings.GetStringValue( ParameterSegments ) )
                .Select( ps => ps.Id )
                .ToList();
            if ( specifiedSegmentIdList.Any() )
            {
                if ( matchType == "all" )
                {
                    var hasUnmatchedElement = specifiedSegmentIdList.Any( id => !personSegmentIdList.Contains( id ) );
                    if ( hasUnmatchedElement )
                    {
                        return false;
                    }
                }
                else if ( matchType == "none" )
                {
                    var hasMatchedElement = specifiedSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                    if ( hasMatchedElement )
                    {
                        return false;
                    }
                }
                else
                {
                    // Apply default match type of "any".
                    var hasMatchedElement = specifiedSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                    if ( !hasMatchedElement )
                    {
                        return false;
                    }
                }
            }

            // Apply the request filters if we are processing a HTTP request.
            if ( requestFilterIdList != null )
            {
                var specifiedRequestIdList = RequestFilterCache.GetByKeys( _settings.GetStringValue( ParameterRequestFilters ) )
                    .Select( ps => ps.Id )
                    .ToList();
                if ( specifiedRequestIdList.Any() )
                {
                    if ( matchType == "all" )
                    {
                        var hasUnmatchedElement = specifiedRequestIdList.Any( id => !requestFilterIdList.Contains( id ) );
                        if ( hasUnmatchedElement )
                        {
                            return false;
                        }
                    }
                    else if ( matchType == "none" )
                    {
                        var hasMatchedElement = specifiedRequestIdList.Any( id => requestFilterIdList.Contains( id ) );
                        if ( hasMatchedElement )
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Apply default match type of "any".
                        var hasMatchedElement = specifiedRequestIdList.Any( id => requestFilterIdList.Contains( id ) );
                        if ( !hasMatchedElement )
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
