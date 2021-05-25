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
using System.Collections.Generic;
using System.Reflection;
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;

namespace Rock.Lava
{
    /// <summary>
    /// Provides access to core functions for Rock Lava using the global instance of the Lava engine.
    /// </summary>
    public static class LavaService
    {
        // A suffix that is added to shortcode elements to avoid naming collisions with other tags and blocks.
        // Note that a suffix is used because the closing tag of a Liquid language element requires the "end" prefix.
        // Also, the suffix must match a regular expression word character, either A to Z or "_" to be compatible with the DotLiquid engine parser.
        public static string ShortcodeInternalNameSuffix = "_";

        private static ILavaEngine _engine = null;
        private static object _initializationLock = new object();
        private static bool _rockLiquidIsEnabled = true;

        /// <summary>
        /// A flag indicating if RockLiquid Lava processing is enabled.
        /// RockLiquid is the Rock-specific fork of the DotLiquid framework that provides Lava rendering for Rock v12 or below.
        /// This flag is used to implement custom Lava processing in the Rock codebase that is only compatible with the implementation of Lava prior to Rock v13.
        /// If RockLiquid is enabled, it is the primary Lava renderer. If a Lava engine is also initialized, it will operate in background verification mode.
        /// </summary>
        public static bool RockLiquidIsEnabled
        {
            get
            {
                return _rockLiquidIsEnabled;
            }
            set
            {
                _rockLiquidIsEnabled = value;

                if ( _rockLiquidIsEnabled )
                {
                    ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;
                }
            }

        }

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
                _engine = null;

                if ( engineType != null )
                {
                    var engine = NewEngineInstance( engineType.GetValueOrDefault( LavaEngineTypeSpecifier.Fluid ), options );

                    // Assign the current instance.
                    _engine = engine;

                    // If RockLiquid is enabled, set the Lava Engine to throw exceptions so they can be logged.
                    if ( RockLiquidIsEnabled )
                    {
                        ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;
                    }
                }
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
                // Instantiate the default engine.
                engine = new RockLiquidEngine();

                options = options ?? new LavaEngineConfigurationOptions();
            }

            engine.Initialize( options );

