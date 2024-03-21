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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Utility;
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava
{
    /// <summary>
    /// Tests for Lava Filters related to operations that are only available in the Rock Web application.
    /// </summary>
    [TestClass]
    public class WebSiteFilterTests : LavaIntegrationTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Rock.Web.RockRouteHandler.ReregisterRoutes();
        }

        #region AddResponseHeader

        [TestMethod]
        [Ignore( "This test is invalid. The HttpResponse.Headers collection is not available to be read when using HttpSimulator (v2.3.0)." )]
        public void AddResponseHeader_ForExistingCookie_RendersCookieValue()
        {
            var template = @"{{ 'public, max-age=120' | AddResponseHeader:'cache-control' }}";

            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                var output = template.ResolveMergeFields( null );

                var header = simulator.Context.Response.Headers.Get( "cache-control" );

                Assert.That.AreEqual( "public, max-age=120", header );
            }
        }

        #endregion

        #region TitleCase

        [DataTestMethod]
        [DataRow( @"{{ ""men's gathering/get-together"" | TitleCase }}", "Men's Gathering/get-together" )]
        [DataRow( @"{{ 'mATTHEw 24:29-41 - KJV' | TitleCase }}", "Matthew 24:29-41 - KJV" )]
        public void TitleCase_TextWithPunctuation_PreservesPunctuation( string inputTemplate, string expectedOutput )
        {
            TestHelper.AssertTemplateOutput( expectedOutput, inputTemplate );
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

            var expectedOutput = @"
1<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
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
            var expectedOutput = @"
1<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithMultipleConditions_ReturnsOnlyMatchingItems()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName == \"Employer\" || Value == \"Outreach Pastor\"'" );

            var expectedOutput = @"
Employer: Rock Solid Church <br>
Position: Outreach Pastor <br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithMultipleConditionsHavingNoMatches_ReturnsEmptyString()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName == \"Unmatched\" || Value == \"Unmatched\"'" );

            var expectedOutput = @"";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithSingleConditionEqualComparison_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer','equal'" );

            var expectedOutput = @"
Employer: Rock Solid Church <br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithSingleConditionNotEqual_ReturnsOnlyNotEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer','notequal'" );

            var excludedOutput = @"
