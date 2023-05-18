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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.RealTime
{
    /// <summary>
    /// Provides implementation configuration and features for the Rock
    /// RealTime system.
    /// </summary>
    internal abstract class Engine
    {
        #region Fields

        /// <summary>
        /// The rock instance identifier. This value is unique for every Rock
        /// installation - but is the same across all servers in a single farm.
        /// This is used to prefix all channel names to allow for sharing
        /// cloud RealTime providers.
        /// </summary>
        private static readonly Lazy<string> _rockInstanceId = new Lazy<string>( () => Rock.Web.SystemSettings.GetRockInstanceId().ToString() );

        /// <summary>
        /// The lazy-initialized list of registered topics.
        /// </summary>
        private readonly Lazy<List<TopicConfiguration>> _registeredTopics;

        /// <summary>
        /// The various state holder dictionaries for connections. This
        /// are valid for the entire lifetime of the connection.
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> _clientStates = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the topics that were found through reflection and registered
        /// in the system for use in real-time communication.
        /// </summary>
        protected List<TopicConfiguration> RegisteredTopics => _registeredTopics.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Engine"/> and sets
        /// all values to defaults.
        /// </summary>
        public Engine()
        {
            _registeredTopics = new Lazy<List<TopicConfiguration>>( BuildTopics );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the registered topics. This is called by the lazy initializer
        /// so that we don't waste CPU cycles while Rock is starting.
        /// </summary>
        /// <returns>A list of <see cref="TopicConfiguration"/> objects that describe all the topics that were found.</returns>
        private List<TopicConfiguration> BuildTopics()
        {
            var types = Rock.Reflection.FindTypes( typeof( Topic<> ) ).Values;
            var topics = new List<TopicConfiguration>();

            foreach ( var type in types )
            {
                try
                {
                    topics.Add( GetTopicConfiguration( type ) );
                }
                catch ( Exception ex )
                {
                    var loggedException = new Exception( $"Error while trying to register real-time topic {type.FullName}.", ex );
                    Rock.Model.ExceptionLogService.LogException( loggedException );
                }
            }

            return topics;
        }

        /// <summary>
        /// Gets the topic configurations. This should only be used for debugging.
        /// </summary>
        /// <returns>An enumeration of all the topic configurations.</returns>
        internal IEnumerable<TopicConfiguration> GetTopicConfigurations()
        {
            return RegisteredTopics.ToList();
        }

        /// <summary>
        /// Get the context to send messages to connections on a topic.
        /// </summary>
        /// <typeparam name="TTopicClient">The type that identifies the interface associated with the topic.</typeparam>
        /// <returns>An instance of <see cref="ITopicContext{T}"/> scoped to the interface <typeparamref name="TTopicClient"/>.</returns>
        public ITopicContext<TTopicClient> GetTopicContext<TTopicClient>()
            where TTopicClient : class
        {
            var clientInterfaceType = typeof( TTopicClient );

            var topicConfiguration = RegisteredTopics
                .FirstOrDefault( tc => tc.ClientInterfaceType == clientInterfaceType );

            if ( topicConfiguration == null )
            {
                throw new Exception( $"Topic for interface '{clientInterfaceType.FullName}' was not found." );
            }

            return ( ITopicContext<TTopicClient> ) topicConfiguration.TopicContext;
        }

        /// <summary>
        /// Get an instance of the topic specified by its identifier. Each time
        /// an incoming message from a connection is handled a new instance
        /// is created to respond to the message. This is the same way as the
        /// base SignalR works.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="topicIdentifier">The identifier of the topic that should be created.</param>
        /// <returns>A new instance of the topic class that will handle the request.</returns>
        public ITopicInternal GetTopicInstance( object realTimeHub, string topicIdentifier )
        {
            var topicConfiguration = RegisteredTopics
                .FirstOrDefault( tc => tc.TopicIdentifier == topicIdentifier );

            if ( topicConfiguration == null )
            {
                return null;
            }

            var topicInstance = ( ITopicInternal ) Activator.CreateInstance( topicConfiguration.TopicType );

            ConfigureTopicInstance( realTimeHub, topicConfiguration, topicInstance );

            return topicInstance;
        }

        /// <summary>
        /// Handles a request for a client connection to connect to a specific
        /// topic. This will track the connection and then call the topic's
        /// <see cref="Topic{T}.OnConnectedAsync"/> method.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="topicIdentifier">The topic identifier that should be connected to.</param>
        /// <param name="connectionIdentifier">The identifier of the connection that should be connected to the topic.</param>
        /// <returns><c>true</c> if the topic was found and connected, <c>false</c> otherwise.</returns>
        public async Task<bool> ConnectToTopic( object realTimeHub, string topicIdentifier, string connectionIdentifier )
        {
            var topicInstance = GetTopicInstance( realTimeHub, topicIdentifier );

            if ( topicInstance == null )
            {
                return false;
            }

            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            var connectedCount = state.ConnectedTopics.IncrementTopic( topicIdentifier );

            if ( connectedCount == 1 )
            {
                try
                {
                    await topicInstance.OnConnectedAsync();
                }
                catch ( Exception ex )
                {
                    state.ConnectedTopics.DecrementTopic( topicIdentifier, out var topicChannels );

                    try
                    {
                        await RemoveFromTopicChannelsAsync( realTimeHub, connectionIdentifier, topicIdentifier, topicChannels );
                    }
                    catch
                    {
                        // Intentionally ignored, this is only best effort.
                    }

                    throw ex;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles a request for a client connection to disconnect from a specific
        /// topic. This will track the disconnection and then call the topic's
        /// <see cref="Topic{T}.OnDisconnectedAsync"/> method.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="topicIdentifier">The topic identifier that should be connected to.</param>
        /// <param name="connectionIdentifier">The identifier of the connection that should be connected to the topic.</param>
        /// <returns><c>true</c> if the topic was found and disconnected, <c>false</c> otherwise.</returns>
        public async Task<bool> DisconnectFromTopicAsync( object realTimeHub, string topicIdentifier, string connectionIdentifier )
        {
            var topicInstance = GetTopicInstance( realTimeHub, topicIdentifier );

            if ( topicInstance == null )
            {
                return false;
            }

            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            if ( state.ConnectedTopics.DecrementTopic( topicIdentifier, out var topicChannels ) > 0 )
            {
                return true;
            }

            try
            {
                await RemoveFromTopicChannelsAsync( realTimeHub, connectionIdentifier, topicIdentifier, topicChannels );
            }
            catch
            {
                // Intentionally ignored, this is only best effort.
            }

            await topicInstance.OnDisconnectedAsync();

            return true;
        }

        /// <summary>
        /// Notifies the engine that a client connection has connected. Handles
        /// common setup of connection data.
        /// </summary>
        /// <param name="realTimeHub">The real time hub.</param>
        /// <param name="connectionIdentifier">The connection identifier.</param>
        public virtual Task ClientConnectedAsync( object realTimeHub, string connectionIdentifier )
        {
            // Force the creation of the connection state.
            GetConnectionState<EngineConnectionState>( connectionIdentifier );

            return Task.CompletedTask;
        }

        /// <summary>
        /// Notifies the engine that a client connection has disconnected. Handles
        /// cleanup such as calling the <see cref="Topic{T}.OnDisconnectedAsync"/>
        /// method on each topic the client has connected to.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="connectionIdentifier">The identifier of the connection that has disconnected.</param>
        public virtual async Task ClientDisconnectedAsync( object realTimeHub, string connectionIdentifier )
        {
            var exceptions = new List<Exception>();

            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            // This is multi-thread safe enough since a client shouldn't be able
            // to send any messages once the Disconnected event has been triggered.
            var topicIdentifiers = state.ConnectedTopics.GetAllConnectedTopics();
            state.ConnectedTopics.Clear();

            foreach ( var topicIdentifier in topicIdentifiers )
            {
                try
                {
                    var topicInstance = GetTopicInstance( realTimeHub, topicIdentifier );

                    if ( topicInstance != null )
                    {
                        await topicInstance.OnDisconnectedAsync();
                    }
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }
            }

            // Clean up the connection state data so we don't leak memory.
            _clientStates.TryRemove( connectionIdentifier, out _ );

            if ( exceptions.Any() )
            {
                throw new AggregateException( "One or more topics threw exceptions while disconnecting.", exceptions );
            }
        }

        /// <summary>
        /// Called whenever a client has been added to a channel. This is used to
        /// track which channels a client is listening on so they can be removed
        /// if the client disconnects from the topic.
        /// </summary>
        /// <param name="connectionIdentifier">The connection identifier.</param>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channelName">Name of the channel.</param>
        public void ClientAddedToChannel( string connectionIdentifier, string topicIdentifier, string channelName )
        {
            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            state.ConnectedTopics.AddTopicChannel( topicIdentifier, channelName );
        }

        /// <summary>
        /// Called whenever a client has been removed from a channel. This is used to
        /// track which channels a client is listening on so they can be removed
        /// if the client disconnects from the topic.
        /// </summary>
        /// <param name="connectionIdentifier">The connection identifier.</param>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channelName">Name of the channel.</param>
        public void ClientRemovedFromChannel( string connectionIdentifier, string topicIdentifier, string channelName )
        {
            var state = GetConnectionState<EngineConnectionState>( connectionIdentifier );

            state.ConnectedTopics.RemoveTopicChannel( topicIdentifier, channelName );
        }

        /// <summary>
        /// Gets a state tracking object for this connection. This object is
        /// unique to each connection. Meaning a single person with two
        /// connections will have two different state objects. The state object
        /// is valid until the client disconnects.
        /// </summary>
        /// <param name="connectionIdentifier">The connection identifier that the state object should be attached to.</param>
        /// <typeparam name="TState">The type of state object.</typeparam>
        /// <returns>An instance of the <typeparamref name="TState"/> object.</returns>
        public TState GetConnectionState<TState>( string connectionIdentifier )
            where TState : class, new()
        {
            return ( TState ) _clientStates.GetOrAdd( connectionIdentifier, _ => new ConcurrentDictionary<Type, object>() )
                .GetOrAdd( typeof( TState ), _ => new TState() );
        }

        /// <summary>
        /// Determines whether the connection has state <typeparamref name="TState"/>.
        /// This check is performed without actually creating the state if it does
        /// not already exist.
        /// </summary>
        /// <typeparam name="TState">The type of state object.</typeparam>
        /// <param name="connectionIdentifier">The connection identifier that the state object should be checked on.</param>
        /// <returns><c>true</c> if the state object exists on the connection; otherwise, <c>false</c>.</returns>
        public bool HasConnectionState<TState>( string connectionIdentifier )
        {
            if ( !_clientStates.TryGetValue( connectionIdentifier, out var clientState ) )
            {
                return false;
            }

            return clientState.ContainsKey( typeof( TState ) );
        }

        /// <summary>
        /// Execute a message on a topic. 
        /// </summary>
        /// <param name="realTimeHub">The real time hub that will be used to initialize the topic.</param>
        /// <param name="connectionId">The identifier of the connection that sent the message.</param>
        /// <param name="topicIdentifier">The topic identifier the message was sent to.</param>
        /// <param name="messageName">The name of the message.</param>
        /// <param name="messageParameters">The message parameters.</param>
        /// <param name="convertParameter">A function to convert the parameters into the specified type.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the result of the method invocation.</returns>
        internal static async Task<object> ExecuteTopicMessageAsync( object realTimeHub, string connectionId, string topicIdentifier, string messageName, object[] messageParameters, Func<object, Type, object> convertParameter )
        {
            object topicInstance;

            // Ensure the connection has joined the topic.
            var state = RealTimeHelper.Engine.GetConnectionState<EngineConnectionState>( connectionId );

            if ( !state.ConnectedTopics.IsConnected( topicIdentifier ) )
            {
                throw new RealTimeException( $"Topic '{topicIdentifier}' must be joined before sending messages to it." );
            }

            // Initialize the topic instance.
            topicInstance = RealTimeHelper.GetTopicInstance( realTimeHub, topicIdentifier );

            if ( topicInstance == null )
            {
                throw new RealTimeException( $"RealTime topic '{topicIdentifier}' was not found." );
            }

            // Find all method that match the message name. Case does not matter.
            var matchingMethods = topicInstance.GetType()
                .GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .Where( m => m.Name.Equals( messageName, StringComparison.OrdinalIgnoreCase ) )
                .ToList();

            if ( matchingMethods.Count <= 0 )
            {
                throw new RealTimeException( $"Message '{messageName}' was not found on topic '{topicIdentifier}'." );
            }
            else if ( matchingMethods.Count > 1 )
            {
                throw new RealTimeException( $"Message '{messageName}' matched multiple methods on topic '{topicIdentifier}'." );
            }

            // Get the details of the method invocation.
            var mi = matchingMethods[0];
            var methodParameters = mi.GetParameters();
            var parms = new object[methodParameters.Length];

            // Convert all the message parameters into method parameters.
            try
            {
                for ( int i = 0; i < methodParameters.Length; i++ )
                {
                    if ( methodParameters[i].ParameterType == typeof( CancellationToken ) )
                    {
                        parms[i] = CancellationToken.None;
                    }
                    else
                    {
                        parms[i] = convertParameter( messageParameters[i], methodParameters[i].ParameterType );
                    }
                }
            }
            catch
            {
                throw new RealTimeException( $"Incorrect parameters passed to message '{messageName}'." );
            }

            // Try to invoke the method. If we get an exception then unwrap it and
            // throw the original exception.
            try
            {
                var result = mi.Invoke( topicInstance, parms );

                if ( result is Task resultTask )
                {
                    await resultTask;

                    // Task<T> is not covariant, so we can't just cast to Task<object>.
                    if ( resultTask.GetType().IsGenericType )
                    {
                        var piResult = resultTask.GetType().GetProperty( "Result" );
                        result = piResult.GetValue( resultTask );
                    }
                    else
                    {
                        result = null;
                    }
                }

                return result;
            }
            catch ( TargetInvocationException ex )
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Gets the qualified channel name that should be used with the Engine
        /// implementation to communicate with all connections to a topic.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic.</param>
        /// <returns>A string that represents the fully qualified channel name to be used.</returns>
        public static string GetQualifiedAllChannelName( string topicIdentifier )
        {
            return $"{_rockInstanceId.Value}-{topicIdentifier}-rock:all";
        }

        /// <summary>
        /// Gets the qualified channel name that should be used with the Engine
        /// implementation.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic this channel is associated with.</param>
        /// <param name="channelName">Name of the channel.</param>
        /// <returns>A string that represents the fully qualified channel name to be used.</returns>
        public static string GetQualifiedChannelName( string topicIdentifier, string channelName )
        {
            return $"{_rockInstanceId.Value}-{topicIdentifier}-{channelName}";
        }

        /// <summary>
        /// Gets the base channel name to use for the specified person.
        /// </summary>
        /// <param name="personAliasId">The identifier of the Person object.</param>
        /// <returns>A string that represents the channel name to be used.</returns>
        public static string GetPersonChannelName( int personAliasId )
        {
            return $"rock:person:{personAliasId}";
        }

        /// <summary>
        /// Gets the qualified channel name that should be used with the Engine
        /// implementation for a personal channel. This channel can be used to
        /// send a message to all connections belonging to the specified person.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic this person is associated with.</param>
        /// <param name="personId">The identifier of the Person object.</param>
        /// <returns>A string that represents the fully qualified channel name to be used.</returns>
        public static string GetQualifiedPersonChannelName( string topicIdentifier, int personId )
        {
            return $"{_rockInstanceId.Value}-{topicIdentifier}-rock:person:{personId}";
        }

        /// <summary>
        /// Gets the base channel name to use for the specified visitor.
        /// </summary>
        /// <param name="personAliasId">The identifier of the PersonAlias object.</param>
        /// <returns>A string that represents the channel name to be used.</returns>
        public static string GetVisitorChannelName( int personAliasId )
        {
            return $"rock:visitor:{personAliasId}";
        }

        /// <summary>
        /// Gets the qualified channel name that should be used with the Engine
        /// implementation for a visitor channel. This channel can be used to
        /// send a message to all connections belonging to the specified visitor.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic this visitor is associated with.</param>
        /// <param name="visitorAliasId">The identifier of the PersonAlias object.</param>
        /// <returns>A string that represents the fully qualified channel name to be used.</returns>
        public static string GetQualifiedVisitorChannelName( string topicIdentifier, int visitorAliasId )
        {
            return $"{_rockInstanceId.Value}-{topicIdentifier}-rock:visitor:{visitorAliasId}";
        }

        /// <summary>
        /// Gets the topic configuration for the given topic type.
        /// </summary>
        /// <param name="topicType">The <see cref="Type"/> that describes the topic to be configured.</param>
        /// <returns>A new instance of <see cref="TopicConfiguration"/> that describes the topic.</returns>
        protected abstract TopicConfiguration GetTopicConfiguration( Type topicType );

        /// <summary>
        /// Performs additional configuration of a topic instance. This is used
        /// to provide custom property values for the different implementations.
        /// </summary>
        /// <param name="realTimeHub">The real hub used to communicate with clients.</param>
        /// <param name="topicConfiguration">The configuration of the topic being setup.</param>
        /// <param name="topicInstance">The instance object that will handle incoming topic messages.</param>
        protected abstract void ConfigureTopicInstance( object realTimeHub, TopicConfiguration topicConfiguration, ITopicInternal topicInstance );

        /// <summary>
        /// Sends a message to the specified clients scoped to the topic.
        /// </summary>
        /// <param name="proxy">The proxy object that will send the actual message.</param>
        /// <param name="topicIdentifier">The identifier of the topic that the message will be scoped to.</param>
        /// <param name="messageName">The name that identifies the message to be sent.</param>
        /// <param name="parameters">The parameters to be passed in the message.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        public abstract Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken );

        /// <summary>
        /// Removes the connection from the specified topic channels. Called when
        /// a connection requests to be disconnected from a topic.
        /// </summary>
        /// <param name="realTimeHub">The real time hub.</param>
        /// <param name="connectionIdentifier">The identifier of the connection to be removed.</param>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channelNames">The channel names to be removed from.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        protected abstract Task RemoveFromTopicChannelsAsync( object realTimeHub, string connectionIdentifier, string topicIdentifier, IEnumerable<string> channelNames );

        #endregion
    }
}
