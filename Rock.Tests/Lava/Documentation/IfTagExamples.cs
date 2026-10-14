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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Documentation
{
    /// <summary>
    /// Tests to verify examples provided in the Lava documentation for the if tag.
    /// </summary>
    /// <remarks>
    /// The examples that read a person attribute stay in Rock.Tests.Integration,
    /// because they seed an attribute value and read it back through the database.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class IfTagExamples
    {
        [TestMethod]
        public void IfTag_DocumentationExample_TestingIfPropertyExists()
        {
            var input = """
                {% if Person.CallSign %}
                    {{ Person.FullName }} you have a call sign... you must be cool!
                {% else %}
                    Oh... hi {{ Person.FullName }}
                {% endif %}
                """;

            var personNoCallSign = new
            {
                FullName = "Ted Decker"
            };

            var personWithCallSign = new
            {
                FullName = "Cindy Decker",
                CallSign = "C.D."
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Test the documentation example.
                var output = LavaRenderTestHelper.Render( engine, input, OptionsForPerson( personNoCallSign ) );

                Assert.AreEqual( "\n    Oh... hi Ted Decker\n", output );

                // Test the inverse case of the example.
                output = LavaRenderTestHelper.Render( engine, input, OptionsForPerson( personWithCallSign ) );

                Assert.AreEqual( "\n    Cindy Decker you have a call sign... you must be cool!\n", output );
            } );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_TestingForEmptyProperty()
        {
            var input = """
                {% if Person.MiddleName == '' %}
                    {{ Person.FullName }}, what no middle name?!
                {% endif %}
                """;

            var person = new
            {
                FullName = "Ted Decker",
                MiddleName = ""
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, OptionsForPerson( person ) );

                Assert.AreEqual( "\n    Ted Decker, what no middle name?!\n", output );
            } );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_TestingForEmptyArray()
        {
            var input = """
                {% if Person.PhoneNumbers != empty %}
                    You have phone numbers
                {% endif %}
                """;

            var personWithNumbers = new
            {
                FullName = "Cindy Decker",
                PhoneNumbers = new string[] { "1234567890" }
            };

            var personWithoutNumbers = new
            {
                FullName = "Ted Decker",
                PhoneNumbers = new string[] { }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, OptionsForPerson( personWithNumbers ) );

                Assert.AreEqual( "\n    You have phone numbers\n", output );

                output = LavaRenderTestHelper.Render( engine, input, OptionsForPerson( personWithoutNumbers ) );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        [TestMethod]
        public void IfTag_DocumentationExample_OrderOfLogicalOperations()
        {
            var input = """
                {% if true or false and false %}
                  This evaluates to true, since the 'and' condition is checked first.
                {% endif %}
                """;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "\n  This evaluates to true, since the 'and' condition is checked first.\n", output );
            } );
        }

        /// <summary>
        /// Builds render options exposing the supplied object as the Person
        /// context variable.
        /// </summary>
        /// <param name="person">The object to expose.</param>
        /// <returns>The render options.</returns>
        private static LavaRenderOptions OptionsForPerson( object person )
        {
            return new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "Person", person } }
            };
        }
    }
}