Employer:
";

            TestHelper.AssertTemplateOutput( excludedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields, OutputMatchType = LavaTestOutputMatchTypeSpecifier.DoesNotContain } );
        }

        /// <summary>
        /// Verify that the example used in the Lava documentation produces the expected outcome.
        /// </summary>
        [TestMethod]
        public void Where_DocumentationExample_IsValid()
        {
            const int mobilePhoneNumberTypeValueId = 12;

            var templateInput = @"
{{ CurrentPerson.NickName }}'s other contact numbers are: {{ CurrentPerson.PhoneNumbers | Where:'NumberTypeValueId', 12, 'notequal' | Select:'NumberFormatted' | Join:', ' }}.'
";

            templateInput.Replace( "<mobilePhoneId>", mobilePhoneNumberTypeValueId.ToString() );

            var expectedOutput = @"
Ted's other contact numbers are: (623) 555-3322,(623) 555-2444.'
";

            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithSingleConditionDefaultComparison_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = GetWhereFilterTestTemplatePersonAttributes( "'AttributeName','Employer'" );

            var expectedOutput = @"
Employer:RockSolidChurch<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues()
        {
            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var templateInput = @"
{% assign personPhones = CurrentPerson.PhoneNumbers | Where:'NumberTypeValue.Value == ""Home""' %}
{% for phone in personPhones %}
    {{ phone.NumberTypeValue.Value }}: {{ phone.NumberFormatted }}<br>
{% endfor %}
";

            var expectedOutput = @"
Home: (623)555-3322 <br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_WithSingleConditionOnEnumProperty_ReturnsOnlyEqualValues()
        {
            var items = new List<TestWhereFilterCollectionItem>();

            items.Add( new TestWhereFilterCollectionItem { Gender = Gender.Male, Name = "Ted Decker" } );
            items.Add( new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Cindy Decker" } );
            items.Add( new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Alex Decker" } );
            items.Add( new TestWhereFilterCollectionItem { Gender = Gender.Male, Name = "Bill Marble" } );
            items.Add( new TestWhereFilterCollectionItem { Gender = Gender.Female, Name = "Alisha Marble" } );

            var mergeFields = new Dictionary<string, object> { { "Items", items }, { "FemaleId", ( int ) Gender.Female } };

            var templateInput = @"
{% assign matches = Items | Where:'Gender','Male' %}
{% for match in matches %}
    {{ match.Name }}<br>
{% endfor %}
{% assign matches = Items | Where:'Gender',FemaleId %}
{% for match in matches %}
    {{ match.Name }}<br>
{% endfor %}
";

            var expectedOutput = @"
Ted Decker<br>Bill Marble<br>
Cindy Decker<br>Alex Decker<br>Alisha Marble<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_RepeatExecutions_ReturnsSameResult()
        {
            Debug.Write( "** Pass 1:" );

            Where_WithSingleConditionOnNestedProperty_ReturnsOnlyEqualValues();

            Debug.Write( "** Pass 2:" );

            Where_WithSingleConditionNotEqual_ReturnsOnlyNotEqualValues();
        }

        [TestMethod]
        public void Where_WithSingleConditionHavingNoMatches_ReturnsEmptyString()
        {
            var items = new List<Dictionary<string, object>>
                {
                   new Dictionary<string, object> { { "Id", "1" } },
                   new Dictionary<string, object> { { "Id", "2" } }
                };

            var mergeFields = new Dictionary<string, object> { { "Items", items } };

            var templateInput = @"
{% assign matches = Items | Where:'Id','3' %}
{% for match in matches %}
    {{ match.Id }}<br>
{% endfor %}
";
            var expectedOutput = @"";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_EmptyInputSetWithSingleCondition_ReturnsEmptyString()
        {
            var items = new List<TestWhereFilterCollectionItem>();

            var mergeFields = new Dictionary<string, object> { { "Items", items } };

            var templateInput = @"
{% assign matches = Items | Where:'Id','1' %}
{% for match in matches %}
    {{ match.Id }}<br>
{% endfor %}
";
            var expectedOutput = @"";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_EmptyInputSetWithMultipleConditions_ReturnsEmptyString()
        {
            var items = new List<TestWhereFilterCollectionItem>();

            var mergeFields = new Dictionary<string, object> { { "Items", items } };

            var templateInput = @"
{% assign matches = Items | Where:'Id ==""1"" || Id == ""2""' %}
{% for match in matches %}
    {{ match.Id }}<br>
{% endfor %}
";
            var expectedOutput = @"";

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields } );
        }

        [TestMethod]
        public void Where_FilterStringAppliedToSqlBlockResults_ReturnsFilteredRecords()
        {
            var templateInput = @"
{% sql %}
    SELECT [NickName], [LastName] FROM [Person] 
{% endsql %}
{% assign deckers = results | Where:'LastName == ""Decker""' %}
{% for person in deckers %}
            {{ person.NickName }}
            {{ person.LastName }} <br/>
{% endfor %}
            ";

            var expectedOutput = @"
Ted Decker<br/>Cindy Decker<br/>Noah Decker<br/>Alex Decker<br/>
";

            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            TestHelper.AssertTemplateOutput( expectedOutput, templateInput, new LavaTestRenderOptions { MergeFields = mergeFields, EnabledCommands = "sql" } );
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

        private Person GetWhereFilterTestPersonTedDecker()
        {
            var rockContext = new RockContext();

            var personTedDecker = new PersonService( rockContext ).Queryable()
                .FirstOrDefault( x => x.LastName == "Decker" && x.NickName == "Ted" );

            var phones = personTedDecker.PhoneNumbers;

            Assert.That.IsNotNull( personTedDecker, "Test person not found in current database." );

            return personTedDecker;
        }

        private class TestWhereFilterCollectionItem : RockDynamic
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public Gender Gender { get; set; }
        }

        #endregion

        #region ReadCookie/WriteCookie

        [TestMethod]
        public void WriteCookie_ForExistingCookie_RendersCookieValue()
        {
            var template = @"{{ 'cookie1' | WriteCookie:'oatmeal' }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    engine.RenderTemplate( template );

                    var cookie = GetExistingCookie( simulator, "cookie1" );

                    Assert.That.AreEqual( "oatmeal", cookie.Value );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithInvalidKey_RendersErrorMessage()
        {
            var template = @"{{ '' | WriteCookie:'fudge' }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    TestHelper.AssertTemplateOutput( "WriteCookie failed: A Key must be specified.", template );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithExpiry_HasCorrectExpiryTime()
        {
            // Set a cookie to expire in 30 minutes.
            var template = "{{ 'cookie1' | WriteCookie:'oreo','30' }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    // Write the cookie value and verify that it exists.
                    engine.RenderTemplate( template );

                    var cookie = GetExistingCookie( simulator, "cookie1" );

                    Assert.That.AreProximate( cookie.Expires, RockInstanceConfig.SystemDateTime, new System.TimeSpan( 0, 35, 0 ) );
                }
            } );
        }

        [TestMethod]
        public void WriteCookie_WithNoCurrentHttpRequest_RendersErrorMessage()
        {
            var template = @"{{ 'cookie1' | WriteCookie:'fudge' }}";
            var expectedOutput = "WriteCookie failed: A Http Session is required.";

            TestHelper.AssertTemplateOutput( expectedOutput, template );
        }

        [TestMethod]
        public void ReadCookie_ForExistingCookie_RendersCookieValue()
        {
            var template = @"{{ 'cookie1' | ReadCookie }}";
            var expectedValue = "choc-chip";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    // Set the cookie in the response.
                    simulator.Context.Response.Cookies.Add( new HttpCookie( "cookie1", expectedValue ) );

                    TestHelper.AssertTemplateOutput( engine, expectedValue, template );
                }
            } );
        }

        [TestMethod]
        public void ReadCookie_ForNonExistentCookie_RendersEmptyString()
        {
            var template = @"{{ 'invalid_cookie_key' | ReadCookie }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var simulator = new Http.TestLibrary.HttpSimulator();

                using ( simulator.SimulateRequest() )
                {
                    TestHelper.AssertTemplateOutput( engine, string.Empty, template );
                }
            } );
        }

        [TestMethod]
        public void ReadCookie_WithNoCurrentHttpRequest_RendersEmptyString()
        {
            var template = @"{{ 'invalid_cookie_key' | ReadCookie }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                TestHelper.AssertTemplateOutput( engine, string.Empty, template );
            } );
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

        #region PageRoute

        [TestMethod]
        public void PageRoute_ForPageNumber_EmitsUrl()
        {
            var pageId = GetPageIdFromRouteName( "Admin" );
            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                TestHelper.AssertTemplateOutput( "/Admin", "{{ " + pageId + " | PageRoute }}" );
            }
        }

        [TestMethod]
        public void PageRoute_WithQueryParameter_EmitsUrlWithQueryString()
        {
            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                TestHelper.AssertTemplateOutput( "/page/12?PersonID=10&GroupId=20", "{{ '12' | PageRoute:'PersonID=10^GroupId=20' }}" );
            }
        }

        #endregion

        #region ResolveRockUrl

        [TestMethod]
        public void ResolveRockUrl_WithCurrentHttpRequest_ReturnsAbsoluteUrl()
        {
            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: true );

            TestHelper.AssertTemplateOutput( fluidEngine,
                "MyRockInstance/page/999",
                @"{{ '~/page/999' | ResolveRockUrl }}",
                new LavaTestRenderOptions() );

            TestHelper.AssertTemplateOutput( fluidEngine,
                "MyRockInstance/Themes/MyTheme/page/999",
                @"{{ '~~/page/999' | ResolveRockUrl }}",
                new LavaTestRenderOptions() );

            // RockLiquid Engine.
            // There is no simple means of mocking the RockPage request handler required to test this scenario.
        }

        [TestMethod]
        public void ResolveRockUrl_WithNoHttpRequest_ReturnsAbsoluteUrlForDefaultSite()
        {
            var rootUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );

            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: false );
            TestHelper.AssertTemplateOutput( fluidEngine, $"{rootUrl}Person/1",
                @"{{ '~/Person/1' | ResolveRockUrl }}" );
            TestHelper.AssertTemplateOutput( fluidEngine, $"{rootUrl}Themes/MyTheme/Person/1",
                @"{{ '~~/Person/1' | ResolveRockUrl }}" );

            // RockLiquid Engine.
            // Note that there is no way to mock the theme setting in RockLiquid, so the default "Rock" theme is expected.
            TestHelper.AssertTemplateOutput( typeof( Rock.Lava.RockLiquid.RockLiquidEngine ),
                $"{rootUrl}Person/1",
                @"{{ '~/Person/1' | ResolveRockUrl }}" );
            TestHelper.AssertTemplateOutput( typeof( Rock.Lava.RockLiquid.RockLiquidEngine ),
                $"{rootUrl}Themes/Rock/Person/1",
                @"{{ '~~/Person/1' | ResolveRockUrl }}" );
        }

        [TestMethod]
        public void ResolveRockUrl_WithAbsoluteUrl_ReturnsInputUnchanged()
        {
            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: true );
            TestHelper.AssertTemplateOutput( fluidEngine,
            $"http://www.microsoft.com/",
            @"{{ 'http://www.microsoft.com/' | ResolveRockUrl }}" );

            // RockLiquid Engine.
            var simulator = new Http.TestLibrary.HttpSimulator();
            using ( simulator.SimulateRequest() )
            {
                TestHelper.AssertTemplateOutput( typeof( Rock.Lava.RockLiquid.RockLiquidEngine ),
                $"http://www.microsoft.com/",
                @"{{ 'http://www.microsoft.com/' | ResolveRockUrl }}" );
            }
        }

        private ILavaEngine GetFluidEngineWithMockHost( bool hasHttpRequest )
        {
            var host = new MockWebsiteLavaHost()
            {
                ApplicationPath = "MyRockInstance/",
                ThemeName = "MyTheme",
                HasActiveHttpRequest = hasHttpRequest
            };
            var fluidEngine = LavaService.NewEngineInstance( typeof( Rock.Lava.Fluid.FluidEngine ),
                    new LavaEngineConfigurationOptions { HostService = host } );

            return fluidEngine;
        }

        #endregion

        #region SetUrlParameter

        [TestMethod]
        public void SetUrlParameter_ModifyRockSiteUrlRoutePageParameterToNewRoute_RendersUrlWithUpdatedPageRoute()
        {
            // If the new page reference has a specific route, it should be returned in preference
            // to the default "/page/{pageId}" route.
            var pageId = GetPageIdFromRouteName( "Login" );

            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/page/19",
                "PageId",
                pageId.ToString(),
                "relative",
                "/Login" );
        }

        [TestMethod]
        public void SetUrlParameter_AddRockSiteRouteUrlQueryParameter_RendersUrlWithNewParameter()
        {
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports/2",
                "ResultLimit",
                "50",
                "relative",
                "/reporting/reports/2?ResultLimit=50" );
        }

        [TestMethod]
        public void SetUrlParameter_AddRockSitePageUrlQueryParameter_RendersUrlWithNewParameter()
        {
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/page/9999",
                "ReportId",
                "1",
                "relative",
                "/page/9999?ReportId=1" );
        }

        [TestMethod]
        public void SetUrlParameter_ModifyRockSiteUrlRouteParameter_RendersUrlWithUpdatedRoute()
        {
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports/2?Param1=1&Param2=2",
                "ReportId",
                "9",
                "relative",
                "/reporting/reports/9?Param1=1&Param2=2" );
        }

        [TestMethod]
        public void SetUrlParameter_SetRockSiteUrlRouteParameterToEmpty_RendersUrlWithParameterRemoved()
        {
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports/2?Param1=1&Param2=2",
                "ReportId",
                string.Empty,
                "relative",
                "/reporting/reports?Param1=1&Param2=2" );
        }

        [TestMethod]
        public void SetUrlParameter_AddRockSiteUrlRouteParameter_RendersUrlWithUpdatedRoute()
        {
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports?Param1=1&Param2=2",
                "ReportId",
                "9",
                "relative",
                "/reporting/reports/9?Param1=1&Param2=2" );
        }

        [TestMethod]
        public void SetUrlParameter_ModifyRockSiteUrlToAlternateRoute_RendersUrlWithUpdatedRoute()
        {
            var newPageId = GetPageIdFromRouteName( "person/{PersonId}/contributions" );

            // Change PageId from "person/{PersonId}/groups" (Page 175)
            // to "person/{PersonId}/contributions" (Page 177).
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/person/1/groups",
                "PageId",
                newPageId.ToString(),
                "relative",
                "/person/1/contributions" );
        }

        [TestMethod]
        public void SetUrlParameter_WithUrlTypeOption_RendersUrlOfSpecifiedType()
        {
            // URL Format: Full
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports?ReportId=1",
                "ReportId",
                "9",
                "full",
                "http://prealpha.rocksolidchurchdemo.com/reporting/reports/9" );
            // URL Format: Relative
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports?ReportId=1",
                "ReportId",
                "9",
                "relative",
                "/reporting/reports/9" );
            // URL Format: Unspecified
            SetUrlParameterRenderTemplateAssert( "http://prealpha.rocksolidchurchdemo.com/reporting/reports?ReportId=1",
                "ReportId",
                "9",
                string.Empty,
                "http://prealpha.rocksolidchurchdemo.com/reporting/reports/9" );
        }

        [TestMethod]
        public void SetUrlParameter_ModifyExternalSiteUrlQueryParameter_RendersUrlWithUpdatedQueryParameter()
        {
            SetUrlParameterRenderTemplateAssert( "https://www.biblegateway.com/passage/?search=john%203%3A16&version=KJV",
                "version",
                "NIV",
                "full",
                "https://www.biblegateway.com/passage/?search=john%203:16&version=NIV" );
        }

        [TestMethod]
        public void SetUrlParameter_WithCurrentInputString_RendersCurrentUrl()
        {
            var inputUrl = "http://www.mysite.com";
            var simulator = new Http.TestLibrary.HttpSimulator();
            using ( simulator.SimulateRequest( new Uri( inputUrl ) ) )
            {
                TestHelper.AssertTemplateOutput( "http://www.mysite.com/",
                    "{{ 'current' | SetUrlParameter }}" );
            }
        }

        [TestMethod]
        public void SetUrlParameter_WithInvalidInputString_RendersInputUnchanged()
        {
            TestHelper.AssertTemplateOutput( "this_is_not_a_url!",
                "{{ 'this_is_not_a_url!' | SetUrlParameter:'Param1','2','full' }}" );
        }

        private static void SetUrlParameterRenderTemplateAssert( string inputUrl, string parameterName, string newValue, string outputUrlFormat, string expectedOutput )
        {
            var simulator = new Http.TestLibrary.HttpSimulator();
            using ( simulator.SimulateRequest( new Uri( inputUrl ) ) )
            {
                TestHelper.AssertTemplateOutput( expectedOutput,
                    "{{ '" + inputUrl + "' | SetUrlParameter:'" + parameterName + "','" + newValue + "','" + outputUrlFormat + "' }}" );
            }
        }

        private int GetPageIdFromRouteName( string routeName )
        {
            // If the new page reference has a specific route, it should be returned in preference
            // to the default "/page/{pageId}" route.
            var dataContext = new RockContext();
            var routeService = new PageRouteService( dataContext );

            var loginRoute = routeService.Queryable()
                .FirstOrDefault( x => x.Route == routeName );

            return loginRoute?.PageId ?? 0;
        }

        #endregion

        #region Web Cache

        [TestMethod]
        public void SetCache_WithQueryParameter_EmitsUrlWithQueryString()
        {
            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                TestHelper.AssertTemplateOutput( "/page/12?PersonID=10&GroupId=20", "{{ '12' | PageRoute:'PersonID=10^GroupId=20' }}" );
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Mocks the WebsiteLavaHost for a test environment in absence of the ASP.Net processing pipeline.
        /// </summary>
        internal class MockWebsiteLavaHost : WebsiteLavaHost
        {
            public bool HasActiveHttpRequest { get; set; } = true;
            public string ApplicationPath { get; set; } = "/";
            public string ThemeName { get; set; } = "Rock";

            internal override HttpRequest GetCurrentRequest()
            {
                if ( HasActiveHttpRequest )
                {
                    var simulator = new Http.TestLibrary.HttpSimulator( "/MyRockWeb" );
                    simulator.SimulateRequest();

                    return base.GetCurrentRequest();
                }
                return null;
            }

            protected override string GetCurrentThemeName()
            {
                return ThemeName;
            }

            internal override string ResolveVirtualPath( string virtualPath )
            {
                if ( HasActiveHttpRequest )
                {
                    if ( virtualPath.StartsWith( "~" ) )
                    {
                        virtualPath = ApplicationPath + virtualPath.TrimStart( '~' ).TrimStart( '/' );
                    }
                }
                else
                {
                    // If we are mocking an action with no associated HttpRequest, return the externalURL.

                }
                return virtualPath;
            }
        }

        #endregion
    }
}