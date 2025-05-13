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

using Microsoft.Extensions.Logging;

namespace Rock.Logging
{
    /// <summary>
    /// This is a simple POCO used to store the System Settings for Rock Logging.
    /// </summary>
    public class RockLogSystemSettings
    {
        /// <summary>
        /// Gets or sets the log level to use for all categories that are
        /// enabled for standard logging.
        /// </summary>
        /// <value>The standard log level.</value>
        public LogLevel StandardLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.None;

        /// <summary>
        /// Gets or sets the standard categories to log. These will be logged
        /// at the level specified by <see cref="StandardLogLevel"/>.
        /// </summary>
        /// <value>The standard categories to log.</value>
        public List<string> StandardCategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock will write logs to the
        /// local file system.
        /// </summary>
        /// <value><c>true</c> if Rock will write logs to the local file system; otherwise, <c>false</c>.</value>
        public bool IsLocalLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock will write logs to the
        /// Observability framework.
        /// </summary>
        /// <value><c>true</c> if Rock will write logs to the Observability framework; otherwise, <c>false</c>.</value>
        public bool IsObservabilityLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the advanced settings as a JSON object that conforms
        /// to the standard Microsoft logging syntax. The root object should be
        /// the "Logging" node - meaning "LogLevel" should be one of the keys
        /// defined at the root of this object.
        /// </summary>
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/" />
        /// <value>The advanced settings.</value>
        public string AdvancedSettings { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        [Obsolete( "Use StandardLogLevel instead." )]
        [RockObsolete( "1.17" )]
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
        [Obsolete( "Use StandardCategories instead." )]
        [RockObsolete( "1.17" )]
        public List<string> DomainsToLog { get; set; }
    }
}
