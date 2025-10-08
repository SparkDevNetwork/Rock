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

namespace Rock.ViewModels.Blocks.Cms.RequestFilterDetail
{
    /// <summary>
    /// Bag that represents Additional Filter Configuration for a Request Filter.
    /// </summary>
    public class AdditionalFilterConfigurationBag
    {
        /// <summary>
        /// Gets or sets the previous activity request filter configuration.
        /// </summary>
        public PreviousActivityRequestFilterBag PreviousActivityRequestFilter { get; set; }

        /// <summary>
        /// Gets or sets the device type request filter configuration.
        /// </summary>
        public DeviceTypeRequestFilterBag DeviceTypeRequestFilter { get; set; }

        /// <summary>
        /// Gets or sets the query string filter expression type (Reporting.FilterExpressionType values).
        /// </summary>
        public int QueryStringRequestFilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the query string request filters.
        /// </summary>
        public List<QueryStringRequestFilterBag> QueryStringRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the cookie filter expression type (Reporting.FilterExpressionType values).
        /// </summary>
        public int CookieRequestFilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the cookie request filters.
        /// </summary>
        public List<CookieRequestFilterBag> CookieRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the browser request filters.
        /// </summary>
        public List<BrowserRequestFilterBag> BrowserRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the IP address request filters.
        /// </summary>
        public List<IPAddressRequestFilterBag> IPAddressRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the geolocation request filters.
        /// </summary>
        public List<GeolocationRequestFilterBag> GeolocationRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the environment request filter configuration.
        /// </summary>
        public EnvironmentRequestFilterBag EnvironmentRequestFilter { get; set; }
    }
}
