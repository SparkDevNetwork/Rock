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
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Rock.PrinterProxy.Shared;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Handles all the communication with printer proxies that are connected
    /// to this Rock instance.
    /// </summary>
    internal sealed class PrinterProxySocket : ProxyWebSocket
    {
        #region Static Feilds

        /// <summary>
        /// Used to tell the long-running monitor task that we no longer need
        /// it running and it should stop.
        /// </summary>
        private static CancellationTokenSource _monitorCancellationTokenSource;

        /// <summary>
        /// The proxies that are currently connected to this Rock instance.
        /// </summary>
        private static readonly Dictionary<int, List<PrinterProxySocket>> _proxies = new Dictionary<int, List<PrinterProxySocket>>();

        /// <summary>
        /// Synchronization object for access to the other static fields.
        /// </summary>
        private static readonly object _lock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// The identifier of the <see cref="Model.Device"/> that this proxy is
        /// connected as.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// The friendly name of this proxy.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The priority that this proxy has. The lowest value has the most
        /// priority. If multiple proxies have the same priority then they may
        /// be used in a round-robin manner.
        /// </summary>
        public int Priority { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the <see cref="PrinterProxySocket"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="WebSocket"/> object that will handling the low-level communication.</param>
        /// <param name="deviceId">The device identifier for the proxy.</param>
        /// <param name="name">The friendly name of the proxy.</param>
        /// <param name="priority">The priority of the printer proxy.</param>
        public PrinterProxySocket( WebSocket socket, int deviceId, string name, int priority )
            : base( socket )
        {
            DeviceId = deviceId;
            Name = name;
            Priority = priority;

            AddProxy( this );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override Task OnMessageAsync( PrinterProxyMessage message, ReadOnlyMemory<byte> extraData, CancellationToken cancellationToken )
        {
            if ( message is PrinterProxyMessagePing pingMessage )
            {
                var pongResponse = new PrinterProxyResponsePing
                {
                    RequestedAt = pingMessage.SentAt,
                    RespondedAt = RockDateTime.Now.ToRockDateTimeOffset()
                };

                return PostResponseAsync( message, pongResponse, cancellationToken );
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the data to the printer by way of the proxied connection.
        /// </summary>
        /// <param name="printer">The device that represents the printer.</param>
        /// <param name="data">The data to be printed.</param>
        /// <param name="cancellationToken">A token that indicates if the print operation should be aborted.</param>
        /// <returns>A string that represents any error message returned by the proxy.</returns>
        public async Task<string> PrintAsync( DeviceCache printer, byte[] data, CancellationToken cancellationToken = default )
        {
            var printMessage = new PrinterProxyMessagePrint
            {
                Address = printer.IPAddress
            };

            return await SendMessageAsync<string>( printMessage, data, cancellationToken );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the best proxy to use for the specified proxy <see cref="Model.Device"/>.
        /// </summary>
        /// <param name="proxyDeviceId">The identifier of the proxy.</param>
        /// <returns>An instance of <see cref="PrinterProxySocket"/> or <see langword="null"/> if no proxy was available.</returns>
        public static PrinterProxySocket GetBestProxyForDevice( int proxyDeviceId )
        {
            lock ( _lock )
            {
                if ( _proxies.TryGetValue( proxyDeviceId, out var proxies ) )
                {
                    return proxies.OrderBy( p => p.Priority ).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds the proxy to the list of known proxies that are being tracked
        /// by this Rock instance.
        /// </summary>
        /// <param name="proxy">The proxy to be tracked.</param>
        private static void AddProxy( PrinterProxySocket proxy )
        {
            lock ( _lock )
            {
                proxy.Closed += Proxy_Closed;

                if ( _proxies.TryGetValue( proxy.DeviceId, out var proxies ) )
                {
                    proxies.Add( proxy );
                }
                else
                {
                    _proxies.Add( proxy.DeviceId, new List<PrinterProxySocket> { proxy } );
                }

                if ( _monitorCancellationTokenSource == null )
                {
                    _monitorCancellationTokenSource = new CancellationTokenSource();

                    Task.Factory.StartNew( () => MonitorLoopAsync( _monitorCancellationTokenSource.Token ),
                        _monitorCancellationTokenSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default );
                }
            }
        }

        /// <summary>
        /// Remove the proxy from the list of known proxies that are being
        /// tracked by this Rock instance. This should be called when the
        /// connection has closed.
        /// </summary>
        /// <param name="proxy">The proxy to be removed.</param>
        private static void RemoveProxy( PrinterProxySocket proxy )
        {
            lock ( _lock )
            {
                proxy.Closed -= Proxy_Closed;

                if ( _proxies.TryGetValue( proxy.DeviceId, out var proxies ) )
                {
                    proxies.Remove( proxy );

                    if ( proxies.Count == 0 )
                    {
                        _proxies.Remove( proxy.DeviceId );
                    }

                    if ( _proxies.Count == 0 && _monitorCancellationTokenSource != null )
                    {
                        _monitorCancellationTokenSource.Cancel();
                        _monitorCancellationTokenSource = null;
                    }
                }
            }
        }

        /// <summary>
        /// This task will be run in the background to monitor all proxy
        /// connections to this Rock instance. If a proxy is no longer responding
        /// then it will be removed.
        /// </summary>
        /// <param name="stoppingToken">The token to indicate when we should stop monitoring.</param>
        /// <returns>A task that represents the operation.</returns>
        private static async Task MonitorLoopAsync( CancellationToken stoppingToken )
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                await Task.Delay( 10_000, stoppingToken );

                List<PrinterProxySocket> proxies;

                lock ( _lock )
                {
                    proxies = _proxies.Values.SelectMany( list => list ).ToList();
                }

                // Run each ping in its own task. Otherwise we risk a single
                // proxy taking a long time to respond and causing timeouts in
                // the other proxies.
                foreach ( var proxy in proxies )
                {
                    _ = Task.Run( () => PingProxyAsync( proxy, stoppingToken ), stoppingToken );
                }
            }
        }

        /// <summary>
        /// Pings a single proxy to ensure it is still alive and responding. If
        /// not then the proxy will be removed.
        /// </summary>
        /// <param name="proxy">The proxy to be pinged.</param>
        /// <param name="cancellationToken">A cancellation token that will tell us if we should give up.</param>
        /// <returns>A task that represents the operation.</returns>
        private static async Task PingProxyAsync( PrinterProxySocket proxy, CancellationToken cancellationToken )
        {
            try
            {
                var ping = new PrinterProxyMessagePing
                {
                    SentAt = RockDateTime.Now.ToRockDateTimeOffset()
                };

                // If the proxy takes more than 5 seconds to respond then assume
                // it is offline.
                var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
                cts.CancelAfter( 5_000 );

                var pong = await proxy.SendMessageAsync<PrinterProxyResponsePing>( ping, cts.Token );
            }
            catch
            {
                RemoveProxy( proxy );

                try
                {
                    proxy.Abort();
                }
                catch
                {
                    // Silently ignore exceptions.
                }

                throw;
            }
        }

        /// <summary>
        /// Called when a proxy has closed its connection. This will be called
        /// if we close the connection or if the remote endpoint closes the
        /// connection.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The arguments for the event.</param>
        private static void Proxy_Closed( object sender, EventArgs e )
        {
            if ( sender is PrinterProxySocket proxy )
            {
                RemoveProxy( proxy );
            }
        }

        #endregion
    }
}
