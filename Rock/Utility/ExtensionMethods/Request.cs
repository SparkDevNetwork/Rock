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
using System.Web;

using Rock.Web.Cache;

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
        /// proxied address. A host name, whether it came from a forwarded header or from the request itself, is used
        /// only when it is a configured site domain (or a loopback name). Otherwise the scheme, host and port come
        /// from the PublicApplicationRoot global attribute and the path and query of the request are kept.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Uri UrlProxySafe( this HttpRequest request )
        {
            return GetProxySafeUri( request.Url, request.Headers, IsProxySafeSiteDomain, GetProxySafePublicApplicationRoot );
        }

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. A host name, whether it came from a forwarded header or from the request itself, is used
        /// only when it is a configured site domain (or a loopback name). Otherwise the scheme, host and port come
        /// from the PublicApplicationRoot global attribute and the path and query of the request are kept.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Uri UrlProxySafe( this HttpRequestBase request )
        {
            return GetProxySafeUri( request.Url, request.Headers, IsProxySafeSiteDomain, GetProxySafePublicApplicationRoot );
        }

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. A host name, whether it came from a forwarded header or from the request itself, is used
        /// only when it is a configured site domain (or a loopback name). Otherwise the scheme, host and port come
        /// from the PublicApplicationRoot global attribute and the path and query of the request are kept.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal static Uri UrlProxySafe( this Net.IRequest request )
        {
            return GetProxySafeUri( request.RequestUri, request.Headers, IsProxySafeSiteDomain, GetProxySafePublicApplicationRoot );
        }

        /// <summary>
        /// Returns the URL of the request as the browser or proxy sent it: the forwarded host, scheme and
        /// port when X-Forwarded-Proto and a forwarded host header are present, otherwise the request URL.
        /// The host is NOT validated. This must never be used to build links, redirects or any URL that is
        /// sent to someone; use <see cref="UrlProxySafe(HttpRequest)"/> for that. It is only for decisions
        /// about the current request itself, such as which site was asked for, the cookie domain or whether
        /// the browser connection is secure.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The URL the request was received on, including any forwarded host.</returns>
        internal static Uri UrlForwardedUnvalidated( this HttpRequest request )
        {
            /*
                10/26/2026 - MSE

                UrlProxySafe now refuses hosts that are not configured site domains. A few callers only need
                to know what the browser asked for (site routing, the shared login cookie domain and the cookie
                Secure flag; the Twilio webhook signature check in App_Code has its own copy because it cannot
                call internal methods). Changing them to the PublicApplicationRoot
                host would break sign-in and routing on hosts such as the server IP address, so they keep the
                old unvalidated behavior through this method. Never use it to build a link.

                Reason: Keep request-only decisions on the host the request was received on.
            */
            return GetForwardedRequestUri( request.Url, request.Headers );
        }

        /// <summary>
        /// Returns the URL of the request as the browser or proxy sent it: the forwarded host, scheme and
        /// port when X-Forwarded-Proto and a forwarded host header are present, otherwise the request URL.
        /// The host is NOT validated. This must never be used to build links, redirects or any URL that is
        /// sent to someone; use <see cref="UrlProxySafe(HttpRequestBase)"/> for that. See
        /// <see cref="UrlForwardedUnvalidated(HttpRequest)"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The URL the request was received on, including any forwarded host.</returns>
        internal static Uri UrlForwardedUnvalidated( this HttpRequestBase request )
        {
            return GetForwardedRequestUri( request.Url, request.Headers );
        }

        /// <summary>
        /// Returns the URL of the request as the browser or proxy sent it: the forwarded host, scheme and
        /// port when X-Forwarded-Proto and a forwarded host header are present, otherwise the request URL.
        /// The host is NOT validated. This must never be used to build links, redirects or any URL that is
        /// sent to someone; use <see cref="UrlProxySafe(Net.IRequest)"/> for that. It is only for decisions
        /// about the current request itself, such as whether the browser connection is secure. See
        /// <see cref="UrlForwardedUnvalidated(HttpRequest)"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The URL the request was received on, including any forwarded host.</returns>
        internal static Uri UrlForwardedUnvalidated( this Net.IRequest request )
        {
            /*
                10/26/2026 - MSE

                Used by the cookie Secure flag decision for block action responses
                (RockMessageResponseContext on later versions), which must follow the scheme the
                browser actually used. Never use it to build a link.

                Reason: Keep request-only decisions on the scheme the request was received on.
            */
            if ( request.RequestUri == null )
            {
                return null;
            }

            return GetForwardedRequestUri( request.RequestUri, request.Headers );
        }

        /// <summary>
        /// Builds the request URL from the forwarded headers without validating the host. This is the logic
        /// UrlProxySafe used before hosts were validated. This must never be used to build links.
        /// </summary>
        /// <param name="requestUri">The URL the server received.</param>
        /// <param name="headers">The request headers.</param>
        /// <returns>The forwarded URL, or <paramref name="requestUri"/> when the request was not forwarded.</returns>
        internal static Uri GetForwardedRequestUri( Uri requestUri, NameValueCollection headers )
        {
            // Some proxies use X-Original-Host.
            // JME 2/22/2024
            // Added a custom header for forward hosts that is used by a language translation service called Weglot. Normally, we would not
            // add custom headers into Rock, but many organizations in the community use this service. We did reach out to them to see if they
            // would change to use the industry standard and were told no. Specifically the issue was when you use Weglot with Auth0.
            var forwardedHost = headers["X-Forwarded-Host"] ?? headers["X-Original-Host"] ?? headers["weglot-forwarded-host"];

            // If no proxy just return the request URL
            var isRequestForwaredFromProxy = forwardedHost != null && headers["X-Forwarded-Proto"] != null;

            if ( !isRequestForwaredFromProxy )
            {
                return requestUri;
            }

            // Assemble a URI from the proxied headers
            var builder = new UriBuilder( requestUri )
            {
                Scheme = headers["X-Forwarded-Proto"].ToString(),
                Host = forwardedHost
            };

            // If we have the original port then use it, otherwise reset to default port.
            if ( headers["X-Forwarded-Port"] != null )
            {
                builder.Port = headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1;
            }
            else
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        /// <summary>
        /// Builds the proxy safe URL of a request. A host name, whether it came from a
        /// forwarded header or from the request itself, is used only when it is one of
        /// the configured site domains. Otherwise the scheme, host and port come from
        /// the PublicApplicationRoot global attribute and the path and query are kept.
        /// </summary>
        /// <param name="requestUri">The URL the server received.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="isSiteDomain">Returns <c>true</c> when a host name is a configured site domain.</param>
        /// <param name="getPublicApplicationRoot">Returns the PublicApplicationRoot global attribute value.</param>
        /// <returns>The URL to use when building links for the request.</returns>
        /// <remarks>Internal so that it can be unit tested without the cache.</remarks>
        internal static Uri GetProxySafeUri( Uri requestUri, NameValueCollection headers, Func<string, bool> isSiteDomain, Func<string> getPublicApplicationRoot )
        {
            if ( requestUri == null )
            {
                return null;
            }

            /*
                10/26/2026 - MSE

                The forwarded host headers and the request's own Host header are sent by the client. Links
                built from them (passwordless sign-in, account confirmation and password reset emails, OAuth
                redirect URIs, the HTTPS upgrade redirect) could point at a host the requester controls and
                deliver the one-time code there. The host is now used only when it is a configured site
                domain; otherwise the PublicApplicationRoot scheme, host and port are used.

                Reason: Do not build links on a client-chosen host.
            */

            // Some proxies use X-Original-Host.
            // JME 2/22/2024
            // Added a custom header for forward hosts that is used by a language translation service called Weglot. Normally, we would not
            // add custom headers into Rock, but many organizations in the community use this service. We did reach out to them to see if they
            // would change to use the industry standard and were told no. Specifically the issue was when you use Weglot with Auth0.
            var forwardedHost = headers["X-Forwarded-Host"] ?? headers["X-Original-Host"] ?? headers["weglot-forwarded-host"];
            var forwardedProto = headers["X-Forwarded-Proto"]?.Trim();
            var isRequestForwardedFromProxy = forwardedHost != null && forwardedProto != null;

            var candidateHost = isRequestForwardedFromProxy ? forwardedHost.Trim() : requestUri.Host;

            /*
                10/26/2026 - MSE

                The candidate must be a plain host name or IP address. UriBuilder does not reject a host that
                contains "/", "#" or "?", so a forwarded host like "evil.test/x.church.org" would pass the
                site domain check (it ends with ".church.org") while the built URL points at evil.test.
                Likewise only http and https are accepted from X-Forwarded-Proto so a link can never use
                another scheme. Anything else is treated like an untrusted host and falls back below.

                Reason: Block host and scheme injection through forwarded headers.
            */
            var isForwardedProtoValid = string.Equals( forwardedProto, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase )
                || string.Equals( forwardedProto, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase );
            var isCandidateTrusted = IsPlainHostName( candidateHost )
                && ( !isRequestForwardedFromProxy || isForwardedProtoValid )
                && isSiteDomain( candidateHost );

            if ( isCandidateTrusted )
            {
                if ( !isRequestForwardedFromProxy )
                {
                    return requestUri;
                }

                // Same construction as before the fix.
                var builder = new UriBuilder( requestUri )
                {
                    Scheme = forwardedProto.ToLowerInvariant(),
                    Host = candidateHost
                };

                builder.Port = headers["X-Forwarded-Port"] != null
                    ? headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1
                    : -1;

                var forwardedUri = builder.Uri;

                // Defense in depth: only use the built URL if its host is exactly the validated host.
                if ( IsSameHost( forwardedUri.Host, candidateHost ) )
                {
                    return forwardedUri;
                }
            }

            // The host is not a trusted configured site domain, so it must not be used to build URLs.
            if ( !Uri.TryCreate( getPublicApplicationRoot()?.Trim(), UriKind.Absolute, out var rootUri )
                || ( rootUri.Scheme != Uri.UriSchemeHttp && rootUri.Scheme != Uri.UriSchemeHttps ) )
            {
                // No usable PublicApplicationRoot: use the URL the server received and ignore the forwarded headers.
                return requestUri;
            }

            return new UriBuilder( requestUri )
            {
                Scheme = rootUri.Scheme,
                Host = rootUri.Host,
                Port = rootUri.IsDefaultPort ? -1 : rootUri.Port
            }.Uri;
        }

        /// <summary>
        /// Determines whether the value is a plain DNS host name or IP address, with no port,
        /// path, query, fragment or other URL delimiter.
        /// </summary>
        /// <param name="host">The host name to check.</param>
        /// <returns><c>true</c> if the value is a plain host name or IP address; otherwise, <c>false</c>.</returns>
        internal static bool IsPlainHostName( string host )
        {
            if ( host.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var hostNameType = Uri.CheckHostName( host );

            if ( hostNameType == UriHostNameType.IPv4 || hostNameType == UriHostNameType.IPv6 )
            {
                return true;
            }

            if ( hostNameType != UriHostNameType.Dns )
            {
                return false;
            }

            /*
                10/26/2026 - MSE

                Uri.CheckHostName returns Dns for some non-ASCII names that turn into URL delimiters
                once a browser applies the IDNA mapping, for example the fullwidth slash (U+FF0F),
                fullwidth number sign (U+FF03) and fullwidth question mark (U+FF1F); the division
                slash (U+2215) is a look-alike. Every character must be a letter, digit, combining
                mark, '-', '.' or '_', and the IDNA (punycode) form must not contain a delimiter.

                UseStd3AsciiRules is NOT used: it rejects underscores, and internal host names such
                as rock_server pass Uri.CheckHostName today, so they must keep working. Instead the
                ASCII form is checked for delimiters directly.

                Reason: Block look-alike delimiter characters in forwarded host names.
            */
            foreach ( var character in host )
            {
                var unicodeCategory = char.GetUnicodeCategory( character );
                var isAllowedCharacter = char.IsLetterOrDigit( character )
                    || character == '-'
                    || character == '.'
                    || character == '_'
                    || unicodeCategory == System.Globalization.UnicodeCategory.NonSpacingMark
                    || unicodeCategory == System.Globalization.UnicodeCategory.SpacingCombiningMark;

                if ( !isAllowedCharacter )
                {
                    return false;
                }
            }

            string asciiHost;

            try
            {
                asciiHost = new System.Globalization.IdnMapping { UseStd3AsciiRules = false }.GetAscii( host );
            }
            catch ( ArgumentException )
            {
                // Intentionally ignored: a name that has no valid IDNA form is not a plain host name.
                return false;
            }

            return asciiHost.IndexOfAny( new[] { '/', '\\', '#', '?', '@', ':', '%', '[', ']', ' ', '\t', '\r', '\n' } ) < 0;
        }

        /// <summary>
        /// Determines whether two host names are the same, ignoring case. IP addresses are compared
        /// as addresses, because the Uri class can expand or bracket an IPv6 address.
        /// </summary>
        /// <param name="uriHost">The host of a built <see cref="Uri"/>.</param>
        /// <param name="expectedHost">The host that was validated.</param>
        /// <returns><c>true</c> if both are the same host; otherwise, <c>false</c>.</returns>
        private static bool IsSameHost( string uriHost, string expectedHost )
        {
            if ( string.Equals( uriHost, expectedHost, StringComparison.OrdinalIgnoreCase ) )
            {
                return true;
            }

            var isUriHostAddress = System.Net.IPAddress.TryParse( uriHost?.Trim( '[', ']' ), out var uriHostAddress );
            var isExpectedHostAddress = System.Net.IPAddress.TryParse( expectedHost?.Trim( '[', ']' ), out var expectedHostAddress );

            return isUriHostAddress && isExpectedHostAddress && uriHostAddress.Equals( expectedHostAddress );
        }

        /// <summary>
        /// Determines whether the host name is a configured site domain, using the site cache.
        /// </summary>
        /// <param name="host">The host name.</param>
        /// <returns><c>true</c> if the host name is a configured site domain; otherwise, <c>false</c>.</returns>
        private static bool IsProxySafeSiteDomain( string host )
        {
            try
            {
                return SiteCache.IsSiteDomain( host );
            }
            catch
            {
                // Intentionally ignored: the cache or database is not available (for example during
                // startup errors), so treat the host as unknown and let the caller fall back safely.
                return false;
            }
        }

        /// <summary>
        /// Gets the PublicApplicationRoot global attribute value.
        /// </summary>
        /// <returns>The PublicApplicationRoot value, or <c>null</c> if it could not be read.</returns>
        private static string GetProxySafePublicApplicationRoot()
        {
            try
            {
                return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
            }
            catch
            {
                // Intentionally ignored: the cache or database is not available, so the caller
                // falls back to the URL the server received.
                return null;
            }
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

        /// <summary>
        /// Returns the Sec-CH-UA-Platform-Version from the request headers if they exist without the quotes.
        /// See https://learn.microsoft.com/en-us/microsoft-edge/web-platform/how-to-detect-win11
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static string UserAgentPlatformVersion( this HttpRequest request )
        {
            var platformVersionHeader = request?.Headers?["Sec-CH-UA-Platform-Version"];
            if ( string.IsNullOrWhiteSpace( platformVersionHeader ) )
            {
                return string.Empty;
            }
            else
            {
                return platformVersionHeader.Trim( '"' );
            }
        }
    }
}