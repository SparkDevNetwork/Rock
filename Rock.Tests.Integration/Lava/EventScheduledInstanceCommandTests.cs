using System.Linq;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
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
  {% endfor %}
{% endeventscheduledinstance %}
";

        private string GetTestTemplate( string parameters )
        {
            var template = LavaTemplateEventOccurrences;

            return template.Replace( "{parameters}", parameters );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithUnknownParameterName_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'1' unknown_parameter:'any_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"unknown_parameter\"." );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventAsName_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

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

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventAsGuid_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( $"eventid:'{StaffMeetingEventGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. An Event reference must be specified." );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithEventInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'no_event' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Cannot find an Event matching the reference \"no_event\"." );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'3m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-02-26|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-03-25|12:00 AM|All Campuses>>" );

            // Staff Meeting recurs every 2 weeks, so our date range of 3 months weeks should not include the meeting in month 4.
            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-04-08|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'5w'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 5 weeks should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'27d'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 27d should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'1020-1-1' daterange:'12m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 0>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:200" );

            var output = template.ResolveMergeFields( null );

            // Ensure that the maximum number of occurrences has been retrieved.
            Assert.That.Contains( output, "<EventCount = 200>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' daterange:'invalid'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. The specified Date Range is invalid." );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
        {
            // First, ensure that there are more than the default maximum number of events to return.
            // The default maximum is 100 events.
            var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:101" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 101>" );

            // Now ensure that the default limit is applied.
            var template2 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1'" );

            var output2 = template2.ResolveMergeFields( null );

            Assert.That.Contains( output2, "<EventCount = 100>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
        {
            // First, ensure that there are more than the test maximum number of events to return.
            var template1 = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:11" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 11>" );

            // Now ensure that the maxoccurences limit is applied.
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:10" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 10>" );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"maxoccurrences\"." );
        }

        [TestMethod]
        [Ignore("In the Fluid framework, this test incorrectly displays the same EventItemOccurrence on each pass through the loop.")]
        // This bug may be fixed, included in the next release: https://github.com/sebastienros/fluid/issues/317
        public void EventScheduledInstanceCommand_EventWithMultipleSchedules_ReturnsMultipleEventItemEntries()
        {
            var template = @"
{%- eventscheduledinstance eventid:'Rock Solid Finances Class' startdate:'2020-1-1' maxoccurrences:'25' daterange:'2m' -%}
    {%- for occurrence in EventItems -%}
        <b>Series {{forloop.index}}</b><br>

        {%- for item in occurrence -%}
            {%- if forloop.first -%}
                {{ item.Name }}
                <b>{{ item.DateTime | Date:'dddd' }} Series</b><br>
                <ol>
            {% endif %}

            <li>{{ item.DateTime | Date:'MMM d, yyyy' }} in {{ item.LocationDescription }}</li>

            {%- if forloop.last -%}
                </ol>
            {% endif %}
        {% endfor %}

    {% endfor %}
{% endeventscheduledinstance %}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine.EngineType, template );

                TestHelper.DebugWriteRenderResult( engine.EngineType, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<b>Series 1</b>" );
                Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                Assert.That.Contains( output, "<b>Series 2</b>" );
                Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }

        #region Test Data

        private const string TestDataForeignKey = "test_data";
        private const string EventFinancesClassGuid = "6EFC00B0-F5D3-4352-BC3B-F09852FB5788";
        private const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
        private const string ScheduleSun1200Guid = "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45";
        private const string FinancesClassOccurrenceSat1630Guid = "E7116C5A-9FEE-42D4-A0DB-7FEBFCCB6B8B";
        private const string FinancesClassOccurrenceSun1200Guid = "3F3EA420-E3F0-435A-9401-C2D058EF37DE";

        private static void InitializeTestData()
        {
            var rockContext = new RockContext();

            // Get existing schedules.
            var scheduleService = new ScheduleService( rockContext );

            var scheduleSat1630Id = scheduleService.GetId( ScheduleSat1630Guid.AsGuid() );
            var scheduleSat1800Id = scheduleService.GetId( ScheduleSun1200Guid.AsGuid() );

            // Get Event "Rock Solid Finances".
            var eventItemService = new EventItemService( rockContext );
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            var financeEvent = eventItemService.Get( EventFinancesClassGuid.AsGuid() );

            // Add an occurrence of this event for each Schedule.
            var financeEvent1 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSat1630Guid.AsGuid() );

            if ( financeEvent1 == null )
            {
                financeEvent1 = new EventItemOccurrence();
            }

            financeEvent1.Location = "Meeting Room 1";
            financeEvent1.ForeignKey = TestDataForeignKey;
            financeEvent1.ScheduleId = scheduleSat1630Id;
            financeEvent1.Guid = FinancesClassOccurrenceSat1630Guid.AsGuid();

            financeEvent.EventItemOccurrences.Add( financeEvent1 );

            var financeEvent2 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSun1200Guid.AsGuid() );

            if ( financeEvent2 == null )
            {
                financeEvent2 = new EventItemOccurrence();
            }

            financeEvent2.Location = "Meeting Room 2";
            financeEvent2.ForeignKey = TestDataForeignKey;
            financeEvent2.ScheduleId = scheduleSat1800Id;
            financeEvent2.Guid = FinancesClassOccurrenceSun1200Guid.AsGuid();

            financeEvent.EventItemOccurrences.Add( financeEvent2 );

            rockContext.SaveChanges();
        }

        #endregion
    }
}
