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
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Rock.Model;
using Rock.Utility.ExtensionMethods;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Rock.Logging
{
    /// <summary>
    /// This is the internal implementation of the IRockLogReader. Since the logger is
    /// Serilog this is also serilog, but could be replaced with something different at some
    /// point in time.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogReader" />
    internal class RockSerilogReader : IRockLogReader
    {
        private readonly IRockLogger _rockLogger;
        private readonly JsonSerializer _jsonSerializer;

        /// <summary>
        /// Gets the record count.
        /// </summary>
        /// <value>
        /// The record count.
        /// </value>
        public int RecordCount
        {
            get
            {
                return GetRockLogRecordCount();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSerilogReader"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public RockSerilogReader( IRockLogger logger )
        {
            _rockLogger = logger;

            _jsonSerializer = JsonSerializer.Create( new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                Culture = CultureInfo.InvariantCulture
            } );
        }

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public List<RockLogEvent> GetEvents( int startIndex, int count )
        {
            _rockLogger.Close();

            var logs = GetLogLines( startIndex, count );

            return GetRockLogEventsFromLogLines( startIndex, count, logs );
        }

        private List<RockLogEvent> GetRockLogEventsFromLogLines( int startIndex, int count, string[] logs )
        {
            var results = new List<RockLogEvent>();
            var reversedStartIndex = ( logs.Length - 1 ) - startIndex;
            var reversedEndIndex = reversedStartIndex - count;

            for ( var i = reversedStartIndex; i > reversedEndIndex && i >= 0; i-- )
            {
                var logEvent = GetLogEventFromLogLine( logs.ElementAt( i ) );
                if ( logEvent != null )
                {
                    results.Add( logEvent );
                }

            }

            return results;
        }

        private string[] GetLogLines( int startIndex, int count )
        {
            var rockLogFiles = _rockLogger.LogFiles;
            if ( rockLogFiles.Count == 0 )
            {
                return new string[] { };
            }

            var currentFileIndex = 0;
            try
            {
                var logs = System.IO.File.ReadAllLines( rockLogFiles[currentFileIndex] );

                while ( ( startIndex >= logs.Length || startIndex + count >= logs.Length ) && currentFileIndex < ( rockLogFiles.Count - 1 ) )
                {
                    currentFileIndex++;
                    var additionalLogs = System.IO.File.ReadAllLines( rockLogFiles[currentFileIndex] );
                    var temp = new string[additionalLogs.Length + logs.Length];
                    additionalLogs.CopyTo( temp, 0 );
                    logs.CopyTo( temp, additionalLogs.Length );

                    logs = temp;
                }

                return logs;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return new string[] { };
            }
        }

        private int GetRockLogRecordCount()
        {
            var rockLogFiles = _rockLogger.LogFiles;
            if ( rockLogFiles.Count == 0 )
            {
                return 0;
            }

            long lines = 0;
            foreach ( var filePath in rockLogFiles )
            {
                _rockLogger.Close();
                using ( var file = System.IO.File.OpenRead( filePath ) )
                {
                    lines += file.CountLines();
                }
            }

            if ( lines >= int.MaxValue )
            {
                return int.MaxValue;
            }
            return Convert.ToInt32( lines );
        }

        private RockLogLevel GetRockLogLevelFromSerilogLevel( LogEventLevel logLevel )
        {
            switch ( logLevel )
            {
                case ( LogEventLevel.Error ):
                    return RockLogLevel.Error;
                case ( LogEventLevel.Warning ):
                    return RockLogLevel.Warning;
                case ( LogEventLevel.Information ):
                    return RockLogLevel.Info;
                case ( LogEventLevel.Debug ):
                    return RockLogLevel.Debug;
                case ( LogEventLevel.Verbose ):
                    return RockLogLevel.All;
                default:
                    return RockLogLevel.Fatal;
            }
        }

        private RockLogEvent GetLogEventFromLogLine( string logLine )
        {
            try
            {
                var evt = LogEventReader.ReadFromString( logLine, _jsonSerializer );

                var domain = evt.Properties["domain"].ToString();
                evt.RemovePropertyIfPresent( "domain" );

                var message = evt.RenderMessage().Replace( "{domain}", "" ).Trim();
                domain = domain.Replace( "\"", "" );

                return new RockLogEvent
                {
                    DateTime = evt.Timestamp.DateTime.ToLocalTime(),
                    Exception = evt.Exception,
                    Level = GetRockLogLevelFromSerilogLevel( evt.Level ),
                    Domain = domain,
                    Message = message
                };
            }
            catch ( Exception ex )
            {
                // In rare instances it is possible that the event didn't get completely flushed to the log
                // and when that happens it won't be able to be correctly parsed so we need to handle that gracefully.
                ExceptionLogService.LogException( ex );
                return null;
            }
        }
    }
}
