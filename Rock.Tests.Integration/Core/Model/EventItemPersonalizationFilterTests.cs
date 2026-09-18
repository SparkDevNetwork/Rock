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
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;

using static Rock.Tests.Integration.TestData.EventsDataManager;

namespace Rock.Tests.Integration.Core.Model
{
    /// <summary>
    /// Tests for the EventItemOccurrence personalization filter used by the Calendar Lava block
    /// and the CalendarEvents Lava command.
    /// </summary>
    /// <remarks>
    /// These tests require a database populated with standard Rock sample data.
    /// </remarks>
    [TestClass]
    [TestCategory( "Core.Events.Personalization" )]
    public class EventItemPersonalizationFilterTests : DatabaseTestsBase
    {
        #region Test Data Identifiers

        private const string TestDataForeignKey = "test_data_event_personalization";

        /// <summary>An event item with no personalization tags of either type.</summary>
        private const string EventUntaggedGuid = "1B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A10";

        /// <summary>An event item tagged with the matched segment only.</summary>
        private const string EventSegmentMatchedGuid = "2B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A11";

        /// <summary>An event item tagged with an unmatched segment only.</summary>
        private const string EventSegmentUnmatchedGuid = "3B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A12";

        /// <summary>An event item tagged with the matched request filter only.</summary>
        private const string EventRequestFilterMatchedGuid = "4B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A13";

        /// <summary>An event item tagged with an unmatched request filter only.</summary>
        private const string EventRequestFilterUnmatchedGuid = "5B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A14";

        /// <summary>An event item tagged with the matched segment but an unmatched request filter.</summary>
        private const string EventSegmentMatchedRequestFilterUnmatchedGuid = "6B0F8C4E-6E3A-4C5D-9A21-7F3B5D2C8A15";

        private const string OccurrenceUntaggedGuid = "1C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B20";
        private const string OccurrenceSegmentMatchedGuid = "2C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B21";
        private const string OccurrenceSegmentUnmatchedGuid = "3C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B22";
        private const string OccurrenceRequestFilterMatchedGuid = "4C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B23";
        private const string OccurrenceRequestFilterUnmatchedGuid = "5C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B24";
        private const string OccurrenceSegmentMatchedRequestFilterUnmatchedGuid = "6C1A9D5F-8E4B-4D6E-8B32-6E4C6D3E9B25";

        /// <summary>The schedule shared by every occurrence created for these tests.</summary>
        private const string TestScheduleGuid = "7D2B0E6A-9F5C-4E7F-9C43-5F5D7E4F0C30";

        private static readonly List<string> AllTestEventGuids = new List<string>
        {
            EventUntaggedGuid,
            EventSegmentMatchedGuid,
            EventSegmentUnmatchedGuid,
            EventRequestFilterMatchedGuid,
            EventRequestFilterUnmatchedGuid,
            EventSegmentMatchedRequestFilterUnmatchedGuid
        };

        #endregion

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            PersonalizationDataManager.Instance.AddDataForTestPersonalization();

            AddTestSchedule();

            AddEventItemWithOccurrence( EventUntaggedGuid, OccurrenceUntaggedGuid, "Personalization Test: Untagged" );
            AddEventItemWithOccurrence( EventSegmentMatchedGuid, OccurrenceSegmentMatchedGuid, "Personalization Test: Segment Matched" );
            AddEventItemWithOccurrence( EventSegmentUnmatchedGuid, OccurrenceSegmentUnmatchedGuid, "Personalization Test: Segment Unmatched" );
            AddEventItemWithOccurrence( EventRequestFilterMatchedGuid, OccurrenceRequestFilterMatchedGuid, "Personalization Test: Request Filter Matched" );
            AddEventItemWithOccurrence( EventRequestFilterUnmatchedGuid, OccurrenceRequestFilterUnmatchedGuid, "Personalization Test: Request Filter Unmatched" );
            AddEventItemWithOccurrence( EventSegmentMatchedRequestFilterUnmatchedGuid, OccurrenceSegmentMatchedRequestFilterUnmatchedGuid, "Personalization Test: Segment Matched, Request Filter Unmatched" );

            SetPersonalizationSegments( EventUntaggedGuid );
            SetPersonalizationRequestFilters( EventUntaggedGuid );

            SetPersonalizationSegments( EventSegmentMatchedGuid, PersonalizationDataManager.Constants.SegmentAllMenGuid );
            SetPersonalizationRequestFilters( EventSegmentMatchedGuid );

            SetPersonalizationSegments( EventSegmentUnmatchedGuid, PersonalizationDataManager.Constants.SegmentAllWomenGuid );
            SetPersonalizationRequestFilters( EventSegmentUnmatchedGuid );

            SetPersonalizationSegments( EventRequestFilterMatchedGuid );
            SetPersonalizationRequestFilters( EventRequestFilterMatchedGuid, PersonalizationDataManager.Constants.FilterQueryParameter1Guid );

