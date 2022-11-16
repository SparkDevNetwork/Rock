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
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

namespace Rock.Rest
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable
    /// querying using the OData query syntax using the precompiled Rock EdmModel.
    /// This also will detect datetime and guid $filter parameters that might be in ODataV3 format
    /// and convert them to the ODataV4 spec.
    /// </summary>
    public class RockEnableQueryAttribute : EnableQueryAttribute
    {
        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public override Microsoft.OData.Edm.IEdmModel GetModel( Type elementClrType, System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor )
        {
            // use the EdmModel that we already created in WebApiConfig (so that we don't have problems with OData4 Open Types)
            var ourModel = WebApiConfig.EdmModel;
            return ourModel;
        }

        /// <summary>
        /// Applies the query to the given entity based on incoming query from uri and query settings.
        /// </summary>
        /// <param name="entity">The original entity from the response message.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public override object ApplyQuery( object entity, ODataQueryOptions queryOptions )
        {
            return base.ApplyQuery( entity, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Applies the query to the given IQueryable based on incoming query from uri and query settings. By default,
        /// the implementation supports $top, $skip, $orderby and $filter. Override this method to perform additional
        /// query composition of the query.
        /// </summary>
        /// <param name="queryable">The original queryable instance from the response message.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        /// <returns>IQueryable.</returns>
        public override IQueryable ApplyQuery( IQueryable queryable, ODataQueryOptions queryOptions )
        {
            return base.ApplyQuery( queryable, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
        /// the query contains unsupported query parameters. Override this method to perform additional validation of
        /// the query.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        public override void ValidateQuery( HttpRequestMessage request, ODataQueryOptions queryOptions )
        {
            base.ValidateQuery( request, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Ensures the v4 o data query options.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns>ODataQueryOptions.</returns>
        private ODataQueryOptions EnsureV4ODataQueryOptions( ODataQueryOptions queryOptions )
        {
            if ( RequestUriHasObsoleteV3Filters( queryOptions ) )
            {
                return ConvertOData3FiltersToODataV4( queryOptions );
            }
            else
            {
                return queryOptions;
            }
        }

        /// <summary>
        /// Return true if the Request has OData V3 filters that were made obsolete in V4
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool RequestUriHasObsoleteV3Filters( ODataQueryOptions queryOptions )
        {
            var rawFilter = queryOptions?.Filter?.RawValue;
            if ( rawFilter.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var isV3DateTimeFilter = _dateTimeFilterCapture.IsMatch( rawFilter );
            var isV3GuidFilter = _guidFilterCapture.IsMatch( rawFilter );
            return isV3DateTimeFilter || isV3GuidFilter;
        }

        /// <summary>
        /// This RegEx is used to find datetime $filters in the obsolete V3 format, which will then be converted
        /// to the V4 spec.
        /// </summary>
        /// <remarks>
        /// $filter in V3 Format - <code>ModifiedDateTime eq datetime'2022-10-04T10:56:50.747'</code>
        /// $filter in V4 Format - <code>ModifiedDateTime eq 2022-10-04T10:56:50.747</code>
        /// </remarks>
        private static readonly Regex _dateTimeFilterCapture = new Regex( @"datetime\'(\S*)\'", RegexOptions.Compiled );

        /// <summary>
        /// This RegEx is used to find guid $filters in the obsolete V3 format, which will then be converted
        /// to the V4 spec.
        /// </summary>
        /// <remarks>
        /// $filter in V3 Format - <code>Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'</code>
        /// $filter in V4 Format - <code>Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53</code>
        /// </remarks>
        private static readonly Regex _guidFilterCapture = new Regex( @"guid\'([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\'", RegexOptions.Compiled );

        /// <summary>
        /// Converts the o data3 filters to o data v4.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns>ODataQueryOptions.</returns>
        private ODataQueryOptions ConvertOData3FiltersToODataV4( ODataQueryOptions queryOptions )
        {
            var originalUrl = queryOptions.Request.RequestUri.OriginalString;
            var rawFilter = queryOptions?.Filter?.RawValue;

            string updatedUrl = ParseUrl( originalUrl, rawFilter );

            var convertedRequest = new HttpRequestMessage( queryOptions.Request.Method, updatedUrl );
            foreach ( var origProperty in queryOptions.Request.Properties )
            {
                convertedRequest.Properties.Add( origProperty.Key, origProperty.Value );
            }

            foreach ( var origProperty in queryOptions.Request.Headers )
            {
                convertedRequest.Headers.Add( origProperty.Key, origProperty.Value );
            }

            var convertedODataQueryOptions = new ODataQueryOptions( queryOptions.Context, convertedRequest );

            return convertedODataQueryOptions;
        }

        internal static string ParseUrl( string originalUrl, string rawFilter )
        {
            /* 10/14/2022 MDP

            ODataV4 has a breaking change regarding datetime and guid filters.

            This will take care of $filters in the obsolete V3 format, which will then be converted
            to the V4 spec.

            For example:

            DateTime $filter in V3 Format - <code>ModifiedDateTime eq datetime'2022-10-04T10:56:50.747'</code>
            DateTime $filter in V4 Format - <code>ModifiedDateTime eq 2022-10-04T10:56:50.747</code>
             
            Guid $filter in V3 Format - <code>Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'</code>
            Guid $filter in V4 Format - <code>Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53</code> 
             
             */

            var dateTimeMatches = _dateTimeFilterCapture.Matches( rawFilter );
            var guidMatches = _guidFilterCapture.Matches( rawFilter );
            if ( guidMatches.Count == 0 && dateTimeMatches.Count == 0 )
            {
                return originalUrl;
            }

            var updatedUrl = originalUrl;

            foreach ( Match match in dateTimeMatches )
            {
                updatedUrl = ParseMatch( updatedUrl, match, true );
            }

            foreach ( Match match in guidMatches )
            {
                updatedUrl = ParseMatch( updatedUrl, match );
            }

            return updatedUrl;
        }

        private static string ToODataV4DateTimeOffsetFormat( string dateTimeString )
        {
            if ( dateTimeString.IsNullOrWhiteSpace() )
            {
                return dateTimeString;
            }

            if ( DateTimeOffset.TryParse( dateTimeString, out DateTimeOffset dateTimeOffset ) )
            {
                return dateTimeOffset.ToString( "o" );
            }

            return dateTimeString;

        }

        private static string ParseMatch( string updatedUrl, Match match, bool parseAsDateTimeOffset = false )
        {
            if ( match.Groups.Count < 2 )
            {
                return updatedUrl;
            }

            var v3Filter = match.Groups[0].Value;
            var originalCapture = match.Groups[1].Value;

            string parsedCapture;

            if ( parseAsDateTimeOffset )
            {
                // ODataV3 would let you have a DateTime filter that wasn't in ISO8601 format
                // but ODataV4 requires it to be in ISO8601 format. 
                parsedCapture = ToODataV4DateTimeOffsetFormat( originalCapture );
            }
            else
            {
                parsedCapture = originalCapture;
            }

            var replace = Uri.EscapeDataString( v3Filter );
            var replaceWith = Uri.EscapeDataString( parsedCapture );


            if ( updatedUrl.Contains( replace ) )
            {
                // if the original is Encoded
                updatedUrl = updatedUrl.Replace( replace, replaceWith );
            }

            if ( updatedUrl.Contains( v3Filter ) )
            {
                // if the original is not Encoded
                updatedUrl = updatedUrl.Replace( v3Filter, parsedCapture );
            }

            return updatedUrl;
        }
    }
}
