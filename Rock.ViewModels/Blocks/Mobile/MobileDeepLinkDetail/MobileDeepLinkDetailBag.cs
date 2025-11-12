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
