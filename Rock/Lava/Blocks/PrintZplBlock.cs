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
using System.IO;
using System.Net.Sockets;
using System.Text;

using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Lava block that prints raw ZPL to a network printer via TCP (default port 9100).
    /// Parameters:
    /// - deviceid: integer/idkey/guid of a Rock Device (preferred if both provided)
    /// - ipaddress: "x.x.x.x" or "x.x.x.x:9100"
    /// Outputs nothing on success. Outputs an error message to the Lava render only on failure.
    ///
    /// Usage:
    /// {% printzpl deviceid:'12' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// Or:
    /// {% printzpl ipaddress:'192.168.1.145' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// Or:
    /// {% printzpl ipaddress:'192.168.1.145:9100' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// </summary>
    public class PrintZplBlock : LavaBlockBase, ILavaSecured
    {
        private const int DefaultTimeoutMs = 5000;
        private string _markup = string.Empty;

        /// <summary>
        /// Initializes the block with the raw markup.
        /// </summary>
        /// <param name="tagName">The Lava tag name.</param>
        /// <param name="markup">The tag markup (parameters).</param>
        /// <param name="tokens">The tokenized Lava content.</param>
        public override void OnInitialize( string tagName, string markup, System.Collections.Generic.List<string> tokens )
        {
            _markup = markup;
            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the block by sending the rendered body (ZPL) to the resolved printer endpoint.
        /// Writes to the output only when an error occurs.
        /// </summary>
        /// <param name="context">The Lava render context.</param>
        /// <param name="result">The output writer.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            if ( !IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, SourceElementName ) );
                return;
            }

            var parms = GetAttributesFromMarkup( _markup, context ).Attributes;

            var bodyWriter = new StringWriter();
            base.OnRender( context, bodyWriter );
            var zpl = ( bodyWriter.ToString() ?? string.Empty ).Trim();

            if ( string.IsNullOrWhiteSpace( zpl ) )
            {
                result.Write( "PrintZpl: No ZPL content was provided in the block body." );
                return;
            }

            var endpoint = ResolvePrinterEndpoint( parms["deviceid"], parms["ipaddress"] );
            if ( endpoint == null )
            {
                result.Write( "PrintZpl: You must provide a valid deviceid or ipaddress (deviceid is preferred if both are provided)." );
                return;
            }

            try
            {
                SendRawTcp( endpoint.Value.Host, endpoint.Value.Port, zpl, DefaultTimeoutMs );
                // Success: output nothing.
            }
            catch ( Exception ex )
            {
                result.Write( $"PrintZpl: {ex.Message}" );
            }
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var attributes = LavaElementAttributes.NewFromMarkup( markup, context );
            attributes.AddOrIgnore( "deviceid", "" );
            attributes.AddOrIgnore( "ipaddress", "" );
            return attributes;
        }

        private static (string Host, int Port)? ResolvePrinterEndpoint( string deviceIdValue, string ipAddressValue )
        {
            if ( !string.IsNullOrWhiteSpace( deviceIdValue ) )
            {
                var fromDevice = ResolveEndpointFromDevice( deviceIdValue );
                if ( fromDevice != null )
                {
                    return fromDevice;
                }
            }

            if ( !string.IsNullOrWhiteSpace( ipAddressValue ) )
            {
                if ( TryParseHostPort( ipAddressValue, out var host, out var port ) )
                {
                    return (host, port);
                }
            }

            return null;
        }

        private static (string Host, int Port)? ResolveEndpointFromDevice( string deviceIdValue )
        {
            var device = DeviceCache.Get( deviceIdValue, true );
            if ( device == null )
            {
                return null;
            }

            var ip = ( device.IPAddress ?? string.Empty ).Trim();
            if ( string.IsNullOrWhiteSpace( ip ) )
            {
                return null;
            }

            if ( !TryParseHostPort( ip, out var host, out var port ) )
            {
                return null;
            }

            return (host, port);
        }

        private static bool TryParseHostPort( string input, out string host, out int port )
        {
            host = null;
            port = 9100;

            input = ( input ?? string.Empty ).Trim();
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return false;
            }

            var lastColon = input.LastIndexOf( ':' );
            if ( lastColon > 0 && lastColon < input.Length - 1 )
            {
                var maybeHost = input.Substring( 0, lastColon ).Trim();
                var maybePort = input.Substring( lastColon + 1 ).Trim();

                var parsedPort = maybePort.AsIntegerOrNull();
                if ( parsedPort.HasValue && parsedPort.Value >= 1 && parsedPort.Value <= 65535 )
                {
                    host = maybeHost;
                    port = parsedPort.Value;
                    return !string.IsNullOrWhiteSpace( host );
                }
            }

            host = input;
            return !string.IsNullOrWhiteSpace( host );
        }

        private static void SendRawTcp( string host, int port, string payload, int timeoutMs )
        {
            var bytes = Encoding.UTF8.GetBytes( EnsureEndsWithNewline( payload ) );

            using ( var client = new TcpClient() )
            {
                var ar = client.BeginConnect( host, port, null, null );
                if ( !ar.AsyncWaitHandle.WaitOne( timeoutMs ) )
                {
                    try
                    { client.Close(); }
                    catch { }
                    throw new TimeoutException( "Connection timed out." );
                }

                client.EndConnect( ar );

                using ( var stream = client.GetStream() )
                {
                    stream.WriteTimeout = timeoutMs;
                    stream.Write( bytes, 0, bytes.Length );
                    stream.Flush();
                }
            }
        }

        private static string EnsureEndsWithNewline( string s )
        {
            if ( string.IsNullOrEmpty( s ) )
            {
                return "\n";
            }

            if ( s.EndsWith( "\n" ) || s.EndsWith( "\r\n" ) )
            {
                return s;
            }

            return s + "\n";
        }

        /// <summary>
        /// Gets the permission key required to execute this block.
        /// </summary>
        public string RequiredPermissionKey => "PrintZpl";
    }
}