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
using System.Linq;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Net.Geolocation;
using Rock.Web.HttpModules;

using UAParser;

namespace Rock.Net
{
    /// <summary>
    /// Provides information on a remote client that is making a request to the server.
    /// </summary>
    public class ClientInformation
    {
        #region Properties

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IpAddress { get; }

        /// <summary>
        /// Gets the parsed details of the browser making the request.
        /// </summary>
        /// <value>The parsed browser details, or <c>null</c> when no user-agent string was supplied.</value>
        public UserAgentInfo BrowserInfo { get; }

        /// <summary>
        /// Gets the browser object that identifies what we know about the browser.
        /// </summary>
        /// <value>
        /// The browser object that identifies what we know about the browser.
        /// </value>
        [Obsolete( "Use BrowserInfo instead. The new property returns a Rock-owned type that does not depend on UAParser." )]
        [RockObsolete( "20.0" )]
        public ClientInfo Browser => BrowserInfo?.OriginalClientInfo;

        /// <summary>
        /// Gets the user agent identifier string.
        /// </summary>
        /// <value>
        /// The user agent identifier string.
        /// </value>
        public string UserAgent { get; }

        /// <summary>
        /// Gets the geolocation data.
        /// </summary>
        /// <value>
        /// The geolocation data.
        /// </value>
        public IpGeolocation Geolocation { get; }

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

            if ( request.RequestContext.HttpContext.Items[RockGateway.GeolocationContextKey] is IpGeolocation geolocation )
            {
                Geolocation = geolocation;
            }

            if ( UserAgent.IsNotNullOrWhiteSpace() )
            {
                BrowserInfo = RockApp.Current.GetRequiredService<IUserAgentParser>().Parse( UserAgent );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initialize from.</param>
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

            Geolocation = IpGeoLookup.Instance.GetGeolocation( IpAddress );

            UserAgent = request.Headers.GetValues( "USER-AGENT" )?.FirstOrDefault() ?? string.Empty;
        }

        #endregion
    }
}
