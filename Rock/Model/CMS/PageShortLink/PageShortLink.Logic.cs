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
        /// Gets the URL, including any UTM components.
        /// </summary>
        /// <value>
        /// The URL, including query string parameters for any specified UTM fields.
        /// </value>
        public virtual string UrlWithUtm
        {
            get
            {
                Uri uri;

                var isValid = Uri.TryCreate( this.Url, UriKind.Absolute, out uri );
                if ( !isValid )
                {
                    return string.Empty;
                }

                // Add UTM Parameters as lowercase values with URL Encoding.
                var hasUtmValues = false;

                var utmSettings = this.GetAdditionalSettings<UtmSettings>();
                var queryString = uri.Query.ParseQueryString();

                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryString, "utm_source", SystemGuid.DefinedType.UTM_SOURCE.AsGuid(), utmSettings.UtmSourceValueId );
                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryString, "utm_medium", SystemGuid.DefinedType.UTM_MEDIUM.AsGuid(), utmSettings.UtmMediumValueId );
                hasUtmValues = hasUtmValues | AddUtmValueToQueryString( queryString, "utm_campaign", SystemGuid.DefinedType.UTM_CAMPAIGN.AsGuid(), utmSettings.UtmCampaignValueId );

                if ( utmSettings.UtmTerm.IsNotNullOrWhiteSpace() )
                {
                    queryString.Add( "utm_term", utmSettings.UtmTerm.Trim().ToLower().UrlEncode() );
                    hasUtmValues = true;
                }

                if ( utmSettings.UtmContent.IsNotNullOrWhiteSpace() )
                {
                    queryString.Add( "utm_content", utmSettings.UtmContent.Trim().ToLower().UrlEncode() );
                    hasUtmValues = true;
                }

                // If no UTM values, return the base URL.
                if ( !hasUtmValues )
                {
                    return this.Url;
                }

                // Construct a new URL that includes the UTM query string parameters.
                var utmUri = new UriBuilder( uri )
                {
                    Query = queryString.ToQueryString()
                };

                // Return the URL, omitting default port numbers if it is the default.
                return utmUri.Uri.ToString();
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
                .ToLower()
                .UrlEncode();
            if ( !utmValue.IsNullOrWhiteSpace() )
            {
                queryString.Add( parameterName, utmValue );
                return true;
            }

            return false;
        }

        #endregion Virtual Properties
    }
}
