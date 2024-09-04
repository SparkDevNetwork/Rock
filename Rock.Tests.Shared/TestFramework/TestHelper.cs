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
using System.Configuration;
using System.Diagnostics;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Rock.Configuration;

namespace Rock.Tests.Shared
{
    public static class TestHelper
    {
        public const string DefaultTaskName = "Main Test Action";

        static TestHelper()
        {
            // Add the console as the default trace output.
            Trace.Listeners.Add( new TextWriterTraceListener( Console.Out ) );
        }

        /// <summary>
        /// Write a message to the current trace output.
        /// </summary>
        /// <param name="message"></param>
        public static void Log( string message )
        {
            var timestamp = DateTime.Now.ToString( "HH:mm:ss.fff" );
            Trace.WriteLine( $"[{timestamp}] {message}" );
        }

        /// <summary>
        /// Gets the path to the RockWeb folder. This is determined automatically
        /// by searching for the solution file.
        /// </summary>
        /// <returns>The path to the RockWeb folder or <c>null</c> if it could not be determined.</returns>
        public static string GetRockWebPath()
        {
            var directory = new DirectoryInfo( Directory.GetCurrentDirectory() );

            while ( directory != null )
            {
                var solutionFile = Path.Combine( directory.FullName, "Rock.sln" );

                if ( File.Exists( solutionFile ) )
                {
                    return Path.Combine( directory.FullName, "RockWeb" ) + Path.DirectorySeparatorChar;
                }

                directory = directory.Parent;
            }

            return null;
        }

        #region Stopwatch

        private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Starts or restarts a named timer.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch ExecuteWithTimer( string message, Action testMethod )
        {
            var stopwatch = StartTimer( message );
            try
            {
                testMethod();
            }
            catch ( Exception ex )
            {
                Log( $"** ERROR:\n{ex.Message}" );
            }
            finally
            {
                EndTimer( message );
            }

            return stopwatch;
        }

        /// <summary>
        /// Starts or restarts a named timer.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch StartTimer( string name = DefaultTaskName )
        {
            Stopwatch stopwatch;
            if ( _stopwatches.ContainsKey( name ) )
            {
                stopwatch = _stopwatches[name];
            }
            else
            {
                stopwatch = new Stopwatch();
                _stopwatches[name] = stopwatch;
                Log( $"** START: {name}" );
            }
            stopwatch.Start();

            return stopwatch;
        }

        /// <summary>
        /// Pauses the named timer.
        /// </summary>
        /// <param name="name"></param>
        public static void PauseTimer( string name = DefaultTaskName )
        {
            if ( !_stopwatches.ContainsKey( name ) )
            {
                return;
            }

            var stopwatch = _stopwatches[name];
            stopwatch.Stop();
        }

        /// <summary>
        /// Finalizes the named timer and prints the elapsed time to debug output.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch EndTimer( string name = DefaultTaskName )
        {
            if ( !_stopwatches.ContainsKey( name ) )
            {
                return null;
            }

            var stopwatch = _stopwatches[name];
            stopwatch.Stop();
            _stopwatches.Remove( name );

            Log( $"**   END: {name} ({stopwatch.ElapsedMilliseconds}ms)" );

            return stopwatch;
        }

        #endregion

        #region RockApp Initialization

        /// <summary>
        /// Configures the RockApp instance for unit testing with the provided
        /// connection string, which may be <c>null</c>.
        /// </summary>
        /// <param name="connectionString">The connection string to use for configuring the RockApp.</param>
        public static void ConfigureRockApp( string connectionString )
        {
            var app = CreateRockApp( connectionString, null );

            RockApp.Current = app;
        }

        /// <summary>
        /// Creates a new scoped RockApp instance with no database configuration.
        /// When the instance is no longer required the scope should be disposed.
        /// </summary>
        /// <returns>An instance of <see cref="RockAppScope"/>.</returns>
        public static RockAppScope CreateScopedRockApp()
        {
            return CreateScopedRockApp( null, null );
        }

        /// <summary>
        /// Creates a new scoped RockApp instance with no database configuration.
        /// When the instance is no longer required the scope should be disposed.
        /// </summary>
        /// <param name="configureApp">A function to call to perform additional configuration of the services.</param>
        /// <returns>An instance of <see cref="RockAppScope"/>.</returns>
        public static RockAppScope CreateScopedRockApp( Action<ServiceCollection> configureApp )
        {
            return CreateScopedRockApp( null, configureApp );
        }

        /// <summary>
        /// Creates a new scoped RockApp instance with database configuration.
        /// When the instance is no longer required the scope should be disposed.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        /// <returns>An instance of <see cref="RockAppScope"/>.</returns>
        public static RockAppScope CreateScopedRockApp( string connectionString )
        {
            return CreateScopedRockApp( connectionString, null );
        }

