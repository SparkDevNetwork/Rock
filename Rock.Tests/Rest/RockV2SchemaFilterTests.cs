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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Rock.Model;
using Rock.Rest.Swagger;
using Rock.Rest.Utility;

using Swashbuckle.Swagger;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests that the REST v2 OpenAPI document names enum members so client
    /// code generators can produce named enum members.
    /// </summary>
    [TestClass]
    public class RockV2SchemaFilterTests
    {
        #region Test Types

        private enum SampleEnum
        {
            First = 0,
            Second = 5,
            Third = 9
        }

        [JsonConverter( typeof( StringEnumConverter ) )]
        private enum SampleStringEnum
        {
            Alpha = 0,
            Beta = 1
        }

        private class SampleBag
        {
            public SampleEnum Sample { get; set; }

            public SampleEnum? NullableSample { get; set; }

            public AttendanceRule AttendanceRule { get; set; }

            public SampleStringEnum StringSample { get; set; }
        }

        #endregion

        [TestMethod]
        public void EnumSchema_HasNamesInValueOrder()
        {
            var schema = GetPropertySchema( nameof( SampleBag.Sample ) );

            CollectionAssert.AreEqual( new object[] { 0, 5, 9 }, schema.@enum.ToArray() );
            CollectionAssert.AreEqual( new[] { "First", "Second", "Third" }, GetNames( schema, "x-enumNames" ) );
            CollectionAssert.AreEqual( new[] { "First", "Second", "Third" }, GetNames( schema, "x-enum-varnames" ) );
        }

        [TestMethod]
        public void NullableEnumSchema_HasNames()
        {
            var schema = GetPropertySchema( nameof( SampleBag.NullableSample ) );

            CollectionAssert.AreEqual( new[] { "First", "Second", "Third" }, GetNames( schema, "x-enumNames" ) );
        }

        [TestMethod]
        public void EnumWithDuplicateValues_ListsEachValueOnceWithNonObsoleteName()
        {
            var schema = GetPropertySchema( nameof( SampleBag.AttendanceRule ) );

            CollectionAssert.AreEqual( new object[] { 0, 1, 2 }, schema.@enum.ToArray() );
            CollectionAssert.AreEqual(
                new[] { nameof( AttendanceRule.None ), nameof( AttendanceRule.AddOnCheckIn ), nameof( AttendanceRule.AlreadyEnrolledInGroup ) },
                GetNames( schema, "x-enumNames" ) );
        }

        [TestMethod]
        public void StringEnumSchema_IsNotChanged()
        {
            var schema = GetPropertySchema( nameof( SampleBag.StringSample ) );

            Assert.AreEqual( "string", schema.type );
            Assert.IsFalse( schema.vendorExtensions?.ContainsKey( "x-enumNames" ) ?? false );
        }

        [TestMethod]
        public void SerializedDocument_EmitsEnumNamesExtension()
        {
            var schema = GetPropertySchema( nameof( SampleBag.Sample ) );
            var json = JsonConvert.SerializeObject( schema, ApiPickerJsonMediaTypeFormatter.CreateV2Formatter().SerializerSettings );

            StringAssert.Contains( json, "\"x-enumNames\":[\"First\",\"Second\",\"Third\"]" );
            StringAssert.Contains( json, "\"enum\":[0,5,9]" );
        }

        #region Support Methods

        /// <summary>
        /// Builds the schema for <see cref="SampleBag"/> the same way the v2
        /// document does and returns the inline schema of one property.
        /// </summary>
        /// <param name="propertyName">The property whose schema is returned.</param>
        /// <returns>The property schema.</returns>
        private static Schema GetPropertySchema( string propertyName )
        {
            var registry = new SchemaRegistry(
                ApiPickerJsonMediaTypeFormatter.CreateV2Formatter().SerializerSettings,
                new Dictionary<Type, Func<Schema>>(),
                new ISchemaFilter[] { new RockV2SchemaFilter() },
                new IModelFilter[0],
                false,
                t => t.FullName,
                false,
                false,
                true );

            registry.GetOrRegister( typeof( SampleBag ) );

            var bagSchema = registry.Definitions[typeof( SampleBag ).FullName];

            return bagSchema.properties.First( p => p.Key.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) ).Value;
        }

        /// <summary>
        /// Gets the names stored in an enum name vendor extension.
        /// </summary>
        /// <param name="schema">The enum schema.</param>
        /// <param name="extensionName">The vendor extension name.</param>
        /// <returns>The names, or an empty list if the extension is missing.</returns>
        private static List<string> GetNames( Schema schema, string extensionName )
        {
            if ( schema.vendorExtensions == null || !schema.vendorExtensions.TryGetValue( extensionName, out var names ) )
            {
                return new List<string>();
            }

            return ( ( IEnumerable<string> ) names ).ToList();
        }

        #endregion
    }
}
