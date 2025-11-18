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
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModels.Rest.Controls;

namespace Rock.ViewModels.Blocks.Mobile.MobileDeepLinkDetail
{

    /// <summary>
    /// Represents a mobile page with its unique identifier and name.
    /// </summary>
    public class MobileDeepLinkDetailBag
    {
        /// <summary>
        /// Gets or sets the identifier of the site.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the deep link route.
        /// </summary>
        public Guid RouteGuid { get; set; }

        /// <summary>
        /// Gets or sets the path prefix for the site's deep links.
        /// </summary>
        public string PathPrefix { get; set; }

        /// <summary>
        /// Gets or sets the route for the deep link.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the mobile page.
        /// </summary>
        public PageRouteValueBag MobilePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the URL is used as a fallback.
        /// </summary>
        public bool UsesUrlAsFallback { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the web fallback page.
        /// </summary>
        public PageRouteValueBag WebFallbackPage { get; set; }

        /// <summary>
        /// Gets or sets the URL of the web fallback page.
        /// </summary>
        public string WebFallbackPageUrl { get; set; }
    }
}
