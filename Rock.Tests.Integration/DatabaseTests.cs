using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    public class DatabaseTests
    {

        /// <summary>
        /// Gets the target migration.
        /// </summary>
        private static string TargetMigration
        {
            get
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
        }
        private static string _targetMigration;

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
        public static void ResetDatabase()
        {
            var testSource = GetOrGenerateArchive();

            ResetDatabase( testSource );
        }

        /// <summary>
        /// Resets the database by using the specified path to a database archive file.
        /// </summary>
        /// <param name="archivePath">The archive path that contains the MDF and LDF files.</param>
        public static void ResetDatabase( string archivePath )
        {

            var cs = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
            var csb = new SqlConnectionStringBuilder( cs );

            //
            // We need to connect to the master database, but track the target database
            // for use later.
            //
            var dbName = csb.InitialCatalog;
            csb.InitialCatalog = "master";

            //
            // If this is a URL, download it.
            //
            if ( archivePath.ToUpper().StartsWith( "HTTP" ) )
            {
                archivePath = DownloadUrl( archivePath );
            }

            using ( var archive = new ZipArchive( File.Open( archivePath, FileMode.Open ) ) )
            {
                using ( var connection = new SqlConnection( csb.ConnectionString ) )
                {
                    connection.Open();

                    //
                    // If the database exists, that means something probably went
                    // horribly wrong on a previous run so we need to manually
                    // delete the database.
                    if ( DoesDatabaseExist( dbName, connection ) )
                    {
                        DeleteDatabase( connection, dbName );
                    }

                    RestoreDatabase( connection, dbName, archive );
                }
            }

            RockCache.ClearAllCachedItems();
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="connection">The connection.</param>
        /// <returns><c>true</c> if the named database exists; otherwise <c>false</c>.</returns>
        private static bool DoesDatabaseExist( string dbName, SqlConnection connection )
        {
            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandText = "SELECT COUNT(*) FROM [sysdatabases] WHERE [name] = @dbName";
                cmd.Parameters.AddWithValue( "dbName", dbName );

                return ( int ) cmd.ExecuteScalar() != 0;
            }
        }

        /// <summary>
        /// Deletes the database from the SQL server so the next test can start clean.
        /// </summary>
        public static void DeleteDatabase()
        {
            var cs = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
            var csb = new SqlConnectionStringBuilder( cs );

            //
            // We need to connect to the master database, but track the target database
            // for use later.
            //
            var dbName = csb.InitialCatalog;
            csb.InitialCatalog = "master";

            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                if ( dbName != "master" )
                {
                    DeleteDatabase( connection, dbName );
                }
            }
        }

        /// <summary>
        /// Restores the database from the files stored in the archive.
        /// </summary>
        /// <param name="connection">The connection to use when running SQL commands.</param>
        /// <param name="dbName">Name of the database to be created.</param>
        /// <param name="archive">The archive that contains the MDF and LDF files.</param>
        private static void RestoreDatabase( DbConnection connection, string dbName, ZipArchive archive )
        {
            var mdf = archive.Entries.Where( e => e.Name.EndsWith( ".mdf" ) ).First();
            var ldf = archive.Entries.Where( e => e.Name.EndsWith( ".ldf" ) ).First();

            //
            // Extract the MDF file from the archive.
            //
            using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" ) ) )
            {
                using ( var reader = mdf.Open() )
                {
                    reader.CopyTo( writer );
                }
            }

            //
            // Extract the LDF file from the archive.
            //
            using ( var writer = File.Create( Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" ) ) )
            {
                using ( var reader = ldf.Open() )
                {
                    reader.CopyTo( writer );
                }
            }

            var dataFile = Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" );

            //
            // Execute the SQL command to create the database from existing files.
            //
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

        /// <summary>
        /// Deletes the database from SQL server as well as from disk.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="name">The name of the database.</param>
        private static void DeleteDatabase( DbConnection connection, string name )
        {
            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandText = $@"
ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{name}];";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Downloads the URL and returns the path on disk to the downloaded file.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>A path to the file on disk.</returns>
        private static string DownloadUrl( string url )
        {
            string filename = Path.GetFileName( url );
            string downloadPath = Path.Combine( GetTempPath(), filename );
            string downloadETagPath = downloadPath + ".etag";

            CleanupArchiveCache();

            try
            {
                var request = WebRequest.Create( ConfigurationManager.AppSettings["RockUnitTestSource"] );

                //
                // If we have a cached version, tell the server to only give us the file
                // if it has changed.
                //
                if ( File.Exists( downloadPath ) && File.Exists( downloadETagPath ) )
                {
                    request.Headers.Add( "If-None-Match", File.ReadAllText( downloadETagPath ) );
                }

                var response = request.GetResponse();

                //
                // Save the archive and ETag information.
                //
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
                //
                // Only throw an exception if it isn't a 302 Not Modified response.
                //
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
        private static void CleanupArchiveCache()
        {
            //
            // Delete any cached files older than 90 days.
            //
            foreach ( string file in Directory.GetFiles( GetTempPath() ) )
            {
                FileInfo fi = new FileInfo( file );

                if ( fi.CreationTime < DateTime.Now.AddDays( -90 ) )
                {
                    fi.Delete();
                }
            }
        }

        /// <summary>
        /// This method checks to see if we have an archive for the most recent
        /// target migration already and if not it builds a new archive.
        /// </summary>
        /// <returns>A string containing the path to the archive.</returns>
        private static string GetOrGenerateArchive()
        {
            string archivePath = Path.Combine( GetTempPath(), $"Snapshot-{TargetMigration}.zip" );

            if ( File.Exists( archivePath ) )
            {
                return archivePath;
            }

            CleanupArchiveCache();

            var cs = ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString;
            var csb = new SqlConnectionStringBuilder( cs );

            //
            // We need to connect to the master database, but track the target database
            // for use later.
            //
            var dbName = csb.InitialCatalog;
            var dataFile = Path.Combine( GetDataPath(), $"{dbName}_Data.mdf" );
            var logFile = Path.Combine( GetDataPath(), $"{dbName}_Log.ldf" );

            csb.InitialCatalog = "master";

            using ( var connection = new SqlConnection( csb.ConnectionString ) )
            {
                connection.Open();

                //
                // If the database exists then delete it, otherwise just make
                // sure that the MDF and LDF files have been deleted.
                //
                if ( DoesDatabaseExist( dbName, connection ) )
                {
                    DeleteDatabase( connection, dbName );
                }
                else
                {
                    File.Delete( dataFile );
                    File.Delete( logFile );
                }

                //
                // Execute the SQL command to create the empty database.
                //
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"
CREATE DATABASE [{dbName}]   
    ON (NAME = '{dbName}', FILENAME = '{dataFile}')
    LOG ON (NAME = '{dbName}_Log', FILENAME = '{logFile}');
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE";
                    cmd.ExecuteNonQuery();
                }

                MigrateDatabase();

                //
                // Shrink the database and log files. This shrinks them from
                // about 133+133MB to about 105+4MB.
                //
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"USE [{dbName}];
DBCC SHRINKFILE ('{dbName}', 1);
DBCC SHRINKFILE ('{dbName}_Log', 1);
USE [master];";
                    cmd.ExecuteNonQuery();
                }

                //
                // Detach the database but leave the files intact.
                //
                using ( var cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = $@"EXEC sp_detach_db '{dbName}', 'true';";
                    cmd.ExecuteNonQuery();
                }
            }

            //
            // Zip up the data and log files.
            //
            using ( var archiveWriter = File.Create( archivePath ) )
            {
                using ( var zipArchive = new ZipArchive( archiveWriter, ZipArchiveMode.Create, false ) )
                {
                    //
                    // Add the MDF data file to the archive.
                    //
                    var mdfEntry = zipArchive.CreateEntry( $"{dbName}.mdf" );
                    using ( var writer = mdfEntry.Open() )
                    {
                        using ( var reader = File.OpenRead( dataFile ) )
                        {
                            reader.CopyTo( writer );
                        }
                    }

                    //
                    // Add the LDF log file to the archive.
                    //
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

        /// <summary>
        /// Migrates the database.
        /// </summary>
        private static void MigrateDatabase()
        {
            var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
            migrator.Update();
        }
    }
}