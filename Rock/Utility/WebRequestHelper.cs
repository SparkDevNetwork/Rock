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
using System.Net;
using System.Web;

using Rock.Model;

namespace Rock.Utility
{
    /// <summary>
    /// Helper Class to do a couple of handy things from a HttpContext
    /// </summary>
    public static class WebRequestHelper
    {
        /// <summary>
        /// Get the host name from request
        /// </summary>
        /// <param name="context">The HTTP Context.</param>
        /// <returns></returns>
        public static string GetHostNameFromRequest( HttpContext context )
        {
            var forwardedHost = context.Request.ServerVariables["HTTP_X_Forwarded_Host"];
            if ( forwardedHost.IsNotNullOrWhiteSpace() )
            {
                return forwardedHost;
            }

            return context.Request.UrlProxySafe().Host;
        }

        /// <summary>
        /// Get if connection is secure
        /// </summary>
        /// <param name="context">The HTTP Context.</param>
        /// <returns></returns>
        public static bool IsSecureConnection( HttpContext context )
        {
            return string.Equals( context.Request.ServerVariables["HTTP_X_FORWARDED_PROTO"], "https", StringComparison.OrdinalIgnoreCase ) || context.Request.IsSecureConnection;
        }

        /// <summary>
        /// Gets the client ip address.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string GetClientIpAddress( HttpRequestBase request )
        {
            string ipAddress = GetXForwardedForIpAddress( request ); // First look for the value in the X-FORWARDED-FOR header (proxies)

            if ( string.IsNullOrWhiteSpace( ipAddress ) )
            {
                ipAddress = request.ServerVariables["REMOTE_ADDR"]; // Then look in the normal header (direct traffic)
            }

            if ( string.IsNullOrWhiteSpace( ipAddress ) )
            {
                ipAddress = request.UserHostAddress;
            }

            if ( string.IsNullOrWhiteSpace( ipAddress ) || ipAddress.Trim() == "::1" )
            {
                ipAddress = string.Empty;
            }

            if ( string.IsNullOrWhiteSpace( ipAddress ) )
            {
                string stringHostName = System.Net.Dns.GetHostName();
                if ( !string.IsNullOrWhiteSpace( stringHostName ) )
                {
                    try
                    {
                        var ipHostEntries = System.Net.Dns.GetHostEntry( stringHostName );
                        if ( ipHostEntries != null )
                        {
                            try
                            {
                                var arrIpAddress = ipHostEntries.AddressList.FirstOrDefault( i => !i.IsIPv6LinkLocal );
                                if ( arrIpAddress != null )
                                {
                                    ipAddress = arrIpAddress.ToString();
                                }
                            }
                            catch
                            {
                                try
                                {
                                    var arrIpAddress = System.Net.Dns.GetHostAddresses( stringHostName ).FirstOrDefault( i => !i.IsIPv6LinkLocal );
                                    if ( arrIpAddress != null )
                                    {
                                        ipAddress = arrIpAddress.ToString();
                                    }
                                }
                                catch
                                {
                                    ipAddress = "127.0.0.1";
                                }
                            }
                        }
                    }
                    catch ( System.Net.Sockets.SocketException ex )
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }
            }

            return ipAddress;
        }

        /// <summary>
        /// Gets the IP Address from the X-Forwarded-For header. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private static string GetXForwardedForIpAddress( HttpRequestBase request )
        {
            return GetXForwardedForIpAddress( request.ServerVariables["HTTP_X_FORWARDED_FOR"] );
        }

