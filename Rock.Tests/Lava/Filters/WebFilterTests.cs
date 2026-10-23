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
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

using RockAppTestHelper = Rock.Tests.Shared.TestFramework.TestHelper;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    /// <summary>
    /// Tests for Lava filters that belong to the Rock web application.
    /// </summary>
    /// <remarks>
    /// The PageRoute, ResolveRockUrl and SetUrlParameter tests stay in
    /// Rock.Tests.Integration: they resolve real page routes, which the class
    /// there reloads from the database in its TestInitialize. The Where tests that
    /// filter a person's attribute values or phone numbers stay for the same
    /// reason - they read Ted Decker and Sarah Simmons from the sample data.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class WebFilterTests
    {
        #region TitleCase

        [TestMethod]
        [DataRow( @"{{ ""men's gathering/get-together"" | TitleCase }}", "Men's Gathering/get-together" )]
        [DataRow( @"{{ 'mATTHEw 24:29-41 - KJV' | TitleCase }}", "Matthew 24:29-41 - KJV" )]
        public void TitleCase_TextWithPunctuation_PreservesPunctuation( string inputTemplate, string expectedOutput )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion TitleCase

        #region Where

        [TestMethod]
        public void Where_WithSingleConditionNumericValue_ReturnsMatchingItems()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Id", ( long ) 1 } },
                new Dictionary<string, object> { { "Id", ( long ) 2 } }
            };

            AssertMatchedIdsForItems( items, "Where:'Id',1", output => Assert.AreEqual( "1<br>", output ) );
        }

        [TestMethod]
        public void Where_WithSingleConditionStringValue_ReturnsMatchingItems()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Id", "1" } },
                new Dictionary<string, object> { { "Id", "2" } }
            };

            AssertMatchedIdsForItems( items, "Where:'Id','1'", output => Assert.AreEqual( "1<br>", output ) );
        }

        [TestMethod]
        public void Where_WithSingleConditionHavingNoMatches_ReturnsEmptyString()
        {
            var items = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Id", "1" } },
                new Dictionary<string, object> { { "Id", "2" } }
            };

            AssertMatchedIdsForItems( items, "Where:'Id','3'", output => Assert.AreEqual( string.Empty, output ) );
        }

        [TestMethod]
        public void Where_EmptyInputSetWithSingleCondition_ReturnsEmptyString()
        {
            var items = new List<TestWhereFilterCollectionItem>();

            AssertMatchedIdsForItems( items, "Where:'Id','1'", output => Assert.AreEqual( string.Empty, output ) );
        }

        [TestMethod]
        public void Where_EmptyInputSetWithMultipleConditions_ReturnsEmptyString()
        {
            var items = new List<TestWhereFilterCollectionItem>();

            AssertMatchedIdsForItems( items, "Where:'Id ==\"1\" || Id == \"2\"'", output => Assert.AreEqual( string.Empty, output ) );
        }

        [TestMethod]
        public void Where_WithSingleConditionOnEnumProperty_ReturnsOnlyEqualValues()
        {
            var items = new List<TestWhereFilterCollectionItem>
            {
                new TestWhereFilterCollectionItem { Gender = Gender.Male, Name = "Ted Decker" },
                new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Cindy Decker" },
                new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Alex Decker" },
                new TestWhereFilterCollectionItem { Gender = Gender.Male, Name = "Bill Marble" },
                new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Alisha Marble" }
            };

            // The enum is matched by name in the first pass and by its underlying
            // integer value in the second.
            var input = """
                {%- assign matches = Items | Where:'Gender','Male' -%}
                {%- for match in matches -%}{{ match.Name }}<br>{%- endfor -%}
                {%- assign matches = Items | Where:'Gender',FemaleId -%}
                {%- for match in matches -%}{{ match.Name }}<br>{%- endfor -%}
                """;

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object>
                {
                    { "Items", items },
                    { "FemaleId", ( int ) Gender.Female }
                }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( "Ted Decker<br>Bill Marble<br>Cindy Decker<br>Alex Decker<br>Alisha Marble<br>", output );
            } );
        }

        #endregion Where

        #region Where: Person Attribute Values

        [TestMethod]
        public void Where_WithMultipleConditions_ReturnsOnlyMatchingItems()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithAttributes();

                AssertAttributeWhere( person, @"'AttributeName == ""Employer"" || Value == ""Outreach Pastor""'", output => Assert.AreEqual( "Employer: Rock Solid Church<br>Position: Outreach Pastor<br>", output ) );
            }
        }

        [TestMethod]
        public void Where_WithMultipleConditionsHavingNoMatches_ReturnsEmptyString()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithAttributes();

                AssertAttributeWhere( person, @"'AttributeName == ""Unmatched"" || Value == ""Unmatched""'", output => Assert.AreEqual( string.Empty, output ) );
            }
        }

        [TestMethod]
        public void Where_WithSingleConditionEqualComparison_ReturnsOnlyEqualValues()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithAttributes();

                AssertAttributeWhere( person, "'AttributeName','Employer','equal'", output => Assert.AreEqual( "Employer: Rock Solid Church<br>", output ) );
            }
        }

        [TestMethod]
        public void Where_WithSingleConditionDefaultComparison_ReturnsOnlyEqualValues()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithAttributes();

                // Omitting the comparison should behave the same as 'equal'.
                AssertAttributeWhere( person, "'AttributeName','Employer'", output => Assert.AreEqual( "Employer: Rock Solid Church<br>", output ) );
            }
        }

        [TestMethod]
        public void Where_WithSingleConditionNotEqual_ReturnsOnlyNotEqualValues()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithAttributes();

                AssertAttributeWhere( person, "'AttributeName','Employer','notequal'", output =>
                {
                    Assert.DoesNotContain( "Employer:", output );
                    Assert.Contains( "Position: Outreach Pastor<br>", output );
                } );
            }
        }

        #endregion Where: Person Attribute Values

        #region Where: Person Phone Numbers

        /// <summary>
        /// The Where filter using a 'contains' comparison on a collection of phone
        /// numbers should return only the number containing the specified value.
        /// </summary>
        [TestMethod]
        public void Where_WithSingleContainsCondition_ReturnsContainedValues()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithPhoneNumbers( "Sarah", "Simmons", out _ );

                var input = "{{ CurrentPerson.PhoneNumbers | Where:'Number', 555, 'contains' | Select:'NumberFormatted' | Join:', ' }}";

                AssertRenderForPerson( person, input, output => Assert.AreEqual( "(623) 555-3322, (623) 555-2444, (623) 555-3323", output ) );
            }
        }

        [TestMethod]
        public void Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithPhoneNumbers( "Ted", "Decker", out _ );

                var input = """
                    {%- assign personPhones = CurrentPerson.PhoneNumbers | Where:'NumberTypeValue.Value == "Home"' -%}
                    {%- for phone in personPhones -%}{{ phone.NumberTypeValue.Value }}: {{ phone.NumberFormatted }}<br>{%- endfor -%}
                    """;

                AssertRenderForPerson( person, input, output => Assert.AreEqual( "Home: (623) 555-3322<br>", output ) );
            }
        }

        /// <summary>
        /// Verify that the example used in the Lava documentation produces the expected outcome.
        /// </summary>
        [TestMethod]
        public void Where_DocumentationExample_IsValid()
        {
            /*
                9/26/26 - CLAUDE

                The predecessor test declared a mobilePhoneNumberTypeValueId local,
                then called templateInput.Replace( "<mobilePhoneId>", ... ) and threw
                the result away - string.Replace returns a new string. The template
                did not contain that placeholder in any case, and instead hard-coded
                the literal 12, which is the mobile phone type's id in the sample
                data.

                Here the id of the seeded mobile phone type is substituted into the
                template, so the test no longer depends on a particular row id.

                Reason: Substitute the phone type id the test seeds rather than
                hard-coding one from the sample data.
            */
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var person = SeedPersonWithPhoneNumbers( "Ted", "Decker", out var mobilePhoneTypeId );

                var input = "{{ CurrentPerson.NickName }}'s other contact numbers are: "
                    + "{{ CurrentPerson.PhoneNumbers | Where:'NumberTypeValueId', " + mobilePhoneTypeId + ", 'notequal' | Select:'NumberFormatted' | Join:', ' }}.'";

                AssertRenderForPerson( person, input, output => Assert.AreEqual( "Ted's other contact numbers are: (623) 555-3322, (623) 555-2444.'", output ) );
            }
        }

        [TestMethod]
        public void Where_RepeatExecutions_ReturnsSameResult()
        {
            Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues();

            Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues();
        }

        #endregion Where: Person Phone Numbers

        #region WriteCookie

        [TestMethod]
        public void WriteCookie_ForExistingCookie_RendersCookieValue()
        {
            var input = "{{ 'cookie1' | WriteCookie:'oatmeal' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    LavaRenderTestHelper.Render( engine, input );

                    var cookie = GetExistingCookie( simulator, "cookie1" );

                    Assert.AreEqual( "oatmeal", cookie.Value );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithInvalidKey_RendersErrorMessage()
        {
            var input = "{{ '' | WriteCookie:'fudge' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    var output = LavaRenderTestHelper.Render( engine, input );

                    Assert.AreEqual( "WriteCookie failed: A Key must be specified.", output );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithExpiry_HasCorrectExpiryTime()
        {
            // Set a cookie to expire in 30 minutes.
            var input = "{{ 'cookie1' | WriteCookie:'oreo','30' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    LavaRenderTestHelper.Render( engine, input );

                    var cookie = GetExistingCookie( simulator, "cookie1" );

                    Assert.That.AreProximate( cookie.Expires, RockDateTime.SystemDateTime, new System.TimeSpan( 0, 35, 0 ) );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithNoCurrentHttpRequest_RendersErrorMessage()
        {
            var input = "{{ 'cookie1' | WriteCookie:'fudge' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "WriteCookie failed: A Http Session is required.", output );
            } );
        }

        #endregion WriteCookie

        #region ReadCookie

        [TestMethod]
        public void ReadCookie_ForExistingCookie_RendersCookieValue()
        {
            var input = "{{ 'cookie1' | ReadCookie }}";
            var expectedValue = "choc-chip";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    // Set the cookie in the response.
                    simulator.Context.Response.Cookies.Add( new HttpCookie( "cookie1", expectedValue ) );

                    var output = LavaRenderTestHelper.Render( engine, input );

                    Assert.AreEqual( expectedValue, output );
                }
            } );
        }

        [TestMethod]
        public void ReadCookie_ForNonExistentCookie_RendersEmptyString()
        {
            var input = "{{ 'invalid_cookie_key' | ReadCookie }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    var output = LavaRenderTestHelper.Render( engine, input );

                    Assert.AreEqual( string.Empty, output );
                }
            } );
        }

        [TestMethod]
        public void ReadCookie_WithNoCurrentHttpRequest_RendersEmptyString()
        {
            var input = "{{ 'invalid_cookie_key' | ReadCookie }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        #endregion ReadCookie

        #region Support Methods

        /// <summary>
        /// Renders the Id of every item matched by the supplied Where filter
        /// expression, one per line with no surrounding whitespace.
        /// </summary>
        /// <remarks>
        /// The assertion is passed in rather than the output returned, so that it
        /// runs once per active engine. Returning the output would assert against
        /// whichever engine happened to run last, and would assert against the
        /// previous engine's text if the last one threw.
        /// </remarks>
        /// <param name="items">The collection to filter, exposed as the Items merge field.</param>
        /// <param name="whereExpression">The Where filter expression to apply, without the leading pipe.</param>
        /// <param name="assertOutput">Asserts against the rendered output.</param>
        private void AssertMatchedIdsForItems( object items, string whereExpression, Action<string> assertOutput )
        {
            var input = "{%- assign matches = Items | " + whereExpression + " -%}"
                + "{%- for match in matches -%}{{ match.Id }}<br>{%- endfor -%}";

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "Items", items } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                assertOutput( LavaRenderTestHelper.Render( engine, input, options ) );
            } );
        }

        /// <summary>
        /// Seeds a person carrying an Employer and a Position attribute value.
        /// </summary>
        /// <returns>The person, with attributes loaded and values set.</returns>
        private static Person SeedPersonWithAttributes()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var textFieldType = MockData.CreateFieldType( rockContext,
                SystemGuid.FieldType.TEXT.AsGuid(),
                "Text",
                "Rock.Field.Types.TextFieldType" );

            var personEntityTypeId = EntityTypeCache.GetId( typeof( Person ) ) ?? 0;

            MockData.CreateAttribute( rockContext, "Employer", "Employer", textFieldType.Id, personEntityTypeId );
            MockData.CreateAttribute( rockContext, "Position", "Position", textFieldType.Id, personEntityTypeId );

            var person = MockData.CreatePerson( rockContext, "Ted", "Decker" );

            person.LoadAttributes();
            person.SetAttributeValue( "Employer", "Rock Solid Church" );
            person.SetAttributeValue( "Position", "Outreach Pastor" );

            return person;
        }

        /// <summary>
        /// Seeds a person carrying a home, work and mobile phone number.
        /// </summary>
        /// <param name="nickName">The person's nick name.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="mobilePhoneTypeId">Receives the id of the seeded mobile phone type.</param>
        /// <returns>The person, with the numbers attached.</returns>
        private static Person SeedPersonWithPhoneNumbers( string nickName, string lastName, out int mobilePhoneTypeId )
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var homeType = MockData.CreateDefinedValue( rockContext, SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), "Home" );
            var workType = MockData.CreateDefinedValue( rockContext, SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), "Work" );
            var mobileType = MockData.CreateDefinedValue( rockContext, SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), "Mobile" );

            mobilePhoneTypeId = mobileType.Id;

            var person = MockData.CreatePerson( rockContext, nickName, lastName );

            // CreatePerson sets FirstName but not NickName, and the documentation
            // example renders the nick name.
            person.NickName = nickName;

            MockData.CreatePhoneNumber( rockContext, person, "6235553322", "(623) 555-3322", homeType );
            MockData.CreatePhoneNumber( rockContext, person, "6235552444", "(623) 555-2444", workType );
            MockData.CreatePhoneNumber( rockContext, person, "6235553323", "(623) 555-3323", mobileType );

            return person;
        }

        /// <summary>
        /// Renders the person's attribute values matched by the supplied Where
        /// filter expression.
        /// </summary>
        /// <param name="person">The person exposed as the CurrentPerson merge field.</param>
        /// <param name="whereParameters">The Where filter parameters, without the leading pipe.</param>
        /// <param name="assertOutput">Asserts against the rendered output.</param>
        private void AssertAttributeWhere( Person person, string whereParameters, Action<string> assertOutput )
        {
            var input = "{%- assign attributesWithValues = CurrentPerson.AttributeValues | Where:" + whereParameters + " -%}"
                + "{%- for attributeValue in attributesWithValues -%}{{ attributeValue.AttributeName }}: {{ attributeValue.Value }}<br>{%- endfor -%}";

            AssertRenderForPerson( person, input, assertOutput );
        }

        /// <summary>
        /// Renders a template with the supplied person as the CurrentPerson merge
        /// field, and asserts against the output.
        /// </summary>
        /// <remarks>
        /// As above, the assertion is passed in so that it runs once per active
        /// engine rather than once against the last engine to finish.
        /// </remarks>
        /// <param name="person">The person to expose.</param>
        /// <param name="input">The template to render.</param>
        /// <param name="assertOutput">Asserts against the rendered output.</param>
        private void AssertRenderForPerson( Person person, string input, Action<string> assertOutput )
        {
            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "CurrentPerson", person } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                assertOutput( LavaRenderTestHelper.Render( engine, input, options ) );
            } );
        }

        /// <summary>
        /// Verifies that the specified cookie exists and returns it.
        /// </summary>
        /// <remarks>
        /// Reading a cookie that was never written simply returns an empty cookie
        /// of the same name, so its presence has to be checked separately.
        /// </remarks>
        /// <param name="simulator">The request simulator holding the response.</param>
        /// <param name="key">The cookie name.</param>
        /// <returns>The cookie.</returns>
        private HttpCookie GetExistingCookie( Http.TestLibrary.HttpSimulator simulator, string key )
        {
            Assert.IsTrue( simulator.Context.Response.Cookies.AllKeys.Contains( key ), "Cookie not found." );

            return simulator.Context.Response.Cookies[key];
        }

        #endregion Support Methods

        #region Support Classes

        /// <summary>
        /// A collection item used to exercise the Where filter against plain
        /// properties, including an enum.
        /// </summary>
        private class TestWhereFilterCollectionItem : RockDynamic
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public Gender Gender { get; set; }
        }

        #endregion Support Classes
    }
}
