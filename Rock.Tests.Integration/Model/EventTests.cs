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
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Model
{
    /// <summary>
    /// Tests related to Calendar Events.
    /// </summary>
    /// <remarks>
    /// These tests require a database populated with standard Rock sample data.
    /// </remarks>
    [TestClass]
    public class EventTests
    {
        #region EventItemService Tests

        /// <summary>
        /// Retrieving a list of active events with the default settings should only return events having occurrences after the current date.
        /// </summary>
        [TestMethod]
        public void EventItemService_GetActiveEventsDefault_ReturnsOnlyEventsHavingOccurrencesAfterCurrentDate()
        {
            var rockContext = new RockContext();
            var eventItemService = new EventItemService( rockContext );
            var events = eventItemService.GetActiveItems();

            // The Event "Warrior Youth Event" has a single occurrence scheduled in the past.
            // It should not be returned in the list of active items.
            var warriorEvent = events.FirstOrDefault( x => x.Name == "Warrior Youth Event" );

            Assert.That.IsNull( warriorEvent, "Unexpected event found in result set." );

            // The Event "Staff Meeting" is endlessly recurring.
            // It should be returned in the list of active items.
            var staffEvent = events.FirstOrDefault( x => x.Name == "Staff Meeting" );

            Assert.That.IsNotNull( staffEvent, "Expected event not found in result set." );
        }

        /// <summary>
        /// Retrieving a list of active events should only return an event scheduled for a single date if it occurs after the specified date.
        /// </summary>
        [TestMethod]
        public void EventItemService_GetActiveEventsAtSpecificDate_ReturnsEventScheduledForSingleDateOnOrAfterSpecifiedDate()
        {
            var rockContext = new RockContext();

            // Get an instance of Event "Warrior Youth Event", which has a single occurrence scheduled for 02-May-2018.
            var eventOccurrenceDate = new DateTime( 2018, 5, 2 );
            var eventItemService = new EventItemService( rockContext );

            var warriorEvent = eventItemService.Queryable().FirstOrDefault( x => x.Name == "Warrior Youth Event" );

            Assert.That.IsNotNull( warriorEvent, "Target event not found." );

            ForceUpdateScheduleEffectiveDates( rockContext, warriorEvent );

            // The "Warrior Youth Event" has a single occurrence on 02-May-2018.
            // Verify that the filter returns the event on the date of the single occurrence.
            var validEvents = eventItemService.Queryable()
                .Where( x => x.Name == "Warrior Youth Event" )
                .HasOccurrencesOnOrAfterDate( eventOccurrenceDate )
                .ToList();

            Assert.That.IsTrue( validEvents.Count() == 1, "Expected Event not found." );

            // ... but not after the date of the single occurrence.
            var invalidEvents = eventItemService.Queryable()
                .Where( x => x.Name == "Warrior Youth Event" )
                .HasOccurrencesOnOrAfterDate( eventOccurrenceDate.AddDays( 1 ) )
                .ToList();

            Assert.That.IsTrue( invalidEvents.Count() == 0, "Unexpected Event found." );
        }

        private void ForceUpdateScheduleEffectiveDates( RockContext rockContext, EventItem eventItem )
        {
            // Ensure that the Event Schedule has been updated to record the EffectiveEndDate.
            // This is only necessary if the RockCleanup job has not been run in the current database
            // since upgrading from v1.12.4.
            foreach ( var occurrence in eventItem.EventItemOccurrences )
            {
                var isUpdated = occurrence.Schedule.EnsureEffectiveStartEndDates();
                if ( isUpdated )
                {
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Retrieving a list of active events by calendar should only return events from the specified calendar.
        /// </summary>
        [TestMethod]
        public void EventItemService_GetActiveEventsByCalendar_ReturnsOnlyEventsInSpecifiedCalendar()
        {
            var rockContext = new RockContext();
            var calendarService = new EventCalendarService( rockContext );

            var internalCalendar = calendarService.Queryable()
                .FirstOrDefault( x => x.Name == "Internal" );

            var eventItemService = new EventItemService( rockContext );
            var internalEvents = eventItemService.GetActiveItemsByCalendarId( internalCalendar.Id )
                .ToList();

            // The Event "Staff Meeting" exists in the Internal calendar.
            // It should be returned in the list of active items.
            var staffEvent = internalEvents.FirstOrDefault( x => x.Name == "Staff Meeting" );

            Assert.That.IsNotNull( staffEvent, "Expected event not found in result set." );

            // The Event "Warrior Youth Event" only exists in the External calendar.
            // It should not be returned in the list of active items.
            var warriorEvent = internalEvents.FirstOrDefault( x => x.Name == "Warrior Youth Event" );

            Assert.That.IsNull( warriorEvent, "Unexpected event found in result set." );
        }

        #endregion

        #region EventItemOccurrence.NextDateTime Tests

        private const string TestDataForeignKey = "test_data";
        private const string EventFinancesClassGuid = "6EFC00B0-F5D3-4352-BC3B-F09852FB5788";
        private const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
        private const string ScheduleSun1200Guid = "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45";
        private const string FinancesClassOccurrenceTestGuid = "73D5B5F2-CDD7-4DC6-B62F-D251CB8832CA";
        private static string MainCampusGuidString = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";

        /// <summary>
        /// Adding an EventItemOccurrence correctly sets the NextDateTime property.
        /// </summary>
        [TestMethod]
        public void EventItemOccurrence_NewWithFutureActiveSchedule_HasCorrectNextDate()
        {
            var rockContext = new RockContext();
            var financeEvent1 = EventItemOccurrenceAddOrUpdateInstance( rockContext, ScheduleSat1630Guid.AsGuid(), deleteExistingInstance: true );

            rockContext.SaveChanges();

            // Verify that the NextDateTime is correctly set.
            var scheduleService = new ScheduleService( rockContext );
            var scheduleSat1630 = scheduleService.Get( ScheduleSat1630Guid.AsGuid() );
            var nextDateTime = scheduleSat1630.GetNextStartDateTime( RockDateTime.Now );

            Assert.AreEqual( financeEvent1.NextStartDateTime, nextDateTime );
        }

        /// <summary>
        /// Modifying the schedule for an EventItemOccurrence correctly adjusts the NextDateTime property.
        /// </summary>
        [TestMethod]
        public void EventItemOccurrence_ModifiedWithActiveSchedule_HasUpdatedNextDate()
        {
            var rockContext = new RockContext();
            var financeEvent1 = EventItemOccurrenceAddOrUpdateInstance( rockContext, ScheduleSat1630Guid.AsGuid(), deleteExistingInstance: true );

            rockContext.SaveChanges();

            // Get existing schedules.
            var scheduleService = new ScheduleService( rockContext );
            var scheduleSat1630 = scheduleService.Get( ScheduleSat1630Guid.AsGuid() );
            var scheduleSat1800 = scheduleService.Get( ScheduleSun1200Guid.AsGuid() );

            // Verify that the NextDateTime is correctly set.
            var nextDateTime1 = scheduleSat1630.GetNextStartDateTime( RockDateTime.Now );
            Assert.AreEqual( financeEvent1.NextStartDateTime, nextDateTime1 );

            // Now modify the event occurrence to use Schedule 2.
            financeEvent1.ScheduleId = scheduleSat1800.Id;

            rockContext.SaveChanges();

            // Verify that the NextDateTime is correctly set.
            var nextDateTime2 = scheduleSat1800.GetNextStartDateTime( RockDateTime.Now );
            Assert.AreEqual( financeEvent1.NextStartDateTime, nextDateTime2 );
        }

        /// <summary>
        /// Setting an inactive schedule for an EventItemOccurrence correctly nullifies the NextDateTime property.
        /// </summary>
        [TestMethod]
        public void EventItemOccurrence_ModifiedWithInactiveSchedule_HasNullNextDate()
        {
            var rockContext = new RockContext();

            // Get existing schedules.
            var scheduleService = new ScheduleService( rockContext );
            var scheduleSat1630 = scheduleService.Get( ScheduleSat1630Guid.AsGuid() );

            // Set the schedule to inactive.
            scheduleSat1630.IsActive = false;
            rockContext.SaveChanges();

            // Add an event occurrence with the inactive schedule
            var financeEvent1 = EventItemOccurrenceAddOrUpdateInstance( rockContext, ScheduleSat1630Guid.AsGuid(), deleteExistingInstance: true );
            rockContext.SaveChanges();

            // Verify that the NextDateTime is correctly set.
            Assert.IsNull( financeEvent1.NextStartDateTime );

            // Set the schedule to active.
            scheduleSat1630.IsActive = true;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Saving changes to an EventItemOccurrence attached to an inactive Event correctly nullifies the NextDateTime property.
        /// </summary>
        [TestMethod]
        public void EventItemOccurrence_UpdatedWithInactiveEvent_HasNullNextDate()
        {
            var rockContext = new RockContext();

            // Get Event "Rock Solid Finances".
            var eventItemService = new EventItemService( rockContext );
            var financeEvent = eventItemService.Get( EventFinancesClassGuid.AsGuid() );

            // Get existing schedules.
            var scheduleService = new ScheduleService( rockContext );
            var scheduleSat1630 = scheduleService.Get( ScheduleSat1630Guid.AsGuid() );

            // Set the event to inactive.
            financeEvent.IsActive = false;
            rockContext.SaveChanges();

            // Add an event occurrence for the inactive event.
            var financeEvent1 = EventItemOccurrenceAddOrUpdateInstance( rockContext, ScheduleSat1630Guid.AsGuid(), deleteExistingInstance: true );
            rockContext.SaveChanges();

            // Verify that the NextDateTime is correctly set.
            Assert.IsNull( financeEvent1.NextStartDateTime );

            // Set the event to active.
            financeEvent.IsActive = true;
            rockContext.SaveChanges();
        }

        private EventItemOccurrence EventItemOccurrenceAddOrUpdateInstance( RockContext rockContext, Guid scheduleGuid, bool deleteExistingInstance )
        {
            // Get existing schedules.
            var scheduleService = new ScheduleService( rockContext );
            var schedule = scheduleService.Get( scheduleGuid );

            // Get Event "Rock Solid Finances".
            var eventItemService = new EventItemService( rockContext );
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            var financeEvent = eventItemService.Get( EventFinancesClassGuid.AsGuid() );

            // Add a new occurrence of this event.
            var financeEvent1 = eventItemOccurrenceService.Get( FinancesClassOccurrenceTestGuid.AsGuid() );
            if ( financeEvent1 != null && deleteExistingInstance )
            {
                eventItemOccurrenceService.Delete( financeEvent1 );
                rockContext.SaveChanges();
            }
            financeEvent1 = new EventItemOccurrence();

            var mainCampusId = CampusCache.GetId( MainCampusGuidString.AsGuid() );

            financeEvent1.Location = "Meeting Room 1";
            financeEvent1.ForeignKey = TestDataForeignKey;
            financeEvent1.ScheduleId = schedule.Id;
            financeEvent1.Guid = FinancesClassOccurrenceTestGuid.AsGuid();
            financeEvent1.CampusId = mainCampusId;

            financeEvent.EventItemOccurrences.Add( financeEvent1 );

            return financeEvent1;
        }

        #endregion
    }
}
