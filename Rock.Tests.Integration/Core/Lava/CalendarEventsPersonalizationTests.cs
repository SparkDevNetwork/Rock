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
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Integration.TestFramework.Lava;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

using static Rock.Tests.Integration.TestData.EventsDataManager;

namespace Rock.Tests.Integration.Core.Lava
{
    /// <summary>
    /// Tests for personalization filtering in the Lava commands "CalendarEvents" and "EventScheduledInstance".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    [TestCategory( "Core.Events.Personalization" )]
    public class CalendarEventsPersonalizationTests : LavaIntegrationTestBase
    {
        #region Test Data Identifiers

        private const string TestDataForeignKey = "test_data_event_personalization";

        private const string TaggedEventGuid = "7D2B0E6A-9F5C-4E7F-9C43-5F5D7E4F0C31";
        private const string TaggedEventOccurrenceGuid = "8E3C1F7B-0A6D-4F80-8D54-4A6E8F5A1D42";
        private const string TaggedEventScheduleGuid = "9F4D2A8C-1B7E-4091-9E65-3B7F906B2E53";

        /// <summary>
        /// The name of the event tagged with a segment that the test person does not belong to.
        /// </summary>
        private const string TaggedEventName = "Personalization Filter Test Event";

        private static string LavaTemplateCalendarEvents = @"
{% calendarevents {parameters} %}
  {% for eventScheduledInstance in EventScheduledInstances %}
    <<{{ eventScheduledInstance.Name }}>>
  {% endfor %}
{% endcalendarevents %}
";

        private static string LavaTemplateEventScheduledInstance = @"
{% eventscheduledinstance {parameters} %}
  {% for eventScheduledInstance in EventScheduledInstances %}
    <<{{ eventScheduledInstance.Name }}>>
  {% endfor %}
{% endeventscheduledinstance %}
";

        #endregion

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            PersonalizationDataManager.Instance.AddDataForTestPersonalization();

            AddTaggedEvent();
        }

        #region CalendarEvents Command

        [TestMethod]
        public void CalendarEventsCommand_WithSegmentFilterDisabled_IncludesEventWithUnmatchedSegment()
        {
            // This is the default configuration, which must behave exactly as it did before
            // personalization filtering existed.
            var template = GetCalendarEventsTemplate( "calendarid:'Internal' maxoccurrences:100" );

            AssertTemplateOutputContainsTaggedEvent( template, "calendarevents", isTaggedEventExpected: true );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithSegmentFilterEnabled_ExcludesEventWithUnmatchedSegment()
        {
            // Bill Marble belongs to ALL_MEN and MARRIED, but the test event is tagged ALL_WOMEN.
            var template = GetCalendarEventsTemplate( "calendarid:'Internal' maxoccurrences:100 filterbysegments:'true'" );

            AssertTemplateOutputContainsTaggedEvent( template, "calendarevents", isTaggedEventExpected: false );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithPersonalizationParameters_DoesNotReportUnknownParameter()
        {
            var template = GetCalendarEventsTemplate( "calendarid:'Internal' filterbysegments:'true' filterbyrequestfilters:'true'" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template,
                    new LavaTestRenderOptions { EnabledCommands = "calendarevents" } );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.DoesNotContain( "Invalid configuration setting", output,
                    "The personalization parameters were reported as unknown configuration settings." );
            } );
        }

        [TestMethod]
        public void CalendarEventsCommand_WithMisspelledPersonalizationParameter_RendersErrorMessage()
        {
            var template = GetCalendarEventsTemplate( "calendarid:'Internal' filterbysegment:'true'" );

            TestHelper.AssertTemplateOutput( "Calendar Events not available. Invalid configuration setting \"filterbysegment\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains, EnabledCommands = "calendarevents" } );
        }

        #endregion

        #region EventScheduledInstance Command

        /*
            8/27/26 - CLAUDE

            The CalendarEvents and EventScheduledInstance commands share a data source class. Personalization
            filtering is applied on the CalendarEvents path only, because a template author using
            EventScheduledInstance has named a specific event and must always get it back. This test pins that
            separation so a future refactor cannot move the filter into the shared method unnoticed.

            Reason: Regression pin for the shared event occurrence data source.
        */
        [TestMethod]
        public void EventScheduledInstanceCommand_ForEventWithUnmatchedSegment_StillRendersEvent()
        {
            var template = LavaTemplateEventScheduledInstance.Replace( "{parameters}", $"eventid:'{TaggedEventGuid}' maxoccurrences:100" );

            AssertTemplateOutputContainsTaggedEvent( template, "eventscheduledinstance", isTaggedEventExpected: true );
        }

        [TestMethod]
        public void EventScheduledInstanceCommand_WithPersonalizationParameter_RendersErrorMessage()
        {
            // The personalization parameters belong to CalendarEvents only, so EventScheduledInstance
            // must continue to report them as unknown rather than silently accepting them.
            var template = LavaTemplateEventScheduledInstance.Replace( "{parameters}", $"eventid:'{TaggedEventGuid}' filterbysegments:'true'" );

            TestHelper.AssertTemplateOutput( "Event Occurrences not available. Invalid configuration setting \"filterbysegments\".",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains, EnabledCommands = "eventscheduledinstance" } );
        }

