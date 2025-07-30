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

using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A special logger factory that is used to capture debug logs for chat
    /// agents. The logs are then made available in the response data.
    /// </summary>
    internal class ChatAgentDebugLoggerFactory : ILoggerFactory
    {
        #region Fields

        /// <summary>
        /// The list of recorded log messages.
        /// </summary>
        private readonly List<ChatDebugLog> _logs = new List<ChatDebugLog>();

        /// <summary>
        /// The base logger factory that this factory wraps.
        /// </summary>
        private readonly ILoggerFactory _baseFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentDebugLoggerFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The base <see cref="ILoggerFactory"/> used to create loggers.</param>
        public ChatAgentDebugLoggerFactory( ILoggerFactory loggerFactory )
        {
            _baseFactory = loggerFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void AddProvider( ILoggerProvider provider )
        {
            _baseFactory.AddProvider( provider );
        }

        /// <inheritdoc/>
        public ILogger CreateLogger( string categoryName )
        {
            return new ChatAgentDebugLogger( categoryName, _baseFactory.CreateLogger( categoryName ), this );
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _baseFactory.Dispose();
        }

        /// <summary>
        /// Adds a log message to the internal list of logs.
        /// </summary>
        /// <param name="message">The message that was logged.</param>
        internal void AddLogMessage( ChatDebugLog message )
        {
            lock( _logs )
            {
                _logs.Add( message );
            }
        }

        /// <summary>
        /// Gets a snapshot of the currently logged messages.
        /// </summary>
        /// <returns>A list of logged messages.</returns>
        internal List<ChatDebugLog> GetLogs()
        {
            lock( _logs )
            {
                return new List<ChatDebugLog>( _logs );
            }
        }

        #endregion
    }
}
