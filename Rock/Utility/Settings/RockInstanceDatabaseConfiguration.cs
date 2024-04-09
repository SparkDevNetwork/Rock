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
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Rock.Utility.Settings
{
    /// <summary>
    /// Returns information about the database associated with a Rock instance.
    /// </summary>
    public class RockInstanceDatabaseConfiguration
    {
        #region Fields

        private string _readOnlyConnectionString = null;
        private string _analyticsConnectionString = null;

        private bool _versionInfoRetrieved = false;
        private string _versionNumber;
        private string _version;
        private string _versionFriendlyName;
        private string _databaseServerOperatingSystem;
        private string _databaseName = null;
        private string _serverName = null;
        private PlatformSpecifier? _platform;
        private bool? _snapshotIsolationAllowed;
        private bool? _readCommittedSnapshotEnabled;
        private string _recoverMode = null;
        private string _edition = null;
        private string _serviceObjective = null;
        private string _compatibility = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockInstanceDatabaseConfiguration"/> class.
        /// </summary>
        public RockInstanceDatabaseConfiguration()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockInstanceDatabaseConfiguration"/> class.
        /// </summary>
        /// <param name="connectionString"></param>
        public RockInstanceDatabaseConfiguration( string connectionString )
        {
            SetConnectionString( connectionString );
        }

        #endregion

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the database connection string for read-only replicas.
        /// If no read-only connection string has been configured then
        /// the standard connection string is returned.
        /// </summary>
        public string ReadOnlyConnectionString => _readOnlyConnectionString ?? ConnectionString;

        /// <summary>
        /// Gets the database connection string to use for analytics.
        /// If no analytics connection string has been configured then
        /// the standard connection string is returned.
        /// </summary>
        public string AnalyticsConnectionString => _analyticsConnectionString ?? ConnectionString;

        /// <summary>
        /// Set the database connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public void SetConnectionString( string connectionString )
        {
            if ( string.IsNullOrWhiteSpace( connectionString ) )
            {
                this.ConnectionString = null;
            }
            else
            {
                // Parse the connection string and store the server name and database name.
                var csBuilder = new SqlConnectionStringBuilder( connectionString );

                _serverName = csBuilder.DataSource;
                _databaseName = csBuilder.InitialCatalog;

                ConnectionString = connectionString;
            }

            // Reset all cached properties.
            _platform = PlatformSpecifier.Unknown;
            _versionInfoRetrieved = false;
            _versionNumber = null;
            _version = null;
            _versionFriendlyName = null;
            _databaseServerOperatingSystem = null;
            _snapshotIsolationAllowed = null;
            _readCommittedSnapshotEnabled = null;
            _edition = null;
            _recoverMode = null;
            _compatibility = null;
        }

        /// <summary>
        /// Sets the read-only replica connection string for this Rock instance.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void SetReadOnlyConnectionString( string connectionString )
        {
            _readOnlyConnectionString = connectionString;
        }

        /// <summary>
        /// Sets the analytics replica connection string for this Rock instance.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void SetAnalyticsConnectionString( string connectionString )
        {
            _analyticsConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the type of database platform on which the Rock database is hosted.
        /// </summary>
        public PlatformSpecifier Platform
        {
            get
            {
                if ( !_versionInfoRetrieved )
                {
                    GetPlatformAndVersionInfo();
                }

                return _platform.Value;
            }
        }

        /// <summary>
        /// Gets a flag indicating if READ COMMITTED SNAPSHOT isolation level is enabled for the database.
        /// If this isolation level is enabled, the database does not hold record locks during the reading phase of a transaction.
        /// </summary>
        public bool ReadCommittedSnapshotEnabled
        {
            get
            {
                if ( _readCommittedSnapshotEnabled == null )
                {
                    GetSnapshotSettings();
                }

                return _readCommittedSnapshotEnabled.Value;
            }
        }

        /// <summary>
        /// Gets a flag indicating if snapshot isolation is enabled for the database.
        /// If this feature is available, each transaction operates on a snapshot of the database in isolation from other concurrent operations.
        /// </summary>
        public bool SnapshotIsolationAllowed
        {
            get
            {
                if ( _snapshotIsolationAllowed == null )
                {
                    GetSnapshotSettings();
                }

                return _snapshotIsolationAllowed.Value;
            }
        }

        /// <summary>
        /// Gets the size of the database, measured in megabytes (MB).
        /// </summary>
        public decimal? DatabaseSize
        {
            get
            {
                // Query to retrieve the database size, but do not cache the result because it may change for each request.
                try
                {
                    var sql = @"
SELECT CAST( SUM(size* 8.0 ) / 1024.0 AS NUMERIC( 12, 2 ) ) AS 'Db Size (MB)'
FROM   sys.database_files
WHERE  data_space_id = 1
";
                    var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        string size = reader.GetValue( 0 ).ToString();

                        decimal sizeInMb;

                        var isValid = decimal.TryParse( reader.GetValue( 0 ).ToString(), out sizeInMb );

                        if ( isValid )
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
        }

        /// <summary>
        /// Gets the size of the database log, measured in megabytes (MB).
        /// </summary>
        public decimal? LogSize
        {
            get
            {
                // Query to retrieve the log size, but do not cache the result because it may change for each request.
                try
                {
                    var sql = @"
SELECT CAST( (size* 8.0)/ 1024.0 AS NUMERIC(12,2) ) AS 'Db Log Size (MB)'
FROM   sys.database_files
WHERE  data_space_id = 0
";
                    var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        string size = reader.GetValue( 0 ).ToString();

                        decimal sizeInMb;

                        var isValid = decimal.TryParse( reader.GetValue( 0 ).ToString(), out sizeInMb );

                        if ( isValid )
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
        }

        /// <summary>
        /// Gets the database platform version number string.
        /// </summary>
        public string VersionNumber
        {
            get
            {
                if ( !_versionInfoRetrieved )
                {
                    GetPlatformAndVersionInfo();
                }

                return _versionNumber;
            }
        }

        /// <summary>
        /// Gets the database platform version string.
        /// </summary>
        public string Version
        {
            get
            {
                if ( !_versionInfoRetrieved )
                {
                    GetPlatformAndVersionInfo();
                }

                return _version;
            }
        }

        /// <summary>
        /// Gets a user-friendly description of the database platform version.
        /// </summary>
        public string VersionFriendlyName
        {
            get
            {
                if ( !_versionInfoRetrieved )
                {
                    GetPlatformAndVersionInfo();
                }

                return _versionFriendlyName;
            }

        }

        /// <summary>
        /// Gets a description of the database server edition or product variant.
        /// </summary>
        public string Edition
        {
            get
            {
                // This needs to be retrieved from the database at the time it's requested as this
                // may change while the service is running (e.g., Azure Sql service scaling).
                GetServiceObjectiveInfo();
                return _edition;
            }
        }

        /// <summary>
        /// Gets a description of the database server RecoverMode or product variant.
        /// </summary>
        public string RecoverMode
        {
            get
            {
                if ( _recoverMode == null )
                {
                    try
                    {
                        var sql = @"
SELECT recovery_model_desc
FROM   sys.databases
WHERE  name = DB_NAME()
";

                        var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                        if ( reader != null )
                        {
                            reader.Read();

                            _recoverMode = reader.GetValue( 0 ).ToString();
                        }
                    }
                    catch
                    {
                        // Ignore errors and continue.
                        _recoverMode = "#ERROR#";
                    }
                }

                return _recoverMode;
            }
        }

        /// <summary>
        /// Gets a description of the expected capability of the database platform, or null if the capability cannot be determined.
        /// </summary>
        public string ServiceObjective
        {
            get
            {
                // This needs to be retrieved from the database at the time it's requested as this
                // may change while the service is running (e.g., Azure Sql service scaling).
                GetServiceObjectiveInfo();

                return _serviceObjective;
            }
        }

        /// <summary>
        /// Get the name of the operating system on which the database server is hosted.
        /// </summary>
        public string DatabaseServerOperatingSystem
        {
            get
            {
                if ( !_versionInfoRetrieved )
                {
                    GetPlatformAndVersionInfo();
                }

                return _databaseServerOperatingSystem;
            }
        }

        /// <summary>
        /// Gets the name of the database server.
        /// </summary>
        public string ServerName
        {
            get
            {
                return _serverName;
            }
        }

        /// <summary>
        /// Gets the name of the database instance.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }
        }

        /// <summary>
        /// Gets the Compatibility Level of the database
        /// </summary>
        public int CompatibilityLevel
        {
            get
            {
                if ( string.IsNullOrWhiteSpace( _compatibility ) )
                {
                    GetCompatibilityLevel();
                }

                return int.Parse( _compatibility );
            }
        }

        /// <summary>
        /// Gets the compatibility version of the database.
        /// </summary>
        public string CompatibilityVersion
        {
            get
            {
                switch ( CompatibilityLevel )
                {
                    case 160:
                        return "SQL Server 2022";
                    case 150:
                        return "SQL Server 2019";
                    case 140:
                        return "SQL Server 2017";
                    case 130:
                        return "SQL Server 2016";
                    case 120:
                        return "SQL Server 2014";
                    case 110:
                        return "SQL Server 2012";
                    case 100:
                        return "SQL Server 2008";
                    case 90:
                        return "SQL Server 2005";
                    case 80:
                        return "SQL Server 2000";
                    default:
                        return CompatibilityLevel.ToString();
                }
            }
        }

        private void GetPlatformAndVersionInfo()
        {
            _versionInfoRetrieved = true;

            string versionNumber = string.Empty;
            string version = string.Empty;
            string dbVersion = string.Empty;
            string editionAndPlatformInfo = string.Empty;

            try
            {
                var sql = @"
SELECT SERVERPROPERTY('productversion'), @@Version;
";

                var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                if ( reader != null )
                {
                    if ( reader.Read() )
                    {
                        versionNumber = reader[0].ToString();
                        version = reader[1].ToString();

                        var versionInfo = version.SplitDelimitedValues( "\n" );

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
                _platform = PlatformSpecifier.AzureSql;
                _databaseServerOperatingSystem = "Azure";
            }
            else if ( dbVersion.StartsWith( "Microsoft SQL Server", System.StringComparison.OrdinalIgnoreCase ) )
            {
                _platform = PlatformSpecifier.SqlServer;
            }
            else
            {
                _platform = PlatformSpecifier.Other;
            }

            _versionNumber = versionNumber;
            _version = version;

            // Parse Version Friendly Name.
            if ( _versionNumber.StartsWith( "11.0" ) )
            {
                _versionFriendlyName = "SQL Server 2012";
            }
            else if ( _versionNumber.StartsWith( "12.0" ) )
            {
                _versionFriendlyName = "SQL Server 2014";
            }
            else if ( _versionNumber.StartsWith( "13.0" ) )
            {
                _versionFriendlyName = "SQL Server 2016";
            }
            else if ( _versionNumber.StartsWith( "14.0" ) )
            {
                _versionFriendlyName = "SQL Server 2017";
            }
            else if ( _versionNumber.StartsWith( "15.0" ) )
            {
                _versionFriendlyName = "SQL Server 2019";
            }
            else if ( _versionNumber.StartsWith( "16.0" ) )
            {
                _versionFriendlyName = "SQL Server 2022";
            }
            else
            {
                _versionFriendlyName = "Unknown";
            }

            // Parse OS Version
            if ( _platform != PlatformSpecifier.AzureSql )
            {
                _databaseServerOperatingSystem = editionAndPlatformInfo.SplitDelimitedValues( " on " )
                    .ToList()
                    .LastOrDefault()
                    .ToStringSafe()
                    .Trim();
            }
        }

        private void GetSnapshotSettings()
        {
            // Get database snapshot isolation details.
            try
            {
                var sql = string.Format( @"
SELECT [snapshot_isolation_state]
       ,[is_read_committed_snapshot_on]
FROM   sys.databases WHERE [name] = '{0}'
"
, _databaseName );

                var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                if ( reader != null )
                {
                    while ( reader.Read() )
                    {
                        _snapshotIsolationAllowed = reader[0].ToStringSafe().AsBoolean();
                        _readCommittedSnapshotEnabled = reader[1].ToStringSafe().AsBoolean();
                    }
                }
            }
            catch
            {
                _snapshotIsolationAllowed = null;
                _readCommittedSnapshotEnabled = null;
            }

        }

        private void GetCompatibilityLevel()
        {
            try
            {
                var sql = @"
SELECT compatibility_level
FROM   sys.databases
WHERE  name = DB_NAME()
";

                var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                if ( reader != null )
                {
                    reader.Read();

                    _compatibility = reader.GetValue( 0 ).ToString();
                }
            }
            catch
            {
                // Ignore errors and continue.
                _compatibility = "0";
            }
        }

        private void GetServiceObjectiveInfo()
        {
            try
            {
                var platform = this.Platform;

                if ( platform == PlatformSpecifier.SqlServer )
                {
                    var sql = @"SELECT SERVERPROPERTY('Edition');";

                    var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        _edition = reader.GetValue( 0 ).ToString();
                    }
                }
                else if ( platform == PlatformSpecifier.AzureSql )
                {
                    var sql = @"
SELECT slo.edition
       ,slo.service_objective
FROM sys.databases d
JOIN sys.database_service_objectives slo
ON d.database_id = slo.database_id
WHERE d.name = '<db_name>';
";
                    sql = sql.Replace( "<db_name>", _databaseName );

                    var reader = GetDataReader( sql, System.Data.CommandType.Text, null );

                    if ( reader != null )
                    {
                        reader.Read();

                        _edition = reader.GetValue( 0 ).ToString();
                        _serviceObjective = reader.GetValue( 1 ).ToString();

                    }
                }
                else
                {
                    _edition = "(unknown)";
                }
            }
            catch
            {
                // Ignore errors and continue.
                _edition = "#ERROR#";
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

            if ( string.IsNullOrWhiteSpace( this.ConnectionString ) )
            {
                return null;
            }

            var con = new SqlConnection( this.ConnectionString );
            con.Open();

            var sqlCommand = new SqlCommand( query, con );
            sqlCommand.CommandType = commandType;

            if ( parameters != null )
            {
                foreach ( var parameter in parameters )
                {
                    var sqlParam = new SqlParameter();
                    sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                    sqlParam.Value = parameter.Value;
                    sqlCommand.Parameters.Add( sqlParam );
                }
            }

            return sqlCommand.ExecuteReader( CommandBehavior.CloseConnection );
        }

        #region Enumerations

        /// <summary>
        /// A database server platform that is capable of hosting an instance of a Rock database.
        /// </summary>
        public enum PlatformSpecifier
        {
            /// <summary>
            /// The database platform is unknown.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// The database platform is an edition of Microsoft SQL Server.
            /// </summary>
            SqlServer = 1,

            /// <summary>
            /// The database is hosted on the Azure platform.
            /// </summary>
            AzureSql = 2,

            /// <summary>
            /// The database is hosted on an unspecified platform.
            /// </summary>
            Other = 3
        }

        #endregion
    }
}
