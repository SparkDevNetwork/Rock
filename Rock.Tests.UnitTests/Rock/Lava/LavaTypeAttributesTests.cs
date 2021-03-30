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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Data;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class LavaTypeAttributesTests : LavaUnitTestBase
    {
        #region Constructors

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            TestHelper.RegisterSafeType( typeof( TestPerson ) );
            TestHelper.RegisterSafeType( typeof( TestCampus ) );
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
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
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

            var expectedOutput = @"
Name: Ted Decker
Date of Birth:
";

            // Date of Birth should be omitted because it is not a named as a Lava property.
            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
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

            var expectedOutput = @"
Name: Ted Decker
Password:
";

            // Password value should be omitted even though it is named in the whitelist, because it is marked with the LavaIgnore attribute.
            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
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
";

            // Name value should be the only available property, because it is the only property marked with LavaInclude.
            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
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

            var expectedOutput = @"
Name: Ted Decker
Password:
";

            // Name value should be the only available property, because it is the only property marked with LavaInclude.
            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
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