        #endregion

        #region Test Helpers

        private string GetCalendarEventsTemplate( string parameters )
        {
            return LavaTemplateCalendarEvents.Replace( "{parameters}", parameters );
        }

        /// <summary>
        /// Renders the template as Bill Marble and asserts whether the tagged test event appears in the output.
        /// </summary>
        private void AssertTemplateOutputContainsTaggedEvent( string template, string enabledCommands, bool isTaggedEventExpected )
        {
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.BillMarble.AsGuid() );

            Assert.IsNotNull( person, "Expected test data not found." );

            var mergeFields = new Dictionary<string, object>
            {
                ["CurrentPerson"] = person,
                ["CurrentVisitor"] = person.PrimaryAlias
            };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template,
                    new LavaTestRenderOptions { EnabledCommands = enabledCommands, MergeFields = mergeFields } );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                var expectedEventOutput = $"<<{TaggedEventName}>>";

                if ( isTaggedEventExpected )
                {
                    Assert.Contains( expectedEventOutput, output, $"Event \"{TaggedEventName}\" was expected but is not rendered." );
                }
                else
                {
                    Assert.DoesNotContain( expectedEventOutput, output, $"Event \"{TaggedEventName}\" is rendered but was expected to be filtered out." );
                }
            } );
        }

        /// <summary>
        /// Adds an event on the Internal calendar with upcoming occurrences, tagged with a segment
        /// that the test person does not belong to.
        /// </summary>
        private static void AddTaggedEvent()
        {
            var rockContext = new RockContext();

            var isScheduleMissing = new ScheduleService( rockContext ).Get( TaggedEventScheduleGuid.AsGuid() ) == null;
            if ( isScheduleMissing )
            {
                EventsDataManager.Instance.AddScheduleWithDailyRecurrence( new AddScheduleDailyRecurrenceActionArgs
                {
                    Guid = TaggedEventScheduleGuid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    StartDateTime = new DateTime( RockDateTime.Today.Ticks, DateTimeKind.Unspecified ),
                    EventDuration = new TimeSpan( 1, 0, 0 ),
                    OccurrenceCount = 10
                } );
            }

            var isEventItemMissing = new EventItemService( rockContext ).Get( TaggedEventGuid.AsGuid() ) == null;
            if ( isEventItemMissing )
            {
                EventsDataManager.Instance.AddEventItem( new CreateEventItemActionArgs
                {
                    Guid = TaggedEventGuid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    Properties = new EventItemInfo
                    {
                        EventName = TaggedEventName,
                        IsActive = true,
                        IsApproved = true,
                        CalendarIdentifiers = new List<string> { "Internal" }
                    },
                    ExistingItemStrategy = CreateExistingItemStrategySpecifier.Fail
                } );
            }

            var isOccurrenceMissing = new EventItemOccurrenceService( rockContext ).Get( TaggedEventOccurrenceGuid.AsGuid() ) == null;
            if ( isOccurrenceMissing )
            {
                EventsDataManager.Instance.AddEventItemOccurrence( new CreateEventItemOccurrenceActionArgs
                {
                    Guid = TaggedEventOccurrenceGuid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    Properties = new EventItemOccurrenceInfo
                    {
                        EventIdentifier = TaggedEventGuid,
                        ScheduleIdentifier = TaggedEventScheduleGuid,
                        MeetingLocationDescription = TaggedEventName
                    },
                    ExistingItemStrategy = CreateExistingItemStrategySpecifier.Update
                } );
            }

            // Tag the event with a segment that Bill Marble does not belong to.
            var eventItem = new EventItemService( rockContext ).Get( TaggedEventGuid.AsGuid() );
            var segmentService = new PersonalizationSegmentService( rockContext );
            var segment = segmentService.GetNoTracking( PersonalizationDataManager.Constants.SegmentAllWomenGuid.AsGuid() );

            segmentService.UpdatePersonalizedEntityForSegments( EntityTypeCache.Get<EventItem>().Id, eventItem.Id, new List<int> { segment.Id } );
        }

        #endregion
    }
}
