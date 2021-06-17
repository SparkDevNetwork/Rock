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
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{


    [TestClass]
    public class CollectionFilterTests : LavaUnitTestBase
    {
        List<string> _TestNameList = new List<string>() { "Ted", "Alisha", "Cynthia", "Brian" };
        List<string> _TestOrderedList = new List<string>() { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };
        List<string> _TestDuplicateStringList = new List<string>() { "Item 1", "Item 2 (duplicate)", "Item 2 (duplicate)", "Item 2 (duplicate)", "Item 3" };

        #region Filter Tests: Distinct

        [TestMethod]
        public void Distinct_OnStringCollectionWithTwoDuplicateEntries_RemovesTwoDuplicateEntries()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestDuplicateStringList } };

            var lavaTemplate = "{{ TestList | Distinct | Join:',' }}";

            TestHelper.AssertTemplateOutput( "Item 1,Item 2 (duplicate),Item 3", lavaTemplate, mergeValues );
        }

        [TestMethod]
        public void Distinct_OnObjectPropertyWithMultipleValues_ReturnsFirstValueOnly()
        {
            var personList = TestHelper.GetTestPersonCollectionForDeckerAndMarble();
            var mergeValues = new LavaDataDictionary { { "PersonList", personList } };

            var lavaTemplate = @"
{% assign distinctPersonList = PersonList | Distinct:'LastName' %}
{% for person in distinctPersonList %}
{{ person.NickName }} {{ person.LastName }}<br>
{% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            // Only the first person of each family in the collection should be returned.
            TestHelper.AssertTemplateOutput( "TedDecker<br>BillMarble<br>", lavaTemplate, mergeValues, ignoreWhitespace:true );
        }

        #endregion

        #region Filter: GroupBy

        [TestMethod]
        public void GroupBy_OnStringCollectionWithDuplicates_RemovesDuplicates()
        {
            var personList = TestHelper.GetTestPersonCollectionForDeckerAndMarble();
            var mergeValues = new LavaDataDictionary { { "PersonList", personList } };

            var lavaTemplate = @"
{% assign groupedPersonList = PersonList | GroupBy:'LastName' %}
{% for member in groupedPersonList %}
    {% assign parts = member | PropertyToKeyValue %}
    <li>{{ parts.Key }}</li>
    <ul>
        {% for m in parts.Value %}
            {{ m.FirstName }}<br>
        {% endfor %}
    </ul>
{% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            TestHelper.AssertTemplateOutput( "<li>Decker</li><ul>Edward<br>Cindy<br>Noah<br>Alex<br></ul><li>Marble</li><ul>William<br>Alisha<br></ul>", lavaTemplate, mergeValues, ignoreWhitespace: true );
        }

        [TestMethod]
        public void AddToArray_AddToStringCollection_AppendsNewItem()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestDuplicateStringList } };

            var lavaTemplate = @"
        {% assign array = '' | AddToArray:'one' %}
        {% assign array = array | AddToArray:'two' | AddToArray:'three' %}
        {% for item in array %}
            {{ item }}<br>
        {% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            TestHelper.AssertTemplateOutput( "one<br>two<br>three<br>", lavaTemplate, mergeValues, ignoreWhitespace: true );
        }

        [TestMethod]
        public void RemoveFromArray_AppliedToStringCollectionWithNoMatchingEntries_RemovesNoEntries()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestOrderedList } };

            var lavaTemplate = @"
        {% assign array = TestList | RemoveFromArray:'Item 0' %}
        {% for item in array %}
            {{ item }}<br>
        {% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            TestHelper.AssertTemplateOutput( "Item 1<br>Item 2<br>Item 3<br>Item 4<br>Item 5<br>", lavaTemplate, mergeValues, ignoreWhitespace: true );
        }

        [TestMethod]
        public void RemoveFromArray_AppliedToStringCollectionWithDuplicateEntries_RemovesAllMatchingEntries()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestDuplicateStringList } };

            var lavaTemplate = @"
        {% assign array = TestList | RemoveFromArray:'Item 2 (duplicate)' %}
        {% for item in array %}
            {{ item }}<br>
        {% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            TestHelper.AssertTemplateOutput( "Item 1<br>Item 3<br>", lavaTemplate, mergeValues, ignoreWhitespace: true );
        }

        [TestMethod]
        public void Sum_AppliedToArrayOfIntegers_ReturnsSumOfIntegers()
        {
            var lavaTemplate = @"
Total: {{ '3,5,7' | Split:',' | Sum }}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            TestHelper.AssertTemplateOutput( "Total:15", lavaTemplate, null, ignoreWhitespace: true );
        }

        #endregion

        #region Filter Tests: Dictionaries

        [TestMethod]
        public void AddToDictionary_AddMultipleKeyValuePairs_ReturnsUpdatedDictionary()
        {
            var lavaTemplate = @"
        {% assign dict = '' | AddToDictionary:'key1','value2' %}
        {% assign dict = dict | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        {{ dict | AllKeysFromDictionary }}
";

            TestHelper.AssertTemplateOutput( "key1key2key3", lavaTemplate, null, ignoreWhitespace: true );
        }

        [TestMethod]
        public void RemoveFromDictionary_AppliedToArrayOfIntegers_ReturnsCorrectSum()
        {
            var lavaTemplate = @"
{% assign dict = '' | AddToDictionary:'key1','value2' | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
{% assign dict = dict | RemoveFromDictionary:'key2' %}
{{ dict | AllKeysFromDictionary }}
";

            TestHelper.AssertTemplateOutput( "key1key3", lavaTemplate, null, ignoreWhitespace: true );
        }

        [TestMethod]
        public void AllKeysFromDictionary_AppliedToDictionary_ReturnsArrayOfKeys()
        {
            var lavaTemplate = @"
{% assign dict = '' | AddToDictionary:'key1','value2' | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
{{ dict | AllKeysFromDictionary }}
";

            TestHelper.AssertTemplateOutput( "key1key2key3", lavaTemplate, null, ignoreWhitespace: true );
        }

        #endregion

        /// <summary>
        /// Searching for strings in a collection returns correct match indicators.
        /// </summary>
        [DataTestMethod]
        [DataRow( "Ted", true )]
        [DataRow( "Brian", true )]
        [DataRow( "Cynthia", true )]
        [DataRow( "Zak", false )]
        public void Contains_SearchStringValuesReturnCorrectMatchIndicators( string searchValue, bool isFound )
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            var lavaTemplate = "{{ TestList | Contains:'<searchValue>' }}";
            lavaTemplate = lavaTemplate.Replace( "<searchValue>", searchValue );

            TestHelper.AssertTemplateOutput( isFound ? "true" : "false", lavaTemplate, mergeValues );
        }

        #region Filter Tests: Index

        /// <summary>
        /// Specifying an index of 0 returns the first item in the collection.
        /// </summary>
        [DataTestMethod]
        [DataRow( 0, "Item 1" )]
        [DataRow( 1, "Item 2" )]
        [DataRow( 2, "Item 3" )]
        public void Index_IndexReferencesReturnExpectedItems( int index, string expectedValue )
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestOrderedList } };

            var lavaTemplate = "{{ TestList | Index:<index> }}";
            lavaTemplate = lavaTemplate.Replace( "<index>", index.ToString() );

            TestHelper.AssertTemplateOutput( expectedValue, lavaTemplate, mergeValues );

        }

        /// <summary>
        /// Specifying an index greater than the number of list items returns an empty string.
        /// </summary>
        [TestMethod]
        public void Index_IndexExceedsItemCount_ProducesEmptyString()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            TestHelper.AssertTemplateOutput( "", "{{ TestList | Index:999 }}", mergeValues );
        }

        #endregion

        #region Filter Tests: OrderBy

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByMultipleTextProperties()
        {
            var members = new List<object>();

            members.Add( new
            {
                GroupRole = new { Name = "Member", IsLeader = false },
                Person = new { FirstName = "Alex" }
            } );
            members.Add( new
            {
                GroupRole = new { Name = "Leader", IsLeader = true },
                Person = new { FirstName = "Ted" }
            } );
            members.Add( new
            {
                GroupRole = new { Name = "Member", IsLeader = false },
                Person = new { FirstName = "Cindy" }
            } );

            var mergeValues = new LavaDataDictionary { { "Members", members } };

            TestHelper.AssertTemplateOutput( "Ted;Alex;Cindy;",
                "{% assign items = Members | OrderBy:'GroupRole.IsLeader desc,Person.FirstName' %}{% for item in items %}{{ item.Person.FirstName }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyAscending()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } } ;

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyDescending()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "D;C;B;A;",
                "{% assign items = Items | OrderBy:'Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByMultipleObjectProperties()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order, SecondOrder DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByNestedObjectProperty()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order, Nested.Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        private List<ExpandoObject> GetOrderByTestCollection()
        {
            var json = @"[
    { ""Title"": ""D"", ""Order"": 4, ""SecondOrder"": 1, ""Nested"": { ""Order"": 1 } },
    { ""Title"": ""A"", ""Order"": 1, ""SecondOrder"": 2, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""C"", ""Order"": 3, ""SecondOrder"": 2, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""B"", ""Order"": 2, ""SecondOrder"": 1, ""Nested"": { ""Order"": 1 } }
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );

            return input;
        }

        #endregion

        /// <summary>
        /// Shuffle applied to an ordered list returns an unordered list.
        /// </summary>
        [TestMethod]
        public void Shuffle_AppliedToOrderedList_ReturnsUnorderedList()
        {
            var orderedOutput = _TestOrderedList.JoinStrings( ";" ) + ";";

            TestHelper.ExecuteTestAction( ( engine ) =>
            {
                // Add a copy of the test list to the context, as it will be modified during the rendering process.
                var mergeValues = new LavaDataDictionary { { "OrderedList", new List<string>( _TestOrderedList ) } };

                // First, verify that the unshuffled lists are equal.
                var orderedResult = TestHelper.GetTemplateOutput( engine.EngineType, "{% assign items = OrderedList %}{% for item in items %}{{ item }};{% endfor %}", mergeValues );

                Assert.That.Equal( orderedOutput, orderedResult );

                // Next, verify that the shuffled lists are not equal.
                // The Shuffle filter can, mathmatically, actually return the same ordered result.
                // To offset this, attempt the shuffle 10 times. If all 10 times we still get the same
                // ordered result back, then go ahead and error as something must be wrong. -dsh
                // [2020-05-05] DJL
                // Perhaps we should fix the Shuffle implementation instead? Is it ever desirable to return an unshuffled result if shuffling is at all possible?
                string shuffledResult = string.Empty;
                for ( int i = 0; i < 10; i++ )
                {
                    shuffledResult = TestHelper.GetTemplateOutput( engine.EngineType, "{% assign items = OrderedList | Shuffle %}{% for item in items %}{{ item }};{% endfor %}", mergeValues );

                    if ( orderedOutput != shuffledResult )
                    {
                        break;
                    }
                }

                Assert.That.NotEqual( orderedOutput, shuffledResult );
            } );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Select_ValidItemPropertyFromItemCollection_ReturnsValueCollection()
        {
            LavaDataDictionary mergeValues;

            //if ( TestHelper.LavaEngine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            //{
            //    mergeValues = new LavaDataDictionary { { "People", TestHelper.GetTestPersonCollectionForDeckerRockLiquid() } };
            //}
            //else
            //{
                mergeValues = new LavaDataDictionary { { "People", TestHelper.GetTestPersonCollectionForDecker() } };
            //}

            TestHelper.AssertTemplateOutput( "Edward;Cindy;Noah;Alex;",
                "{% assign names = People | Select:'FirstName' %}{% for name in names %}{{ name }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForArrayTarget_ReturnsItemCount()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            TestHelper.AssertTemplateOutput( _TestNameList.Count.ToString(), "{{ TestList | Size }}", mergeValues );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForStringTarget_ReturnsCharacterCount()
        {
            var testString = "123456789";

            var mergeValues = new LavaDataDictionary { { "TestString", testString } };

            TestHelper.AssertTemplateOutput( testString.Length.ToString(), "{{ TestString | Size }}", mergeValues );
        }
    }
}
