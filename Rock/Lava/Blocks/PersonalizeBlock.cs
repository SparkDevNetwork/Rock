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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// A Lava Block that provides a means of rendering content based on a set of personalization filters.
    /// Segments and filters can be used to determine the visibility of the block for the current user and request.
    /// </summary>
    public class PersonalizeBlock : LavaBlockBase
    {
        #region Filter Parameter Names

        /// <summary>
        /// Parameter name for specifying the match type. If not specified, the default value is "any"
        /// </summary>
        public static readonly string ParameterMatchType = "matchtype";
        /// <summary>
        /// Parameter name for specifying a delimited list of personalization segments.
        /// </summary>
        public static readonly string ParameterSegments = "segment";
        /// <summary>
        /// Parameter name for specifying a delimited list of request filters.
        /// </summary>
        public static readonly string ParameterRequestFilters = "requestfilter";
        /// <summary>
        /// Parameter name for specifying the Person for whom the block should be evaluated.
        /// This parameter is most useful for testing purposes.
        /// </summary>
        public static readonly string ParameterPersonIdentifier = "person";

        #endregion

        /// <summary>
        /// The name of the element as it is used in the source document.
        /// </summary>
        public static readonly string TagSourceName = "personalize";

        private string _attributesMarkup;
        private bool _renderErrors = true;
        private string _matchContent = null;
        private string _otherwiseContent = null;
        LavaElementAttributes _settings = new LavaElementAttributes();

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="PersonalizeBlock"/> class.
        /// </summary>
        public PersonalizeBlock()
        {
            this.IncludeClosingTokenInParseResult = false;
        }

        #endregion

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;

            // Scan the block tokens and separate matched and unmatched content.
            // If unmatched content exists, it is shown when the block filter is not satisfied.
            var otherwiseFound = false;
            foreach ( var token in tokens )
            {
                var scanToken = token.Replace( " ", "" ).Replace( "-", "" ).ToLower();
                if ( scanToken == "{%otherwise%}" )
                {
                    if ( otherwiseFound )
                    {
                        // If more than one alternate tag exists, throw an error.
                        throw new Exception( "Unexpected {% otherwise %} encountered." );
                    }
                    otherwiseFound = true;
                    continue;
                }

                if ( otherwiseFound )
                {
                    _otherwiseContent += token;
                }
                else
                {
                    _matchContent += token;
                }
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
            try
            {
                _settings.ParseFromMarkup( _attributesMarkup, context );

                var showContent = ShowContentForCurrentRequest( context );

                // Render the internal template, before or after the {% otherwise %} tag if it exists.
                var content = ( showContent ) ? _matchContent : _otherwiseContent;

                if ( !string.IsNullOrEmpty( content ) )
                {
                    var engine = context.GetService<ILavaEngine>();
                    var renderResult = engine.RenderTemplate( content, new LavaRenderParameters { Context = context } );
                    result.Write( renderResult.Text );
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

        /// <summary>
        /// Determine if the block content should be shown for the current request and user.
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// Changes to this method should remain synchronised with the Personalize.ShowContentForCurrentRequest() method.
        /// </remarks>
        /// <returns></returns>
        private bool ShowContentForCurrentRequest( ILavaRenderContext context )
        {
            var matchType = _settings.GetString( ParameterMatchType, "any" ).ToLower();

            // Apply the request filters if we are processing a HTTP request.
            // Do this first because we may have the opportunity to exit early and avoid retrieving personalization segments.
            bool? requestFilterIsValid = null;
            var requestFilterParameterString = _settings.GetStringOrNull( ParameterRequestFilters )
                ?? _settings.GetString( "requestfilters" );

            if ( !string.IsNullOrWhiteSpace( requestFilterParameterString ) )
            {
                var currentFilterIdList = LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList();
                if ( currentFilterIdList != null )
                {
                    var requiredRequestIdList = RequestFilterCache.GetByKeys( requestFilterParameterString )
                        .Select( ps => ps.Id )
                        .ToList();
                    if ( matchType == "all" )
                    {
                        // All of the specified filters must be matched, so we need to fail for any invalid keys.
                        var requestFilterParameterCount = requestFilterParameterString.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).Count();
                        requestFilterIsValid = ( requiredRequestIdList.Count == requestFilterParameterCount )
                            && requiredRequestIdList.All( id => currentFilterIdList.Contains( id ) );
                    }
                    else if ( matchType == "none" )
                    {
                        requestFilterIsValid = !requiredRequestIdList.Any( id => currentFilterIdList.Contains( id ) );
                    }
                    else
                    {
                        // Apply default match type of "any".
                        requestFilterIsValid = requiredRequestIdList.Any( id => currentFilterIdList.Contains( id ) );
                    }
                }
            }

            // Check for an early exit to avoid the overhead of processing segments.
            if ( requestFilterIsValid != null )
            {
                if ( requestFilterIsValid.Value )
                {
                    if ( requestFilterIsValid.Value && matchType == "any" )
                    {
                        return true;
                    }
                }
                else
                {
                    if ( matchType != "any" )
                    {
                        // If request filters exist and the match conditions are not satisfied, do not show the content.
                        return false;
                    }
                }
            }

            // Determine if the current block segments match the segments for the user in the current context.
            bool? segmentFilterIsValid = null;
            var segmentParameterString = _settings.GetStringOrNull( ParameterSegments )
                ?? _settings.GetString( "segments" );
            if ( !string.IsNullOrWhiteSpace( segmentParameterString ) )
            {
                // Get personalization segments for the target person.
                var personReference = _settings.GetString( "person" );
                Person person;
                if ( !string.IsNullOrEmpty( personReference ) )
                {
                    person = LavaHelper.GetPersonFromInputParameter( personReference, context );
                }
                else
                {
                    // If the Lava context contains a Person variable, prefer it to the CurrentPerson.
                    // This allows the block to be used in processes where there is no active user.
                    person = context.GetMergeField( "Person" ) as Person;
                    if ( person == null )
                    {
                        person = LavaHelper.GetCurrentPerson( context );
                    }
                }

                var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie( context,
                    System.Web.HttpContext.Current,
                    person );

                var requiredSegmentIdList = PersonalizationSegmentCache.GetByKeys( segmentParameterString )
                    .Select( ps => ps.Id )
                    .ToList();
                if ( matchType == "all" )
                {
                    // All of the specified segments must be matched, so we need to fail for any invalid keys.
                    var segmentParameterCount = segmentParameterString.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).Count();
                    segmentFilterIsValid = ( requiredSegmentIdList.Count == segmentParameterCount )
                        && requiredSegmentIdList.All( id => personSegmentIdList.Contains( id ) );
                }
                else if ( matchType == "none" )
                {
                    segmentFilterIsValid = !requiredSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                }
                else
                {
                    // Apply default match type of "any".
                    segmentFilterIsValid = requiredSegmentIdList.Any( id => personSegmentIdList.Contains( id ) );
                }
            }

            // If no parameters are specified for the block, do not show the content.
            if ( requestFilterIsValid == null && segmentFilterIsValid == null )
            {
                return false;
            }

            bool showContent = false;
            if ( matchType == "all" )
            {
                showContent = requestFilterIsValid.GetValueOrDefault( true ) && segmentFilterIsValid.GetValueOrDefault( true );
            }
            else if ( matchType == "any" )
            {
                showContent = requestFilterIsValid.GetValueOrDefault( false ) || segmentFilterIsValid.GetValueOrDefault( false );
            }
            else if ( matchType == "none" )
            {
                showContent = requestFilterIsValid.GetValueOrDefault( true ) && segmentFilterIsValid.GetValueOrDefault( true );
            }

            return showContent;
        }
    }
}
