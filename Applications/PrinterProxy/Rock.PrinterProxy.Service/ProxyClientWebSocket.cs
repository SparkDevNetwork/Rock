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
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

using Rock.PrinterProxy.Shared;

namespace Rock.PrinterProxy.Service;

/// <summary>
/// Client side implementation of the proxy web socket. This handles all
/// low-level communication between the proxy service and the Rock server.
/// </summary>
class ProxyClientWebSocket : ProxyWebSocket
{
    /// <summary>
    /// The instance to use when logging messages.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyClientWebSocket"/> class.
    /// </summary>
    /// <param name="socket">The <see cref="WebSocket"/> used for communication.</param>
    /// <param name="logger">The instance used for logging.</param>
    public ProxyClientWebSocket( WebSocket socket, ILogger logger )
        : base( socket )
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task OnMessageAsync( PrinterProxyMessage message, ReadOnlyMemory<byte> extraData, CancellationToken cancellationToken )
    {
        if ( message is PrinterProxyMessagePing pingMessage )
        {
            var pongResponse = new PrinterProxyResponsePing
            {
                RequestedAt = pingMessage.SentAt,
                RespondedAt = DateTimeOffset.Now
            };

            await PostResponseAsync( message, pongResponse, cancellationToken );
        }
        else if ( message is PrinterProxyMessagePrint printMessage )
        {
            var printResult = await SendPrintDataAsync( printMessage.Address, extraData, cancellationToken );

            await PostResponseAsync( message, printResult, cancellationToken );
        }
    }

    /// <summary>
    /// Sends the requested data to the printer to be printed.
    /// </summary>
    /// <param name="address">The address (and optional port) to connect to.</param>
    /// <param name="data">The data to be sent.</param>
    /// <param name="cancellationToken">A token that indicates if the operation should be cancelled.</param>
    /// <returns>An empty string if everything worked or an error message.</returns>
    private async Task<string> SendPrintDataAsync( string address, ReadOnlyMemory<byte> data, CancellationToken cancellationToken )
    {
        try
        {
            using var socket = await OpenSocketAsync( address, cancellationToken );
            using var ns = new NetworkStream( socket );

            await ns.WriteAsync( data, cancellationToken );

            _logger.LogDebug( "Sent {bytes} bytes to printer {address}.", data.Length, address );

            return string.Empty;
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to print to device {address}.", address );

            return ex.Message;
        }
    }

    /// <summary>
    /// Opens a socket to the IP Address.
    /// </summary>
    /// <param name="ipAddress">The ip address and optional port number.</param>
    /// <param name="cancellationToken">The token that lets us know when to abort the connection attempt.</param>
    /// <returns>A new isntance of <see cref="Socket"/>.</returns>
    private static async Task<Socket> OpenSocketAsync( string ipAddress, CancellationToken cancellationToken )
    {
        int printerPort = 9100;
        var printerIpAddress = ipAddress;

        // If the user specified in 0.0.0.0:1234 syntax then pull our the IP and port numbers.
        if ( printerIpAddress.Contains( ':' ) )
        {
            var segments = printerIpAddress.Split( ':' );

            printerIpAddress = segments[0];
            if ( !int.TryParse( segments[1], out printerPort ) )
            {
                printerPort = 9100;
            }
        }

        var printerEndpoint = new IPEndPoint( IPAddress.Parse( printerIpAddress ), printerPort );
        var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

        await socket.ConnectAsync( printerEndpoint, cancellationToken );

        return socket;
    }
}
