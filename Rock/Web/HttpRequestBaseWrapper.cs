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
using System.Net;
using System.Net.Http;
using System.Web;

using Rock.Net;

namespace Rock.Web
{
    class HttpRequestBaseWrapper : IRequest
    {
        #region Properties

        /// <inheritdoc/>
        public IPAddress RemoteAddress { get; }

        /// <inheritdoc/>
        public Uri RequestUri { get; }

        /// <inheritdoc/>
        public NameValueCollection QueryString { get; }

        /// <inheritdoc/>
        public IDictionary<string, object> RouteData { get; }

        /// <inheritdoc/>
        public NameValueCollection Headers { get; }

        /// <inheritdoc/>
        public IDictionary<string, string> Cookies { get; }

        /// <inheritdoc/>
        public string Method { get; }

        /// <inheritdoc/>
        public bool CookiesValuesAreUrlDecoded { get; private set; }

        #endregion

        public HttpRequestBaseWrapper( HttpRequestBase request )
        {
            if ( request.UserHostAddress != null )
            {
                if ( IPAddress.TryParse( request.UserHostAddress, out IPAddress address ) )
                {
                    RemoteAddress = address;
                }
            }

            RequestUri = request.Url;

            Method = request.HttpMethod.ToUpper();

            QueryString = new NameValueCollection( StringComparer.OrdinalIgnoreCase );
            foreach ( string key in request.QueryString.Keys )
            {
                QueryString[key] = request.QueryString[key];
            }

            RouteData = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );
            foreach ( var kvp in request.RequestContext.RouteData.Values )
            {
                RouteData.Add( kvp.Key, kvp.Value );
            }

            Headers = new NameValueCollection( StringComparer.OrdinalIgnoreCase );

            foreach ( var key in request.Headers.AllKeys )
            {
                Headers.Add( key, request.Headers.Get( key ) );
            }

            Cookies = new Dictionary<string, string>();
            foreach ( var key in request.Cookies.AllKeys )
            {
                Cookies.AddOrReplace( key, request.Cookies[key].Value );
            }
        }
    }
}
