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
using Newtonsoft.Json;
using Rock.Lava;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class SerializationFilterTests : LavaUnitTestBase
    {
        /// <summary>
        /// The filter should accept a Person object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void ToJSON_ForDynamicObject_ProducesJsonString()
        {
            var person = TestHelper.GetTestPersonTedDecker();

            var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

            var personJson = person.ToJson( indentOutput: true );

            TestHelper.AssertTemplateOutput( personJson, "{{ CurrentPerson | ToJSON }}", mergeValues );
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

            TestHelper.AssertTemplateOutput( numbersJson, "{{ Numbers | ToJSON }}", mergeValues );
        }

        /// <summary>
        /// The filter should accept a Person object as input and return a valid JSON string.
        /// </summary>
        [TestMethod]
        public void FromJSON_ForTestPersonObject_ProducesJsonObject()
        {
            var person = TestHelper.GetTestPersonTedDecker();

            var jsonString = person.ToJson();

            var mergeValues = new LavaDataDictionary { { "JsonString", jsonString } };

            TestHelper.AssertTemplateOutput( "Ted Decker - North Campus",
                "{% assign jsonObject = JsonString | FromJSON %}{{ jsonObject.NickName }} {{ jsonObject.LastName }} - {{ jsonObject.Campus.Name }}",
                mergeValues );
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

            TestHelper.AssertTemplateOutput( dictionaryJson, "{{ Dictionary | ToJSON }}", mergeValues );
        }
    }
}
