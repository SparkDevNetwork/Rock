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
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

using Swashbuckle.Swagger;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests that the v2 OpenAPI schema names enum values, so that client
    /// generators can produce named enum members.
    /// </summary>
    [TestClass]
    public class RockV2SchemaFilterTests
    {
        private static readonly Assembly _restAssembly = Assembly.Load( "Rock.Rest" );

        private static ISchemaFilter CreateFilter()
        {
            var filterType = _restAssembly.GetType( "Rock.Rest.Swagger.RockV2SchemaFilter", true );

            return ( ISchemaFilter ) Activator.CreateInstance( filterType, true );
        }

        /// <summary>
        /// Builds the inline schema Swashbuckle produces for an enum: an
        /// integer with one entry per value, repeating a value that has an
        /// obsolete alias.
        /// </summary>
        private static Schema CreateEnumSchema( Type enumType )
        {
            return new Schema
            {
                type = "integer",
                format = "int32",
                @enum = Enum.GetValues( enumType ).Cast<object>().ToList()
            };
        }

        private static List<string> GetEnumNames( Schema schema )
        {
            Assert.IsNotNull( schema.vendorExtensions );
            Assert.IsTrue( schema.vendorExtensions.ContainsKey( "x-enumNames" ) );

            return ( ( IEnumerable<string> ) schema.vendorExtensions["x-enumNames"] ).ToList();
        }

        [TestMethod]
        public void Apply_EnumWithObsoleteAlias_NamesEachValueOnceWithItsCurrentName()
        {
            var enumType = typeof( GroupType ).GetProperty( nameof( GroupType.AttendanceRule ) ).PropertyType;
            var attendanceRuleSchema = CreateEnumSchema( enumType );
            var schema = new Schema
            {
                type = "object",
                properties = new Dictionary<string, Schema>
                {
                    ["attendanceRule"] = attendanceRuleSchema
                }
            };

            CreateFilter().Apply( schema, null, typeof( GroupType ) );

            CollectionAssert.AreEqual(
                new[] { "None", "AddOnCheckIn", "AlreadyEnrolledInGroup" },
                GetEnumNames( attendanceRuleSchema ) );
            CollectionAssert.AreEqual(
                new long[] { 0, 1, 2 },
                attendanceRuleSchema.@enum.Select( v => Convert.ToInt64( v ) ).ToArray() );
        }

        [TestMethod]
        public void Apply_NullableAndCollectionEnums_AreNamed()
        {
            var enumType = typeof( GroupType ).GetProperty( nameof( GroupType.AttendanceRule ) ).PropertyType;
            var nullableSchema = CreateEnumSchema( enumType );
            var itemsSchema = CreateEnumSchema( enumType );
            var schema = new Schema
            {
                type = "object",
                properties = new Dictionary<string, Schema>
                {
                    ["optionalRule"] = nullableSchema,
                    ["rules"] = new Schema { type = "array", items = itemsSchema },
                    ["count"] = new Schema { type = "integer", format = "int32" }
                }
            };

            CreateFilter().Apply( schema, null, typeof( EnumPropertiesBag ) );

            CollectionAssert.AreEqual( new[] { "None", "AddOnCheckIn", "AlreadyEnrolledInGroup" }, GetEnumNames( nullableSchema ) );
            CollectionAssert.AreEqual( new[] { "None", "AddOnCheckIn", "AlreadyEnrolledInGroup" }, GetEnumNames( itemsSchema ) );
            Assert.IsFalse( schema.properties["count"].vendorExtensions?.ContainsKey( "x-enumNames" ) == true );
        }

        /// <summary>
        /// A plain object with nullable and collection enum properties.
        /// </summary>
        private class EnumPropertiesBag
        {
            public AttendanceRule? OptionalRule { get; set; }

            public List<AttendanceRule> Rules { get; set; }

            public int Count { get; set; }
        }
    }
}
