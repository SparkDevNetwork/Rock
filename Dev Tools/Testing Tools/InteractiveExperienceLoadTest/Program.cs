using System.Net;
using System.Text;

using RestSharp;

namespace InteractiveExperienceLoadTest
{
    internal class Program
    {
        static async Task Main( string[] args )
        {
            var cfg = ReadConfig();

            if ( cfg == null )
            {
                return;
            }

            Console.WriteLine();
            Console.Write( "Press enter to begin test..." );
            Console.ReadLine();
            
            // Initialize the Rest API client.
            var restClient = new RestClient( new RestClientOptions
            {
                BaseUrl = cfg.BaseUri,
                CookieContainer = new CookieContainer()
            } );

            // Attempt to login.
            if ( !await LoginAsync( restClient, cfg ) )
            {
                Console.WriteLine( "Login failed." );
                return;
            }

            // Initialize all clients.
            var clients = new List<Client>();

            for ( int i = 0; i < cfg.ClientCount; i++ )
            {
                clients.Add( new Client( cfg, restClient ) );
            }

            // Capture the CTRL-C sequence so we can gracefully quit.
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += ( s, e ) =>
            {
                cts.Cancel();
                e.Cancel = true;
            };

            // Connect all clients.
            await ForEachClientAsync( clients, c => c.ConnectAsync( cts.Token ), cfg.ConcurrencyLimit, cts.Token );

            // Wait until CTRL-C is pressed and update the screen.
            while ( !cts.IsCancellationRequested )
            {
                UpdateClientDisplayStatus( clients );

                Console.WriteLine();
                Console.WriteLine( "Press CTRL-C to stop." );

                await Task.Delay( 250, CancellationToken.None );
            }

            // Disconnect all the clients.
            await ForEachClientAsync( clients, c => c.DisconnectAsync(), cfg.ConcurrencyLimit, CancellationToken.None );

            if ( clients.Any( c => c.State == ClientState.Error ) )
            {
                Console.WriteLine();
                Console.WriteLine( "The following errors occurrred:" );
                foreach ( var client in clients )
                {
                    if ( client.State == ClientState.Error && client.Error != null )
                    {
                        Console.WriteLine( client.Error );
                    }
                }
            }
        }

        /// <summary>
        /// Reads the configuration for how the program will run.
        /// </summary>
        /// <returns>A configuration object or <c>null</c> if configuration was invalid.</returns>
        static Configuration? ReadConfig()
        {
            // Read URL.
            var uriString = ConsoleHelper.Prompt( "Live Experience URL", "http://localhost:6229/page/745" );

            if ( !Uri.TryCreate( uriString, UriKind.Absolute, out var uri ) )
            {
                Console.WriteLine( "Invalid URL." );
                return null;
            }

            // Read occurrence id.
            var occurrenceId = ConsoleHelper.Prompt( "Experience Occurrence Id", required: true );

            // Read username.
            var username = ConsoleHelper.Prompt( "Username", required: true );

            // Read password.
            var password = ConsoleHelper.Prompt( "Password", secure: true, required: true );

            // Read client count.
            var clientCountText = ConsoleHelper.Prompt( "Client Count", "10" );

            if ( !int.TryParse( clientCountText, out var clientCount ) )
            {
                clientCount = 10;
            }

            // Read concurrency limit.
            var concurrencyText = ConsoleHelper.Prompt( "Concurrency", "5" );
            
            if ( !int.TryParse( concurrencyText, out var concurrency ) )
            {
                clientCount = 5;
            }

            // Read delay variance.
            var delayVarianceText = ConsoleHelper.Prompt( "Delay Variance", "500" );

            if ( !int.TryParse( delayVarianceText, out var delayVariance ) )
            {
                delayVariance = 500;
            }

            return new Configuration
            {
                Username = username,
                Password = password,
                BaseUri = new Uri( uri.GetLeftPart( UriPartial.Authority ), UriKind.Absolute ),
                LivePageUri = uri,
                OccurrenceId = occurrenceId,
                ClientCount = clientCount,
                ConcurrencyLimit = concurrency,
                DelayVariance = delayVariance
            };
        }

        /// <summary>
        /// Loops over each client and runs the <paramref name="process"/>
        /// function on each item. While waiting for the tasks to complete
        /// the status of all the clients will be displayed on screen.
        /// </summary>
        /// <param name="clients">The list of clients to be processed.</param>
        /// <param name="process">The function to call for each client.</param>
        /// <param name="concurrencyLimit">The maximum number of concurrent clients to be processed.</param>
        /// <param name="cancellationToken">A token that signals if the operation should be aborted.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        static async Task ForEachClientAsync( IEnumerable<Client> clients, Func<Client, Task> process, int concurrencyLimit, CancellationToken cancellationToken )
        {
            var task = Parallel.ForEachAsync( clients, process, concurrencyLimit, cancellationToken );

            while ( !task.IsCompleted )
            {
                UpdateClientDisplayStatus( clients );

                await Task.Delay( 250, CancellationToken.None );
            }

            await task;

            UpdateClientDisplayStatus( clients );
        }

        /// <summary>
        /// Performs an API login to get the authenticated cookie.
        /// </summary>
        /// <param name="client">The RestClient to be used for login, this also gets the cookie.</param>
        /// <param name="configuration">The program configuration.</param>
        /// <returns><c>true</c> if login was successful; <c>false</c> otherwise.</returns>
        static async Task<bool> LoginAsync( IRestClient client, Configuration configuration )
        {
            var loginRequest = new RestRequest( "/api/Auth/Login", Method.Post );

            loginRequest.AddBody( new LoginParameters
            {
                Username = configuration.Username,
                Password = configuration.Password
            }, ContentType.Json );

            var loginResponse = await client.ExecuteAsync<string>( loginRequest );

            if ( !loginResponse.IsSuccessful || loginResponse.Cookies == null || !loginResponse.Cookies.Any( c => c.Name == ".ROCK" ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the screen to show the status of all the clients.
        /// </summary>
        /// <param name="clients">The clients.</param>
        static void UpdateClientDisplayStatus( IEnumerable<Client> clients )
        {
            var sb = new StringBuilder( 2000 );

            int columnCount = 0;

            foreach ( var client in clients )
            {
                if ( columnCount >= 50 )
                {
                    sb.AppendLine();
                    columnCount = 0;
                }

                sb.Append( client.ToString() );

                columnCount++;
            }

            Console.Clear();
            Console.WriteLine( sb.ToString() );
        }
    }
}