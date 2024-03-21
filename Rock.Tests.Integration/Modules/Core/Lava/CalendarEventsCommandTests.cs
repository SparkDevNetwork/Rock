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

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.Events;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava
{
    /// <summary>
    /// Tests for Lava Command "CalendarEvents".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class CalendarEventsCommandTests : LavaIntegrationTestBase
    {
        private static string LavaTemplateCalendarEvents = @"
{% calendarevents {parameters} %}
  {% assign eventScheduledInstanceCount = EventScheduledInstances | Size %}
  <<EventCount = {{ EventScheduledInstances | Size }}>>
  {% for eventScheduledInstance in EventScheduledInstances %}
    <<{{ eventScheduledInstance.Name }}|{{ eventScheduledInstance.Date | Date: 'yyyy-MM-dd' }}|{{ eventScheduledInstance.Time }}|{{ eventScheduledInstance.Location }}>>
    <<Calendars: {{ eventScheduledInstance.CalendarNames | Join:', ' }}>>
    <<Audiences: {{ eventScheduledInstance.AudienceNames | Join:', ' }}>>
    <<Campus: {{ eventScheduledInstance.Campus }}>>
  {% endfor %}
{% endcalendarevents %}
";

        private static string InternalCalendarGuidString = "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798";
        private static string YouthAudienceGuidString = "59CD7FD8-6A62-4C3B-8966-1520E74EED58";
        private static string MainCampusGuidString = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            EventsDataManager.Instance.AddDataForRockSolidFinancesClass();
        }

        private string GetTestTemplate( string parameters )
        {
            var template = LavaTemplateCalendarEvents;

            return template.Replace( "{parameters}", parameters );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithUnknownParameterName_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' unknown_parameter:'any_value'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Invalid configuration setting \"unknown_parameter\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsName_RetrievesEventsInCorrectCalendar()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Calendars: Internal>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsId_RetrievesEventsInCorrectCalendar()
        {
            // CalendarId = 1 represents the Public calendar in the standard test data.
            var template = GetTestTemplate( "calendarid:'1' startdate:'2018-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Calendars: Internal, Public>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsGuid_RetrievesEventsInCorrectCalendar()
        {
            var template = GetTestTemplate( $"calendarid:'{InternalCalendarGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Calendars: Internal>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. A calendar reference must be specified.",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'no_calendar' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Cannot find a calendar matching the reference \"no_calendar\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceAsName_RetrievesEventsWithMatchingAudience()
        {
            // This filter should return the Warrior Youth Event scheduled once on 2018-05-02.
            var template = GetTestTemplate( "calendarid:'Public' audienceids:'Youth' startdate:'2018-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Audiences: All Church, Adults, Youth>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        public void CalendarEventsCommand_WithAudienceAsMultipleValues_RetrievesEventsWithAnyMatchingAudience()
        {
            var template = GetTestTemplate( "calendarid:'Public' audienceids:'Men,Women' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Audiences: Internal>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceAsId_RetrievesEventsWithMatchingAudience()
        {
            var rockContext = new RockContext();

            var audienceGuid = SystemGuid.DefinedType.CONTENT_CHANNEL_AUDIENCE_TYPE.AsGuid();

            var definedValueId = new DefinedTypeService( rockContext ).Queryable()
                .FirstOrDefault( x => x.Guid == audienceGuid )
                .DefinedValues.FirstOrDefault( x => x.Value == "All Church" ).Id;

            var template = GetTestTemplate( $"calendarid:'Public' audienceids:'{definedValueId}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Audiences: All Church,",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceAsGuid_RetrievesEventsWithMatchingAudience()
        {
            var template = GetTestTemplate( $"calendarid:'Public' audienceids:'{YouthAudienceGuidString}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Audiences: All Church, Adults, Youth>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' audienceids:'no_audience'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Cannot apply an audience filter for the reference \"no_audience\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCampusAsName_RetrievesEventsWithMatchingCampus()
        {
            // This filter should return the Warrior Youth Event scheduled once on 2018-05-02.
            var template = GetTestTemplate( "calendarid:'Public' campusids:'Main Campus' startdate:'2018-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        public void CalendarEventsCommand_WithCampusAsMultipleValues_RetrievesEventsWithAnyMatchingCampus()
        {
            var template = GetTestTemplate( "calendarid:'Public' campusids:'Main Campus,Stepping Stone' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );

            TestHelper.AssertTemplateOutput( "<Campus: Stepping Stone>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCampusAsId_RetrievesEventsWithMatchingCampus()
        {
            var rockContext = new RockContext();

            var campusId = new CampusService( rockContext ).Queryable()
                .FirstOrDefault().Id;

            var template = GetTestTemplate( $"calendarid:'Public' campusids:'{campusId}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCampusAsGuid_RetrievesEventsWithMatchingCampus()
        {
            var template = GetTestTemplate( $"calendarid:'Public' campusids:'{MainCampusGuidString}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCampusSpecified_RetrievesEventsWithMatchingCampusAndUnspecifiedCampus()
        {
            var rockContext = new RockContext();

            var campusId = new CampusService( rockContext ).Get( MainCampusGuidString.AsGuid() ).Id;

            var template = GetTestTemplate( $"calendarid:'Public' campusids:'{campusId}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( new List<string> { "<Campus: Main Campus>", "<Campus: All Campuses>" },
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCampusInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' campusids:'no_campus'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Cannot apply a campus filter for the reference \"no_campus\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'3m'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( template );

                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-02-26|12:00 AM|All Campuses>>" );
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-03-25|12:00 AM|All Campuses>>" );

                // Staff Meeting recurs every 2 weeks, so our date range of 3 months weeks should not include the meeting in month 4.
                Assert.That.DoesNotContain( result.Text, "<<Staff Meeting|2020-04-08|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'3w'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( template );

                // Staff Meeting recurs every 2 weeks, so our date range should only include 2 occurrences.
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

                Assert.That.DoesNotContain( result.Text, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'27d'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( template );

                // Staff Meeting recurs every 2 weeks, so our date range of 27d should only include 2 occurrences.
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( result.Text, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

                Assert.That.DoesNotContain( result.Text, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'1020-1-1' daterange:'12m'" );

            TestHelper.AssertTemplateOutput( "<EventCount = 0>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:200" );

            // Ensure that the maximum number of occurrences has been retrieved.
            TestHelper.AssertTemplateOutput( "<EventCount = 200>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' daterange:'invalid'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. The specified Date Range is invalid.",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
        {
            // First, ensure that there are more than the default maximum number of events to return.
            // The default maximum is 100 events.
            var template1 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:101" );

            TestHelper.AssertTemplateOutput( "<EventCount = 101>",
                template1,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );

            // Now ensure that the default limit is applied.
            var template2 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1'" );

            TestHelper.AssertTemplateOutput( "<EventCount = 100>",
                template2,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
        {
            // First, ensure that there are more than the test maximum number of events to return.
            var template1 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:11" );

            TestHelper.AssertTemplateOutput( "<EventCount = 11>",
                template1,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );

            // Now ensure that the maxoccurences limit is applied.
            var template2 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:10" );

            TestHelper.AssertTemplateOutput( "<EventCount = 10>",
                template2,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Invalid configuration setting \"maxoccurrences\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        /// <summary>
        /// Retrieve information about a known event that exists in the Rock sample data set, and verify the information is accurate.
        /// </summary>
        [TestMethod]
        public void CalendarEventsCommand_ForSampleDataKnownEvents_ReturnsExpectedEventData()
        {
            var input = @"
{% calendarevents calendarid:'Internal' startdate:'2021-1-1' maxoccurrences:2 %}
    {% for item in EventScheduledInstances %}
        Name={{ item.Name }}<br>
        Date={{ item.Date | Date:'yyyy-MM-dd' }}<br>
        Time={{ item.Time }}<br>
        DateTime={{ item.DateTime | Date:'yyyy-MM-ddTHH:mm:sszzz' }}
        <hr>
    {% endfor %}
{% endcalendarevents %}
";
            var rockTimeOffset = LavaDateTime.ConvertToRockDateTime( new DateTime( 2021, 1, 1, 0, 0, 0, DateTimeKind.Unspecified ) ).ToString( "zzz" );

            var expectedOutput = @"
Name=Rock Solid Finances Class<br>Date=2021-01-02<br>Time=4:30 PM<br>DateTime=2021-01-02T16:30:00<offset><hr>
Name=Rock Solid Finances Class<br>Date=2021-01-03<br>Time=12:00 PM<br>DateTime=2021-01-03T12:00:00<offset><hr>
";
            expectedOutput = expectedOutput.Replace( "<offset>", rockTimeOffset );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }
    }
}
