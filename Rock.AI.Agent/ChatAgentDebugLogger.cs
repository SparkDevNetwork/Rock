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

using Microsoft.Extensions.Logging;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A specialized logger that captures debug logs and then passed them along
    /// to the base logger for normal logging operation.
    /// </summary>
    internal class ChatAgentDebugLogger : ILogger
    {
        #region Fields

        /// <summary>
        /// The category name for this logger.
        /// </summary>
        private readonly string _category;

        /// <summary>
        /// The base logger that will handle the actual logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The factory that created this logger, used to capture debug logs.
        /// </summary>
        private readonly ChatAgentDebugLoggerFactory _factory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentDebugLogger"/> class.
        /// </summary>
        /// <param name="category">The category name for this logger.</param>
        /// <param name="logger">The base logger that will handle the actual logging.</param>
        /// <param name="factory">The factory that created this logger, used to capture debug logs.</param>
        public ChatAgentDebugLogger( string category, ILogger logger, ChatAgentDebugLoggerFactory factory )
        {
            _category = category;
            _logger = logger;
            _factory = factory;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>( TState state )
        {
            return _logger.BeginScope( state );
        }

        /// <inheritdoc/>
        public bool IsEnabled( LogLevel logLevel )
        {
            // Always return true to ensure all logs are captured. The base
            // logger will handle its own filtering based on log level.
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            _logger.Log( logLevel, eventId, state, exception, formatter );

            _factory.AddLogMessage( new ChatDebugLog( _category, logLevel, formatter( state, exception ) ) );
        }

        #endregion
    }
}
