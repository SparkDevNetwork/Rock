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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rock.Lava;
using Rock.Utility;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Tests security of access to merge object properties in a Lava template.
    /// </summary>
    [TestClass]
    public class DynamicObjectTests : LavaUnitTestBase
    {
        #region Constructors

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            TestHelper.RegisterSafeType( typeof( PersonRockDynamic ) );
            TestHelper.RegisterSafeType( typeof( CampusRockDynamic ) );

        }

        #endregion

        /// <summary>
        /// Accessing the property of a nested dynamically-typed object should return the correct value.
        /// </summary>
        [TestMethod]
        public void AnonymousType_DotNotationPropertyAccess_ReturnsValue()
        {
            var groupMember = new
            {
                GroupName = "Group 1",
                GroupRole = new { Name = "Member", IsLeader = false },
                Person = new { FirstName = "Alex", LastName = "Andrews", Address = new { Street = "1 Main St", City = "MyTown" } }
            };

            var mergeValues = new LavaDataDictionary { { "GroupMember", groupMember } };

            TestHelper.AssertTemplateOutput( "Group 1: Andrews, Alex (1 Main St)",
                "{{ GroupMember.GroupName }}: {{ GroupMember.Person.LastName }}, {{ GroupMember.Person.FirstName }} ({{ GroupMember.Person.Address.Street }})",
                mergeValues );

        }

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void RockDynamicType_DotNotationPropertyAccess_ReturnsPropertyValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetRockDynamicTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "Decker", "{{ CurrentPerson.LastName }}", mergeValues );
        }

        /// <summary>
        /// Accessing a nested property using dot-notation "Campus.Name" should return the correct value.
        /// </summary>
        [TestMethod]
        public void RockDynamicType_DotNotationNestedPropertyAccess_ReturnsPropertyValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetRockDynamicTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "North Campus", "{{ CurrentPerson.Campus.Name }}", mergeValues );
        }

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void RockDynamicType_DotNotationInvalidPropertyName_ReturnsEmptyString()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetRockDynamicTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( string.Empty, "{{ CurrentPerson.NonexistentProperty }}", mergeValues );
        }

        /// <summary>
        /// Serializing and Deserializing a type derived from RockDynamic should produce a consistent result.
        /// </summary>
        [TestMethod]
        [Ignore( "RockDynamic does not correctly handle round-tripping for dynamically-defined members." )]
        public void RockDynamicType_SerializeDeserialize_CanRoundtrip()
        {
            var dynamicObject = RockDynamicObjectWithCustomPropertyAccess.NewWithData();

            var json = JsonConvert.SerializeObject( dynamicObject );

            var dynamicFromJson = JsonConvert.DeserializeObject<RockDynamicObjectWithCustomPropertyAccess>( json );

            var mergeValues = new LavaDataDictionary { { "Colors", dynamicFromJson } };

            var template = @"Color 1: {{ Colors.Color1 }}, Color 2: {{ Colors.Color2 }}, Color 3: {{ Colors.Color3 }}";

            TestHelper.AssertTemplateOutput( "Color 1: red, Color 2: green, Color 3: blue", template, mergeValues );
        }

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void LavaDataObjectType_DotNotationPropertyAccess_ReturnsPropertyValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetLavaDataObjectTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "Decker", "{{ CurrentPerson.LastName }}", mergeValues );
        }

        /// <summary>
        /// Accessing a nested property using dot-notation "Campus.Name" should return the correct value.
        /// </summary>
        [TestMethod]
        public void LavaDataObjectType_DotNotationNestedPropertyAccess_ReturnsPropertyValue()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetLavaDataObjectTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( "North Campus", "{{ CurrentPerson.Campus.Name }}", mergeValues );
        }

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void LavaDataObjectType_DotNotationInvalidPropertyName_ReturnsEmptyString()
        {
            var mergeValues = new LavaDataDictionary { { "CurrentPerson", GetLavaDataObjectTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( string.Empty, "{{ CurrentPerson.NonexistentProperty }}", mergeValues );
        }

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void LavaDataObjectType_WithCustomPropertyAccessor_ReturnsPropertyValue()
        {
            var dynamicObject = LavaDataObjectWithCustomPropertyAccess.NewWithData();

            var mergeValues = new LavaDataDictionary { { "Colors", dynamicObject } };

            var template = @"Color 1: {{ Colors.Color1 }}, Color 2: {{ Colors.Color2 }}, Color 3: {{ Colors.Color3 }}";

            // This test is only valid for the Fluid Engine.
            TestHelper.AssertTemplateOutput( "Color 1: red, Color 2: green, Color 3: blue", template, mergeValues );
        }

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void LavaDataObjectType_SerializeDeserialize_CanRoundtrip()
        {
            var dynamicObject = LavaDataObjectWithCustomPropertyAccess.NewWithData();

            var json = JsonConvert.SerializeObject( dynamicObject );

            // TODO: json string is empty here - dynamic properties of wrapped object not serialized correctly.
            // RockDynamic passes this test - but it does not implement IDictionary.
            // JsonConvert favors processing the object as IDictionary, uses IDictionary.GetEnumerator()

            var dynamicFromJson = JsonConvert.DeserializeObject<LavaDataObjectWithCustomPropertyAccess>( json );

            // TODO: object not correctly popoulated from JSON string - check TrySetProperty
            var mergeValues = new LavaDataDictionary { { "Colors", dynamicFromJson } };

            var template = @"Color 1: {{ Colors.Color1 }}, Color 2: {{ Colors.Color2 }}, Color 3: {{ Colors.Color3 }}";

            TestHelper.AssertTemplateOutput( "Color 1: red, Color 2: green, Color 3: blue", template, mergeValues );
        }

        #region Test Data

        #region RockDynamic

        /// <summary>
        /// Return an initialized Person object for test subject Ted Decker.
        /// </summary>
        /// <returns></returns>
        public PersonRockDynamic GetRockDynamicTestPersonTedDecker()
        {
            var campus = new CampusRockDynamic { Name = "North Campus", Id = 1 };
            var person = new PersonRockDynamic { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1 };

            return person;
        }

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary
        [DotLiquid.LiquidType( "Id", "NickName", "FirstName", "LastName", "Campus" )]
        public class PersonRockDynamic : RockDynamic
        {
            public int Id { get; set; }
            public string NickName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public CampusRockDynamic Campus { get; set; }

            public override string ToString()
            {
                return $"{NickName} {LastName}";
            }
        }

        /// <summary>
        /// A representation of a Campus used for testing purposes.
        /// </summary>
        [DotLiquid.LiquidType( "Id", "Name" )]
        public class CampusRockDynamic : RockDynamic
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// A class that is derived from LavaDataObject and implements a custom property value getter.
        /// </summary>
        private class RockDynamicObjectWithCustomPropertyAccess : RockDynamic
        {
            private Dictionary<string, object> _internalDictionary = new Dictionary<string, object>();

            public static RockDynamicObjectWithCustomPropertyAccess NewWithData()
            {
                var dynamicObject = new RockDynamicObjectWithCustomPropertyAccess();

                dynamicObject["Color1"] = "red";
                dynamicObject["Color2"] = "green";
                dynamicObject["Color3"] = "blue";

                return dynamicObject;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                var keys = base.GetDynamicMemberNames().ToList();

                keys.AddRange( _internalDictionary.Keys );

                return keys;
            }

            public override bool TrySetMember( SetMemberBinder binder, object value )
            {
                return base.TrySetMember( binder, value );
            }

            public override bool TryGetMember( GetMemberBinder binder, out object result )
            {
                var exists = base.TryGetMember( binder, out result );

                if ( !exists )
                {
                    exists = _internalDictionary.ContainsKey( binder.Name );

                    if ( exists )
                    {
                        result = _internalDictionary[binder.Name];
                    }

                    if ( !exists )
                    {
                        result = null;
                    }
                }

                return exists;
            }
        }

        #endregion

        #region LavaDataObject

        /// <summary>
        /// Return an initialized Person object for test subject Ted Decker.
        /// </summary>
        /// <returns></returns>
        public PersonLavaDataObject GetLavaDataObjectTestPersonTedDecker()
        {
            var campus = new CampusLavaDataObject { Name = "North Campus", Id = 1 };
            var person = new PersonLavaDataObject { FirstName = "Edward", NickName = "Ted", LastName = "Decker", Campus = campus, Id = 1 };

            return person;
        }

        /// <summary>
        /// A representation of a Person used for testing purposes.
        /// </summary>
        public class PersonLavaDataObject : LavaDataObject
        {
            public int Id { get; set; }
            public string NickName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public CampusLavaDataObject Campus { get; set; }

            public override string ToString()
            {
                return $"{NickName} {LastName}";
            }
        }

        /// <summary>
        /// A representation of a Campus used for testing purposes.
        /// </summary>
        public class CampusLavaDataObject : LavaDataObject
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// A class that is derived from LavaDataObject and implements a custom property value getter.
        /// </summary>
        private class LavaDataObjectWithCustomPropertyAccess : LavaDataObject
        {
            private Dictionary<string, object> _internalDictionary;


            public static LavaDataObjectWithCustomPropertyAccess NewWithData()
            {
                var dynamicObject = new LavaDataObjectWithCustomPropertyAccess();

                dynamicObject["Color1"] = "red";
                dynamicObject["Color2"] = "green";
                dynamicObject["Color3"] = "blue";

                return dynamicObject;
            }

            protected override bool OnTryGetValue( string memberName, out object result )
            {
                var exists = base.OnTryGetValue( memberName, out result );

                if ( !exists )
                {
                    exists = _internalDictionary.ContainsKey( memberName );

                    if ( exists )
                    {
                        result = _internalDictionary[memberName];
                    }

                    if ( !exists )
                    {
                        result = null;
                    }
                }

                return exists;
            }
        }

        #endregion

        #endregion
    }
}
