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
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Rock.Lava;
using Rock.Lava.Fluid;

namespace Rock.Tests.Lava.Shared
{
    /// <summary>
    /// Builds Lava engine instances configured for testing.
    /// </summary>
    /// <remarks>
    /// The component discovery here is the same for a unit test and an integration
    /// test. The one difference is dynamic shortcodes, whose definitions are rows
    /// in the LavaShortcode table, so only a caller with a database supplies
    /// <c>configureEngine</c> to register them.
    /// </remarks>
    public static class LavaTestEngineFactory
    {
        #region Fields

        private static bool _engineFactoryRegistered;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The culture a test engine formats its output with, unless the caller
        /// supplies its own configuration.
        /// </summary>
        public static CultureInfo EngineCulture { get; } = CultureInfo.GetCultureInfo( "en-US" );

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a Fluid engine with Rock's filters, tags, blocks and static
        /// shortcodes registered.
        /// </summary>
        /// <param name="engineOptions">
        /// Engine configuration. Defaults apply per setting rather than all or
        /// nothing, so a caller that supplies one setting keeps the test defaults
        /// for the rest: the en-US engine culture, a website host service and the
        /// RenderToOutput exception handling strategy. The object passed in is not
        /// modified.
        /// </param>
        /// <param name="configureEngine">An optional callback to register additional components, such as dynamic shortcodes.</param>
        /// <returns>The configured engine.</returns>
        public static ILavaEngine CreateFluidEngine( LavaEngineConfigurationOptions engineOptions = null, Action<ILavaEngine> configureEngine = null )
        {
            /*
                9/26/26 - CLAUDE

                Code that asks LavaService for an engine rather than receiving one -
                the merge template types are an example - resolves it through a
                factory the service has to be told about. The predecessor helper
                registered that factory as a side effect of its own initialization,
                so building an engine here has to do the same or those callers fail
                with "the service type FluidEngine is not registered".

                Reason: Keep LavaService able to resolve an engine for code that
                does not take one as a parameter.
            */
            RegisterEngineFactory();

            var options = ApplyDefaultOptions( engineOptions );

            var engine = new FluidEngine();

            engine.Initialize( options );

            RegisterFilters( engine );
            RegisterTags( engine );
            RegisterBlocks( engine );
            RegisterStaticShortcodes( engine );

            configureEngine?.Invoke( engine );

            return engine;
        }