            SetPersonalizationSegments( EventRequestFilterUnmatchedGuid );
            SetPersonalizationRequestFilters( EventRequestFilterUnmatchedGuid, PersonalizationDataManager.Constants.FilterQueryParameter2Guid );

            SetPersonalizationSegments( EventSegmentMatchedRequestFilterUnmatchedGuid, PersonalizationDataManager.Constants.SegmentAllMenGuid );
            SetPersonalizationRequestFilters( EventSegmentMatchedRequestFilterUnmatchedGuid, PersonalizationDataManager.Constants.FilterQueryParameter2Guid );
        }

        #region Tests

        [TestMethod]
        public void FilterByPersonalization_WithBothFiltersDisabled_ReturnsEveryEvent()
        {
            AssertVisibleEvents(
                filterByPersonalizationSegments: false,
                filterByRequestFilters: false,
                expectedEventGuids: AllTestEventGuids.ToArray() );
        }

        [TestMethod]
        public void FilterByPersonalization_WithSegmentFilterOnly_ExcludesOnlyUnmatchedSegments()
        {
            // Event items with no segments are unaffected, including those carrying an unmatched request filter.
            AssertVisibleEvents(
                filterByPersonalizationSegments: true,
                filterByRequestFilters: false,
                expectedEventGuids: new[]
                {
                    EventUntaggedGuid,
                    EventSegmentMatchedGuid,
                    EventRequestFilterMatchedGuid,
                    EventRequestFilterUnmatchedGuid,
                    EventSegmentMatchedRequestFilterUnmatchedGuid
                } );
        }

        [TestMethod]
        public void FilterByPersonalization_WithRequestFilterOnly_ExcludesOnlyUnmatchedRequestFilters()
        {
            // Event items with no request filters are unaffected, including those carrying an unmatched segment.
            AssertVisibleEvents(
                filterByPersonalizationSegments: false,
                filterByRequestFilters: true,
                expectedEventGuids: new[]
                {
                    EventUntaggedGuid,
                    EventSegmentMatchedGuid,
                    EventSegmentUnmatchedGuid,
                    EventRequestFilterMatchedGuid
                } );
        }

        [TestMethod]
        public void FilterByPersonalization_WithBothFiltersEnabled_RequiresEachEnabledTypeToMatchIndependently()
        {
            // The event tagged with a matched segment and an unmatched request filter must be hidden,
            // because an enabled type that is tagged and unmatched is disqualifying on its own.
            AssertVisibleEvents(
                filterByPersonalizationSegments: true,
                filterByRequestFilters: true,
                expectedEventGuids: new[]
                {
                    EventUntaggedGuid,
                    EventSegmentMatchedGuid,
                    EventRequestFilterMatchedGuid
                } );
        }

        [TestMethod]
        public void FilterByPersonalization_WithNoMatchedIdentifiers_ExcludesEveryTaggedEvent()
        {
            // This is the behavior of a site that has personalization disabled: the matched id lists are
            // always empty, so every tagged event is hidden while untagged events remain visible.
            var rockContext = new RockContext();

            var visibleEventGuids = GetTestOccurrenceQuery( rockContext )
                .FilterByPersonalization( rockContext, true, true, new List<int>(), new List<int>() )
                .Select( o => o.EventItem.Guid )
                .ToList();

            CollectionAssert.AreEquivalent( new List<Guid> { EventUntaggedGuid.AsGuid() }, visibleEventGuids );
        }

        [TestMethod]
        public void FilterByPersonalization_WithNullMatchedIdentifiers_ExcludesEveryTaggedEvent()
        {
            var rockContext = new RockContext();

            var visibleEventGuids = GetTestOccurrenceQuery( rockContext )
                .FilterByPersonalization( rockContext, true, true, null, null )
                .Select( o => o.EventItem.Guid )
                .ToList();

            CollectionAssert.AreEquivalent( new List<Guid> { EventUntaggedGuid.AsGuid() }, visibleEventGuids );
        }

        #endregion

        #region Test Helpers

        /// <summary>
        /// Applies the filter to the test event occurrences and asserts that exactly the expected event items remain.
        /// </summary>
        private void AssertVisibleEvents( bool filterByPersonalizationSegments, bool filterByRequestFilters, string[] expectedEventGuids )
        {
            var rockContext = new RockContext();

            var matchedSegmentIds = new List<int> { GetSegmentId( PersonalizationDataManager.Constants.SegmentAllMenGuid ) };
            var matchedRequestFilterIds = new List<int> { GetRequestFilterId( PersonalizationDataManager.Constants.FilterQueryParameter1Guid ) };

            var visibleEventGuids = GetTestOccurrenceQuery( rockContext )
                .FilterByPersonalization( rockContext, filterByPersonalizationSegments, filterByRequestFilters, matchedSegmentIds, matchedRequestFilterIds )
                .Select( o => o.EventItem.Guid )
                .ToList();

            var expectedGuids = expectedEventGuids.Select( g => g.AsGuid() ).ToList();

            CollectionAssert.AreEquivalent( expectedGuids, visibleEventGuids );
        }

