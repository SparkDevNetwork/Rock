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
using System.Collections.Concurrent;
using System.Linq;
using System.Web;

using UAParser;

namespace Rock.Net
{
    /// <summary>
    /// Provides information on a remote client that is making a request to the server.
    /// </summary>
    public class ClientInformation
    {
        #region Private Fields

        /// <summary>
        /// Cached copies of the client info for a given user agent string.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ClientInfo> _cachedBrowserInfo = new ConcurrentDictionary<string, ClientInfo>();

        /// <summary>
        /// The shared UA parser that will be used.
        /// </summary>
        private static readonly Parser _uaParser = Parser.GetDefault();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IpAddress { get; }

        /// <summary>
        /// Gets the browser object that identifies what we know about the browser.
        /// </summary>
        /// <value>
        /// The browser object that identifies what we know about the browser.
        /// </value>
        public ClientInfo Browser => GetClientInfoForUserAgent( UserAgent );

        /// <summary>
        /// Gets the user agent identifier string.
        /// </summary>
        /// <value>
        /// The user agent identifier string.
        /// </value>
        public string UserAgent { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initialize from.</param>
        internal ClientInformation( HttpRequest request )
        {
            // Set IP Address.
            IpAddress = Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress( request.ServerVariables["HTTP_X_FORWARDED_FOR"] )
                ?? request.ServerVariables["REMOTE_ADDR"]
                ?? string.Empty;

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            UserAgent = request.UserAgent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initalize from.</param>
        internal ClientInformation( IRequest request )
        {
            IpAddress = Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress( request.Headers["X-FORWARDED-FOR"] )
                ?? request.RemoteAddress?.ToString()
                ?? string.Empty;

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            UserAgent = request.Headers.GetValues( "USER-AGENT" )?.FirstOrDefault() ?? string.Empty;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the client information from the user agent string. This uses
        /// caching to reduce overhead from parsing.
        /// </summary>
        /// <param name="userAgent">The user agent string.</param>
        /// <returns>The details from the user agent string.</returns>
        internal static ClientInfo GetClientInfoForUserAgent( string userAgent )
        {
            if ( userAgent.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Prevent abuse of cache.
            if ( _cachedBrowserInfo.Count > 10_000 )
            {
                _cachedBrowserInfo.Clear();
            }

            return _cachedBrowserInfo.GetOrAdd( userAgent, ua => _uaParser.Parse( ua ) );
        }

        #endregion
    }
}
