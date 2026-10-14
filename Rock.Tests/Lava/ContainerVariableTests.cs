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

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests the accessibility of different container-type variables when resolving a Lava template.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class ContainerVariableTests
    {
        /// <summary>
        /// Referencing an existing dictionary entry with a string key type should return the dictionary value.
        /// </summary>
        [TestMethod]
        public void Dictionary_WithStringKey_ReturnsMatchingValueForKey()
        {
            var value = new Dictionary<string, string> { { "a", "One" }, { "b", "Two" }, { "c", "Three" } };

            var mergeValues = new LavaDataDictionary { { "StringDictionary", value } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ StringDictionary['b'] }}", options );

                Assert.AreEqual( "Two", output );
            } );
        }

        /// <summary>
        /// Referencing an existing dictionary entry with an integer key type should return the dictionary value.
        /// </summary>
        [TestMethod]
        public void Dictionary_WithIntegerKey_ReturnsMatchingValueForKey()
        {
            var value = new Dictionary<int, string> { { 1, "One" }, { 2, "Two" }, { 3, "Three" } };

            var mergeValues = new LavaDataDictionary { { "IntDictionary", value } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ IntDictionary[2] }}", options );

                Assert.AreEqual( "Two", output );
            } );
        }

        /// <summary>
        /// Referencing an existing dictionary entry with an Enum key type should return the dictionary value.
        /// </summary>
        [TestMethod]
        public void Dictionary_WithEnumKey_ReturnsMatchingValueForKey()
        {
            var value = new Dictionary<Gender, string>
            {
                { Gender.Unknown, "Unknown" },
                { Gender.Male, "Male" },
                { Gender.Female, "Female" }
            };

            var mergeValues = new LavaDataDictionary { { "EnumDictionary", value } };

            // This use case only works with Lava library implementations that use the LavaDataObject
            // as the default proxy wrapper. It exists here for test coverage, but is not used in any production Lava templates.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ EnumDictionary['Male'] }}", options );

                Assert.AreEqual( "Male", output );
            } );
        }

        /// <summary>
        /// Referencing an existing dictionary entry with an Enum key type should return the dictionary value.
        /// </summary>
        [TestMethod]
        public void LavaDataDictionary_WithKeysDifferingOnlyByCase_ReturnsMatchingValueForKey()
        {
            var mergeValues = new LavaDataDictionary()
            {
                { "case", "lower" },
                { "CASE", "upper" },
                { "Case", "mixed" }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ case }},{{ CASE }},{{ Case }}", options );

                Assert.AreEqual( "lower,upper,mixed", output );
            } );
        }
    }
}
