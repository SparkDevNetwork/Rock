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
using Rock.Lava;
using Rock.Web.Utilities;

namespace Rock.Utility.Settings
{
    /// <summary>
    /// Provides configuration information for a Rock instance.
    /// </summary>
    public class RockInstanceConfigurationService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockInstanceConfigurationService"/> class.
        /// </summary>
        public RockInstanceConfigurationService()
        {
            this.Database = new RockInstanceDatabaseConfiguration();
        }

        /// <summary>
        /// Returns the date and time of the application host server.
        /// </summary>
        public DateTime SystemDateTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Returns the date and time of the application host server, localised according to the timezone specified in the Rock application settings.
        /// </summary>
        public DateTime RockDateTime
        {
            get
            {
                return Rock.RockDateTime.Now;
            }
        }

        /// <summary>
        /// Returns the install path of the Rock application on the host.
        /// </summary>
        public string PhysicalDirectory
        {
            get
            {
                // Returns the path in which the current instance is hosted, or the path of the executing assembly if we are not running in a hosting environment.
                var path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath
                           ?? AppDomain.CurrentDomain.BaseDirectory;

                return path;
            }
        }

        /// <summary>
        /// Returns the virtual path of the Rock application on the host.
        /// </summary>
        public string ApplicationDirectory
        {
            get
            {
                // Refer: https://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path
                var path = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
                            ?? AppDomain.CurrentDomain.BaseDirectory;

                return path;
            }
        }

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <value>
        /// The name of the machine.
        /// </value>
        public string MachineName
        {
            get
            {
                return System.Environment.MachineName;
            }
        }

        /// <summary>
        /// Gets the ASP net version.
        /// </summary>
        /// <value>
        /// The ASP net version.
        /// </value>
        public string AspNetVersion
        {
            get
            {
                return RockUpdateHelper.GetDotNetVersion();
            }
        }

        /// <summary>
        /// Returns the database properties of the Rock application.
        /// </summary>
        public RockInstanceDatabaseConfiguration Database { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is clustered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is clustered; otherwise, <c>false</c>.
        /// </value>
        public bool IsClustered
        {
            get
            {
                return Rock.Web.SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER ).AsBooleanOrNull() ?? false;
            }
        }

        /// <summary>
        /// Gets the name of the rendering engine that is currently used to render Lava templates.
        /// </summary>
        public string LavaEngineName
        {
            get
            {
                var engine = LavaService.GetCurrentEngine();

                if ( engine == null )
                {
                    return "DotLiquid";
                }
                else
                {
                    var engineName = engine.EngineName;

                    if ( LavaService.RockLiquidIsEnabled )
                    {
                        engineName = $"DotLiquid (with {engineName} verification)";
                    }

                    return engineName;
                }
            }
        }
    }
}
