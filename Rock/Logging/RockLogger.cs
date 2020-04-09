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
namespace Rock.Logging
{
    /// <summary>
    /// This is the static class that is used to log data in Rock.
    /// </summary>
    public static class RockLogger
    {
        private static IRockLogger _log;
        /// <summary>
        /// Gets the logger with logging methods.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public static IRockLogger Log
        {
            get
            {
                if ( _log == null )
                {
                    // In the future the RockLogConfiguration could be gotten via dependency injection, but not today.
                    _log = new RockLoggerSerilog( new RockLogConfiguration() );
                }
                return _log;
            }
        }

        /// <summary>
        /// Gets the log reader.
        /// </summary>
        /// <value>
        /// The log reader.
        /// </value>
        public static IRockLogReader LogReader => new RockSerilogReader( Log );

    }
}
