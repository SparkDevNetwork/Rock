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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.Events;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

using static Rock.Tests.Integration.Events.EventsDataManager;

namespace Rock.Tests.Integration.Modules.Core.Lava
{
    /// <summary>
    /// Tests for Lava Command "EventScheduledInstance".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class EventScheduledInstanceCommandTests : LavaIntegrationTestBase
    {
        private static string StaffMeetingEventGuidString = "93104654-DAFA-489B-A175-5F2AB3A846F1";

        private static string LavaTemplateEventOccurrences = @";
{% eventscheduledinstance {parameters} %}
  {% assign eventItemOccurrenceCount = EventScheduledInstances | Size %}
  <<EventCount = {{ EventScheduledInstances | Size }}>>
  {% for eventItemOccurrence in EventScheduledInstances %}
    <<{{ eventItemOccurrence.Name }}|{{ eventItemOccurrence.Date | Date: 'yyyy-MM-dd' }}|{{ eventItemOccurrence.Time }}|{{ eventItemOccurrence.Location }}>>
    <<Calendars: {{ eventItemOccurrence.CalendarNames | Join:', ' }}>>
    <<Audiences: {{ eventItemOccurrence.AudienceNames | Join:', ' }}>>
    <<Campus: {{ eventItemOccurrence.Campus }}>>
  {% endfor %}
{% endeventscheduledinstance %}
";

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            EventsDataManager.Instance.AddDataForRockSolidFinancesClass();
        }

