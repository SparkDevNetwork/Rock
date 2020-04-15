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
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
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
        private readonly IRockLogger rockLogger;
        private readonly string rockLogDirectory;
        private readonly string searchPattern;
        private readonly JsonSerializer jsonSerializer;

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
                /*
	                04/05/2020 - MSB 
	                The number of log records could be large and span multiple files we don't want to
                    incur the cost of counting the number of records in the log file because that would
                    mean reading every line in all files. So we are setting this value to a high value, but
                    we can't use int.MaxValue because that appears to cause some sort of issue in
                    the GridView.PageCount property and it would always return 1.

                    The purpose of this field is to return the number of log records that exists, and is used be the LogViewer to set the pagination.
                */
                return 2000000000;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSerilogReader"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public RockSerilogReader( IRockLogger logger )
        {
            rockLogger = logger;

            rockLogDirectory = System.IO.Path.GetFullPath( System.IO.Path.GetDirectoryName( rockLogger.LogConfiguration.LogPath ) );

            searchPattern = System.IO.Path.GetFileNameWithoutExtension( rockLogger.LogConfiguration.LogPath );
            searchPattern += "*";
            searchPattern += System.IO.Path.GetExtension( rockLogger.LogConfiguration.LogPath );

            jsonSerializer = JsonSerializer.Create( new JsonSerializerSettings
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
            rockLogger.Close();

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
                results.Add( GetLogEventFromLogLine( logs.ElementAt( i ) ) );
            }

            return results;
        }

        private string[] GetLogLines( int startIndex, int count )
        {
            if ( !System.IO.Directory.Exists( rockLogDirectory ) )
            {
                return new string[] { };
            }

            var rockLogFiles = System.IO.Directory.GetFiles( rockLogDirectory, searchPattern ).OrderByDescending( s => s ).ToList();
            if ( rockLogFiles.Count == 0 )
            {
                return new string[] { };
            }

            var currentFileIndex = 0;
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
            var evt = LogEventReader.ReadFromString( logLine, jsonSerializer );

            var domain = evt.Properties["domain"].ToString();
            evt.RemovePropertyIfPresent( "domain" );

            var message = evt.RenderMessage().Replace( "{domain}", "" ).Trim();
            domain = domain.Replace( "\"", "" );

            return new RockLogEvent
            {
                DateTime = evt.Timestamp.DateTime,
                Exception = evt.Exception,
                Level = GetRockLogLevelFromSerilogLevel( evt.Level ),
                Domain = domain,
                Message = message
            };
        }
    }
}
