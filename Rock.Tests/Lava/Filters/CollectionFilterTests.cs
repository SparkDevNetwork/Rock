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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Lava.Shared;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class CollectionFilterTests
    {
        List<string> _TestNameList = new List<string>() { "Ted", "Alisha", "Cynthia", "Brian" };
        List<string> _TestOrderedList = new List<string>() { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };
        List<string> _TestDuplicateStringList = new List<string>() { "Item 1", "Item 2 (duplicate)", "Item 2 (duplicate)", "Item 2 (duplicate)", "Item 3" };

        [TestMethod]
        public void Compact_DocumentationExample_ProducesExpectedOutput()
        {
            var template = @"
{% assign fruits = '' | AddToArray:'apples' | AddToArray:nil | AddToArray:'oranges' | AddToArray:nil | AddToArray:'peaches' %}
Whole Fruit: {{ fruits | Join:', ' }}
{% assign squashedFruits = fruits | Compact %}
Squashed Fruit: {{ squashedFruits | Join:', ' }}
";
            var expectedOutput = "\n\nWhole Fruit: apples, , oranges, , peaches\n\nSquashed Fruit: apples, oranges, peaches\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void Compact_ForArrayWithNullValues_RemovesNullValues()
        {
            var names = new List<string>() { null, "Alisha", "Brian", null, "Cynthia" };
            var mergeValues = new LavaDataDictionary { { "TestList", names } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestList | Compact | Join:',' }}", options );

                Assert.AreEqual( "Alisha,Brian,Cynthia", output );
            } );
        }

        [TestMethod]
        public void Concat_DocumentationExample_ProducesExpectedOutput()
        {
            var template = @"
{% assign primaryColors = 'red, yellow, blue' | Split: ', ' %}
{% assign secondaryColors = 'orange, green, violet' | Split: ', ' %}
{% assign allColors = primaryColors | Concat: secondaryColors %}
{{ allColors | Join:', ' }}
";
            var expectedOutput = "\n\n\n\nred, yellow, blue, orange, green, violet\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void Concat_WithMultipleArrayInput_ProducesSingleArray()
        {
            var template = @"
{% assign fruits = 'apples, oranges, peaches' | Split: ', ' %}
{% assign vegetables = 'carrots, turnips, potatoes' | Split: ', ' %}
{% assign everything = fruits | Concat: vegetables %}
{% for item in everything %}
{{ item }},
{% endfor %}
";
            var expectedOutput = "\n\n\n\n\napples,\n\noranges,\n\npeaches,\n\ncarrots,\n\nturnips,\n\npotatoes,\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #region Filter Tests: Distinct

        [TestMethod]
        public void Distinct_OnStringCollectionWithTwoDuplicateEntries_RemovesTwoDuplicateEntries()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestDuplicateStringList } };

            var lavaTemplate = "{{ TestList | Distinct | Join:',' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "Item 1,Item 2 (duplicate),Item 3", output );
            } );
        }

        [TestMethod]
        public void Distinct_OnObjectPropertyWithMultipleValues_ReturnsFirstValueOnly()
        {
            var personList = LavaTestData.GetTestPersonCollectionForDeckerAndMarble();
            var mergeValues = new LavaDataDictionary { { "PersonList", personList } };

            var lavaTemplate = @"
{% assign distinctPersonList = PersonList | Distinct:'LastName' %}
{% for person in distinctPersonList %}
{{ person.NickName }} {{ person.LastName }}<br>
{% endfor %}
";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            // Only the first person of each family in the collection should be returned.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "\n\n\nTed Decker<br>\n\nBill Marble<br>\n\n", output );
            } );
        }

        [TestMethod]
        public void Distinct_IncludedInFilterChain_ProvidesValidInputToNextFilter()
        {
            var lavaTemplate = @"
{% assign testArray = '2,1,3,5,4,2' | Split:',' %}
{{ testArray  | Distinct | Contains:'2' }}
";
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( "\n\ntrue\n", output );
            } );
        }

        #endregion

        #region Filter: GroupBy

        [TestMethod]
        public void GroupBy_OnStringCollectionWithDuplicates_RemovesDuplicates()
        {
            var personList = LavaTestData.GetTestPersonCollectionForDeckerAndMarble();
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "\n\n\n    \n    <li>Decker</li>\n    <ul>\n        \n            Edward<br>\n        \n            Cindy<br>\n        \n            Noah<br>\n        \n            Alex<br>\n        \n    </ul>\n\n    \n    <li>Marble</li>\n    <ul>\n        \n            William<br>\n        \n            Alisha<br>\n        \n    </ul>\n\n", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "\n        \n        \n        \n            one<br>\n        \n            two<br>\n        \n            three<br>\n        \n", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "\n        \n        \n            Item 1<br>\n        \n            Item 2<br>\n        \n            Item 3<br>\n        \n            Item 4<br>\n        \n            Item 5<br>\n        \n", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "\n        \n        \n            Item 1<br>\n        \n            Item 3<br>\n        \n", output );
            } );
        }

        [TestMethod]
        [DataRow( "3,5,7", "15" )]
        [DataRow( "3.1,5.1,7.1", "15.3" )]
        [DataRow( "3.1,5.01,7.001", "15.111" )]
        [DataRow( "A,B,C", "0" )]
        public void Sum_AppliedToArrayOfNumbers_ReturnsSumOfValues( string values, string sumValue )
        {
            var lavaTemplate = $"Total: {{{{ '{values}' | Split:',' | Sum }}}}";

            lavaTemplate = lavaTemplate.Replace( "`", "\"" );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( $"Total: {sumValue}", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( "\n        \n        \n        key1key2key3\n", output );
            } );
        }

        [TestMethod]
        public void AddToDictionary_AddExistingKey_ReplacesExistingValue()
        {
            var lavaTemplate = @"
        {% assign dict = '' | AddToDictionary:'key1','value1' %}
        {% assign dict = dict | AddToDictionary:'key1','value2' %}
        {{ dict | ToJSON }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( "\n        \n        \n        {\n  \"key1\": \"value2\"\n}\n", output );
            } );
        }

        [TestMethod]
        public void AddToDictionary_DocumentationExample1_ProducesExpectedOutput()
        {
            var lavaTemplate = @"
{% assign colors = '' | AddToDictionary:'success','green' | AddToDictionary:'warning','orange' | AddToDictionary:'error','red' %}
<div style='color:{{ colors[""success""]}}'>
    This request is approved.
</div>
<div style='color:{{ colors[""warning""]}}'>
    This request is incomplete.
</div>
<div style='color:{{ colors[""error""]}}'>
    This request is denied.
</div>
";
            var expectedOutput = "\n\n<div style='color:green'>\n    This request is approved.\n</div>\n<div style='color:orange'>\n    This request is incomplete.\n</div>\n<div style='color:red'>\n    This request is denied.\n</div>\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void RemoveFromDictionary_RemoveExistingKey_ReturnsUpdatedDictionary()
        {
            var lavaTemplate = @"
{% assign dict = '' | AddToDictionary:'key1','value2' | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
{% assign dict = dict | RemoveFromDictionary:'key2' %}
{{ dict | AllKeysFromDictionary }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( "\n\n\nkey1key3\n", output );
            } );
        }

        [TestMethod]
        public void AllKeysFromDictionary_AppliedToDictionary_ReturnsArrayOfKeys()
        {
            var lavaTemplate = @"
{% assign dict = '' | AddToDictionary:'key1','value2' | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
{{ dict | AllKeysFromDictionary }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate );

                Assert.AreEqual( "\n\nkey1key2key3\n", output );
            } );
        }

        #endregion

        /// <summary>
        /// Searching for strings in a collection returns correct match indicators.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted", true )]
        [DataRow( "Brian", true )]
        [DataRow( "Cynthia", true )]
        [DataRow( "Zak", false )]
        public void Contains_SearchStringValuesReturnCorrectMatchIndicators( string searchValue, bool isFound )
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            var lavaTemplate = "{{ TestList | Contains:'<searchValue>' }}";
            lavaTemplate = lavaTemplate.Replace( "<searchValue>", searchValue );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( isFound ? "true" : "false", output );
            } );
        }

        [TestMethod]
        public void Contains_WithInputAsEnumerable_ReturnCorrectMatchIndicators()
        {
            var mergeValues = new LavaDataDictionary();
            var lavaTemplate = @"{{ TestEnumerable | Contains:'2' }}";

            var itemList = new List<string>() { "1", "2", "3" };

            // IEnumerable collection.
            var enumerableCollection = new Stack( itemList );
            mergeValues["TestEnumerable"] = enumerableCollection;

            Assert.IsTrue( enumerableCollection is IEnumerable
                && !( enumerableCollection is IEnumerable<object> )
                && !( enumerableCollection is IList ) );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "true", output );
            } );

            // IEnumerable<> collection.
            var queueCollection = new Queue<string>( itemList );
            mergeValues["TestEnumerable"] = queueCollection;

            Assert.IsTrue( queueCollection is IEnumerable<string>
                && !( queueCollection is IList ) );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "true", output );
            } );

            // IList collection.
            mergeValues["TestEnumerable"] = itemList;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "true", output );
            } );
        }

        [TestMethod]
        public void Contains_WithInputAsList_ReturnCorrectMatchIndicators()
        {
            var mergeValues = new LavaDataDictionary();
            var lavaTemplate = @"{{ TestEnumerable | Contains:'2' }}";

            var itemList = new List<string>() { "1", "2", "3" };

            // IList collection.
            var itemArray = new ArrayList( itemList );
            mergeValues["TestEnumerable"] = itemArray;

            Assert.IsTrue( itemArray is IList
                && !( itemArray is IList<string> ) );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "true", output );
            } );

            // IList<T> collection.
            mergeValues["TestEnumerable"] = itemList;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( "true", output );
            } );
        }

        #region Filter Tests: Index

        /// <summary>
        /// Specifying an index of 0 returns the first item in the collection.
        /// </summary>
        [TestMethod]
        [DataRow( 0, "Item 1" )]
        [DataRow( 1, "Item 2" )]
        [DataRow( 2, "Item 3" )]
        public void Index_IndexReferencesReturnExpectedItems( int index, string expectedValue )
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestOrderedList } };

            var lavaTemplate = "{{ TestList | Index:<index> }}";
            lavaTemplate = lavaTemplate.Replace( "<index>", index.ToString() );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, lavaTemplate, options );

                Assert.AreEqual( expectedValue, output );
            } );

        }

        /// <summary>
        /// Specifying an index greater than the number of list items returns an empty string.
        /// </summary>
        [TestMethod]
        public void Index_IndexExceedsItemCount_ProducesEmptyString()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestList | Index:999 }}", options );

                Assert.AreEqual( "", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign items = Members | OrderBy:'GroupRole.IsLeader desc,Person.FirstName' %}{% for item in items %}{{ item.Person.FirstName }};{% endfor %}", options );

                Assert.AreEqual( "Ted;Alex;Cindy;", output );
            } );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyAscending()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign items = Items | OrderBy:'Order' %}{% for item in items %}{{ item.Title }};{% endfor %}", options );

                Assert.AreEqual( "A;B;C;D;", output );
            } );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyDescending()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign items = Items | OrderBy:'Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}", options );

                Assert.AreEqual( "D;C;B;A;", output );
            } );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByMultipleObjectProperties()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign items = Items | OrderBy:'Order, SecondOrder DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}", options );

                Assert.AreEqual( "A;B;C;D;", output );
            } );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByNestedObjectProperty()
        {
            var mergeValues = new LavaDataDictionary { { "Items", GetOrderByTestCollection() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign items = Items | OrderBy:'Order, Nested.Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}", options );

                Assert.AreEqual( "A;B;C;D;", output );
            } );
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

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Add a copy of the test list to the context, as it will be modified during the rendering process.
                var mergeValues = new LavaDataDictionary { { "OrderedList", new List<string>( _TestOrderedList ) } };

                // First, verify that the unshuffled lists are equal.
                var orderedResult = LavaRenderTestHelper.Render( engine, "{% assign items = OrderedList %}{% for item in items %}{{ item }};{% endfor %}", new LavaRenderOptions { MergeFields = mergeValues } );

                Assert.AreEqual( orderedOutput, orderedResult );

                // Next, verify that the shuffled lists are not equal.
                // The Shuffle filter can, mathmatically, actually return the same ordered result.
                // To offset this, attempt the shuffle 10 times. If all 10 times we still get the same
                // ordered result back, then go ahead and error as something must be wrong. -dsh
                // [2020-05-05] DJL
                // Perhaps we should fix the Shuffle implementation instead? Is it ever desirable to return an unshuffled result if shuffling is at all possible?
                string shuffledResult = string.Empty;
                for ( int i = 0; i < 10; i++ )
                {
                    shuffledResult = LavaRenderTestHelper.Render( engine, "{% assign items = OrderedList | Shuffle %}{% for item in items %}{{ item }};{% endfor %}", new LavaRenderOptions { MergeFields = mergeValues } );

                    if ( orderedOutput != shuffledResult )
                    {
                        break;
                    }
                }

                Assert.AreNotEqual( orderedOutput, shuffledResult );
            } );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Select_ItemPropertyFromLavaDataDictionaryCollection_ReturnsValue()
        {
            // The Select filter should work correctly on any collection of objects that supports the
            // ILavaDataDictionary interface. This includes objects that inherit from LavaDataObject,
            // or are proxied using LavaDataObject.
            var mergeValues = new LavaDataDictionary { { "People", LavaTestData.GetTestPersonCollectionForDecker() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign names = People | Select:'FirstName' %}{% for name in names %}{{ name }};{% endfor %}", options );

                Assert.AreEqual( "Edward;Cindy;Noah;Alex;", output );
            } );
        }

        /// <summary>
        /// Selecting a specific key-value pair from a collection of dictionaries returns a list of values.
        /// </summary>
        [TestMethod]
        public void Select_KeyValueFromDictionaryCollection_ReturnsListOfValues()
        {
            // The Select filter should work correctly on any collection of objects that supports the
            // IDictionary<string,object> interface.
            var dictionary1 = new Dictionary<string, object>()
            {
                { "Key1", "Value1-1" },
                { "Key2", "Value1-2" },
                { "Key3", "Value1-3" },
            };
            var dictionary2 = new Dictionary<string, object>()
            {
                { "Key1", "Value2-1" },
                { "Key2", "Value2-2" },
                { "Key3", "Value2-3" },
            };

            var mergeValues = new LavaDataDictionary { { "Dictionaries", new List<Dictionary<string, object>> { dictionary1, dictionary2 } } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{% assign values = Dictionaries | Select:'Key2' %}{% for value in values %}{{ value }};{% endfor %}", options );

                Assert.AreEqual( "Value1-2;Value2-2;", output );
            } );
        }


        /// <summary>
        /// The Sort filter is case-sensitive, sorting alphabetically with uppercase before lowercase values.
        /// </summary>
        [TestMethod]
        public void Sort_ForArrayTarget_IsCaseSensitive()
        {
            var unsortedNames = new List<string>() { "Ted", "brian", "Cynthia", "alisha" };
            var mergeValues = new LavaDataDictionary { { "TestList", unsortedNames } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestList | Sort | Join:',' }}", options );

                Assert.AreEqual( "Cynthia,Ted,alisha,brian", output );
            } );
        }

        /// <summary>
        /// The SortNatural filter is case-insensitive, sorting alphabetically without regard to case.
        /// </summary>
        [TestMethod]
        public void SortNatural_ForArrayTarget_IsCaseSensitive()
        {
            var unsortedNames = new List<string>() { "Ted", "brian", "Cynthia", "alisha" };
            var mergeValues = new LavaDataDictionary { { "TestList", unsortedNames } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestList | SortNatural | Join:',' }}", options );

                Assert.AreEqual( "alisha,brian,Cynthia,Ted", output );
            } );

        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForArrayTarget_ReturnsItemCount()
        {
            var mergeValues = new LavaDataDictionary { { "TestList", _TestNameList } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestList | Size }}", options );

                Assert.AreEqual( _TestNameList.Count.ToString(), output );
            } );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForStringTarget_ReturnsCharacterCount()
        {
            var testString = "123456789";

            var mergeValues = new LavaDataDictionary { { "TestString", testString } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ TestString | Size }}", options );

                Assert.AreEqual( testString.Length.ToString(), output );
            } );
        }
    }
}
