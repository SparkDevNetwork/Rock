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
using System.Linq;

using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// The default Lava Engine factory implementation. This creates an instance
    /// of the Lava Engine currently recommended for use in Rock.
    /// </summary>
    internal class LavaEngineFactory : ILavaEngineFactory
    {
        /// <inheritdoc/>
        public ILavaEngine CreateEngine( LavaEngineConfigurationOptions options )
        {
            var fluidEngine = new FluidEngine();

            InitializeLavaEngineInstance( fluidEngine, options );

            return fluidEngine;
        }

        /// <summary>
        /// Creates and returns a default configuration for the Lava Engine.
        /// This will be used if no specific configuration is provided when
        /// creating a new Lava Engine instance.
        /// </summary>
        /// <returns>An instance of <see cref="LavaEngineConfigurationOptions"/> that describe how the engine should be configured.</returns>
        private static LavaEngineConfigurationOptions GetDefaultEngineConfiguration()
        {
            var defaultEnabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues( "," ).ToList();

            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = new WebsiteLavaFileSystem(),
                HostService = new WebsiteLavaHost(),
                CacheService = new WebsiteLavaTemplateCacheService(),
                DefaultEnabledCommands = defaultEnabledLavaCommands,
                InitializeDynamicShortcodes = true
            };

            return engineOptions;
        }

        /// <summary>
        /// Initialize a specific Lava Engine instance.
        /// </summary>
        /// <param name="engine">The engine to be initialized.</param>
        /// <param name="options">The options that describe how to initialize the engine.</param>
        private static void InitializeLavaEngineInstance( ILavaEngine engine, LavaEngineConfigurationOptions options )
        {
            options = options ?? GetDefaultEngineConfiguration();

            InitializeLavaFilters( engine );
            InitializeLavaTags( engine );
            InitializeLavaBlocks( engine );

            if ( options.InitializeDynamicShortcodes )
            {
                InitializeLavaShortcodes( engine );
            }

            InitializeLavaSafeTypes( engine );

            engine.Initialize( options );
        }

        /// <summary>
        /// Initialize and register the default Lava filters for the engine.
        /// </summary>
        /// <param name="engine">The engine to register the filers for.</param>
        private static void InitializeLavaFilters( ILavaEngine engine )
        {
            // Register the common Rock.Lava filters first, then overwrite with the engine-specific filters.
            engine.RegisterFilters( typeof( Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( Rock.Lava.LavaFilters ) );
        }

        /// <summary>
        /// Initialize and register the default Lava shortcodes for the engine.
        /// </summary>
        /// <param name="engine">The engine to register the shortcodes for.</param>
        private static void InitializeLavaShortcodes( ILavaEngine engine )
        {
            // Register shortcodes defined in the codebase.
            try
            {
                var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Select( a => a.Value ).ToList();

                foreach ( var shortcodeType in shortcodeTypes )
                {
                    // Create an instance of the shortcode to get the registration name.
                    var instance = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = shortcodeType.Name;
                    }

                    // Register the shortcode with a factory method to create a new instance of the shortcode from the System.Type defined in the codebase.
                    engine.RegisterShortcode( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                        return shortcode;
                    } );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            // Register shortcodes defined in the current database.
            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                // Register the shortcode with the current Lava Engine.
                // The provider is responsible for retrieving the shortcode definition from the data store and managing the web-based shortcode cache.
                WebsiteLavaShortcodeProvider.RegisterShortcode( engine, shortcode.TagName );
            }
        }

        /// <summary>
        /// Initialize and register the custom Lava tags for the engine.
        /// </summary>
        /// <param name="engine">The engine to register the tags for.</param>
        private static void InitializeLavaTags( ILavaEngine engine )
        {
            // Get all tags and call OnStartup methods
            try
            {
                var elementTypes = Rock.Reflection.FindTypes( typeof( ILavaTag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as ILavaTag;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterTag( name, ( tagName ) =>
                    {
                        var tag = Activator.CreateInstance( elementType ) as ILavaTag;
                        return tag;
                    } );

                    try
                    {
                        instance.OnStartup( engine );
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Initialize and register the custom Lava blocks for the engine.
        /// </summary>
        /// <param name="engine">The engine to register the custom blocks for.</param>
        private static void InitializeLavaBlocks( ILavaEngine engine )
        {
            // Get all blocks and call OnStartup methods
            try
            {
                var blockTypes = Rock.Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

                foreach ( var blockType in blockTypes )
                {
                    var blockInstance = Activator.CreateInstance( blockType ) as ILavaBlock;

                    engine.RegisterBlock( blockInstance.SourceElementName, ( blockName ) =>
                    {
                        return Activator.CreateInstance( blockType ) as ILavaBlock;
                    } );

                    try
                    {
                        blockInstance.OnStartup( engine );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, null );
                    }

                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Initializes the lava safe types on the engine. This takes care
        /// of special types that we don't have direct access to so we can't
        /// add the proper interfaces to them.
        /// </summary>
        /// <param name="engine">The engine.</param>
        private static void InitializeLavaSafeTypes( ILavaEngine engine )
        {
            engine.RegisterSafeType( typeof( Common.Mobile.DeviceData ) );
            engine.RegisterSafeType( typeof( Utility.RockColor ) );
            engine.RegisterSafeType( typeof( Utilities.ColorPair ) );
        }
    }
}
