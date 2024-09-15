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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Nest;
using Rock.Attribute;

namespace Rock.Logging
{
    /// <summary>
    /// An implementation of RockLogger that logs messages in a memory buffer and emits events that can be monitored.
    /// </summary>
    /// <remarks>
    /// This logger is best suited to transient logging, particularly for internal processes that
    /// produce real-time notifications or testing environments that require a light-weight logging implementation.
    /// </remarks>
    /// <seealso cref="Rock.Logging.IRockLogger" />
    [RockInternal( "1.15" )]
    public class RockLoggerMemoryBuffer : IRockLogger
    {
        private const string DEFAULT_DOMAIN = "OTHER";
        private HashSet<string> _domains;
        private Queue<RockLogEvent> _logEvents = new Queue<RockLogEvent>( 10000 );

        /// <summary>
        /// Gets the current buffer of log events.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<RockLogEvent> GetLogEvents()
        {
            var list = new ReadOnlyCollection<RockLogEvent>( _logEvents.ToList() );
            return list;
        }

        #region Events

        /// <summary>
        /// Arguments for the EventLogged event.
        /// </summary>
        public class EventLoggedArgs : EventArgs
        {
            /// <summary>
            /// The event.
            /// </summary>
            public RockLogEvent Event;
        }

        /// <summary>
        /// An event that occurs when a log event is added to the buffer.
        /// </summary>
        public event EventHandler<EventLoggedArgs> EventLogged;

        /// <summary>
        /// Fires an event that occurs when a log event is added to the buffer.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEventLogged( EventLoggedArgs e )
        {
            EventLogged?.Invoke( this, e );
        }

        #endregion

        /// <summary>
        /// Gets the log configuration.
        /// </summary>
        /// <value>
        /// The log configuration.
        /// </value>
        public IRockLogConfiguration LogConfiguration { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerMemoryBuffer"/> class.
        /// </summary>
        public RockLoggerMemoryBuffer()
        {
            Initialize( null );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerMemoryBuffer"/> class.
        /// </summary>
        /// <param name="rockLogConfiguration">The rock log configuration.</param>
        public RockLoggerMemoryBuffer( IRockLogConfiguration rockLogConfiguration )
        {
            Initialize( rockLogConfiguration );
        }

        private void Initialize( IRockLogConfiguration rockLogConfiguration )
        {
            rockLogConfiguration = rockLogConfiguration ?? this.GetDefaultConfiguration();

            this.LogConfiguration = rockLogConfiguration;

            _domains = new HashSet<string>();
            if ( rockLogConfiguration.DomainsToLog != null )
            {
                foreach ( var domain in rockLogConfiguration.DomainsToLog )
                {
                    _domains.Add( domain );
                }
            }

            this.Clear();
        }

        private RockLogConfiguration GetDefaultConfiguration()
        {
            // By default, log everything.
            var rockLogConfiguration = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                DomainsToLog = new List<string>()
            };
            return rockLogConfiguration;
        }

        #endregion

        /// <summary>
        /// Clear all events from the buffer.
        /// </summary>
        public void Clear()
        {
            // Clear the queue.
            _logEvents.Clear();
        }

        #region IRockLogger implementation

        /// <summary>
        /// Closes this instance and releases file locks.
        /// </summary>
        void IRockLogger.Close()
        {
            // No effect.
        }

        void IRockLogger.Delete()
        {
            // Clear the queue.
            _logEvents.Clear();
        }

        bool IRockLogger.ShouldLogEntry( RockLogLevel logLevel, string domain )
        {
            return ShouldLogEntry( logLevel, domain );
        }

        /// <summary>
        /// Determins if the logger is enabled for the specified RockLogLevel and Domain.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
        private bool ShouldLogEntry( RockLogLevel logLevel, string domain )
        {
            if ( logLevel > LogConfiguration.LogLevel || logLevel == RockLogLevel.Off )
            {
                return false;
            }

            if ( _domains.Count > 0
                 && !_domains.Contains( domain.ToUpper() ) )
            {
                return false;
            }
            return true;
        }

        void IRockLogger.ReloadConfiguration()
        {
            // No effect.
        }

        List<string> IRockLogger.LogFiles
        {
            get
            {
                return new List<string>();
            }
        }

        #region WriteToLog Methods

        private static Regex _regexPropertyPlaceholder = new Regex( @"\{.*?\}", RegexOptions.Compiled );

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        private void WriteToLogInternal( RockLogLevel logLevel, Exception exception, string domain, string messageTemplate, params object[] propertyValues )
        {
            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            if ( propertyValues != null )
            {
                foreach ( var propertyValue in propertyValues )
                {
                    messageTemplate = _regexPropertyPlaceholder.Replace( messageTemplate, propertyValue.ToStringSafe(), 1 );
                }
            }

            if ( exception != null )
            {
                messageTemplate += "\nEXCEPTION: " + exception.ToString();
            }

            WriteToLogInternal( logLevel, domain, messageTemplate );
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="propertyValues">The property values.</param>
        private void WriteToLogInternal( RockLogLevel logLevel, string domain, string messageTemplate, params object[] propertyValues )
        {
            WriteToLogInternal( logLevel, null, domain, messageTemplate, propertyValues );
        }

        private void WriteToLogInternal( RockLogLevel logLevel, string domain, string messageTemplate )
        {
            if ( !ShouldLogEntry( logLevel, domain ) )
            {
                return;
            }

            var logEvent = new RockLogEvent
            {
                DateTime = RockDateTime.Now,
                Domain = domain,
                Level = logLevel,
                Message = messageTemplate,
                Exception = null
            };

            _logEvents.Enqueue( logEvent );

            // Trigger a client notification.
            var args = new EventLoggedArgs
            {
                Event = logEvent
            };

            OnEventLogged( args );
        }

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
            WriteToLogInternal( logLevel, domain, messageTemplate );
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
            WriteToLogInternal( logLevel, domain, messageTemplate, propertyValues );
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
            WriteToLogInternal( logLevel, exception, domain, messageTemplate );
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
            WriteToLogInternal( logLevel, exception, domain, messageTemplate, propertyValues );
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

        #endregion
    }

    /// <summary>
    /// Configuration settings for an in-memory RockLogger instance.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogConfiguration" />
    public class RockLoggerInMemoryConfiguration : IRockLogConfiguration
    {
        private RockLogLevel _logLevel;
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public RockLogLevel LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        private List<string> _domainsToLog;
        /// <summary>
        /// Gets or sets the domains to log.
        /// </summary>
        /// <value>
        /// The domains to log.
        /// </value>
        public List<string> DomainsToLog
        {
            get
            {
                return _domainsToLog;
            }
            set
            {
                _domainsToLog = value;
            }
        }

        int IRockLogConfiguration.MaxFileSize
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        int IRockLogConfiguration.NumberOfLogFiles
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        string IRockLogConfiguration.LogPath
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        DateTime IRockLogConfiguration.LastUpdated
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
