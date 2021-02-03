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
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;

namespace Rock.Lava
{
    /// <summary>
    /// Provides access to core functions for the Rock Lava Engine.
    /// </summary>
    public static class LavaEngine
    {
        // A suffix that is added to shortcode elements to avoid naming collisions with other tags and blocks.
        // Note that a suffix is used because the closing tag of a Liquid language element requires the "end" prefix.
        // Also, the suffix must match a regular expression word character, either A to Z or "_" to be compatible with the DotLiquid engine parser.
        public static string ShortcodeInternalNameSuffix = "_";

        private static ILavaEngine _instance = null;
        private static LavaEngineTypeSpecifier _liquidFramework = LavaEngineTypeSpecifier.DotLiquid;
        private static object _initializationLock = new object();

        /// <summary>
        /// Initialize the Lava Engine with the specified configuration options.
        /// </summary>
        /// <param name="engineType"></param>
        /// <param name="options"></param>
        public static void Initialize( LavaEngineTypeSpecifier? engineType, LavaEngineConfigurationOptions options )
        {
            lock ( _initializationLock )
            {
                _liquidFramework = engineType ?? LavaEngineTypeSpecifier.DotLiquid;

                ILavaEngine engine;

                if ( options == null )
                {
                    options = new LavaEngineConfigurationOptions();
                }

                if ( _liquidFramework == LavaEngineTypeSpecifier.Fluid )
                {
                    engine = new FluidEngine();

                    options.FileSystem = new FluidFileSystem( options.FileSystem );
                }
                else
                {
                    engine = new DotLiquidEngine();

                    options.FileSystem = new DotLiquidFileSystem( options.FileSystem );
                }

                engine.Initialize( options );

                _instance = engine;
            }
        }

        /// <summary>
        /// Returns the current instance of the Lava Engine.
        /// </summary>
        public static ILavaEngine CurrentEngine
        {
            get
            {
                lock ( _initializationLock )
                {
                    if ( _instance == null )
                    {
                        // Initialize a default instance.
                        Initialize( _liquidFramework, new LavaEngineConfigurationOptions() );
                    }
                }

                return _instance;
            }
        }
    }
}