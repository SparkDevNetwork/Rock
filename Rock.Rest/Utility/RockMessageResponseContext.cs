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
using System.Globalization;
using System.Net.Http;
using System.Text;

using Rock.Configuration;
using Rock.Enums.Net;
using Rock.Net;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// An implementation of <see cref="IRockResponseContext"/> that can be
    /// used when dealing with API requests.
    /// </summary>
    internal class RockMessageResponseContext : IRockResponseContext
    {
        #region Fields

        /// <summary>
        /// The request that this response is associated with.
        /// </summary>
        private readonly IRequest _request;

        /// <summary>
        /// The HTML element identifiers that have already been seen and should
        /// be ignored on further adds.
        /// </summary>
        private readonly HashSet<string> _seenIds = new HashSet<string>();

        private readonly List<BrowserCookie> _cookies = new List<BrowserCookie>();

        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RockResponseContext"/>.
        /// </summary>
        /// <param name="request">The request that this response will be associated with.</param>
        internal RockMessageResponseContext( IRequest request )
        {
            _request = request;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb )
        {
        }

        /// <inheritdoc/>
        public void AddCookie( BrowserCookie cookie )
        {
            cookie = new BrowserCookie( cookie );

            if ( cookie.Path.IsNullOrWhiteSpace() )
            {
                cookie.Path = RockApp.Current.ResolveRockUrl( "~" );
            }

            if ( cookie.SameSite == CookieSameSiteMode.Unspecified )
            {
                var sameSiteCookieSetting = GlobalAttributesCache.Get()
                    .GetValue( "core_SameSiteCookieSetting" )
                    .ConvertToEnumOrNull<Rock.Security.Authorization.SameSiteCookieSetting>() ?? Rock.Security.Authorization.SameSiteCookieSetting.Lax;

                if ( sameSiteCookieSetting == Security.Authorization.SameSiteCookieSetting.None )
                {
                    cookie.SameSite = CookieSameSiteMode.None;
                }
                else if ( sameSiteCookieSetting == Security.Authorization.SameSiteCookieSetting.Lax )
                {
                    cookie.SameSite = CookieSameSiteMode.Lax;
                }
                else
                {
                    cookie.SameSite = CookieSameSiteMode.Strict;
                }
            }

            if ( !cookie.Secure )
            {
                // Check the requested URL to see if it is for a secure connection.
                if ( _request.UrlProxySafe().Scheme == "https" )
                {
                    cookie.Secure = true;
                }
            }

            _cookies.Add( cookie );
        }

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie )
        {
            cookie = new BrowserCookie( cookie )
            {
                Expires = DateTime.Now.AddDays( -1 )
            };

            AddCookie( cookie );
        }

        /// <inheritdoc/>
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
        {
        }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false )
        {
        }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value )
        {
            _headers.AddOrReplace( name, value );
        }

        /// <inheritdoc/>
        public void SetPageTitle( string title )
        {
        }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title )
        {
        }

        /// <summary>
        /// Updates the response message with the values from this instance.
        /// </summary>
        /// <param name="message"></param>
        public void Update( HttpResponseMessage message )
        {
            foreach ( var cookie in _cookies )
            {
                message.Headers.Add( "Set-Cookie", BuildCookie( cookie ) );
            }

            foreach ( var header in _headers )
            {
                if ( message.Headers.Contains( header.Key ) )
                {
                    message.Headers.Remove( header.Key );
                }

                message.Headers.Add( header.Key, header.Value );
            }
        }

        /// <summary>
        /// Builds a cookie string that can be used as a Set-Cookie header value.
        /// We build it this way to ensure that it is compatible with the way
        /// WebForms writes the cookies. Using the normal CookieHeaderValue class
        /// causes the entire cookie value to be URI encoded. While this might
        /// be technically more accurate, it is not compatible with the way we
        /// expect cookies to be written. In the future we may want to change
        /// this.
        /// </summary>
        /// <param name="cookie">The cookie to build.</param>
        /// <returns>The string to pass to the "Set-Cookie" header value.</returns>
        private static string BuildCookie( BrowserCookie cookie )
        {
            var stringBuilder = new StringBuilder();

            AppendCookieSegment( stringBuilder, $"{cookie.Name}={cookie.Value}", null );

            AppendCookieSegment( stringBuilder, "expires", ( cookie.Expires ?? DateTime.MinValue ).ToUniversalTime().ToString( "r", CultureInfo.InvariantCulture ) );

            //if ( cookie.MaxAge.HasValue )
            //{
            //    AppendCookieSegment( stringBuilder, "max-age", ( ( int ) cookie.MaxAge.Value.TotalSeconds ).ToString( NumberFormatInfo.InvariantInfo ) );
            //}

            if ( cookie.Domain != null )
            {
                AppendCookieSegment( stringBuilder, "domain", cookie.Domain );
            }

            if ( cookie.Path != null )
            {
                AppendCookieSegment( stringBuilder, "path", $"{cookie.Path}" );
            }

            if ( cookie.SameSite != CookieSameSiteMode.Unspecified )
            {
                AppendCookieSegment( stringBuilder, "samesite", cookie.SameSite.ToString() );
            }

            if ( cookie.Secure )
            {
                AppendCookieSegment( stringBuilder, "secure", null );
            }

            if ( cookie.HttpOnly )
            {
                AppendCookieSegment( stringBuilder, "httponly", null );
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Appends a name value pair segment to the cookie string.
        /// </summary>
        /// <param name="builder">The builder to append the segment to.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value or null.</param>
        private static void AppendCookieSegment( StringBuilder builder, string name, string value )
        {
            if ( builder.Length > 0 )
            {
                builder.Append( "; " );
            }

            builder.Append( name );

            if ( value != null )
            {
                builder.Append( "=" );
                builder.Append( value );
            }
        }

        #endregion
    }
}
