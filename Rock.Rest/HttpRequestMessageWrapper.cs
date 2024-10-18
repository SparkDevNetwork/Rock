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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

using Rock.Net;

namespace Rock.Rest
{
    /// <summary>
    /// A wrapper around HttpRequestMessage that makes it compatible with
    /// RockRequestContext.
    /// </summary>
    internal class HttpRequestMessageWrapper : IRequest
    {
        #region Properties

        /// <inheritdoc/>
        public IPAddress RemoteAddress { get; }

        /// <inheritdoc/>
        public Uri RequestUri { get; }

        /// <inheritdoc/>
        public NameValueCollection QueryString { get; }

        /// <inheritdoc/>
        public NameValueCollection Headers { get; }

        /// <inheritdoc/>
        public IDictionary<string, string> Cookies { get; }

        /// <inheritdoc/>
        public string Method { get; }

        /// <inheritdoc/>
        public bool CookiesValuesAreUrlDecoded { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessageWrapper"/> class.
        /// </summary>
        /// <param name="request">The request to be wrapped.</param>
        public HttpRequestMessageWrapper( HttpRequestMessage request )
        {
            var requestBase = request.Properties.ContainsKey( "MS_HttpContext" )
                ? ( request.Properties["MS_HttpContext"] as HttpContextWrapper )?.Request
                : null;

            if ( requestBase?.UserHostAddress != null )
            {
                if ( IPAddress.TryParse( requestBase.UserHostAddress, out IPAddress address ) )
                {
                    RemoteAddress = address;
                }
            }

            RequestUri = request.RequestUri;

            Method = GetMethodAsUppercaseString( request.Method );

            QueryString = new NameValueCollection( StringComparer.OrdinalIgnoreCase );
            foreach ( var kvp in request.GetQueryNameValuePairs() )
            {
                QueryString.Add( kvp.Key, kvp.Value );
            }

            Headers = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            if ( requestBase != null )
            {
                // Use the HttpRequestBase if it is available since it is faster
                // and includes more headers. This will be the case 99.9% of the time.
                foreach ( var key in requestBase.Headers.AllKeys )
                {
                    Headers.Add( key, requestBase.Headers.Get( key ) );
                }
            }
            else
            {
                // The Headers property of HttpRequestMessage is broken. Or rather
                // it isn't reliable. Sometimes it splits the values on a comma and
                // other times on a white-space character. Calling ToString() on it
                // will return the headers in a known and consistent format.
                // https://github.com/microsoft/referencesource/blob/e7b9db69533dca155a33f96523875e9c50445f44/System/net/System/Net/Http/Headers/HttpHeaders.cs#L221
                var headers = request.Headers.ToString().Split( new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var header in headers )
                {
                    var segments = header.Split( new char[] { ':' }, 2 );

                    if ( segments.Length == 2 )
                    {
                        Headers.Add( segments[0], segments[1].Trim() );
                    }
                }
            }

#if WEBFORMS
            // HttpRequestMessage.Headers.GetCookies() will barf on perfectly
            // valid cookie names if they contain a ':' character. So we try to
            // access cookies via the HttpContext first, which does not have
            // that problem.
            if ( requestBase != null )
            {
                Cookies = new Dictionary<string, string>();
                foreach ( var key in requestBase.Cookies.AllKeys )
                {
                    Cookies.AddOrReplace( key, requestBase.Cookies[key].Value );
                }
            }
            else
#endif
            {
                Cookies = new Dictionary<string, string>();
                foreach ( var cookie in request.Headers.GetCookies().SelectMany( c => c.Cookies ) )
                {
                    Cookies.AddOrReplace( cookie.Name, cookie.Value );
                }

                // HttpRequestMessage.Headers.GetCookies() automatically decodes cookie values.
                CookiesValuesAreUrlDecoded = true;
            }
        }

        #endregion

        #region Methods

        private string GetMethodAsUppercaseString( HttpMethod method )
        {
            if ( method == HttpMethod.Delete )
            {
                return "DELETE";
            }
            else if ( method == HttpMethod.Get )
            {
                return "GET";
            }
            else if ( method == HttpMethod.Head )
            {
                return "HEAD";
            }
            else if ( method == HttpMethod.Options )
            {
                return "OPTIONS";
            }
            else if ( method == HttpMethod.Post )
            {
                return "POST";
            }
            else if ( method == HttpMethod.Put )
            {
                return "PUT";
            }
            else if ( method == HttpMethod.Trace )
            {
                return "TRACE";
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
