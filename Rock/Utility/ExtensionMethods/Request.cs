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
using System.Web;

namespace Rock
{
    /// <summary>
    ///
    /// </summary>
    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. The client's original address will be in the "X-Forwarded-For" header. This method will check
        /// if the request is proxied. If so it will return the original source URI, otherwise if it's not proxied it will
        /// return the Request.Uri.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Uri UrlProxySafe( this HttpRequest request )
        {
            // Some proxies use X-Original-Host.
            // JME 2/22/2024
            // Added a custom header for forward hosts that is used by a language translation service called Weglot. Normally, we would not
            // add custom headers into Rock, but many organizations in the community use this service. We did reach out to them to see if they
            // would change to use the industry standard and were told no. Specifically the issue was when you use Weglot with Auth0. 
            var forwardedHost = request.Headers["X-Forwarded-Host"] ?? request.Headers["X-Original-Host"] ?? request.Headers["weglot-forwarded-host"];

            // If no proxy just return the request URL
            var isRequestForwardedFromProxy = forwardedHost != null && request.Headers["X-Forwarded-Proto"] != null;

            if ( !isRequestForwardedFromProxy )
            {
                return request.Url;
            }

            // Assemble a URI from the proxied headers
            var builder = new UriBuilder( request.Url )
            {
                Scheme = request.Headers["X-Forwarded-Proto"].ToString(),
                Host = forwardedHost,
            };

            // If we have the original port then use it, otherwise reset to default port.
            if ( request.Headers["X-Forwarded-Port"] != null )
            {
                builder.Port = request.Headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1;
            }
            else
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. The client's original address will be in the "X-Forwarded-For" header. This method will check
        /// if the request is proxied. If so it will return the original source URI, otherwise if it's not proxied it will
        /// return the Request.Uri.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Uri UrlProxySafe( this HttpRequestBase request )
        {
            // Some proxies use X-Original-Host.
            // JME 2/22/2024
            // Added a custom header for forward hosts that is used by a language translation service called Weglot. Normally, we would not
            // add custom headers into Rock, but many organizations in the community use this service. We did reach out to them to see if they
            // would change to use the industry standard and were told no. Specifically the issue was when you use Weglot with Auth0. 
            var forwardedHost = request.Headers["X-Forwarded-Host"] ?? request.Headers["X-Original-Host"] ?? request.Headers["weglot-forwarded-host"];

            // If no proxy just return the request URL
            var isRequestForwaredFromProxy = forwardedHost != null && request.Headers["X-Forwarded-Proto"] != null;

            if ( !isRequestForwaredFromProxy )
            {
                return request.Url;
            }

            // Assemble a URI from the proxied headers
            var builder = new UriBuilder( request.Url )
            {
                Scheme = request.Headers["X-Forwarded-Proto"].ToString(),
                Host = forwardedHost,
            };

            // If we have the original port then use it, otherwise reset to default port.
            if ( request.Headers["X-Forwarded-Port"] != null )
            {
                builder.Port = request.Headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1;
            }
            else
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. The client's original address will be in the "X-Forwarded-For" header. This method will check
        /// if the request is proxied. If so it will return the original source URI, otherwise if it's not proxied it will
        /// return the Request.Uri.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal static Uri UrlProxySafe( this Net.IRequest request )
        {
            // Some proxies use X-Original-Host.
            // JME 2/22/2024
            // Added a custom header for forward hosts that is used by a language translation service called Weglot. Normally, we would not
            // add custom headers into Rock, but many organizations in the community use this service. We did reach out to them to see if they
            // would change to use the industry standard and were told no. Specifically the issue was when you use Weglot with Auth0. 
            var forwardedHost = request.Headers["X-Forwarded-Host"] ?? request.Headers["X-Original-Host"] ?? request.Headers["weglot-forwarded-host"];

            // If no proxy just return the request URL
            var isRequestForwaredFromProxy = forwardedHost != null && request.Headers["X-Forwarded-Proto"] != null;

            if ( !isRequestForwaredFromProxy )
            {
                return request.RequestUri;
            }

            // Assemble a URI from the proxied headers
            var builder = new UriBuilder( request.RequestUri )
            {
                Scheme = request.Headers["X-Forwarded-Proto"].ToString(),
                Host = forwardedHost
            };

            // If we have the original port then use it, otherwise reset to default port.
            if ( request.Headers["X-Forwarded-Port"] != null )
            {
                builder.Port = request.Headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1;
            }
            else
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        /// <summary>
        /// Returns a common (friendly name) for the URL of the Referrer. This is used for analytics
        /// (e.g. how many refers came from Google). If a friendly name can not be determined then
        /// the host name will be returned.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string UrlReferrerNormalize( this HttpRequest request )
        {
            // Consider making this a defined value someday

            if ( request.UrlReferrer == null )
            {
                return null;
            }

            switch ( request.UrlReferrer.Host )
            {
                case string s when s.Contains( "google.com" ):
                    return "Google";
                case string s when s.Contains( "bing.com" ):
                    return "Bing";
                case string s when s.Contains( "facebook.com" ):
                    return "Facebook";
                case string s when s.Contains( "twitter.com" ):
                    return "Twitter";
                case string s when s.Contains( "linkedin.com" ):
                    return "LinkedIn";
                case string s when s.Contains( "instagram.com" ):
                    return "Instagram";
                case string s when s.Contains( "pinterest.com" ):
                    return "Pinterest";
                case string s when s.Contains( "duckduckgo.com" ):
                    return "DuckDuckGo";
                case string s when s.Contains( "reddit.com" ):
                    return "Reddit";
            }

            // If it wasn't a common site then return the URL host
            return request.UrlReferrer.Host;
        }

        /// <summary>
        /// Returns the search terms from a referral site
        /// e.g. https:www.google.com?q=test would return test
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string UrlReferrerSearchTerms( this HttpRequest request )
        {
            if ( request.UrlReferrer == null )
            {
                return null;
            }

            // We want to return a friendly string of the query string parm "q" which most search engines use for the individuals query
            var parms = HttpUtility.ParseQueryString( request.UrlReferrer.AbsoluteUri );
            var searchQuery = parms["q"];

            if ( searchQuery != null )
            {
                return null;
            }

            return HttpUtility.UrlDecode( searchQuery );
        }
    }
}