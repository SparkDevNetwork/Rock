using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data.SqlClient;
using Microsoft.Win32;

namespace Rock.Install.Utilities
{
    static public class EnvironmentChecks
    {
        const string dotNetVersionRequired = "4.5";
        const double iisVersionRequired = 7.0;
        

        /// <summary>
        /// Checks that Rock is not already on the file system by checking for the existance of
        /// the web.ConnectionStrings.config
        /// </summary>
        public static bool CheckRockNotInstalled(string serverPath, out string errorDetails)
        {
            bool checksPassed = false;
            errorDetails = string.Empty;

            // check for rock database connection string
            string connectionStringFile = serverPath + @"\web.ConnectionStrings.config";
            string rockDll = serverPath + @"\bin\Rock.dll";

            if (File.Exists(connectionStringFile) && File.Exists(rockDll))
            {
                errorDetails = "Rock is already installed on this server.";
                checksPassed = false;
            }
            else
            {
                errorDetails = "Rock is not yet installed on this machine.";
                checksPassed = true;
            }

            return checksPassed;
        }

        /// <summary>
        /// CheckEnvironment checks to ensure we are connected to the internet and have write access to the web 
        /// server file system.
        /// </summary>
        public static bool CheckFileSystemPermissions(string serverPath, out string errorDetails)
        {

            bool checksPassed = false;
            errorDetails = string.Empty;

            // check for write access to the file system

            // first get user that the server is running as
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
            string userName = user.Translate(typeof(System.Security.Principal.NTAccount)).ToString();

            bool canWrite = false;

            // check rights on directory
            string filename = serverPath + @"\write-permission.test";

            try
            {
                File.Create(filename).Dispose();
            }
            catch (Exception ex)
            {

            }

            if (File.Exists(filename))
            {
                canWrite = true;
                File.Delete(filename);
            }

            if (!canWrite)
            {
                errorDetails += "The username " + userName + " does not have write access to the server's file system.";
            }
            else
            {
                errorDetails += "Your server's file permissions look correct.";
                checksPassed = true;
            }

            return checksPassed;
        }

        /// <summary>
        /// Checks the version of the .net runtime installed
        /// </summary>
        public static bool CheckDotNetVersion(out string errorDetails)
        {

            bool checksFailed = false;
            errorDetails = string.Empty;

            // check .net
            // ok this is not easy as .net 4.5 actually reports as 4.0.30319.269 so instead we need to search for the existence of an assembly that
            // only exists in 4.5 (could also look for Environment.Version.Major == 4 && Environment.Version.Revision > 17000 but this is not future proof)
            // sigh... Microsoft... :)
            if (!(Type.GetType("System.Reflection.ReflectionContext", false) != null))
            {
                errorDetails = "The server does not have the correct .Net runtime.  You have .Net version " + System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString() + " the Rock ChMS version requires " + dotNetVersionRequired + ".";
            }
            else
            {
                errorDetails += "You have the correct version of .Net (4.5+).";
                checksFailed = true;
            }

            return checksFailed;
        }

        /// <summary>
        /// Checks the version of the IIS installed
        /// </summary>
        public static bool CheckIisVersion(out string errorDetails)
        {

#if DEBUG
            errorDetails = "IIS version is correct (d).";
            return true;
#else
            bool checksPassed = false;
            errorDetails = string.Empty;

            RegistryKey parameters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\W3SVC\\Parameters");
            string iisVersion = parameters.GetValue("MajorVersion") + "." + parameters.GetValue("MinorVersion");

            if (double.Parse(iisVersion) >= iisVersionRequired)
            {
                errorDetails = "Your IIS version is correct.  You have version " + iisVersion + ".";
                checksPassed = true;
            }
            else
            {
                errorDetails = "The server's IIS version is not correct.  You have version " + iisVersion + " Rock requires version " + iisVersionRequired.ToString() + " or greater.";
            }

            return checksPassed; 
#endif

        }

        /// <summary>
        /// Checks the SQL Server configuration
        /// </summary>
        public static bool CheckSqlServer(string dbServer, string dbUsername, string dbPassword, string dbDatabase, out string errorDetails)
        {

            bool checksPassed = true;
            errorDetails = string.Empty;

            // check that user can login
            if (!CheckSqlLogin(dbServer, dbUsername, dbPassword))
            {
                errorDetails = String.Format("The user '{0}' could not login to {1}.", dbUsername, dbServer);
                return false;
            }

            string sqlVersion = string.Empty;
            // check sql version
            if (!CheckSqlServerVersion(dbServer, dbUsername, dbPassword, out sqlVersion))
            {
                errorDetails = string.Format("You are running SQL Server version: {0} <a href='http://www.rockchms.com/installer/help/sqlserver-version.html' class='btn btn-info btn-xs'>Let's Fix It Together</a>", sqlVersion);
                return false;
            }

            // check if database exists
            if (!CheckSqlDatabaseExists(dbServer, dbUsername, dbPassword, dbDatabase))
            {
                // try creating the database
                if (!CheckSqlPermissionsCreateDatabase(dbServer, dbUsername, dbPassword))
                {
                    errorDetails = String.Format("The database '{0}' does not exist and the user '{1}' does not have permissions to create a database. <a href='http://www.rockchms.com/installer/help/sqlserver-no-database-no-permissions.html' class='btn btn-info btn-xs'>Let's Fix It Together</a>", dbDatabase, dbUsername);
                    return false;
                }

                errorDetails = String.Format("The '{0}' database does not exist on the server, but you have persmissions to create it.  Rock will create it for you as part of the install.", dbDatabase);
            }
            else
            {
                // check that we have permissions to create a table in the database
                if (!CheckSqlPermissionsCreateTable(dbServer, dbUsername, dbPassword, dbDatabase))
                {
                    errorDetails = String.Format("The user '{0}' does not have write access to the database '{1}'. <a href='http://www.rockchms.com/installer/help/sqlserver-permissions.html' class='btn btn-info btn-xs'>Let's Fix It Together</a>", dbUsername, dbDatabase);
                    return false;
                }
                
                // since the database exists make sure it's empty
                if (!CheckSqlServerEmpty(dbServer, dbUsername, dbPassword, dbDatabase))
                {
                    errorDetails = String.Format("The database '{0}' is not empty. To protect your existing data Rock must be installed into a empty database. <a href='http://www.rockchms.com/installer/help/sqlserver-empty.html' class='btn btn-info btn-xs'>Let's Fix It Together</a>", dbDatabase);
                    return false;
                }
            }

            if (errorDetails == string.Empty)
            {
                errorDetails = "Your database settings all look good.";
            }

            return checksPassed;
        }

