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
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Rock.Observability;
using Rock.SystemKey;

namespace Rock.Web.Cache.NonEntities
{
    /// <summary>
    /// Class to help ensure that the observation of database commands is as performant as possible.
    /// </summary>
    public class DbCommandObservabilityCache : ItemCache<DbCommandObservabilityCache>
    {
        #region Properties

        /// <summary>
        /// The hash of the command.
        /// </summary>
        public string CommandHash { get; set; }

        /// <summary>
        /// The table name for the command.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The type of SQL command (select, update, insert, delete, exec)
        /// </summary>
        public string CommandType { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for the DbCommandTableCache
        /// </summary>
        public DbCommandObservabilityCache()
        {
            DefaultLifespan = TimeSpan.FromMinutes( 5 );
        }

        #endregion

        #region Private Variables

        // Create a list of valid SQL command names
        private static List<string> _commandNames = new List<string> { "SELECT", "DELETE", "UPDATE", "EXEC" };

        #endregion

        #region Properties

        /// <summary>
        /// This is a cached listed of query hashes that have been targeted to provide additional telemetry for. These are stored
        /// as system settings but are cached here as a list to improve performance.
        /// </summary>
        public static List<string> TargetedQueryHashes
        {
            get
            {
                var hashes = _targetedQueryHashes;

                if ( hashes != null )
                {
                    return hashes;
                }

                // We have to jump through some hoops since getting system
                // settings could cause a database query which could call
                // us again which could... and so forth until we hit a stack
                // overflow exception and crash. So set a default value
                // temporarily to prevent further database requests and then
                // start a task to load the actual values.
                _targetedQueryHashes = new List<string>();

                System.Threading.Tasks.Task.Run( () =>
                {
                    var targetedQueryHashes = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES );

                    // If there aren't any hashes assign a empty list to prevent
                    // checking for every query and to prevent null reference
                    // exceptions.
                    if ( targetedQueryHashes.IsNullOrWhiteSpace() )
                    {
                        _targetedQueryHashes = new List<string>();
                    }
                    else
                    {
                        _targetedQueryHashes = targetedQueryHashes.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    }
                } );

                // This value might be wrong for the first couple queries until
                // the task finishes, but I'm not sure there is anything we
                // can do about that.
                return new List<string>();
            }
        }
        private static List<string> _targetedQueryHashes = null;

        /// <summary>
        /// Gets a value indicating whether all query statements should be
        /// included with the activity.
        /// </summary>
        /// <value><c>true</c> if all query statements should be included; otherwise, <c>false</c>.</value>
        internal static bool IsQueryIncluded
        {
            get
            {
                var isQueryIncluded = _isQueryIncluded;

                if ( isQueryIncluded.HasValue )
                {
                    return isQueryIncluded.Value;
                }

                // We have to jump through some hoops since getting system
                // settings could cause a database query which could call
                // us again which could... and so forth until we hit a stack
                // overflow exception and crash. So set a default value
                // temporarily to prevent further database requests and then
                // start a task to load the actual values.
                _isQueryIncluded = false;

                System.Threading.Tasks.Task.Run( () =>
                {
                    _isQueryIncluded = SystemSettings.GetValue( SystemSetting.OBSERVABILITY_INCLUDE_QUERY_STATEMENTS ).AsBoolean();
                } );

                // This value might be wrong for the first couple queries until
                // the task finishes, but I'm not sure there is anything we
                // can do about that.
                return false;
            }
        }
        private static bool? _isQueryIncluded;

        #endregion

        #region Cache Methods

        /// <summary>
        /// Returns table name from cache.  If item does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public static DbCommandObservabilityCache Get( string command )
        {
            var hash = command.XxHash();

            return GetOrAddExisting( command, () => LoadByCommand( command, hash ) );
        }

