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
using System.Linq;
using System.Net.Http;
using System.Text;
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
            if ( RequestUriHasObsoleteV3Syntax( queryOptions ) )
            {
                return ConvertOData3SyntaxToODataV4( queryOptions );
            }
            else
            {
                return queryOptions;
            }
        }

        /// <summary>
        /// return false, if it detects if the SelectExpand would throw an exception,
        /// probably due to unsupported v3 syntax.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        private bool SelectExpandIsValid( ODataQueryOptions queryOptions )
        {
            try
            {
                // Check if it is valid by seeing if it throws an exception.
                // There doesn't appear to something like an "bool IsValid", so
                // we'll just have to check for an exception
                var checkExpandClause = queryOptions?.SelectExpand?.SelectExpandClause;
            }
            catch ( Microsoft.Data.OData.ODataException )
            {
                return false;
            }
            catch ( Microsoft.OData.ODataException )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if the Request has OData V3 Syntax that were made obsolete in V4
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        private bool RequestUriHasObsoleteV3Syntax( ODataQueryOptions queryOptions )
        {
            var rawFilter = queryOptions?.Filter?.RawValue ?? string.Empty;
            var selectExpandRawExpand = queryOptions?.SelectExpand?.RawExpand ?? string.Empty;
            var selectExpandRawSelect = queryOptions?.SelectExpand?.RawSelect ?? string.Empty;

            if ( selectExpandRawSelect.IsNotNullOrWhiteSpace() )
            {
                if ( !SelectExpandIsValid( queryOptions ) )
                {
                    return true;
                }
            }

            if ( rawFilter.IsNullOrWhiteSpace() && selectExpandRawExpand.IsNullOrWhiteSpace() && selectExpandRawSelect.IsNullOrWhiteSpace() )
            {
                return false;
            }

            bool isV3FilterSyntax;
            if ( rawFilter.IsNotNullOrWhiteSpace() )
            {
                var isV3DateTimeFilter = _dateTimeFilterCapture.IsMatch( rawFilter );
                var isV3GuidFilter = _guidFilterCapture.IsMatch( rawFilter );
                isV3FilterSyntax = isV3DateTimeFilter || isV3GuidFilter;
            }
            else
            {
                isV3FilterSyntax = false;
            }

            bool isV3SelectExpandSyntax;

            if ( selectExpandRawExpand.Contains( "/" ) || selectExpandRawSelect.Contains( "/" ) )
            {
                isV3SelectExpandSyntax = true;
            }
            else
            {
                isV3SelectExpandSyntax = false;
            }

            return isV3FilterSyntax || isV3SelectExpandSyntax;
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
        private ODataQueryOptions ConvertOData3SyntaxToODataV4( ODataQueryOptions queryOptions )
        {
            var originalUrl = queryOptions.Request.RequestUri.OriginalString;
            var rawFilter = queryOptions?.Filter?.RawValue;
            var selectExpandRawExpand = queryOptions?.SelectExpand?.RawExpand;
            var selectExpandRawSelect = queryOptions?.SelectExpand?.RawSelect;

            // Fix up any $filter unsupported v3 syntax to be in v4 format
            string updatedUrl = ParseRawFilterFromOriginalUrl( originalUrl, rawFilter );

            // Fix up any $expand unsupported v3 syntax to be in v4 format
            updatedUrl = ParseExpandClauseFromOriginalUrl( updatedUrl, selectExpandRawExpand );

            var convertedODataQueryOptions = CreateConvertedODataQueryOptions( queryOptions, updatedUrl );

            // Now that we got the $expand in v4 syntax, lets take our convertedODataQueryOptions and check if the remaining $select is OK 
            bool selectSyntaxThrowsException = selectExpandRawSelect.IsNotNullOrWhiteSpace() && !SelectExpandIsValid( convertedODataQueryOptions );
            if ( selectSyntaxThrowsException )
            {
                // if the $select has unsupported v3 syntax, remove the entire $select clause.
                updatedUrl = ParseSelectClauseFromOriginalUrl( selectSyntaxThrowsException, convertedODataQueryOptions, updatedUrl );
                convertedODataQueryOptions = CreateConvertedODataQueryOptions( convertedODataQueryOptions, updatedUrl );
            }

            return convertedODataQueryOptions;
        }

        private static ODataQueryOptions CreateConvertedODataQueryOptions( ODataQueryOptions queryOptions, string updatedUrl )
        {
            var convertedRequest = new HttpRequestMessage( queryOptions.Request.Method, updatedUrl );
            foreach ( var origProperty in queryOptions.Request.Properties )
            {
                convertedRequest.Properties.Add( origProperty.Key, origProperty.Value );
            }

            foreach ( var origProperty in queryOptions.Request.Headers )
            {
                convertedRequest.Headers.Add( origProperty.Key, origProperty.Value );
            }

            return new ODataQueryOptions( queryOptions.Context, convertedRequest );
        }

        private static string ParseSelectClauseFromOriginalUrl( bool selectSyntaxThrowsException, ODataQueryOptions queryOptions, string originalUrl )
        {
            if ( !selectSyntaxThrowsException )
            {
                return originalUrl;
            }

            /* 10-21-2022 MDP
              
             OData v3 'nested' $select work quite a bit different in ODataV4. OData v3 had something you
             might call the "slash-slash" syntax. OData now wants the nested select to be nested in the
             $expand clause (which kinda makes more sense). It's possible that a later version of
             the Microsoft.AspNet.OData packages will support "slash-slash" syntax again, so we will
             only remove invalid $select clauses if they raise an exception.

             Examples
               - Not nested, these will work in both OData V3 and V4
                 - ~api\GroupMember?$select=Id,PersonId 
                 - ~api\People?$select=Id,FirstName,LastName
              - Still Not nested even it does include a navigation property, these will work in both OData V3 and V4.
                 - ~api\GroupMember?$expand=Person&$select=Id,PersonId,Person 
                 - ~api\People?$expand=PhoneNumbers&$select=Id,FirstName,LastName,PhoneNumbers

              - Nested, these will work in v3 but not v4
                 - ~api\GroupMember?$expand=Members/Person$select=Id,Members/Person/LastName
                 - ~api\People?$expand=PhoneNumbers&$select=Id,FirstName,LastName,PhoneNumbers
             
             */

            // Remove the $select clause from the original URL if it is causing an exception
            // this will result in return the full object data instead of just the fields
            // that were specified, but at least they will get what they need.
            // If they need a nested $select, they can use the odata v4 syntax
            var rawSelect = queryOptions?.SelectExpand?.RawSelect;
            string replaceUnencoded = rawSelect;
            string replaceEncoded = Uri.EscapeDataString( rawSelect );
            string replaceWith = "";
            var updatedUrl = originalUrl.Replace( replaceUnencoded, replaceWith );
            updatedUrl = updatedUrl.Replace( replaceEncoded, replaceWith );

            return updatedUrl;
        }

        internal static string ParseExpandClauseFromOriginalUrl( string originalUrl, string rawSelectExpand )
        {
            if ( rawSelectExpand.IsNullOrWhiteSpace() || !rawSelectExpand.Contains( '/' ) )
            {
                return originalUrl;
            }

            var originalRawSelectExpand = rawSelectExpand;
            var updatedRawSelectExpand = rawSelectExpand;

            var expandSections = rawSelectExpand.Split( ',' );
            foreach ( var expandSection in expandSections )
            {
                var trailingParentheses = new List<string>();
                var selectExpandParts = expandSection.Split( '/' );
                if ( selectExpandParts.Length < 2 )
                {
                    continue;
                }

                var originalRawSelectExpandSection = expandSection;
                var updatedRawSelectExpandBuilder = new StringBuilder();
                updatedRawSelectExpandBuilder.Append( selectExpandParts[0] );
                var remainingParts = selectExpandParts.Skip( 1 ).ToArray();
                foreach ( var part in remainingParts )
                {
                    updatedRawSelectExpandBuilder.Append( $"($expand={part}" );
                    trailingParentheses.Add( ")" );
                }

                var updatedRawSelectExpandSection = updatedRawSelectExpandBuilder.ToString() + trailingParentheses.AsDelimited( "" );
                updatedRawSelectExpand = updatedRawSelectExpand.Replace( originalRawSelectExpandSection, updatedRawSelectExpandSection );
            }

            var updatedUrl = originalUrl;

            // if the original is Encoded
            updatedUrl = updatedUrl.Replace( Uri.EscapeDataString( originalRawSelectExpand ), Uri.EscapeDataString( updatedRawSelectExpand ) );

            // if the original is Not Encoded
            updatedUrl = updatedUrl.Replace( originalRawSelectExpand, updatedRawSelectExpand );

            return updatedUrl;
        }

        internal static string ParseRawFilterFromOriginalUrl( string originalUrl, string rawFilter )
        {
            if ( rawFilter.IsNullOrWhiteSpace() )
            {
                return originalUrl;
            }

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
                updatedUrl = ParseFilterMatch( updatedUrl, match, true );
            }

            foreach ( Match match in guidMatches )
            {
                updatedUrl = ParseFilterMatch( updatedUrl, match );
            }

            return updatedUrl;
        }

        // Derived from https://github.com/OData/odata.net/blob/7dcad74478debcfe54e93c63f47b7cb57f2d2e67/src/PlatformHelper.cs#L325-L346
        private static string ToODataV4DateTimeOffsetFormat( string dateTimeText )
        {
            // The XML DateTime pattern is described here: http://www.w3.org/TR/xmlschema-2/#dateTime
            // If timezone is specified, the indicator will always be at the same place from the end of the string, so we can look there for the Z or +/-.
            //
            // UTC timezone, for example: "2012-12-21T15:01:23.1234567Z"
            if ( dateTimeText.Length > 1 && ( dateTimeText[dateTimeText.Length - 1] == 'Z' || dateTimeText[dateTimeText.Length - 1] == 'z' ) )
            {
                return dateTimeText;
            }

            // Timezone offset from UTC, for example: "2012-12-21T15:01:23.1234567-08:00" or "2012-12-21T15:01:23.1234567+08:00"
            const int timeZoneSignOffset = 6;
            if ( dateTimeText.Length > timeZoneSignOffset && ( dateTimeText[dateTimeText.Length - timeZoneSignOffset] == '-' || dateTimeText[dateTimeText.Length - timeZoneSignOffset] == '+' ) )
            {
                return dateTimeText;
            }

            // No timezone specified, for example: "2012-12-21T15:01:23.1234567", so add the RockDateTime timezone offset
            var baseUtcOffset = RockDateTime.OrgTimeZoneInfo.BaseUtcOffset;
            string offsetSuffix;
            if ( baseUtcOffset.Hours >= 0 )
            {
                // if there is a positive offset, prefix with '+'
                offsetSuffix = $"+{baseUtcOffset.Hours:00}:{baseUtcOffset.Minutes:00}";
            }
            else
            {
                // if there is a negative offset, the '-' will be included already
                offsetSuffix = $"{baseUtcOffset.Hours:00}:{baseUtcOffset.Minutes:00}";
            }

            return dateTimeText + offsetSuffix;
        }

        private static string ParseFilterMatch( string updatedUrl, Match match, bool parseAsDateTimeOffset = false )
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

            var replaceEncoded = Uri.EscapeDataString( v3Filter );
            var replaceWithEncoded = Uri.EscapeDataString( parsedCapture );

            if ( updatedUrl.Contains( replaceEncoded ) )
            {
                // if the original is Encoded
                updatedUrl = updatedUrl.Replace( replaceEncoded, replaceWithEncoded );
            }

            var replaceNotEncoded = v3Filter;
            var replaceWithNotEncoded = parsedCapture;

            if ( updatedUrl.Contains( replaceNotEncoded ) )
            {
                // if the original is not Encoded
                updatedUrl = updatedUrl.Replace( replaceNotEncoded, replaceWithNotEncoded );
            }

            return updatedUrl;
        }
    }
}