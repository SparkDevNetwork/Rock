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
        /// Gets the data path where we will store files.
        /// </summary>
        /// <returns>A string that contains the full path to our temporary folder.</returns>
        private static string GetDataPath()
        {
            string path = Path.Combine( Directory.GetCurrentDirectory(), "Data" );

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory( path );
            }

            return path;
        }

        /// <summary>
        /// Resets the database by using the default data source.
        /// </summary>
        public static void ResetDatabase()
        {
            var testSource = ConfigurationManager.AppSettings["RockUnitTestSource"];

            ResetDatabase( testSource );
        }

        /// <summary>
        /// Resets the database by using the specified path to a database archive file.
        /// </summary>
        /// <param name="archivePath">The archive path that contains the MDF and LDF files.</param>
        public static void ResetDatabase( string archivePath )
        {
            try
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
            if ( archivePath.ToUpper().StartsWith("HTTP"))
            {
                archivePath = DownloadUrl( archivePath );
            }

            using ( var archive = new ZipArchive( File.Open( archivePath, FileMode.Open ) ) )
            {
                using ( var connection = new SqlConnection( csb.ConnectionString ) )
                {
                    connection.Open();

                    //
                    // Check if the database already exists as if something went horribly wrong
                    // then it may not have been deleted.
                    //
                    using ( var cmd = connection.CreateCommand() )
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM [sysdatabases] WHERE [name] = @dbName";
                        cmd.Parameters.AddWithValue( "dbName", dbName );

                        if ( ( int ) cmd.ExecuteScalar() != 0 )
                        {
                            DeleteDatabase( connection, dbName );
                        }
                    }

                    CreateDatabase( connection, dbName, archive );
                }
            }

            RockCache.ClearAllCachedItems();
            }
            catch ( Exception ex )
            {
                throw ex;
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
        /// Creates the database from the files stored in the archive.
        /// </summary>
        /// <param name="connection">The connection to use when running SQL commands.</param>
        /// <param name="dbName">Name of the database to be created.</param>
        /// <param name="archive">The archive that contains the MDF and LDF files.</param>
        private static void CreateDatabase( DbConnection connection, string dbName, ZipArchive archive )
        {
            var mdf = archive.Entries.Where( e => e.Name.EndsWith( ".mdf" ) ).First();
            var ldf = archive.Entries.Where( e => e.Name.EndsWith( ".ldf" ) ).First();

            //
            // Extract the MDF file from the archive.
            //
            using ( var writer = File.Create( Path.Combine( GetDataPath(), string.Format( "{0}_Data.mdf", dbName ) ) ) )
            {
                using ( var reader = mdf.Open() )
                {
                    reader.CopyTo( writer );
                }
            }

            //
            // Extract the LDF file from the archive.
            //
            using ( var writer = File.Create( Path.Combine( GetDataPath(), string.Format( "{0}_Log.ldf", dbName ) ) ) )
            {
                using ( var reader = ldf.Open() )
                {
                    reader.CopyTo( writer );
                }
            }

            //
            // Execute the SQL command to create the database from existing files.
            //
            string sql = string.Format( @"
CREATE DATABASE [{0}]   
    ON (FILENAME = '{1}\{0}_Data.mdf'),  
    (FILENAME = '{1}\{0}_Log.ldf')  
    FOR ATTACH;", dbName, GetDataPath() );

            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandText = sql;
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
            string sql = string.Format( @"
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{0}] ;", name );

            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }

            File.Delete( Path.Combine( GetDataPath(), string.Format( "{0}_Data.mdf", name ) ) );
            File.Delete( Path.Combine( GetDataPath(), string.Format( "{0}_Log.mdf", name ) ) );
        }

        /// <summary>
        /// Downloads the URL and returns the path on disk to the downloaded file.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>A path to the file on disk.</returns>
        private static string DownloadUrl( string url )
        {
            string tempPath = Path.Combine( Path.GetTempPath(), "RockUnitTests" );
            string filename = Path.GetFileName( url );
            string downloadPath = Path.Combine( tempPath, filename );
            string downloadETagPath = downloadPath + ".etag";

            if ( !Directory.Exists( tempPath ) )
            {
                Directory.CreateDirectory( tempPath );
            }
            else
            {
                //
                // Delete any cached files older than 90 days.
                //
                foreach ( string file in Directory.GetFiles( tempPath ) )
                {
                    FileInfo fi = new FileInfo( file );

                    if ( fi.CreationTime < DateTime.Now.AddDays( -90 ) )
                    {
                        fi.Delete();
                    }
                }
            }

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
    }
}
