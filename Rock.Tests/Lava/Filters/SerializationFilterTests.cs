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
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class SerializationFilterTests
    {
        /// <summary>
        /// The filter should accept a Person object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void ToJSON_ForDynamicObject_ProducesJsonString()
        {
            var person = LavaTestData.GetTestPersonTedDecker();

            var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

            var personJson = person.ToJson( indentOutput: true );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ CurrentPerson | ToJSON }}", options );

                // Render normalizes line endings to a line feed; ToJson uses Environment.NewLine.
                Assert.AreEqual( personJson.NormalizeLineEndings(), output );
            } );
        }

        /// <summary>
        /// The filter should accept a Person object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void ToJSON_ForTestArray_ProducesJsonString()
        {
            var numbers = new int[] { 1, 2, 3 };

            var mergeValues = new LavaDataDictionary { { "Numbers", numbers } };

            var numbersJson = numbers.ToJson( indentOutput: true );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ Numbers | ToJSON }}", options );

                // Render normalizes line endings to a line feed; ToJson uses Environment.NewLine.
                Assert.AreEqual( numbersJson.NormalizeLineEndings(), output );
            } );
        }

        /// <summary>
        /// The filter should accept a Person object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void FromJSON_ForTestPersonObject_ProducesJsonObject()
        {
            var person = LavaTestData.GetTestPersonTedDecker();

            var jsonString = person.ToJson();

            var mergeValues = new LavaDataDictionary { { "JsonString", jsonString } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign jsonObject = JsonString | FromJSON %}{{ jsonObject.NickName }} {{ jsonObject.LastName }} - {{ jsonObject.Campus.Name }}", options );

                Assert.AreEqual( "Ted Decker - North Campus", output );
            } );
        }

        /// <summary>
        /// The filter should accept a dictionary object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void ToJSON_ForDictionary_ProducesJsonString()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "FirstName", "Ted" },
                { "LastName", "Decker" }
            };

            var mergeValues = new LavaDataDictionary { { "Dictionary", dictionary } };

            var dictionaryJson = dictionary.ToJson( indentOutput: true );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ Dictionary | ToJSON }}", options );

                // Render normalizes line endings to a line feed; ToJson uses Environment.NewLine.
                Assert.AreEqual( dictionaryJson.NormalizeLineEndings(), output );
            } );
        }
    }
}
