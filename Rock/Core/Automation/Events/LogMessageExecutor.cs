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

using Rock.Logging;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Handles execution for the <see cref="LogMessage"/> event component.
    /// </summary>
    class LogMessageExecutor : AutomationEventExecutor
    {
        #region Fields

        /// <summary>
        /// The logger instance that will handle logging the messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The level of logging to use when logging the message.
        /// </summary>
        private readonly LogLevel _level;

        /// <summary>
        /// The message template to log. This is merged with Lava to get the
        /// final message.
        /// </summary>
        private readonly string _messageTemplate;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageExecutor"/> class.
        /// </summary>
        /// <param name="category">The category that will be used when logging the message.</param>
        /// <param name="level">The level of logging to use when logging the message.</param>
        /// <param name="messageTemplate">The lava template to use to generate the final message to log.</param>
        public LogMessageExecutor( string category, LogLevel level, string messageTemplate )
        {
            _logger = RockLogger.LoggerFactory.CreateLogger( category );
            _level = level;
            _messageTemplate = messageTemplate;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            var mergeFields = new Dictionary<string, object>();

            foreach ( var value in request.Values )
            {
                mergeFields[value.Key] = value.Value;
            }

            var message = _messageTemplate.ResolveMergeFields( mergeFields );

            _logger.Log( _level, message );
        }

        #endregion
    }
}
