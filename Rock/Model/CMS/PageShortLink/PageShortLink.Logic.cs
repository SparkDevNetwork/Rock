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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PageShortLink
    {
        private static Random _random = new Random( Guid.NewGuid().GetHashCode() );
        private static char[] alphaCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        /// Gets a random token.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetRandomToken( int length )
        {
            StringBuilder sb = new StringBuilder();
            int poolSize = alphaCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( alphaCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }

        #region Virtual Properties

        /// <summary>
        /// Gets the URL, including query parameters for UTM values.
        /// </summary>
        /// <value>
        /// The URL, including query string parameters for any specified UTM fields.
        /// </value>
        /// <remarks>
        /// The Url produced by this is not guaranteed to be well-formed.
        /// </remarks>
        public virtual string UrlWithUtm
        {
            get
            {
                if ( this.Url.IsNullOrWhiteSpace() )
                {
                    return string.Empty;
                }

                string protocolDomainAndPath;
                string fragment;
                NameValueCollection queryParameters;

                ParseUrl( this.Url.RemoveCrLf().Trim(),
                    out protocolDomainAndPath,
                    out queryParameters,
                    out fragment );

                // Add UTM Parameters as lowercase values with URL Encoding.
                var hasUtmValues = false;

                var utmSettings = this.GetAdditionalSettings<UtmSettings>();

                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryParameters, "utm_source", SystemGuid.DefinedType.UTM_SOURCE.AsGuid(), utmSettings.UtmSourceValueId );
                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryParameters, "utm_medium", SystemGuid.DefinedType.UTM_MEDIUM.AsGuid(), utmSettings.UtmMediumValueId );
                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryParameters, "utm_campaign", SystemGuid.DefinedType.UTM_CAMPAIGN.AsGuid(), utmSettings.UtmCampaignValueId );

                if ( utmSettings.UtmTerm.IsNotNullOrWhiteSpace() )
                {
                    queryParameters.Add( "utm_term", utmSettings.UtmTerm.Trim().ToLower() );
                    hasUtmValues = true;
                }

                if ( utmSettings.UtmContent.IsNotNullOrWhiteSpace() )
                {
                    queryParameters.Add( "utm_content", utmSettings.UtmContent.Trim().ToLower() );
                    hasUtmValues = true;
                }

                // If no UTM values, return the base URL.
                if ( !hasUtmValues )
                {
                    return this.Url;
                }

                // Construct a new URL that includes the UTM query string parameters.
                // The query parameters are stored in a HttpValueCollection, so the ToString() implementation returns a URL-encoded query string.
                var urlWithUtm = protocolDomainAndPath + "?" + queryParameters.ToQueryStringEscaped();

                if ( fragment.IsNotNullOrWhiteSpace() )
                {
                    urlWithUtm += fragment;
                }

                return urlWithUtm;
            }
        }

        #endregion Virtual Properties

        /// <summary>
        /// Extract a set of query parameters from a Url.
        /// If the input Url is not well-formed, this function attempts to parse for a query string
        /// delimited by the first "?" character.
        /// </summary>
        /// <param name="url">The input Url.</param>
        /// <param name="protocolDomainAndPath"></param>
        /// <param name="queryParameters">The query parameters as a HttpValueCollection:NameValueCollection of Name/Value pairs.</param>
        /// <param name="fragment">The fragment portion of the Url, including the leading '#' character if not empty.</param>
        /// <returns></returns>
        private void ParseUrl( string url, out string protocolDomainAndPath, out NameValueCollection queryParameters, out string fragment )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                protocolDomainAndPath = string.Empty;
                queryParameters = new NameValueCollection();
                fragment = string.Empty;
                return;
            }

            // Create a builder for the Uri and attempt to parse the existing Url.
            UriBuilder utmUri = null;
            try
            {
                utmUri = new UriBuilder( url );
            }
            catch
            {
                // The Url is not well-formed.
            }

            if ( utmUri != null )
            { 
                fragment = utmUri.Fragment;
                queryParameters = System.Web.HttpUtility.ParseQueryString( utmUri.Query );

                utmUri.Query = string.Empty;
                utmUri.Fragment = string.Empty;

                protocolDomainAndPath = utmUri.Uri.ToString();
            }
            else
            {
                // The Url is not well-formed, so parse the query string and fragment segments, and leave the remainder unchanged.
                // The parsing process assumes that although the Url has some incorrect parts, it is otherwise properly encoded.

                // Remove the fragment portion of the Url if it exists.
                var fragmentStartIndex = url.IndexOf( "#" );
                if ( fragmentStartIndex >= 0 )
                {
                    fragment = url.Substring( fragmentStartIndex );
                    url = url.Substring( 0, fragmentStartIndex );
                }
                else
                {
                    fragment = string.Empty;
                }

                // Parse the query string and base path.
                string queryString;
                var queryStartIndex = url.IndexOf( "?" ) + 1;
                if ( queryStartIndex > 0 )
                {
                    protocolDomainAndPath = url.Substring( 0, queryStartIndex - 1 );
                    queryString = url.Substring( queryStartIndex );
                }
                else
                {
                    protocolDomainAndPath = url;
                    queryString = string.Empty;
                }

                queryParameters = System.Web.HttpUtility.ParseQueryString( queryString );
            }
        }

        private bool AddUtmValueToQueryString( NameValueCollection queryString, string parameterName, Guid definedTypeGuid, int? utmDefinedValueId )
        {
            if ( !utmDefinedValueId.HasValue )
            {
                return false;
            }

            var utmValue = DefinedTypeCache.Get( definedTypeGuid )
                ?.DefinedValues.FirstOrDefault( v => v.Id == utmDefinedValueId )
                ?.Value
                .Trim()
                .ToLower();
            if ( !utmValue.IsNullOrWhiteSpace() )
            {
                queryString.Set( parameterName, utmValue );
                return true;
            }

            return false;
        }
    }
}