        /// <summary>
        /// Creates a new scoped RockApp instance with database configuration.
        /// When the instance is no longer required the scope should be disposed.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        /// <param name="configureApp">A function to call to perform additional configuration of the services.</param>
        /// <returns>An instance of <see cref="RockAppScope"/>.</returns>
        public static RockAppScope CreateScopedRockApp( string connectionString, Action<ServiceCollection> configureApp )
        {
            var app = CreateRockApp( connectionString, configureApp );

            return new RockAppScope( app );
        }

        /// <summary>
        /// Creates a new RockApp object with the provided connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to be used for the RockApp object.</param>
        /// <param name="configureApp">A function to call to perform additional configuration of the services.</param>
        private static RockApp CreateRockApp( string connectionString, Action<ServiceCollection> configureApp )
        {
            var sc = new ServiceCollection();

            var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

            hostingMock.Setup( a => a.ApplicationStartDateTime )
                .Returns( DateTime.Now );
            hostingMock.Setup( a => a.VirtualRootPath ).Returns( "/" );
            hostingMock.Setup( a => a.WebRootPath )
                .Returns( GetRockWebPath() ?? Directory.GetCurrentDirectory() );

            sc.AddSingleton<IConnectionStringProvider>( new TestConnectionStringProvider( connectionString ) );
            sc.AddSingleton<IInitializationSettings, TestInitializationSettings>();
            sc.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            sc.AddSingleton( hostingMock.Object );

            configureApp?.Invoke( sc );

            var app = new RockApp( sc.BuildServiceProvider() );

            if ( app.GetDatabaseConfiguration() is DatabaseConfiguration databaseConfig )
            {
                databaseConfig.IsDatabaseAvailable = connectionString.IsNotNullOrWhiteSpace();
            }

            return app;
        }

        /// <summary>
        /// Connection string provider for running integration unit tests.
        /// </summary>
        private class TestConnectionStringProvider : IConnectionStringProvider
        {
            /// <inheritdoc/>
            public string ConnectionString { get; }

            /// <inheritdoc/>
            public string ReadOnlyConnectionString { get; }

            /// <inheritdoc/>
            public string AnalyticsConnectionString { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TestConnectionStringProvider"/> class.
            /// </summary>
            /// <param name="connectionString">The connection string to be used.</param>
            public TestConnectionStringProvider( string connectionString )
            {
                ConnectionString = connectionString;
                ReadOnlyConnectionString = connectionString;
                AnalyticsConnectionString = connectionString;
            }
        }

        /// <summary>
        /// Provides the initialization settings for integration unit tests.
        /// </summary>
        private class TestInitializationSettings : InitializationSettings
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestInitializationSettings"/> class.
            /// </summary>
            /// <param name="connectionStringProvider">The interface for providing connection strings.</param>
            public TestInitializationSettings( IConnectionStringProvider connectionStringProvider )
                : base( connectionStringProvider )
            {
                // This should probably be updated to hard code most of these values
                // rather than trying to pull them from the app.config.
                var settings = ConfigurationManager.AppSettings;

                IsRunScheduledJobsEnabled = settings["RunJobsInIISContext"]?.AsBoolean() ?? false;
                OrganizationTimeZone = settings["OrgTimeZone"]?.ToStringSafe();
                PasswordKey = settings["PasswordKey"]?.ToStringSafe();
                DataEncryptionKey = settings["DataEncryptionKey"]?.ToStringSafe();
                RockStoreUrl = settings["RockStoreUrl"]?.ToStringSafe();
                IsDuplicateGroupMemberRoleAllowed = settings["AllowDuplicateGroupMembers"]?.AsBoolean() ?? false;
                IsCacheStatisticsEnabled = settings["CacheManagerEnableStatistics"]?.AsBoolean() ?? false;
                ObservabilityServiceName = settings["ObservabilityServiceName"]?.ToStringSafe();
                AzureSignalREndpoint = settings["AzureSignalREndpoint"]?.ToStringSafe();
                AzureSignalRAccessKey = settings["AzureSignalRAccessKey"]?.ToStringSafe();
                SparkApiUrl = settings["SparkApiUrl"]?.ToStringSafe();
                NodeName = settings["NodeName"]?.ToStringSafe();
            }

            /// <inheritdoc/>
            public override void Save()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A wrapper around a RockApp that scopes itself so a using statement
        /// will shutdown the RockApp instance and restore the previous
        /// configuration.
        /// </summary>
        public class RockAppScope : IDisposable
        {
            public RockApp App { get; }

            private readonly RockApp _previousApp;

            /// <summary>
            /// Initializes a new instance of the RockAppScope class.
            /// </summary>
            /// <param name="serviceProvider">The service provider to be used within the scope.</param>
            public RockAppScope( RockApp app )
            {
                App = app;
                _previousApp = RockApp.Current;

                RockApp.Current = app;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if ( ReferenceEquals( RockApp.Current, App ) )
                {
                    RockApp.Current = _previousApp;
                }
                else
                {
                    throw new InvalidOperationException( "RockApp.Current is not expected value while disposing RockAppScope." );
                }
            }
        }

        #endregion
    }
}