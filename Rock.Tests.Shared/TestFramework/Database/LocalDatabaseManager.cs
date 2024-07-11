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
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Manages Rock databases for a LocalDb instance used during developer testing.
    /// </summary>
    public class LocalDatabaseManager
    {
        private bool _IsDatabaseInitialized = false;

        public string DatabaseCreatorKey = "RockIntegrationTestProject";

        /// <summary>
        /// The connection string of the target database.
        /// </summary>
        public string ConnectionString = null;

        /// <summary>
        /// Specifies a target migration for the test database.
        /// If not specified, the most recent migration is targeted.
        /// </summary>
        public string TargetMigrationName = null;

        /// <summary>
        /// An optional initializer that can configure a new database and add sample data.
        /// </summary>
        public ITestDatabaseInitializer DatabaseInitializer = null;

        /// <summary>
        /// Specifies the number of days after which a database image file will be removed from the local archive.
        /// If set to 0, archive files will never be removed.
        /// </summary>
        public int ArchiveRetentionDays = 180;

        #region Public Properties and Methods

        /// <summary>
        /// Can the target database be reset by restoring from an archived database image.
        /// This will result in the loss of existing data.
        /// </summary>
        public bool IsDatabaseResetPermitted { get; set; } = false;

        /// <summary>
        /// Can the target database be reset by restoring from an archived database image.
        /// This will result in the loss of existing data.
        /// </summary>
        public bool IsCreatorKeyVerificationRequiredForDatabaseReset { get; set; } = false;

        /// <summary>
        /// Initializes a new database and loads sample data.
        /// </summary>
        public bool InitializeTestDatabase()
        {
            if ( _IsDatabaseInitialized )
            {
                return true;
            }

            _IsDatabaseInitialized = true;

            var success = InitializeSqlServerDatabaseForLocal( false );
            return success;
        }


        /// <summary>
        /// Initializes a new database and loads sample data.
        /// </summary>
        public bool InitializeTestDatabase( bool rebuildArchiveImage )
        {
            _IsDatabaseInitialized = true;

            //var forceReplaceExisting = ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Rebuild );
            var success = InitializeSqlServerDatabaseForLocal( rebuildArchiveImage );
            return success;
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <param name="connectionString">The connection.</param>
        /// <returns><c>true</c> if the named database exists; otherwise <c>false</c>.</returns>
        public bool DatabaseExists( string connectionString )
        {
            var csb = new SqlConnectionStringBuilder( connectionString );
            var dbName = csb.InitialCatalog;
            csb.InitialCatalog = "master";
            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM [sysdatabases] WHERE [name] = @dbName";
                    cmd.Parameters.AddWithValue( "dbName", dbName );

                    return ( int ) cmd.ExecuteScalar() != 0;
                }
            }
        }

        /// <summary>
        /// Deletes the database from the SQL server so the next test can start clean.
        /// </summary>
        public void DeleteDatabase()
        {
            DeleteDatabase( ConnectionString );
        }

        /// <summary>
        /// Resets the database by using the default data source.
        /// </summary>
        public void ResetDatabase()
        {
            if ( IsLocalDbInstance() )
            {
                if ( !DatabaseExists( ConnectionString ) )
                {
                    InitializeTestDatabase();
                }

                // Reset the database from the archive image.
                InitializeSqlServerDatabaseForLocal( recreateArchive: false );
            }
        }

        /// <summary>
        /// Runs the plugin migrations for the specified plugin assembly.
        /// </summary>
        /// <param name="pluginAssembly">The plugin assembly.</param>
        /// <returns></returns>
        /// <remarks>
        /// This code replicates the functionality of <see cref="RockApplicationStartupHelper.RunPluginMigrations(Assembly)"/>
        /// but removes dependencies on other aspects of the Rock application startup process.
        /// </remarks>
        public void ApplyPluginMigrations( Assembly pluginAssembly )
        {
            var pluginAssemblyName = pluginAssembly.GetName().Name;

            LogHelper.Log( $"Applying migrations from assembly \"{pluginAssemblyName}\"..." );

            // Migrate any plugins from the plugin assembly that have pending migrations
            var pluginMigrationTypes = Rock.Reflection.SearchAssembly( pluginAssembly, typeof( Rock.Plugin.Migration ) ).Select( a => a.Value ).ToList();

            // If any plugin migrations types were found
            if ( !pluginMigrationTypes.Any() )
            {
                return;
            }

            // put the migrations to run in a Dictionary so that we can run them in the correct order
            // based on MigrationNumberAttribute
            var migrationTypesByNumber = new Dictionary<int, Type>();

            // Iterate plugin migrations
            var migrationNumber = 0;

            foreach ( var migrationType in pluginMigrationTypes )
            {
                // Get the MigrationNumberAttribute for the migration
                var migrationNumberAttr = migrationType.GetCustomAttribute<Rock.Plugin.MigrationNumberAttribute>();
                if ( migrationNumberAttr != null )
                {
                    migrationNumber = migrationNumberAttr.Number;
                }
                else
                {
                    migrationNumber++;
                }

                migrationTypesByNumber.Add( migrationNumber, migrationType );
            }

            // Create EF service for plugin migrations
            var rockContext = new RockContext();
            var pluginMigrationService = new PluginMigrationService( rockContext );

            // Get the versions that have already been installed
            var installedMigrationNumbers = pluginMigrationService.Queryable()
                .Where( m => m.PluginAssemblyName == pluginAssemblyName )
                .Select( a => a.MigrationNumber ).ToArray();

            // narrow it down to migrations that haven't already been installed
            migrationTypesByNumber = migrationTypesByNumber
                .Where( a => !installedMigrationNumbers.Contains( a.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );

            // Iterate each migration in the assembly in MigrationNumber order 
            var migrationTypesToRun = migrationTypesByNumber.OrderBy( a => a.Key )
                .Select( a => a.Value )
                .ToList();

            LogHelper.Log( $"Found {migrationTypesToRun.Count} migrations in assembly." );

            if ( !migrationTypesToRun.Any() )
            {
                return;
            }

            using ( var sqlConnection = new SqlConnection( this.ConnectionString ) )
            {
                LogHelper.Log( $"Applying migrations to target database \"{sqlConnection.Database} \"..." );

                try
                {
                    sqlConnection.Open();
                }
                catch ( SqlException ex )
                {
                    throw new Exception( "Error connecting to the SQL database. Please check the 'RockContext' connection string in the web.ConnectionString.config file.", ex );
                }

                // Iterate thru each plugin migration in this assembly, if one fails, will log the exception and stop running migrations for this assembly
                foreach ( Type migrationType in migrationTypesToRun )
                {
                    migrationNumber = migrationType.GetCustomAttribute<Rock.Plugin.MigrationNumberAttribute>()?.Number ?? 0;

                    using ( var sqlTxn = sqlConnection.BeginTransaction() )
                    {
                        var transactionActive = true;
                        try
                        {
                            // Create an instance of the migration and run the up migration
                            var migration = Activator.CreateInstance( migrationType ) as Rock.Plugin.Migration;

                            LogHelper.Log( $"Applying migration \"{migrationType.Name}\"..." );

                            migration.SqlConnection = sqlConnection;
                            migration.SqlTransaction = sqlTxn;
                            migration.Up();
                            sqlTxn.Commit();
                            transactionActive = false;

                            // Save the plugin migration version so that it is not run again
                            if ( migrationNumber > 0 )
                            {
                                var pluginMigration = new PluginMigration();
                                pluginMigration.PluginAssemblyName = pluginAssemblyName;
                                pluginMigration.MigrationNumber = migrationNumber;
                                pluginMigration.MigrationName = migrationType.Name;
                                pluginMigrationService.Add( pluginMigration );
                                rockContext.SaveChanges();
                            }
                        }
                        catch ( Exception ex )
                        {
                            if ( transactionActive )
                            {
                                sqlTxn.Rollback();
                            }

                            throw new Exception( $"##Plugin Migration error occurred in {migrationNumber}, {migrationType.Name}##", ex );
                        }
                    }
                }
            }
        }

        #endregion

        private bool IsLocalDbInstance()
        {
            var csb = new SqlConnectionStringBuilder( this.ConnectionString );

            if ( csb.DataSource != null
                && csb.DataSource.StartsWith( @"(LocalDB)\", StringComparison.OrdinalIgnoreCase ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the target migration.
        /// </summary>
        private string GetTargetMigration()
        {
            if ( _targetMigration == null )
            {
                var targetMigrations = typeof( Rock.Migrations.RockMigration )
                    .Assembly
                    .GetExportedTypes()
                       .Where( a => typeof( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ).IsAssignableFrom( a ) )
                    .Select( a => ( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ) Activator.CreateInstance( a ) )
                    .Select( a => a.Id )
                    .OrderByDescending( a => a )
                    .ToList();

                _latestMigration = targetMigrations.FirstOrDefault();

                var targetMigrationName = this.TargetMigrationName.ToStringSafe().Trim().ToLower();
                if ( targetMigrationName.IsNullOrWhiteSpace() )
                {
                    _targetMigration = targetMigrations.FirstOrDefault();
                }
                else
                {
                    // Find a matching migration name, allowing for partial matches that do not include the datetime prefix.
                    targetMigrations = targetMigrations.Where( m => m.EndsWith( targetMigrationName, StringComparison.InvariantCultureIgnoreCase ) )
                        .ToList();
                    if ( !targetMigrations.Any() )
                    {
                        throw new Exception( $"Target Migration is invalid. No match found. [TargetMigrationName={targetMigrationName}]" );
                    }
                    else
                    {
                        if ( targetMigrations.Count() > 1 )
                        {
                            throw new Exception( $"Target Migration is invalid. Multiple matches found. [TargetMigrationName={targetMigrationName}]" );
                        }

                        _targetMigration = targetMigrations.First();
                    }
                }
            }

            return _targetMigration;
        }

        private string _targetMigration;
        private string _latestMigration;

        /// <summary>
        /// Migrates the database.
        /// </summary>
        private void MigrateDatabase( string connectionString )
        {
            var connection = new DbConnectionInfo( connectionString, "System.Data.SqlClient" );

            var config = new Rock.Migrations.Configuration();
            config.TargetDatabase = connection;

            var migrator = new System.Data.Entity.Migrations.DbMigrator( config );

            LogHelper.Log( $"Applying database migrations... [Target=\"{_targetMigration}\"]" );
            try
            {
                migrator.Update( _targetMigration );
            }
            catch ( Exception ex )
            {
                throw new Exception( "Test Database migration failed. Verify that the database connection string specified in the test project is valid. You may need to manually synchronize the database or configure the test environment to force-create a new database.", ex );
            }
        }

        /// <summary>
        /// Gets the data path where we will store files.
        /// </summary>
        /// <returns>A string that contains the full path to our temporary folder.</returns>
        private string GetDataPath()
        {
            string path = Path.Combine( Directory.GetCurrentDirectory(), "Data" );

            if ( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }

            return path;
        }

        /// <summary>
        /// Gets the path where our temporary files are stored.
        /// </summary>
        /// <returns>A string containing the path to temporary files.</returns>
        private string GetTempPath()
        {
            string tempPath = Path.Combine( Path.GetTempPath(), "RockUnitTests" );

            if ( !Directory.Exists( tempPath ) )
            {
                Directory.CreateDirectory( tempPath );
            }

            return tempPath;
        }

        /// <summary>
        /// Resets the database by using the default data source.
        /// </summary>
        private bool InitializeSqlServerDatabaseForLocal( bool recreateArchive )
        {
            var restoreDatabase = this.IsDatabaseResetPermitted;

            if ( DatabaseExists( ConnectionString ) )
            {
                // If the database exists and should never be replaced, continue without verifying the migration level.
                if ( !this.IsDatabaseResetPermitted )
                {
                    return true;
                }

                if ( recreateArchive )
                {
                    // Remove the database from the local server.
                    DeleteDatabase( ConnectionString );
                }
            }
            else
            {
                // If the database does not exist, it must be restored.
                restoreDatabase = true;
            }

            if ( recreateArchive )
            {
                var fileName = GetArchiveFileName();
                DeleteArchiveFile( new FileInfo( fileName ) );

                restoreDatabase = true;
            }

            if ( restoreDatabase )
            {
                // Restore the database from the archive.
                var testSource = GetOrGenerateLocalDatabaseArchive( ConnectionString );

                RestoreLocalDatabaseFromArchive( ConnectionString, testSource );
            }

            RockCache.ClearAllCachedItems();

            return true;
        }

        /// <summary>
        /// Replaces the database specified in the connection string with the image stored in the specified archive file.
        /// </summary>
        /// <param name="archivePath">The archive path that contains the MDF and LDF files.</param>
        private void RestoreLocalDatabaseFromArchive( string connectionString, string archivePath )
        {
            var csbTarget = new SqlConnectionStringBuilder( connectionString );
            var targetDbName = csbTarget.InitialCatalog;

            var csbMaster = new SqlConnectionStringBuilder( connectionString );
            csbMaster.InitialCatalog = "master";

            LogHelper.Log( $"Restoring local database from archive..." );
            LogHelper.Log( $"Target database is \"{targetDbName}\"." );
            LogHelper.Log( $"Target migration is \"{_targetMigration}\"." );

            if ( _latestMigration != _targetMigration )
            {
                LogHelper.Log( $"Latest migration is \"{_latestMigration}\"." );
            }

            // If this is a URL, download it.
            if ( archivePath.ToUpper().StartsWith( "HTTP" ) )
            {
                archivePath = DownloadUrlToFile( archivePath );
            }

            // If the database already exists, remove it.
            if ( DatabaseExists( connectionString ) )
            {
                DeleteDatabase( connectionString );
            }

            // Extract database files from archive.
            LogHelper.Log( $"Reading archive \"{archivePath}\"." );

            var dataFile = Path.Combine( GetDataPath(), $"{targetDbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{targetDbName}_Log.ldf" );

            using ( var archive = new ZipArchive( File.Open( archivePath, FileMode.Open, FileAccess.Read ) ) )
            {
                var mdf = archive.Entries.Where( e => e.Name.EndsWith( ".mdf" ) ).First();
                var ldf = archive.Entries.Where( e => e.Name.EndsWith( ".ldf" ) ).First();

                // Extract the MDF file from the archive.
                using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{targetDbName}_Data.mdf" ) ) )
                {
                    using ( var reader = mdf.Open() )
                    {
                        reader.CopyTo( writer );
                    }
                }

                // Extract the LDF file from the archive.
                using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{targetDbName}_Log.ldf" ) ) )
                {
                    using ( var reader = ldf.Open() )
                    {
                        reader.CopyTo( writer );
                    }
                }
            }

            using ( var connection = new SqlConnection( csbMaster.ConnectionString ) )
            {
                connection.Open();

                // Execute the SQL command to create the database from existing files.
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
CREATE DATABASE [{targetDbName}]
    ON (FILENAME = '{dataFile}'),
    (FILENAME = '{logFile}')
    FOR ATTACH;
ALTER DATABASE [{targetDbName}] SET RECOVERY SIMPLE;
";

                    try
                    {
                        var result = cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        throw;
                    }
                }

                connection.Close();
            }

            // Execute a test query on another connection to ensure the target database is ready.
            using ( var connection = new SqlConnection( csbTarget.ConnectionString ) )
            {
                connection.Open();

                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM [__MigrationHistory]";

                    try
                    {
                        var result = cmd.ExecuteScalar();
                    }
                    catch ( Exception ex )
                    {
                        throw new Exception( "Could not access the target database.", ex );
                    }
                }

                connection.Close();
            }

            LogHelper.Log( $"Clearing Rock cache..." );
            RockCache.ClearAllCachedItems();

            LogHelper.Log( $"Database restored." );
        }

        /// <summary>
        /// Deletes the database from SQL server as well as from disk.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="name">The name of the database.</param>
        private void DeleteDatabase( string connectionString )
        {
            if ( !DatabaseExists( connectionString ) )
            {
                return;
            }

            var csbTarget = new SqlConnectionStringBuilder( connectionString );
            var name = csbTarget.InitialCatalog;
            var databaseDescription = $"{csbTarget.DataSource}\\{csbTarget.InitialCatalog}";

            var csbMaster = new SqlConnectionStringBuilder( connectionString );
            csbMaster.InitialCatalog = "master";

            if ( !this.IsDatabaseResetPermitted )
            {
                throw new Exception( $"Delete database failed. Command is not enabled for current test environment. [Database={databaseDescription}]" );
            }

            // Verify that the target database is a test database.
            if ( string.IsNullOrWhiteSpace( DatabaseCreatorKey ) )
            {
                throw new Exception( $"Delete database failed. The DatabaseCreatorKey configuration setting must have a value." );
            }

            var sql = $@"
IF (EXISTS (SELECT *
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = '_TestDatabaseSettings') )
BEGIN
    SELECT [Value]
    FROM [_TestDatabaseSettings]
    WHERE [Key] = 'DatabaseCreatorKey';
END
";

            var creatorId = DbService.ExecuteScalar( csbTarget.ConnectionString, sql ).ToStringSafe();

            if ( string.IsNullOrEmpty( creatorId ) )
            {
                creatorId = "(not found)";
            }
            if ( this.IsCreatorKeyVerificationRequiredForDatabaseReset
                && creatorId != DatabaseCreatorKey )
            {
                throw new Exception( $"Delete database failed. Database Creator key mismatch. [Database={databaseDescription}, ExpectedCreatorKey={DatabaseCreatorKey}, ActualCreatorKey={creatorId}]" );
            }

            var sqlDrop = $@"
ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{name}];";

            DbService.ExecuteCommand( csbMaster.ConnectionString, sqlDrop );
        }

        /// <summary>
        /// Downloads the URL and returns the path on disk to the downloaded file.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>A path to the file on disk.</returns>
        /// <remarks>
        /// This method can be used to retrieve a database archive from a URL.
        /// </remarks>
        private string DownloadUrlToFile( string url )
        {
            string filename = Path.GetFileName( url );
            string downloadPath = Path.Combine( GetTempPath(), filename );
            string downloadETagPath = downloadPath + ".etag";

            CleanupLocalDatabaseArchiveCache();

            try
            {
                var request = WebRequest.Create( ConfigurationManager.AppSettings["RockUnitTestSource"] );

                // If we have a cached version, tell the server to only give us the file
                // if it has changed.
                if ( File.Exists( downloadPath ) && File.Exists( downloadETagPath ) )
                {
                    request.Headers.Add( "If-None-Match", File.ReadAllText( downloadETagPath ) );
                }

                var response = request.GetResponse();

                // Save the archive and ETag information.
                using ( var srcStream = response.GetResponseStream() )
                {
                    using ( var destStream = File.Create( downloadPath ) )
                    {
                        srcStream.CopyTo( destStream );
                    }
                }

                File.WriteAllText( downloadETagPath, response.Headers["ETag"] );
            }
            catch ( WebException ex )
            {
                // Only throw an exception if it isn't a 302 Not Modified response.
                if ( ex.Response == null || ( ( HttpWebResponse ) ex.Response ).StatusCode != HttpStatusCode.NotModified )
                {
                    throw ex;
                }
            }

            return downloadPath;
        }

        /// <summary>
        /// Removes expired files from the database archive cache.
        /// </summary>
        private void CleanupLocalDatabaseArchiveCache()
        {
            // Delete any archive files older than the specified retention days.
            var archivePath = GetTempPath();

            var archiveFiles = Directory.GetFiles( archivePath, "*.zip" );

            LogHelper.Log( $"Scanning database image cache... [Folder={archivePath}, Files={archiveFiles.Count()}, RetentionDays={this.ArchiveRetentionDays}]" );

            if ( this.ArchiveRetentionDays <= 0 )
            {
                return;
            }

            var expiryBeforeDate = RockDateTime.Now.AddDays( -1 * this.ArchiveRetentionDays );

            foreach ( string file in archiveFiles )
            {
                var fi = new FileInfo( file );
                if ( fi.CreationTime < expiryBeforeDate )
                {
                    DeleteArchiveFile( fi );
                }
            }
        }

        private void DeleteArchiveFile( FileInfo fi )
        {
            if ( fi.Exists
                 && fi.Extension == ".zip"
                 && fi.Directory.FullName == GetTempPath() )
            {
                LogHelper.Log( $"Removing archive file \"{fi.Name} \"..." );

                fi.Delete();
            }
        }

        /// <summary>
        /// Get the full path of the database archive file.
        /// </summary>
        /// <param name="migrationName">The name of the current database migration. If not specified, the most recent migration is used.</param>
        /// <param name="datasetName">A name to identify the set of data stored in the database image.</param>
        /// <returns></returns>
        private string GetArchiveFileName()
        {
            var migrationName = GetTargetMigration();

            var datasetName = this.DatabaseInitializer?.DatasetIdentifier;
            if ( string.IsNullOrWhiteSpace( datasetName ) )
            {
                datasetName = "basic";
            }

            var archivePath = Path.Combine( GetTempPath(), $"Snapshot-{migrationName}-{datasetName}.zip" );

            return archivePath;
        }

        /// <summary>
        /// This method checks to see if we have an archive for the most recent
        /// target migration already and if not it builds a new archive.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>A string containing the path to the archive.</returns>
        private string GetOrGenerateLocalDatabaseArchive( string connectionString )
        {
            var archivePath = GetArchiveFileName();

            if ( File.Exists( archivePath ) )
            {
                return archivePath;
            }

            CleanupLocalDatabaseArchiveCache();

            // Create a new database and archive it.
            if ( string.IsNullOrWhiteSpace( connectionString ) )
            {
                throw new Exception( "A database connection string must be provided." );
            }

            var csbTarget = new SqlConnectionStringBuilder( connectionString );

            var csbMaster = new SqlConnectionStringBuilder( connectionString );
            csbMaster.InitialCatalog = "master";

            var dbName = csbTarget.InitialCatalog;
            var dataFile = Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" );

            // If the database exists then delete it, otherwise just make
            // sure that the MDF and LDF files have been deleted.
            if ( DatabaseExists( csbTarget.ConnectionString ) )
            {
                DeleteDatabase( csbTarget.ConnectionString );
            }
            else
            {
                File.Delete( dataFile );
                File.Delete( logFile );
            }

            LogHelper.Log( $"Creating new test database \"{dbName}\"..." );

            var sqlCreate = $@"
CREATE DATABASE [{dbName}]
    ON (NAME = '{dbName}', FILENAME = '{dataFile}')
    LOG ON (NAME = '{dbName}_Log', FILENAME = '{logFile}');
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE;
";

            DbService.ExecuteScalar( csbMaster.ConnectionString, sqlCreate );

            var sqlCreateSettings = $@"
CREATE TABLE [_TestDatabaseSettings]
    ( [Key] varchar(100) PRIMARY KEY, [Value] varchar(100) );
INSERT INTO [_TestDatabaseSettings]
    ( [Key], [Value] )
VALUES
    ( 'DatabaseCreatorKey','{DatabaseCreatorKey}' );
";

            // Execute a test query to ensure that the database is ready.
            _ = DbService.ExecuteCommand( csbTarget.ConnectionString, sqlCreateSettings );

            // Apply the Rock database migrations.
            var sqlGetDbId = $@"
SELECT DB_ID('{dbName}') AS [DatabaseId]
";
            var dbId = DbService.ExecuteScalar( csbMaster.ConnectionString, sqlGetDbId ).ToStringSafe();

            if ( string.IsNullOrWhiteSpace( dbId ) )
            {
                throw new Exception( "The test database could not be created." );
            }

            MigrateDatabase( csbTarget.ConnectionString );

            // Initialize the new database with data specific to the requested snapshot.
            if ( this.DatabaseInitializer != null )
            {
                LogHelper.Log( $"Initializing Database... [Initializer={this.DatabaseInitializer.GetType()}]" );

                DatabaseInitializer.Initialize();

                LogHelper.Log( $"Initializing Database: completed." );
            }

            // Make sure all Entity Types are registered.
            // This is necessary because some components are only registered at runtime,
            // including the Rock.Bus.Transport.InMemory Type that is required to start the Rock Message Bus.
            EntityTypeService.RegisterEntityTypes();

            // Shrink the database and log files.
            LogHelper.Log( $"Creating test database archive..." );

            var sqlShrink = $@"USE [{dbName}];
DBCC SHRINKFILE ([{dbName}], 1);
DBCC SHRINKFILE ([{dbName}_Log], 1);
USE [master];";

            DbService.ExecuteCommand( csbMaster.ConnectionString, sqlShrink );

            // Detach the database but leave the files intact.
            var sqlDetach = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
EXEC sp_detach_db '{dbName}', 'true';";

            DbService.ExecuteCommand( csbMaster.ConnectionString, sqlDetach );

            // Zip up the data and log files.
            using ( var archiveWriter = File.Create( archivePath ) )
            {
                using ( var zipArchive = new ZipArchive( archiveWriter, ZipArchiveMode.Create, false ) )
                {
                    // Add the MDF data file to the archive.
                    var mdfEntry = zipArchive.CreateEntry( $"{dbName}.mdf" );
                    using ( var writer = mdfEntry.Open() )
                    {
                        using ( var reader = File.OpenRead( dataFile ) )
                        {
                            reader.CopyTo( writer );
                        }
                    }

                    // Add the LDF log file to the archive.
                    var ldfEntry = zipArchive.CreateEntry( $"{dbName}_Log.ldf" );
                    using ( var writer = ldfEntry.Open() )
                    {
                        using ( var reader = File.OpenRead( logFile ) )
                        {
                            reader.CopyTo( writer );
                        }
                    }
                }
            }

            File.Delete( dataFile );
            File.Delete( logFile );

            LogHelper.Log( $"Test database archive created. [{archivePath}]" );

            return archivePath;
        }
    }
}