        /// <summary>
        /// Teaches <see cref="LavaService"/> how to build a configured engine, for
        /// callers that resolve one from the service rather than receiving it.
        /// </summary>
        public static void RegisterEngineFactory()
        {
            if ( _engineFactoryRegistered )
            {
                return;
            }

            _engineFactoryRegistered = true;

            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engine = new FluidEngine();

                engine.Initialize( ApplyDefaultOptions( options as LavaEngineConfigurationOptions ) );

                RegisterFilters( engine );
                RegisterTags( engine );
                RegisterBlocks( engine );
                RegisterStaticShortcodes( engine );

                return engine;
            } );
        }

        /// <summary>
        /// Returns a copy of the supplied options with the test defaults filled in
        /// for every setting the caller left unset.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The defaults apply per setting rather than as a single all-or-nothing
        /// object. A caller that supplies options usually cares about one of them -
        /// a file system, a cache service, a set of default commands - and has no
        /// reason to also give up the host service or the exception handling
        /// strategy that every other test engine has.
        /// </para>
        /// <para>
        /// The culture is pinned rather than read from the calling thread. The
        /// engine writes numbers and dates using the culture it was initialized
        /// with, and Initialize falls back to the culture of the thread that built
        /// it. In Rock that is the culture the application started under, which
        /// does not change when a request arrives for a visitor in another culture.
        /// Without pinning, a test that runs its body under a client culture - the
        /// SetCulture tests do - would build its engine under that culture too and
        /// get output formatted for it, which is not what happens in Rock. Pinning
        /// also takes the developer's own regional settings out of the result. A
        /// test that is about the engine's culture passes its own.
        /// </para>
        /// <para>
        /// A copy is returned because the options object belongs to the caller, and
        /// a caller that builds two engines from one object should get the same
        /// engine twice rather than have the first call decide the second.
        /// </para>
        /// </remarks>
        /// <param name="engineOptions">The caller's options; may be null.</param>
        /// <returns>The options to initialize an engine with.</returns>
        private static LavaEngineConfigurationOptions ApplyDefaultOptions( LavaEngineConfigurationOptions engineOptions )
        {
            engineOptions = engineOptions ?? new LavaEngineConfigurationOptions();

            return new LavaEngineConfigurationOptions
            {
                Culture = engineOptions.Culture ?? EngineCulture,
                TimeZone = engineOptions.TimeZone,
                CacheService = engineOptions.CacheService,
                FileSystem = engineOptions.FileSystem,
                HostService = engineOptions.HostService ?? new WebsiteLavaHost(),
                DefaultEnabledCommands = engineOptions.DefaultEnabledCommands,
                ExceptionHandlingStrategy = engineOptions.ExceptionHandlingStrategy ?? ExceptionHandlingStrategySpecifier.RenderToOutput,
                InitializeDynamicShortcodes = engineOptions.InitializeDynamicShortcodes
            };
        }

        /// <summary>
        /// Registers the Lava filters. The common filters are registered first so
        /// that the web-specific implementations overwrite them.
        /// </summary>
        /// <param name="engine">The engine to register against.</param>
        private static void RegisterFilters( ILavaEngine engine )
        {
            engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( LavaFilters ) );
        }

        /// <summary>
        /// Registers every Lava tag found in the loaded assemblies.
        /// </summary>
        /// <param name="engine">The engine to register against.</param>
        private static void RegisterTags( ILavaEngine engine )
        {
            RegisterElements<ILavaTag>( engine,
                "Tag",
                null,
                ( name, tagType ) => engine.RegisterTag( name, ( tagName ) => Activator.CreateInstance( tagType ) as ILavaTag ) );
        }

        /// <summary>
        /// Registers every Lava block found in the loaded assemblies. Shortcodes
        /// also implement <see cref="ILavaBlock"/> but are registered separately,
        /// so they are excluded here.
        /// </summary>
        /// <param name="engine">The engine to register against.</param>
        private static void RegisterBlocks( ILavaEngine engine )
        {
            RegisterElements<ILavaBlock>( engine,
                "Block",
                types => types.Where( t => !typeof( ILavaShortcode ).IsAssignableFrom( t ) ),
                ( name, blockType ) => engine.RegisterBlock( name, ( blockName ) => Activator.CreateInstance( blockType ) as ILavaBlock ) );
        }

        /// <summary>
        /// Registers the shortcodes that are implemented as classes. Shortcodes
        /// defined as LavaShortcode rows are not registered here.
        /// </summary>
        /// <param name="engine">The engine to register against.</param>
        private static void RegisterStaticShortcodes( ILavaEngine engine )
        {
            RegisterElements<ILavaShortcode>( engine,
                "Shortcode",
                null,
                ( name, shortcodeType ) => engine.RegisterShortcode( name, ( shortcodeName ) => Activator.CreateInstance( shortcodeType ) as ILavaShortcode ) );
        }

        /// <summary>
        /// Finds every Lava element of the given kind in the loaded assemblies and
        /// registers it against the engine.
        /// </summary>
        /// <remarks>
        /// A type is registered only once an instance of it has been constructed
        /// here, so the factory the engine is given is known to work by the time
        /// the engine calls it.
        /// </remarks>
        /// <typeparam name="TElement">The element interface to search for.</typeparam>
        /// <param name="engine">The engine to register against.</param>
        /// <param name="elementKind">The kind of element, used in failure messages.</param>
        /// <param name="filterTypes">Narrows the discovered types; may be null.</param>
        /// <param name="registerElement">Registers one element, by name and type, against the engine.</param>
        private static void RegisterElements<TElement>( ILavaEngine engine, string elementKind, Func<IEnumerable<Type>, IEnumerable<Type>> filterTypes, Action<string, Type> registerElement )
            where TElement : class, IRockLavaElement
        {
            var elementTypes = Rock.Reflection.FindTypes( typeof( TElement ) ).Select( a => a.Value ).AsEnumerable();

            if ( filterTypes != null )
            {
                elementTypes = filterTypes( elementTypes );
            }

            foreach ( var elementType in elementTypes.ToList() )
            {
                var instance = CreateElement<TElement>( elementKind, elementType );

                if ( instance == null )
                {
                    continue;
                }

                var name = instance.SourceElementName.IsNullOrWhiteSpace() ? elementType.Name : instance.SourceElementName;

                registerElement( name, elementType );

                StartComponent( instance, engine, elementKind, elementType );
            }
        }

        /// <summary>
        /// Constructs a Lava element, reporting rather than propagating a failure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 9/27/26 - CLAUDE
        /// </para>
        /// <para>
        /// The predecessor helper caught failures around this loop and the tests
        /// depend on that. Type discovery returns every non-abstract type that
        /// implements the interface, from plugin assemblies as well as Rock's own,
        /// and nothing guarantees such a type has a public parameterless
        /// constructor. Because every test now builds its own engine, an
        /// unhandled failure here would fail the whole Lava suite rather than
        /// leave one element unavailable.
        /// </para>
        /// <para>
        /// Reason: One uninstantiable element must not take the engine down.
        /// </para>
        /// </remarks>
        /// <typeparam name="TElement">The element interface being constructed.</typeparam>
        /// <param name="elementKind">The kind of element, used in the failure message.</param>
        /// <param name="elementType">The type to construct.</param>
        /// <returns>The instance, or null when it could not be constructed.</returns>
        private static TElement CreateElement<TElement>( string elementKind, Type elementType )
            where TElement : class, IRockLavaElement
        {
            TElement instance = null;

            try
            {
                instance = Activator.CreateInstance( elementType ) as TElement;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( $"Lava component initialization failure. Construction failed for Lava {elementKind} \"{elementType.FullName}\". {ex.Message}" );

                return null;
            }

            if ( instance == null )
            {
                Debug.WriteLine( $"Lava component initialization failure. Lava {elementKind} \"{elementType.FullName}\" is not a {typeof( TElement ).Name}." );
            }

            return instance;
        }

        /// <summary>
        /// Runs a component's startup routine, reporting rather than propagating a
        /// failure.
        /// </summary>
        /// <remarks>
        /// A component whose OnStartup reaches for something the current test
        /// environment does not provide must not take the whole engine down with
        /// it, because the tests that do not use that component are still valid.
        /// The failure is written to the debug output rather than the Rock
        /// exception log, which would itself require a database.
        /// </remarks>
        /// <param name="component">The component being started.</param>
        /// <param name="engine">The engine the component is registered against.</param>
        /// <param name="componentKind">The kind of component, used in the failure message.</param>
        /// <param name="componentType">The component's type, used in the failure message.</param>
        private static void StartComponent( IRockLavaElement component, ILavaEngine engine, string componentKind, Type componentType )
        {
            try
            {
                component.OnStartup( engine );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( $"Lava component initialization failure. Startup failed for Lava {componentKind} \"{componentType.FullName}\". {ex.Message}" );
            }
        }

        #endregion Methods
    }
}
