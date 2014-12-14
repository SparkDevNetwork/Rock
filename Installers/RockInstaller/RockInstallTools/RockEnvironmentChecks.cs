using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RockInstallTools
{
    public class RockEnvironmentChecks
    {

        private static string requiredDotnetVersion = "4.5.1";
        private static double iisVersionRequired = 7.0;
        private static int minimumDatabaseSize = 500;

        // internet connection test method
        public static EnvironmentCheckResult ConnectedToInternetTest()
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.Message = "You are connected to the Internet.";
            result.DidPass = true;

            WebClient client = new WebClient();

            try
            {
                string results = client.DownloadString( "https://rockrms.blob.core.windows.net/install/html-alive.txt" );

                if ( !results.Contains( "success" ) )
                {
                    result.DidPass = false;
                    result.Message = "It does not appear you are connected to the Internet. Rock requires a connection to download the installer.";
                }
            }
            catch ( Exception ex )
            {
                result.DidPass = false;
                result.Message = "Could not connect to the Internet.  Error: " + ex.Message;
                return result;
            }
            finally
            {
                client = null;
            }

            return result;
        }

        // writer permissions test method
        public static EnvironmentCheckResult WriteToFilesystemTest(string serverPath)
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#WebServerPermissions";
            result.Message = String.Format( "The username {0} does not have write access to the server's file system.", System.Security.Principal.WindowsIdentity.GetCurrent().Name );
            result.DidPass = false;

            string filename = serverPath + @"\write-permission.test";

            try
            {
                File.Create( filename ).Dispose();

                if ( File.Exists( filename ) )
                {
                    File.Delete( filename );
                    result.DidPass = true;
                    result.Message = "Your server's file permissions look correct.";
                }
            }
            catch ( Exception ex )
            {
                result.DidPass = false;
                result.Message = "Could not write to the file system. Error: " + ex.Message;
            }

            return result;
        }

        // check dot net version
        public static EnvironmentCheckResult DotNetVersionTest()
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#IncorrectDotNETVersion";
            result.DidPass = false;

            // check .net
            // ok this is not easy as .net 4.5.1 actually reports as 4.0.378675 or 4.0.378758 depending on how it was installed
            // http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions
            if ( System.Environment.Version.Major > 4 )
            {
                result.DidPass = true;
            }
            else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build > 30319 )
            {
                result.DidPass = true;
            }
            else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 && System.Environment.Version.Revision >= 18408 )
            {
                result.DidPass = true;
            }

            string version = System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString();

            if ( result.DidPass )
            {
                result.Message = String.Format( "You have the correct version of .Net ({0}+).", requiredDotnetVersion );
            }
            else
            {
                result.Message = String.Format( "The server does not have the correct .Net runtime.  You have .Net version {0} Rock requires version {1}.", version, requiredDotnetVersion );
            }
            

            return result;
        }

        // ensure rock is not yet installed
        public static EnvironmentCheckResult RockInstalledTest(string serverPath)
        {
            EnvironmentCheckResult isInstalledResult = new EnvironmentCheckResult();
            isInstalledResult.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#IsRockInstalledAlready";
            isInstalledResult.Message = "Website is empty.";
            isInstalledResult.DidPass = true;

            string rockFile = serverPath + @"\bin\Rock.dll";
            if ( File.Exists( rockFile ) )
            {
                isInstalledResult.DidPass = false;
                isInstalledResult.Message = "It appears that Rock is already installed in this directory. You must remove this version of Rock before proceeding.";
            }

            // check that sql server spatial files don't exist
            string sqlSpatialFiles32Bit = serverPath + @"\SqlServerTypes\x86\SqlServerSpatial110.dll";
            string sqlSpatialFiles64Bit = serverPath + @"\SqlServerTypes\x64\SqlServerSpatial110.dll";
            if ( File.Exists( sqlSpatialFiles32Bit ) || File.Exists( sqlSpatialFiles64Bit ) )
            {
                isInstalledResult.DidPass = false;
                isInstalledResult.Message = "You must remove the 'SqlServerTypes' folder before proceeding. You may need to stop the webserver to enable deletion.";
            }

            return isInstalledResult;
        }

        // check trust level
        public static EnvironmentCheckResult CheckTrustLevel( )
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#WebServerTrust";
            result.DidPass = false;

            try
            {
                new AspNetHostingPermission( AspNetHostingPermissionLevel.Unrestricted ).Demand();
                result.DidPass = true;
                result.Message = "Your webserver is configured for Full-Trust.";
            }
            catch ( System.Security.SecurityException )
            {
                result.DidPass = false;
                result.Message = "Your webserver is not configured for Full-Trust.";
            }

            return result;
        }

        // check iis version
        public static EnvironmentCheckResult CheckIisVersion( string iisString )
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#CheckIISVersion";
            result.DidPass = false;

            try
            {
                iisString = iisString.Split( '/' )[1];

                double iisVersion = Convert.ToDouble( iisString );

                if ( iisVersion >= iisVersionRequired )
                {
                    result.Message = "Your IIS version is correct.  You have version " + iisVersion + ".";
                    result.DidPass = true;
                }
                else
                {
                    result.Message = "The server's IIS version is not correct.  You have version " + iisVersion + " Rock requires version " + iisVersionRequired.ToString() + " or greater.";
                }
            }
            catch ( Exception ex )
            {
                result.DidPass = true;
                result.Message = "We could not determine your IIS version please ensure you are running IIS v " + iisVersionRequired.ToString() + " or better."; ;
            }

            return result; 
        }

        // check sql server
        public static EnvironmentCheckResult CheckSqlServer( string dbServer, string dbUsername, string dbPassword, string dbDatabase )
        {
            EnvironmentCheckResult result = new EnvironmentCheckResult();
            result.Message = "Your database settings all look good.";
            result.DidPass = true;

            // check that user can login
            DatabaseConnectionResult connectResult = CheckSqlLogin( dbServer, dbUsername, dbPassword );
            if ( !connectResult.CanConnect  )
            {
                result.Message = connectResult.Message;
                result.DidPass = false;
                return result;
            }

            // check sql version
            DatabaseConfigurationResult sqlVersionResult = CheckSqlServerVersion( dbServer, dbUsername, dbPassword );
            if ( !sqlVersionResult.Success )
            {
                result.Message = string.Format( "You are running SQL Server version: {0}", sqlVersionResult.Message );
                result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#SqlServerWrongVersion";
                result.DidPass = false;
                return result;
            }

            // check if database exists
            if ( !CheckSqlDatabaseExists( dbServer, dbUsername, dbPassword, dbDatabase ) )
            {
                // try creating the database
                if ( !CheckSqlPermissionsCreateDatabase( dbServer, dbUsername, dbPassword ) )
                {
                    result.Message = String.Format( "The database '{0}' does not exist and the user '{1}' does not have permissions to create a database.", dbDatabase, dbUsername );
                    result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#NoDatabaseNoPermissions";
                    result.DidPass = false;
                    return result;
                }

                result.Message = String.Format( "The '{0}' database does not exist on the server, but you have permissions to create it.  Rock will create it for you as part of the install.", dbDatabase );
            }
            else
            {
                // check that we have permissions to create a table in the database
                if ( !CheckSqlPermissionsCreateTable( dbServer, dbUsername, dbPassword, dbDatabase ) )
                {
                    result.Message = String.Format( "The user '{0}' does not have write access to the database '{1}'.", dbUsername, dbDatabase );
                    result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#SqlServerPermissions";
                    result.DidPass = false;
                    return result;
                }

                // since the database exists make sure it's empty
                if ( !CheckSqlServerEmpty( dbServer, dbUsername, dbPassword, dbDatabase ) )
                {
                    result.Message = String.Format( "The database '{0}' is not empty. To protect your existing data Rock must be installed into a empty database.", dbDatabase );
                    result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#DatabaseNotEmpty";
                    result.DidPass = false;
                    return result;
                }
            }

            DatabaseConfigurationResult sizeResult = CheckSqlDatabaseSize( dbServer, dbUsername, dbPassword, dbDatabase );
            if ( !sizeResult.Success )
            {
                result.Message = sizeResult.Message;
                result.HelpLink = "http://www.rockrms.com/Rock/LetsFixThis#DatabaseTooSmall";
                result.DidPass = false;
                return result;
            }

            return result;
        }

        #region Sql Helpers

        // check login
        public static DatabaseConnectionResult CheckSqlLogin( string dbServer, string dbUsername, string dbPassword )
        {
            DatabaseConnectionResult dbConnectResult = new DatabaseConnectionResult();

            // check password for characters that are not supported in a connection string
            Match match = Regex.Match( dbPassword, @"[,;(){}\[\]]", RegexOptions.IgnoreCase );
            if ( match.Success ) 
            {
                // we have an illegal character
                dbConnectResult.CanConnect = false;
                dbConnectResult.Message = "Please do not use one of the following characters in your database password '[] {}() , ; ? * ! @'.";
            }
            else
            {
                // setup connection
                string connectionString = String.Format( "user id={0};password={1};server={2};connection timeout=4", dbUsername, dbPassword, dbServer );
                SqlConnection testConnection = new SqlConnection( connectionString );

                // try connection
                try
                {
                    testConnection.Open();
                    dbConnectResult.CanConnect = true;
                    dbConnectResult.Message = String.Format( "The user {0} connected successfully to {1}.", dbUsername, dbServer );

                }
                catch ( Exception ex )
                {
                    dbConnectResult.CanConnect = false;
                    dbConnectResult.Message = String.Format( "The user {0} could not connect to the server {1}. Error message: {2}.", dbUsername, dbServer, ex.Message );
                }
                finally
                {
                    testConnection = null;
                }
            }

            

            return dbConnectResult;
        }

        // check sql server version
        private static DatabaseConfigurationResult CheckSqlServerVersion( string servername, string username, string password )
        {
            DatabaseConfigurationResult result = new DatabaseConfigurationResult();
            result.Success = false;

            string version = "0";
            string versionInfo = string.Empty;
            

            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};connection timeout=4", username, password, servername );
            SqlConnection testConnection = new SqlConnection( connectionString );

            // try connection
            try
            {
                testConnection.Open();
                SqlCommand versionCommand = new SqlCommand( "SELECT SERVERPROPERTY('productversion'), @@Version", testConnection );

                SqlDataReader versionReader = versionCommand.ExecuteReader();

                while ( versionReader.Read() )
                {
                    version = versionReader[0].ToString();
                    versionInfo = versionReader[1].ToString();
                }

                string[] versionParts = version.Split( '.' );

                int majorVersion = -1;
                Int32.TryParse( versionParts[0], out majorVersion );

                if ( majorVersion >= 10 )
                {
                    result.Success = true;
                }

                result.Message = versionInfo.Split( '-' )[0].ToString().Replace( "(RTM)", "" ).Trim();
            }
            catch ( Exception ex )
            {
                result.Success = false;
            }
            finally
            {
                testConnection = null;
            }

            return result;
        }

        // check if database exists
        public static bool CheckSqlDatabaseExists( string dbServer, string dbUsername, string dbPassword, string dbDatabase )
        {

            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};connection timeout=4", dbUsername, dbPassword, dbServer );
            SqlConnection testConnection = new SqlConnection( connectionString );

            // try connection
            try
            {
                testConnection.Open();

                string sql = String.Format( "SELECT count([name]) FROM master.dbo.sysdatabases WHERE [name] = '{0}'", dbDatabase );
                SqlCommand emptyCommand = new SqlCommand( sql, testConnection );

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if ( objectCount > 0 )
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch ( Exception ex )
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }

            return true;
        }

        // check to see if the database provided exists
        private static bool CheckSqlPermissionsCreateDatabase( string dbServer, string dbUsername, string dbPassword )
        {
            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};connection timeout=4", dbUsername, dbPassword, dbServer );
            SqlConnection testConnection = new SqlConnection( connectionString );

            // try connection
            try
            {
                testConnection.Open();

                string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'DATABASE') where permission_name = 'CREATE DATABASE'";
                SqlCommand emptyCommand = new SqlCommand( sql, testConnection );

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if ( objectCount > 0 )
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch ( Exception ex )
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }

            return true;
        }

        // check if user provided has permissions to create database
        private static bool CheckSqlPermissionsCreateTable( string servername, string username, string password, string database )
        {

            bool permissionsCorrect = false;

            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};Initial Catalog={3};connection timeout=4", username, password, servername, database );
            SqlConnection testConnection = new SqlConnection( connectionString );

            // try connection
            try
            {
                testConnection.Open();

                // test permissions by creating and dropping a table
                SqlCommand testCommand = new SqlCommand( "CREATE TABLE InstallText (Name TEXT); ", testConnection );
                testCommand.ExecuteNonQuery();

                testCommand = new SqlCommand( "DROP TABLE InstallText; ", testConnection );
                testCommand.ExecuteNonQuery();

                permissionsCorrect = true;
            }
            catch ( Exception ex )
            {
                permissionsCorrect = false;
            }
            finally
            {
                testConnection = null;
            }

            return permissionsCorrect;
        }

        // check if database is of proper size
        private static DatabaseConfigurationResult CheckSqlDatabaseSize( string servername, string username, string password, string database )
        {
            DatabaseConfigurationResult result = new DatabaseConfigurationResult();
            result.Success = true;
            result.Message = "Database meets size requirements";
           
            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};Initial Catalog={3};connection timeout=4", username, password, servername, database );
            SqlConnection testConnection = new SqlConnection( connectionString );
            SqlDataReader reader;

            // try connection
            try
            {
                testConnection.Open();

                // test permissions by creating and dropping a table
                SqlCommand testCommand = new SqlCommand( "sp_helpdb  " + database, testConnection );
                reader = testCommand.ExecuteReader();

                // get second data table
                reader.NextResult();

                reader.Read();
                string size = reader.GetValue( 5 ).ToString();

                if ( size != "Unlimited" )
                {
                    if ( size.Contains( "KB" ) )
                    {
                        size = size.Replace( " KB", "" );
                        int sizeInKB = Int32.Parse( size );

                        int sizeInMB = sizeInKB / 1024;

                        if ( sizeInMB < minimumDatabaseSize )
                        {
                            result.Success = false;
                            result.Message = String.Format( "The database '{0}' is not large enough to properly support Rock. Please ensure it is at least {1}MB in size (current size is {2}MB).", database, minimumDatabaseSize.ToString(), sizeInMB.ToString());
                        }
                    }
                }


            }
            catch ( Exception ex )
            {
                // we'll default to everything is OK
                result.Message = "Assuming database size is acceptable as we were unable to determine its size.";
                return result;
            }
            finally
            {
                testConnection = null;
            }

            return result;
        }

        // check is database is empty
        private static bool CheckSqlServerEmpty( string servername, string username, string password, string database )
        {

            // setup connection
            string connectionString = String.Format( "user id={0};password={1};server={2};Initial Catalog={3};connection timeout=4", username, password, servername, database );
            SqlConnection testConnection = new SqlConnection( connectionString );

            // try connection
            try
            {
                testConnection.Open();
                SqlCommand emptyCommand = new SqlCommand( "SELECT count([name]) FROM sysobjects WHERE [type] in ('P', 'U', 'V') AND category = 0", testConnection );

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if ( objectCount > 0 )
                {
                    return false;
                }
                else
                {
                    return true;

                }

            }
            catch ( Exception ex )
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }
        }

        #endregion
    }

    #region Return Classes
    public class DatabaseConnectionResult
    {
        public bool CanConnect
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }

    public class DatabaseConfigurationResult
    {
        public bool Success
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }

    public class EnvironmentCheckResult
    {
        public bool DidPass
        {
            get;
            set;
        }

        public string AsListItem
        {
            get
            {
                if ( this.DidPass )
                {
                    return String.Format( "<li><i class='{0}'></i> {1} </li>", this.IconCss, this.Message );
                }
                else
                {
                    return String.Format( "<li><i class='{0}'></i> {1} {2} </li>", this.IconCss, this.Message, this.HelpText );
                }
            }
        }

        public string IconCss
        {
            get
            {
                if ( this.DidPass )
                {
                    return "fa fa-check-circle pass";
                }
                else
                {
                    return "fa fa-exclamation-triangle fail";
                }
            }
        }

        public string Message
        {
            get;
            set;
        }

        public string HelpText
        {
            get
            {
                return String.Format( "<a href='{0}' class='btn btn-info btn-xs'>Let's Fix It Together</a>", this.HelpLink );
            }
        }

        public string HelpLink
        {
            get;
            set;
        }
    }
    #endregion
}
