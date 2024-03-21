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

using DotLiquid;

using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;
using Rock.Tests.Shared.Lava;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Performance.Modules.Lava
{
    /// <summary>
    /// Run a series of tests to benchmark the performance of selected Lava engine implementations.
    /// </summary>
    public class LavaPerformanceTestConsole
    {
        private const int _defaultTestSetCount = 5000;
        private const int _updateFrequencyMs = 1000;
        private const bool _showFirstRenderResult = false;

        // Enable/disable template caching for Fluid.
        // Template caching should always be enabled if comparing benchmarks with the RockLiquid engine,
        // because caching cannot be disabled for the RockLiquid engine.
        private const bool _templateCachingEnabled = true;

        public static void Execute( string[] args )
        {
            string command;

            // Get the command-line args.
            if ( args != null && args.Length > 0 )
            {
                command = args[0].Trim().ToLower();
            }
            else
            {
                command = "lava-pt";
            }

            if ( command == "lava-pt" )
            {
                Execute( args );
            }
            else
            {
                throw new Exception( $"Unknown Command \"{command}\"." );
            }
        }

        public static void Execute()
        {
            // Continue executing until the user presses CTRL-C.
            while ( true )
            {
                Console.Clear();

                var testTemplates = TestData.GetLavaTemplates();

                ConsoleOutputManager.WriteHeading1( $"" );
                ConsoleOutputManager.WriteHeading1( $"**" );
                ConsoleOutputManager.WriteHeading1( $"** Lava Parse/Render Performance Test" );
                ConsoleOutputManager.WriteHeading1( $"**" );
                ConsoleOutputManager.WriteHeading1( $"** Templates in Current Test Set:" );
                foreach ( var testEntry in testTemplates )
                {
                    ConsoleOutputManager.WriteHeading1( $"** --> { testEntry.TestName } " );
                }

                ConsoleOutputManager.WriteHeading1( $"**" );

                // Prompt for test parameters.
                ConsoleOutputManager.WriteInfo( $"Enter the number of Test Sets to execute (default={ _defaultTestSetCount })? " );

                var testCount = Console.ReadLine().AsIntegerOrNull() ?? _defaultTestSetCount;

                // Initialize the test environment.
                ConsoleOutputManager.WriteInfo( "" );
                ConsoleOutputManager.WriteInfo( "Initializing environment..." );
                InitializeEnvironment();

                ConsoleOutputManager.WriteInfo( "Initializing Lava framework..." );
                InitializeLava();

                // Run the tests.
                ConsoleOutputManager.WriteInfo( "Starting test run..." );
                RunParseAndRenderPerformanceTests( testCount );

                // Wait for user input.
                ConsoleOutputManager.WriteConsole( OutputMessageTypeSpecifier.Heading1, "Test complete. Press any key to continue..." );
                Console.ReadKey();
            }
        }

        private static void InitializeEnvironment()
        {
            // Execute some Entity Framework queries so the framework is primed.
            // This ensures that the startup overhead won't affect the test results.
            var rockContext = new RockContext();

            var person = new PersonService( rockContext ).Get( 1 );
            var groups = new GroupService( rockContext ).Queryable()
                                    .Where( g =>
                                        g.GroupTypeId == 25
                                        && g.Members.Count() > 2
                                        && g.IsActive == true
                                    )
                                    .Select( g => new { g.Members, g.Name, g.Description } )
                                    .ToList();
        }

        private static void RunParseAndRenderPerformanceTests( int totalSets )
        {
            var testTemplates = TestData.GetLavaTemplates();

            RunParseAndRenderPerformanceTest( typeof( FluidEngine ), testTemplates, totalSets );
            RunParseAndRenderPerformanceTest( typeof( RockLiquidEngine ), testTemplates, totalSets );
        }

        private static void RunParseAndRenderPerformanceTest( Type engineType, List<TestDataTemplateItem> testTemplates, int totalSets )
        {
            ConsoleOutputManager.WriteHeading2( $"**" );
            ConsoleOutputManager.WriteHeading2( $"** Testing Engine: { engineType.Name }" );
            ConsoleOutputManager.WriteHeading2( $"**" );

            var resultTracker = new TestResultTracker();

            // Create a Lava engine with template caching disabled.
            var engineOptions = new LavaEngineConfigurationOptions { CacheService = new NullTemplateCacheService() };

            var engine = LavaService.NewEngineInstance( engineType, engineOptions );

            var totalIterationsPerSet = testTemplates.Count;

            var mergeFields = new LavaDataDictionary();

            engine.ClearTemplateCache();

            ConsoleOutputManager.WriteInfo( $"Working: ..." );

            double lastUpdateElapsedMs = 0;

            for ( int repeatCount = 1; repeatCount <= totalSets; repeatCount++ )
            {
                for ( int i = 0; i < totalIterationsPerSet; i++ )
                {
                    var test = testTemplates[i];

                    var elapsedTime = ExecuteAndGetElapsedTime( () =>
                    {
                        var options = new LavaRenderParameters();

                        // If template rendering fails, the test is invalid.
                        options.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;

                        var result = engine.RenderTemplate( test.TemplateText );

                        if ( repeatCount == 1 && _showFirstRenderResult )
                        {
                            ConsoleOutputManager.WriteDetail( $"" );
                            ConsoleOutputManager.WriteDetail( $"<< START: Template Output \"{ testTemplates[i].TestName }\" >>" );
                            ConsoleOutputManager.WriteDetail( result.Text );
                            ConsoleOutputManager.WriteDetail( $"<<   END: Template Output >>" );

                            ConsoleOutputManager.WriteConsole( OutputMessageTypeSpecifier.Information, "Working: ..." );
                        }

                        // Verify the output?
                        if ( result.HasErrors )
                        {
                            throw new Exception( "Render error." );
                        }
                    } );

                    resultTracker.AddResult( test.TestName, i, elapsedTime.Ticks );

                    lastUpdateElapsedMs += elapsedTime.TotalMilliseconds;

                    if ( lastUpdateElapsedMs > _updateFrequencyMs )
                    {
                        // Show some progress on the console...
                        ConsoleOutputManager.WriteConsole( OutputMessageTypeSpecifier.Information, "." );
                        lastUpdateElapsedMs = 0;
                    }
                }
            };

            var testCount = testTemplates.Count * totalSets;

            ConsoleOutputManager.WriteHeading2( $"" );
            ConsoleOutputManager.WriteHeading2( $"--" );
            ConsoleOutputManager.WriteHeading2( $"-- Results Summary - { engineType.Name }" );
            ConsoleOutputManager.WriteHeading2( $"--" );
            ConsoleOutputManager.WriteHeading2( $"-- Total Tests: { testCount } items" );

            var headerRow = $"-- {"Test Name",20} | {"Total",13} | {"Min",13} | {"Max",13} | {"Mean",13} | {"Median",13}";

            var ruleRow = new string( '-', headerRow.Length );

            ConsoleOutputManager.WriteHeading2( ruleRow );
            ConsoleOutputManager.WriteHeading2( headerRow );
            ConsoleOutputManager.WriteHeading2( ruleRow );

            foreach ( var test in testTemplates )
            {
                var results = GetResultsForTest( resultTracker, test.TestName );

                ConsoleOutputManager.WriteHeading2( $"-- {test.TestName,20} | " + results.AsDelimited( " | " ) );
            }

            var resultsAll = GetResultsForTest( resultTracker, null );

            ConsoleOutputManager.WriteHeading2( ruleRow );
            ConsoleOutputManager.WriteHeading2( $"-- {"All",20} | " + resultsAll.AsDelimited( " | " ) );
            ConsoleOutputManager.WriteHeading2( ruleRow );
            ConsoleOutputManager.WriteHeading2( $"--" );
        }

        private static List<string> GetResultsForTest( TestResultTracker resultTracker, string testKey )
        {
            var results = new List<string>();

            results.Add( resultTracker.GetTotalTime( testKey ).ToString( @"hh\:mm\:ss\.ffff" ) );
            results.Add( resultTracker.GetMinTime( testKey ).ToString( @"hh\:mm\:ss\.ffff" ) );
            results.Add( resultTracker.GetMaxTime( testKey ).ToString( @"hh\:mm\:ss\.ffff" ) );
            results.Add( resultTracker.GetMeanTime( testKey ).ToString( @"hh\:mm\:ss\.ffff" ) );
            results.Add( resultTracker.GetMedianTime( testKey ).ToString( @"hh\:mm\:ss\.ffff" ) );

            return results;
        }

        /// <summary>
        /// Execute an action and return the elapsed time.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <returns></returns>
        private static TimeSpan ExecuteAndGetElapsedTime( Action testMethod )
        {
            var stopwatch = Stopwatch.StartNew();

            testMethod();

            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        #region Lava Initialization

        /// <summary>
        /// Initialize the Lava framework.
        /// </summary>
        private static void InitializeLava()
        {
            InitializeLavaEngines();

            InitializeRockLiquidLibrary();
        }

        private static void InitializeLavaEngines()
        {
            // Register the RockLiquid Engine (pre-v13).
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = new LavaEngineConfigurationOptions
                {
                    DefaultEnabledCommands = new List<string> { "all" }
                };

                var rockLiquidEngine = new RockLiquidEngine();

                InitializeLavaEngineInstance( rockLiquidEngine, engineOptions );

                return rockLiquidEngine;
            } );

            // Register the Fluid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = new LavaEngineConfigurationOptions
                {
                    FileSystem = new WebsiteLavaFileSystem(),
                    DefaultEnabledCommands = new List<string> { "all" }
                };

                if ( _templateCachingEnabled )
                {
                    engineOptions.CacheService = new WebsiteLavaTemplateCacheService();
                }

                // Note:
                var fluidEngine = new FluidEngine();

                InitializeLavaEngineInstance( fluidEngine, engineOptions );

                return fluidEngine;
            } );
        }

        private static void InitializeRockLiquidLibrary()
        {
            _ = LavaService.NewEngineInstance( typeof( RockLiquidEngine ) );

            // Register the set of filters that are compatible with RockLiquid.
            Template.RegisterFilter( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            Template.RegisterFilter( typeof( global::Rock.Lava.RockFilters ) );

            // Initialize the RockLiquid file system.
            Template.FileSystem = new LavaFileSystem();

            // Execute Startups to allow RockLiquid Blocks to register.
            var startups = new Dictionary<int, List<IRockStartup>>();
            foreach ( var startupType in Reflection.FindTypes( typeof( IRockStartup ) ).Select( a => a.Value ).ToList() )
            {
                var startup = Activator.CreateInstance( startupType ) as IRockStartup;
                startups.AddOrIgnore( startup.StartupOrder, new List<IRockStartup>() );
                startups[startup.StartupOrder].Add( startup );
            }

            foreach ( var startupList in startups.OrderBy( s => s.Key ).Select( s => s.Value ) )
            {
                foreach ( var startup in startupList )
                {
                    startup.OnStartup();
                }
            }
        }

        private static void InitializeLavaEngineInstance( ILavaEngine engine, LavaEngineConfigurationOptions options )
        {
            // Initialize the Lava engine.
            engine.Initialize( options );

            // Initialize Lava extensions.
            InitializeLavaFilters( engine );
            InitializeLavaTags( engine );
            InitializeLavaBlocks( engine );
            InitializeLavaShortcodes( engine );
        }

        private static void InitializeLavaFilters( ILavaEngine engine )
        {
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                return;
            }

            // Register the common Rock.Lava filters first, then overwrite with the engine-specific filters.
            engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( global::Rock.Lava.LavaFilters ) );
        }

        private static void InitializeLavaShortcodes( ILavaEngine engine )
        {
            // Register shortcodes defined in the codebase.
            var shortcodeTypes = Reflection.FindTypes( typeof( ILavaShortcode ) ).Select( a => a.Value ).ToList();

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

            // Register shortcodes defined in the current database.
            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                // Register the shortcode with the current Lava Engine.
                // The provider is responsible for retrieving the shortcode definition from the data store and managing the web-based shortcode cache.
                WebsiteLavaShortcodeProvider.RegisterShortcode( engine, shortcode.TagName );
            }
        }

        private static void InitializeLavaTags( ILavaEngine engine )
        {
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                return;
            }

            // Get all tags and call OnStartup methods
            var elementTypes = Reflection.FindTypes( typeof( ILavaTag ) ).Select( a => a.Value ).ToList();

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

                    throw lavaException;
                }
            }
        }

        private static void InitializeLavaBlocks( ILavaEngine engine )
        {
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                return;
            }

            // Get all blocks and call OnStartup methods
            var blockTypes = Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

            foreach ( var blockType in blockTypes )
            {
                var blockInstance = Activator.CreateInstance( blockType ) as ILavaBlock;

                engine.RegisterBlock( blockInstance.SourceElementName, ( blockName ) =>
                {
                    return Activator.CreateInstance( blockType ) as ILavaBlock;
                } );

                blockInstance.OnStartup( engine );
            }
        }

        #endregion
    }
}