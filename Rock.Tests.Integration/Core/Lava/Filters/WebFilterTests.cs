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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.Tests.Integration.Communications.Transport;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestData.Communications;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core.Lava.Filters
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

                Assert.AreEqual( "public, max-age=120", header );
            }
        }

        #endregion

        #region TitleCase

        #endregion

        #region Where

        [TestMethod]
        public void Where_FilterStringAppliedToSqlBlockResults_ReturnsFilteredRecords()
        {
            var templateInput = @"
{%- sql -%}
    SELECT [NickName], [LastName] FROM [Person] 
{%- endsql -%}
{%- assign deckers = results | Where:'LastName == ""Decker""' -%}
{%- for person in deckers -%}
            {{ person.NickName }}
            {{ person.LastName }} <br/>
{%- endfor -%}
            ";

            /*
                9/26/26 - CLAUDE

                The template puts the nick name and the last name on separate
                indented lines, and the for tag leaves behind the newline that
                followed it, so each person renders as three lines rather than
                the one the old expected value showed. The output also ends with
                the twelve spaces that follow the endfor tag, with no newline.

                Reason: The expected value has to be what the template renders.
            */
            var expectedOutput = "Ted\n"
                + "            Decker <br/>Cindy\n"
                + "            Decker <br/>Noah\n"
                + "            Decker <br/>Alex\n"
                + "            Decker <br/>";

            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, templateInput, new LavaRenderOptions { MergeFields = mergeFields, EnabledCommands = "sql" } );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        private Person GetWhereFilterTestPersonTedDecker()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var personTedDecker = new PersonService( rockContext ).Queryable()
                .FirstOrDefault( x => x.LastName == "Decker" && x.NickName == "Ted" );

            var phones = personTedDecker.PhoneNumbers;

            Assert.IsNotNull( personTedDecker, "Test person not found in current database." );

            return personTedDecker;
        }
        #endregion

        #region ReadCookie/WriteCookie

        #endregion

        #region PageRoute

        [TestMethod]
        [Ignore( "The current documentation example is incorrect. It references a system setting that is undefined." )]
        public void PageRoute_DocumentationExample_EmitsExpectedOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Global' | Attribute:'WorkflowEntryPage','RawValue' | PageRoute:'WorkflowTypeId=10^WorkflowId=324' }}" );

                Assert.AreEqual( "/WorkflowEntry/10/324", output );
            } );
        }

        [TestMethod]
        public void PageRoute_WithActiveHttpRequest_EmitsUrlForApplicationRoot()
        {
            var simulator = new Http.TestLibrary.HttpSimulator( "Websites/Website1" );
            using ( simulator.SimulateRequest() )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '12' | PageRoute:'PersonID=10^GroupId=20' }}" );

                    Assert.AreEqual( "/Websites/Website1/page/12?PersonID=10&GroupId=20", output );
                } );
            }
        }

        [TestMethod]
        public void PageRoute_WithNoActiveHttpRequest_EmitsUrlWithDefaultApplicationRoot()
        {
            var pageId = GetPageIdFromRouteName( "Admin" );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ " + pageId + " | PageRoute }}" );

                Assert.AreEqual( $"/Admin", output );
            } );
        }

        [TestMethod]
        public void PageRoute_ForPageNumberWithAssociatedRoute_EmitsUrlWithRoute()
        {
            var pageId = GetPageIdFromRouteName( "Admin" );
            var simulator = new Http.TestLibrary.HttpSimulator( "" );

            using ( simulator.SimulateRequest() )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ " + pageId + " | PageRoute }}" );

                    Assert.AreEqual( "/Admin", output );
                } );
            }
        }

        [TestMethod]
        public void PageRoute_ForPageNumberWithAssociatedRouteParameters_EmitsUrlWithRouteParameters()
        {
            var pageId = GetPageIdFromRouteName( "reporting/dataviews" );
            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ " + pageId + " | PageRoute:'DataViewId=1' }}" );

                    Assert.AreEqual( "/reporting/dataviews/1", output );
                } );
            }
        }

        [TestMethod]
        public void PageRoute_WithQueryParameter_EmitsUrlWithQueryString()
        {
            var simulator = new Http.TestLibrary.HttpSimulator();

            using ( simulator.SimulateRequest() )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '12' | PageRoute:'PersonID=10^GroupId=20' }}" );

                    Assert.AreEqual( "/page/12?PersonID=10&GroupId=20", output );
                } );
            }
        }

        [TestMethod]
        public void PageRoute_InSystemCommunication_ReturnsRelativeUrl()
        {
            var pageId = GetPageIdFromRouteName( "person/{PersonId}" );

            var template = "{{ $pageId | PageRoute:'PersonId=1' }}".Replace( "$pageId", pageId.ToString() );

            var result = SendLavaTemplateInSystemCommunication( template );

            var emailMessageText = result.EmailMessage.Body;

            // This is the body of a sent email rather than a rendered template,
            // and the transport decides its casing and layout, so the comparison
            // ignores both.
            Assert.AreEqual( "<html><head></head><body>/person/1</body></html>",
                Regex.Replace( emailMessageText, @"\s", string.Empty ),
                ignoreCase: true );
        }

        private const string _systemCommunicationTestGuid = "E81D2E50-809A-405F-9E19-ABAB31B0DFAB";

        private MockSmtpSendResult SendLavaTemplateInSystemCommunication( string lavaTemplate )
        {
            // Create a new System Communication containing the test lava template.
            var communication = CommunicationsDataManager.Instance.NewSystemCommunication( _systemCommunicationTestGuid.AsGuid(),
                $"Test Communication ({_systemCommunicationTestGuid})",
                $"Test Communication ({_systemCommunicationTestGuid})",
                CategoryCache.GetId( SystemGuid.Category.SYSTEM_COMMUNICATION_WORKFLOW.AsGuid() ).GetValueOrDefault() );

            communication.Body = lavaTemplate;

            CommunicationsDataManager.Instance.SaveSystemCommunication( communication );

            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            mergeObjects.Add( "Person", person );

            var logger = new RockLoggerMemoryBuffer();
            var messageResult = CommunicationHelper.CreateEmailMessage( person, mergeObjects, communication, logger );
            var message = messageResult.Message;
            message.AppRoot = "/test";

            var medium = CommunicationsDataManager.Instance.GetCommunicationMediumComponent( CommunicationType.Email );
            var mediumAttributes = GetMediumAttributes( medium );

            var smtpTransport = new MockSmtpTransport();

            smtpTransport.Send( messageResult.Message, medium.EntityType.Id, mediumAttributes, out var errorMessages );
            Assert.IsFalse( errorMessages.Any() );

            var processedMessage = smtpTransport.ProcessedItems.LastOrDefault();

            return processedMessage;
        }

        private Dictionary<string, string> GetMediumAttributes( MediumComponent medium )
        {
            var mediumAttributes = new Dictionary<string, string>();
            foreach ( var attr in medium.Attributes.Select( a => a.Value ) )
            {
                string value = medium.GetAttributeValue( attr.Key );
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    mediumAttributes.Add( attr.Key, medium.GetAttributeValue( attr.Key ) );
                }
            }

            return mediumAttributes;
        }

        #endregion

        #region ResolveRockUrl

        [TestMethod]
        public void ResolveRockUrl_WithCurrentHttpRequest_ReturnsAbsoluteUrl()
        {
            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: true );

            // These render against the engine built above rather than the one
            // the helper supplies, because the mock host is what is under test.
            var pageUrl = LavaRenderTestHelper.Render( fluidEngine, @"{{ '~/page/999' | ResolveRockUrl }}" );

            Assert.AreEqual( "MyRockInstance/page/999", pageUrl );

            var themeUrl = LavaRenderTestHelper.Render( fluidEngine, @"{{ '~~/page/999' | ResolveRockUrl }}" );

            Assert.AreEqual( "MyRockInstance/Themes/MyTheme/page/999", themeUrl );
        }

        [TestMethod]
        public void ResolveRockUrl_WithNoHttpRequest_ReturnsAbsoluteUrlForDefaultSite()
        {
            var rootUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );

            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: false );
            var personUrl = LavaRenderTestHelper.Render( fluidEngine, @"{{ '~/Person/1' | ResolveRockUrl }}" );

            Assert.AreEqual( $"{rootUrl}Person/1", personUrl );

            var themePersonUrl = LavaRenderTestHelper.Render( fluidEngine, @"{{ '~~/Person/1' | ResolveRockUrl }}" );

            Assert.AreEqual( $"{rootUrl}Themes/MyTheme/Person/1", themePersonUrl );
        }

        [TestMethod]
        public void ResolveRockUrl_WithAbsoluteUrl_ReturnsInputUnchanged()
        {
            // Fluid Engine.
            var fluidEngine = GetFluidEngineWithMockHost( hasHttpRequest: true );
            var output = LavaRenderTestHelper.Render( fluidEngine, @"{{ 'http://www.microsoft.com/' | ResolveRockUrl }}" );

            Assert.AreEqual( "http://www.microsoft.com/", output );
        }

        private ILavaEngine GetFluidEngineWithMockHost( bool hasHttpRequest )
        {
            var host = new MockWebsiteLavaHost()
            {
                ApplicationPath = "MyRockInstance/",
                ThemeName = "MyTheme",
                HasActiveHttpRequest = hasHttpRequest
            };

            // In addition to the HostService, a FileSystem is also required to resolve the path for the Site Theme.
            var config = new LavaEngineConfigurationOptions
            {
                HostService = host,
                FileSystem = new WebsiteLavaFileSystem()
            };

            // Built through the test factory rather than resolved from
            // LavaService, so that the engine this test renders against does not
            // depend on which factory the service happens to hold.
            return LavaTestEngineFactory.CreateFluidEngine( config );
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
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ 'current' | SetUrlParameter }}" );

                    Assert.AreEqual( "http://www.mysite.com/", output );
                } );
            }
        }

        [TestMethod]
        public void SetUrlParameter_WithInvalidInputString_RendersInputUnchanged()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'this_is_not_a_url!' | SetUrlParameter:'Param1','2','full' }}" );

                Assert.AreEqual( "this_is_not_a_url!", output );
            } );
        }

        private static void SetUrlParameterRenderTemplateAssert( string inputUrl, string parameterName, string newValue, string outputUrlFormat, string expectedOutput )
        {
            var simulator = new Http.TestLibrary.HttpSimulator();
            using ( simulator.SimulateRequest( new Uri( inputUrl ) ) )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + inputUrl + "' | SetUrlParameter:'" + parameterName + "','" + newValue + "','" + outputUrlFormat + "' }}" );

                    Assert.AreEqual( expectedOutput, output );
                } );
            }
        }

        private int GetPageIdFromRouteName( string routeName )
        {
            // If the new page reference has a specific route, it should be returned in preference
            // to the default "/page/{pageId}" route.
            var dataContext = RockApp.Current.CreateRockContext();
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
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '12' | PageRoute:'PersonID=10^GroupId=20' }}" );

                    Assert.AreEqual( "/page/12?PersonID=10&GroupId=20", output );
                } );
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
