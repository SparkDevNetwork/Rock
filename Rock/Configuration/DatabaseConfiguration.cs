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

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

using Rock.Enums.Configuration;

namespace Rock.Configuration
{
    /// <inheritdoc cref="IDatabaseConfiguration" path="/summary"/>
    internal class DatabaseConfiguration : IDatabaseConfiguration
    {
        #region Fields

        private readonly string _connectionString;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDatabaseAvailable { get; set; }

        /// <inheritdoc/>
        public DatabasePlatform Platform { get; private set; }

        /// <inheritdoc/>
        public bool IsReadCommittedSnapshotEnabled { get; private set; }

        /// <inheritdoc/>
        public bool IsSnapshotIsolationAllowed { get; private set; }

        /// <inheritdoc/>
        public string VersionNumber { get; private set; }

        /// <inheritdoc/>
        public string Version { get; private set; }

        /// <inheritdoc/>
        public string Edition { get; private set; }

        /// <inheritdoc/>
        public string RecoveryModel { get; private set; }

        /// <inheritdoc/>
        public string ServiceObjective { get; private set; }

        /// <inheritdoc/>
        public string DatabaseServerOperatingSystem { get; private set; }

        /// <inheritdoc/>
        public string ServerName { get; private set; }

        /// <inheritdoc/>
        public string DatabaseName { get; private set; }

