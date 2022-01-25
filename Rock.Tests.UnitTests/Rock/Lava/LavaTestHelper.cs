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
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// A base class for Lava testing helpers that provides some common services.
    /// </summary>
    /// <remarks>This class will be moved to Rock.Tests.Shared in the future.</remarks>
    public class LavaTestHelper
    {
        public static bool FluidEngineIsEnabled { get; set; }
        public static bool DotLiquidEngineIsEnabled { get; set; }
        public static bool RockLiquidEngineIsEnabled { get; set; }

        private static ILavaEngine _rockliquidEngine = null;
        private static ILavaEngine _dotliquidEngine = null;
        private static ILavaEngine _fluidEngine = null;

        public void Initialize( bool testRockLiquidEngine, bool testDotLiquidEngine, bool testFluidEngine )
        {
            // Verify the test environment: RockLiquidEngine and DotLiquidEngine are mutually exclusive test environments.
            if ( testRockLiquidEngine && testDotLiquidEngine )
            {
                throw new Exception( "RockLiquidEngine/DotLiquidEngine cannot be tested simultaneously because they require different global configurations of the DotLiquid library." );
            }

            RockLiquidEngineIsEnabled = testRockLiquidEngine;
            DotLiquidEngineIsEnabled = testDotLiquidEngine;
            FluidEngineIsEnabled = testFluidEngine;

            RegisterLavaEngines();

            if ( RockLiquidEngineIsEnabled )
            {
                // Initialize the Rock variant of the DotLiquid Engine
                var engineOptions = new LavaEngineConfigurationOptions();

                _rockliquidEngine = LavaService.NewEngineInstance( typeof( RockLiquidEngine ), engineOptions );

                // Register the common Rock.Lava filters first, then overwrite with the web-based RockFilters as needed.
                RegisterFilters( _rockliquidEngine );
            }

            if ( DotLiquidEngineIsEnabled )
            {
                // Initialize the DotLiquid Engine
                var engineOptions = new LavaEngineConfigurationOptions();

                _dotliquidEngine = LavaService.NewEngineInstance( typeof( DotLiquidEngine ), engineOptions );

                RegisterFilters( _dotliquidEngine );
            }

            if ( FluidEngineIsEnabled )
            {
                // Initialize Fluid Engine
                var engineOptions = new LavaEngineConfigurationOptions();

                _fluidEngine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

                RegisterFilters( _fluidEngine );
            }
        }

        /// <summary>
        /// Sets the RockDateTime timezone to the current system local timezone.
        /// </summary>
        public static void SetRockDateTimeToLocalTimezone()
        {
            RockDateTime.Initialize( TimeZoneInfo.Local );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToAlternateTimezone()
        {
            var tz = GetTestTimeZoneAlternate();

            RockDateTime.Initialize( tz );

            // Re-initialize the lava engine options.
            var options = GetCurrentEngineOptions();
            _fluidEngine.Initialize( options );
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneAlternate()
        {
            TimeZoneInfo tz;

            // Set to India Standard Time, or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "India Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'IST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

                Assert.That.IsNotNull( tz, "Timezone 'MST' is not available in this environment." );
            }

            // To simplify the process of testing date/time differences, we need to ensure that the selected timezone is not subject to Daylight Saving Time.
            // If a DST-affected timezone is used, some tests will fail when executed across DST boundary dates.
            Assert.That.IsFalse( tz.SupportsDaylightSavingTime, "Test Timezone should not be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone supports daylight saving time.
        /// </summary>
        public static void SetRockDateTimeToDaylightSavingTimezone()
        {
            // Set to Mountain Standard Time (MST), a timezone that supports Daylight Saving Time (DST).
            var tz = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'MST' is not available in this environment." );

            Assert.That.IsTrue( tz.SupportsDaylightSavingTime, "Test Timezone should be configured for Daylight Saving Time (DST)." );

            RockDateTime.Initialize( tz );

            // Re-initialize the lava engine options.
            var options = GetCurrentEngineOptions();
            _fluidEngine.Initialize( options );
        }

        /// <summary>
        /// Get the Lava Engine configuration for the currenttest environment.
        /// </summary>
        /// <returns></returns>
        private static LavaEngineConfigurationOptions GetCurrentEngineOptions()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = new WebsiteLavaFileSystem(),
                CacheService = new WebsiteLavaTemplateCacheService(),
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            return engineOptions;
        }

        private static void RegisterLavaEngines()
        {
            // Register the RockLiquid Engine (pre-v13).
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = new LavaEngineConfigurationOptions();

                var rockLiquidEngine = new RockLiquidEngine();

                rockLiquidEngine.Initialize( engineOptions );

                return rockLiquidEngine;
            } );

            // Register the DotLiquid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = GetCurrentEngineOptions();

                var dotLiquidEngine = new DotLiquidEngine();

                dotLiquidEngine.Initialize( engineOptions );

                return dotLiquidEngine;
            } );

            // Register the Fluid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = GetCurrentEngineOptions();

                var fluidEngine = new FluidEngine();

                fluidEngine.Initialize( engineOptions );

                return fluidEngine;
            } );
        }

        private static void RegisterFilters( ILavaEngine engine )
        {
            // Register the common Rock.Lava filters first, then overwrite with the web-specific filters.
            if ( engine.EngineIdentifier == TestGuids.LavaEngines.RockLiquid.AsGuid() )
            {
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                engine.RegisterFilters( typeof( global::Rock.Lava.RockFilters ) );
            }
            else
            {
                engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
                engine.RegisterFilters( typeof( global::Rock.Lava.LavaFilters ) );
            }
        }

        public ILavaEngine GetEngineInstance( Type engineType )
        {
            if ( engineType == typeof( FluidEngine ) )
            {
                return _fluidEngine;
            }
            if ( engineType == typeof( DotLiquidEngine ) )
            {
                return _dotliquidEngine;
            }
            else if ( engineType == typeof( RockLiquidEngine ) )
            {
                return _rockliquidEngine;
            }

            throw new Exception( $"Cannot return an instance of engine type \"{ engineType }\"." );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( Type engineType, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            var engine = GetEngineInstance( engineType );

            return GetTemplateOutput( engine, inputTemplate, mergeValues );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( ILavaEngine engine, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            if ( engine == null )
            {
                throw new Exception( "Engine instance is required." );
            }

            var context = engine.NewRenderContext( mergeValues );

            return GetTemplateOutput( engine, inputTemplate, new LavaRenderParameters { Context = context } );
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( ILavaEngine engine, string inputTemplate, LavaRenderParameters renderParameters )
        {
            inputTemplate = inputTemplate ?? string.Empty;

            if ( engine == null )
            {
                throw new Exception( "Engine instance is required." );
            }

            var result = engine.RenderTemplate( inputTemplate.Trim(), renderParameters );

            if ( result.HasErrors )
            {
                throw result.Error;
            }

            return result.Text;
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaRenderParameters parameters, bool ignoreWhitespace = false )
        {
            var engines = GetActiveTestEngines();

            var exceptions = new List<Exception>();

            foreach ( var engine in engines )
            {
                try
                {
                    AssertTemplateOutput( engine, expectedOutput, inputTemplate, parameters, ignoreWhitespace );
                }
                catch ( Exception ex )
                {
                    Debug.Write( $"**\n** ERROR\n**\n{ex.Message}" );

                    exceptions.Add( new Exception( $"Engine \"{ engine.EngineName }\" reported an error.", ex ) );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( "At least one engine reported errors.", exceptions );
            }
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhitespace = false )
        {
            var parameters = LavaRenderParameters.WithContext( LavaRenderContext.FromMergeValues( mergeValues ) );

            AssertTemplateOutput( expectedOutput, inputTemplate, parameters, ignoreWhitespace );
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( Type engineType, string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhitespace = false )
        {
            var engine = GetEngineInstance( engineType );

            AssertTemplateOutput( engine, expectedOutput, inputTemplate, mergeValues, ignoreWhitespace );
        }

        #region Active Engines

        private List<ILavaEngine> _activeEngines = null;

        private List<ILavaEngine> GetActiveTestEngines()
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

            return _activeEngines;
        }

        #endregion

        /// <summary>
        /// For each of the currently enabled Lava Engines, register a safe type.
        /// </summary>
        /// <param name="type"></param>
        public void RegisterSafeType( Type type )
        {
            if ( DotLiquidEngineIsEnabled )
            {
                _dotliquidEngine.RegisterSafeType( type );
            }

            if ( FluidEngineIsEnabled )
            {
                _fluidEngine.RegisterSafeType( type );
            }
        }

        /// <summary>
        /// For each of the currently enabled Lava Engines, process the specified action.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void ExecuteForActiveEngines( Action<ILavaEngine> testMethod )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                testMethod( engine );
            }
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhitespace = false )
        {
            var context = engine.NewRenderContext( mergeValues );

            AssertTemplateOutput( engine, expectedOutput, inputTemplate, LavaRenderParameters.WithContext( context ), ignoreWhitespace );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( ILavaEngine engine, string expectedOutput, string inputTemplate, LavaRenderParameters renderParameters, bool ignoreWhitespace = false )
        {
            var outputString = GetTemplateOutput( engine, inputTemplate, renderParameters );

            var debugString = outputString;

            if ( ignoreWhitespace )
            {
                expectedOutput = expectedOutput.RemoveWhiteSpace();
                outputString = outputString.RemoveWhiteSpace();

                debugString += "\n(Comparison ignores WhiteSpace)";
            }

            DebugWriteRenderResult( engine, inputTemplate, debugString );

            Assert.That.Equal( expectedOutput, outputString );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            foreach ( var engine in GetActiveTestEngines() )
            {
                AssertTemplateOutputRegex( engine, expectedOutput, inputTemplate, mergeValues );
            }
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( ILavaEngine engine, string expectedOutputRegex, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            var outputString = GetTemplateOutput( engine, inputTemplate, mergeValues );

            var regex = new Regex( expectedOutputRegex );

            WriteOutputToDebug( engine, outputString );

            StringAssert.Matches( outputString, regex );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTime? expectedOutput, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                AssertTemplateOutputDate( engine, expectedOutput, inputTemplate, maximumDelta );
            }
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTimeOffset? expectedOutput, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                AssertTemplateOutputDate( engine, expectedOutput, inputTemplate, maximumDelta );
            }
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( string expectedRockDateTimeOutput, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            AssertTemplateOutputDate( LavaDateTime.ParseToOffset( expectedRockDateTimeOutput ), inputTemplate, maximumDelta );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( ILavaEngine engine, DateTimeOffset? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            var outputString = GetTemplateOutput( engine, inputTemplate );

            var outputDateUtc = LavaDateTime.ParseToOffset( outputString, null );

            WriteOutputToDebug( engine, outputString );

            Assert.That.IsNotNull( outputDateUtc, $"Template Output does not represent a valid DateTime. [Output=\"{ outputString }\"]" );

            try
            {
                if ( maximumDelta != null )
                {
                    DateTimeAssert.AreEqual( expectedDateTime, outputDateUtc, maximumDelta.Value );
                }
                else
                {
                    DateTimeAssert.AreEqual( expectedDateTime, outputDateUtc );
                }
            }
            catch ( Exception ex )
            {
                var info = $@"
Test Environment:
LavaEngine = { engine.EngineName },
LocalDateTime = {DateTimeOffset.Now},
LocalTimeZoneName = {TimeZoneInfo.Local.DisplayName},
LocalTimeZoneOffset = { TimeZoneInfo.Local.BaseUtcOffset }
RockDateTime = { LavaDateTime.NowOffset },
RockTimeZoneName = { RockDateTime.OrgTimeZoneInfo.DisplayName },
RockTimeZoneOffset = { RockDateTime.OrgTimeZoneInfo.BaseUtcOffset }
";
                throw new Exception( $"Lava Date/Time test failed.\n{ info }", ex );
            }
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( ILavaEngine engine, DateTime? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            AssertDateIsUtc( expectedDateTime );

            var outputString = GetTemplateOutput( engine, inputTemplate );

            var outputDateUtc = LavaDateTime.ParseToUtc( outputString, null );

            WriteOutputToDebug( engine, outputString );

            Assert.That.IsNotNull( outputDateUtc, $"Template Output does not represent a valid DateTime. [Output=\"{ outputString }\"]" );

            if ( maximumDelta != null )
            {
                DateTimeAssert.AreEqual( expectedDateTime, outputDateUtc, maximumDelta.Value );
            }
            else
            {
                DateTimeAssert.AreEqual( expectedDateTime, outputDateUtc );
            }
        }

        public void AssertDateIsUtc( DateTime? dateTime )
        {
            // Verify that we have an expected output date in UTC, to avoid any possible ambiguity between Rock time and Local time.
            if ( dateTime == null || dateTime.Value.Kind != DateTimeKind.Utc )
            {
                throw new Exception( "Expected DateTime must be expressed in UTC." );
            }
        }

        /// <summary>
        /// Write the rendered template to debug output.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="outputString"></param>
        private void WriteOutputToDebug( ILavaEngine engine, string outputString )
        {
            Debug.Print( $"\n**\n** Template Output ({engine.EngineName}):\n**\n{outputString}" );
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
    }
}
