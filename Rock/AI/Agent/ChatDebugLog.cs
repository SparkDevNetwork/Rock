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
    /// A single log entry from a chat agent request.
    /// </summary>
    internal class ChatDebugLog
    {
        #region Properties

        /// <summary>
        /// The category of the log entry, such as "Rock.AI.Agent.Skills.CommunicationSkill".
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The log level of the logged message entry.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// The text message that was logged.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The date and time this message was logged.
        /// </summary>
        public DateTime Timestamp { get; } = RockDateTime.Now;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatDebugLog"/> class.
        /// </summary>
        /// <param name="category">The category of the log entry, used to group related log messages.</param>
        /// <param name="logLevel">The severity level of the log entry, indicating its importance or urgency.</param>
        /// <param name="message">The content of the log message providing details about the event or operation.</param>
        internal ChatDebugLog( string category, LogLevel logLevel, string message )
        {
            Category = category;
            LogLevel = logLevel;
            Message = message;
        }

        #endregion
    }
}
