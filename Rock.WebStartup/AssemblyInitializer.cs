using System;
using System.Web;

namespace Rock
{
    /// <summary>
    /// Initializer for the project and also used by Rock for all startup logic relating to the web project.
    /// </summary>
    public static class AssemblyInitializer
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            try
            {

                // Step 1: Load registered HTTP Modules - http://blog.davidebbo.com/2011/02/register-your-http-modules-at-runtime.html
                var activeHttpModules = Rock.Web.HttpModules.HttpModuleContainer.GetActiveComponents(); // takes 8 seconds :(

                foreach ( var httpModule in activeHttpModules )
                {
                    HttpApplication.RegisterModule( httpModule.GetType() );
                }

            }
            catch ( Exception ) { } // incase something bad happens when access the database, like a problem with a migration
        }
    }
}
