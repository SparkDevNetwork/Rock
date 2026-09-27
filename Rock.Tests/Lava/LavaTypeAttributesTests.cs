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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Lava.Shared;

using Rock.Lava;
using Rock.Tests.Lava.Filters;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaTypeAttributesTests
    {
        #region Support Methods

        /// <summary>
        /// Registers the types these tests expose to Lava. Each test gets its own
        /// engine, so this runs per test rather than once for the class.
        /// </summary>
        /// <param name="engine">The engine under test.</param>
        private static void RegisterSafeTypes( ILavaEngine engine )
        {
            engine.RegisterSafeType( typeof( TestPerson ) );
            engine.RegisterSafeType( typeof( TestCampus ) );
        }

        #endregion

        #region LavaTypeAttribute

        /// <summary>
        /// Referencing a non-existent property of an input object should return an empty string.
        /// </summary>
        [TestMethod]
        public void LavaTypeAttribute_WithoutNamedProperties_ShouldRenderAllProperties()
        {
            var testObject = new TestLavaTypeAttributeWithoutNamedPropertiesClass();

            var mergeValues = new LavaDataDictionary { { "PersonInfo", testObject } };

            var template = @"
Name: {{ PersonInfo.Name }}
Email: {{ PersonInfo.Email }}
";

            var expectedOutput = @"
Name: Ted Decker
Email: tdecker@rocksolidchurch.com
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterSafeTypes, engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// A property that is not named as included in the LavaType attribute definition should not be exposed during the rendering process.
        /// </summary>
        [TestMethod]
        public void LavaTypeAttribute_WithNamedProperties_DoesNotExposeUnnamedUndecoratedProperty()
        {
            var testObject = new TestLavaTypeAttributeWithNamedPropertiesClass();

            var mergeValues = new LavaDataDictionary { { "PersonInfo", testObject } };

            var template = @"
Name: {{ PersonInfo.Name }}
Date of Birth: {{ PersonInfo.DateOfBirth }}
";

            var expectedOutput = "\nName: Ted Decker\nDate of Birth: \n";

            // Date of Birth should be omitted because it is not a named as a Lava property.
            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterSafeTypes, engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// A property named as included in the LavaType attribute definition but also marked with the LavaIgnore attribute should not be exposed during the rendering process.
        /// </summary>
        [TestMethod]
        public void LavaTypeAttribute_WithNamedPropertyMarkedAsIgnored_DoesNotExposeIgnoredProperty()
        {
            var testObject = new TestLavaTypeAttributeWithNamedPropertiesClass();

            var mergeValues = new LavaDataDictionary { { "PersonInfo", testObject } };

            var template = @"
Name: {{ PersonInfo.Name }}
Password: {{ PersonInfo.Password }}
";

            var expectedOutput = "\nName: Ted Decker\nPassword: \n";

            // Password value should be omitted even though it is named in the whitelist, because it is marked with the LavaIgnore attribute.
            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterSafeTypes, engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion

        #region LavaVisible/LavaHidden

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void LavaVisibleAttribute_PropertyWithVisibleAttribute_IsExposedInLava()
        {
            var testObject = new TestLavaTypeAttributeOnIndividualProperty();

            var mergeValues = new LavaDataDictionary { { "PersonInfo", testObject } };

            var template = @"
Name: {{ PersonInfo.Name }}
";

            var expectedOutput = @"
Name: Ted Decker
".NormalizeLineEndings();

            // Name value should be the only available property, because it is the only property marked with LavaInclude.
            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterSafeTypes, engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Accessing a nested property using dot-notation "Campus.Name" should return the correct value.
        /// </summary>
        [TestMethod]
        public void LavaHiddenAttribute_PropertyWithHiddenAttribute_IsNotExposed()
        {
            var testObject = new TestLavaTypeAttributeOnIndividualProperty();

            var mergeValues = new LavaDataDictionary { { "PersonInfo", testObject } };

            var template = @"
Name: {{ PersonInfo.Name }}
Password: {{ PersonInfo.Password }}
";

            var expectedOutput = "\nName: Ted Decker\nPassword: \n";

            // Name value should be the only available property, because it is the only property marked with LavaInclude.
            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterSafeTypes, engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion

        [LavaType]
        public class TestLavaTypeAttributeWithoutNamedPropertiesClass
        {
            public string Name { get; set; } = "Ted Decker";
            public string Email { get; set; } = "tdecker@rocksolidchurch.com";
        }

        [LavaType( "Name", "Email", "Password" )]
        public class TestLavaTypeAttributeWithNamedPropertiesClass
        {
            public string Name { get; set; } = "Ted Decker";
            public string Email { get; set; } = "tdecker@rocksolidchurch.com";
            public string DateOfBirth { get; set; } = "1-Aug-1980";

            [LavaHidden]
            public string Password { get; set; } = "this-should-remain-secret";
        }

        [LavaType]
        public class TestLavaTypeAttributeOnIndividualProperty
        {
            [LavaVisible]
            public string Name { get; set; } = "Ted Decker";
            public string Email { get; set; } = "tdecker@rocksolidchurch.com";

            [LavaHidden]
            public string Password { get; set; } = "secret_password";
        }

    }
}
