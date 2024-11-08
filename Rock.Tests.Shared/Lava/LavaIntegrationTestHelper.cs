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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.Lava
{
    public class LavaIntegrationTestHelper
    {
        private static LavaIntegrationTestHelper _instance = null;
        public static LavaIntegrationTestHelper CurrentInstance
        {
            get
            {
                if ( _instance == null )
                {
                    throw new Exception( "Helper not configured. Call the Initialize() method to initialize this helper before use." );
                }

                return _instance;
            }

        }
        public static bool FluidEngineIsEnabled { get; set; }
        public static bool DotLiquidEngineIsEnabled { get; set; }
        public static bool RockLiquidEngineIsEnabled { get; set; }

        private static ILavaEngine _rockliquidEngine = null;
        private static ILavaEngine _dotliquidEngine = null;
        private static ILavaEngine _fluidEngine = null;

        public static void Initialize( bool testRockLiquidEngine, bool testDotLiquidEngine, bool testFluidEngine, bool loadShortcodes )
        {
            // Verify the test environment: RockLiquidEngine and DotLiquidEngine are mutually exclusive test environments.
            if ( testRockLiquidEngine && testDotLiquidEngine )
            {
                throw new Exception( "RockLiquidEngine/DotLiquidEngine cannot be tested simultaneously because they require different global configurations of the DotLiquid library." );
            }

            RockLiquidEngineIsEnabled = testRockLiquidEngine;
            DotLiquidEngineIsEnabled = testDotLiquidEngine;
            FluidEngineIsEnabled = testFluidEngine;

            RegisterLavaEngineFactories();

            var engineOptions = new LavaEngineConfigurationOptions();

            engineOptions.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;
            engineOptions.FileSystem = new MockFileProvider();
            engineOptions.CacheService = new NullTemplateCacheService();
            engineOptions.InitializeDynamicShortcodes = loadShortcodes;
            engineOptions.HostService = new WebsiteLavaHost();

            if ( RockLiquidEngineIsEnabled )
            {
                // Initialize the Rock variant of the DotLiquid Engine.
                _rockliquidEngine = global::Rock.Lava.LavaService.NewEngineInstance( typeof( RockLiquidEngine ), engineOptions );
            }

            if ( DotLiquidEngineIsEnabled )
            {
                // Initialize the Lava library DotLiquid Engine.
                _dotliquidEngine = global::Rock.Lava.LavaService.NewEngineInstance( typeof( DotLiquidEngine ), engineOptions );
            }

            if ( FluidEngineIsEnabled )
            {
                // Initialize the Fluid Engine.
                _fluidEngine = global::Rock.Lava.LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );
            }

            _instance = new LavaIntegrationTestHelper();
        }

        /// <summary>
        /// Register factory functions that can create instances of the various Lava Engines on demand.
        /// </summary>
        private static void RegisterLavaEngineFactories()
        {
            // Register the RockLiquid Engine (pre-v13).
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engine = new RockLiquidEngine();
                var engineOptions = options as LavaEngineConfigurationOptions;

                engine.Initialize( engineOptions );

                // Initialize the RockLiquid Engine
                RegisterFilters( engine );
                RegisterTags( engine );
                RegisterBlocks( engine );

                RegisterStaticShortcodes( engine );

                if ( engineOptions?.InitializeDynamicShortcodes ?? false )
                {
                    RegisterDynamicShortcodes( engine );
                }

                return engine;
            } );

            // Register the DotLiquid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engine = new DotLiquidEngine();
                var engineOptions = options as LavaEngineConfigurationOptions;

                engine.Initialize( engineOptions );

                // Initialize the DotLiquid Engine
                RegisterFilters( engine );
                RegisterTags( engine );
                RegisterBlocks( engine );

                RegisterStaticShortcodes( engine );

                if ( engineOptions?.InitializeDynamicShortcodes ?? false )
                {
                    RegisterDynamicShortcodes( engine );
                }

                return engine;
            } );

            // Register the Fluid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engine = new FluidEngine();
                var engineOptions = options as LavaEngineConfigurationOptions;

                engine.Initialize( engineOptions );

                // Initialize Fluid Engine
                RegisterFilters( engine );
                RegisterTags( engine );
                RegisterBlocks( engine );

                RegisterStaticShortcodes( engine );

                if ( engineOptions?.InitializeDynamicShortcodes ?? false )
                {
                    RegisterDynamicShortcodes( engine );
                }

                return engine;
            } );
        }

        /// <summary>
        /// Gets a new Lava Engine instance of the specified type.
        /// </summary>
        /// <param name="engineType"></param>
        /// <param name="engineOptions"></param>
        /// <returns></returns>
        public static ILavaEngine NewEngineInstance( Type engineType, LavaEngineConfigurationOptions engineOptions )
        {
            engineOptions = engineOptions ?? new LavaEngineConfigurationOptions();

            var engine = global::Rock.Lava.LavaService.NewEngineInstance( engineType, engineOptions );
            return engine;
        }

        /// <summary>
        /// Gets an existing Lava Engine instance of the specified type and sets it as the current engine.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetEngineInstance<T>()
            where T : class, ILavaEngine
        {
            var engine = GetEngineInstance( typeof( T ) ) as T;
            return engine;
        }

        /// <summary>
        /// Gets an existing Lava Engine instance of the specified type and sets it as the current engine.
        /// </summary>
        /// <param name="engineType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ILavaEngine GetEngineInstance( Type engineType )
        {
            ILavaEngine engine = null;

            if ( engineType == typeof( DotLiquidEngine ) )
            {
                engine = _dotliquidEngine;
            }
            else if ( engineType == typeof( FluidEngine ) )
            {
                engine = _fluidEngine;
            }
            else if ( engineType == typeof( RockLiquidEngine ) )
            {
                engine = _rockliquidEngine;
            }

            if ( engine == null )
            {
                throw new Exception( $"Lava Engine instance not available. Engine Type \"{engineType}\" is not configured for this test run." );
            }

            // Set the global instance of the engine to ensure that it is available to Lava components.
            LavaService.SetCurrentEngine( engine );

            return engine;
        }

        private static void RegisterFilters( ILavaEngine engine )
        {
            // Register the common Rock.Lava filters first, then overwrite with the web-specific filters.
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                engine.RegisterFilters( typeof( Rock.Lava.RockFilters ) );
            }
            else
            {
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                engine.RegisterFilters( typeof( Rock.Lava.LavaFilters ) );
            }
        }

        private static void RegisterTags( ILavaEngine engine )
        {
            // Get all tags and call OnStartup methods
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                // Find all tag elements that implement IRockStartup.
                var elementTypes = Rock.Reflection.FindTypes( typeof( DotLiquid.Tag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockStartup;

                    if ( instance == null )
                    {
                        continue;
                    }

                    try
                    {
                        // RockLiquid blocks register themselves with the DotLiquid framework during their startup process.
                        instance.OnStartup();
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }

            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                return;
            }

            // Get all Lava tags and call the OnStartup method.
            try
            {
                List<Type> elementTypes;

                elementTypes = Rock.Reflection.FindTypes( typeof( ILavaTag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as ILavaTag;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterTag( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( elementType ) as ILavaTag;

                        return shortcode;
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

        private static void RegisterBlocks( ILavaEngine engine )
        {
            // Get all blocks and call OnStartup methods
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                // Find all tag elements that implement IRockStartup.
                var elementTypes = Rock.Reflection.FindTypes( typeof( DotLiquid.Block ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockStartup;

                    if ( instance == null )
                    {
                        continue;
                    }

                    try
                    {
                        // RockLiquid blocks register themselves with the DotLiquid framework during their startup process.
                        instance.OnStartup();
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }
            else
            {
                try
                {
                    // Get Lava block components, except shortcodes which are registered separately.
                    var elementTypes = Rock.Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

                    elementTypes = elementTypes.Where( x => !( typeof( ILavaShortcode ).IsAssignableFrom( x ) ) ).ToList();

                    foreach ( var elementType in elementTypes )
                    {
                        var instance = Activator.CreateInstance( elementType ) as ILavaBlock;

                        var name = instance.SourceElementName;

                        if ( string.IsNullOrWhiteSpace( name ) )
                        {
                            name = elementType.Name;
                        }

                        engine.RegisterBlock( name, ( shortcodeName ) =>
                        {
                            var shortcode = Activator.CreateInstance( elementType ) as ILavaBlock;

                            return shortcode;
                        } );

                        try
                        {
                            instance.OnStartup( engine );
                        }
                        catch ( Exception ex )
                        {
                            var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Block \"{0}\".", elementType.FullName ), ex );

                            ExceptionLogService.LogException( lavaException, null );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                }
            }
        }

        private static void RegisterStaticShortcodes( ILavaEngine engine )
        {
            // Get all shortcodes and call OnStartup methods
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
        }

        private static void RegisterDynamicShortcodes( ILavaEngine engine )
        {
            // Register dynamic shortcodes with a factory method to ensure that the latest definition is retrieved from the global cache each time the shortcode is used.
            Func<string, DynamicShortcodeDefinition> shortCodeFactory = ( shortcodeName ) =>
            {
                var shortcodeDefinition = LavaShortcodeCache.All().Where( c => c.TagName == shortcodeName ).FirstOrDefault();

                if ( shortcodeDefinition == null )
                {
                    return null;
                }

                var newShortcode = new DynamicShortcodeDefinition();

                newShortcode.Name = shortcodeDefinition.Name;
                newShortcode.TemplateMarkup = shortcodeDefinition.Markup;

                var parameters = RockSerializableDictionary.FromUriEncodedString( shortcodeDefinition.Parameters );

                newShortcode.Parameters = new Dictionary<string, string>( parameters.Dictionary );

                newShortcode.EnabledLavaCommands = shortcodeDefinition.EnabledLavaCommands.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList();

                if ( shortcodeDefinition.TagType == TagType.Block )
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Block;
                }
                else
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Inline;
                }

                return newShortcode;
            };

            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                engine.RegisterShortcode( shortcode.TagName, shortCodeFactory );
            }
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( Type engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            var engine = GetEngineInstance( engineType );

            return GetTemplateOutput( engine, inputTemplate, mergeFields );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( ILavaEngine engine, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var result = engine.RenderTemplate( inputTemplate.Trim(), mergeFields );

            return result.Text;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( Type engineType, string inputTemplate, ILavaRenderContext context )
        {
            var engine = GetEngineInstance( engineType );

            return GetTemplateOutput( engine, inputTemplate, context );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( ILavaEngine engine, string inputTemplate, ILavaRenderContext context )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var result = engine.RenderTemplate( inputTemplate.Trim(), LavaRenderParameters.WithContext( context ) );

            return result.Text;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( Type engineType, string inputTemplate, LavaTestRenderOptions options )
        {
            var engine = GetEngineInstance( engineType );

            return GetTemplateOutput( engine, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( ILavaEngine engine, string inputTemplate, LavaTestRenderOptions options )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var context = engine.NewRenderContext();

            var parameters = LavaRenderParameters.WithContext( context );

            if ( options != null )
            {
                context.SetEnabledCommands( options.EnabledCommands, options.EnabledCommandsDelimiter );
                context.SetMergeFields( options.MergeFields );

                parameters.ExceptionHandlingStrategy = options.ExceptionHandlingStrategy;
            }

            var result = engine.RenderTemplate( inputTemplate.Trim(), parameters );

            return result.Text;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public LavaRenderResult GetTemplateRenderResult( Type engineType, string inputTemplate, LavaRenderParameters parameters = null, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            return GetTemplateRenderResult( engine, inputTemplate, parameters, options );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public LavaRenderResult GetTemplateRenderResult( ILavaEngine engine, string inputTemplate, LavaRenderParameters parameters = null, LavaTestRenderOptions options = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var context = engine.NewRenderContext();

            if ( parameters == null )
            {
                parameters = LavaRenderParameters.WithContext( context );
            }

            // If options are specified, replace the render parameters.
            if ( options != null )
            {
                context.SetEnabledCommands( options.EnabledCommands, options.EnabledCommandsDelimiter );
                context.SetMergeFields( options.MergeFields );

                if ( options.ExceptionHandlingStrategy != null )
                {
                    parameters.ExceptionHandlingStrategy = options.ExceptionHandlingStrategy;
                }
            }

            var result = engine.RenderTemplate( inputTemplate.Trim(), parameters );

            return result;
        }

        /// <summary>
        /// Execute an action and return the elapsed time.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <returns></returns>
        public TimeSpan ExecuteAndGetElapsedTime( Action testMethod )
        {
            var stopwatch = Stopwatch.StartNew();

            testMethod();

            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        public void ExecuteForActiveEngines( Action<ILavaEngine> testMethod )
        {
            var engines = GetActiveTestEngineTypes();
            ExecuteForEngines( engines, testMethod );
        }

        /// <summary>
        /// For each of the specified Lava Engines, process the specified action.
        /// </summary>
        /// <param name="engines"></param>
        /// <param name="testMethod"></param>
        public void ExecuteForEngines( List<Type> engines, Action<ILavaEngine> testMethod )
        {
            // Add a handler to catch unobserved exceptions.
            var exceptionHandler = new EventHandler<UnobservedTaskExceptionEventArgs>( TaskScheduler_UnobservedTaskException );
            TaskScheduler.UnobservedTaskException += exceptionHandler;

            try
            {
                var exceptions = new List<Exception>();
                var failedEngines = new List<string>();
                foreach ( var engineType in engines )
                {
                    var engine = GetEngineInstance( engineType );
                    LavaService.SetCurrentEngine( engine );

                    Debug.Print( $"\n**\n** Lava Render Test: {engine.EngineName}\n**\n" );

                    try
                    {
                        testMethod( engine );
                    }
                    catch ( Exception ex )
                    {
                        failedEngines.Add( engine.EngineName );

                        // Write the error to debug output.
                        Debug.Print( $"\n** ERROR: {engine.EngineName}\n** {ex.ToString()}" );

                        exceptions.Add( ex );
                    }
                }

                if ( exceptions.Any() )
                {
                    throw new AggregateException( $"Test failed for {failedEngines.AsDelimited( "/" )}.", exceptions );
                }
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= exceptionHandler;
            }
        }

        private static void TaskScheduler_UnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
        {
            var ex = e.Exception.Flatten();
            Debug.WriteLine( $"**\n** Unhandled Exception:\n{ex}" );
            // Prevent this exception from terminating the test execution.
            e.SetObserved();
        }

        private static List<ILavaEngine> _activeEngines = null;
        private static object _activeEnginesLock = new Object();

        private List<ILavaEngine> GetActiveTestEngines()
        {
            lock ( _activeEnginesLock )
            {
                if ( _activeEngines == null )
                {
                    _activeEngines = new List<ILavaEngine>();

                    // Test the Fluid engine first, because it is the most likely to fail!
                    if ( FluidEngineIsEnabled )
                    {
                        _activeEngines.Add( _fluidEngine );
                    }

                    if ( DotLiquidEngineIsEnabled )
                    {
                        _activeEngines.Add( _dotliquidEngine );
                    }

                    if ( RockLiquidEngineIsEnabled )
                    {
                        _activeEngines.Add( _rockliquidEngine );
                    }
                }
            }

            return _activeEngines;
        }

        private List<Type> GetActiveTestEngineTypes()
        {
            var engineTypes = GetActiveTestEngines().Select( e => e.GetType() ).ToList();
            return engineTypes;
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var requirement = new LavaTestOutputMatchRequirement( expectedOutput, options?.OutputMatchType ?? LavaTestOutputMatchTypeSpecifier.Equal );

            AssertTemplateOutput( new List<LavaTestOutputMatchRequirement> { requirement },
                inputTemplate,
                options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( Type engineType, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            AssertTemplateOutput( engine, expectedOutput, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( IEnumerable<string> expectedOutputs, string inputTemplate, LavaTestRenderOptions options = null )
        {
            ExecuteForActiveEngines( ( engine ) =>
            {
                AssertTemplateOutput( engine, expectedOutputs, inputTemplate, options );
            } );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( Type engineType, IEnumerable<string> expectedOutputs, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            AssertTemplateOutput( engine, expectedOutputs, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            AssertTemplateOutput( engine, new List<string> { expectedOutput }, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, IEnumerable<string> expectedOutputs, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var requirements = new List<LavaTestOutputMatchRequirement>();
            foreach ( var expectedOutput in expectedOutputs )
            {
                var requirement = new LavaTestOutputMatchRequirement
                {
                    MatchValue = expectedOutput,
                    MatchType = options?.OutputMatchType ?? LavaTestOutputMatchTypeSpecifier.Equal
                };
                requirements.Add( requirement );
            }

            AssertTemplateOutput( engine, requirements, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( IEnumerable<LavaTestOutputMatchRequirement> matchRequirements, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engines = options?.LavaEngineTypes ?? new List<Type>();
            if ( !engines.Any() )
            {
                engines = GetActiveTestEngineTypes();
            }

            ExecuteForEngines( engines, ( engine ) =>
            {
                AssertTemplateOutput( engine, matchRequirements, inputTemplate, options );
            } );

        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, IEnumerable<LavaTestOutputMatchRequirement> matchRequirements, string inputTemplate, LavaTestRenderOptions options = null )
        {
            try
            {
                options = options ?? new LavaTestRenderOptions();

                var outputText = GetTemplateOutput( engine, inputTemplate, options );

                Assert.IsNotNull( outputText, "Template failed to render." );

                DebugWriteRenderResult( engine, inputTemplate, outputText );

                // Apply formatting options to the output.
                var outputCompareText = outputText;
                if ( options.IgnoreWhiteSpace )
                {
                    outputCompareText = Regex.Replace( outputCompareText, @"\s*", string.Empty );
                }

                if ( options.IgnoreCase )
                {
                    outputCompareText = outputCompareText.ToLower();
                }

                var errorMessages = new List<string>();

                foreach ( var matchRequirement in matchRequirements )
                {
                    // Determine the correct match type. Multiple matches for "equal" are interpreted as "contains".
                    var matchType = matchRequirement.MatchType;
                    if ( matchRequirements.Count() > 1
                         && matchType == LavaTestOutputMatchTypeSpecifier.Equal )
                    {
                        matchType = LavaTestOutputMatchTypeSpecifier.Contains;
                    }

                    var expectedOutputText = matchRequirement.MatchValue ?? string.Empty;

                    // If ignoring whitespace, strip it from the comparison string.
                    var expectedOutputCompareText = expectedOutputText;
                    if ( options.IgnoreWhiteSpace )
                    {
                        expectedOutputCompareText = Regex.Replace( expectedOutputCompareText, @"\s*", string.Empty );
                    }

                    var matchWild = options.Wildcards != null && options.Wildcards.Any();
                    var matchRegex = options.OutputMatchType == LavaTestOutputMatchTypeSpecifier.RegEx;

                    if ( matchWild || matchRegex )
                    {
                        // Replace wildcards with a non-Regex symbol.
                        foreach ( var wildcard in options.Wildcards )
                        {
                            expectedOutputCompareText = expectedOutputCompareText.Replace( wildcard, "<<<wildCard>>>" );
                        }

                        if ( matchWild )
                        {
                            // If this is a wildcard match, escape all other Regex characters.
                            expectedOutputCompareText = Regex.Escape( expectedOutputCompareText );
                        }

                        // Require a match of 1 or more characters for a wildcard.
                        expectedOutputCompareText = expectedOutputCompareText.Replace( "<<<wildCard>>>", "(.+)" );

                        if ( matchWild && !options.IgnoreWhiteSpace )
                        {
                            // If this is a wildcard match and whitespace is ignored, add RegEx anchors for the start and end of the template.
                            expectedOutputCompareText = "^" + expectedOutputCompareText + "$";
                        }

                        var regex = new Regex( expectedOutputCompareText );
                        var regexMatch = regex.Match( outputCompareText );
                        if ( !regexMatch.Success )
                        {
                            var msg = "No match found for regular expression. [Regex=" + expectedOutputCompareText + "]";

                            // Add the first difference from the unaltered inputs for diagnostic purposes.
                            string message;
                            var isValid = GetSimpleTextMatchResult( expectedOutputText, outputText, options.IgnoreWhiteSpace, options.IgnoreCase, out message );

                            throw new Exception( "No match found for regular expression. See the inner exception for the first detected plain text match failure. [Regex=" + expectedOutputCompareText + "]", new Exception( message ) );
                        }
                    }
                    else
                    {
                        if ( matchType == LavaTestOutputMatchTypeSpecifier.Equal )
                        {
                            string message;
                            var isValid = GetSimpleTextMatchResult( expectedOutputCompareText, outputCompareText, options.IgnoreWhiteSpace, options.IgnoreCase, out message );

                            if ( !isValid )
                            {
                                Assert.That.Fail( message );
                            }
                        }
                        else
                        {
                            if ( options.IgnoreCase )
                            {
                                expectedOutputCompareText = expectedOutputCompareText.ToLower();
                            }

                            if ( matchType == LavaTestOutputMatchTypeSpecifier.Contains )
                            {
                                Assert.That.Contains( outputCompareText, expectedOutputCompareText );
                            }
                            else if ( matchType == LavaTestOutputMatchTypeSpecifier.DoesNotContain )
                            {
                                Assert.That.DoesNotContain( outputCompareText, expectedOutputCompareText );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                // Specify the engine identifier in the top-level exception.
                throw new Exception( $"[{engine.EngineName}] Template render failed.", ex );
            }
        }

        public void AssertTextMatch( string expected, string actual, bool ignoreWhitespace = false, bool ignoreCase = false )
        {
            var isMatch = GetSimpleTextMatchResult( expected, actual, ignoreWhitespace, ignoreCase, out string message );

            if ( !isMatch )
            {
                Assert.That.Fail( message );
            }
        }

        private bool GetSimpleTextMatchResult( string expected, string actual, bool ignoreWhitespace, bool ignoreCase, out string message )
        {
            message = string.Empty;

            expected = expected.ToString();
            actual = actual.ToString();

            if ( string.IsNullOrEmpty( expected ) && !string.IsNullOrEmpty( actual ) )
            {
                message = GetTextMatchFailureMessage( "(empty)", actual, 1, 1 );
                return false;
            }

            if ( string.IsNullOrEmpty( actual ) && !string.IsNullOrEmpty( expected ) )
            {
                message = GetTextMatchFailureMessage( expected, "(empty)", 1, 1 );
                return false;
            }

            // Perform a simple comparison.
            var compareType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if ( expected.Equals( actual, compareType ) )
            {
                return true;
            }

            // Perform a character-by-character comparison.
            var indexExpected = 0;
            var indexActual = 0;
            while ( true )
            {
                var expectedChar = GetNextCharacter( expected, ref indexExpected, ignoreWhitespace );
                var actualChar = GetNextCharacter( actual, ref indexActual, ignoreWhitespace );

                if ( expectedChar == null && actualChar == null )
                {
                    // We have reached the end of at least one string.
                    if ( !ignoreWhitespace && indexExpected != indexActual )
                    {
                        message = GetTextMatchFailureMessage( expected, actual, indexExpected, indexActual );
                        return false;
                    }

                    break;
                }
                if ( expectedChar == actualChar )
                {
                    continue;
                }
                if ( ignoreCase && char.ToUpperInvariant( expectedChar.GetValueOrDefault() ) == char.ToUpperInvariant( actualChar.GetValueOrDefault() ) )
                {
                    continue;
                }

                message = GetTextMatchFailureMessage( expected, actual, indexExpected, indexActual );
                return false;
            }

            return true;
        }

        private char? GetNextCharacter( string input, ref int cursorIndex, bool skipWhiteSpace = false )
        {
            cursorIndex++;
            if ( cursorIndex < input.Length )
            {
                if ( skipWhiteSpace )
                {
                    if ( char.IsWhiteSpace( input[cursorIndex] ) )
                    {
                        return GetNextCharacter( input, ref cursorIndex, skipWhiteSpace );
                    }
                }
                return input[cursorIndex];
            }

            return null;
        }

        private string GetTextMatchFailureMessage( string expected, string actual, int indexExpected, int indexActual )
        {
            const int showCharacterBufferMax = 50;

            // Get the position indicators for the match failure.
            var matchedText = actual.Substring( 0, indexActual );
            var line = matchedText.Where( c => c == '\n' )
                .Count() + 1;
            var column = indexActual;
            var lastCrIndex = matchedText.LastIndexOf( '\n' );
            if ( lastCrIndex > 0 )
            {
                column = column - lastCrIndex;
            }

            // Get the expected text snippet, including the last matched character for context.
            var expectedText = string.Empty;
            if ( indexExpected < expected.Length )
            {
                var startIndex = indexExpected > 0 ? indexExpected - 1 : 0;
                if ( showCharacterBufferMax > expected.Length - startIndex )
                {
                    expectedText = expected.Substring( startIndex );
                }
                else
                {
                    expectedText = expected.Substring( startIndex, showCharacterBufferMax + 1 )
                        .Truncate( showCharacterBufferMax );
                }
            }

            // Get the actual text snippet, including the last matched character for context.
            var actualText = string.Empty;
            if ( indexActual < actual.Length )
            {
                var startIndex = indexActual > 0 ? indexActual - 1 : 0;
                if ( showCharacterBufferMax > actual.Length - startIndex )
                {
                    actualText = actual.Substring( startIndex );
                }
                else
                {
                    actualText = actual.Substring( startIndex, showCharacterBufferMax + 1 )
                        .Truncate( showCharacterBufferMax );
                }
            }

            return $"Match failed (Line={line}, Column={column}, Offset={indexExpected}, Expected={expectedText}, Actual={actualText}).\n-->Expected: {expectedText}\n---->Actual: {actualText}";
        }

        /// <summary>
        /// Verify that the specified template is valid and renders some content.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsValid( string inputTemplate, LavaRenderParameters options = null )
        {
            ExecuteForActiveEngines( ( engine ) =>
            {
                AssertTemplateIsValid( engine, inputTemplate, options );
            } );
        }

        /// <summary>
        /// Verify that the specified template is valid and renders some content.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsValid( ILavaEngine engine, string inputTemplate, LavaRenderParameters options )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var result = engine.RenderTemplate( inputTemplate.Trim(), options );
            Assert.That.IsNull( result.Error, "The template failed to render." );
            Assert.That.IsFalse( result.Text.IsNullOrWhiteSpace(), "The template produced no output." );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            ExecuteForActiveEngines( ( engine ) =>
            {
                AssertTemplateIsInvalid( engine, inputTemplate, mergeFields );
            } );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( Type engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            var engine = GetEngineInstance( engineType );

            AssertTemplateIsInvalid( engine, inputTemplate, mergeFields );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( ILavaEngine engine, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            Assert.That.ThrowsException<LavaException>(
            () =>
            {
                var renderOptions = new LavaRenderParameters
                {
                    Context = engine.NewRenderContext( mergeFields ),
                    ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw
                };

                _ = engine.RenderTemplate( inputTemplate.Trim(), renderOptions );
            },
            "Invalid template expected." );
        }

        /// <summary>
        /// Write the results of template rendering to the debug output, with some additional configuration details.
        /// Useful to document the result of a test that would otherwise produce no output.
        /// </summary>
        /// <param name="outputString"></param>
        public void DebugWriteRenderResult( ILavaEngine engine, string inputString, string outputString )
        {
            Debug.Print( $"\n** [{engine.EngineName}] Input:\n{inputString}" );
            Debug.Print( $"\n** [{engine.EngineName}] Output:\n{outputString}" );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTime? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            ExecuteForActiveEngines( ( engine ) =>
            {
                var outputString = GetTemplateOutput( engine, inputTemplate );

                DebugWriteRenderResult( engine, inputTemplate, outputString );

                DateTime outputDate;

                var isValidDate = DateTime.TryParse( outputString, out outputDate );

                Assert.That.True( isValidDate, $"Template Output does not represent a valid DateTime. [Output=\"{outputString}\"]" );

                if ( maximumDelta != null )
                {
                    Assert.That.AreProximate( expectedDateTime, outputDate, maximumDelta.Value );
                }
                else
                {
                    Assert.That.AreEqual( expectedDateTime, outputDate );
                }
            } );
        }

        /// <summary>
        /// Resolve the specified template to a date and verify that it is equivalent to the expected date.
        /// </summary>
        /// <param name="expectedDateString"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( string expectedDateString, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            bool isValid;
            DateTime expectedDate;

            isValid = DateTime.TryParse( expectedDateString, out expectedDate );

            Assert.That.True( isValid, "Expected Date String input is not a valid date." );

            AssertTemplateOutputDate( expectedDate, inputTemplate, maximumDelta );
        }

        #region Test Configuration

        /// <summary>
        /// Get the Lava Engine configuration for the currenttest environment.
        /// </summary>
        /// <returns></returns>
        private static LavaEngineConfigurationOptions GetCurrentEngineOptions()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = new WebsiteLavaFileSystem(),
                HostService = new WebsiteLavaHost(),
                CacheService = new WebsiteLavaTemplateCacheService(),
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            return engineOptions;
        }

        /// <summary>
        /// Sets the RockDateTime timezone to the current system local timezone.
        /// </summary>
        public static void SetRockOrganizationLocalTimeZone()
        {
            RockDateTime.Initialize( TimeZoneInfo.Local );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockOrganizationTestTimeZone()
        {
            // Set to India Standard Time, or an alternative if that is the local timezone in the current environment.
            SetRockOrganizationTimeZone( "India Standard Time" );

            if ( RockDateTime.OrgTimeZoneInfo.Id == TimeZoneInfo.Local.Id )
            {
                SetRockOrganizationTimeZone( "Mountain Standard Time" );
            }

            // To simplify the process of testing date/time differences, we need to ensure that the selected timezone is not subject to Daylight Saving Time.
            // If a DST-affected timezone is used, some tests will fail when executed across DST boundary dates.
            Assert.That.IsFalse( RockDateTime.OrgTimeZoneInfo.SupportsDaylightSavingTime, "Test Timezone should not be configured for Daylight Saving Time (DST)." );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockOrganizationTimeZone( string timeZoneId )
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById( timeZoneId );

                RockDateTime.Initialize( tz );
            }
            catch ( TimeZoneNotFoundException )
            {
                throw new Exception( $"Timezone '{timeZoneId}' is not available in this environment." );
            }

            // Re-initialize the lava engine options.
            var options = GetCurrentEngineOptions();
            _fluidEngine.Initialize( options );
        }

        #endregion

        #region Test Data

        /// <summary>
        /// Return an initialized Person object for test subject Ted Decker.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonTedDecker()
        {
            var campus = new TestCampus { Name = "North Campus", Id = 1 };
            var person = new TestPerson { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1, Guid = "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" };

            return person;
        }

        /// <summary>
        /// Return an initialized Person object for test subject Alisha Marble.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonAlishaMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 2 };
            var person = new TestPerson { FirstName = "Alisha", NickName = "Alisha", LastName = "Marble", Campus = campus, Id = 2 };

            return person;
        }

        /// <summary>
        /// Return a collection of initialized Person objects for the Decker family.
        /// </summary>
        /// <returns></returns>
        public List<TestPerson> GetTestPersonCollectionForDecker()
        {
            var personList = new List<TestPerson>();

            personList.Add( GetTestPersonTedDecker() );
            personList.Add( new TestPerson { FirstName = "Cindy", LastName = "Decker", Id = 2 } );
            personList.Add( new TestPerson { FirstName = "Noah", LastName = "Decker", Id = 3 } );
            personList.Add( new TestPerson { FirstName = "Alex", LastName = "Decker", Id = 4 } );

            return personList;
        }

        #endregion

        #region Test Classes

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class TestPerson : LavaDataObject
        {
            public int Id { get; set; }
            public string Guid { get; set; }
            public string NickName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public TestCampus Campus { get; set; }

            public override string ToString()
            {
                return $"{NickName} {LastName}";
            }
        }

        /// <summary>
        /// A representation of a Campus used for testing purposes.
        /// </summary>
        public class TestCampus : LavaDataObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        public void AssertRenderTestIsValid( LavaTemplateRenderTestCase testCase )
        {
            var options = new LavaTestRenderOptions
            {
                IgnoreWhiteSpace = true,
                Wildcards = new List<string> { "<guid>" }
            };

            var inputTemplate = GetTestCaseInputTemplate( testCase );

            if ( testCase.MatchRequirements.Any() )
            {
                AssertTemplateOutput( testCase.MatchRequirements, inputTemplate, options );
            }
            else
            {
                var expectedOutput = GetTestCaseExpectedOutput( testCase );
                AssertTemplateOutput( expectedOutput, inputTemplate, options );
            }
        }

        /// <summary>
        /// Build a Lava template that contains the specified test cases.
        /// The template can be copied to a Rock application instance for testing purposes.
        /// </summary>
        /// <param name="testCases"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public string BuildRenderTestTemplate( List<LavaTemplateRenderTestCase> testCases, string heading )
        {
            var templateBuilder = new StringBuilder();

            templateBuilder.AppendLine( $"<h1>{heading}</h1>" );

            var testCasesByCategory = testCases.GroupBy( c => c.Category ).ToList();

            foreach ( var testCaseCategory in testCasesByCategory )
            {
                templateBuilder.AppendLine( $"<h2>{testCaseCategory.Key}</h2>" );
                foreach ( var testCase in testCaseCategory )
                {
                    templateBuilder.AppendLine( GetTestCaseInputTemplate( testCase ) );
                }
            }

            return templateBuilder.ToString();
        }

        private string GetTestCaseInputTemplate( LavaTemplateRenderTestCase testCase )
        {
            var templateBuilder = new StringBuilder();

            templateBuilder.AppendLine( $"<h3>{testCase.Name}</h3>" );
            templateBuilder.AppendLine( $"<p>{testCase.Description}</p>" );
            templateBuilder.AppendLine( $"<hr>" );
            templateBuilder.AppendLine( testCase.InputTemplate );

            return templateBuilder.ToString();
        }

        private string GetTestCaseExpectedOutput( LavaTemplateRenderTestCase testCase )
        {
            var templateBuilder = new StringBuilder();
            templateBuilder.AppendLine( $"<h3>{testCase.Name}</h3>" );
            templateBuilder.AppendLine( $"<p>{testCase.Description}</p>" );
            templateBuilder.AppendLine( $"<hr>" );
            templateBuilder.AppendLine( testCase.ExpectedOutput );

            return templateBuilder.ToString();
        }
    }

    /// <summary>
    /// Specifies a test for validating content in the output of a Lava template.
    /// </summary>
    public class LavaTestOutputMatchRequirement
    {
        #region Factory Methods

        public static LavaTestOutputMatchRequirement NewContainsText( string text )
        {
            var requirement = new LavaTestOutputMatchRequirement
            {
                MatchType = LavaTestOutputMatchTypeSpecifier.Contains,
                MatchValue = text
            };
            return requirement;
        }

        #endregion

        #region Constructors

        public LavaTestOutputMatchRequirement()
        {
        }

        #endregion

        public LavaTestOutputMatchRequirement( string value, LavaTestOutputMatchTypeSpecifier match )
        {
            MatchValue = value;
            MatchType = match;
        }

        public string MatchValue { get; set; }
        public LavaTestOutputMatchTypeSpecifier MatchType { get; set; }
    }

    /// <summary>
    /// A test case for a Lava Template.
    /// </summary>
    public class LavaTemplateRenderTestCase
    {
        public string Category;
        public string Name;
        public string Description;
        public string InputTemplate;
        public string ExpectedOutput;

        public List<LavaTestOutputMatchRequirement> MatchRequirements = new List<LavaTestOutputMatchRequirement>();
    }
}
