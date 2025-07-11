﻿// <copyright>
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

namespace Rock.Logging
{
    /// <summary>
    /// As simple class the represents the events that are logged by the RockLogger.
    /// </summary>
    public class RockLogEvent
    {
        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>
        /// The domain.
        /// </value>
        [Obsolete( "This is not used and will be removed in the future." )]
        [RockObsolete( "17.0" )]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the logged category.
        /// </summary>
        /// <value>The logged category.</value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        [Obsolete( "This is not used and will be removed in the future." )]
        [RockObsolete( "17.0" )]
        public RockLogLevel Level { get; set; }

        /// <summary>
        /// Gets the serialized exception.
        /// </summary>
        /// <value>
        /// The serialized exception.
        /// </value>
        public string SerializedException
        {
            get
            {
                if ( Exception == null )
                {
                    return string.Empty;
                }

                return Exception.ToString();
            }
        }
    }
}
