using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Web.Cache
{
    public class RockMemoryCache : MemoryCache
    {
        private static object s_initLock;
        private static RockMemoryCache s_defaultCache;
        private static Int64 s_totalSize = 0;

        static RockMemoryCache()
        {
            RockMemoryCache.s_initLock = new object();
        }

        public RockMemoryCache() : base("RockDefault", null)
        {
        }

        public static Int64 TotalSize
        {
            get { return s_totalSize; }
        }

        public static new RockMemoryCache Default
        {
            get
            {
                if (RockMemoryCache.s_defaultCache == null)
                {
                    lock( RockMemoryCache.s_initLock)
                    {
                        if (RockMemoryCache.s_defaultCache == null)
                        {
                            RockMemoryCache.s_defaultCache = new RockMemoryCache();
                        }
                    }
                }
                return RockMemoryCache.s_defaultCache;
            }
        }

        public override void Set( CacheItem item, CacheItemPolicy policy )
        {
            base.Set( item, policy );
            LogItem( item.Key, item.Value );
        }

        public override void Set( string key, object value, CacheItemPolicy policy, string regionName = null )
        {
            base.Set( key, value, policy, regionName );
            LogItem( key, value );
        }

        public override void Set( string key, object value, DateTimeOffset absoluteExpiration, string regionName = null )
        {
            base.Set( key, value, absoluteExpiration, regionName );
            LogItem( key, value );
        }

        private void LogItem( string key, object value )
        {
            //if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            //{
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                StringBuilder sb = new StringBuilder();
                int size = 0;

                try
                {
                    using ( var memStream = new MemoryStream() )
                    {
                        binaryFormatter.Serialize( memStream, value );
                        size = memStream.ToArray().Length;
                        memStream.Close();
                    }

                    s_totalSize += size;
                    System.Diagnostics.Debug.WriteLine( string.Format( "{0} added {1:N0} bytes to Cache. Total: {2:N0} bytes.", key, size, s_totalSize ) );

                }
                catch { }
            //}
        }
    }
}
