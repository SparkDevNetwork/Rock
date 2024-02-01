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
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.Database;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    public enum DatabaseRefreshStrategySpecifier
    {
        // Never update or replace an existing test database.
        Never = 0,
        // Restore the database from an existing snapshot if it exists, or create a new database.
        // Do not replace an existing database unless the creator is verified as the Rock test framework.
        Verified = 1,
        // Force the database to be replaced if it exists.
        Force = 2
    }

    /// <summary>
    /// A helper class to manage database instances for the testing process.
    /// </summary>
    public class TestDatabaseHelper
    {
        public const string SampleDataSourceKey = "com.rockrms.test.SampleDataSource";

        private static bool _IsDatabaseInitialized = false;
        private static DatabaseRefreshStrategySpecifier _databaseDeleteStrategy = DatabaseRefreshStrategySpecifier.Verified;

        public static string DatabaseCreatorKey = "RockIntegrationTestProject";
        public static bool DatabaseMigrateIsAllowed = false;
        public static string ConnectionString = null;
        public static string SampleDataUrl = null;
        public static bool DatabaseRemoteDeleteIsAllowed = false;
        public static ITestDatabaseInitializer DatabaseInitializer = null;
        public static string DefaultSnapshotName = null;
        public static bool SampleDataIsEnabled = true;

        #region Public Properties and Methods

        public static DatabaseRefreshStrategySpecifier DatabaseRefreshStrategy
        {
            get
            {
                return _databaseDeleteStrategy;
            }
            set
            {
                _databaseDeleteStrategy = value;
            }
        }

        /// <summary>
        /// Initializes a new database and loads sample data.
        /// </summary>
        public static bool InitializeTestDatabase( string snapshotName = null )
        {
            snapshotName = snapshotName ?? DefaultSnapshotName;

            if ( _IsDatabaseInitialized )
            {
                return true;
            }
             
            _IsDatabaseInitialized = true;

            bool success;
            var forceReplaceExisting = ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Force );

            if ( IsLocalDbInstance() )
            {
                success = InitializeSqlServerDatabaseForLocal( forceReplaceExisting, snapshotName );
            }
            else
            {
                success = InitializeSqlServerDatabaseForRemote( ConnectionString,
                    snapshotName,
                    SampleDataUrl,
                    forceReplaceExisting );
            }

            return success;
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <param name="connectionString">The connection.</param>
        /// <returns><c>true</c> if the named database exists; otherwise <c>false</c>.</returns>
        public static bool DatabaseExists( string connectionString )
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
        public static void DeleteDatabase()
        {
            DeleteDatabase( ConnectionString );
        }

        /// <summary>
        /// Resets the database by using a previously stored snapshot.
        /// </summary>
        public static void CreateSnapshot( string snapshotName, bool overwriteExisting = false )
        {
            if ( !IsLocalDbInstance() )
            {
                throw new NotImplementedException( "Reset is not available for a remote database." );
            }

            CreateLocalDatabaseArchive( ConnectionString, snapshotName, overwriteExisting );
        }

        /// <summary>
        /// Resets the database by using a previously stored snapshot.
        /// </summary>
        public static void RestoreSnapshot( string snapshotName = "default" )
        {
            if ( !IsLocalDbInstance() )
            {
                throw new NotImplementedException( "Reset is not available for a remote database." );
            }

            if ( DatabaseExists( ConnectionString ) )
            {
                // Remove the database from the local server.
                DeleteDatabase( ConnectionString );
            }

            string archivePath;
            if ( snapshotName == "default" )
            {
                // If the default snapshot does not exist, create it.
                archivePath = GetArchiveFileNameForCurrentMigration();
            }
            else
            {
                archivePath = GetArchiveFileName( snapshotName );
            }

            RestoreLocalDatabaseFromArchive( ConnectionString, archivePath );
        }

        /// <summary>
        /// Resets the database by using the default data source.
        /// </summary>
        public static void ResetDatabase()
        {
            if ( IsLocalDbInstance() )
            {
                if ( !DatabaseExists( ConnectionString ) )
                {
                    throw new Exception( "ResetDatabase failed. No database image is available to perform the reset. Use the InitializeTestDatabase() method to create a new local database image." );
                }

                // Reset the database from the archive image.
                InitializeSqlServerDatabaseForLocal( recreateArchive: false );
            }
            else
            {
                if ( string.IsNullOrEmpty( ConnectionString ) )
                {
                    throw new NotImplementedException( "ResetDatabase failed. The ConnectionString property is not set." );
                }
                else
                {
                    throw new NotImplementedException( "ResetDatabase failed. Reset is not available for a remote database." );
                }
            }
        }

        #endregion

        private static bool IsLocalDbInstance()
        {
            var csb = new SqlConnectionStringBuilder( ConnectionString );

            if ( csb.DataSource != null
                && csb.DataSource.StartsWith( @"(LocalDB)\", StringComparison.OrdinalIgnoreCase ) )
            {
                return true;
            }
            return false;
        }

        private static bool InitializeSqlServerDatabaseForRemote( string connectionString, string snapshotName, string sampleDataUrl, bool forceReplace = false )
        {
            var csb = new SqlConnectionStringBuilder( connectionString );
            var dbName = csb.InitialCatalog;

            LogHelper.Log( $"Connecting to database server \"{ csb.DataSource }\"..." );
            LogHelper.Log( $"Target database is \"{dbName}\"." );

            var databaseExists = DatabaseExists( connectionString );

            var lastMigration = string.Empty;
            var targetMigration = string.Empty;

            csb.InitialCatalog = "master";
            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                var migrateDatabase = false;
                var createDatabase = false;
                var loadSampleData = false;
                var autoMigrationEnabled = false;

                if ( databaseExists )
                {
                    LogHelper.Log( "Verifying migrations..." );

                    var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
                    lastMigration = migrator.GetDatabaseMigrations().FirstOrDefault();
                    targetMigration = GetTargetMigration();

                    autoMigrationEnabled = migrator.Configuration.AutomaticMigrationsEnabled;

                    migrateDatabase = lastMigration != targetMigration;
                    loadSampleData = lastMigration == null;

                    if ( forceReplace )
                    {
                        // Backup the current database.
                        using ( var cmd = connection.CreateCommand() )
                        {
                            LogHelper.Log( $"Backing up existing database..." );

                            cmd.CommandText = $@"
BACKUP DATABASE [{dbName}]
    TO DISK = '{dbName}.bak')";
                            cmd.ExecuteNonQuery();
                        }

                        LogHelper.Log( $"Deleting existing database..." );

                        DeleteDatabase( connectionString );

                        createDatabase = true;
                        loadSampleData = true;
                    }
                }
                else
                {
                    createDatabase = true;
                    loadSampleData = true;
                }

                //
                // Execute the SQL command to create the empty database.
                //
                if ( createDatabase )
                {
                    LogHelper.Log( $"Creating new database..." );

                    using ( var cmd = connection.CreateCommand() )
                    {
                        cmd.CommandText = $@"
CREATE DATABASE [{dbName}];
    --ON (NAME = '{dbName}')
    --LOG ON (NAME = '{dbName}_Log');
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE";
                        using ( var txn = cmd.Connection.BeginTransaction() )
                        {
                            try
                            {
                                var result = cmd.ExecuteNonQuery();
                                txn.Commit();
                            }
                            catch
                            {
                                txn.Rollback();
                                throw;
                            }
                        }
                    }
                }

                if ( createDatabase || migrateDatabase )
                {
					if ( !createDatabase )
                    {
                        TestHelper.Log( $"Target database migration level does not match the current Rock version. [TargetMigration={targetMigration}, LatestMigration={lastMigration}]" );
                    }

                    if ( autoMigrationEnabled )
                    {
                        LogHelper.Log( $"Running migrations..." );

                        MigrateDatabase( connection.ConnectionString );
                    }
                    else
                    {
                        LogHelper.Log( $"Automatic migrations are disabled." );
                    }
                }

                if ( loadSampleData )
                {
                    DatabaseInitializer?.InitializeSnapshot( snapshotName, sampleDataUrl );
                }

                LogHelper.Log( $"Task complete." );
            }

            return true;
        }

        private static bool ValidateSampleDataForActiveDatabase( string sampleDataUrl )
        {
            LogHelper.Log( $"Verifying sample data..." );

            var sampleDataId = SystemSettings.GetValue( SampleDataSourceKey );
            if ( string.IsNullOrEmpty( sampleDataId ) || sampleDataId != sampleDataUrl )
            {
                return false;
            }

            LogHelper.Log( $"Sample data verified.  [SampleDataSource={sampleDataId}]" );
            return true;
        }

        /// <summary>
        /// Gets the target migration.
        /// </summary>
        private static string GetTargetMigration()
        {
            if ( _targetMigration == null )
            {
                _targetMigration = typeof( Rock.Migrations.RockMigration )
                    .Assembly
                    .GetExportedTypes()
                       .Where( a => typeof( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ).IsAssignableFrom( a ) )
                    .Select( a => ( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ) Activator.CreateInstance( a ) )
                    .Select( a => a.Id )
                    .OrderByDescending( a => a )
                    .First();
            }

            return _targetMigration;
        }

        private static string _targetMigration;

        /// <summary>
        /// Migrates the database.
        /// </summary>
        private static void MigrateDatabase( string connectionString )
        {
            var connection = new DbConnectionInfo( connectionString, "System.Data.SqlClient" );

            var config = new Rock.Migrations.Configuration();
            config.TargetDatabase = connection;

            var migrator = new System.Data.Entity.Migrations.DbMigrator( config );

            LogHelper.Log( $"Applying database migrations... [Target=\"{ _targetMigration }\"]" );
            try
            {
                migrator.Update( _targetMigration );
            }
            catch ( Exception ex )
            {
                throw new Exception( "Test Database migration failed. Verify that the database connection string specified in the test project is valid. You may need to manually synchronize the database or configure the test environment to force-create a new database.", ex );
            }
        }

        #region Local Database

        /// <summary>
        /// Gets the data path where we will store files.
        /// </summary>
        /// <returns>A string that contains the full path to our temporary folder.</returns>
        private static string GetDataPath()
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
        private static string GetTempPath()
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
        private static bool InitializeSqlServerDatabaseForLocal( bool recreateArchive, string snapshotName = null )
        {
            var restoreDatabase = ( TestDatabaseHelper.DatabaseRefreshStrategy != DatabaseRefreshStrategySpecifier.Never );

            if ( DatabaseExists( ConnectionString ) )
            {
                // If the database exists and should never be replaced, continue without verifying the migration level.
                if ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Never )
                {
                    return true;
                }

                if ( recreateArchive )
                {
                    // Remove the database from the local server, and delete the associated archive file.
                    DeleteDatabase( ConnectionString );
                }
            }
            else
            {
                // If the database has been manually deleted, make sure that the archive is recreated.
                recreateArchive = true;
            }

            if ( recreateArchive )
            {
                var fileName = GetArchiveFileNameForCurrentMigration();
                DeleteArchiveFile( new FileInfo( fileName ) );

                restoreDatabase = true;
            }

            var sampleDataUrl = SampleDataIsEnabled ? SampleDataUrl : string.Empty;

            if ( restoreDatabase )
            {
                // Restore the database from the archive.
                var testSource = GetOrGenerateLocalDatabaseArchive( ConnectionString, snapshotName, sampleDataUrl );

                RestoreLocalDatabaseFromArchive( ConnectionString, testSource );
            }

            RockCache.ClearAllCachedItems();

            if ( restoreDatabase )
            {
                if ( SampleDataIsEnabled )
                {
                    // Verify that the test database is valid.
                    var valid = ValidateSampleDataForActiveDatabase( sampleDataUrl );

                    if ( !valid )
                    {
                        LogHelper.Log( $"Invalid Sample Data. Sample data key does not match the sample data set specified for this test run. To refresh the test data set, delete the existing database. [Expected=\"{SampleDataUrl}\"]" );
                        throw new Exception( $"Invalid Sample Data. Required sample data key not found in target database." );
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Resets the database by using the specified path to a database archive file.
        /// </summary>
        /// <param name="archivePath">The archive path that contains the MDF and LDF files.</param>
        private static void RestoreLocalDatabaseFromArchive( string connectionString, string archivePath )
        {
            var csbTarget = new SqlConnectionStringBuilder( connectionString );
            var targetDbName = csbTarget.InitialCatalog;

            var csbMaster = new SqlConnectionStringBuilder( connectionString );
            csbMaster.InitialCatalog = "master";

            LogHelper.Log( $"Restoring local database from archive..." );
            LogHelper.Log( $"Target database is \"{targetDbName}\"." );

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

            using ( var archive = new ZipArchive( File.Open( archivePath, FileMode.Open ) ) )
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
ALTER DATABASE [{targetDbName}] SET RECOVERY SIMPLE;";

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
        private static void DeleteDatabase( string connectionString )
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

            if ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Never )
            {
                throw new Exception( $"Delete database failed. Command is not enabled for current test environment. [Database={ databaseDescription }]" );
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
            if ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Verified
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
        private static string DownloadUrlToFile( string url )
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
        /// Cleanups the archive cache.
        /// </summary>
        private static void CleanupLocalDatabaseArchiveCache( int retentionDays = 90 )
        {
            // Delete any archive files older than the specified retention days.
            var files = Directory.GetFiles( GetTempPath() );
            foreach ( string file in files )
            {
                FileInfo fi = new FileInfo( file );
                if ( fi.CreationTime < RockDateTime.Now.AddDays( -1 * retentionDays ) )
                {
                    DeleteArchiveFile( fi );
                }
            }
        }

        private static void DeleteArchiveFile( FileInfo fi )
        {
            if ( fi.Exists
                 && fi.Extension == ".zip"
                 && fi.Directory.FullName == GetTempPath() )
            {
                fi.Delete();
            }
        }

        private static string GetArchiveFileNameForCurrentMigration()
        {
            return GetArchiveFileName( GetTargetMigration() );
        }

        private static string GetArchiveFileName( string archiveName )
        {
            var archivePath = Path.Combine( GetTempPath(), $"Snapshot-{archiveName}.zip" );

            return archivePath;
        }

        /// <summary>
        /// This method checks to see if we have an archive for the most recent
        /// target migration already and if not it builds a new archive.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sampleDataUrl">
        /// The URL for the document containing the sample data. If not specified, sample data will not be added to the new database.
        /// </param>
        /// <returns>A string containing the path to the archive.</returns>
        private static string GetOrGenerateLocalDatabaseArchive( string connectionString, string snapshotName, string sampleDataUrl )
        {
            var archivePath = GetArchiveFileNameForCurrentMigration();

            if ( File.Exists( archivePath ) )
            {
                return archivePath;
            }

            CleanupLocalDatabaseArchiveCache();

            //
            // Create a new database and archive it.
            //
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
            DatabaseInitializer?.InitializeSnapshot( snapshotName, sampleDataUrl );

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

        /// <summary>
        /// This method archives the local database referenced by the connection string.
        /// </summary>
        /// <returns>A string containing the path to the archive.</returns>
        private static string CreateLocalDatabaseArchive( string connectionString, string snapshotName, bool overwrite = false ) //, string sampleDataUrl )
        {
            var archivePath = GetArchiveFileName( snapshotName );
            if ( File.Exists( archivePath ) )
            {
                if ( !overwrite )
                {
                    throw new Exception( $"Database archive \"{ snapshotName }\" already exists." );
                }

                File.Delete( archivePath );
            }

            //
            // Create a new database and archive it.
            //
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

            LogHelper.Log( $"Database archive created. [{archivePath}]" );

            return archivePath;
        }

        #endregion
    }
}