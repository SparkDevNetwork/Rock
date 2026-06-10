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

using Rock.Lava;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    public class ObjectFilterTests : LavaUnitTestBase
    {
        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void Property_AnonymousObjectFirstLevelPropertyAccess_ReturnsPropertyValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", TestHelper.GetTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "Decker", "{{ CurrentPerson | Property:'LastName' }}", mergeValues );
        }

        /// <summary>
        /// Accessing a nested property using dot-notation "Campus.Name" should return the correct value.
        /// </summary>
        [TestMethod]
        public void Property_AnonymousObjectSecondLevelPropertyAccess_ReturnsValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", TestHelper.GetTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "North Campus", "{{ CurrentPerson | Property:'Campus.Name' }}", mergeValues );
        }

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void Property_InvalidPropertyName_ReturnsEmptyString()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", TestHelper.GetTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( string.Empty, "{{ CurrentPerson | Property:'NonexistentProperty' }}", mergeValues );
        }

        /// <summary>
        /// For testing the Where filter using the optional 'contains' equality parameter against
        /// a single object.
        /// </summary>
        [TestMethod]
        public void Where_ObjectWithSingleContainsConditionMatch_ReturnsContainedValue()
        {
            var singlePocoObject = new List<object>
            {
                new
                {
                    Number = "6235558888",
                    NumberFormatted = "(623) 555-8888"
                }
            };

            var mergeValues = new LavaDataDictionary { { "Item", singlePocoObject } };

            var templateInput = @"{%- assign matches = Item | Where:'NumberFormatted','55-88','contains' %}
{%- for match in matches %}
{{- match.Number }}<br>
{%- endfor %}";

            var expectedOutput = @"6235558888<br>";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, mergeValues );
        }

        /// <summary>
        /// For testing the Where filter using the optional 'contains' equality parameter against
        /// a dictionary with of objects.
        /// </summary>
        [TestMethod]
        public void Where_DictionaryWithSingleContainsConditionHavingOneMatch_ReturnsContainedValue()
        {
            var items = new List<Dictionary<string, object>>
                {
                   new Dictionary<string, object> { { "Id", "11" } },
                   new Dictionary<string, object> { { "Id", "22" } },
                   new Dictionary<string, object> { { "Id", "33" } }
                };

            var mergeValues = new LavaDataDictionary { { "Items", items } };

            var templateInput = @"{%- assign matches = Items | Where:'Id','2','contains' %}
{%- for match in matches %}
{{- match.Id }}<br>
{%- endfor %}";

            var expectedOutput = @"22<br>";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, mergeValues );
        }

        /// <summary>
        /// Reproduces a regression introduced when the 'contains' option was added to the Where
        /// filter (see commit 28da0262). When applied to a collection of dictionaries that
        /// contains at least one item whose filter property value is null, the filter would throw
        /// an exception, but it should simply skip the null-valued item.
        /// </summary>
        [TestMethod]
        public void Where_DictionaryCollectionWithNullPropertyValueAndEqualComparison_ReturnsMatchingItemsWithoutError()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Int", 1 }, { "GroupingId", 4 } },
                new Dictionary<string, object> { { "Int", 2 }, { "GroupingId", 5 } },
                new Dictionary<string, object> { { "Int", 3 }, { "GroupingId", null } }
            };

            var mergeValues = new LavaDataDictionary { { "Items", items } };

            var templateInput = @"{%- assign matches = Items | Where:'GroupingId',5 %}
{%- for match in matches %}
{{- match.Int }};
{%- endfor %}";

            // Only the dictionary whose GroupingId equals 5 should be returned; the null-valued
            // item must be skipped without throwing.
            TestHelper.AssertTemplateOutput( "2;", templateInput, mergeValues, ignoreWhitespace: true );
        }

        /// <summary>
        /// The Where filter must not throw when a dictionary collection contains a null-valued
        /// property and the 'notequal' comparison is used.
        /// </summary>
        [TestMethod]
        public void Where_DictionaryCollectionWithNullPropertyValueAndNotEqualComparison_DoesNotThrow()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Int", 1 }, { "GroupingId", 4 } },
                new Dictionary<string, object> { { "Int", 2 }, { "GroupingId", 5 } },
                new Dictionary<string, object> { { "Int", 3 }, { "GroupingId", null } }
            };

            var mergeValues = new LavaDataDictionary { { "Items", items } };

            var templateInput = @"{%- assign matches = Items | Where:'GroupingId',5,'notequal' %}
{%- for match in matches %}
{{- match.Int }};
{%- endfor %}";

            // Both the GroupingId=4 and the GroupingId=null items are "not equal" to 5 and
            // should be returned. (The null-valued item was previously throwing, aborting the
            // entire template.) GroupingId=5 is the only item excluded.
            TestHelper.AssertTemplateOutput( "1;3;", templateInput, mergeValues, ignoreWhitespace: true );
        }

        /// <summary>
        /// The Where filter must not throw when a dictionary collection contains a null-valued
        /// property and the 'contains' comparison is used.
        /// </summary>
        [TestMethod]
        public void Where_DictionaryCollectionWithNullPropertyValueAndContainsComparison_DoesNotThrow()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Int", 1 }, { "Name", "Alpha" } },
                new Dictionary<string, object> { { "Int", 2 }, { "Name", "Bravo" } },
                new Dictionary<string, object> { { "Int", 3 }, { "Name", null } }
            };

            var mergeValues = new LavaDataDictionary { { "Items", items } };

            var templateInput = @"{%- assign matches = Items | Where:'Name','Bra','contains' %}
{%- for match in matches %}
{{- match.Int }};
{%- endfor %}";

            // Only Bravo contains 'Bra'. The null-valued item must be skipped without throwing.
            TestHelper.AssertTemplateOutput( "2;", templateInput, mergeValues, ignoreWhitespace: true );
        }

        /// <summary>
        /// End-to-end reproduction of the reported bug. Verify the Where filter does not throw/fail
        /// when one of the resulting dictionaries has a null value for the filter property.
        /// </summary>
        [TestMethod]
        public void Where_DictionaryCollectionWithNullPropertyValue_ReturnsMatchingItemsWithoutError()
        {
            var templateInput = @"
{% assign sampleObj1 = '' | AddToDictionary:'Int',1 | AddToDictionary:'GroupingId',4 %}
{% assign sampleObj2 = '' | AddToDictionary:'Int',2 | AddToDictionary:'GroupingId',5 %}
{% assign sampleObj3 = '' | AddToDictionary:'Int',3 | AddToDictionary:'GroupingId',null %}
{% assign sampleArray = '' | AddToArray:sampleObj1 | AddToArray:sampleObj2 | AddToArray:sampleObj3 %}
{% assign matches = sampleArray | Where:'GroupingId',5 %}
{% for match in matches %}{{ match.Int }};{% endfor %}
Good evening!
";

            // The matching item is emitted AND the literal text after the filter is preserved -
            // the bug otherwise short-circuits the entire template render with an NRE.
            TestHelper.AssertTemplateOutput( "2; Good evening!", templateInput, ignoreWhitespace: true );
        }

        /// <summary>
        /// Accessing the property of a nested dynamically-typed object should return the correct value.
        /// </summary>
        //[TestMethod]
        //public void Property_AnonymousObjectPropertyAccess_ReturnsValue()
        //{
        //    var groupMember = new
        //    {
        //        GroupName = "Group 1",
        //        GroupRole = new { Name = "Member", IsLeader = false },
        //        Person = new { FirstName = "Alex", LastName = "Andrews", Address = new { Street = "1 Main St", City = "MyTown" } }
        //    };

        //    var mergeValues = new LavaDictionary { { "GroupMember", groupMember } };

        //    _helper.AssertTemplateOutput( "Group 1: Andrews, Alex (1 Main St)",
        //        "{{ GroupMember.GroupName }}: {{ GroupMember.Person.LastName }}, {{ GroupMember.Person.FirstName }} ({{ GroupMember.Person.Address.Street }})",
        //        mergeValues );

        //}
    }
}
