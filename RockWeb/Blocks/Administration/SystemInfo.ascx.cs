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

namespace RockWeb.Blocks.Administration
{
    public partial class SystemInfo : Rock.Web.UI.Block
	{
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var cache = MemoryCache.Default;

            StringBuilder sbItems = new StringBuilder();
            Dictionary<string, int> cacheSize = new Dictionary<string, int>();
            int totalSize = 0;

            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            foreach ( KeyValuePair<string, object> cachItem in cache.AsQueryable().ToList() )
            {
                int size = 0;

                try
                {
                    using ( var memStream = new MemoryStream() )
                    {
                        binaryFormatter.Serialize( memStream, cachItem.Value );
                        size = memStream.ToArray().Length;
                        memStream.Close();
                    }

                    sbItems.AppendFormat( "{0} ({1:N0} bytes)<br/>", cachItem.Key, size );
                    totalSize += size;

                }
                catch (SystemException ex)
                {
                    sbItems.AppendFormat( "{0} (Could Not Determine Size: {1})<br/>", cachItem.Key, ex.Message );
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "Cache Items: {0}<br/>", cache.Count() );
            sb.AppendFormat( "Approximate Current Size: {0:N0} (bytes)<br/><br/>", totalSize );
            sb.AppendFormat( "Cache Memory Limit: {0:N0} (bytes)<br/>", cache.CacheMemoryLimit );
            sb.AppendFormat( "Physical Memory Limit: {0} %<br/>", cache.PhysicalMemoryLimit );
            sb.AppendFormat( "Polling Interval: {0}<br/>", cache.PollingInterval );

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

        }

    }
}