        /// <summary>
        /// Gets a query for the occurrences of the event items created by this test class only,
        /// so that unrelated sample data cannot affect the result.
        /// </summary>
        private IQueryable<EventItemOccurrence> GetTestOccurrenceQuery( RockContext rockContext )
        {
            var testEventGuids = AllTestEventGuids.Select( g => g.AsGuid() ).ToList();

            return new EventItemOccurrenceService( rockContext )
                .Queryable()
                .Where( o => testEventGuids.Contains( o.EventItem.Guid ) );
        }

        /// <summary>
        /// Adds the schedule shared by every test occurrence, if it does not already exist.
        /// </summary>
        private static void AddTestSchedule()
        {
            var rockContext = new RockContext();

            var isScheduleMissing = new ScheduleService( rockContext ).Get( TestScheduleGuid.AsGuid() ) == null;
            if ( !isScheduleMissing )
            {
                return;
            }

            EventsDataManager.Instance.AddScheduleWithDailyRecurrence( new AddScheduleDailyRecurrenceActionArgs
            {
                Guid = TestScheduleGuid.AsGuid(),
                ForeignKey = TestDataForeignKey,
                StartDateTime = new DateTime( RockDateTime.Today.Ticks, DateTimeKind.Unspecified ),
                EventDuration = new TimeSpan( 1, 0, 0 ),
                OccurrenceCount = 10
            } );
        }

        /// <summary>
        /// Adds an event item and a single occurrence of it, if they do not already exist.
        /// </summary>
        private static void AddEventItemWithOccurrence( string eventItemGuid, string occurrenceGuid, string eventName )
        {
            var rockContext = new RockContext();

            var isEventItemMissing = new EventItemService( rockContext ).Get( eventItemGuid.AsGuid() ) == null;
            if ( isEventItemMissing )
            {
                EventsDataManager.Instance.AddEventItem( new CreateEventItemActionArgs
                {
                    Guid = eventItemGuid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    Properties = new EventItemInfo
                    {
                        EventName = eventName,
                        IsActive = true,
                        IsApproved = true,
                        CalendarIdentifiers = new List<string> { "Internal" }
                    },
                    ExistingItemStrategy = CreateExistingItemStrategySpecifier.Fail
                } );
            }

            var isOccurrenceMissing = new EventItemOccurrenceService( rockContext ).Get( occurrenceGuid.AsGuid() ) == null;
            if ( isOccurrenceMissing )
            {
                EventsDataManager.Instance.AddEventItemOccurrence( new CreateEventItemOccurrenceActionArgs
                {
                    Guid = occurrenceGuid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    Properties = new EventItemOccurrenceInfo
                    {
                        EventIdentifier = eventItemGuid,
                        ScheduleIdentifier = TestScheduleGuid,
                        MeetingLocationDescription = eventName
                    },
                    ExistingItemStrategy = CreateExistingItemStrategySpecifier.Update
                } );
            }
        }

        /// <summary>
        /// Replaces the personalization segments tagged on an event item. Passing no segments clears them.
        /// </summary>
        private static void SetPersonalizationSegments( string eventItemGuid, params string[] segmentGuids )
        {
            var rockContext = new RockContext();

            var eventItem = new EventItemService( rockContext ).Get( eventItemGuid.AsGuid() );
            var segmentService = new PersonalizationSegmentService( rockContext );

            var segmentIds = segmentGuids
                .Select( g => segmentService.GetNoTracking( g.AsGuid() ).Id )
                .ToList();

            segmentService.UpdatePersonalizedEntityForSegments( EntityTypeCache.Get<EventItem>().Id, eventItem.Id, segmentIds );
        }

        /// <summary>
        /// Replaces the request filters tagged on an event item. Passing no filters clears them.
        /// </summary>
        private static void SetPersonalizationRequestFilters( string eventItemGuid, params string[] requestFilterGuids )
        {
            var rockContext = new RockContext();

            var eventItem = new EventItemService( rockContext ).Get( eventItemGuid.AsGuid() );
            var requestFilterService = new RequestFilterService( rockContext );

            var requestFilterIds = requestFilterGuids
                .Select( g => requestFilterService.GetNoTracking( g.AsGuid() ).Id )
                .ToList();

            requestFilterService.UpdatePersonalizedEntityForRequestFilters( EntityTypeCache.Get<EventItem>().Id, eventItem.Id, requestFilterIds );
        }

        private static int GetSegmentId( string segmentGuid )
        {
            return new PersonalizationSegmentService( new RockContext() ).GetNoTracking( segmentGuid.AsGuid() ).Id;
        }

        private static int GetRequestFilterId( string requestFilterGuid )
        {
            return new RequestFilterService( new RockContext() ).GetNoTracking( requestFilterGuid.AsGuid() ).Id;
        }

        #endregion
    }
}
