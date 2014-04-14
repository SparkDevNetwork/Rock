using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Rock.Web
{
    /// <summary>
    /// Allows Static HTML Resources to have a Fingerprint so that we can set long cache expiration dates
    /// see http://madskristensen.net/post/cache-busting-in-aspnet
    /// </summary>
    public class Fingerprint
    {
        /// <summary>
        /// Tags the specified root relative path.
        /// </summary>
        /// <param name="rootRelativePath">The root relative path.</param>
        /// <returns></returns>
        public static string Tag( string rootRelativePath )
        {
            if ( HttpRuntime.Cache[rootRelativePath] == null )
            {
                string absolute = HostingEnvironment.MapPath( "~" + rootRelativePath );
                DateTime date = File.GetLastWriteTime( absolute );

                string result = rootRelativePath + "?v=" + date.Ticks;
                HttpRuntime.Cache.Insert( rootRelativePath, result, new CacheDependency( absolute ) );
            }

            return HttpRuntime.Cache[rootRelativePath] as string;
        }
    }

}