            return engine;
        }

        /// <summary>
        /// Gets the global instance of the Lava Engine.
        /// </summary>
        public static ILavaEngine GetCurrentEngine()
        {
            return _engine;
        }

        /// <summary>
        /// Sets the global instance of the Lava Engine.
        /// </summary>
        public static void SetCurrentEngine( ILavaEngine engine )
        {
            /// Set the global instance of the Lava Engine.
            /// Used for internal test purposes.
            lock ( _initializationLock )
            {
                _engine = engine;

                // Reset the exception strategy to ensure that it is aligned with the RockLiquidIsEnabled setting.
                ExceptionHandlingStrategy = _engine.ExceptionHandlingStrategy;
            }
        }

        /// <summary>
        /// Remove all items from the template cache.
        /// </summary>
        public static void ClearTemplateCache()
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.ClearTemplateCache();
        }

        /// <summary>
        /// Set configuration options for the current Lava engine.
        /// </summary>
        /// <param name="options"></param>
        public static void Initialize( LavaEngineConfigurationOptions options = null )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.Initialize( options );
        }

        /// <summary>
        /// Creates a new Lava render context instance.
        /// </summary>
        /// <returns></returns>
        public static ILavaRenderContext NewRenderContext()
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.NewRenderContext();
        }

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public static ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.NewRenderContext( enabledCommands );
        }

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public static ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.NewRenderContext( mergeFields, enabledCommands );
        }

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public static ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.NewRenderContext( mergeFields, enabledCommands );
        }

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        public static ILavaRenderContext NewRenderContext( LavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.NewRenderContext( mergeFields, enabledCommands );
        }

        /// <summary>
        /// Register one or more filter functions that are implemented by the supplied Type.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="implementingType"></param>
        public static void RegisterFilters( Type implementingType )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterFilters( implementingType );
        }

        /// <summary>
        /// Register a filter function.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="filterMethod"></param>
        /// <param name="filterName"></param>
        public static void RegisterFilter( MethodInfo filterMethod, string filterName = null )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterFilter( filterMethod, filterName );
        }

        /// <summary>
        /// Register a Lava Tag elemennt.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterTag( name, factoryMethod );
        }

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterBlock( name, factoryMethod );
        }

        /// <summary>
        /// Registers a shortcode definition that can be used to create new instances of a shortcode during the rendering process.
        /// </summary>
        /// <param name="shortcodeDefinition"></param>
        public static void RegisterShortcode( DynamicShortcodeDefinition shortcodeDefinition )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterShortcode( shortcodeDefinition );
        }

        /// <summary>
        /// Registers a shortcode with a factory method that provides the definition of the shortcode on demand.
        /// The supplied definition is used to create a new DynamicShortcode instance to render the shortcode.
        /// This method of registration is suitable for shortcodes that can be modified at runtime, such as user-defined shortcodes stored in a Rock database.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterShortcode( name, factoryMethod );
        }

        /// <summary>
        /// Registers a shortcode with a factory method that provides a new instance of the shortcode.
        /// This method of registration is suitable for shortcodes that are defined by a code component and cannot be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterShortcode( string name, Func<string, ILavaShortcode> factoryMethod )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterShortcode( name, factoryMethod );
        }

        /// <summary>
        /// Remove the registration entry for a Tag with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public static void DeregisterTag( string name )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.DeregisterTag( name );
        }

        /// <summary>
        /// Remove the registration entry for a Block with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public static void DeregisterBlock( string name )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.DeregisterBlock( name );
        }

        /// <summary>
        /// Remove the registration entry for a Shortcode with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public static void DeregisterShortcode( string name )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.DeregisterShortcode( name );
        }

        /// <summary>
        /// Gets the collection of all registered Lava document elements.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ILavaElementInfo> GetRegisteredElements()
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.GetRegisteredElements();
        }

        /// <summary>
        /// Parse the provided text into a compiled Lava template object. The resulting template can be used to render output with a variety of render contexts.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>A compiled template object.</returns>
        public static LavaParseResult ParseTemplate( string inputTemplate )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.ParseTemplate( inputTemplate );
        }

        /// <summary>
        /// Render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public static LavaRenderResult RenderTemplate( string inputTemplate )
        {
            return RenderTemplate( inputTemplate, LavaRenderParameters.Default() );
        }

        /// <summary>
        /// Render the provided template in a new context with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields">The collection of merge fields to be added to the context used to render the template.</param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public static LavaRenderResult RenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields )
        {
            var result = RenderTemplate( inputTemplate, LavaRenderParameters.WithContext( _engine.NewRenderContext( mergeFields ) ) );

            return result;
        }

        /// <summary>
        /// Render the provided template in a new context with the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters">The settings applied to the rendering process.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        public static LavaRenderResult RenderTemplate( string inputTemplate, LavaRenderParameters parameters )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.RenderTemplate( inputTemplate, parameters );
        }

        /// <summary>
        /// Render the provided template in a new context with the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters">The settings applied to the rendering process.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        public static LavaRenderResult RenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.RenderTemplate( inputTemplate, parameters );
        }

        /// <summary>
        /// Render the provided template in a new context with the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters">The settings applied to the rendering process.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        public static LavaRenderResult RenderTemplate( ILavaTemplate inputTemplate, ILavaRenderContext context )
        {
            if ( _engine == null )
            {
                return null;
            }

            return _engine.RenderTemplate( inputTemplate, context );
        }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// The [LavaVisible] and [LavaHidden] custom attributes can be applied to determine the visibility of individual properties.
        /// If these attributes are not applied to any members of the type, all members are visible by default.
        /// </remarks>
        public static void RegisterSafeType( Type type )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterSafeType( type );
        }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers">
        /// The names of the properties that are visible to the Lava renderer.
        /// Specifying this parameter overrides the effect of any [LavaVisible] and [LavaHidden] custom attributes applied to the type.
        /// </param>
        public static void RegisterSafeType( Type type, IEnumerable<string> allowedMembers )
        {
            if ( _engine == null )
            {
                return;
            }

            _engine.RegisterSafeType( type, allowedMembers );
        }

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        public static bool AreEqualValue( object left, object right )
        {
            if ( _engine == null )
            {
                return false;
            }

            return _engine.AreEqualValue( left, right );
        }

        /// <summary>
        /// Gets the component that implements template caching for the Lava Engine.
        /// </summary>
        public static ILavaTemplateCacheService TemplateCacheService
        {
            get
            {
                return _engine?.TemplateCacheService;
            }
        }

        /// <summary>
        /// The descriptive name of the Liquid framework on which Lava is currently operating.
        /// </summary>
        public static string CurrentEngineName
        {
            get
            {
                return _engine?.EngineName;
            }
        }

        /// <summary>
        /// The Liquid framework currently used to parse and render Lava templates.
        /// </summary>
        public static LavaEngineTypeSpecifier? CurrentEngineType
        {
            get
            {
                return _engine?.EngineType;
            }
        }

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        public static ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy
        {
            get
            {
                return ( _engine == null ? ExceptionHandlingStrategySpecifier.Ignore : _engine.ExceptionHandlingStrategy );
            }
            set
            {
                if ( _engine == null )
                {
                    return;
                }

                // If RockLiquid is enabled, set the Lava Engine to throw exceptions so they can be logged.
                if ( RockLiquidIsEnabled )
                {
                    _engine.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;

                    return;
                }

                _engine.ExceptionHandlingStrategy = value;
            }
        }
    }
}