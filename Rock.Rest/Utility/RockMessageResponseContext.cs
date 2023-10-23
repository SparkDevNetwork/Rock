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
using System.Net.Http;
using System.Net.Http.Headers;

using Rock.Enums.Net;
using Rock.Net;
using Rock.Web;

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
        internal RockMessageResponseContext()
        {
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
            _cookies.Add( cookie );
        }

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie )
        {
            cookie.Expires = DateTime.Now.AddDays( -1 );
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
            var cookies = _cookies
                .Select( cookie =>
                {
                    var messageCookie = new CookieHeaderValue( cookie.Name, cookie.Value )
                    {
                        Domain = cookie.Domain,
                        Path = cookie.Path,
                        Secure = cookie.Secure,
                        HttpOnly = cookie.HttpOnly,
                        Expires = cookie.Expires ?? DateTime.MinValue
                    };

                    // This is an ugly hack, but it works. CookieHeaderValue does not
                    // support same site.
                    switch ( cookie.SameSite )
                    {
                        case CookieSameSiteMode.None:
                            messageCookie.Path += "; SameSite=None";
                            break;

                        case CookieSameSiteMode.Lax:
                            messageCookie.Path += "; SameSite=Lax";
                            break;

                        case CookieSameSiteMode.Strict:
                            messageCookie.Path += "; SameSite=Strict";
                            break;

                        case CookieSameSiteMode.Unspecified:
                        default:
                            messageCookie.Path += "; SameSite=None";
                            break;
                    }

                    return messageCookie;
                } );

            message.Headers.AddCookies( cookies );

            foreach ( var header in _headers )
            {
                if ( message.Headers.Contains( header.Key ) )
                {
                    message.Headers.Remove( header.Key );
                }

                message.Headers.Add( header.Key, header.Value );
            }
        }

        #endregion
    }
}
