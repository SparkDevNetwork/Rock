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
    /// Tests for Lava Command "CalendarEvents".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class CalendarEventsCommandTests
    {
        private static string InternalCalendarGuidString = "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798";
        private static string YouthAudienceGuidString = "59CD7FD8-6A62-4C3B-8966-1520E74EED58";

        private static string LavaTemplateCalendarEvents = @";
{% calendarevents {parameters} %}
  {% assign eventItemOccurrenceCount = EventItemOccurrences | Size %}
  <<EventCount = {{ EventItemOccurrences | Size }}>>
  {% for eventItemOccurrence in EventItemOccurrences %}
    <<{{ eventItemOccurrence.Name }}|{{ eventItemOccurrence.Date | Date: 'yyyy-MM-dd' }}|{{ eventItemOccurrence.Time }}|{{ eventItemOccurrence.Location }}>>
    <<Calendars: {{ eventItemOccurrence.CalendarNames | Join:', ' }}>>
    <<Audiences: {{ eventItemOccurrence.AudienceNames | Join:', ' }}>>
  {% endfor %}
{% endcalendarevents %}
";

        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            // Initialize the Lava Engine.
            Liquid.UseRubyDateFormat = false;
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

            Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );

            // Register the Lava commands required for testing.
            var lavaCommand = new CalendarEvents();

            lavaCommand.OnStartup();
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

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. Invalid configuration setting \"unknown_parameter\"." );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsName_RetrievesEventsInCorrectCalendar()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Calendars: Internal>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsId_RetrievesEventsInCorrectCalendar()
        {
            var template = GetTestTemplate( "calendarid:'1' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Calendars: Internal, Public>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarAsGuid_RetrievesEventsInCorrectCalendar()
        {
            var template = GetTestTemplate( $"calendarid:'{InternalCalendarGuidString}' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Calendars: Internal>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarNotSpecified_RendersErrorMessage()
        {
            var template = GetTestTemplate( "startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. A calendar reference must be specified." );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithCalendarInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'no_calendar' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. Cannot find a calendar matching the reference \"no_calendar\"." );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceAsName_RetrievesEventsWithMatchingAudience()
        {
            var template = GetTestTemplate( "calendarid:'Public' audienceids:'Youth' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Audiences: All Church, Adults, Youth>" );
        }
        public void CalendarEventsCommand_WithAudienceAsMultipleValues_RetrievesEventsWithAnyMatchingAudience()
        {
            var template = GetTestTemplate( "calendarid:'Public' audienceids:'Men,Women' startdate:'2020-1-1' daterange:'12m' maxoccurrences:2" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Audiences: Internal>" );
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

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Audiences: All Church," );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceAsGuid_RetrievesEventsWithMatchingAudience()
        {
            var template = GetTestTemplate( $"calendarid:'Public' audienceids:'{YouthAudienceGuidString}' startdate:'2018-1-1'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<Audiences: All Church, Adults, Youth" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithAudienceInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' audienceids:'no_audience'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. Cannot apply an audience filter for the reference \"no_audience\"." );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInMonths_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'3m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-02-26|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-03-25|12:00 AM|All Campuses>>" );

            // Staff Meeting recurs every 2 weeks, so our date range of 3 months weeks should not include the meeting in month 4.
            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-04-08|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInWeeks_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'5w'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 5 weeks should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInDays_ReturnsExpectedEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' daterange:'27d'" );

            var output = template.ResolveMergeFields( null );

            // Staff Meeting recurs every 2 weeks, so our date range of 27d should only include 2 occurrences.
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-01|12:00 AM|All Campuses>>" );
            Assert.That.Contains( output, "<<Staff Meeting|2020-01-15|12:00 AM|All Campuses>>" );

            Assert.That.DoesNotContain( output, "<<Staff Meeting|2020-01-29|12:00 AM|All Campuses>>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeContainingNoEvents_ReturnsNoEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'1020-1-1' daterange:'12m'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 0>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeUnspecified_ReturnsAllEvents()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:200" );

            var output = template.ResolveMergeFields( null );

            // Ensure that the maximum number of occurrences has been retrieved.
            Assert.That.Contains( output, "<EventCount = 200>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithDateRangeInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' daterange:'invalid'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. The specified Date Range is invalid." );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesUnspecified_ReturnsDefaultNumberOfOccurrences()
        {
            // First, ensure that there are more than the default maximum number of events to return.
            // The default maximum is 100 events.
            var template1 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:101" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 101>" );

            // Now ensure that the default limit is applied.
            var template2 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1'" );

            var output2 = template2.ResolveMergeFields( null );

            Assert.That.Contains( output2, "<EventCount = 100>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesLessThanAvailableEvents_ReturnsMaxOccurrences()
        {
            // First, ensure that there are more than the test maximum number of events to return.
            var template1 = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:11" );

            var output1 = template1.ResolveMergeFields( null );

            Assert.That.Contains( output1, "<EventCount = 11>" );

            // Now ensure that the maxoccurences limit is applied.
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:10" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "<EventCount = 10>" );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMaxOccurrencesInvalidValue_RendersErrorMessage()
        {
            var template = GetTestTemplate( "calendarid:'Internal' startdate:'2020-1-1' maxoccurrences:'invalid_value'" );

            var output = template.ResolveMergeFields( null );

            Assert.That.Contains( output, "Calendar Events not available. Invalid configuration setting \"maxoccurrences\"." );
        }
    }
}
