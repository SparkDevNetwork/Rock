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
            // Verify that the filter returns the event on the date of the single occurrence...
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
    }
}