        /// <summary>
        /// Clears the cache of targeted query hashes.
        /// </summary>
        public static void ClearTargetedQueryHashes()
        {
            _targetedQueryHashes = null;
            _isQueryIncluded = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the by command. This method was benchmark to run in 654.93 ns on a small SQL command and 4.29 μs on a larger one.
        /// </summary>
        /// <param name="commandText">The sql command.</param>
        /// <param name="hash">The hash of the command.</param>
        /// <returns></returns>
        private static DbCommandObservabilityCache LoadByCommand( string commandText, string hash )
        {
            commandText = commandText.Trim();

            var item = new DbCommandObservabilityCache();
            item.CommandHash = hash;

            // Start logic to determine the prefix
            var tableName = string.Empty;
            var sqlFirstWord = string.Empty;

            // Replace formatting characters with spaces. Example: UPDATE table\r\nSET
            commandText = commandText.Replace( '\r', ' ' ).Replace( '\n', ' ' );

            try
            {
                // Get the word from SQL
                sqlFirstWord = commandText.Substring( 0, commandText.IndexOf( " " ) ).ToUpper();

                // If the first word doesn't give us a clue then we'll call it a 'statement'
                if ( !_commandNames.Contains( sqlFirstWord ) )
                {
                    sqlFirstWord = "STATEMENT";
                }

                switch ( sqlFirstWord )
                {
                    case "SELECT":
                    case "DELETE":
                    case "STATEMENT":
                        {
                            tableName = ParseSelectForTable( commandText );
                            break;
                        }
                    case "UPDATE":
                        {
                            tableName = $"{commandText.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries )[1]}";
                            break;
                        }
                    case "INSERT":
                        {
                            // Note: INTO is optional in TSQL and is not used by EF
                            tableName = $"{commandText.Replace( "INTO", "" ).Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries )[1]}";
                            break;
                        }
                    case "EXEC":
                        {
                            tableName = $"{commandText.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries )[1]}";
                            break;
                        }
                }

                item.CommandType = sqlFirstWord;
            }
            catch ( Exception ) { }

            // Clean up the table a bit
            tableName = tableName.Replace( "[dbo].", "" ).Replace( "[", "" ).Replace( "]", "" ).Trim().ToLower();

            item.Prefix = $"{sqlFirstWord} {tableName}";

            return item;
        }

        /// <summary>
        /// Gets the table name from the SQL statement
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="iterationCount"></param>
        /// <returns></returns>
        private static string ParseSelectForTable( string commandText, int iterationCount = 0 )
        {
            var tableName = "undefined";

            if ( iterationCount > 4 )
            {
                return tableName;
            }

            var indexOfFrom = commandText.IndexOfNth( "from ", iterationCount, StringComparison.OrdinalIgnoreCase );

            if ( indexOfFrom > 0 )
            {
                tableName = commandText.Substring( indexOfFrom ).Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries )[1].Trim();

                // Check for
                // * SELECTS with sub queries (starts with a '(' )
                // * DELETES with alias (the alias for this in EF is a)
                if ( tableName.StartsWith( "(" ) || tableName == "a" )
                {
                    tableName = ParseSelectForTable( commandText, ( iterationCount + 1 ) );
                }
            }

            return tableName;
        }

        /// <summary>
        /// Updates the activity with custom tags for the database query. If
        /// the query is targetted for additional information then the factory
        /// method will be called to retrieve the query parameters.
        /// </summary>
        /// <typeparam name="T">The type of the context data to pass to the factory method.</typeparam>
        /// <param name="activity">The activity to update.</param>
        /// <param name="commandText">The SQL command text for updating the activity.</param>
        /// <param name="factoryContext">The factory context used for generating command parameters.</param>
        /// <param name="commandParametersFactory">The function that generates the command parameters based on the factory context.</param>
        internal static void UpdateActivity<T>( Activity activity, string commandText, T factoryContext, Func<T, IEnumerable<KeyValuePair<string, string>>> commandParametersFactory )
        {
            if ( activity == null )
            {
                return;
            }

            var observabilityInfo = Get( commandText );
            var isTargeted = TargetedQueryHashes.Contains( observabilityInfo.CommandHash );

            activity.DisplayName = $"DB: {observabilityInfo.Prefix} ({observabilityInfo.CommandHash})";
            activity.AddTag( "db.system", "mssql" );
            activity.AddTag( "rock.otel_type", "rock-db" );
            activity.AddTag( "rock.db.hash", observabilityInfo.CommandHash );

            ObservabilityHelper.IncrementDbQueryCount( activity );

            if ( isTargeted || IsQueryIncluded )
            {
                activity.AddTag( "db.query", commandText.Truncate( ObservabilityHelper.MaximumAttributeLength, false ) );
            }

            // Check if this query should get additional observability telemetry
            if ( isTargeted )
            {
                // Append stack trace
                var stackTrace = TrimInfrastructureFromStackTrace( new StackTrace( true ).ToString() );

                activity.AddTag( "rock.db.stacktrace", stackTrace.Truncate( ObservabilityHelper.MaximumAttributeLength ) );

                // Append parameters
                var parameters = new StringBuilder();
                foreach ( var keyValue in commandParametersFactory( factoryContext ) )
                {
                    parameters.Append( $"{keyValue.Key}: {keyValue.Value}{Environment.NewLine}" );
                }

                activity.AddTag( "rock.db.parameters", parameters.ToString().Truncate( ObservabilityHelper.MaximumAttributeLength ) );
            }

            // Add observability metric
            var tags = RockMetricSource.CommonTags;
            tags.Add( "operation", observabilityInfo.CommandType );
            RockMetricSource.DatabaseQueriesCounter?.Add( 1, tags );
        }

        /// <summary>
        /// Since we are limited on space in the tag values this trims off any
        /// stack frames that would be considered inconsequential from the
        /// start of the trace. Such as EF and our own internal code related
        /// to recording the activity information.
        /// </summary>
        /// <param name="stackTrace">The stack trace to be trimmed.</param>
        /// <returns>A string that contains the trimmed stack trace information.</returns>
        private static string TrimInfrastructureFromStackTrace( string stackTrace )
        {
            var stackTraceLines = stackTrace.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

            var includedLines = stackTraceLines.SkipWhile( line =>
                line.StartsWith( "   at Rock.Data.Interception." )
                || line.StartsWith( "   at Rock.Web.Cache.NonEntities.DbCommandObservabilityCache." )
                || line.StartsWith( "   at System.Data.Entity." ) );

            return string.Join( Environment.NewLine, includedLines );
        }

        #endregion
    }
}
