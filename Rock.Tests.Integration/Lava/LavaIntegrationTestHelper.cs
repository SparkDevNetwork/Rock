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
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Lava
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

        public static void Initialize( bool testRockLiquidEngine, bool testDotLiquidEngine, bool testFluidEngine )
        {
            RockLiquidEngineIsEnabled = testRockLiquidEngine;
            DotLiquidEngineIsEnabled = testDotLiquidEngine;
            FluidEngineIsEnabled = testFluidEngine;

            var engineOptions = new LavaEngineConfigurationOptions();

            engineOptions.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;

            if ( RockLiquidEngineIsEnabled )
            {
                // Initialize the Rock variant of the DotLiquid Engine
                engineOptions.FileSystem = new MockFileProvider();
                engineOptions.CacheService = new WebsiteLavaTemplateCacheService();

                _rockliquidEngine = global::Rock.Lava.LavaService.NewEngineInstance( LavaEngineTypeSpecifier.RockLiquid, engineOptions );

                RegisterFilters( _rockliquidEngine );
                RegisterTags( _rockliquidEngine );
                RegisterBlocks( _rockliquidEngine );

                RegisterStaticShortcodes( _rockliquidEngine );
                RegisterDynamicShortcodes( _rockliquidEngine );
            }

            if ( DotLiquidEngineIsEnabled )
            {
                // Initialize the DotLiquid Engine
                engineOptions.FileSystem = new MockFileProvider();
                engineOptions.CacheService = new WebsiteLavaTemplateCacheService();

                _dotliquidEngine = global::Rock.Lava.LavaService.NewEngineInstance( LavaEngineTypeSpecifier.DotLiquid, engineOptions );

                RegisterFilters( _dotliquidEngine );
                RegisterTags( _dotliquidEngine );
                RegisterBlocks( _dotliquidEngine );

                RegisterStaticShortcodes( _dotliquidEngine );
                RegisterDynamicShortcodes( _dotliquidEngine );
            }

            if ( FluidEngineIsEnabled )
            {
                // Initialize Fluid Engine
                engineOptions = new LavaEngineConfigurationOptions();

                engineOptions.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;
                engineOptions.FileSystem = new MockFileProvider();
                var cacheService = new WebsiteLavaTemplateCacheService();
                engineOptions.CacheService = cacheService;

                _fluidEngine = global::Rock.Lava.LavaService.NewEngineInstance( LavaEngineTypeSpecifier.Fluid, engineOptions );

                RegisterFilters( _fluidEngine );
                RegisterTags( _fluidEngine );
                RegisterBlocks( _fluidEngine );

                RegisterStaticShortcodes( _fluidEngine );
                RegisterDynamicShortcodes( _fluidEngine );
            }

            _instance = new LavaIntegrationTestHelper();
        }

        public ILavaEngine GetEngineInstance( LavaEngineTypeSpecifier engineType )
        {
            ILavaEngine engine = null;

            if ( engineType == LavaEngineTypeSpecifier.DotLiquid )
            {
                engine = _dotliquidEngine;
            }
            else if ( engineType == LavaEngineTypeSpecifier.Fluid )
            {
                engine = _fluidEngine;
            }
            else if ( engineType == LavaEngineTypeSpecifier.RockLiquid )
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
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
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
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
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

            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
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
            if ( engine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
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
                    var elementTypes = Rock.Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

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
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
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
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, ILavaRenderContext context )
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
        public string GetTemplateOutput( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaTestRenderOptions options )
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
        /// For each of the currently enabled Lava Engines, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        public void ExecuteForActiveEngines( Action<ILavaEngine> testMethod )
        {
            var engines = GetActiveTestEngines();

            var exceptions = new List<Exception>();

            foreach ( var engine in engines )
            {
                LavaService.SetCurrentEngine( engine );

                Debug.Print( $"\n**\n** Lava Render Test: {engine.EngineType}\n**\n" );

                try
                {
                    testMethod( engine );
                }
                catch (Exception ex)
                {
                    // Write the error to debug output.
                    Debug.Print( $"\n** ERROR:\n{ex.ToString()}" );

                    exceptions.Add( ex );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( "Test failed for one or more Lava engines.", exceptions );
            }
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

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            ExecuteForActiveEngines( ( engine ) =>
            {
                AssertTemplateOutput( engine, expectedOutput, inputTemplate, options );
            } );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( LavaEngineTypeSpecifier engineType, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            var engine = GetEngineInstance( engineType );

            AssertTemplateOutput( engine, expectedOutput, inputTemplate, options );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, string expectedOutput, string inputTemplate, LavaTestRenderOptions options = null )
        {
            options = options ?? new LavaTestRenderOptions();

            var outputString = GetTemplateOutput( engine, inputTemplate, options );

            Assert.IsNotNull( outputString, "Template failed to render." );

            DebugWriteRenderResult( engine.EngineType, inputTemplate, outputString );

            // If ignoring whitespace, strip it from the input and output.
            if ( options.IgnoreWhiteSpace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }

            var matchRegex = options.OutputMatchType == LavaTestOutputMatchTypeSpecifier.RegEx
                || ( options.Wildcards != null && options.Wildcards.Any() );

            if ( matchRegex )
            {
                // Replace wildcards with a non-Regex symbol.
                foreach ( var wildcard in options.Wildcards )
                {
                    expectedOutput = expectedOutput.Replace( wildcard, "<<<wildCard>>>" );
                }

                expectedOutput = Regex.Escape( expectedOutput );

                expectedOutput = expectedOutput.Replace( "<<<wildCard>>>", "(.*)" );

                var regex = new Regex( expectedOutput );

                StringAssert.Matches( outputString, regex );
            }
            else
            {
                if ( options.OutputMatchType == LavaTestOutputMatchTypeSpecifier.Equal )
                {
                    Assert.That.Equal( expectedOutput, outputString );
                }
                else if ( options.OutputMatchType == LavaTestOutputMatchTypeSpecifier.Contains )
                {
                    Assert.That.Contains( outputString, expectedOutput );
                }
                else if ( options.OutputMatchType == LavaTestOutputMatchTypeSpecifier.DoesNotContain )
                {
                    Assert.That.DoesNotContain( outputString, expectedOutput );
                }
            }
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
                AssertTemplateIsInvalid( engine.EngineType, inputTemplate, mergeFields );
            } );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( LavaEngineTypeSpecifier engineType, string inputTemplate, LavaDataDictionary mergeFields = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            var engine = GetEngineInstance( engineType );

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
        public void DebugWriteRenderResult( LavaEngineTypeSpecifier engineType, string inputString, string outputString )
        {
            var engine = GetEngineInstance( engineType );

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
                var outputString = GetTemplateOutput( engine.EngineType, inputTemplate );

                DebugWriteRenderResult( engine.EngineType, inputTemplate, outputString );

                DateTime outputDate;

                var isValidDate = DateTime.TryParse( outputString, out outputDate );

                Assert.That.True( isValidDate, $"Template Output does not represent a valid DateTime. [Output=\"{ outputString }\"]" );

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

    }

    /// <summary>
    /// A set of options for testing the rendering of a Lava template.
    /// </summary>
    public class LavaTestRenderOptions
    {
        public IDictionary<string, object> MergeFields = null;
        public string EnabledCommands = null;
        public string EnabledCommandsDelimiter = ",";

        public bool IgnoreWhiteSpace = true;

        public List<string> Wildcards = new List<string>();

        public LavaTestOutputMatchTypeSpecifier OutputMatchType = LavaTestOutputMatchTypeSpecifier.Equal;

        public ExceptionHandlingStrategySpecifier? ExceptionHandlingStrategy;
    }

    public enum LavaTestOutputMatchTypeSpecifier
    {
        Equal = 0,
        Contains = 1,
        DoesNotContain = 2,
        RegEx = 3
    }
}
