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
using Rock.Lava.RockLiquid;

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
        private static object _initializationLock = new object();

        /// <summary>
        /// Initialize the global instance of the Lava Engine with the specified configuration options.
        /// </summary>
        /// <param name="engineType"></param>
        /// <param name="options"></param>
        public static void Initialize( LavaEngineTypeSpecifier? engineType, LavaEngineConfigurationOptions options )
        {
            lock ( _initializationLock )
            {
                // Release the current instance.
                _instance = null;

                var engine = NewEngineInstance( engineType.GetValueOrDefault( LavaEngineTypeSpecifier.RockLiquid ), options );

                // Assign the current instance.
                _instance = engine;
            }
        }

        /// <summary>
        /// Create a new Lava Engine instance with the specified configuration options.
        /// </summary>
        /// <param name="engineType"></param>
        /// <param name="options"></param>
        public static ILavaEngine NewEngineInstance( LavaEngineTypeSpecifier engineType, LavaEngineConfigurationOptions options )
        {
            ILavaEngine engine = null;

            if ( engineType == LavaEngineTypeSpecifier.Fluid )
            {
                engine = new FluidEngine();

                options = options ?? new LavaEngineConfigurationOptions();

                if ( options.FileSystem != null )
                {
                    options.FileSystem = new FluidFileSystem( options.FileSystem );
                }
            }
            else if ( engineType == LavaEngineTypeSpecifier.DotLiquid )
            {
                engine = new DotLiquidEngine();

                options = options ?? new LavaEngineConfigurationOptions();

                if ( options.FileSystem != null )
                {
                    options.FileSystem = new DotLiquidFileSystem( options.FileSystem );
                }
            }
            else if ( engineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                engine = new RockLiquidEngine();

                options = options ?? new LavaEngineConfigurationOptions();
            }

            engine.Initialize( options );

            return engine;
        }

        /// <summary>
        /// Returns the global instance of the Lava Engine.
        /// </summary>
        public static ILavaEngine CurrentEngine
        {
            get
            {
                lock ( _initializationLock )
                {
                    if ( _instance == null )
                    {
                        // Make sure that the global engine instance has been intentionally initialized before it is first accessed.
                        // This provides more certainty for the order of events in the Rock application startup process.
                        throw new LavaException( "LavaEngine not initialized. The Initialize() method must be called before the engine instance can be accessed." );
                    }
                }

                return _instance;
            }
            set
            {
                /// Set the global instance of the Lava Engine.
                /// Used for internal test purposes.
                lock ( _initializationLock )
                {
                    _instance = value;
                }
            }

        }
    }
}