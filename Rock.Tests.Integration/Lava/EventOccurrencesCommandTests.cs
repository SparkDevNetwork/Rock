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
    /// Tests for Lava Command "EventOccurrences".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class EventOccurrencesCommandTests
    {
        private static string StaffMeetingEventGuidString = "93104654-DAFA-489B-A175-5F2AB3A846F1";

        private static string LavaTemplateEventOccurrences = @";
{% eventoccurrences {parameters} %}
  {% assign eventItemOccurrenceCount = EventItemOccurrences | Size %}
  <<EventCount = {{ EventItemOccurrences | Size }}>>
  {% for eventItemOccurrence in EventItemOccurrences %}
    <<{{ eventItemOccurrence.Name }}|{{ eventItemOccurrence.Date | Date: 'yyyy-MM-dd' }}|{{ eventItemOccurrence.Time }}|{{ eventItemOccurrence.Location }}>>
    <<Calendars: {{ eventItemOccurrence.CalendarNames | Join:', ' }}>>
    <<Audiences: {{ eventItemOccurrence.AudienceNames | Join:', ' }}>>
  {% endfor %}
{% endeventoccurrences %}
";

        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            // Initialize the Lava Engine.
            Liquid.UseRubyDateFormat = false;
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

            Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );

            // Register the Lava commands required for testing.
            var lavaCommand = new EventOccurrences();

            lavaCommand.OnStartup();
        }

        private string GetTestTemplate( string parameters )
        {
            var template = LavaTemplateEventOccurrences;

            return template.Replace( "{parameters}", parameters );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithUnknownParameterName_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'1' unknown_parameter:'any_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"unknown_parameter\"." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventAsName_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventAsId_RetrievesOccurrencesInCorrectEvent()
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
        public void EventOccurrencesCommand_WithEventAsGuid_RetrievesOccurrencesInCorrectEvent()
        {
            var template = GetTestTemplate( $"eventid:'{StaffMeetingEventGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. An Event reference must be specified." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithEventInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'no_event' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Cannot find an Event matching the reference \"no_event\"." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
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
        public void EventOccurrencesCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'5w'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 5 weeks should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' daterange:'27d'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 27d should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'1020-1-1' daterange:'12m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 0>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:200" );

            var output = template.ResolveMergeFields( null );

            // Ensure that the maximum number of occurrences has been retrieved.
            Assert.That.Contains( output, "<EventCount = 200>" );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' daterange:'invalid'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. The specified Date Range is invalid." );
        }

        [TestMethod]
        public void EventOccurrencesCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
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
        public void EventOccurrencesCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
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
        public void EventOccurrencesCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "eventid:'Staff Meeting' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Event Occurrences not available. Invalid configuration setting \"maxoccurrences\"." );
        }
    }
}
