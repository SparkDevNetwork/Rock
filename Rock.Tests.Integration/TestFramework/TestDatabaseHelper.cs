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
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Rock.Model;
using Rock.Tests.Integration.Core.Jobs;
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
        private static bool _IsDatabaseInitialized = false;

        public static string DatabaseCreatorId = "RockIntegrationTestProject";
        public static bool DatabaseMigrateIsAllowed = false;

        public static string ConnectionString = null;
        public static string SampleDataUrl = null;

        private const string DatabaseCreatorKey = "com.rockrms.test.DatabaseCreator";
        private const string SampleDataSourceKey = "com.rockrms.test.SampleDataSource";

        public static bool DatabaseRemoteDeleteIsAllowed = false;

        private static DatabaseRefreshStrategySpecifier _databaseDeleteStrategy = DatabaseRefreshStrategySpecifier.Verified;
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


        /// <summary>
        /// Initializes a new database and loads sample data.
        /// </summary>
        public static bool InitializeTestDatabase()
        {
            if ( _IsDatabaseInitialized )
            {
                return true;
            }

            _IsDatabaseInitialized = true;

            bool success;
            var forceReplaceExisting = (DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Force);

            if ( IsLocalDbInstance() )
            {
                success = InitializeSqlServerDatabaseForLocal( forceReplaceExisting );
            }
            else
            {
                success = InitializeSqlServerDatabaseForRemote( ConnectionString, SampleDataUrl, forceReplaceExisting );
            }

            return success;
        }

        private static bool InitializeSqlServerDatabaseForRemote( string connectionString, string sampleDataUrl, bool forceReplace = false )
        {
            // We need to connect to the master database, but track the target database
            // for use later.
            var csb = new SqlConnectionStringBuilder( connectionString );
            var dbName = csb.InitialCatalog;

            TestHelper.Log( $"Connecting to database server \"{ csb.DataSource }\"..." );
            TestHelper.Log( $"Target database is \"{dbName}\"." );

            var databaseExists = DatabaseExists( connectionString );

            var lastMigration = string.Empty;
            var targetMigration = string.Empty;

            csb.InitialCatalog = "master";
            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                var migrateDatabase = false;
                var createDatabase = false;
                var autoMigrationEnabled = false;

                if ( databaseExists )
                {
                    TestHelper.Log( "Verifying migrations..." );

                    var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
                    lastMigration = migrator.GetDatabaseMigrations().FirstOrDefault();
                    targetMigration = GetTargetMigration();

                    autoMigrationEnabled = migrator.Configuration.AutomaticMigrationsEnabled;

                    migrateDatabase = lastMigration != targetMigration;

                    if ( forceReplace )
                    {
                        // Backup the current database.
                        using ( var cmd = connection.CreateCommand() )
                        {
                            TestHelper.Log( $"Backing up existing database..." );

                            cmd.CommandText = $@"
BACKUP DATABASE [{dbName}]
    TO DISK = '{dbName}.bak')";
                            cmd.ExecuteNonQuery();
                        }

                        TestHelper.Log( $"Deleting existing database..." );

                        DeleteDatabase( connectionString );

                        createDatabase = true;
                    }
                }
                else
                {
                    createDatabase = true;
                }

                //
                // Execute the SQL command to create the empty database.
                //
                if ( createDatabase )
                {
                    TestHelper.Log( $"Creating new database..." );

                    using ( var cmd = connection.CreateCommand() )
                    {
                        cmd.CommandText = $@"
CREATE DATABASE [{dbName}];
    --ON (NAME = '{dbName}')
    --LOG ON (NAME = '{dbName}_Log');
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE";
                        cmd.ExecuteNonQuery();
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
                        TestHelper.Log( $"Running migrations..." );

                        MigrateDatabase();
                    }
                    else
                    {
                        TestHelper.Log( $"Automatic migrations are disabled." );
                    }
                }

                if ( createDatabase )
                {
                    AddSampleDataForActiveDatabase( sampleDataUrl );
                }

                TestHelper.Log( $"Task complete." );
            }

            return true;
        }

        private static bool ValidateSampleDataForActiveDatabase( string sampleDataUrl )
        {
            TestHelper.Log( $"Verifying sample data..." );

            var sampleDataId = SystemSettings.GetValue( SampleDataSourceKey );
            if ( string.IsNullOrEmpty( sampleDataId ) || sampleDataId != sampleDataUrl )
            {
                return false;
            }

            TestHelper.Log( $"Sample data verified.  [SampleDataSource={sampleDataId}]" );
            return true;
        }

        private static bool AddSampleDataForActiveDatabase( string sampleDataUrl )
        {
            // Set some global flags to mark this as a test database.
            SystemSettings.SetValue( DatabaseCreatorKey, DatabaseCreatorId );

            TestHelper.Log( $"Loading sample data..." );

            // Make sure all Entity Types are registered.
            // This is necessary because some components are only registered at runtime,
            // including the Rock.Bus.Transport.InMemory Type that is required to start the Rock Message Bus.
            EntityTypeService.RegisterEntityTypes();

            var factory = new SampleDataManager();
            var args = new SampleDataManager.SampleDataImportActionArgs();

            factory.CreateFromXmlDocumentFile( sampleDataUrl, args );

            // Run the Rock Cleanup job to ensure calculated fields are updated.
            TestHelper.Log( $"Running RockCleanup Job..." );

            var jobContext = new TestJobContext();
            var job = new Rock.Jobs.RockCleanup();

            job.ExecuteInternal( jobContext );

            // Set the sample data identifiers.
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );
            SystemSettings.SetValue( SampleDataSourceKey, sampleDataUrl );

            TestHelper.Log( $"Sample Data loaded." );

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
        /// Migrates the database.
        /// </summary>
        private static void MigrateDatabase()
        {
            var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );

            if ( !migrator.Configuration.AutomaticMigrationsEnabled )
            {
                return;
            }
            try
            {
                migrator.Update();
            }
            catch (Exception ex)
            {
                throw new Exception( "Test Database migration failed. You may need to manually synchronize the database or configure the test environment to force-create a new database.", ex );
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
        private static bool InitializeSqlServerDatabaseForLocal( bool replaceExistingDatabase )
        {
            if ( replaceExistingDatabase )
            {
                // Remove the database from the local server, and delete the associated archive file.
                DeleteDatabase( ConnectionString );

                var fileName = GetCurrentArchiveFileName();
                DeleteArchiveFile( new FileInfo( fileName ) );
            }

            // Create a new database and archive it.
            var testSource = GetOrGenerateLocalDatabaseArchive( ConnectionString, SampleDataUrl );

            RestoreLocalDatabaseFromArchive( ConnectionString, testSource );

            RockCache.ClearAllCachedItems();

            // Verify that the test database is valid.
            var valid = ValidateSampleDataForActiveDatabase( SampleDataUrl );

            if ( !valid )
            {
                TestHelper.Log( $"Invalid Sample Data. Sample data key does not match the sample data set specified for this test run. To refresh the test data set, delete the existing database. [Expected=\"{SampleDataUrl}\"]" );
                throw new Exception( $"Invalid Sample Data. Required sample data key not found in target database." );
            }

            return true;
        }

        /// <summary>
        /// Resets the database by using the default data source.
        /// </summary>
        public static void ResetDatabase()
        {
            if ( IsLocalDbInstance() )
            {
                var forceReplaceExisting = ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Force );
                InitializeSqlServerDatabaseForLocal( forceReplaceExisting );
            }
            else
            {
                throw new NotImplementedException( "Reset is not available for a remote database." );
            }
        }

        /// <summary>
        /// Resets the database by using the specified path to a database archive file.
        /// </summary>
        /// <param name="archivePath">The archive path that contains the MDF and LDF files.</param>
        private static void RestoreLocalDatabaseFromArchive( string connectionString, string archivePath )
        {
            var csb = new SqlConnectionStringBuilder( connectionString );
            var dbName = csb.InitialCatalog;

            TestHelper.Log( $"Restoring local database from archive..." );
            TestHelper.Log( $"Target database is \"{dbName}\"." );
            TestHelper.Log( $"Archive source is \"{archivePath}\"." );

            csb.InitialCatalog = "master";

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
            TestHelper.Log( $"Reading archive \"{archivePath}\"." );

            var dataFile = Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" );

            using ( var archive = new ZipArchive( File.Open( archivePath, FileMode.Open ) ) )
            {
                var mdf = archive.Entries.Where( e => e.Name.EndsWith( ".mdf" ) ).First();
                var ldf = archive.Entries.Where( e => e.Name.EndsWith( ".ldf" ) ).First();

                // Extract the MDF file from the archive.
                using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" ) ) )
                {
                    using ( var reader = mdf.Open() )
                    {
                        reader.CopyTo( writer );
                    }
                }

                // Extract the LDF file from the archive.
                using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" ) ) )
                {
                    using ( var reader = ldf.Open() )
                    {
                        reader.CopyTo( writer );
                    }
                }
            }

            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                // Execute the SQL command to create the database from existing files.
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
CREATE DATABASE [{dbName}]   
    ON (FILENAME = '{dataFile}'),  
    (FILENAME = '{logFile}')  
    FOR ATTACH;";
                    cmd.ExecuteNonQuery();
                }
            }

            RockCache.ClearAllCachedItems();

            TestHelper.Log( $"Database restored." );
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

            var csb = new SqlConnectionStringBuilder( connectionString );
            var name = csb.InitialCatalog;
            var databaseDescription = $"{csb.DataSource}\\{csb.InitialCatalog}";
            csb.InitialCatalog = "master";

            if ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Never )
            {
                throw new Exception( $"Delete database failed. Command is not enabled for current test environment. [Database={ databaseDescription }]" );
            }

            // Verify that the target database is a test database.
            var creatorId = SystemSettings.GetValue( DatabaseCreatorKey );
            if ( string.IsNullOrEmpty( creatorId ) )
            {
                creatorId = "(not found)";
            }
            if ( DatabaseRefreshStrategy == DatabaseRefreshStrategySpecifier.Verified
                && creatorId != DatabaseCreatorId )
            {
                throw new Exception( $"Delete database failed. Database Creator key mismatch. [Database={databaseDescription}, ExpectedCreatorId={DatabaseCreatorId}, ActualCreatorId={creatorId}]" );
            }

            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{name}];";
                    cmd.ExecuteNonQuery();
                }
            }
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

        private static string GetCurrentArchiveFileName()
        {
            var archivePath = Path.Combine( GetTempPath(), $"Snapshot-{GetTargetMigration()}.zip" );

            return archivePath;
        }

        /// <summary>
        /// This method checks to see if we have an archive for the most recent
        /// target migration already and if not it builds a new archive.
        /// </summary>
        /// <returns>A string containing the path to the archive.</returns>
        private static string GetOrGenerateLocalDatabaseArchive( string connectionString, string sampleDataUrl )
        {
            var archivePath = GetCurrentArchiveFileName();

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
            if ( string.IsNullOrWhiteSpace( sampleDataUrl ) )
            {
                throw new Exception( "A sample data file path or URL must be provided." );
            }

            var csb = new SqlConnectionStringBuilder( connectionString );

            // We need to connect to the master database, but track the target database
            // for use later.
            var dbName = csb.InitialCatalog;
            var dataFile = Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" );

            // If the database exists then delete it, otherwise just make
            // sure that the MDF and LDF files have been deleted.
            if ( DatabaseExists( connectionString ) )
            {
                DeleteDatabase( connectionString );
            }
            else
            {
                File.Delete( dataFile );
                File.Delete( logFile );
            }

            csb.InitialCatalog = "master";
            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                // Execute the SQL command to create the empty database on the LocalDB server.
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
CREATE DATABASE [{dbName}]   
    ON (NAME = '{dbName}', FILENAME = '{dataFile}')
    LOG ON (NAME = '{dbName}_Log', FILENAME = '{logFile}');
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE";
                    cmd.ExecuteNonQuery();
                }

                // Apply the Rock database migrations.
                MigrateDatabase();

                // Load the sample data.
                AddSampleDataForActiveDatabase( sampleDataUrl );

                // Shrink the database and log files.
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"USE [{dbName}];
DBCC SHRINKFILE ([{dbName}], 1);
DBCC SHRINKFILE ([{dbName}_Log], 1);
USE [master];";
                    cmd.ExecuteNonQuery();
                }

                // Detach the database but leave the files intact.
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
EXEC sp_detach_db '{dbName}', 'true';";
                    cmd.ExecuteNonQuery();
                }
            }

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

            return archivePath;
        }

        #endregion
    }
}