        /// <summary>
        /// Gets the IP Address from the X-Forwarded-For header. 
        /// </summary>
        /// <param name="headerValue">The value of the X-Forwarded-For header.</param>
        /// <returns></returns>
        public static string GetXForwardedForIpAddress( string headerValue )
        {
            /* 10/7/2021 - JME 
               Gets the IP Address from the X-Forward-For header. This can be a single address or in complex environments it could be a commma
               delimited list of proxies.
               Example from parner church with a CDN AND Web Farm: X-Forwarded-For: 68.14.xxx.xx, 147.243.xxx.xxx, 147.243.xxxx.xxx:57275
            */

            if ( headerValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var ipAddress = headerValue.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries ).ToList().FirstOrDefault();

            /* 12/20/2022 - DSH
               The X-Forwarded-For header can contain either an IPv4 or IPv6
               address, with or without a port number. Therefor the actual content
               might match one of the following four values:
               169.254.18.24
               169.254.18.24:28372
               fe80::260:97ff:fe02:6ea5
               [fe80::260:97ff:fe02:6ea5]:28372
             */
            if ( ipAddress != null )
            {
                // Check for either IPv4 or IPv6 with port number.
                if ( ipAddress.StartsWith( "[" ) && ipAddress.Contains( "]" ) )
                {
                    // IPv6 with port number.
                    ipAddress = ipAddress.Substring( 1, ipAddress.IndexOf( "]" ) - 1 );
                }
                else
                {
                    var colonSegments = ipAddress.Split( ':' );

                    // Check for IPv4 with port number.
                    if ( colonSegments.Length == 2 )
                    {
                        ipAddress = colonSegments[0];
                    }
                }
            }

            return ipAddress;
        }

        /// <summary>
        /// Determines whether the Client's IP Address falls within a range.
        /// Uses suggested method from https://stackoverflow.com/a/2138724.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="beginningIPAddress">The beginning ip address.</param>
        /// <param name="endingIPAddress">The ending ip address.</param>
        /// <returns><c>true</c> if [is ip address in range] [the specified request]; otherwise, <c>false</c>.</returns>
        public static bool IsIPAddressInRange( HttpRequestBase request, string beginningIPAddress, string endingIPAddress )
        {
            return IsIPAddressInRange( GetClientIpAddress( request ), beginningIPAddress, endingIPAddress );
        }

        /// <summary>
        /// Determines whether the Client's IP Address falls within a range.
        /// Uses suggested method from https://stackoverflow.com/a/2138724.
        /// </summary>
        /// <param name="clientIPAddress">The IP address to check.</param>
        /// <param name="beginningIPAddress">The beginning ip address.</param>
        /// <param name="endingIPAddress">The ending ip address.</param>
        /// <returns><c>true</c> if [is ip address in range] [the specified request]; otherwise, <c>false</c>.</returns>
        internal static bool IsIPAddressInRange( string clientIPAddress, string beginningIPAddress, string endingIPAddress )
        {
            // Using suggested method from https://stackoverflow.com/a/2138724
            if ( !IPAddress.TryParse( clientIPAddress, out var parsedClientIPAddress ) )
            {
                return false;
            }

            if ( !IPAddress.TryParse( beginningIPAddress, out var parsedBeginningIPAddress ) )
            {
                return false;
            }

            if ( !IPAddress.TryParse( endingIPAddress, out var parsedEndingIPAddress ) )
            {
                return false;
            }
            
            var rangeAddressFamily = parsedBeginningIPAddress.AddressFamily;

            if ( parsedClientIPAddress.AddressFamily != rangeAddressFamily )
            {
                // IP AddressFamilies are different. IPv4 vs IPv6, etc,
                // so return false;
                return false;
            }

            byte[] addressBytes = parsedClientIPAddress.GetAddressBytes();
            var lowerBytes = parsedBeginningIPAddress.GetAddressBytes();
            var upperBytes = parsedEndingIPAddress.GetAddressBytes();

            bool lowerBoundary = true;
            bool upperBoundary = true;

            for ( int i = 0; i < lowerBytes.Length &&
                ( lowerBoundary || upperBoundary ); i++ )
            {
                if ( ( lowerBoundary && addressBytes[i] < lowerBytes[i] ) ||
                    ( upperBoundary && addressBytes[i] > upperBytes[i] ) )
                {
                    return false;
                }

                lowerBoundary &= ( addressBytes[i] == lowerBytes[i] );
                upperBoundary &= ( addressBytes[i] == upperBytes[i] );
            }

            return true;
        }
    }
}
