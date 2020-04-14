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
using Serilog;
using Serilog.Formatting.Compact;

namespace Rock.Logging
{
    /// <summary>
    /// This is the internal implementation of the IRockLogger interface. This
    /// one uses Serilog, but a different implementation could be used in the
    /// future if required.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogger" />
    internal class RockLoggerSerilog : IRockLogger
    {
        private const string DEFAULT_DOMAIN = "OTHER";
        private DateTime _ConfigurationLastLoaded;
        private ILogger _logger;
        private HashSet<string> _domains;

        /// <summary>
        /// Gets the log configuration.
        /// </summary>
        /// <value>
        /// The log configuration.
        /// </value>
        public IRockLogConfiguration LogConfiguration { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerSerilog"/> class.
        /// </summary>
        /// <param name="rockLogConfiguration">The rock log configuration.</param>
        public RockLoggerSerilog( IRockLogConfiguration rockLogConfiguration )
        {
            LogConfiguration = rockLogConfiguration;
            LoadConfiguration( LogConfiguration );
        }

        ~RockLoggerSerilog()
        {
            if ( _logger != null )
            {
                ( ( IDisposable ) _logger ).Dispose();
                _logger = null;
            }
        }

        /// <summary>
        /// Closes this instance and releases file locks.
        /// </summary>
        public void Close()
        {
            if ( _logger != null )
            {
                ( ( IDisposable ) _logger ).Dispose();
                _logger = null;
            }
        }

        #region WriteToLog Methods
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void WriteToLog( RockLogLevel logLevel, string messageTemplate )
        {
            WriteToLog( logLevel, DEFAULT_DOMAIN, messageTemplate );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void WriteToLog( RockLogLevel logLevel, string domain, string messageTemplate )
        {
            EnsureLoggerExistsAndUpdated();

            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            var serilogLogLevel = GetLogEventLevelFromRockLogLevel( logLevel );
            _logger.Write( serilogLogLevel, GetMessageTemplateWithDomain( messageTemplate ), domain.ToUpper() );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void WriteToLog( RockLogLevel logLevel, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( logLevel, DEFAULT_DOMAIN, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void WriteToLog( RockLogLevel logLevel, string domain, string messageTemplate, params object[] propertyValues )
        {
            EnsureLoggerExistsAndUpdated();

            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            var serilogLogLevel = GetLogEventLevelFromRockLogLevel( logLevel );
            _logger.Write( serilogLogLevel, GetMessageTemplateWithDomain( messageTemplate ), AddDomainToObjectArray( propertyValues, domain.ToUpper() ) );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void WriteToLog( RockLogLevel logLevel, Exception exception, string messageTemplate )
        {
            WriteToLog( logLevel, exception, DEFAULT_DOMAIN, messageTemplate );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void WriteToLog( RockLogLevel logLevel, Exception exception, string domain, string messageTemplate )
        {
            EnsureLoggerExistsAndUpdated();

            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            var serilogLogLevel = GetLogEventLevelFromRockLogLevel( logLevel );
            _logger.Write( serilogLogLevel, exception, GetMessageTemplateWithDomain( messageTemplate ), domain.ToUpper() );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void WriteToLog( RockLogLevel logLevel, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( logLevel, exception, DEFAULT_DOMAIN, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void WriteToLog( RockLogLevel logLevel, Exception exception, string domain, string messageTemplate, params object[] propertyValues )
        {
            EnsureLoggerExistsAndUpdated();

            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            var serilogLogLevel = GetLogEventLevelFromRockLogLevel( logLevel );
            _logger.Write( serilogLogLevel, exception, GetMessageTemplateWithDomain( messageTemplate ), AddDomainToObjectArray( propertyValues, domain.ToUpper() ) );
        }
        #endregion

        #region Verbose Methods
        /// <summary>
        /// Log Verbose with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Verbose( string messageTemplate )
        {
            WriteToLog( RockLogLevel.All, messageTemplate );
        }

        /// <summary>
        /// Log Verbose with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Verbose( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.All, domain, messageTemplate );
        }

        /// <summary>
        /// Log Verbose with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Verbose( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.All, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Log Verbose with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Verbose( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.All, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Log Verbose with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Verbose( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.All, exception, messageTemplate );
        }

        /// <summary>
        /// Log Verbose with the specified domain and message template.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Verbose( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.All, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Log Verbose with the specified exception and message template.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Verbose( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.All, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Log Verbose with the specified domain, exception, and message template.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Verbose( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.All, exception, domain, messageTemplate, propertyValues );
        }
        #endregion

        #region Debug Methods

        /// <summary>
        /// Logs Debug with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Debug( string messageTemplate )
        {
            WriteToLog( RockLogLevel.Debug, messageTemplate );
        }

        /// <summary>
        /// Logs Debug with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Debug( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Debug, domain, messageTemplate );
        }

        /// <summary>
        /// Logs Debug with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Debug( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Debug, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs Debug with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Debug( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Debug, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs Debug with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Debug( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Debug, exception, messageTemplate );
        }

        /// <summary>
        /// Logs Debug with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Debug( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Debug, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Logs Debug with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Debug( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Debug, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs Debug with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Debug( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Debug, exception, domain, messageTemplate, propertyValues );
        }

        #endregion

        #region Information Methods
        /// <summary>
        /// Logs information with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Information( string messageTemplate )
        {
            WriteToLog( RockLogLevel.Info, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Information( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Info, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Information( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Info, exception, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Information( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Info, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Information( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Info, domain, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Information( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Info, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Information( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Info, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Information( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Info, exception, domain, messageTemplate, propertyValues );
        }
        #endregion

        #region Warning Methods
        /// <summary>
        /// Warnings the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Warning( string messageTemplate )
        {
            WriteToLog( RockLogLevel.Warning, messageTemplate );
        }

        /// <summary>
        /// Logs warnings the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Warning( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Warning, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Warning( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Warning, exception, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Warning( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Warning, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Warning( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Warning, domain, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Warning( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Warning, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Warning( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Warning, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Logs information with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Warning( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Warning, exception, domain, messageTemplate, propertyValues );
        }
        #endregion

        #region Error Methods
        /// <summary>
        /// Logs errors with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Error( string messageTemplate )
        {
            WriteToLog( RockLogLevel.Error, messageTemplate );
        }

        /// <summary>
        /// Logs errors with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Error( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Error, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs errors with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Error( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Error, exception, messageTemplate );
        }

        /// <summary>
        /// Logs errors with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Error( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Error, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs errors with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Error( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Error, domain, messageTemplate );
        }

        /// <summary>
        /// Logs errors with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Error( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Error, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs errors with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Error( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Error, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Logs errors with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Error( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Error, exception, domain, messageTemplate, propertyValues );
        }
        #endregion

        #region Fatal Methods
        /// <summary>
        /// Logs fatal message with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        public void Fatal( string messageTemplate )
        {
            WriteToLog( RockLogLevel.Fatal, messageTemplate );
        }

        /// <summary>
        /// Logs fatal message with the specified message template.
        /// </summary>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Fatal( string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Fatal, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs fatal message with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Fatal( Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Fatal, exception, messageTemplate );
        }

        /// <summary>
        /// Logs fatal message with the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Fatal( Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Fatal, exception, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs fatal message with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Fatal( string domain, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Fatal, domain, messageTemplate );
        }

        /// <summary>
        /// Logs fatal message with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Fatal( string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Fatal, domain, messageTemplate, propertyValues );
        }

        /// <summary>
        /// Logs fatal message with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        public void Fatal( string domain, Exception exception, string messageTemplate )
        {
            WriteToLog( RockLogLevel.Fatal, exception, domain, messageTemplate );
        }

        /// <summary>
        /// Logs fatal message with the specified domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        public void Fatal( string domain, Exception exception, string messageTemplate, params object[] propertyValues )
        {
            WriteToLog( RockLogLevel.Fatal, exception, domain, messageTemplate, propertyValues );
        }
        #endregion

        #region Private Helper Methods
        private string GetMessageTemplateWithDomain( string messageTemplate )
        {
            return $"{{domain}} {messageTemplate}";
        }

        private object[] AddDomainToObjectArray( object[] propertyValues, string domain )
        {
            var properties = new List<object>( propertyValues );
            properties.Insert( 0, domain );
            return properties.ToArray();
        }

        private Serilog.Events.LogEventLevel GetLogEventLevelFromRockLogLevel( RockLogLevel logLevel )
        {
            switch ( logLevel )
            {
                case ( RockLogLevel.Error ):
                    return Serilog.Events.LogEventLevel.Error;
                case ( RockLogLevel.Warning ):
                    return Serilog.Events.LogEventLevel.Warning;
                case ( RockLogLevel.Info ):
                    return Serilog.Events.LogEventLevel.Information;
                case ( RockLogLevel.Debug ):
                    return Serilog.Events.LogEventLevel.Debug;
                case ( RockLogLevel.All ):
                    return Serilog.Events.LogEventLevel.Verbose;
                default:
                    return Serilog.Events.LogEventLevel.Fatal;
            }
        }

        private void LoadConfiguration( IRockLogConfiguration rockLogConfiguration )
        {
            _domains = new HashSet<string>( LogConfiguration.DomainsToLog.Select( s => s.ToUpper() ).Distinct() );
            _logger = new LoggerConfiguration()
                 .MinimumLevel
                 .Verbose()
                 .WriteTo
                 .File( new CompactJsonFormatter(),
                     rockLogConfiguration.LogPath,
                     rollingInterval: RollingInterval.Infinite,
                     buffered: true,
                     shared: false,
                     retainedFileCountLimit: rockLogConfiguration.NumberOfLogFiles,
                     rollOnFileSizeLimit: true,
                     fileSizeLimitBytes: rockLogConfiguration.MaxFileSize * 1024 * 1024 )
                 .CreateLogger();

            _ConfigurationLastLoaded = DateTime.Now;
        }

        private void EnsureLoggerExistsAndUpdated()
        {
            if ( _ConfigurationLastLoaded < LogConfiguration.LastUpdated || _logger == null )
            {
                Close();
                LoadConfiguration( LogConfiguration );
            }
        }

        private bool ShouldLogEntry( RockLogLevel logLevel, string domain )
        {
            if ( logLevel > LogConfiguration.LogLevel || logLevel == RockLogLevel.Off )
            {
                return false;
            }

            if ( !_domains.Contains( domain.ToUpper() ) )
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
