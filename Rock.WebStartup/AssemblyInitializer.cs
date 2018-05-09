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
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Application_Initialize: {0}", RockDateTime.Now.ToString( "hh:mm:ss.FFF" ) ) );
                    new Rock.Model.AttributeService( new Data.RockContext() ).Get( 0 );
                    System.Diagnostics.Debug.WriteLine( string.Format( "ConnectToDatabase - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                    stopwatch.Restart();
                }

                // Step 1: Load registered HTTP Modules - http://blog.davidebbo.com/2011/02/register-your-http-modules-at-runtime.html
                var activeHttpModules = Rock.Web.HttpModules.HttpModuleContainer.GetActiveComponents(); // takes 8 seconds :(

                foreach ( var httpModule in activeHttpModules )
                {
                    HttpApplication.RegisterModule( httpModule.GetType() );
                }

                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "WebStartup.Initialize - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                }
            }
            catch ( Exception ) { } // incase something bad happens when access the database, like a problem with a migration
        }
    }
}
