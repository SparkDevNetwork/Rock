using System.Text.RegularExpressions;

using RestSharp;

using Rock.RealTime.Client;
using Rock.RealTime.Client.Engines;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace InteractiveExperienceLoadTest
{
    /// <summary>
    /// A single client to simulate a connection to the interactive
    /// experience system.
    /// </summary>
    internal class Client
    {
        #region Fields

        /// <summary>
        /// The random number generator to use for all clients.
        /// </summary>
        private static readonly Random _rng = Random.Shared;

        /// <summary>
        /// The lock object for <see cref="_actions"/>.
        /// </summary>
        private static readonly object _actionsLock = new object();

        /// <summary>
        /// The list of known actions from the server.
        /// </summary>
        private static readonly List<string> _actions = new List<string>() { string.Empty };

        #endregion

        #region Properties

        /// <summary>
        /// The configuration data provided by the individual.
        /// </summary>
        public Configuration Configuration { get; }

        /// <summary>
        /// The client to use when talking to the Rock API server.
        /// </summary>
        public IRestClient RestClient { get; }

        /// <summary>
        /// The realtime engine to use for talking to the Rock server.
        /// </summary>
        public Engine Engine { get; }

        /// <summary>
        /// The topic we are connected to.
        /// </summary>
        public Topic? Topic { get; private set; }

        /// <summary>
        /// The current state of this instance.
        /// </summary>
        public ClientState State { get; private set; }

        /// <summary>
        /// The current action being displayed.
        /// </summary>
        public int CurrentAction { get; private set; }

        /// <summary>
        /// Any error message if <see cref="State"/> is <see cref="ClientState.Error"/>.
        /// </summary>
        public string? Error { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Client"/>.
        /// </summary>
        /// <param name="configuration">The configuration options.</param>
        /// <param name="restClient">The rest client.</param>
        public Client( Configuration configuration, IRestClient restClient )
        {
            Configuration = configuration;
            RestClient = restClient;
            Engine = new AspNetEngine
            {
                CookieContainer = restClient.Options.CookieContainer!
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the current action displayed for this client.
        /// </summary>
        /// <param name="actionId">The identifier of the action or an empty string.</param>
        private void SetCurrentAction( string actionId )
        {
            if ( string.IsNullOrEmpty( actionId ) )
            {
                CurrentAction = 0;
                return;
            }

            int index;

            lock ( _actionsLock )
            {
                index = _actions.IndexOf( actionId );

                if ( index == -1 )
                {
                    index = _actions.Count;
                    _actions.Add( actionId );
                }
            }

            CurrentAction = index;
        }

        /// <summary>
        /// Gets a token to use when connecting to the realtime engine. This
        /// scrapes the live experience page looking for the value.
        /// </summary>
        /// <returns>A string that represents the token or an empty string if it could not be found.</returns>
        async Task<string> GetExperienceToken()
        {
            if ( Configuration.LivePageUri == null )
            {
                return string.Empty;
            }

            var request = new RestRequest( Configuration.LivePageUri, Method.Get );

            request.AddQueryParameter( "InteractiveExperienceOccurrenceId", Configuration.OccurrenceId ?? "0" );

            var response = await RestClient.ExecuteAsync( request );

            if ( !response.IsSuccessful || string.IsNullOrWhiteSpace( response.Content ) )
            {
                return string.Empty;
            }

            var regex = new Regex( "\"experienceToken\"\\s*:\\s*\"([^\"]+)\"" );
            var match = regex.Match( response.Content );

            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Connects to the server and performs all login tasks.
        /// </summary>
        /// <param name="cancellationToken">A token to signal when we should abort the connection.</param>
        /// <returns>A task the represents the operation.</returns>
        public async Task ConnectAsync( CancellationToken cancellationToken )
        {
            if ( cancellationToken.IsCancellationRequested )
            {
                return;
            }

            if ( State != ClientState.Initializing || Configuration.BaseUri == null )
            {
                if ( State != ClientState.Error )
                {
                    Error = "Invalid client state for connect.";
                    State = ClientState.Error;
                }

                return;
            }

            try
            {
                State = ClientState.Connecting;

                await Task.Delay( _rng.Next( Configuration.DelayVariance ), cancellationToken );

                var tokenTask = GetExperienceToken();

                await Engine.ConnectAsync( Configuration.BaseUri, cancellationToken );
                Topic = await Engine.JoinTopicAsync( "Rock.RealTime.Topics.InteractiveExperienceParticipantTopic", cancellationToken );
                Topic.OnDisconnected( () => State = ClientState.Disconnected );
                Topic.On<string, string, ActionRenderConfigurationBag>( "ShowAction", ( occurrenceIdKey, actionIdKey, actionData ) => SetCurrentAction( actionIdKey ) );
                Topic.On<string>( "ClearActions", ( occurrenceIdKey ) => SetCurrentAction( "" ) );

                var joinResponse = await Topic.Server.InvokeAsync<JoinExperienceResponseBag>( "JoinExperience", new object[] { await tokenTask }, cancellationToken );

                _ = Task.Factory.StartNew( async () =>
                {
                    while ( !cancellationToken.IsCancellationRequested )
                    {
                        try
                        {
                            await Task.Delay( 30_000, cancellationToken );
                            await Topic.Server.InvokeAsync<object>( "PingExperience", new object[] { joinResponse.OccurrenceIdKey }, cancellationToken );
                        }
                        catch ( TaskCanceledException )
                        {
                            // Intentionally ignore.
                        }
                    }
                } );

                SetCurrentAction( joinResponse.CurrentActionIdKey );
                State = ClientState.Connected;
            }
            catch ( Exception ex )
            {
                Error = ex.Message;
                State = ClientState.Error;
            }
        }

        /// <summary>
        /// Disconnects from the server and performs any cleanup.
        /// </summary>
        /// <returns>A task that represents the operation.</returns>
        public async Task DisconnectAsync()
        {
            if ( State == ClientState.Error )
            {
                return;
            }

            try
            {
                await Task.Delay( _rng.Next( Configuration.DelayVariance ), CancellationToken.None );
                await Engine.DisconnectAsync();

                State = ClientState.Disconnected;
            }
            catch ( Exception ex )
            {
                Error = ex.Message;
                State = ClientState.Error;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            switch ( State )
            {
                case ClientState.Initializing:
                    return "I";

                case ClientState.Connecting:
                    return "C";

                case ClientState.Connected:
                    return $"{CurrentAction:X}";

                case ClientState.Disconnected:
                    return "D";

                case ClientState.Error:
                    return "E";
            }

            return "E";
        }

        #endregion
    }
}
