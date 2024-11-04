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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Rock.Bus.Message;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// The print provider for Rock that will send rendered label data to
    /// the physical printer devices.
    /// </summary>
    internal class LabelPrintProvider
    {
        /// <summary>
        /// The command sequence to append to make the label cut. This replaces
        /// the normal ^XZ end sequence so we need to append that again.
        /// </summary>
        private static readonly byte[] _cutAndEndLabel = new byte[]
        {
            (byte)'^',
            (byte)'M',
            (byte)'M',
            (byte)'C',
            (byte)'^',
            (byte)'X',
            (byte)'Z',
            (byte)'\r',
            (byte)'\n'
        };

        /// <summary>
        /// The command sequence to append to make not backfeed which also
        /// supresses cutting. This replaces the normal ^XZ end sequence so we
        /// need to append that again.
        /// </summary>
        private static readonly byte[] _supressBackfeedAndEndLabel = new byte[]
        {
            (byte)'^',
            (byte)'X',
            (byte)'B',
            (byte)'^',
            (byte)'X',
            (byte)'Z',
            (byte)'\r',
            (byte)'\n'
        };

        /// <summary>
        /// Print a set of labels to the associated printer devices. Labels are
        /// not checked for if they should print from server or client so that
        /// filtering must be done first.
        /// </summary>
        /// <param name="labels">The labels to print.</param>
        /// <param name="cancellationToken">The token that describes when the operation should be aborted.</param>
        /// <returns>A list of error messages generated during printing.</returns>
        public virtual async Task<List<string>> PrintLabelsAsync( IEnumerable<RenderedLabel> labels, CancellationToken cancellationToken )
        {
            var errors = new List<string>();
            var labelsByDevice = labels.Where( l => l.PrintTo != null )
                .GroupBy( l => l.PrintTo.Id )
                .ToList();

            foreach ( var deviceLabels in labelsByDevice )
            {
                var deviceLabelsList = deviceLabels.ToList();
                var deviceErrors = await PrintDeviceLabelsAsync( deviceLabelsList[0].PrintTo, deviceLabelsList, cancellationToken );

                if ( deviceErrors.Any() )
                {
                    errors.AddRange( deviceErrors );
                }
            }

            return errors;
        }

        /// <summary>
        /// Print a set of labels to a single device.
        /// </summary>
        /// <param name="printerDevice">The printer device to print the labels to.</param>
        /// <param name="labels">The labels to print to the device.</param>
        /// <param name="cancellationToken">A token to indicate if the operation should be aborted.</param>
        /// <returns>A list of error messages resulting from the print attempt.</returns>
        private async Task<List<string>> PrintDeviceLabelsAsync( DeviceCache printerDevice, List<RenderedLabel> labels, CancellationToken cancellationToken )
        {
            var messages = new List<string>();
            var printerHasCutter = printerDevice.GetAttributeValue( SystemKey.DeviceAttributeKey.DEVICE_HAS_CUTTER ).AsBoolean();

            for ( int labelIndex = 0; labelIndex < labels.Count; labelIndex++ )
            {
                try
                {
                    var labelContent = labels[labelIndex].Data;

                    if ( printerHasCutter )
                    {
                        labelContent = AmendWithCutCommands( labelContent, labelIndex == labels.Count - 1 );
                    }

                    if ( printerDevice.ProxyDeviceId.HasValue )
                    {
                        var proxy = CloudPrintSocket.GetBestProxyForDevice( printerDevice.ProxyDeviceId.Value );

                        if ( proxy != null )
                        {
                            var message = await proxy.PrintAsync( printerDevice, labelContent, cancellationToken );

                            if ( message.IsNotNullOrWhiteSpace() )
                            {
                                messages.Add( message );
                            }
                        }
                        else
                        {
                            var response = await CloudPrintLabelMessage.RequestAsync( printerDevice.ProxyDeviceId.Value, printerDevice.Id, labelContent, cancellationToken );

                            if ( response.Message.IsNotNullOrWhiteSpace() )
                            {
                                messages.Add( response.Message );
                            }
                        }
                    }
                    else
                    {
                        using ( var socket = await OpenSocketAsync( printerDevice.IPAddress, cancellationToken ) )
                        {
                            using ( var ns = new NetworkStream( socket ) )
                            {
                                await ns.WriteAsync( labelContent, 0, labelContent.Length, cancellationToken );
                            }
                        }
                    }
                }
                catch ( TaskCanceledException )
                {
                    return new List<string> { "Timed out waiting for labels to print." };
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    return new List<string> { $"Unable to print label: {ex.Message}" };
                }
            }

            return messages;
        }

        /// <summary>
        /// Amends the print data with the cut commands required for the
        /// operation.
        /// </summary>
        /// <param name="labelContent">The print data.</param>
        /// <param name="isLastLabel"><c>true</c> if this is the last label to be printed.</param>
        /// <returns>A new array of bytes to be printed.</returns>
        private byte[] AmendWithCutCommands( byte[] labelContent, bool isLastLabel )
        {
            var index = Array.LastIndexOf( labelContent, ( byte ) '^' );

            // Ensure we have the expected last command.
            if ( index == -1 || index > labelContent.Length - 3 || labelContent[index + 1] != 'X' || labelContent[index + 2] != 'Z' )
            {
                return labelContent;
            }

            if ( isLastLabel )
            {
                // If this is the last label and it ends in ^XZ then insert the
                // cut command.
                var newContent = new byte[index - 1 + _cutAndEndLabel.Length];

                labelContent.CopyTo( newContent, 0 );
                _cutAndEndLabel.CopyTo( newContent, newContent.Length - _cutAndEndLabel.Length );

                return newContent;
            }
            else
            {
                // If this is not the last label and it ends in ^XZ then insert the
                // supress backfeed command.
                var newContent = new byte[index - 1 + _supressBackfeedAndEndLabel.Length];

                labelContent.CopyTo( newContent, 0 );
                _supressBackfeedAndEndLabel.CopyTo( newContent, newContent.Length - _supressBackfeedAndEndLabel.Length );

                return newContent;
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
            if ( printerIpAddress.Contains( ":" ) )
            {
                var segments = printerIpAddress.Split( ':' );

                printerIpAddress = segments[0];
                printerPort = segments[1].AsInteger();
            }

            var printerEndpoint = new IPEndPoint( IPAddress.Parse( printerIpAddress ), printerPort );
            var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

#if WEBFORMS
            // .NET Framework doesn't have a way to cancel a connect request
            // so we need to fake it by closing the socket when the token
            // triggers.
            using ( cancellationToken.Register( () => socket.Close() ) )
            {
                try
                {
                    await socket.ConnectAsync( printerEndpoint );
                }
                catch ( NullReferenceException ) when ( cancellationToken.IsCancellationRequested )
                {
                    // NRE is thrown in .NET Framework when the socket is closed
                    // while still connecting.
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch ( ObjectDisposedException ) when ( cancellationToken.IsCancellationRequested )
                {
                    // OBE is sometimes thrown in .NET Framework when the socket is closed
                    // while still connecting.
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
#else
            await socket.ConnectAsync( printerEndpoint, cancellationToken );
#endif

            return socket;
        }
    }
}
