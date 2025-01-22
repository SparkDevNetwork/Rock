﻿// <copyright>
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
using Rock.Lava.Fluid;
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

        private static ILavaEngine _fluidEngine = null;

        public void Initialize( bool testFluidEngine )
        {
            FluidEngineIsEnabled = testFluidEngine;

            RegisterLavaEngines();

            if ( FluidEngineIsEnabled )
            {
                // Initialize Fluid Engine
                var engineOptions = new LavaEngineConfigurationOptions();

                _fluidEngine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

                RegisterFilters( _fluidEngine );
            }
        }

        /// <summary>
        /// Sets the RockDateTime timezone.
        /// </summary>
        public static void SetRockTimeZone( TimeZoneInfo tz )
        {
            RockDateTime.Initialize( tz );

            // Re-initialize the lava engine options.
            var options = GetCurrentEngineOptions();
            _fluidEngine.Initialize( options );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to the current system local timezone.
        /// </summary>
        public static void SetRockDateTimeToLocalTimezone()
        {
            SetRockTimeZone( TimeZoneInfo.Local );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a region that is ahead of UTC time (UTC+HH:MM).
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToUtcPositiveTimezone()
        {
            var tz = GetTestTimeZoneForUtcPositive();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that behind UTC time (UTC-HH:MM).
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToUtcNegativeTimezone()
        {
            var tz = GetTestTimeZoneForUtcNegative();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone supports daylight saving time.
        /// </summary>
        public static void SetRockDateTimeToDaylightSavingTimezone()
        {
            var tz = GetTestTimeZoneForDaylightSaving();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneForUtcPositive()
        {
            TimeZoneInfo tz;

            // Set to India Standard Time (UTC+05:30), or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "India Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'IST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                // Set to Tokyo Standard Time aka Japan Standard Time (UTC+09:00)
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById( "Tokyo Standard Time" );
                }
                catch ( TimeZoneNotFoundException )
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById( "Japan Standard Time" );
                }

                Assert.That.IsNotNull( tz, "Timezone 'Tokyo Standard Time' is not available in this environment." );
            }

            // To simplify the process of testing date/time differences, we need to ensure that the selected timezone is not subject to Daylight Saving Time.
            // If a DST-affected timezone is used, some tests will fail when executed across DST boundary dates.
            Assert.That.IsFalse( tz.SupportsDaylightSavingTime, "Test Timezone should not be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneForUtcNegative()
        {
            TimeZoneInfo tz;

            // Set to UCT-07:00, or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'MST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                // Set to UCT-07:00.
                tz = TimeZoneInfo.FindSystemTimeZoneById( "Hawaiian Standard Time" );
                Assert.That.IsNotNull( tz, "Timezone 'Hawaiian Standard Time' is not available in this environment." );
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
        public static TimeZoneInfo GetTestTimeZoneForDaylightSaving()
        {
            // Set to Central Standard Time (CST), a timezone that supports Daylight Saving Time (DST).
            var tz = TimeZoneInfo.FindSystemTimeZoneById( "Central Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'CST' is not available in this environment." );

            Assert.That.IsTrue( tz.SupportsDaylightSavingTime, "Test Timezone should be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// For each of the known test timezones, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <remarks>
        /// Operations involving calendar events are sensitive to timezone conversions.
        /// In Rock, we need to be concerned with the distinction between the local server/system time and Rock time,
        /// and the effects of daylight saving time (DST) in some regions.
        /// For this reason, calendar operations need to be tested for timezones with these characteristics:
        /// * positive UTC offset
        /// * negative UTC offset
        /// * daylight saving time.
        /// </remarks>
        public static void ExecuteForTimeZones( Action<TimeZoneInfo> testMethod )
        {
            var timeZones = new List<TimeZoneInfo>
            {
                GetTestTimeZoneForUtcNegative(),
                GetTestTimeZoneForUtcPositive(),
                GetTestTimeZoneForDaylightSaving()
            };

            ExecuteForTimeZones( testMethod, timeZones );
        }

        /// <summary>
        /// For each of the specified timezones, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <param name="timeZones"></param>
        public static void ExecuteForTimeZones( Action<TimeZoneInfo> testMethod, List<TimeZoneInfo> timeZones )
        {
            // Get the current timezone.
            var tzCurrent = RockDateTime.OrgTimeZoneInfo;

            try
            {
                // Execute the test action for each of the test timezones.
                foreach ( var timeZone in timeZones )
                {
                    SetRockTimeZone( timeZone );

                    try
                    {
                        testMethod( timeZone );
                    }
                    catch ( Exception ex )
                    {
                        throw new Exception( $"Lava Render Failed. See inner exception for details. [TimeZone= {timeZone.DisplayName}]", ex );
                    }
                }
            }
            finally
            {
                // Restore the original timezone for subsequent tests.
                SetRockTimeZone( tzCurrent );
            }
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
                HostService = new WebsiteLavaHost(),
                CacheService = new WebsiteLavaTemplateCacheService(),
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            return engineOptions;
        }

        private static void RegisterLavaEngines()
        {
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
            engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( global::Rock.Lava.LavaFilters ) );
        }

        public ILavaEngine GetEngineInstance( Type engineType )
        {
            if ( engineType == typeof( FluidEngine ) )
            {
                return _fluidEngine;
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
        public void AssertTemplateOutputDate( DateTime? expectedOutput, string inputTemplate, TimeSpan? maximumDelta = null, LavaDataDictionary mergeValues = null )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                AssertTemplateOutputDate( engine, expectedOutput, inputTemplate, maximumDelta, mergeValues );
            }
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTimeOffset? expectedOutput, string inputTemplate, TimeSpan? maximumDelta = null, LavaDataDictionary mergeValues = null )
        {
            var engines = GetActiveTestEngines();

            foreach ( var engine in engines )
            {
                AssertTemplateOutputDate( engine, expectedOutput, inputTemplate, maximumDelta, mergeValues );
            }
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( string expectedRockDateTimeOutput, string inputTemplate, TimeSpan? maximumDelta = null, LavaDataDictionary mergeValues = null )
        {
            AssertTemplateOutputDate( LavaDateTime.ParseToOffset( expectedRockDateTimeOutput ), inputTemplate, maximumDelta, mergeValues );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( ILavaEngine engine, DateTimeOffset? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null, LavaDataDictionary mergeValues = null )
        {
            var outputString = GetTemplateOutput( engine, inputTemplate, mergeValues );

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
        public void AssertTemplateOutputDate( ILavaEngine engine, DateTime? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null, LavaDataDictionary mergeValues = null )
        {
            var expectedOffset = LavaDateTime.ConvertToRockOffset( expectedDateTime ?? DateTime.MinValue );

            AssertTemplateOutputDate( engine, expectedOffset, inputTemplate, maximumDelta, mergeValues );
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