        #region Private SQL Helper Methods

        /// <summary>
        /// Checks that the login provided can login to the server
        /// </summary>
        public static bool CheckSqlLogin(string dbServer, string dbUsername, string dbPassword)
        {
            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};connection timeout=10", dbUsername, dbPassword, dbServer);
            SqlConnection testConnection = new SqlConnection(connectionString);

            // try connection
            try
            {
                testConnection.Open();

            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }

            return true;
        }

        /// <summary>
        /// Checks SQL Server version
        /// </summary>
        private static bool CheckSqlServerVersion(string servername, string username, string password, out string currentVersion) {

            currentVersion = string.Empty;
            string version = "0";
            string versionInfo = string.Empty;
            bool versionPassed = false;
                        
            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};connection timeout=10", username, password, servername);
            SqlConnection testConnection = new SqlConnection(connectionString);
                        
            // try connection
            try
            {
                testConnection.Open();
                SqlCommand versionCommand= new SqlCommand("SELECT SERVERPROPERTY('productversion'), @@Version", testConnection);
                            
                SqlDataReader versionReader = versionCommand.ExecuteReader();

                while(versionReader.Read())
                {
                    version = versionReader[0].ToString();
                    versionInfo = versionReader[1].ToString();
                }
                            
                string[] versionParts = version.Split('.');
                            
                int majorVersion = -1;
                Int32.TryParse(versionParts[0], out majorVersion);
                                    
                if (majorVersion >= 10) {
                        versionPassed = true;
                }

                currentVersion = versionInfo.Split('-')[0].ToString().Replace("(RTM)", "").Trim();
            }
            catch(Exception ex)
            {
                versionPassed = false;
            }
            finally {
                    testConnection = null;
            }
                        
            return versionPassed;
        }

        /// <summary>
        /// Checks to see if the database they provided exists
        /// </summary>
        private static bool CheckSqlDatabaseExists(string dbServer, string dbUsername, string dbPassword, string dbDatabase)
        {
            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};connection timeout=10", dbUsername, dbPassword, dbServer);
            SqlConnection testConnection = new SqlConnection(connectionString);

            // try connection
            try
            {
                testConnection.Open();

                string sql = String.Format("SELECT count([name]) FROM master.dbo.sysdatabases WHERE [name] = '{0}'", dbDatabase);
                SqlCommand emptyCommand = new SqlCommand(sql, testConnection);

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if (objectCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the user provided can create a database on the server
        /// </summary>
        private static bool CheckSqlPermissionsCreateDatabase(string dbServer, string dbUsername, string dbPassword)
        {
            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};connection timeout=10", dbUsername, dbPassword, dbServer);
            SqlConnection testConnection = new SqlConnection(connectionString);

            // try connection
            try
            {
                testConnection.Open();

                string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'DATABASE') where permission_name = 'CREATE TABLE'";
                SqlCommand emptyCommand = new SqlCommand(sql, testConnection);

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if (objectCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                testConnection = null;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the user provided can create a table in the database provided
        /// </summary>
        private static bool CheckSqlPermissionsCreateTable(string servername,  string username, string password, string database)
        {

            bool permissionsCorrect = false;

            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
            SqlConnection testConnection = new SqlConnection(connectionString);

            // try connection
            try
            {
                testConnection.Open();

                // test permissions by creating and dropping a table
                SqlCommand testCommand = new SqlCommand("CREATE TABLE InstallText (Name TEXT); ", testConnection);
                testCommand.ExecuteNonQuery();

                testCommand = new SqlCommand("DROP TABLE InstallText; ", testConnection);
                testCommand.ExecuteNonQuery();

                permissionsCorrect = true;
            }
            catch (Exception ex)
            {
                permissionsCorrect = false;
            }
            finally
            {
                testConnection = null;
            }

            return permissionsCorrect;
        }

        /// <summary>
        /// Checks to see if the database provided is empty
        /// </summary>
        private static bool CheckSqlServerEmpty(string servername, string username, string password, string database)
        {

            // setup connection
            string connectionString = String.Format("user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", username, password, servername, database);
            SqlConnection testConnection = new SqlConnection(connectionString);

            // try connection
            try
            {
                testConnection.Open();
                SqlCommand emptyCommand = new SqlCommand("SELECT count([name]) FROM sysobjects WHERE [type] in ('P', 'U', 'V') AND category = 0", testConnection);

                // get count of db objects
                Int32 objectCount = (Int32)emptyCommand.ExecuteScalar();

                if (objectCount > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }

            }
            catch (Exception ex)
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
}
