//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web;

namespace Rock.Web
    
    /// <summary>
    /// Provides application start, and module initialization and disposal events to the implementing class.
    /// </summary>
    public abstract class HttpModule : IHttpModule
        
        #region Static privates

        private static bool applicationStarted = false;
        private static object applicationStartLock = new object();

        #endregion

        #region IHttpModule implementation

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public virtual void Dispose()
            
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public virtual void Init( HttpApplication context )
            
            if ( !applicationStarted )
                
                lock ( applicationStartLock )
                    
                    if ( !applicationStarted )
                        
                        this.Application_Start( context );
                        applicationStarted = true;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Method that will be called once on application start.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Application_Start( HttpApplication context )
            
        }
    }
}
