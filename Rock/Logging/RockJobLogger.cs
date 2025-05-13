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
using System.Text;

using Microsoft.Extensions.Logging;

namespace Rock.Logging
{
    /// <summary>
    /// Special logger that wraps a base logger and appends additional information
    /// to the logged messages for Rock Jobs.
    /// </summary>
    internal class RockJobLogger : ILogger
    {
        /// <summary>
        /// The base logger that performs the actual logging.
        /// </summary>
        private readonly ILogger _baseLogger;

        /// <summary>
        /// Gets or sets the ServiceJob identifier.
        /// </summary>
        /// <value>The ServiceJob identifier.</value>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the name of the ServiceJob.
        /// </summary>
        /// <value>The name of the ServiceJob.</value>
        public string JobName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobLogger"/> class.
        /// This is used to add common information to logged messages from jobs.
        /// </summary>
        /// <param name="baseLogger">The base logger that will be used for logging.</param>
        /// <param name="jobId">The identifier of the ServiceJob instance.</param>
        /// <param name="jobName">The name of the ServiceJob instance.</param>
        public RockJobLogger( ILogger baseLogger, int jobId, string jobName )
        {
            _baseLogger = baseLogger;
            JobId = jobId;
            JobName = jobName;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>( TState state )
        {
            return _baseLogger.BeginScope( state );
        }

        /// <inheritdoc/>
        public bool IsEnabled( LogLevel logLevel )
        {
            return _baseLogger.IsEnabled( logLevel );
        }

        /// <inheritdoc/>
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            var prefix = new StringBuilder( $"Job ID: {JobId}" );

            if ( JobName.IsNotNullOrWhiteSpace() )
            {
                prefix.Append( $", Job Name: {JobName}" );
            }

            if ( state is IReadOnlyList<KeyValuePair<string, object>> stateList && stateList.Count == 1 && stateList[0].Key == "{OriginalFormat}" )
            {
                _baseLogger.Log( logLevel, eventId, exception, $"{prefix}, {stateList[0].Value}" );
            }
            else
            {
                // We don't know how to update the message in this case.
                _baseLogger.Log( logLevel, eventId, state, exception, formatter );
            }
        }
    }
}
