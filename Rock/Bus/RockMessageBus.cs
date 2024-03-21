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
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Rock.Bus.Consumer;
using Rock.Bus.Faults;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Bus.Statistics;
using Rock.Bus.Transport;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Bus
{
    /// <summary>
    /// Rock Bus Process Controls: Start the bus
    /// </summary>
    public static class RockMessageBus
    {
        /// <summary>
        /// The stat observer
        /// </summary>
        private static StatObserver _statObserver = new StatObserver();

        /// <summary>
        /// The receive fault observer
        /// </summary>
        private static ReceiveFaultObserver _receiveFaultObserver = new ReceiveFaultObserver();

        /// <summary>
        /// Is the bus started?
        /// </summary>
        private static bool _isBusStarted = false;

        /// <summary>
        /// Wait lock for the startup process.
        /// </summary>
        private static SemaphoreSlim _initSemaphore = new SemaphoreSlim( 1, 1 );

        private static TaskCompletionSource<bool> _busStartupCompleted = new TaskCompletionSource<bool>();

        /// <summary>
        /// The bus
        /// </summary>
        private static IBusControl _bus = null;

        /// <summary>
        /// The transport component
        /// </summary>
        private static TransportComponent _transportComponent = null;

        /// <summary>
        /// Gets the stat log.
        /// </summary>
        public static StatLog StatLog { get; } = new StatLog();

        /// <summary>
        /// If <see cref="IsRockStarted"/> has been <c>false</c> for more than 20 minutes. An error should be logged if attempting a publish or consume.
        /// </summary>
        public const int MAX_SECONDS_SINCE_STARTTIME_LOG_ERROR = 20 * 60;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is rock started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is rock started; otherwise, <c>false</c>.
        /// </value>
        public static bool IsRockStarted { get; set; } = false;

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <returns></returns>
        public static string NodeName
        {
            get
            {
                if ( !_nodeName.IsNullOrWhiteSpace() )
                {
                    return _nodeName;
                }

                _nodeName = ConfigurationManager.AppSettings["NodeName"];

                if ( _nodeName.IsNullOrWhiteSpace() )
                {
                    _nodeName = Environment.MachineName;
                }

                if ( _nodeName.IsNullOrWhiteSpace() )
                {
                    _nodeName = Guid.NewGuid().ToString();
                }

                return _nodeName;
            }
        }

        private static string _nodeName;

        /// <summary>
        /// Gets a value indicating whether this instance is using the in memory transport.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in memory transport; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInMemoryTransport => _transportComponent is InMemory;

        /// <summary>
        /// Starts this bus.
        /// </summary>
        public static async Task StartAsync()
        {
            var components = TransportContainer.Instance.Components.Select( c => c.Value.Value );
            var inMemoryTransport = components.FirstOrDefault( c => c is InMemory );
            var activeNonInMemoryTransports = components.Where( c => c != inMemoryTransport && c.IsActive );

            // If something besides in-memory transport is active, then it will be the transport.
            if ( activeNonInMemoryTransports.Any() )
            {
                _transportComponent = activeNonInMemoryTransports.First();
            }
            else
            {
                // If there is no active transport, in-memory is used. Effectively, in-memory cannot
                // be deactivated because it would cause Rock to not function.
                _transportComponent = inMemoryTransport;
            }

            try
            {
                await ConfigureAndStartBusAsync();
            }
            catch ( Exception e )
            {
                // An error occured with the chosen transport. Try one more time with in-memory to see if Rock
                // can still start to allow UI based configuration
                if ( _transportComponent == inMemoryTransport )
                {
                    // Already tried in-memory and it is what threw the exception
                    throw;
                }

                // Switch to in-memory
                var originalTransport = _transportComponent;
                _transportComponent = inMemoryTransport;

                // Start-up with in-memory
                await ConfigureAndStartBusAsync();

                // Log that the original transport did not work
                ExceptionLogService.LogException( new BusException( $"Could not start the message bus transport: {originalTransport.GetType().Name}", e ) );
            }

            if ( _transportComponent == inMemoryTransport && !inMemoryTransport.IsActive )
            {
                // Set the in memory transport as active for the UI since it is being used
                using ( var rockContext = new RockContext() )
                {
                    inMemoryTransport.SetAttributeValue( InMemory.BaseAttributeKey.Active, true.ToString() );
                    inMemoryTransport.SaveAttributeValue( InMemory.BaseAttributeKey.Active, rockContext );
                }
            }
        }

        /// <summary>
        /// Starts the unit test in-memory bus.
        /// </summary>
        internal static async Task StartTestMemoryBusAsync()
        {
            _transportComponent = new InMemory( false );

            await ConfigureAndStartBusAsync();
        }

        /// <summary>
        /// Determines whether the message was sent by this Rock instance.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if [is from self] [the specified message]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFromSelf<TQueue>( IRockMessage<TQueue> message )
            where TQueue : IPublishEventQueue, new()
        {
            return message?.SenderNodeName == NodeName;
        }

        /// <summary>
        /// Publishes (Broadcasts) the event message. This will send the message to the MessageQueue, and Queue will broadcast it to <b>all endpoints.</b>
        /// <para>
        /// See https://triumph.slab.com/posts/event-bus-65nk4duh#hjiec-publish-method for how this works
        /// </para>
        /// </summary>
        /// <param name="message">The message.</param>
        public static Task PublishAsync<TQueue, TMessage>( TMessage message )
            where TQueue : IPublishEventQueue, new()
            where TMessage : class, IEventMessage<TQueue>
        {
            return PublishAsync( message, typeof( TMessage ) );
        }

        /// <inheritdoc cref="PublishAsync{TQueue, TMessage}(TMessage)"/>
        public static Task PublishAsync<TQueue>( IEventMessage<TQueue> message, Type messageType )
            where TQueue : IPublishEventQueue, new()
        {
            message.SenderNodeName = NodeName;

            // NOTE: Use Task.Run to wrap an async instead of directly using async, otherwise async will get an exception if it isn't done before the HttpContext is disposed.
            return Task.Run( async () =>
            {
                if ( !IsReady() && _busStartupCompleted != null )
                {
                    /* 06/21/2022 MP
                      
                    If the bus is still in the process of starting, we'll wait
                    for the bus to be started, and then do the publish. This can
                    happen since CacheUpdateMessages can be published prior to
                    the MessageBus getting started.
                     
                    */

                    // Wait for up to 45 seconds.
                    await Task.WhenAny( _busStartupCompleted.Task, Task.Delay( maxStartupWaitTimeSeconds * 1000 ) );
                }

                if ( !IsReady() )
                {
                    // Just in case it still isn't ready, log an exception.
                    ExceptionLogService.LogException( new BusException( $"A message publish attempt could not be published before the message bus was able to be ready: {RockMessage.GetLogString( message )}" ) );
                    return;
                }

                await _bus.Publish( message, messageType, context =>
                {
                    context.TimeToLive = RockQueue.GetTimeToLive<TQueue>();
                } );
            } );
        }

        /// <summary>
        /// Sends the command message. This will send the message to the MessageQueue, and Queue will direct the command to a <b>single endpoint</b>.
        /// <para>
        /// See https://triumph.slab.com/posts/event-bus-65nk4duh#hyx3e-send-method for how this works
        /// </para>
        /// </summary>
        /// <param name="message">The message.</param>
        public static Task SendAsync<TQueue, TMessage>( TMessage message )
            where TQueue : ISendCommandQueue, new()
            where TMessage : class, ICommandMessage<TQueue>
        {
            return SendAsync( message, typeof( TMessage ) );
        }

        /// <inheritdoc cref="SendAsync{TQueue, TMessage}(TMessage)"/>
        public static Task SendAsync<TQueue>( ICommandMessage<TQueue> message, Type messageType )
            where TQueue : ISendCommandQueue, new()
        {
            RockLogger.Log.Debug( RockLogDomains.Core, "Send Message Async: {@message} Message Type: {1}", message, messageType );

            if ( !IsReady() )
            {
                ExceptionLogService.LogException( $"A message was sent before the message bus was ready: {RockMessage.GetLogString( message )}" );
                return Task.CompletedTask;
            }

            var queue = RockQueue.Get<TQueue>();
            var endpoint = _transportComponent.GetSendEndpoint( _bus, queue.NameForConfiguration );
            message.SenderNodeName = NodeName;

            // NOTE: Use Task.Run to wrap an async instead of directly using async, otherwise async will get an exception if it isn't done before the HttpContext is disposed.
            return Task.Run( async () =>
            {
                await endpoint.Send( message, messageType, context =>
                {
                    context.TimeToLive = RockQueue.GetTimeToLive( queue );
                } );
            } );
        }

        private const int maxStartupWaitTimeSeconds = 45;

        /// <summary>
        /// Configures and starts the bus.
        /// </summary>
        /// <returns></returns>
        private async static Task ConfigureAndStartBusAsync()
        {
            // If the startup process is being executed, wait here.
            await _initSemaphore.WaitAsync();
            try
            {
                // If the bus has been initialized by an awaited caller, exit now.
                if ( IsReady() )
                {
                    return;
                }
                if ( _transportComponent == null )
                {
                    throw new Exception( "An active transport component is required for Rock to run correctly" );
                }

                _bus = _transportComponent.GetBusControl( RockConsumer.ConfigureRockConsumers );
                _bus.ConnectConsumeObserver( _statObserver );
                _bus.ConnectReceiveObserver( _receiveFaultObserver );

                // Allow the bus to try to connect for some seconds at most
                var cancelToken = new CancellationTokenSource();
                var task = _bus.StartAsync( cancelToken.Token );

                var delay = Task.Delay( TimeSpan.FromSeconds( maxStartupWaitTimeSeconds ) );

                if ( await Task.WhenAny( task, delay ) == task )
                {
                    // Task completed within timeout.
                    // Consider that the task may have faulted or been canceled.
                    // We re-await the task so that any exceptions/cancellation is rethrown.
                    // https://stackoverflow.com/a/11191070/13215483
                    await task;
                }
                else
                {
                    // The bus did not connect after some seconds
                    cancelToken.Cancel();
                    throw new Exception( $"The bus failed to connect using {_transportComponent.GetType().Name} within {maxStartupWaitTimeSeconds} seconds" );
                }

                _isBusStarted = true;
                _busStartupCompleted.SetResult( true );
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        /// <summary>
        /// Determines whether this instance is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReady()
        {
            return _isBusStarted && _transportComponent != null && _bus != null;
        }

        /// <summary>
        /// Gets the name of the transport.
        /// </summary>
        /// <returns></returns>
        public static string GetTransportName()
        {
            return _transportComponent?.EntityType?.FriendlyName;
        }

        /// <summary>
        /// Gets a completed task. This is a helper to be used for when there is a Task
        /// return type and no task is really necessary. The task returned is immediately
        /// resolved.
        /// </summary>
        /// <returns></returns>
        internal static Task GetCompletedTask()
        {
            return Task.Delay( 0 );
        }
    }
}
