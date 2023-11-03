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

using Rock.Data;
using System.Collections.Concurrent;
using System;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using Rock.SystemKey;
using System.Linq;

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
                if ( _targetedQueryHashes == null )
                {
                    var targetedQueryHashes = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES );

                    // If there aren't any hashes assign a empty list to prevent checking for every query and to prevent null reference exceptions.
                    if ( targetedQueryHashes.IsNullOrWhiteSpace() )
                    {
                        _targetedQueryHashes = new List<string>();
                    }

                    _targetedQueryHashes = targetedQueryHashes.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                }
                return _targetedQueryHashes;
            }
        }
        private static List<string> _targetedQueryHashes = null;
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
            var tableName = string.Empty;

            if ( iterationCount > 4 )
            {
                return "undefined";
            }

            var indexOfFrom = commandText.IndexOfNth( "from", iterationCount, StringComparison.OrdinalIgnoreCase );

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
            else
            {
                return "undefined";
            }

            return tableName;
        }
        #endregion
    }
}
