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
using System.Linq;
using System.Web;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RequestFilter
    {
        /// <summary>
        /// Requests the meets criteria.
        /// </summary>
        /// <param name="requestFilterId">The request filter identifier.</param>
        /// <param name="request">The request.</param>
        /// <param name="site">The site.</param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.
        /// </returns>
        public static bool RequestMeetsCriteria( int requestFilterId, HttpRequest request, SiteCache site )
        {
            var requestFilter = RequestFilterCache.Get( requestFilterId );
            var requestFilterConfiguration = requestFilter?.FilterConfiguration;
            if ( requestFilter == null || requestFilterConfiguration == null )
            {
                // somehow null so return false
                return false;
            }

            if ( requestFilter.SiteId.HasValue && site != null )
            {
                if ( requestFilter.SiteId.Value != site.Id )
                {
                    // this request filter is limited to a site other than the one we are on.
                    return false;
                }
            }

            /* All of these are AND'd so, if any are false we can return false */

            // Check against Previous Activity
            var previousActivityFilter = requestFilterConfiguration.PreviousActivityRequestFilter;
            if ( !previousActivityFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Device Type
            var deviceTypeFilter = requestFilterConfiguration.DeviceTypeRequestFilter;
            if ( !deviceTypeFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against Query String
            if ( requestFilterConfiguration.QueryStringRequestFilters.Any() )
            {
                bool queryStringRequestFiltersMatch;
                if ( requestFilterConfiguration.QueryStringRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    queryStringRequestFiltersMatch = requestFilterConfiguration.QueryStringRequestFilters.All( a => a.IsMatch( request ) );
                }
                else
                {
                    queryStringRequestFiltersMatch = requestFilterConfiguration.QueryStringRequestFilters.Any( a => a.IsMatch( request ) );
                }

                if ( !queryStringRequestFiltersMatch )
                {
                    return false;
                }
            }

            // Check against Cookie values
            if ( requestFilterConfiguration.CookieRequestFilters.Any() )
            {
                bool cookieFiltersMatch;
                if ( requestFilterConfiguration.CookieRequestFilterExpressionType == FilterExpressionType.GroupAll )
                {
                    cookieFiltersMatch = requestFilterConfiguration.CookieRequestFilters.All( a => a.IsMatch( request ) );
                }
                else
                {
                    cookieFiltersMatch = requestFilterConfiguration.CookieRequestFilters.Any( a => a.IsMatch( request ) );
                }

                if ( !cookieFiltersMatch )
                {
                    return false;
                }
            }

            // Check against Browser type and version
            bool browserFiltersMatch = true;
            foreach ( var browserRequestFilter in requestFilterConfiguration.BrowserRequestFilters )
            {
                var isMatch = browserRequestFilter.IsMatch( request );
                browserFiltersMatch = browserFiltersMatch || isMatch;
            }

            if ( !browserFiltersMatch )
            {
                return false;
            }

            // Check based on IPAddress Range
            bool ipAddressFiltersMatch = true;
            foreach ( var ipAddressRequestFilter in requestFilterConfiguration.IPAddressRequestFilters )
            {
                var isMatch = ipAddressRequestFilter.IsMatch( request );
                ipAddressFiltersMatch = ipAddressFiltersMatch || isMatch;
            }

            if ( !ipAddressFiltersMatch )
            {
                return false;
            }


            // Check against Environment
            var environmentRequestFilter = requestFilterConfiguration.EnvironmentRequestFilter;
            if ( !environmentRequestFilter.IsMatch( request ) )
            {
                return false;
            }

            // Check against geolocation
            bool geolocationFiltersMatch = false;
            foreach ( var geolocationRequestFilter in requestFilterConfiguration.GeolocationRequestFilters )
            {
                var isMatch = geolocationRequestFilter.IsMatch( request );
                geolocationFiltersMatch = geolocationFiltersMatch || isMatch;
            }

            if ( requestFilterConfiguration.GeolocationRequestFilters.Any() && !geolocationFiltersMatch )
            {
                return false;
            }

            // if none of the filters return false, then return true;
            return true;
        }
    }
}
