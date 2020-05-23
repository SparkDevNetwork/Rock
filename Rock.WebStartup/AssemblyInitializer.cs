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
using Rock.WebStartup;

namespace Rock
{
    /// <summary>
    /// Initializer that runs prior to RockWeb's Global.Application_Start. (see comments on PreApplicationStartMethod in AssemblyInfo.cs)
    /// This calls <seealso cref="RockApplicationStartupHelper.RunApplicationStartup"/> to take care of most for all startup logic relating to the web project.
    /// </summary>
    public static class AssemblyInitializer
    {
        /// <summary>
        /// Contains any Exception that occurred during <see cref="AssemblyInitializer.Initialize"/>
        /// </summary>
        /// <value>
        /// The assembly initializer exception.
        /// </value>
        public static Exception AssemblyInitializerException { get; private set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            if ( System.Web.Hosting.HostingEnvironment.InClientBuildManager == true )
            {
                // AssemblyInitializer is pre-loaded before RockWeb starts,
                // but it is also loaded when just loading the Rock solution or building RockWeb!
                // see https://stackoverflow.com/questions/13642691/avoid-aspnet-compiler-running-preapplicationstart-method/13689600#13689600

                // We don't want RockApplicationStartupHelper.RunApplicationStartup to run in the situation, but 
                // we can detect that with System.Web.Hosting.HostingEnvironment.InClientBuildManager

                // so just exit

                RockApplicationStartupHelper.LogStartupMessage( "AssemblyInitializer started by Visual Studio" );
                return;
            }

            RockApplicationStartupHelper.LogStartupMessage( "AssemblyInitializer started by RockWeb" );

            try
            {
                RockApplicationStartupHelper.RunApplicationStartup();
            }
            catch ( RockStartupException rockStartupException )
            {
                AssemblyInitializerException = rockStartupException;
            }
            catch ( Exception ex )
            {
                AssemblyInitializerException = new RockStartupException( "Error occurred in RunApplicationStartup", ex );
            }
        }
    }
}
