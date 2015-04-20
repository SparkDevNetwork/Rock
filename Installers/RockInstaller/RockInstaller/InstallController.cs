using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using Ionic.Zip;
using Microsoft.AspNet.SignalR;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data.SqlClient;
using System.IO;
using RockInstallTools;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace RockInstaller
{
    public class InstallController : Hub
    {

        private static List<ConsoleMessage> consoleHistory = new List<ConsoleMessage>();
        private static string serverPath;
        private static string sqlScripts = "sql-latest.zip";
        private static string rockSource = "rock-install-latest.zip";
        private static string sqlConfigureScript = "sql-config.sql";
        private static string baseStorageUrl = "http://storage.rockrms.com/install/";

        static InstallController()
        {
            serverPath = (System.Web.HttpContext.Current == null)
                    ? System.Web.Hosting.HostingEnvironment.MapPath( "~/" )
                    : System.Web.HttpContext.Current.Server.MapPath( "~/" );
        }


        # region Client Messaging Methods
        public void SendConsoleMessage( string message )
        {
            this.SendConsoleMessage( new ConsoleMessage( message ) );
        }
        
        public void SendConsoleMessage( ConsoleMessage message )
        {
            this.SendConsoleMessage( message, true, true );
        }

        public void SendConsoleMessage( ConsoleMessage message, bool sendToAllClients, bool writeToHistory )
        {
            if ( writeToHistory )
            {
                consoleHistory.Add( message );
            }
            
            if (sendToAllClients) {
                Clients.All.AddConsoleMessage( message );
            }
            else
            {
                Clients.Caller.AddConsoleMessage( message );
            }
        }

        public void UpdateProgressBar( int percentComplete )
        {
            Clients.Caller.UpdateProgressbar( percentComplete );
        }

        public void UpdateStep( string previousStep, string nextStepTitle )
        {
            Clients.Caller.UpdateStep( previousStep, nextStepTitle );
        }

        public void ReportError( string errorMessage )
        {
            Clients.Caller.ReportError( errorMessage );
        }

        public void RedirectToComplete( )
        {
            Clients.Caller.RedirectToComplete();
        }

        #endregion

        #region Helper Methods
        public void ReportServerInfo()
        {
            var context = Context.Request.GetHttpContext();
            
            this.SendConsoleMessage( new ConsoleMessage("Server Information", ConsoleMessageType.Highlight) );
            this.SendConsoleMessage( String.Format( "IIS Version: {0}", context.Request.ServerVariables["SERVER_SOFTWARE"] ) );
            this.SendConsoleMessage( String.Format( ".Net Version: {0}", System.Environment.Version ) );
            this.SendConsoleMessage( String.Format( "Operating System:  {0}", System.Environment.OSVersion ) );
            this.SendConsoleMessage( String.Format( "Machine Name:  {0}", System.Environment.MachineName ) );
            this.SendConsoleMessage( String.Format( "Current Directory:  {0}", System.Environment.CurrentDirectory ) );
            this.SendConsoleMessage( String.Format( "64 Bit OS/Process:  {0}/{1}", System.Environment.Is64BitOperatingSystem, System.Environment.Is64BitProcess ) );
        }

        private void ReportInstallData( InstallData installData )
        {
            this.SendConsoleMessage( new ConsoleMessage( "--= Install Data =--", ConsoleMessageType.Highlight ) );

            this.SendConsoleMessage( new ConsoleMessage( "Installer Properties", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Installer Version: {0}", installData.InstallerProperties.InstallVersion ) );

            this.SendConsoleMessage( new ConsoleMessage( "Connection String", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Server: {0}", installData.ConnectionString.Server ) );
            this.SendConsoleMessage( String.Format( "Database: {0}", installData.ConnectionString.Database ) );
            this.SendConsoleMessage( String.Format( "Username: {0}", installData.ConnectionString.Username ) );
            this.SendConsoleMessage( String.Format( "Password: {0}", "*****"));

            this.SendConsoleMessage( new ConsoleMessage( "Admin User", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Username: {0}", installData.AdminUser.Username ) );
            this.SendConsoleMessage( String.Format( "Password: {0}", "***** (Ha! Good one...)" ) );

            this.SendConsoleMessage( new ConsoleMessage( "Organization Info", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Name: {0}", installData.Organization.Name ) );
            this.SendConsoleMessage( String.Format( "Email: {0}", installData.Organization.Email ) );
            this.SendConsoleMessage( String.Format( "Phone: {0}", installData.Organization.Phone ) );
            this.SendConsoleMessage( String.Format( "Website: {0}", installData.Organization.Website ) );

            this.SendConsoleMessage( new ConsoleMessage( "Hosting Info", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Internal Url: {0}", installData.HostingInfo.InternalUrl ) );
            this.SendConsoleMessage( String.Format( "External Url: {0}", installData.HostingInfo.ExternalUrl ) );
            this.SendConsoleMessage( String.Format( "Timezone: {0}", installData.HostingInfo.Timezone + " (sounds like a nice place)" ) );

            this.SendConsoleMessage( new ConsoleMessage( "Email Settings", ConsoleMessageType.Highlight ) );
            this.SendConsoleMessage( String.Format( "Server: {0}", installData.EmailSettings.Server ) );
            this.SendConsoleMessage( String.Format( "Port: {0}", installData.EmailSettings.Port ) );
            this.SendConsoleMessage( String.Format( "Use SSL: {0}", installData.EmailSettings.UseSsl.ToString() ) );
            this.SendConsoleMessage( String.Format( "Relay Username: {0}", installData.EmailSettings.RelayUsername ) );
            this.SendConsoleMessage( String.Format( "Relay Password: {0}", "*****" ) );
        }

        // test the database login that was given
        public void TestDatabaseConnection( string server, string username, string password )
        {
            DatabaseConnectionResult dbResult =  RockEnvironmentChecks.CheckSqlLogin( server, username, password );
            Clients.Caller.RespondDbConnect( dbResult );
        }

        // test the environment
        public void TestRockEnvironment( string server, string username, string password, string database )
        {
            var context = Context.Request.GetHttpContext();
            StringBuilder testMessages = new StringBuilder();

            // check dotnet version
            EnvironmentCheckResult dotnetResults = RockEnvironmentChecks.DotNetVersionTest();
            testMessages.Append( dotnetResults.AsListItem );

            // check file permissions
            EnvironmentCheckResult filePermissionsResults = RockEnvironmentChecks.WriteToFilesystemTest( serverPath );
            testMessages.Append( filePermissionsResults.AsListItem );

            // check trust level
            EnvironmentCheckResult trustLevelResult = RockEnvironmentChecks.CheckTrustLevel();
            testMessages.Append( trustLevelResult.AsListItem );

            // check iis version
            EnvironmentCheckResult iisResult = RockEnvironmentChecks.CheckIisVersion( context.Request.ServerVariables["SERVER_SOFTWARE"] );
            testMessages.Append( iisResult.AsListItem );

            // check sql
            EnvironmentCheckResult sqlResult = RockEnvironmentChecks.CheckSqlServer( server, username, password, database );
            testMessages.Append( sqlResult.AsListItem );

            // check rock is not installed
            EnvironmentCheckResult rockInstalledResult = RockEnvironmentChecks.RockInstalledTest( serverPath );
            testMessages.Append( rockInstalledResult.AsListItem );

            EnvironmentCheckResults testResults = new EnvironmentCheckResults();

            if ( dotnetResults.DidPass && filePermissionsResults.DidPass && trustLevelResult.DidPass && iisResult.DidPass && sqlResult.DidPass && rockInstalledResult.DidPass )
            {
                // all tests passed
                testResults.Success = true;
                testResults.Results = String.Format( @"
                                            <h1>Pass!</h1> 
                                            <p>Your environment passed all tests and looks like a good home for the Rock RMS.  What are we waiting 
                                                for? Let's get started!!!</p>
                                             <ul class='list-unstyled fa-ul environment-test'>
                                                {0}
                                             </ul>", testMessages.ToString() );
            }
            else
            {
                testResults.Success = false;
                testResults.Results = String.Format( @"
                                            <h1>We Have Some Work To Do</h1> 
                                            <p>The server environment doesn't currently meet all of the requirements.  That's OK, we'll help get you back on track.</p>
                                             <ul class='list-unstyled fa-ul environment-test'>
                                                {0}
                                             </ul>", testMessages.ToString() );
            }

            Clients.Caller.RespondEnvironmentCheck( testResults );
        }
        
        #endregion


        // starts the install process
        public void StartInstall(InstallData installData)
        {
            ReportServerInfo();
            ReportInstallData(installData);

            // download sql file
            ActivityResult result = DownloadSqlStep( installData );
            if ( !result.Success ) {
                this.SendConsoleMessage( new ConsoleMessage( "Error downloading SQL file:" + result.Message, ConsoleMessageType.Critical ) );
                this.ReportError( String.Format( "<div class='alert alert-danger'><strong>That Wasn't Suppose To Happen</strong> An error occurred in the downloading of Rock's SQL files. Message: {0}</div>", result.Message ) );
                return;
            }

            Thread.Sleep( 1000 ); // slow the process to let progress bars catchup

            // download rock
            UpdateStep( "step-downloadsql", "Step 2: Downloading Rock" );

            result = DownloadRockStep( installData );
            if ( !result.Success )
            {
                this.SendConsoleMessage( new ConsoleMessage( "Error downloading Rock file:" + result.Message, ConsoleMessageType.Critical ) );
                this.ReportError( String.Format( "<div class='alert alert-danger'><strong>That Wasn't Suppose To Happen</strong> An error occurred in the downloading of Rock. Message: {0}</div>", result.Message ) );
                return;
            }

            Thread.Sleep( 1000 ); // slow the process to let progress bars catchup

            // create database
            UpdateStep( "step-downloadrock", "Step 3: Creating Database" );

            result = CreateDatabase( installData );
            if ( !result.Success )
            {
                this.SendConsoleMessage( new ConsoleMessage( "Error creating database:" + result.Message, ConsoleMessageType.Critical ) );
                this.ReportError( String.Format( "<div class='alert alert-danger'><strong>That Wasn't Suppose To Happen</strong> An error occurred creating database. Message: {0}</div>", result.Message ) );
                return;
            }

            Thread.Sleep( 1000 ); // slow the process to let progress bars catchup

            // configuring rock
            UpdateStep( "step-sql", "Step 4: Configuring Rock" );

            result = ConfigureRock( installData );
            if (!result.Success) {
                this.SendConsoleMessage( new ConsoleMessage( "Error configuring Rock:" + result.Message, ConsoleMessageType.Critical ) );
                this.ReportError( String.Format( "<div class='alert alert-danger'><strong>That Wasn't Suppose To Happen</strong> An error occurred while configuring Rock. Message: {0}</div>", result.Message ) );
                return;
            }

            Thread.Sleep( 1000 ); // slow the process to let progress bars catchup

            // unzip rock
            UpdateStep( "step-configure", "Step 5: Installing Rock Application" );

            result = InstallRockApp( installData );
            if ( !result.Success )
            {
                this.SendConsoleMessage( new ConsoleMessage( "Error installing Rock application:" + result.Message, ConsoleMessageType.Critical ) );
                this.ReportError( String.Format( "<div class='alert alert-danger'><strong>That Wasn't Suppose To Happen</strong> An error occurred while installing the Rock application.  At this point you may need to delete all files off of the web server and recreate the database once you address the error. Message: {0}</div>", result.Message ) );
                return;
            }

            Thread.Sleep( 1000 ); // slow the process to let progress bars catchup

            // redirect to the final complete page
            this.RedirectToComplete();
        }

        #region Install Steps
        private ActivityResult DownloadSqlStep(InstallData installData)
        {
            ActivityResult result = new ActivityResult();

            this.SendConsoleMessage( new ConsoleMessage( "--= Download SQL Step =--", ConsoleMessageType.Highlight ) );

            string serverDir = baseStorageUrl + installData.InstallerProperties.InstallVersion;
            this.SendConsoleMessage( "Downloading file: " + serverDir + "/Data/" + sqlScripts );

            result = DownloadFile( serverDir + "/Data/" + sqlScripts, serverPath + @"\" + sqlScripts, 10, 1 );

            this.SendConsoleMessage( "File Download Complete!" );

            return result;
        }

        private ActivityResult DownloadRockStep( InstallData installData )
        {
            ActivityResult result = new ActivityResult();

            this.SendConsoleMessage( new ConsoleMessage( "--= Download Rock Step =--", ConsoleMessageType.Highlight ) );

            string rockURL = baseStorageUrl + installData.InstallerProperties.InstallVersion;
            this.SendConsoleMessage( "Downloading file: " + rockURL + "/Data/" + rockSource );

            result = DownloadFile( rockURL + "/Data/" + rockSource, serverPath + @"\" + rockSource, 10, 1 );

            if ( result.Success )
            {
                this.SendConsoleMessage( "File Download Complete!" );
            }

            return result;
        }

        private ActivityResult CreateDatabase( InstallData installData )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;

            this.SendConsoleMessage( new ConsoleMessage( "--= Creating Database =--", ConsoleMessageType.Highlight ) );

            // create database if needed
            if ( !RockEnvironmentChecks.CheckSqlDatabaseExists( installData.ConnectionString.Server, installData.ConnectionString.Username, installData.ConnectionString.Password, installData.ConnectionString.Database ) )
            {
                this.SendConsoleMessage( String.Format("Database {0} does not exist creating it.", installData.ConnectionString.Database) );
                
                string sql = String.Format("CREATE DATABASE [{0}]", installData.ConnectionString.Database);

                string connectionString = String.Format( "user id={0};password={1};server={2};connection timeout=10", installData.ConnectionString.Username, installData.ConnectionString.Password, installData.ConnectionString.Server );
                SqlConnection conn = new SqlConnection( connectionString );

                try
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand( sql, conn );
                    command.ExecuteNonQuery();
                    this.SendConsoleMessage( String.Format("Database {0} created successfully.", installData.ConnectionString.Database) );
                }
                catch ( Exception ex )
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                this.SendConsoleMessage( String.Format( "Database {0} exists, will attempt to install using it.", installData.ConnectionString.Database ) );
            }

            // tease the user with a little progress
            this.UpdateProgressBar( 10 );

            // unzip sql files
            if ( result.Success )
            {
                this.SendConsoleMessage( "Unziping file: " + sqlScripts );
                string sqlZipFile = serverPath + @"\" + sqlScripts;
                result = UnzipFile( sqlZipFile, serverPath, 10, 1, 30, 10 );

                if ( result.Success )
                {
                    this.SendConsoleMessage( String.Format( "{0} sucessfully unzipped.", sqlScripts ) );
                }
            }

            // run sql install
            if ( result.Success )
            {
                this.SendConsoleMessage( "Preparing to run sql-install.sql" );

                result = RunSqlScript( installData, serverPath + @"\sql-install.sql", 10, 1, 60, 40 );

                if ( result.Success )
                {
                    this.SendConsoleMessage( String.Format( "Successfully ran sql-install.sql." ) );
                }
            }

            return result;
        }

        private ActivityResult ConfigureRock( InstallData installData )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;

            string tempWebConfig = serverPath + @"\webconfig.xml";

            this.SendConsoleMessage( new ConsoleMessage( "--= Configuring Rock =--", ConsoleMessageType.Highlight ) );

            string passwordKey = RockInstallUtilities.GeneratePasswordKey();

            // write connection string file
            result = WriteConnectionStringFile( installData.ConnectionString.Server, installData.ConnectionString.Database, installData.ConnectionString.Username, installData.ConnectionString.Password );
            if ( result.Success )
            {
                this.UpdateProgressBar( 20 );
                this.SendConsoleMessage( String.Format( "Successfully created web.ConnectionString.config." ) );
            }

            // create and run custom sql config
            if ( result.Success )
            {
                // download configure script
                string urlSqlConfigScript = baseStorageUrl + installData.InstallerProperties.InstallVersion + "/Data/" + sqlConfigureScript;
                string localSqlConfigScript = serverPath + @"\" + sqlConfigureScript;

                this.SendConsoleMessage( "Downloading SQL configuration script." );
                result = this.DownloadFile( urlSqlConfigScript, localSqlConfigScript, 0, 0 );

                if ( result.Success )
                {
                    this.SendConsoleMessage( "SQL configuration script downloaded." );

                    // merge settings
                    this.SendConsoleMessage( "Merging values into SQL config script." );
                    string sqlScript = System.IO.File.ReadAllText( localSqlConfigScript );
                    sqlScript = sqlScript.Replace( "{AdminPassword}", RockInstallUtilities.EncodePassword( installData.AdminUser.Password, "7E10A764-EF6B-431F-87C7-861053C84131", passwordKey ) );
                    sqlScript = sqlScript.Replace( "{AdminUsername}", SqlClean(installData.AdminUser.Username) );
                    sqlScript = sqlScript.Replace( "{PublicAppRoot}", RockInstallUtilities.CleanBaseAddress( installData.HostingInfo.ExternalUrl ) );
                    sqlScript = sqlScript.Replace( "{PublicAppSite}", RockInstallUtilities.GetDomainFromString( installData.HostingInfo.ExternalUrl ) );
                    sqlScript = sqlScript.Replace( "{InternalAppRoot}", RockInstallUtilities.CleanBaseAddress( installData.HostingInfo.InternalUrl ) );
                    sqlScript = sqlScript.Replace( "{InternalAppSite}", RockInstallUtilities.GetDomainFromString( installData.HostingInfo.InternalUrl ) );
                    sqlScript = sqlScript.Replace( "{OrgName}", SqlClean(installData.Organization.Name ));
                    sqlScript = sqlScript.Replace( "{OrgPhone}", SqlClean(installData.Organization.Phone ));
                    sqlScript = sqlScript.Replace( "{OrgEmail}", SqlClean(installData.Organization.Email ));
                    sqlScript = sqlScript.Replace( "{OrgWebsite}", SqlClean(installData.Organization.Website ));
                    sqlScript = sqlScript.Replace( "{SafeSender}", RockInstallUtilities.GetDomainFromEmail( installData.Organization.Email ) );
                    sqlScript = sqlScript.Replace( "{EmailException}", SqlClean(installData.Organization.Email ));
                    sqlScript = sqlScript.Replace( "{SmtpServer}", SqlClean(installData.EmailSettings.Server) );
                    sqlScript = sqlScript.Replace( "{SmtpPort}", installData.EmailSettings.Port );
                    sqlScript = sqlScript.Replace( "{SmtpUser}", SqlClean(installData.EmailSettings.RelayUsername ));
                    sqlScript = sqlScript.Replace( "{SmtpPassword}", SqlClean( installData.EmailSettings.RelayPassword ) );
                    sqlScript = sqlScript.Replace( "{SmtpUseSsl}", installData.EmailSettings.UseSsl.ToString() );

                    System.IO.File.WriteAllText( localSqlConfigScript, sqlScript );

                    this.SendConsoleMessage( "Values merged into SQL config script." );

                    // run sql configure script
                    this.SendConsoleMessage( "Running SQL config script." );
                    result = this.RunSql( installData, localSqlConfigScript );

                    if ( result.Success )
                    {
                        this.SendConsoleMessage( "Successfully ran SQL config script." );
                        this.UpdateProgressBar( 40 );
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            // extract web.config
            string rockZipFile = serverPath + @"\" + rockSource;
            if ( result.Success )
            {
                this.SendConsoleMessage( "Extracting web.config file for edits." );

                try
                {
                    using ( FileStream fsZipFile = File.Create( tempWebConfig ) )
                    {
                        using ( ZipFile zip = ZipFile.Read( rockZipFile ) )
                        {
                            ZipEntry webConfigEntry = zip["web.config"];
                            webConfigEntry.Extract( fsZipFile );

                            // remove file from zip
                            zip.RemoveEntry( "web.config" );
                            zip.Save();
                        }
                    }

                    this.SendConsoleMessage( "Extracted web.config to webconfig.xml." );
                    this.UpdateProgressBar( 60 );
                }
                catch ( Exception ex )
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }

            try
            {
                // edit web.config
                XDocument document = XDocument.Load( tempWebConfig );

                // update timezone
                var node = document.Descendants( "appSettings" ).Elements( "add" ).Where( e => e.Attribute( "key" ).Value == "OrgTimeZone" ).FirstOrDefault();
                node.SetAttributeValue( "value", installData.HostingInfo.Timezone );

                // update password key
                node = document.Descendants( "appSettings" ).Elements( "add" ).Where( e => e.Attribute( "key" ).Value == "PasswordKey" ).FirstOrDefault();
                node.SetAttributeValue( "value", passwordKey );

                // update data encryption key
                string dataEncryptionKey = RockInstallUtilities.GeneratePasswordKey( 128 );
                node = document.Descendants( "appSettings" ).Elements( "add" ).Where( e => e.Attribute( "key" ).Value == "DataEncryptionKey" ).FirstOrDefault();
                node.SetAttributeValue( "value", dataEncryptionKey );

                // update machine key
                string validationKey = RockInstallUtilities.GenerateMachineKey( 64 );
                string decryptionKey = RockInstallUtilities.GenerateMachineKey( 32 );
                node = document.Descendants( "system.web" ).Elements( "machineKey" ).FirstOrDefault();
                node.SetAttributeValue( "validationKey", validationKey );
                node.SetAttributeValue( "decryptionKey", decryptionKey );

                document.Save( tempWebConfig );
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = "An error occurred while customizing the web.config file for Rock. " + ex.Message;
            }

            this.UpdateProgressBar( 100 );
            return result;
        }

        private ActivityResult InstallRockApp( InstallData installData )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;

            // if we're not in development unzip the file, otherwise we'll fake it... (for that full experience)
            if ( !installData.InstallerProperties.IsDebug )
            {
                this.SendConsoleMessage( "Preparing to unzip the rock application file." );
                
                // unzip the rock zip
                result = UnzipFile( serverPath + @"\" + rockSource, serverPath + @"\rock", 10, 1, 100, 0 );

                this.SendConsoleMessage( "Unzip complete." );
            }
            else
            {
                // fake it...
                for ( int i = 0; i <= 100; i++ )
                {
                    UpdateProgressBar( i );
                    Thread.Sleep( 100 ); // nap time...
                }
            }

            return result;
        }

        #endregion

        #region Utility Methods

        private string SqlClean( string item )
        {
            return item.Replace( "'", "''" );
        }

        private ActivityResult RunSqlScript( InstallData installData, string sqlScriptFile, int consoleMessageReportFrequency, int progressbarEventFrequency, int percentOfStep, int startPercent )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;
            SqlConnection conn = null;

            string currentScript = string.Empty;

            try
            {
                string sql = System.IO.File.ReadAllText( sqlScriptFile );

                string connectionString = String.Format( "user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", installData.ConnectionString.Username, installData.ConnectionString.Password, installData.ConnectionString.Server, installData.ConnectionString.Database );
                conn = new SqlConnection( connectionString );
                conn.Open();

                RockScriptParser scriptParser = new RockScriptParser(sql);

                int sqlScriptCount = scriptParser.ScriptCount;
                int scriptsRun = 0;
                int scriptNextConsolePercentile = consoleMessageReportFrequency;
                int scriptNextProgressbarPercentile = progressbarEventFrequency;

                if ( consoleMessageReportFrequency != 0 )
                {
                    this.SendConsoleMessage( String.Format("There are {0} scripts to run.", sqlScriptCount) );
                }

                using ( SqlTransaction transaction = conn.BeginTransaction() )
                {
                    foreach ( var script in scriptParser.ScriptCollection )
                    {
                        currentScript = script;

                        if ( !string.IsNullOrWhiteSpace( script ) )
                        {
                            SqlCommand command = new SqlCommand( script, conn, transaction );
                            command.ExecuteNonQuery();
                        }

                        scriptsRun++;

                        // calculate current percentile
                        int currentPercentile = (int)(((double)scriptsRun / (double)sqlScriptCount) * 100);

                        // update console messages
                        if ( consoleMessageReportFrequency != 0 )
                        {
                            if ( sqlScriptCount == scriptsRun )
                            {
                                this.SendConsoleMessage( "100% of scripts run" );
                            }
                            else if ( currentPercentile >= scriptNextConsolePercentile )
                            {
                                this.SendConsoleMessage( currentPercentile + "% of scripts run" );
                                scriptNextConsolePercentile = currentPercentile + consoleMessageReportFrequency;
                            }
                        }

                        // update progress bar
                        if ( progressbarEventFrequency != 0 )
                        {
                            if ( sqlScriptCount == scriptsRun )
                            {
                                this.UpdateProgressBar( (int)(100 * (percentOfStep * .01)) + startPercent );
                            }
                            else if ( currentPercentile >= scriptNextProgressbarPercentile )
                            {
                                this.UpdateProgressBar( (int)(currentPercentile * (percentOfStep * .01)) + startPercent );
                                scriptNextProgressbarPercentile = currentPercentile + progressbarEventFrequency;
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = ex.Message + " Current Script: " + currentScript;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        private ActivityResult RunSql( InstallData installData, string sqlFile )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;
            SqlConnection conn = null;

            try
            {
                string sql = System.IO.File.ReadAllText( sqlFile );

                string connectionString = String.Format( "user id={0};password={1};server={2};Initial Catalog={3};connection timeout=10", installData.ConnectionString.Username, installData.ConnectionString.Password, installData.ConnectionString.Server, installData.ConnectionString.Database );
                conn = new SqlConnection( connectionString );
                conn.Open();

                SqlCommand command = new SqlCommand( sql, conn );
                command.ExecuteNonQuery();
                
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        private ActivityResult DownloadFile( string remoteFile, string localFile, int consoleMessageReportFrequency, int progressbarEventFrequency )
        {
            // solution from http://forums.xamarin.com/discussion/2022/threading-help

            // consoleMessageReportFrequency - how often, in percents, should a console message be sent (0 = none)
            // progressbarEventFrequency - how often should a progress bar event be fired, in percents (0 = none)

            ActivityResult result = new ActivityResult();
            result.Success = true;

            try
            {
                Uri url = new Uri( remoteFile );

                // Get the total size of the file
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create( url );
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                response.Close();
                double dTotal = (double)response.ContentLength;

                // keeps track of the total bytes downloaded so we can update the progress bar
                Int64 iRunningByteTotal = 0;

                int nextProgressbarPercentToNotify = progressbarEventFrequency;
                int nextConsolePercentToNotify = consoleMessageReportFrequency;

                // use the webclient object to download the file
                using ( System.Net.WebClient client = new System.Net.WebClient() )
                {
                    // open the file at the remote URL for reading
                    using ( System.IO.Stream streamRemote = client.OpenRead( url ) )
                    {
                        // using the FileStream object, we can write the downloaded bytes to the file system
                        using ( Stream streamLocal = new FileStream( localFile, FileMode.Create, FileAccess.Write, FileShare.None ) )
                        {
                            // loop the stream and get the file into the byte buffer
                            int iByteSize = 0;
                            int bufferSize = 65536;
                            byte[] byteBuffer = new byte[bufferSize];

                            while ( (iByteSize = streamRemote.Read( byteBuffer, 0, byteBuffer.Length )) > 0 )
                            {
                                // write the bytes to the file system at the file path specified
                                streamLocal.Write( byteBuffer, 0, iByteSize );
                                iRunningByteTotal += iByteSize;

                                // calculate the progress out of a base "100"
                                double dIndex = (double)(iRunningByteTotal);
                                double dProgressPercentage = (dIndex / dTotal);
                                int iProgressPercentage = (int)(dProgressPercentage * 100);

                                // send console notifications
                                if ( consoleMessageReportFrequency != 0 )
                                {
                                    if ( iProgressPercentage == 100 )
                                    {
                                        this.SendConsoleMessage( "100% downloaded" );
                                    } else if ( iProgressPercentage >= nextConsolePercentToNotify )
                                    {
                                        this.SendConsoleMessage( nextConsolePercentToNotify.ToString() + "% downloaded" );
                                        nextConsolePercentToNotify += consoleMessageReportFrequency;
                                    }
                                }

                                // send progress bar notifications
                                if ( progressbarEventFrequency != 0 )
                                {
                                    if ( iProgressPercentage == 100 )
                                    {
                                        this.UpdateProgressBar( 100 );
                                    } else if ( iProgressPercentage >= nextProgressbarPercentToNotify )
                                    {
                                        this.UpdateProgressBar( iProgressPercentage );
                                        nextProgressbarPercentToNotify += progressbarEventFrequency;
                                    }
                                }
                            } // while..
                            streamLocal.Close();
                            streamLocal.Dispose();
                        }
                        streamRemote.Close();
                        streamRemote.Dispose();
                    }
                    client.Dispose();
                }
            }
            catch ( OutOfMemoryException mex )
            {
                result.Success = false;
                result.Message = @"The server ran out of memory while downloading Rock. You may want to consider using a server with more available resources or
                                    try installing again.";
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private ActivityResult WriteConnectionStringFile( string server, string database, string username, string password )
        {
            ActivityResult result = new ActivityResult();
            result.Success = true;

            try
            {
                string configContents = String.Format( @"<add name=""RockContext"" connectionString=""Data Source={0};Initial Catalog={1}; User Id={2}; password={3};MultipleActiveResultSets=true"" providerName=""System.Data.SqlClient""/>", server, database, username, password );

                StreamWriter configFile = new StreamWriter( serverPath + "/web.ConnectionStrings.config", false );
                configFile.WriteLine( @"<?xml version=""1.0""?>" );
                configFile.WriteLine( @"<connectionStrings>" );
                configFile.WriteLine( "\t" + configContents );
                configFile.WriteLine( "</connectionStrings>" );
                configFile.Flush();
                configFile.Close();
                configFile.Dispose();
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private ActivityResult UnzipFile( string sourceFile, string destPath, int consoleMessageReportFrequency, int progressbarEventFrequency, int percentOfStep, int startPercent )
        {
            // consoleMessageReportFrequency - how often, in percents, should a console message be sent (0 = none)
            // progressbarEventFrequency - how often should a progress bar event be fired, in percents (0 = none)
            // percentOfStep - what percentage of this step is this unzip process
            // startPercent - what is the starting percentage to use for the progressbar
            
            ActivityResult result = new ActivityResult();
            result.Success = true;

            int zipFileCount = 0;
            int unzippedFiles = 0;
            int unzipNextConsolePercentile = consoleMessageReportFrequency;
            int unzipNextProgressbarPercentile = progressbarEventFrequency;

            try
            {
                using ( ZipFile zip = ZipFile.Read( sourceFile ) )
                {
                    zipFileCount = zip.Count;

                    // unzip each file updating console and progressbar
                    foreach ( ZipEntry e in zip )
                    {
                        e.Extract( destPath, ExtractExistingFileAction.OverwriteSilently );

                        unzippedFiles++;

                        // calculate current percentile
                        int currentPercentile = (int)(((double)unzippedFiles / (double)zipFileCount) * 100);

                        // update console messages
                        if ( consoleMessageReportFrequency != 0 )
                        {
                            if ( zipFileCount == unzippedFiles )
                            {
                                this.SendConsoleMessage( "100% unzipped" );
                            }
                            else if ( currentPercentile >= unzipNextConsolePercentile )
                            {
                                this.SendConsoleMessage( currentPercentile + "% unzipped" );
                                unzipNextConsolePercentile = currentPercentile + consoleMessageReportFrequency;
                            }
                        }

                        // update progress bar
                        if ( progressbarEventFrequency != 0 )
                        {
                            if ( zipFileCount == unzippedFiles )
                            {
                                this.UpdateProgressBar( (int)(100 * (percentOfStep * .01)) + startPercent );
                            }
                            else if ( currentPercentile >= unzipNextProgressbarPercentile )
                            {
                                int progressbarState = (int)(currentPercentile * (percentOfStep * .01)) + startPercent;
                                this.UpdateProgressBar( progressbarState );
                                unzipNextProgressbarPercentile = currentPercentile + progressbarEventFrequency;
                            }
                        }
                    } 
                }
            }
            catch ( Exception ex )
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        #endregion
    }

    #region Utility Classes
    public class ActivityResult {
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

    public class InstallData
    {
        public ConnectionString ConnectionString
        {
            get;
            set;
        }

        public AdminUser AdminUser
        {
            get;
            set;
        }

        public HostingInfo HostingInfo
        {
            get;
            set;
        }

        public Organization Organization
        {
            get;
            set;
        }

        public EmailSettings EmailSettings
        {
            get;
            set;
        }

        public InstallerProperties InstallerProperties
        {
            get;
            set;
        }
    }

    public class ConnectionString {
        public string Server
        {
            get;
            set;
        }

        public string Database
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }
    }

    public class AdminUser {

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }
    }

    public class HostingInfo {

        public string InternalUrl
        {
            get;
            set;
        }

        public string ExternalUrl
        {
            get;
            set;
        }

        public string Timezone
        {
            get;
            set;
        }
    }

    public class Organization {

        public string Name
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string Phone
        {
            get;
            set;
        }

        public string Website
        {
            get;
            set;
        }
    }

    public class EmailSettings {

        public string Server
        {
            get;
            set;
        }

        public string Port
        {
            get;
            set;
        }

        public bool UseSsl
        {
            get;
            set;
        }

        public string RelayUsername
        {
            get;
            set;
        }

        public string RelayPassword
        {
            get;
            set;
        }
    }

    public class InstallerProperties {

        public string InstallVersion
        {
            get;
            set;
        }

        public bool IsDebug
        {
            get;
            set;
        }
    }

    public class ConsoleMessage
    {
        public string Message
        {
            get;
            set;
        }

        [JsonConverter( typeof( StringEnumConverter ) )]
        public ConsoleMessageType Type
        {
            get;
            set;
        }

        public string UtcTime
        {
            get;
            set;
        }

        public ConsoleMessage( string message ) : this(message, ConsoleMessageType.Info)
        {}

        public ConsoleMessage( string message, ConsoleMessageType type )
        {
            Message = message;
            Type = type;

            DateTime nowUtc = DateTime.UtcNow;
            UtcTime = String.Format( "{0}:{1}:{2}", nowUtc.Hour, nowUtc.Minute, nowUtc.Second );
        }
    }

    public enum ConsoleMessageType
    {
        Info, Warning, Critical, Highlight
    }

    public class EnvironmentCheckResults {
        public bool Success
        {
            get;
            set;
        }
        public string Results
        {
            get;
            set;
        }
    }

    #endregion
}