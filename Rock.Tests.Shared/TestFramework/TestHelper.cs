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
using System.Diagnostics;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Rock.Configuration;
using Rock.Data;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    public static class TestHelper
    {
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

        #region RockApp Initialization

        /// <summary>
        /// Configures the RockApp instance for unit testing with the provided
        /// connection string, which may be <c>null</c>.
        /// </summary>
        /// <param name="connectionString">The connection string to use for configuring the RockApp.</param>
        internal static void ConfigureRockApp( string connectionString )
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
            return CreateScopedRockApp( null );
        }

        /// <summary>
        /// Creates a new scoped RockApp instance with database configuration.
        /// When the instance is no longer required the scope should be disposed.
        /// </summary>
        /// <param name="configureApp">A function to call to perform additional configuration of the services.</param>
        /// <returns>An instance of <see cref="RockAppScope"/>.</returns>
        public static RockAppScope CreateScopedRockApp( Action<ServiceCollection> configureApp )
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            // Provide a mocked IDatabaseConfiguration. The real implementation
            // queries a live database in its constructor to determine the
            // platform, version, edition and size - none of which is available
            // when running against a mocked context. Using the real one would
            // open a connection to a non-existent server and block until the
            // connection timeout elapsed.
            var databaseConfigurationMock = new Mock<IDatabaseConfiguration>( MockBehavior.Loose );
            databaseConfigurationMock.Setup( m => m.IsDatabaseAvailable ).Returns( true );

            var app = CreateRockApp( "Server=localhost\\MockInstance;Database=Rock", sc =>
            {
                sc.AddSingleton( rockContextFactory );
                sc.AddSingleton<IDatabaseConfiguration>( databaseConfigurationMock.Object );
                configureApp?.Invoke( sc );
            } );

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
            hostingMock.Setup( a => a.NodeName ).Returns( "TestNode" );

            sc.AddSingleton<IConnectionStringProvider>( new TestConnectionStringProvider( connectionString ) );
            sc.AddSingleton<IInitializationSettings, TestInitializationSettings>();
            sc.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            sc.AddSingleton<IUserAgentParser, UserAgentParser>();
            sc.AddSingleton( hostingMock.Object );

            sc.AddSingleton<IRockContextFactory, RockContextFactory>();

            configureApp?.Invoke( sc );

            var app = new RockApp( sc.BuildServiceProvider() );

            if ( app.GetDatabaseConfiguration() is DatabaseConfiguration databaseConfig )
            {
                databaseConfig.IsDatabaseAvailable = connectionString.IsNotNullOrWhiteSpace();
            }

            return app;
        }

        /// <summary>
        /// Gets the path to the RockWeb folder. This is determined automatically
        /// by searching for the solution file.
        /// </summary>
        /// <returns>The path to the RockWeb folder or <c>null</c> if it could not be determined.</returns>
        private static string GetRockWebPath()
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
                    RockCache.ClearAllCachedItems( false );
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