        private string GetTestTemplate( string parameters )
        {
            var template = LavaTemplateEventOccurrences;

            return template.Replace( "{parameters}", parameters );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithUnknownParameterName_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'1' unknown_parameter:'any_value'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"unknown_parameter\"." );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventAsName_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventAsId_RetrievesOccurrencesInCorrectEvent()
        {
            // Get Event Item Id for "Warrior Youth Event".
            var rockContext = new RockContext();

            var eventItemService = new EventItemService( rockContext );
            var eventId = eventItemService.GetId( StaffMeetingEventGuidString.AsGuid() );

            Assert.That.IsNotNull( eventId, "Expected test data not found." );

            var template = GetTestTemplate( $"eventid:{eventId} startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            } );

        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventAsGuid_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( $"eventid:'{StaffMeetingEventGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "Event Occurrences not available. An Event reference must be specified." );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'no_event' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "Event Occurrences not available. Cannot find an Event matching the reference \"no_event\"." );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'3m'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-02-26|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-03-25|12:00 AM|All Campuses>>" );

                // Staff Meeting recurs every 2 weeks, so our date range of 3 months weeks should not include the meeting in month 4.
                Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-04-08|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'4w'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Staff Meeting recurs every 2 weeks, so our date range of 4 weeks should only include 2 occurrences.
                Assert.That.Contains( output, "<<EventCount = 2>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
                Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

                Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            const string NewEventGuid = "F7CE040E-CD1C-41E0-81D7-9F88BCDAF6A7";
            const string testScheduleGuid = "E31C0108-3F3A-4101-B135-A7B8482A226C";

            var rockContext = new RockContext();

            // Create a new Event that occurs daily.
            var startDateTime = RockDateTime.New( 2020, 1, 1 ).Value.AddHours( 19 ).AddMinutes( 30 );
            var endDate = startDateTime.AddMonths( 3 );

            var schedule = ScheduleTestHelper.GetScheduleWithDailyRecurrence( startDateTime, endDate, new TimeSpan( 1, 0, 0 ) );

            var addScheduleArgs = new EventsDataManager.AddScheduleDailyRecurrenceActionArgs
            {
                Guid = testScheduleGuid.AsGuid(),
                StartDateTime = startDateTime,
                EndDateTime = endDate,
                EventDuration = new TimeSpan( 1, 0, 0 )
            };
            var createEventInfo = new CreateEventItemActionArgs
            {
                Guid = NewEventGuid.AsGuid(),
                Properties = new EventItemInfo
                {
                    EventName = "Test Daily Event"
                }
            };
            var createOccurrenceInfo = new CreateEventItemOccurrenceActionArgs
            {
                Properties = new EventItemOccurrenceInfo
                {
                    EventIdentifier = NewEventGuid,
                    MeetingLocationDescription = "Test Location",
                    ScheduleIdentifier = testScheduleGuid,
                    CampusIdentifier = "Main Campus"
                }
            };

            var eventsManager = EventsDataManager.Instance;
            eventsManager.AddScheduleWithDailyRecurrence( addScheduleArgs );

            eventsManager.DeleteEventItem( NewEventGuid, rockContext );
            rockContext.SaveChanges();

            eventsManager.AddEventItem( createEventInfo );
            eventsManager.AddEventItemOccurrence( createOccurrenceInfo );

            try
            {
                // A request for a date range of 1 day with 2020-01-01 as the first day should yield 1 entry.
                var templateDays01 = GetTestTemplate( "eventid:'Test Daily Event' startdate:'2020-01-01' daterange:'1d'" );

                // A request for a date range of 10 days with 2020-01-01 as the first day should yield 10 entries,
                // from 2020-01-01 to 2020-01-10.
                var templateDays10 = GetTestTemplate( "eventid:'Test Daily Event' startdate:'2020-01-01' daterange:'10d'" );

                TestHelper.ExecuteForActiveEngines( ( engine ) =>
                {
                    // Verify output for 1 day range.
                    var output01 = TestHelper.GetTemplateOutput( engine, templateDays01 );

                    Assert.That.Contains( output01, "<<EventCount = 1>>" );
                    Assert.That.Contains( output01, "<<Test Daily Event|2020-01-01|7:30 PM|Main Campus>>" );
                    Assert.That.DoesNotContain( output01, "<<Test Daily Event|2020-01-02|7:30 PM|Main Campus>>" );

                    // Verify output for 10 day range.
                    var output10 = TestHelper.GetTemplateOutput( engine, templateDays10 );

                    Assert.That.Contains( output10, "<<EventCount = 10>>" );
                    Assert.That.Contains( output10, "<<Test Daily Event|2020-01-01|7:30 PM|Main Campus>>" );
                    Assert.That.Contains( output10, "<<Test Daily Event|2020-01-10|7:30 PM|Main Campus>>" );
                    Assert.That.DoesNotContain( output10, "<<Test Daily Event|2020-01-11|7:30 PM|Main Campus>>" );
                } );
            }
            finally
            {
                // Remove the test event so it does not interfere with other tests.
                eventsManager.DeleteEventItem( NewEventGuid, rockContext );
                rockContext.SaveChanges();
            }
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'1020-1-1' daterange:'12m'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<EventCount = 0>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:200" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Ensure that the maximum number of occurrences has been retrieved.
                Assert.That.Contains( output, "<EventCount = 200>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' daterange:'invalid'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "Event Occurrences not available. The specified Date Range is invalid." );
            } );            
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
        {
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // First, ensure that there are more than the default maximum number of events to return.
                // The default maximum is 100 events.
                var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:101" );

                var output1 = TestHelper.GetTemplateOutput( engine, template1 );

                TestHelper.DebugWriteRenderResult( engine, template1, output1 );

                Assert.That.Contains( output1, "<EventCount = 101>" );

                // Now ensure that the default limit is applied.
                var template2 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1'" );

                var output2 = TestHelper.GetTemplateOutput( engine, template2 );

                TestHelper.DebugWriteRenderResult( engine, template2, output2 );

                Assert.That.Contains( output2, "<EventCount = 100>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
        {
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // First, ensure that there are more than the test maximum number of events to return.
                var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:11" );

                var output1 = TestHelper.GetTemplateOutput( engine, template1 );

                Assert.That.Contains( output1, "<EventCount = 11>" );

                // Now ensure that the maxoccurences limit is applied.
                var template2 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:10" );

                var output2 = TestHelper.GetTemplateOutput( engine, template2 );

                Assert.That.Contains( output2, "<EventCount = 10>" );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"maxoccurrences\"." );
            } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_EventWithMultipleSchedules_ReturnsMultipleEventItemEntries()
        {
            var template = @"
{% eventscheduledinstance eventid:'Rock Solid Finances Class' startdate:'2020-1-1' maxoccurrences:'25' daterange:'2m' %}
    {% for occurrence in EventItems %}
        <b>Series {{forloop.index}}</b><br>
        {% for item in occurrence %}
            {% if forloop.first %}
                {{ item.Name }}
                <b>{{ item.DateTime | Date:'dddd' }} Series</b><br>
                <ol>
            {% endif %}
            <li>{{ item.DateTime | Date:'MMM d, yyyy' }} in {{ item.LocationDescription }}</li>
            {% if forloop.last %}
                </ol>
            {% endif %}
        {% endfor %}
    {% endfor %}
{% endeventscheduledinstance %}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<b>Series 1</b>" );
                Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                Assert.That.Contains( output, "<b>Series 2</b>" );
                Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }

        /// <summary>
        /// Retrieve information about a known event that exists in the Rock sample data set, and verify the information is accurate.
        /// </summary>
        [TestMethod]
        public void EventScheduledInstanceCommand_ForSampleDataKnownEvents_ReturnsExpectedEventData()
        {
            var input = @"
{% eventscheduledinstance eventid:'Customs & Classics Car Show' startdate:'2018-4-1' maxoccurrences:1 %}
    {% for item in EventScheduledInstances %}
        Name={{ item.Name }}<br>
        Date={{item.Date | Date:'yyyy-MM-dd' }}<br>
        Time={{ item.Time }}<br>
        DateTime={{ item.DateTime | Date:'yyyy-MM-ddTHH:mm:sszzz' }}
    {% endfor %}
{% endeventscheduledinstance %}
";

            var rockTimeOffset = LavaDateTime.ConvertToRockDateTime( new DateTime( 2021, 9, 1, 0, 0, 0, DateTimeKind.Unspecified ) ).ToString( "zzz" );

            var expectedOutput = $@"
Name=Customs & Classics Car Show<br>Date=2018-04-16<br>Time=9:24 AM<br>DateTime=2018-04-16T09:24:13<offset>
";
            expectedOutput = expectedOutput.Replace( "<offset>", rockTimeOffset );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithCampusAsName_RetrievesEventsWithMatchingCampus()
        {
            var template = GetTestTemplate( "eventid:'Rock Solid Finances Class' campusids:'Main Campus' startdate:'2018-1-1'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<Campus: Main Campus>" );
                Assert.That.DoesNotContain( output, "<Campus: Secondary Campus>" );
            } );
        }

        public void EventScheduledInstanceCommand_WithCampusAsMultipleValues_RetrievesEventsWithAnyMatchingCampus()
        {
            var template = GetTestTemplate( "eventid:'Rock Solid Finances Class' campusids:'Main Campus,Stepping Stone' startdate:'2020-1-1' daterange:'12m' maxoccurrences:99" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );

            TestHelper.AssertTemplateOutput( "<Campus: Stepping Stone>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithCampusAsId_RetrievesEventsWithMatchingCampus()
        {
            var rockContext = new RockContext();

            var campusId = new CampusService( rockContext ).Queryable()
                .FirstOrDefault().Id;

            var template = GetTestTemplate( $"eventid:'Rock Solid Finances Class' campusids:'{campusId}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithCampusAsGuid_RetrievesEventsWithMatchingCampus()
        {
            var template = GetTestTemplate( $"eventid:'Rock Solid Finances Class' campusids:'{MainCampusGuidString}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Campus: Main Campus>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithCampusInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' campusids:'no_campus'" );

            TestHelper.AssertTemplateOutput( "Event Occurrences not available. Cannot apply a campus filter for the reference \"no_campus\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithCampusSpecified_RetrievesEventsWithMatchingCampusAndUnspecifiedCampus()
        {
            // The "Warrior Youth Event" is not assigned to a specific Campus, so it should be returned for all campus filter values.
            var template = GetTestTemplate( $"eventid:'Warrior Youth Event' campusids:'{MainCampusGuidString}' startdate:'2018-1-1'" );

            TestHelper.AssertTemplateOutput( "<Campus: All Campuses>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        private static string MainCampusGuidString = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";
    }
}
