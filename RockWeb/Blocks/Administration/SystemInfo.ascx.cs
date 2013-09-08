//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Caching;
using System.Reflection;

using Rock.Web.UI.Controls;
using Rock.VersionInfo;

namespace RockWeb.Blocks.Administration
{
    public partial class SystemInfo : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lRockVersion.Text = VersionInfo.GetRockProductVersion();
            lExecLocation.Text = Assembly.GetExecutingAssembly().Location;

            var cache = MemoryCache.Default;

            StringBuilder sbItems = new StringBuilder();
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

            lCacheOverview.Text = sb.ToString();
            lCacheObjects.Text = sbItems.ToString();

            var routes = new SortedDictionary<string, System.Web.Routing.Route>();
            foreach ( System.Web.Routing.Route route in System.Web.Routing.RouteTable.Routes )
                if ( ! routes.ContainsKey( route.Url ) ) routes.Add( route.Url, route );

            sb = new StringBuilder();
            foreach(var routeItem in routes)
                sb.AppendFormat("{0}<br/>", routeItem.Key);
            lRoutes.Text = sb.ToString();
        }


        /// <summary>
        /// Used to manually flush the attribute cache.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearCache_Click( object sender, EventArgs e )
        {
            foreach ( var attribute in new Rock.Model.AttributeService().Queryable().ToList() )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
            }

            foreach ( var blockType in new Rock.Model.BlockTypeService().Queryable().ToList() )
            {
                Rock.Web.Cache.BlockTypeCache.Flush( blockType.Id );
            }

            foreach ( var block in new Rock.Model.BlockService().Queryable().ToList() )
            {
                Rock.Web.Cache.BlockCache.Flush( block.Id );
            }

            foreach ( var page in new Rock.Model.PageService().Queryable().ToList() )
            {
                Rock.Web.Cache.PageCache.Flush( page.Id );
            }
        }
    }
}