        /// <inheritdoc/>
        public int CompatibilityLevel { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfiguration"/> class.
        /// </summary>
        /// <param name="settings">The initialization settings for the connection string.</param>
        public DatabaseConfiguration( IInitializationSettings settings )
        {
            if ( string.IsNullOrWhiteSpace( settings.ConnectionString ) )
            {
                _connectionString = string.Empty;
            }
            else
            {
                // Parse the connection string and store the server name and database name.
                var csBuilder = new SqlConnectionStringBuilder( settings.ConnectionString );

                ServerName = csBuilder.DataSource;
                DatabaseName = csBuilder.InitialCatalog;

                _connectionString = settings.ConnectionString;
            }

            PopulatePlatformAndVersionInfo();
            PopulateSnapshotSettings();
            PopulateCompatibilityLevel();
            PopulateServiceObjectiveInfo();
            PopulateRecoveryModel();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public decimal? GetDatabaseSize()
        {
            return GetStorageSize( 1 );
        }

        /// <inheritdoc/>
        public decimal? GetLogSize()
        {
            return GetStorageSize( 0 );
        }

        /// <summary>
        /// Gets the size of the specific storage type in MB.
        /// </summary>
        /// <param name="storageId">The storage type.</param>
        /// <returns>The number of MB used.</returns>
        private decimal? GetStorageSize( int storageId )
        {
            // Query to retrieve the database size, but do not cache the result because it may change for each request.
            try
            {
                var sql = $@"
SELECT CAST( SUM(size* 8.0 ) / 1024.0 AS NUMERIC( 12, 2 ) ) AS 'Db Size (MB)'
FROM   sys.database_files
WHERE  data_space_id = {storageId}
";
                var reader = GetDataReader( sql, CommandType.Text, null );

                if ( reader != null )
                {
                    reader.Read();

                    var size = reader.GetValue( 0 ).ToString();

                    if ( decimal.TryParse( size, out var sizeInMb ) )
                    {
                        return sizeInMb;
                    }
                }
            }
            catch
            {
                // Ignore errors and continue.
            }

            return null;
        }

        /// <summary>
        /// Populates properties about the platform and version.
        /// </summary>
        private void PopulatePlatformAndVersionInfo()
        {
            var dbVersion = string.Empty;
            var editionAndPlatformInfo = string.Empty;

            try
            {
                var sql = @"
SELECT SERVERPROPERTY('productversion'), @@Version;
";

                var reader = GetDataReader( sql, CommandType.Text, null );

                if ( reader != null )
                {
                    if ( reader.Read() )
                    {
                        VersionNumber = reader[0].ToString();
                        Version = reader[1].ToString();

                        var versionInfo = Version.SplitDelimitedValues( "\n" );

                        dbVersion = versionInfo[0];
                        editionAndPlatformInfo = versionInfo[3];
                    }
                }
            }
            catch
            {
                // Ignore and continue.
            }

            // Parse Version Description
            if ( dbVersion.StartsWith( "Microsoft SQL Azure", System.StringComparison.OrdinalIgnoreCase ) )
            {
                Platform = DatabasePlatform.AzureSql;
                DatabaseServerOperatingSystem = "Azure";
            }
            else if ( dbVersion.StartsWith( "Microsoft SQL Server", System.StringComparison.OrdinalIgnoreCase ) )
            {
                Platform = DatabasePlatform.SqlServer;
            }
            else
            {
                Platform = DatabasePlatform.Other;
            }

            // Parse OS Version
            if ( Platform != DatabasePlatform.AzureSql )
            {
                DatabaseServerOperatingSystem = editionAndPlatformInfo.SplitDelimitedValues( " on " )
                    .ToList()
                    .LastOrDefault()
                    .ToStringSafe()
                    .Trim();
            }
        }

        /// <summary>
        /// Populates the snapshot settings from the database.
        /// </summary>
        private void PopulateSnapshotSettings()
        {
            // Get database snapshot isolation details.
            try
            {
                var sql = string.Format( @"
SELECT [snapshot_isolation_state]
       ,[is_read_committed_snapshot_on]
FROM   sys.databases WHERE [name] = '{0}'
"
, DatabaseName );

                var reader = GetDataReader( sql, CommandType.Text, null );

                if ( reader != null )
                {
                    while ( reader.Read() )
                    {
                        IsSnapshotIsolationAllowed = reader[0].ToStringSafe().AsBoolean();
                        IsReadCommittedSnapshotEnabled = reader[1].ToStringSafe().AsBoolean();
                    }
                }
            }
            catch
            {
                IsSnapshotIsolationAllowed = false;
                IsReadCommittedSnapshotEnabled = false;
            }

        }

        /// <summary>
        /// Populates the compatibility level property.
        /// </summary>
        private void PopulateCompatibilityLevel()
        {
            try
            {
                var sql = @"
SELECT compatibility_level
FROM   sys.databases
WHERE  name = DB_NAME()
";

                var reader = GetDataReader( sql, CommandType.Text, null );

                if ( reader != null )
                {
                    reader.Read();

                    CompatibilityLevel = reader.GetValue( 0 ).ToString().AsInteger();
                }
            }
            catch
            {
                // Ignore errors and continue.
                CompatibilityLevel = 0;
            }
        }

        /// <summary>
        /// Populates the service objective property.
        /// </summary>
        private void PopulateServiceObjectiveInfo()
        {
            try
            {
                if ( Platform == DatabasePlatform.SqlServer )
                {
                    var sql = @"SELECT SERVERPROPERTY('Edition');";

                    var reader = GetDataReader( sql, CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        Edition = reader.GetValue( 0 ).ToString();
                    }
                }
                else if ( Platform == DatabasePlatform.AzureSql )
                {
                    var sql = @"
SELECT slo.edition
       ,slo.service_objective
FROM sys.databases d
JOIN sys.database_service_objectives slo
ON d.database_id = slo.database_id
WHERE d.name = '<db_name>';
";
                    sql = sql.Replace( "<db_name>", DatabaseName );

                    var reader = GetDataReader( sql, CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        Edition = reader.GetValue( 0 ).ToString();
                        ServiceObjective = reader.GetValue( 1 ).ToString();

                    }
                }
                else
                {
                    Edition = "(unknown)";
                }
            }
            catch
            {
                // Ignore errors and continue.
                Edition = "#ERROR#";
            }
        }

        private void PopulateRecoveryModel()
        {
            try
            {
                var sql = @"
SELECT recovery_model_desc
FROM   sys.databases
WHERE  name = DB_NAME()
";

                var reader = GetDataReader( sql, CommandType.Text, null );

                if ( reader != null )
                {
                    reader.Read();

                    RecoveryModel = reader.GetValue( 0 ).ToString();
                }
            }
            catch
            {
                // Ignore errors and continue.
                RecoveryModel = "#ERROR#";
            }
        }

        /// <summary>
        /// Gets a data reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            /* [2020-04-17] DL
             * 1. This function replicates DbService.GetDataReader(), but removes the dependency
             * on the configuration file connection string to make this component test-friendly.
             * 2. Entity Framework data access methods are intentionally avoided, because attempting to retrieve
             * database properties may fail in situations where the data model does not match the target database.
             */

            if ( string.IsNullOrWhiteSpace( _connectionString ) )
            {
                return null;
            }

            var con = new SqlConnection( _connectionString );
            con.Open();

            var sqlCommand = new SqlCommand( query, con )
            {
                CommandType = commandType
            };

            if ( parameters != null )
            {
                foreach ( var parameter in parameters )
                {
                    var sqlParam = new SqlParameter
                    {
                        ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key,
                        Value = parameter.Value
                    };
                    sqlCommand.Parameters.Add( sqlParam );
                }
            }

            return sqlCommand.ExecuteReader( CommandBehavior.CloseConnection );
        }

        #endregion
    }
}
