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
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

using Rock;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.VersionInfo;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Displays system information on the installed version of Rock.
    /// </summary>
    [DisplayName( "System Information" )]
    [Category( "Administration" )]
    [Description( "Displays system information on the installed version of Rock." )]
    public partial class SystemInfo : Rock.Web.UI.RockBlock
    {
        #region Fields

        private string _catalog = String.Empty;

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Get Version, database info and executing assembly location
            lRockVersion.Text = string.Format( "{0} <small>({1})</small>", VersionInfo.GetRockProductVersionFullName(), VersionInfo.GetRockProductVersionNumber() );
            lClientCulture.Text = System.Globalization.CultureInfo.CurrentCulture.ToString();
            lDatabase.Text = GetDbInfo();
            lSystemDateTime.Text = DateTime.Now.ToString( "G" ) + " " + DateTime.Now.ToString( "zzz" );
            lRockTime.Text = Rock.RockDateTime.Now.ToString( "G" ) + " " + Rock.RockDateTime.OrgTimeZoneInfo.BaseUtcOffset.ToString();
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            if ( currentProcess != null && currentProcess.StartTime != null )
            {
                lProcessStartTime.Text = currentProcess.StartTime.ToString( "G" ) + " " + DateTime.Now.ToString( "zzz" );
            }
            else
            {
                lProcessStartTime.Text = "-";
            }

            try
            {
                lExecLocation.Text = Assembly.GetExecutingAssembly().Location + "<br/>" + HttpRuntime.AppDomainAppPath;
            }
            catch { }
            lLastMigrations.Text = GetLastMigrationData();

            var transactionQueueStats = RockQueue.TransactionQueue.ToList().GroupBy( a => a.GetType().Name ).ToList().Select( a => new { Name = a.Key, Count = a.Count() } );
            lTransactionQueue.Text = transactionQueueStats.Select( a => string.Format( "{0}: {1}", a.Name, a.Count ) ).ToList().AsDelimited( "<br/>" );

            lCacheOverview.Text = GetCacheInfo();
            lRoutes.Text = GetRoutesInfo();
            lThreads.Text = GetThreadInfo();

            // register btnDumpDiagnostics as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( btnDumpDiagnostics );
        }

        protected override void OnPreRender( EventArgs e )
        {
            if ( Context.Items.Contains( "Cache_Hits" ) )
            {
                var cacheHits = Context.Items["Cache_Hits"] as System.Collections.Generic.Dictionary<string, bool>;
                if ( cacheHits != null )
                {
                    var misses = cacheHits.Where( c => !c.Value );
                    if ( misses.Any() )
                    {
                        lFalseCacheHits.Text = string.Format( "<p><strong>Cache Misses:</strong><br /> {0}</p>",
                            misses.Select( c => c.Key )
                                .OrderBy( c => c )
                                .ToList()
                                .AsDelimited( "<br />" ) );
                    }
                }
            }
            
            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Used to manually flush the attribute cache.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnClearCache_Click( object sender, EventArgs e )
        {
            var msgs = RockCache.ClearAllCachedItems();

            // Flush today's Check-in Codes
            Rock.Model.AttendanceCodeService.FlushTodaysCodes();

            string webAppPath = Server.MapPath( "~" );

            // Check for any unregistered entity types, field types, and block types
            EntityTypeService.RegisterEntityTypes( webAppPath );
            FieldTypeService.RegisterFieldTypes( webAppPath );
            BlockTypeService.RegisterBlockTypes( webAppPath, Page, false );
            msgs.Add( "EntityTypes, FieldTypes, BlockTypes have been re-registered" );

            // Delete all cached files
            try
            {
                var dirInfo = new DirectoryInfo( Path.Combine( webAppPath, "App_Data/Cache" ) );
                foreach ( var childDir in dirInfo.GetDirectories() )
                {
                    childDir.Delete( true );
                }
                foreach ( var file in dirInfo.GetFiles().Where( f => f.Name != ".gitignore" ) )
                {
                    file.Delete();
                }
                msgs.Add( "Cached files have been deleted" );
            }
            catch ( Exception ex )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Visible = true;
                nbMessage.Text = "The following error occurred when attempting to delete cached files: " + ex.Message;
                return;
            }

            nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Success;
            nbMessage.Visible = true;
            nbMessage.Title = "Clear Cache";
            nbMessage.Text = string.Format( "<p>{0}</p>", msgs.AsDelimited( "<br />" ) );
        }

        protected void btnRestart_Click( object sender, EventArgs e )
        {
            RestartWebApplication();
        }

        /// <summary>
        /// Creates a text version (mostly) of the Diagnostics data that is sent via the HttpResponse to the client.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDumpDiagnostics_Click( object sender, EventArgs e )
        {
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;

            response.ClearHeaders();
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/plain";
            response.AddHeader( "content-disposition", "attachment; filename=RockDiagnostics-" + System.Environment.MachineName + ".txt" );
            response.Charset = "";

            ResponseWrite( "Version:", lRockVersion.Text, response );
            ResponseWrite( "Database:", lDatabase.Text.Replace( "<br />", Environment.NewLine.ToString() ), response );
            ResponseWrite( "Execution Location:", lExecLocation.Text, response );
            ResponseWrite( "Migrations:", GetLastMigrationData().Replace( "<br />", Environment.NewLine.ToString() ), response );
            ResponseWrite( "Cache:", lCacheOverview.Text.Replace( "<br />", Environment.NewLine.ToString() ), response ); ;
            ResponseWrite( "Routes:", lRoutes.Text.Replace( "<br />", Environment.NewLine.ToString() ), response );
            ResponseWrite( "Threads:", lThreads.Text.Replace( "<br />", Environment.NewLine.ToString() ), response );

            // Last and least...
            ResponseWrite( "Server Variables:", "", response );
            foreach ( string key in Request.ServerVariables )
            {
                if ( !key.Equals("HTTP_COOKIE", StringComparison.OrdinalIgnoreCase ) )
                {
                    ResponseWrite( key, Request.ServerVariables[key], response );
                } 
            }

            response.Flush();
            response.End();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Queries the MigrationHistory and the PluginMigration tables and returns 
        /// the name (MigrationId) of the last core migration that was run and a table
        /// listing the last plugin assembly's migration name and number that was run. 
        /// </summary>
        /// <returns>An HTML fragment of the MigrationId of the last core migration and a table of the
        /// last plugin migrations.</returns>
        private string GetLastMigrationData()
        {
            StringBuilder sb = new StringBuilder();

            var result = DbService.ExecuteScaler( "SELECT TOP 1 [MigrationId] FROM [__MigrationHistory] ORDER BY [MigrationId] DESC ", CommandType.Text, null );
            if ( result != null )
            {
                sb.AppendFormat( "Last Core Migration: {0}{1}", (string)result, Environment.NewLine );
            }

            var tableResult = DbService.GetDataTable( @"
    WITH summary AS 
    (
        SELECT p.[PluginAssemblyName], p.MigrationName, p.[MigrationNumber], ROW_NUMBER() 
            OVER( PARTITION BY p.[PluginAssemblyName] ORDER BY p.[MigrationNumber] DESC ) AS section
        FROM [PluginMigration] p
    )
    SELECT s.[PluginAssemblyName], s.MigrationName, s.[MigrationNumber]
    FROM summary s
    WHERE s.section = 1", System.Data.CommandType.Text, null );

            if ( tableResult != null )
            {
                sb.AppendFormat( "<table class='table table-condensed'>" );
                sb.Append( "<tr><th>Plugin Assembly</th><th>Migration Name</th><th>Number</th></tr>" );
                foreach ( DataRow row in tableResult.Rows )
                {
                    sb.AppendFormat( "<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", row[0].ToString(), row[1].ToString(), row[2].ToString() );
                }
                sb.AppendFormat( "</table>" );
            }

            return sb.ToString();
        }

        private string GetCacheInfo()
        {
            StringBuilder sb = new StringBuilder();

            var cacheStats = RockCache.GetAllStatistics();
            foreach ( CacheItemStatistics cacheItemStat in cacheStats.OrderBy( s => s.Name ) )
            {
                foreach( CacheHandleStatistics cacheHandleStat in cacheItemStat.HandleStats )
                {
                    var stats = new List<string>();
                    cacheHandleStat.Stats.ForEach( s => stats.Add( string.Format( "{0}: {1:N0}", s.CounterType.ConvertToString(), s.Count ) ) );
                    sb.AppendFormat( "<p><strong>{0}:</strong><br/>{1}</p>{2}", cacheItemStat.Name, stats.AsDelimited(", "), Environment.NewLine );
                }
            }

            lCacheObjects.Text = sb.ToString();

            return string.Empty;
        }

        private string GetRoutesInfo()
        {
            var pageService = new PageService( new RockContext() );
            
            var routes = new Dictionary<string, System.Web.Routing.Route>();
            foreach ( System.Web.Routing.Route route in System.Web.Routing.RouteTable.Routes.OfType<System.Web.Routing.Route>() )
            {
                if ( !routes.ContainsKey( route.Url ) )
                    routes.Add( route.Url, route );
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<table class='table table-condensed'>" );
            sb.Append( "<tr><th>Route</th><th>Pages</th></tr>" );
            foreach ( var routeItem in routes )
            {
                //sb.AppendFormat( "{0}<br />", routeItem.Key );
                var pages = pageService.GetListByIds( routeItem.Value.PageIds() );

                sb.AppendFormat( "<tr><td>{0}</td><td>{1}</td></tr>", routeItem.Key, string.Join( "<br />", pages.Select( n => n.InternalName + " (" + n.Id.ToString() + ")" ).ToArray() ) );
            }

            sb.AppendFormat( "</table>" );
            return sb.ToString();
        }

        /// <summary>
        /// Gets thread pool details such as the number of threads in use and the maximum number of threads.
        /// </summary>
        /// <returns></returns>
        private string GetThreadInfo()
        {
            int maxWorkerThreads = 0;
            int maxIoThreads = 0;
            int availWorkerThreads = 0;
            int availIoThreads = 0;

            ThreadPool.GetMaxThreads( out maxWorkerThreads, out maxIoThreads );
            ThreadPool.GetAvailableThreads( out availWorkerThreads, out availIoThreads );
            var workerThreadsInUse = maxWorkerThreads - availWorkerThreads;
            var pctUse = ( ( float ) workerThreadsInUse / ( float ) maxWorkerThreads );
            string badgeType = string.Empty;
            if ( pctUse > .1 )
            {
                if ( pctUse < .3 )
                {
                    badgeType = "badge badge-warning";
                }
                else
                {
                    badgeType = "badge badge-danger";
                }
            }

            return string.Format( "<span class='{0}'>{1}</span> out of {2} worker threads in use ({3}%)", badgeType, workerThreadsInUse, maxWorkerThreads, ( int ) Math.Ceiling( pctUse * 100 ) );
        }

        private string GetDbInfo()
        {
            StringBuilder databaseResults = new StringBuilder();
            
            var csBuilder = new System.Data.Odbc.OdbcConnectionStringBuilder( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
            object dataSource, catalog = string.Empty;
            if ( csBuilder.TryGetValue( "data source", out dataSource ) && csBuilder.TryGetValue( "initial catalog", out catalog ) )
            {
                _catalog = catalog.ToString();
                databaseResults.Append( string.Format( "Name: {0} <br /> Server: {1}", catalog, dataSource ) );
            }

            try
            {
                // get database version
                var reader = DbService.GetDataReader( "SELECT SERVERPROPERTY('productversion'), @@Version ", System.Data.CommandType.Text, null );
                if ( reader != null )
                {
                    string version = "";
                    string versionInfo = "";
                    
                    while ( reader.Read() )
                    {
                        version = reader[0].ToString();
                        versionInfo = reader[1].ToString();
                    }

                    databaseResults.Append( string.Format( "<br />Database Version: {0}", versionInfo ) );
                }

                try
                {
                    // get database size
                    reader = DbService.GetDataReader( "sp_helpdb '" + catalog.ToStringSafe().Replace("'", "''") + "'", System.Data.CommandType.Text, null );
                    if ( reader != null )
                    {
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

                                databaseResults.AppendFormat( "<br />Database Size: {0}", sizeInMB );
                            }
                        }
                        else
                        {
                            databaseResults.Append( "<br />Database Size: Unlimited" );
                        }
                    }
                }
                catch
                {
                    databaseResults.AppendFormat( "<br />Database Size: unable to determine" );
                }

                try
                {
                    // get database snapshot isolation details
                    reader = DbService.GetDataReader( string.Format( "SELECT [snapshot_isolation_state], [is_read_committed_snapshot_on] FROM sys.databases WHERE [name] = '{0}'", _catalog ), System.Data.CommandType.Text, null );
                    if ( reader != null )
                    {
                        bool isAllowSnapshotIsolation = false;
                        bool isReadCommittedSnapshopOn = true;

                        while ( reader.Read() )
                        {
                            isAllowSnapshotIsolation = reader[0].ToStringSafe().AsBoolean();
                            isReadCommittedSnapshopOn = reader[1].ToString().AsBoolean();
                        }

                        databaseResults.AppendFormat( "<br />Allow Snapshot Isolation: {0}<br />Is Read Committed Snapshot On: {1}<br />", isAllowSnapshotIsolation.ToYesNo(), isReadCommittedSnapshopOn.ToYesNo() );
                    }
                }
                catch { }
            }
            catch ( Exception ex )
            {
                databaseResults.AppendFormat( "Unable to read database system information: {0}", ex.Message );
            }

            return databaseResults.ToString();
        }

        // method from Rick Strahl http://weblog.west-wind.com/posts/2006/Oct/08/Recycling-an-ASPNET-Application-from-within
        private bool RestartWebApplication()
        {
            bool Error = false;
            try
            {
                // *** This requires full trust so this will fail
                // *** in many scenarios
                HttpRuntime.UnloadAppDomain();
            }
            catch
            {
                Error = true;
            }

            if ( !Error )
                return true;

            // *** Couldn't unload with Runtime - let's try modifying web.config
            string ConfigPath = HttpContext.Current.Request.PhysicalApplicationPath + "\\web.config";

            try
            {
                File.SetLastWriteTimeUtc( ConfigPath, DateTime.UtcNow );
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ResponseWrite( string key, string value, HttpResponse response )
        {
            response.Write( string.Format( "{0}: {1}{2}", key, value, System.Environment.NewLine ) );
        }

        #endregion
    }
}
