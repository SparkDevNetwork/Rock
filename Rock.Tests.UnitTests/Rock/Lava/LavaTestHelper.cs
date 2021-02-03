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
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Lava
{
    public class LavaTestHelper
    {
        public static LavaTestHelper New( LavaEngineTypeSpecifier? engineType = null )
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                // Use the default Rock template provider for testing.
                CacheService = new LavaTemplateCache()
            };

            global::Rock.Lava.LavaEngine.Initialize( engineType, engineOptions );

            var engine = global::Rock.Lava.LavaEngine.CurrentEngine;

            // Register the common Rock.Lava filters first, then overwrite with the web-based RockFilters as needed.
            engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( global::Rock.Lava.RockFilters ) );

            var helper = new LavaTestHelper();

            return helper;
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

            bool isValidTemplate = global::Rock.Lava.LavaEngine.CurrentEngine.TryRender( inputTemplate.Trim(), out outputString, mergeValues );

            Assert.That.True( isValidTemplate, "Lava Template is invalid." );

            return outputString;
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output.
        /// </summary>
        /// <param name="expectedOutput"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutput( string expectedOutput, string inputTemplate, LavaDataDictionary mergeValues = null, bool ignoreWhitespace = false )
        {
            var outputString = GetTemplateOutput( inputTemplate, mergeValues );

            var debugString = outputString;

            if ( ignoreWhitespace )
            {
                expectedOutput = expectedOutput.RemoveWhiteSpace();
                outputString = outputString.RemoveWhiteSpace();

                debugString += "\n(Comparison ignores WhiteSpace)";
            }

            WriteOutputToDebug( debugString );

            Assert.That.Equal( expectedOutput, outputString );
        }

        /// <summary>
        /// Process the specified input template and verify against the expected output regular expression.
        /// </summary>
        /// <param name="expectedOutputRegex"></param>
        /// <param name="inputTemplate"></param>
        public void AssertTemplateOutputRegex( string expectedOutputRegex, string inputTemplate, LavaDataDictionary mergeValues = null )
        {
            var outputString = GetTemplateOutput( inputTemplate, mergeValues );

            var regex = new Regex( expectedOutputRegex );

            WriteOutputToDebug( outputString );
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

            DateTime outputDate;

            var isValidDate = DateTime.TryParse( outputString, out outputDate );

            WriteOutputToDebug( outputString );
            Assert.That.True( isValidDate, $"Template Output does not represent a valid DateTime. [Output=\"{ outputString }\"]" );

            if ( maximumDelta != null )
            {
                DateTimeAssert.AreEqual( expectedDateTime, outputDate, maximumDelta.Value );
            }
            else
            {
                DateTimeAssert.AreEqual( expectedDateTime, outputDate );
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

        /// <summary>
        /// Write the rendered template to debug output.
        /// </summary>
        /// <param name="outputString"></param>
        private void WriteOutputToDebug( string outputString )
        {
            var engineName = global::Rock.Lava.LavaEngine.CurrentEngine.EngineName;

            Debug.Print( $"\n**\n** Template Output ({engineName}):\n**\n{outputString}" );
        }

        #region Test Data

        /// <summary>
        /// Return an initialized Person object for test subject Ted Decker.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonTedDecker()
        {
            var campus = new TestCampus { Name = "North Campus", Id = 1 };
            var person = new TestPerson { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1 };

            return person;
        }

        /// <summary>
        /// Return an initialized Person object for test subject Alisha Marble.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonAlishaMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 102 };
            var person = new TestPerson { FirstName = "Alisha", NickName = "Alisha", LastName = "Marble", Campus = campus, Id = 2 };

            return person;
        }

        /// <summary>
        /// Return an initialized Person object for test subject Alisha Marble.
        /// </summary>
        /// <returns></returns>
        public TestPerson GetTestPersonBillMarble()
        {
            var campus = new TestCampus { Name = "South Campus", Id = 101 };
            var person = new TestPerson { FirstName = "William", NickName = "Bill", LastName = "Marble", Campus = campus, Id = 2 };

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

        /// <summary>
        /// Return a collection of initialized Person objects for the Decker family.
        /// </summary>
        /// <returns></returns>
        public List<TestPerson> GetTestPersonCollectionForDeckerAndMarble()
        {
            var personList = new List<TestPerson>();

            personList.Add( GetTestPersonTedDecker() );
            personList.Add( new TestPerson { FirstName = "Cindy", LastName = "Decker", Id = 2 } );
            personList.Add( new TestPerson { FirstName = "Noah", LastName = "Decker", Id = 3 } );
            personList.Add( new TestPerson { FirstName = "Alex", LastName = "Decker", Id = 4 } );

            personList.Add( GetTestPersonBillMarble() );
            personList.Add( GetTestPersonAlishaMarble() );

            return personList;
        }

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class TestPerson : LavaDataObject
        {
            public int Id { get; set; }
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

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class TestSecuredRockDynamicObject : RockDynamic
        {
            [LavaInclude]
            public int Id { get; set; }
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
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class TestSecuredLavaDataObject : LavaDataObject
        {
            [LavaInclude]
            public int Id { get; set; }
            public string NickName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public TestCampus Campus { get; set; }

            public override string ToString()
            {
                return $"{NickName} {LastName}";
            }
        }

        #endregion
    }
}
