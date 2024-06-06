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

using Rock.Attribute;
using Rock.Lava;

namespace Rock.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="RockApp"/>.
    /// </summary>
    public static class RockAppExtensions
    {
        /// <summary>
        /// Reads the initialization settings that are configured to be used
        /// the next time Rock starts. Changes can be persisted by calling
        /// <see cref="InitializationSettings.Save()"/> on the object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal method</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public classes. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <returns>An instance of <see cref="InitializationSettings"/> that can be modified.</returns>
        [RockInternal( "1.16.6" )]
        public static InitializationSettings ReadConfiguredInitializationSettings( this RockApp app )
        {
            // Use DI to get the actual object instance.
            return ( InitializationSettings ) app.GetService( typeof( InitializationSettings ) );
        }

        /// <summary>
        /// Gets the database configuration being used by this Rock instance.
        /// </summary>
        /// <returns>An instance of <see cref="IDatabaseConfiguration"/> that describes the database.</returns>
        [RockInternal( "1.16.6" )]
        public static IDatabaseConfiguration GetDatabaseConfiguration( this RockApp app )
        {
            return ( IDatabaseConfiguration ) app.GetService( typeof ( IDatabaseConfiguration ) );
        }

        /// <summary>
        /// Get the name of the current Lava engine that is being used to
        /// process Lava. If this is called before Rock has finished starting
        /// up it will always return <c>DotLiquid</c>.
        /// </summary>
        /// <param name="app">The RockApp for which to retrieve the Lava engine name.</param>
        /// <returns>The current Lava engine name.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "This method is meant to be used as the central point to get the engine name. Even though it doesn't use the RockApp itself now, it might in the future." )]
        public static string GetCurrentLavaEngineName( this RockApp app )
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

        /// <summary>
        /// Checks if the database is available for the Rock application instance.
        /// </summary>
        /// <param name="app">The RockApp instance to inspect.</param>
        /// <returns><c>true</c> if the database is available for the specified RockApp; otherwise, <c>false</c>.</returns>
        public static bool IsDatabaseAvailable( this RockApp app )
        {
            return app?.GetDatabaseConfiguration().IsDatabaseAvailable ?? false;
        }
    }
}
