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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.SiteDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class SiteBag : EntityBagBase
    {
        /// <summary>
        /// The Allowed Frame Domains designates which external domains/sites are allowed to embed iframes of this site.
        /// It controls what is put into the Content-Security-Policy HTTP response header.
        /// This is in accordance with the Content Security Policy described here http://w3c.github.io/webappsec-csp/#csp-header
        /// and here https://www.owasp.org/index.php/Content_Security_Policy_Cheat_Sheet
        /// </summary>
        public string AllowedFrameDomains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        public bool AllowIndexing { get; set; }

        /// <summary>
        /// Gets or sets the change password page.
        /// </summary>
        public ListItemBag ChangePasswordPage { get; set; }

        /// <summary>
        /// Gets or sets the change password page route.
        /// </summary>
        public ListItemBag ChangePasswordPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the communication page.
        /// </summary>
        public ListItemBag CommunicationPage { get; set; }

        /// <summary>
        /// Gets or sets the communication page route.
        /// </summary>
        public ListItemBag CommunicationPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the default Rock.Model.Page page for the site.
        /// </summary>
        public ListItemBag DefaultPage { get; set; }

        /// <summary>
        /// Gets or sets the default Rock.Model.PageRoute page route for this site. If this value is null, the DefaultPage will be used
        /// </summary>
        public ListItemBag DefaultPageRoute { get; set; }

        /// <summary>
        /// Gets or sets a user defined description/summary  of the Site.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether predictable Ids are disabled.
        /// </summary>
        public bool DisablePredictableIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this site should be available to be used for shortlinks (the shortlink can still reference the URL of other sites).
        /// </summary>
        public bool EnabledForShortening { get; set; }

        /// <summary>
        /// Enabling this feature will prevent other sites from using this sites routes and prevent routes from other sites from working on this site.
        /// </summary>
        public bool EnableExclusiveRoutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable mobile redirect].
        /// </summary>
        public bool EnableMobileRedirect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether geo-location lookups should be performed on interactions.
        /// </summary>
        public bool EnablePageViewGeoTracking { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log Page Views into the Interaction tables for pages in this site
        /// </summary>
        public bool EnablePageViews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable personalization].
        /// </summary>
        public bool EnablePersonalization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether /[enable visitor tracking].
        /// </summary>
        public bool EnableVisitorTracking { get; set; }

        /// <summary>
        /// Gets or sets the path to the error page.
        /// </summary>
        public string ErrorPage { get; set; }

        /// <summary>
        /// Gets or sets the external URL.
        /// </summary>
        public string ExternalUrl { get; set; }

        /// <summary>
        /// Gets or sets the favicon binary file.
        /// </summary>
        public ListItemBag FavIconBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the Google analytics code.
        /// </summary>
        public string GoogleAnalyticsCode { get; set; }

        /// <summary>
        /// Gets or sets the index starting location.
        /// </summary>
        public string IndexStartingLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the login Rock.Model.Page page for the site.
        /// </summary>
        public ListItemBag LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the login Rock.Model.PageRoute page route for this site. If this value is null, the LoginPage will be used
        /// </summary>
        public ListItemBag LoginPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the mobile page.
        /// </summary>
        public ListItemBag MobilePage { get; set; }

        /// <summary>
        /// Gets or sets the name of the Site. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content of the page header.
        /// </summary>
        public string PageHeaderContent { get; set; }

        /// <summary>
        /// Gets or sets the 404 Rock.Model.Page page for the site.
        /// </summary>
        public ListItemBag PageNotFoundPage { get; set; }

        /// <summary>
        /// Gets or sets the 404 Rock.Model.PageRoute page route for this site.
        /// </summary>
        public ListItemBag PageNotFoundPageRoute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [redirect tablets].
        /// </summary>
        public bool RedirectTablets { get; set; }

        /// <summary>
        /// Gets or sets the registration Rock.Model.Page page for the site.
        /// </summary>
        public ListItemBag RegistrationPage { get; set; }

        /// <summary>
        /// Gets or sets the registration Rock.Model.PageRoute page route for this site. If this value is null, the RegistrationPage will be used
        /// </summary>
        public ListItemBag RegistrationPageRoute { get; set; }

        /// <summary>
        /// Gets or sets the number of days to keep page views logged.
        /// </summary>
        /// <value>
        /// The duration of the retention.
        /// </value>
        public int? RetentionDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires encryption].
        /// </summary>
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets the site logo binary file.
        /// </summary>
        public ListItemBag SiteLogoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the site domains.
        /// </summary>
        /// <value>
        /// The site domains.
        /// </value>
        public string SiteDomains { get; set; }

        /// <summary>
        /// Gets or sets the name of the Theme that is used on the Site.
        /// </summary>
        public string Theme { get; set; }
    }
}
