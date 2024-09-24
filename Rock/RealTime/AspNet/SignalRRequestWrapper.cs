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

using Microsoft.AspNet.SignalR;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// Proxies a SignalR IRequest object to a standard Rock IRequest object.
    /// </summary>
    internal class SignalRRequestWrapper : Rock.Net.IRequest
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
        public bool CookiesValuesAreUrlDecoded => true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRRequestWrapper"/> class.
        /// </summary>
        /// <param name="request">The request that will be wrapped.</param>
        public SignalRRequestWrapper( IRequest request )
        {
            if ( request.Environment.TryGetValue( "server.RemoteIpAddress", out var ipAddress ) )
            {
                if ( IPAddress.TryParse( ipAddress.ToString(), out var address ) )
                {
                    RemoteAddress = address;
                }
            }

            // request.Url is not available with Azure SignalR and doesn't really
            // make much sense anyway since it would just be the /rock-rt endpoint.
            // Therefore, do not set a RequestUri.

            Method = GetMethodAsUppercaseString( request );

            QueryString = new NameValueCollection( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var qs in request.QueryString )
            {
                QueryString.Add( qs.Key, qs.Value );
            }

            Headers = new NameValueCollection( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var header in request.Headers )
            {
                Headers.Add( header.Key, header.Value );
            }

            Cookies = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var cookie in request.Cookies )
            {
                Cookies.AddOrReplace( cookie.Key, cookie.Value.Value );
            }
        }

        private string GetMethodAsUppercaseString( IRequest request )
        {
            if ( request is Microsoft.AspNet.SignalR.Owin.ServerRequest owinRequest )
            {
                return owinRequest.GetHttpContext()?.Request?.HttpMethod?.ToUpper();
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
