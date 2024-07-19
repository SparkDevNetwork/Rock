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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Configuration class for Filters on <seealso cref="RequestFilter"/>
    /// </summary>
    public class PersonalizationRequestFilterConfiguration
    {
        /// <summary>
        /// Gets or sets the previous activity request filter.
        /// </summary>
        /// <value>The previous activity request filter.</value>
        public PreviousActivityRequestFilter PreviousActivityRequestFilter { get; set; } = new PreviousActivityRequestFilter();

        /// <summary>
        /// Gets or sets the device type request filter.
        /// </summary>
        /// <value>The device type request filter.</value>
        public DeviceTypeRequestFilter DeviceTypeRequestFilter { get; set; } = new DeviceTypeRequestFilter();

        /// <summary>
        /// Gets or sets the type of the query string request filter expression.
        /// </summary>
        /// <value>The type of the query string request filter expression.</value>
        public FilterExpressionType QueryStringRequestFilterExpressionType { get; set; } = FilterExpressionType.GroupAny;

        /// <summary>
        /// Gets or sets the query string request filters. These are either AND'd or OR'd depending on <see cref="QueryStringRequestFilterExpressionType"/>.
        /// </summary>
        /// <value>The query string request filters.</value>
        public List<QueryStringRequestFilter> QueryStringRequestFilters { get; set; } = new List<QueryStringRequestFilter>();

        /// <summary>
        /// Gets or sets the type of the cookie request filter expression.
        /// </summary>
        /// <value>The type of the cookie request filter expression.</value>
        public FilterExpressionType CookieRequestFilterExpressionType { get; set; } = FilterExpressionType.GroupAny;

        /// <summary>
        /// Gets or sets the cookie request filters. These are either AND'd or OR'd depending on <see cref="CookieRequestFilterExpressionType"/>.
        /// </summary>
        /// <value>The cookie request filters.</value>
        public List<CookieRequestFilter> CookieRequestFilters { get; set; } = new List<CookieRequestFilter>();

        /// <summary>
        /// Gets or sets the browser request filters. These are always OR'd.
        /// </summary>
        /// <value>The browser request filters.</value>
        public List<BrowserRequestFilter> BrowserRequestFilters { get; set; } = new List<BrowserRequestFilter>();

        /// <summary>
        /// Gets or sets the ip address request filters. These are always OR'd.
        /// </summary>
        /// <value>The ip address request filters.</value>
        public List<IPAddressRequestFilter> IPAddressRequestFilters { get; set; } = new List<IPAddressRequestFilter>();

        /// <summary>
        /// Gets or sets the geolocation request filters. These are always OR'd.
        /// </summary>
        /// <value>The geolocation request filters.</value>
        public List<GeolocationRequestFilter> GeolocationRequestFilters { get; set; } = new List<GeolocationRequestFilter>();

        /// <summary>
        /// Gets or sets the environment request filter.
        /// </summary>
        /// <value>The environment request filter.</value>
        public EnvironmentRequestFilter EnvironmentRequestFilter { get; set; } = new EnvironmentRequestFilter();

    }
}
