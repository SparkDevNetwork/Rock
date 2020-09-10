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

namespace Rock.Logging
{
    /// <summary>
    /// This is a simple POCO used to store the System Settings for Rock Logging.
    /// </summary>
    public class RockLogSystemSettings
    {
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public RockLogLevel LogLevel { get; set; }
        /// <summary>
        /// Gets or sets the maximum size of the file.
        /// </summary>
        /// <value>
        /// The maximum size of the file.
        /// </value>
        public int MaxFileSize { get; set; }
        /// <summary>
        /// Gets or sets the number of log files.
        /// </summary>
        /// <value>
        /// The number of log files.
        /// </value>
        public int NumberOfLogFiles { get; set; }
        /// <summary>
        /// Gets or sets the domains to log.
        /// </summary>
        /// <value>
        /// The domains to log.
        /// </value>
        public List<string> DomainsToLog { get; set; }
    }
}
