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
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Rock.Attribute;
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
    [RockInternal( "1.17", true )]
    public class RockSerilogReader : IRockLogReader
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly string _rockLogDirectory;
        private readonly string _searchPattern;

        private int _malformedLogEventCount = 0;

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
        /// <param name="configuration">The configuration for Serilog.</param>
        internal RockSerilogReader( SerilogConfiguration configuration )
        {
            _jsonSerializer = JsonSerializer.Create( new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                Culture = CultureInfo.InvariantCulture
            } );

            _rockLogDirectory = Path.GetFullPath( Path.GetDirectoryName( configuration.LogPath ) );

            _searchPattern = Path.GetFileNameWithoutExtension( configuration.LogPath ) +
                                "*" +
                                Path.GetExtension( configuration.LogPath );
        }

        /// <summary>
        /// Gets the log files.
        /// </summary>
        /// <value>
        /// The log files.
        /// </value>
        public List<string> GetLogFiles()
        {
            if ( !Directory.Exists( _rockLogDirectory ) )
            {
                return new List<string>();
            }

            return Directory.GetFiles( _rockLogDirectory, _searchPattern ).OrderByDescending( s => s ).ToList();
        }

        /// <summary>
        /// Deletes all of the log files.
        /// </summary>
        public void Delete()
        {
            if ( !Directory.Exists( _rockLogDirectory ) )
            {
                return;
            }

            foreach ( var file in GetLogFiles() )
            {
                try
                {
                    System.IO.File.Delete( file );
                }
                catch ( Exception ex )
                {
                    // If you get an exception it is probably because the file is in use
                    // and we can't delete it. So just move on.
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public List<RockLogEvent> GetEvents( int startIndex, int count )
        {
            var logs = GetLogLines( startIndex, count );

            return GetRockLogEventsFromLogLines( startIndex, count, logs );
        }

        private List<RockLogEvent> GetRockLogEventsFromLogLines( int startIndex, int count, string[] logs )
        {
            _malformedLogEventCount = 0;
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
            var rockLogFiles = GetLogFiles();
            if ( rockLogFiles.Count == 0 )
            {
                return new string[] { };
            }

            var currentFileIndex = 0;
            try
            {
                var logs = ReadAllLinesSharedAccess( rockLogFiles[currentFileIndex] );

                while ( ( startIndex >= logs.Length || startIndex + count >= logs.Length ) && currentFileIndex < ( rockLogFiles.Count - 1 ) )
                {
                    currentFileIndex++;
                    var additionalLogs = ReadAllLinesSharedAccess( rockLogFiles[currentFileIndex] );
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

        private string[] ReadAllLinesSharedAccess( string filename )
        {
            using ( var fs = File.Open( filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                var list = new List<string>();

                using ( StreamReader streamReader = new StreamReader( fs, Encoding.UTF8 ) )
                {
                    string item;

                    while ( ( item = streamReader.ReadLine() ) != null )
                    {
                        list.Add( item );
                    }
                }

                return list.ToArray();
            }
        }

        private int GetRockLogRecordCount()
        {
            var rockLogFiles = GetLogFiles();
            if ( rockLogFiles.Count == 0 )
            {
                return 0;
            }

            long lines = 0;

            try
            {
                foreach ( var filePath in rockLogFiles )
                {
                    var logFileInfo = new System.IO.FileInfo( filePath );

                    // if the logFile is zero-length, we'll get an i/o error when reading it, so skip it
                    if ( logFileInfo.Exists && logFileInfo.Length > 0 )
                    {
                        using ( var file = logFileInfo.Open( FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                        {
                            lines += file.CountLines();
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                // If you get an exception it is probably because a file is in use
                // and we can't read it. So just move on.
                ExceptionLogService.LogException( ex );
                return 0;
            }

            lines -= _malformedLogEventCount;

            if ( lines >= int.MaxValue )
            {
                return int.MaxValue;
            }

            return Convert.ToInt32( lines );
        }

        private Microsoft.Extensions.Logging.LogLevel GetRockLogLevelFromSerilogLevel( LogEventLevel logLevel )
        {
            switch ( logLevel )
            {
                case ( LogEventLevel.Fatal ):
                    return Microsoft.Extensions.Logging.LogLevel.Critical;
                case ( LogEventLevel.Error ):
                    return Microsoft.Extensions.Logging.LogLevel.Error;
                case ( LogEventLevel.Warning ):
                    return Microsoft.Extensions.Logging.LogLevel.Warning;
                case ( LogEventLevel.Information ):
                    return Microsoft.Extensions.Logging.LogLevel.Information;
                case ( LogEventLevel.Debug ):
                    return Microsoft.Extensions.Logging.LogLevel.Debug;
                case ( LogEventLevel.Verbose ):
                    return Microsoft.Extensions.Logging.LogLevel.Trace;
                default:
                    return Microsoft.Extensions.Logging.LogLevel.None;
            }
        }

        private RockLogEvent GetLogEventFromLogLine( string logLine )
        {
            try
            {
                var evt = LogEventReader.ReadFromString( logLine, _jsonSerializer );

                var category = evt.Properties["SourceContext"].ToString();
                evt.RemovePropertyIfPresent( "SourceContext" );

                var message = evt.RenderMessage();
                category = category.Replace( "\"", "" );

                return new RockLogEvent
                {
                    DateTime = evt.Timestamp.DateTime.ToLocalTime(),
                    Exception = evt.Exception,
                    LogLevel = GetRockLogLevelFromSerilogLevel( evt.Level ),
                    Category = category,
                    Message = message
                };
            }
            catch ( Exception ex )
            {
                // In rare instances it is possible that the event didn't get completely flushed to the log
                // and when that happens it won't be able to be correctly parsed so we need to handle that gracefully.
                ExceptionLogService.LogException( ex );
                _malformedLogEventCount++;
                return null;
            }
        }
    }
}
