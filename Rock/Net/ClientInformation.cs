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
using System.Net.Http;
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
        /// The shared UA parser that will be used.
        /// </summary>
        private static readonly Parser _uaParser = Parser.GetDefault();

        /// <summary>
        /// The browser information is lazy loaded since it can take a few
        /// milliseconds to parse the regex and is only rarely used.
        /// </summary>
        private readonly Lazy<ClientInfo> _browser;

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
        public ClientInfo Browser => _browser.Value;

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
            //
            // Set IP Address.
            //
            IpAddress = string.Empty;

            // http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
            string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( !string.IsNullOrEmpty( ipAddress ) )
            {
                string[] addresses = ipAddress.Split( ',' );
                if ( addresses.Length != 0 )
                {
                    IpAddress = addresses[0];
                }
            }
            else
            {
                IpAddress = request.ServerVariables["REMOTE_ADDR"];
            }

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            UserAgent = request.UserAgent;
            _browser = new Lazy<ClientInfo>( () => _uaParser.Parse( UserAgent ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initalize from.</param>
        internal ClientInformation( IRequest request )
        {
            //
            // Set IP Address.
            //
            IpAddress = string.Empty;

            // http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
            if ( request.Headers["X-FORWARDED-FOR"] != null )
            {
                IpAddress = request.Headers.GetValues( "X-FORWARDED-FOR" ).First();
            }
            else
            {
                IpAddress = request.RemoteAddress?.ToString() ?? string.Empty;
            }

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            UserAgent = request.Headers.GetValues( "USER-AGENT" ).FirstOrDefault() ?? string.Empty;
            _browser = new Lazy<ClientInfo>( () => _uaParser.Parse( UserAgent ) );
        }

        #endregion
    }
}
