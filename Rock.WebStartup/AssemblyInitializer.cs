// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
        /// 
        /// </summary>
        /// <seealso cref="System.Exception" />
        internal class RockAssemblyInitializerException : Exception
        {
            public RockAssemblyInitializerException( string message, Exception innerException )
                : base( message, innerException )
            {
            }
        }

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
                    try
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "Application_Initialize: {0}", RockDateTime.Now.ToString( "hh:mm:ss.FFF" ) ) );
                        new Rock.Model.AttributeService( new Data.RockContext() ).GetSelect( 0, a => a.Id );
                        System.Diagnostics.Debug.WriteLine( string.Format( "ConnectToDatabase - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                        stopwatch.Restart();
                    }
                    catch
                    {
                        // ignore
                    }
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
            catch ( Exception ex )
            {
                // incase something bad happens when access the database, like a problem with a migration. We will just log this,
                var rockAssemblyInitializerException = new RockAssemblyInitializerException( $"Exception in AssemblyInitializer.Initialize {ex.Message}. It is OK to ignore this because it is probably due to pending migrations.", ex );
                System.Diagnostics.Debug.WriteLine( rockAssemblyInitializerException.Message );
                Rock.Model.ExceptionLogService.LogException( rockAssemblyInitializerException );
            }
        }
    }
}
