using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Rock.Helpers
{
    public abstract class HttpModule : IHttpModule
    {
        #region Static privates

        private static bool applicationStarted = false;
        private static object applicationStartLock = new object();

        #endregion

        #region IHttpModule implementation

        public virtual void Dispose()
        {
        }

        public virtual void Init( HttpApplication context )
        {
            if ( !applicationStarted )
            {
                lock ( applicationStartLock )
                {
                    if ( !applicationStarted )
                    {
                        this.Application_Start( context );
                        applicationStarted = true;
                    }
                }
            }
        }

        #endregion

        public virtual void Application_Start( HttpApplication context )
        {
        }
    }
}
