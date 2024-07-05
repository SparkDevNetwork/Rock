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
using System.Net.WebSockets;

using Microsoft.Extensions.Options;

using Polly.Retry;

using Polly;

namespace Rock.PrinterProxy.Service;

/// <summary>
/// This is the main background worker for the printer proxy. It is in charge
/// of making sure the proxy stays connected and reconnects when the options
/// have changed.
/// </summary>
class ProxyWorker : BackgroundService
{
    #region Fields

    /// <summary>
    /// The service provider that will be used to create the proxy instances.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The logger that any messages will be logged to.
    /// </summary>
    private readonly ILogger<ProxyWorker> _logger;

    /// <summary>
    /// Monitors for changes to the proxy options.
    /// </summary>
    private readonly IOptionsMonitor<PrinterProxyOptions> _optionsMonitor;

    /// <summary>
    /// Synchronization object for the worker.
    /// </summary>
    private readonly SemaphoreSlim _lock = new( 1 );

    /// <summary>
    /// The current proxy instance that is running.
    /// </summary>
    private ProxyClientWebSocket? _proxy;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyWorker"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public ProxyWorker( IServiceProvider serviceProvider )
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<ProxyWorker>>();
        _optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<PrinterProxyOptions>>();

        _optionsMonitor.OnChange( OnConfigurationChanged );
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        while ( !stoppingToken.IsCancellationRequested )
        {
            try
            {
                await AttemptStartProxyAsync( stoppingToken );
            }
            catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
            {
                break;
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to connect to Rock server." );
            }

            await Task.Delay( 5000, stoppingToken );
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync( CancellationToken cancellationToken )
    {
        await AttemptStopProxyAsync( cancellationToken );
        await base.StopAsync( cancellationToken );
    }

    /// <summary>
    /// Attempts to start the proxy instance if it is not already running.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the operation.</returns>
    private async Task AttemptStartProxyAsync( CancellationToken cancellationToken )
    {
        if ( _proxy != null )
        {
            return;
        }

        var ws = await ConnectAsync( cancellationToken );
        var proxy = new ProxyClientWebSocket( ws, _logger );

        proxy.Closed += Proxy_Closed;

        await _lock.WaitAsync( cancellationToken );

        try
        {
            if ( _proxy == null )
            {
                _ = await Task.Factory.StartNew( () => proxy.RunAsync( cancellationToken ), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default );
                _proxy = proxy;
            }
            else
            {
                // Close the socket and forget about it, we are already connected.
                _ = proxy.CloseAsync( cancellationToken );
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Attempts to stop the proxy instance if it is running.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the operation.</returns>
    private async Task AttemptStopProxyAsync( CancellationToken cancellationToken = default )
    {
        await _lock.WaitAsync( cancellationToken );

        try
        {
            if ( _proxy != null )
            {
                await _proxy.CloseAsync( cancellationToken );
            }
        }
        finally
        {
            _proxy = null;
            _lock.Release();
        }
    }

    /// <summary>
    /// Connect to the Rock instance. This will continue to retry until
    /// cancelled if the initial connect fails.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to signal that the connect should be aborted.</param>
    /// <returns>An instance of <see cref="ClientWebSocket"/> that has been connected.</returns>
    private async Task<ClientWebSocket> ConnectAsync( CancellationToken cancellationToken )
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry( new RetryStrategyOptions
            {
                MaxDelay = TimeSpan.FromSeconds( 60 ),
                MaxRetryAttempts = int.MaxValue,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds( 5 )
            } )
            .AddTimeout( TimeSpan.FromSeconds( 10 ) )
            .Build();

        return await pipeline.ExecuteAsync( ConnectOnceAsync, cancellationToken );
    }

    /// <summary>
    /// Attempts to connect one time to the server using the current options.
    /// An exception will be thrown if the connect failed.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to signal that the connect should be aborted.</param>
    /// <returns>An instance of <see cref="ClientWebSocket"/> in a connected state.</returns>
    private async ValueTask<ClientWebSocket> ConnectOnceAsync( CancellationToken cancellationToken )
    {
        var options = _optionsMonitor.CurrentValue;
        var baseUrl = options.Url;
        var name = options.Name;

        if ( string.IsNullOrWhiteSpace( baseUrl ) || string.IsNullOrWhiteSpace( options.Id ) )
        {
            _logger.LogError( "Missing required configuration values." );
            throw new Exception( "Missing required configuration values." );
        }

        _logger.LogInformation( "Connecting to server {server}.", baseUrl );

        if ( !baseUrl.Contains( "://" ) )
        {
            baseUrl = "wss://" + baseUrl;
        }
        else
        {
            baseUrl = baseUrl
                .Replace( "https://", "wss://" )
                .Replace( "http://", "ws://" );
        }

        if ( string.IsNullOrWhiteSpace( name ) )
        {
            name = Environment.MachineName;
        }

        var uri = new Uri( baseUrl );
        uri = new Uri( uri, $"api/v2/checkin/printerproxy/{options.Id}?name={name}&priority={options.Priority}" );

        var ws = new ClientWebSocket();

        try
        {
            await ws.ConnectAsync( uri, cancellationToken ).ConfigureAwait( false );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Unable to connect to server {server}.", baseUrl );
            throw;
        }

        _logger.LogInformation( "Established connection to server {server}.", baseUrl );

        return ws;
    }

    /// <summary>
    /// Handles the event when the proxy is closed. Make sure this is our
    /// current proxy and then change us to disconnected state.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    private void Proxy_Closed( object? sender, EventArgs e )
    {
        if ( sender is ProxyClientWebSocket proxy )
        {
            Task.Run( async () =>
            {
                await _lock.WaitAsync();

                try
                {
                    if ( proxy == _proxy )
                    {
                        _logger.LogInformation( "Disconnected from server." );
                        proxy.Closed -= Proxy_Closed;
                        _proxy = null;
                    }
                }
                finally
                {
                    _lock.Release();
                }
            } );
        }
    }

    /// <summary>
    /// Event handler for when the printer configuration is changed.
    /// </summary>
    /// <param name="options">The new printer options.</param>
    private void OnConfigurationChanged( PrinterProxyOptions options )
    {
        _logger.LogInformation( "Configuration changed, restarting proxy." );

        Task.Run( () => AttemptStopProxyAsync() );
    }
}
