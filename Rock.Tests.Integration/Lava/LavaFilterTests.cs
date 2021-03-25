﻿// <copyright>
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

using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests for Lava Filters.
    /// </summary>
    /// <remarks>
    /// These tests exist in the integration tests project because they require a configured Lava Engine to operate correctly.
    /// </remarks>
    [TestClass]
    public class LavaFilterTests
    {
        #region Initialization

        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            // Initialize the Lava Engine.
            Liquid.UseRubyDateFormat = false;
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

            Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );
        }

        #endregion

        #region Where

        [TestMethod]
        public void Where_WithSingleConditionNumericValue_ReturnsMatchingItems()
        {
            var items = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { { "Id", (long)1 } },
               new Dictionary<string, object> { { "Id", (long)2 } }
            };

            var mergeFields = new Dictionary<string, object> { { "Items", items } };

            var templateInput = @"
{% assign matches = Items | Where:'Id',1 %}
{% for match in matches %}
    {{ match.Id }}<br>
{% endfor %}
";

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.Contains( output, "1<br>" );
            Assert.That.DoesNotContain( output, "2<br>" );
        }

        [TestMethod]
        public void Where_WithSingleConditionStringValue_ReturnsMatchingItems()
        {
            var items = new List<Dictionary<string, object>>
                {
                   new Dictionary<string, object> { { "Id", "1" } },
                   new Dictionary<string, object> { { "Id", "2" } }
                };

            var mergeFields = new Dictionary<string, object> { { "Items", items } };

            var templateInput = @"
{% assign matches = Items | Where:'Id','1' %}
{% for match in matches %}
    {{ match.Id }}<br>
{% endfor %}
";

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.Contains( output, "1<br>" );
            Assert.That.DoesNotContain( output, "2<br>" );
        }

        [TestMethod]
        public void Where_WithMultipleConditions_ReturnsOnlyMatchingItems()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPerson() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName == \"Employer\" || Value == \"Outreach Pastor\"'" );

            var expectedOutput = @"
Employer: Rock Solid Church <br>
Position: Outreach Pastor <br>
";
            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.AreEqualIgnoreWhitespace( expectedOutput, output );
        }

        [TestMethod]
        public void Where_WithSingleConditionEqualComparison_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPerson() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer','equal'" );

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.Contains( output, "Employer:" );
            Assert.That.DoesNotContain( output, "Position:" );
        }

        [TestMethod]
        public void Where_WithSingleConditionNotEqual_ReturnsOnlyNotEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPerson() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer','notequal'" );

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.DoesNotContain( output, "Employer:" );
            Assert.That.Contains( output, "Position:" );
        }

        [TestMethod]
        public void Where_WithSingleConditionDefaultComparison_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPerson() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer'" );

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.Contains( output, "Employer:" );
            Assert.That.DoesNotContain( output, "Position:" );
        }

        [TestMethod]
        public void Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPerson() } };

            var templateInput = @"
{% assign personPhones = CurrentPerson.PhoneNumbers | Where:'NumberTypeValue.Value == ""Home""' %}
{% for phone in personPhones %}
    {{ phone.NumberTypeValue.Value }}: {{ phone.NumberFormatted }} <br>
{% endfor %}
";

            var output = templateInput.ResolveMergeFields( mergeFields );

            Assert.That.Contains( output, "Home:" );
            Assert.That.DoesNotContain( output, "Work:" );
        }

        private string GetWhereFilterTestTemplatePersonAttributes( string whereParameters )
        {
            var template = @"
{% assign attributesWithValues = CurrentPerson.AttributeValues | Where:{whereParameters} %}
{% for attributeValue in attributesWithValues %}
    {{ attributeValue.AttributeName }}: {{ attributeValue.Value }} <br>
{% endfor %}";

            template = template.Replace( "{whereParameters}", whereParameters );

            return template;
        }

        private Person GetWhereFilterTestPerson()
        {
            var rockContext = new RockContext();

            var personTedDecker = new PersonService( rockContext ).Queryable()
                .FirstOrDefault( x => x.LastName == "Decker" && x.NickName == "Ted" );

            var phones = personTedDecker.PhoneNumbers;

            Assert.That.IsNotNull( personTedDecker, "Test person not found in current database." );

            return personTedDecker;
        }

        #endregion

        #region ReadCookie/WriteCookie

        [TestMethod]
        public void WriteCookie_ForExistingCookie_RendersCookieValue()
        {
            var template = @"{{ 'cookie1' | WriteCookie:'oatmeal' }}";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                var output = template.ResolveMergeFields( null );

                var cookie = GetExistingCookie( simulator, "cookie1" );

                Assert.That.AreEqual( "oatmeal", cookie.Value );
            }
        }

        [TestMethod]
        public void WriteCookie_WithInvalidKey_RendersErrorMessage()
        {
            var template = @"{{ '' | WriteCookie:'fudge' }}";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                var output = template.ResolveMergeFields( null );

                Assert.That.AreEqual( "WriteCookie failed: A Key must be specified.", output );
            }
        }

        [TestMethod]
        public void WriteCookie_WithExpiry_HasCorrectExpiryTime()
        {
            // Set a cookie to expire in 30 minutes.
            var template = "{{ 'cookie1' | WriteCookie:'oreo','30' }}";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                // Write the cookie value and verify that it exists.
                var output = template.ResolveMergeFields( null );

                var cookie = GetExistingCookie( simulator, "cookie1" );

                Assert.That.AreProximate( cookie.Expires, RockDateTime.Now, new System.TimeSpan( 0, 35, 0 ) );
            }
        }

        [TestMethod]
        public void WriteCookie_WithNoCurrentHttpRequest_RendersErrorMessage()
        {
            var template = @"{{ 'cookie1' | WriteCookie:'fudge' }}";

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( "WriteCookie failed: A Http Session is required.", output );
        }

        [TestMethod]
        public void ReadCookie_ForExistingCookie_RendersCookieValue()
        {
            var template = @"{{ 'cookie1' | ReadCookie }}";
            var expectedValue = "choc-chip";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                // Set the cookie in the response.
                simulator.Context.Response.Cookies.Add( new HttpCookie( "cookie1", expectedValue ) );

                var output = template.ResolveMergeFields( null );

                Assert.That.AreEqualIgnoreNewline( expectedValue, output );
            }
        }

        [TestMethod]
        public void ReadCookie_ForNonExistentCookie_RendersEmptyString()
        {
            var template = @"{{ 'invalid_cookie_key' | ReadCookie }}";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                var output = template.ResolveMergeFields( null );

                Assert.That.IsTrue( output.IsNullOrWhiteSpace(), "Unexpected template output." );
            }
        }

        [TestMethod]
        public void ReadCookie_WithNoCurrentHttpRequest_RendersEmptyString()
        {
            var template = @"{{ 'invalid_cookie_key' | ReadCookie }}";

            var output = template.ResolveMergeFields( null );

            Assert.That.IsTrue( output.IsNullOrWhiteSpace(), "Unexpected template output." );
        }

        /// <summary>
        /// Verify that the specified cookie exists and retrieve it.
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private HttpCookie GetExistingCookie( Http.TestLibrary.HttpSimulator simulator, string key )
        {
            // Check if the cookie exists. If not, reading the cookie simply returns an empty cookie of the same name.
            Assert.That.IsTrue( simulator.Context.Response.Cookies.AllKeys.Contains( key ), "Cookie not found." );

            return simulator.Context.Response.Cookies[key];
        }

        #endregion
    }
}
