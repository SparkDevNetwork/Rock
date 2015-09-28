// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.VersionInfo;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "System Information" )]
    [Category( "Administration" )]
    [Description( "Displays system information on the installed version of Rock." )]
    public partial class SystemInfo : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Get Version, database info and executing assembly location
            lRockVersion.Text = VersionInfo.GetRockProductVersionFullName();
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

            lExecLocation.Text = Assembly.GetExecutingAssembly().Location;
            lLastMigrations.Text = GetLastMigrationData();

            lCacheOverview.Text = GetCacheInfo();
            lRoutes.Text = GetRoutesInfo();

            // register btnDumpDiagnostics as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( btnDumpDiagnostics );
        }

        #endregion

        #region Events

        /// <summary>
        /// Used to manually flush the attribute cache.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearCache_Click( object sender, EventArgs e )
        {
            // Clear all cached items
            Rock.Web.Cache.RockMemoryCache.Clear();

            // Clear the static object that contains all auth rules (so that it will be refreshed)
            Rock.Security.Authorization.Flush();

            // Flush the static entity attributes cache
            Rock.Web.Cache.AttributeCache.FlushEntityAttributes();

            // Check for any unregistered entity types, field types, and block types
            string webAppPath = Server.MapPath("~");
            EntityTypeService.RegisterEntityTypes( webAppPath );
            FieldTypeService.RegisterFieldTypes( webAppPath );
            BlockTypeService.RegisterBlockTypes( webAppPath, Page, false );

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
            }
            catch( Exception ex )
            {
                nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessage.Visible = true;
                nbMessage.Text = "Cache has been cleared, but following error occurred when attempting to delete cached files: " + ex.Message;
                return;
            }

            nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Success;
            nbMessage.Visible = true;
            nbMessage.Text = "The cache has been cleared.";
        }

        protected void btnRestart_Click( object sender, EventArgs e )
        {
            RestartWebApplication();
        }

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
            ResponseWrite( "Database:", lDatabase.Text.Replace("<br />", Environment.NewLine.ToString()), response );
            ResponseWrite( "Execution Location:", lExecLocation.Text, response );
            ResponseWrite( "Cache:", lCacheOverview.Text, response ); ;
            ResponseWrite( "Routes:", lRoutes.Text, response );

            foreach ( string key in Request.ServerVariables )
            {
                ResponseWrite( key, Request.ServerVariables[key], response );
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
                sb.AppendFormat( "Last Core Migration: {0}", (string)result );
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
                sb.AppendFormat( "</table" );
            }

            return sb.ToString();
        }

        private string GetCacheInfo()
        {
            var cache = Rock.Web.Cache.RockMemoryCache.Default;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<p><strong>Cache Items:</strong><br /> {0}</p>", cache.Count() );
            sb.AppendFormat( "<p><strong>Cache Memory Limit:</strong><br /> {0:N0} (bytes)</p>", cache.CacheMemoryLimit );
            sb.AppendFormat( "<p><strong>Physical Memory Limit:</strong><br /> {0} %</p>", cache.PhysicalMemoryLimit );
            sb.AppendFormat( "<p><strong>Polling Interval:</strong><br /> {0}</p>", cache.PollingInterval );
            lCacheObjects.Text = cache.GroupBy( a => a.Value.GetType() ).Select( a => new
            {
                a.Key.Name,
                Count = a.Count()
            } ).OrderBy( a => a.Name ).Select( a => string.Format( "{0}: {1} items", a.Name, a.Count ) ).ToList().AsDelimited( "</br>" );
            

            return sb.ToString();
        }

        private string GetRoutesInfo()
        {
            var routes = new SortedDictionary<string, System.Web.Routing.Route>();
            foreach ( System.Web.Routing.Route route in System.Web.Routing.RouteTable.Routes )
            {
                if ( !routes.ContainsKey( route.Url ) ) routes.Add( route.Url, route );
            }

            StringBuilder sb = new StringBuilder();
            foreach ( var routeItem in routes )
            {
                sb.AppendFormat( "{0}<br/>", routeItem.Key );
            }

            return sb.ToString();
        }

        private string GetDbInfo()
        {
            StringBuilder databaseResults = new StringBuilder();
            
            var csBuilder = new System.Data.Odbc.OdbcConnectionStringBuilder( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
            object dataSource, catalog = string.Empty;
            if ( csBuilder.TryGetValue( "data source", out dataSource ) && csBuilder.TryGetValue( "initial catalog", out catalog ) )
            {
                databaseResults.Append( string.Format( "Name: {0} <br /> Server: {1}", catalog, dataSource ));
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
                
                // get database size
                reader = DbService.GetDataReader( "sp_helpdb " + catalog, System.Data.CommandType.Text, null );
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

                            databaseResults.Append( string.Format("<br />Database Size: {0}", sizeInMB) );
                        }
                    }
                    else
                    {
                        databaseResults.Append( "<br />Database Size: Unlimited" );
                    }
                }
            }
            catch {}

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