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
            lExecLocation.Text = Assembly.GetExecutingAssembly().Location;

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

        private string GetCacheInfo()
        {
            var cache = Rock.Web.Cache.RockMemoryCache.Default;

            //StringBuilder sbItems = new StringBuilder();
            Dictionary<string, int> cacheSize = new Dictionary<string, int>();
            int totalSize = 0;

            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            foreach ( KeyValuePair<string, object> cachItem in cache.AsQueryable().ToList() )
            {
                //int size = 0;

                //try
                //{
                //    using ( var memStream = new MemoryStream() )
                //    {
                //        binaryFormatter.Serialize( memStream, cachItem.Value );
                //        size = memStream.ToArray().Length;
                //        memStream.Close();
                //    }

                //    sbItems.AppendFormat( "<p>{0} ({1:N0} bytes)</p>", cachItem.Key, size );
                //    totalSize += size;

                //}
                //catch (SystemException ex)
                //{
                //    sbItems.AppendFormat( "<p>{0} (Could Not Determine Size: {1})</p>", cachItem.Key, ex.Message );
                //}
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<p><strong>Cache Items:</strong><br /> {0}</p>", cache.Count() );
            sb.AppendFormat( "<p><strong>Approximate Current Size:</strong><br /> {0:N0} (bytes)</p>", totalSize );
            sb.AppendFormat( "<p><strong>Cache Memory Limit:</strong><br /> {0:N0} (bytes)</p>", cache.CacheMemoryLimit );
            sb.AppendFormat( "<p><strong>Physical Memory Limit:</strong><br /> {0} %</p>", cache.PhysicalMemoryLimit );
            sb.AppendFormat( "<p><strong>Polling Interval:</strong><br /> {0}</p>", cache.PollingInterval );

            //Type type = cache.GetType();

            //FieldInfo[] fields = type.GetFields( BindingFlags.NonPublic | BindingFlags.Instance );
            //foreach(var field in fields)
            //    if ( field.Name == "_stats" )
            //    {
            //        object statObj = field.GetValue(cache);
            //        Type statType = statObj.GetType();
            //        foreach ( var statField in statType.GetFields( BindingFlags.NonPublic | BindingFlags.Instance ) )
            //        {
            //            if ( statField.Name == "_cacheMemoryMonitor" ||
            //                statField.Name == "_physicalMemoryMonitor")
            //            {
            //                object monitorObj = statField.GetValue( statObj );
            //                Type monitorType = monitorObj.GetType();
            //                foreach ( var monitorField in monitorType.GetFields( BindingFlags.NonPublic | BindingFlags.Instance ) )
            //                {
            //                    if ( monitorField.Name == "_sizedRef" )
            //                    {
            //                        object sizeObj = monitorField.GetValue( monitorObj );
            //                        Type sizeType = sizeObj.GetType();
            //                        foreach ( var sizeField in sizeType.GetProperties( BindingFlags.NonPublic | BindingFlags.Instance ) )
            //                        {
            //                            sb.AppendFormat( "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}: {1}<br/>", sizeField.Name, sizeField.GetValue( sizeObj, null ) );
            //                        }
            //                    }
            //                    else
            //                    {
            //                        sb.AppendFormat( "&nbsp;&nbsp;&nbsp;&nbsp;{0}: {1}<br/>", monitorField.Name, monitorField.GetValue( monitorObj ) );
            //                    }
            //                }

            //                int currentPressure = (int)monitorType.InvokeMember( "GetCurrentPressure", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, monitorObj, null );
            //                sb.AppendFormat( "&nbsp;&nbsp;&nbsp;&nbsp;Current Pressure: {0} %<br/>", currentPressure );
            //            }
            //            else
            //            {
            //                sb.AppendFormat( "{0}: {1}<br/>", statField.Name, statField.GetValue( statObj ) );
            //            }
            //        }
            //    }
            //lCacheObjects.Text = sbItems.ToString();

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