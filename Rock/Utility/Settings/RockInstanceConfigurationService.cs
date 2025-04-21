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
    [Obsolete( "RockApp should be used for access Rock instance data." )]
    [RockObsolete( "1.16.6" )]
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
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
        public DateTime SystemDateTime
        {
            get
            {
                /* This property intentionally returns the system date of the local server.  This property should
                 * be used whenever it is necessary to use the local server clock instead of RockDateTime.Now. */
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Returns the date and time of the application host server, localised according to the timezone specified in the Rock application settings.
        /// </summary>
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
        public DateTime RockDateTime
        {
            get
            {
                // RockDateTime represents Rock organization time rather than a local system time, so set the Kind to reflect this and avoid confusion.
                var rockDateTime = DateTime.SpecifyKind( Rock.RockDateTime.Now, DateTimeKind.Unspecified );

                return rockDateTime;
            }
        }

        /// <summary>
        /// Returns the date and time of the application host server, localised according to the timezone specified in the Rock application settings.
        /// </summary>
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
        public DateTimeOffset RockDateTimeOffset
        {
            get
            {
                var dtoRock = new DateTimeOffset( DateTime.UtcNow );

                dtoRock = dtoRock.ToOffset( Rock.RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dtoRock ) );

                return dtoRock;
            }
        }

        /// <summary>
        /// Returns the install path of the Rock application on the host.
        /// </summary>
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
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
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
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
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
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
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
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
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
        public RockInstanceDatabaseConfiguration Database { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is clustered using RockWebFarm.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is clustered; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
        public bool IsClustered
        {
            get
            {
                if ( Rock.WebFarm.RockWebFarm.IsEnabled() )
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the name of the rendering engine that is currently used to render Lava templates.
        /// </summary>
        [Obsolete( "RockApp should be used for access Rock instance data." )]
        [RockObsolete( "1.16.6" )]
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
