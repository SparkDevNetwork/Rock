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
    public class LavaTestHelper
    {
        public static LavaTestHelper New( LavaEngineTypeSpecifier? engineType = null )
        {
            engineType = engineType ?? LavaEngineTypeSpecifier.DotLiquid;

            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = new MockFileProvider(),
                CacheService = new LavaTemplateCache()
            };

            global::Rock.Lava.LavaEngine.Initialize( engineType, engineOptions );

            var engine = global::Rock.Lava.LavaEngine.CurrentEngine;

            engine.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;

            engine.RegisterFilters( typeof( Rock.Lava.RockFilters ) );

            RegisterBlocks( engine );
            RegisterTags( engine );

            RegisterStaticShortcodes( engine );
            RegisterDynamicShortcodes( engine );

            var helper = new LavaTestHelper();

            return helper;
        }

        private static void RegisterTags( ILavaEngine engine )
        {
            // Get all blocks and call OnStartup methods
            try
            {
                var elementTypes = Rock.Reflection.FindTypes( typeof( IRockLavaTag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockLavaTag;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterTag( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( elementType ) as IRockLavaTag;

                        return shortcode;
                    } );

                    try
                    {
                        instance.OnStartup();
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
            try
            {
                var elementTypes = Rock.Reflection.FindTypes( typeof( IRockLavaBlock ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as IRockLavaBlock;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterBlock( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( elementType ) as IRockLavaBlock;

                        return shortcode;
                    } );

                    try
                    {
                        instance.OnStartup();
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

        private static void RegisterStaticShortcodes( ILavaEngine engine )
        {
            // Get all shortcodes and call OnStartup methods
            try
            {
                var shortcodeTypes = Rock.Reflection.FindTypes( typeof( IRockShortcode ) ).Select( a => a.Value ).ToList();

                foreach ( var shortcodeType in shortcodeTypes )
                {
                    engine.RegisterStaticShortcode( shortcodeType.Name, ( shortcodeName ) =>
                    { 
                        var shortcode = Activator.CreateInstance( shortcodeType ) as IRockShortcode;

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

                var parameters = Rock.Lava.RockSerializableDictionary.FromUriEncodedString( shortcodeDefinition.Parameters );

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
                engine.RegisterDynamicShortcode( shortcode.TagName, shortCodeFactory );
            }
        }

        public ILavaEngine LavaEngine
        {
            get
            {
                return global::Rock.Lava.LavaEngine.CurrentEngine;
            }
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            string outputString;

            inputTemplate = inputTemplate ?? string.Empty;

            global::Rock.Lava.LavaEngine.CurrentEngine.TryRender( inputTemplate.Trim(), out outputString, mergeValues );

            return outputString;
        }

        /// <summary>
        /// Process the specified input template and return the result.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public string GetTemplateOutput( string inputTemplate, ILavaContext context )
        {
            string outputString;

            inputTemplate = inputTemplate ?? string.Empty;

            var isValidTemplate = global::Rock.Lava.LavaEngine.CurrentEngine.TryRender( inputTemplate.Trim(), out outputString, context );

            //Assert.That.True( isValidTemplate, "Lava Template is invalid." );

            return outputString;
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, ILavaContext context, bool ignoreWhiteSpace = false )
        {
            var outputString = GetTemplateOutput( inputTemplate, context );

            WriteTemplateRenderToDebug( inputTemplate, outputString );

            if ( ignoreWhiteSpace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }
            
            Assert.That.Equal( expectedOutput, outputString );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhitespace = false )
        {
            var outputString = GetTemplateOutput( inputTemplate, mergeValues );

            if ( ignoreWhitespace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }

            WriteTemplateRenderToDebug( inputTemplate, outputString );
            Assert.That.Equal( expectedOutput, outputString );
        }

        /// <summary>
        /// Verify that the specified template is invalid.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        public void AssertTemplateIsInvalid( string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            string outputString;

            inputTemplate = inputTemplate ?? string.Empty;

            var isValid = global::Rock.Lava.LavaEngine.CurrentEngine.TryRender( inputTemplate.Trim(), out outputString, mergeValues );

            Assert.That.IsFalse(isValid, "Invalid template expected." );
        }

        /// <summary>
        /// Write a rendered template to debug, with some additional configuration details.
        /// </summary>
        /// <param name="outputString"></param>
        public void WriteTemplateRenderToDebug( string inputString, string outputString )
        {
            var engineName = global::Rock.Lava.LavaEngine.CurrentEngine.EngineName;

            Debug.Print( $"\n**\n** Template Input:\n**\n{inputString}" );
            Debug.Print( $"\n**\n** Template Output ({engineName}):\n**\n{outputString}" );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutputRegex, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            var outputString = GetTemplateOutput( inputTemplate, mergeValues );

            var regex = new Regex(expectedOutputRegex);

            WriteTemplateRenderToDebug( inputTemplate, outputString );
            StringAssert.Matches( outputString, regex );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputWithWildcard( string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhiteSpace = false, string wildCard = "*" )
        {
            var outputString = GetTemplateOutput( inputTemplate, mergeValues );

            // Replace the wildcards with a non-Regex symbol.
            expectedOutput = expectedOutput.Replace( wildCard, "<<<wildCard>>>" );

            if ( ignoreWhiteSpace )
            {
                outputString = Regex.Replace( outputString, @"\s*", string.Empty );
                expectedOutput = Regex.Replace( expectedOutput, @"\s*", string.Empty );
            }

            expectedOutput = Regex.Escape( expectedOutput );
            expectedOutput = expectedOutput.Replace( "/", @"\/" );

            expectedOutput = expectedOutput.Replace( "<<<wildCard>>>", "(.*)" );

            var regex = new Regex( expectedOutput );

            WriteTemplateRenderToDebug( inputTemplate, outputString );
            StringAssert.Matches( outputString, regex );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutputRegex, string inputTemplate, ILavaContext context, bool ignoreWhiteSpace = true )
        {
            var outputString = GetTemplateOutput( inputTemplate, context );

            // If ignoring whitespace, replace any whitespace in the expected output regex with a greedy whitespace match.
            if ( ignoreWhiteSpace )
            {
                expectedOutputRegex = Regex.Replace( expectedOutputRegex, @"\s+", @"\s*" );
            }
            
            var regex = new Regex( expectedOutputRegex );

            WriteTemplateRenderToDebug( inputTemplate, outputString );
            StringAssert.Matches( outputString, regex );
        }

        /// <summary>
        /// Process the specified input template and verify the output against an expected DateTime result.
        /// </summary>
        /// <param name="expectedDateTime"></param>
        /// <param name="inputTemplate"></param>
        /// <param name="maximumDelta"></param>
        public void AssertTemplateOutputDate( DateTime? expectedDateTime, string inputTemplate, TimeSpan? maximumDelta = null )
        {
            var outputString = GetTemplateOutput( inputTemplate );

            WriteTemplateRenderToDebug( inputTemplate, outputString );

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
}
