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
            lServerCulture.Text = System.Globalization.CultureInfo.CurrentCulture.ToString();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearCache_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            foreach ( int id in new Rock.Model.AttributeService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.AttributeCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.BlockTypeService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.BlockTypeCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.BlockService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.BlockCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.PageService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.PageCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.DefinedTypeService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.DefinedTypeCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.DefinedValueService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.DefinedValueCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.GroupTypeService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.GroupTypeCache.Flush( id );
            }

            Guid securityRoleGuid = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid();
            foreach ( int id in new Rock.Model.GroupService( rockContext ).Queryable()
                .Where( g =>
                    g.IsSecurityRole ||
                    g.GroupType.Guid.Equals( securityRoleGuid ) )
                .Select( a => a.Id ) )
            {
                Rock.Security.Role.Flush( id );
            }

            foreach ( int id in new Rock.Model.CampusService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.CampusCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.CategoryService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.CategoryCache.Flush( id );
            }

            foreach ( int id in new Rock.Model.LayoutService( rockContext ).Queryable().Select( a => a.Id ) )
            {
                Rock.Web.Cache.LayoutCache.Flush( id );
            }

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
            ResponseWrite( "Database:", lDatabase.Text, response );
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
            var cache = MemoryCache.Default;

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
            sb.AppendFormat( "<p>Cache Items: {0}</p>", cache.Count() );
            sb.AppendFormat( "<p>Approximate Current Size: {0:N0} (bytes)</p>", totalSize );
            sb.AppendFormat( "<p>Cache Memory Limit: {0:N0} (bytes)</p>", cache.CacheMemoryLimit );
            sb.AppendFormat( "<p>Physical Memory Limit: {0} %</p>", cache.PhysicalMemoryLimit );
            sb.AppendFormat( "<p>Polling Interval: {0}</p>", cache.PollingInterval );

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
            var csBuilder = new System.Data.Odbc.OdbcConnectionStringBuilder( ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString );
            object dataSource, catalog = string.Empty;
            if ( csBuilder.TryGetValue( "data source", out dataSource ) && csBuilder.TryGetValue( "initial catalog", out catalog ) )
            {
                return string.Format( "{0} @ {1}", catalog, dataSource );
            }
            return string.Empty;
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