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
using System.Linq;

using Newtonsoft.Json;

using Rock.Common.Mobile;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Web.Cache
{
    internal static class DeepLinkCache
    {
        /// <summary>
        /// The routes cache key.
        /// </summary>
        private const string RoutesCacheKey = "DeepLinkCache:Routes";

        /// <summary>
        /// The apple payload cache key.
        /// </summary>
        private const string ApplePayloadCacheKey = "DeepLinkCache:ApplePayload";

        /// <summary>
        /// The android payload cache key.
        /// </summary>
        private const string AndroidPayloadCacheKey = "DeepLinkCache:AndroidPayload";

        /// <summary>
        /// Gets a list of <see cref="DeepLinkRoute"/>s based on the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>List&lt;DeepLinkRoute&gt;.</returns>
        public static List<DeepLinkRoute> GetDeepLinksForPrefix( string prefix )
        {
            var routeTable = RockCache.GetOrAddExisting( RoutesCacheKey, BuildDeepLinkRoutes ) as Dictionary<string, List<DeepLinkRoute>>;

            if ( routeTable.TryGetValue( prefix.ToLower(), out var routes ) )
            {
                return routes;
            }
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the apple payload.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetApplePayload()
        {
            return RockCache.GetOrAddExisting( ApplePayloadCacheKey, BuildAASA ) as string;
        }

        /// <summary>
        /// Gets the android payload.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetAndroidPayload()
        {
            return RockCache.GetOrAddExisting( AndroidPayloadCacheKey, BuildAssetLinks ) as string;
        }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        public static void Flush()
        {
            RockCache.Remove( RoutesCacheKey );
            RockCache.Remove( ApplePayloadCacheKey );
            RockCache.Remove( AndroidPayloadCacheKey );
        }

        /// <summary>
        /// Builds the deep link routes.
        /// </summary>
        /// <returns>Dictionary&lt;System.String, List&lt;DeepLinkRoute&gt;&gt;.</returns>
        private static Dictionary<string, List<DeepLinkRoute>> BuildDeepLinkRoutes()
        {
            // All of our mobile sites.
            var sites = SiteCache.All()
                .Where( x => x.SiteType == SiteType.Mobile );

            var routes = new Dictionary<string, List<DeepLinkRoute>>();
            foreach ( var site in sites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                // If the additional settings doesn't exist or deep linking is disabled or the prefix is unset OR no routes exist.
                if ( additionalSettings == null
                    || !additionalSettings.IsDeepLinkingEnabled
                    || additionalSettings.DeepLinkPathPrefix.IsNullOrWhiteSpace()
                    || additionalSettings.DeepLinkRoutes == null)
                {
                    continue;
                }

                routes.TryAdd( additionalSettings.DeepLinkPathPrefix, additionalSettings.DeepLinkRoutes );
            }

            return routes;
        }

        /// <summary>
        /// Builds the aasa.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string BuildAASA()
        {
            // Fetching all of our mobile sites.
            var mobileSites = GetDeepLinkSites();


            // In this area, we are mostly working on constructing our data in a way that easily converts into the
            // required format of the AASA file. See: https://gist.github.com/mat/e35393e9dfd9d7fb0972
            var appLinks = new AASAResponse();
            var detailsList = new List<AASADeepLinkDetails>();

            // We're going to loop through each site and do a couple of things.
            // 1. Construct the AppId and Path for each application, if unclear please refer to link above.
            // 2. Add to parent list of details.
            foreach ( var site in mobileSites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                var appDetails = new AASADeepLinkDetails
                {
                    AppId = $"{additionalSettings.TeamIdentifier}.{additionalSettings.BundleIdentifier}",
                    Paths = new List<string> { $"/{additionalSettings.DeepLinkPathPrefix}/*" }
                };

                detailsList.Add( appDetails );
            }

            appLinks.DetailsList = detailsList;

            // Setting the parent key as 'applinks'.
            var aasaResponse = new Dictionary<string, object>
            {
                ["applinks"] = appLinks
            };

            return aasaResponse.ToJson();
        }

        /// <summary>
        /// Gets a list of mobile sites with deep linking enabled.
        /// </summary>
        /// <returns></returns>
        private static List<SiteCache> GetDeepLinkSites()
        {
            return SiteCache.All()
                .Where( x => x.SiteType == SiteType.Mobile )
                .Where( x => x.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>().IsDeepLinkingEnabled == true )
                .ToList();
        }

        /// <summary>
        /// Generates the asset links response.
        /// </summary>
        /// <returns>A JSON string containing the asset links data.</returns>
        /// <seealso href="https://developer.android.com/training/app-links"/>
        private static string BuildAssetLinks()
        {
            // Fetching all of our mobile sites.
            var mobileSites = GetDeepLinkSites();

            // In this area, we are focusing on constructing our data to easily convert to the assetlinks.json file that is
            // required for deep linking. See: https://developer.android.com/training/app-links/verify-site-associations#web-assoc.
            var assetLinks = new List<object>();

            foreach ( var site in mobileSites )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                if ( !additionalSettings.IsDeepLinkingEnabled )
                {
                    continue;
                }

                // Building the asset links from our POCOs.
                var appDetails = new AssetLinksTargetDetails
                {
                    PackageName = additionalSettings.PackageName,
                    CertificateFingerprints = new string[] { additionalSettings.CertificateFingerprint }
                };

                var linkData = new Dictionary<string, object>
                {
                    ["relation"] = new List<string> { "delegate_permission/common.handle_all_urls" },
                    ["target"] = appDetails
                };

                assetLinks.Add( linkData );
            }

            return assetLinks.ToJson();
        }

        /// <summary>
        /// POCO for the entire AASA response information.
        /// </summary>
        private class AASAResponse
        {
            /// <summary>
            /// Gets or sets the apps identifier, in our use case, we leave this list empty. 
            /// </summary>
            /// <value>
            /// The apps.
            /// </value>
            [JsonProperty( "apps" )]
            public List<string> Apps { get; set; } = new List<string>();

            /// <summary>
            /// Gets or sets the deep link details list.
            /// </summary>
            /// <value>
            /// The deep link details list.
            /// </value>
            [JsonProperty( "details" )]
            public List<AASADeepLinkDetails> DetailsList { get; set; }
        }

        /// <summary>
        /// POCO for the deep link details information. 
        /// </summary>
        private class AASADeepLinkDetails
        {
            /// <summary>
            /// Gets or sets the application identifier.
            /// </summary>
            /// <value>
            /// The application identifier.
            /// </value>
            [JsonProperty( "appID" )]
            public string AppId { get; set; }

            /// <summary>
            /// Gets or sets the paths.
            /// </summary>
            /// <value>
            /// The paths.
            /// </value>
            [JsonProperty( "paths" )]
            public List<string> Paths { get; set; }
        }

        /// <summary>
        /// POCO for the deep link details information.
        /// </summary>
        private class AssetLinksTargetDetails
        {
            /// <summary>
            /// Gets or sets the namespace.
            /// </summary>
            /// <value>
            /// The namespace.
            /// </value>
            [JsonProperty( "namespace" )]
            public string Namespace { get; set; } = "android_app";

            /// <summary>
            /// Gets or sets the name of the package.
            /// </summary>
            /// <value>
            /// The name of the package.
            /// </value>
            [JsonProperty( "package_name" )]
            public string PackageName { get; set; }

            /// <summary>
            /// Gets or sets the certificate fingerprint.
            /// </summary>
            /// <value>
            /// The certificate fingerprint.
            /// </value>
            [JsonProperty( "sha256_cert_fingerprints" )]
            public string[] CertificateFingerprints { get; set; }
        }
    }
}