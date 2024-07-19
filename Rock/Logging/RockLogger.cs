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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Debug;

using Rock.Attribute;
using Rock.Observability;
using Rock.SystemKey;

using Serilog;
using Serilog.Formatting.Compact;

namespace Rock.Logging
{
    /// <summary>
    /// This is the static class that is used to log data in Rock.
    /// </summary>
    public static class RockLogger
    {
        private static ServiceProvider _serviceProvider;

        private static readonly DynamicConfigurationProvider _standardConfigurationProvider = new DynamicConfigurationProvider();

        private static readonly DynamicConfigurationProvider _advancedConfigurationProvider = new DynamicConfigurationProvider();

        /// <summary>
        /// Gets the logger with logging methods.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        [Obsolete( "This is not used and will be removed in the future." )]
        [RockObsolete( "1.17" )]
        public static IRockLogger Log => _log.Value;
#pragma warning disable CS0618 // Type or member is obsolete
        private static readonly Lazy<IRockLogger> _log = new Lazy<IRockLogger>( () => new LegacySerilogLogger() );
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets the logger factory currently associated with this application
        /// instance.
        /// </summary>
        /// <value>The logger factory.</value>
        public static ILoggerFactory LoggerFactory { get; private set; } = new NullLoggerFactory();

        /// <summary>
        /// Gets the log reader.
        /// </summary>
        /// <value>
        /// The log reader.
        /// </value>
        public static IRockLogReader LogReader => new RockSerilogReader( GetSerilogConfiguration() );

        internal static SeriSinkWrapper SinkWrapper { get; } = new SeriSinkWrapper();

        internal static void Initialize()
        {
            _standardConfigurationProvider.Set( "LogLevel:Default", "None" );

            var serviceCollection = new ServiceCollection();
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .Add( _standardConfigurationProvider )
                .Add( _advancedConfigurationProvider )
                .Build();

            serviceCollection.AddLogging( cfg =>
            {
                cfg.AddConfiguration( configuration );

                ObservabilityHelper.ConfigureLoggingBuilder( cfg );
                cfg.AddProvider( new DebugLoggerProvider() );

                var seriLogger = new LoggerConfiguration()
                     .MinimumLevel
                     .Verbose()
                     .WriteTo
                     .Sink( SinkWrapper )
                     .CreateLogger();

                cfg.AddProvider( new Serilog.Extensions.Logging.SerilogLoggerProvider( seriLogger ) );
            } );

            _serviceProvider = serviceCollection.BuildServiceProvider();
            LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        /// <summary>
        /// Gets the standard categories that have been defined in Rock.
        /// </summary>
        /// <returns>A list of category names.</returns>
        [RockInternal( "1.17", true )]
        public static List<string> GetStandardCategories()
        {
            var typeCategories = Reflection.FindTypes( typeof( object ) )
                .Select( p => p.Value )
                .Where( t => t.GetCustomAttribute<RockLoggingCategoryAttribute>() != null )
                .Select( t => t.FullName );

            var assemblyCategories = Reflection.GetRockAndPluginAssemblies()
                .SelectMany( a => a.GetCustomAttributes<RockLoggingCategoryAttribute>() )
                .Where( a => a.CategoryName.IsNotNullOrWhiteSpace() )
                .Select( a => a.CategoryName );

            return typeCategories
                .Union( assemblyCategories )
                .Distinct()
                .OrderBy( cat => cat )
                .ToList();
        }

        /// <summary>
        /// Reloads the configuration defined in the database and reconfigures
        /// the loggers to match the new settings.
        /// </summary>
        [RockInternal( "1.17", true )]
        public static void ReloadConfiguration()
        {
            var configuration = Rock.Web.SystemSettings.GetValue( SystemSetting.ROCK_LOGGING_SETTINGS ).FromJsonOrNull<RockLogSystemSettings>();

            ReloadConfiguration( configuration, GetSerilogConfiguration() );
        }

        /// <summary>
        /// Loads the configuration specified by the system settings.
        /// </summary>
        internal static SerilogConfiguration GetSerilogConfiguration()
        {
            var configuration = Rock.Web.SystemSettings.GetValue( SystemSetting.ROCK_LOGGING_SETTINGS ).FromJsonOrNull<RockLogSystemSettings>();
            var serilogConfiguration = new SerilogConfiguration();

            if ( configuration == null )
            {
                serilogConfiguration.NumberOfLogFiles = 20;
                serilogConfiguration.MaxFileSize = 20;
            }
            else
            {
                serilogConfiguration.NumberOfLogFiles = Math.Max( configuration.NumberOfLogFiles, 1 );
                serilogConfiguration.MaxFileSize = Math.Max( configuration.MaxFileSize, 1 );
            }

            serilogConfiguration.LogPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data\\Logs\\Rock.log" );

            return serilogConfiguration;
        }

        /// <summary>
        /// Loads the configuration specified by the system settings.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="serilogConfiguration">The configuration for serilog.</param>
        internal static void ReloadConfiguration( RockLogSystemSettings configuration, SerilogConfiguration serilogConfiguration )
        {
            LoadLogLevelConfiguration( configuration );

            if ( configuration.IsLocalLoggingEnabled )
            {
                SinkWrapper.SetLogger( CreateSerilogLogger( serilogConfiguration ) );
            }
        }

        /// <summary>
        /// Loads the configuration specified by the system settings.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        private static void LoadLogLevelConfiguration( RockLogSystemSettings configuration )
        {
            var root = new Dictionary<string, object>();
            var logLevel = new Dictionary<string, string>();

            root.Add( "LogLevel", logLevel );
            logLevel.Add( "Default", "None" );

            if ( configuration.StandardCategories != null )
            {
                foreach ( var category in configuration.StandardCategories )
                {
                    logLevel.AddOrReplace( category, configuration.StandardLogLevel.ToString() );
                }
            }

            _standardConfigurationProvider.LoadFromJson( root.ToJson(), true );
            _advancedConfigurationProvider.LoadFromJson( configuration.AdvancedSettings, true );
        }

        /// <summary>
        /// Creates the serilog logger.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A logger instance.</returns>
        internal static Serilog.Core.Logger CreateSerilogLogger( SerilogConfiguration configuration )
        {
            return new LoggerConfiguration()
                 .MinimumLevel
                 .Verbose()
                 .WriteTo
                 .File( new CompactJsonFormatter(),
                     configuration.LogPath,
                     rollingInterval: RollingInterval.Infinite,
                     buffered: true,
                     shared: false,
                     flushToDiskInterval: TimeSpan.FromSeconds( 10 ),
                     retainedFileCountLimit: configuration.NumberOfLogFiles,
                     rollOnFileSizeLimit: true,
                     fileSizeLimitBytes: configuration.MaxFileSize * 1024 * 1024 )
                 .CreateLogger();
        }

        /// <summary>
        /// Recycles the serilog writer which frees up any old files.
        /// </summary>
        [RockInternal( "1.17", true )]
        public static void RecycleSerilog()
        {
            SinkWrapper.SetLogger( CreateSerilogLogger( GetSerilogConfiguration() ) );
        }
    }
}
