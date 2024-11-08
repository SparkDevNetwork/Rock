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

using Microsoft.Extensions.Logging;

using Rock.Attribute;

namespace Rock.Logging
{
    /// <summary>
    /// An implementation of ILogger that logs messages in a memory buffer and emits events that can be monitored.
    /// </summary>
    /// <remarks>
    /// This logger is best suited to transient logging, particularly for internal processes that
    /// produce real-time notifications or testing environments that require a light-weight logging implementation.
    /// </remarks>
    [RockInternal( "1.15", true )]
    public class RockLoggerMemoryBuffer : ILogger
    {
        private readonly Queue<RockLogEvent> _logEvents = new Queue<RockLogEvent>( 10000 );

        private readonly LogLevel _logLevel;

        private readonly string _category = "Rock.Logging.RockLoggerMemoryBuffer";

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
        [Obsolete( "This is not used and will be removed in the future." )]
        [RockObsolete( "1.17" )]
        public IRockLogConfiguration LogConfiguration { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerMemoryBuffer"/> class.
        /// </summary>
        public RockLoggerMemoryBuffer()
            : this( LogLevel.Information )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerMemoryBuffer"/> class.
        /// </summary>
        public RockLoggerMemoryBuffer( LogLevel logLevel )
        {
            _logLevel = logLevel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLoggerMemoryBuffer"/> class.
        /// </summary>
        /// <param name="rockLogConfiguration">The rock log configuration.</param>
        [Obsolete( "This is not used and will be removed in the future." )]
        [RockObsolete( "1.17" )]
        public RockLoggerMemoryBuffer( IRockLogConfiguration rockLogConfiguration )
            : this( LogLevel.Information )
        {
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

        /// <inheritdoc/>
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            OnEventLogged( new EventLoggedArgs
            {
                Event = new RockLogEvent
                {
                    Category = _category,
                    Message = formatter( state, exception ),
                    DateTime = RockDateTime.Now,
                    Exception = exception,
                    LogLevel = logLevel
                }
            } );
        }

        /// <inheritdoc/>
        public bool IsEnabled( LogLevel logLevel )
        {
            return logLevel >= _logLevel && logLevel < LogLevel.None;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>( TState state )
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Configuration settings for an in-memory RockLogger instance.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogConfiguration" />
    [Obsolete( "This is not used and will be removed in the future." )]
    [RockObsolete( "1.17" )]
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
