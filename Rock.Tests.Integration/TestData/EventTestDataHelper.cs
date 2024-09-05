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
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Web.Cache;

using System.Collections.Generic;

using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Events
{
    /// <summary>
    /// Provides actions to manage Event data.
    /// </summary>
    public class EventsDataManager
    {
        private static Lazy<EventsDataManager> _dataManager = new Lazy<EventsDataManager>();
        public static EventsDataManager Instance => _dataManager.Value;

        private const string TestDataForeignKey = "test_data";
        private const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
        private const string ScheduleSun1200Guid = "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45";
        private const string FinancesClassOccurrenceSat1630Guid = "E7116C5A-9FEE-42D4-A0DB-7FEBFCCB6B8B";
        private const string FinancesClassOccurrenceSun1200Guid = "3F3EA420-E3F0-435A-9401-C2D058EF37DE";

        #region Event Item

        public class EventItemInfo
        {
            public string EventName;
            public bool? IsActive;
            public bool? IsApproved;

            public List<string> CalendarIdentifiers = new List<string>();
        }

        public class CreateEventItemActionArgs : CreateEntityActionArgsBase<EventItemInfo>
        {
        }

        public class UpdateEventItemActionArgs : UpdateEntityActionArgsBase<EventItemInfo>
        {
        }

        public bool DeleteEventItem( string eventItemIdentifier, RockContext context )
        {
            var eventItemService = new EventItemService( context );
            var eventItem = eventItemService.Get( eventItemIdentifier );

            if ( eventItem == null )
            {
                return false;
            }

            return eventItemService.Delete( eventItem );
        }

        /// <summary>
        /// Add a new EventItem.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddEventItem( CreateEventItemActionArgs args )
        {
            EventItem newEventItem = null;

            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var eventItemService = new EventItemService( rockContext );
                if ( args.Guid != null )
                {
                    newEventItem = eventItemService.Get( args.Guid.Value );
                    if ( newEventItem != null )
                    {
                        if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteEventItem( args.Guid.Value.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }

                            newEventItem = null;
                        }
                    }
                }

                if ( newEventItem == null )
                {
                    newEventItem = new EventItem();
                    newEventItem.Guid = args.Guid.Value;
                    newEventItem.IsActive = true;
                    newEventItem.IsApproved = true;

                    eventItemService.Add( newEventItem );
                }

                UpdateEventItemPropertiesFromInfo( newEventItem, args.Properties, rockContext );

                rockContext.SaveChanges();
            } );

            return newEventItem.Id;
        }

        /// <summary>
        /// Update an existing EventItem.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdateEventItem( UpdateEventItemActionArgs args )
        {
            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var eventItemService = new EventItemService( rockContext );
                var eventItem = eventItemService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdateEventItemPropertiesFromInfo( eventItem, args.Properties, rockContext );

                rockContext.SaveChanges();
            } );
        }

        private void UpdateEventItemPropertiesFromInfo( EventItem newEvent, EventItemInfo actionInfo, RockContext rockContext )
        {
            if ( actionInfo.EventName != null )
            {
                newEvent.Name = actionInfo.EventName;
            }
            if ( actionInfo.IsActive != null )
            {
                newEvent.IsActive = actionInfo.IsActive.Value;
            }
            if ( actionInfo.IsApproved != null )
            {
                newEvent.IsApproved = actionInfo.IsApproved.Value;
            }

            // Assign the Calendars to which the Event can be added.
            // If no calendars are specified, default to the Internal calendar.
            if ( actionInfo.CalendarIdentifiers != null )
            {
                var calendarIdentifiers = actionInfo.CalendarIdentifiers;
                if ( !calendarIdentifiers.Any() )
                {
                    calendarIdentifiers.Add( "Internal" );
                }

                var eventCalendarService = new EventCalendarService( rockContext );
                foreach ( var calendarIdentifier in calendarIdentifiers )
                {
                    var eventCalendar = eventCalendarService.GetByIdentifierOrThrow( calendarIdentifier );

                    var calendar = new EventCalendarItem();
                    calendar.EventCalendarId = eventCalendar.Id;
                    calendar.EventItem = newEvent;

                    newEvent.EventCalendarItems.Add( calendar );
                }
            }
        }


        #endregion

        #region Event Item Occurrence

        public class EventItemOccurrenceInfo
        {
            public string ForeignKey { get; set; }
            public string EventIdentifier { get; set; }
            public string ScheduleIdentifier { get; set; }
            public string CampusIdentifier { get; set; }

            public string MeetingLocationDescription { get; set; }
        }

        public class CreateEventItemOccurrenceActionArgs : CreateEntityActionArgsBase<EventItemOccurrenceInfo>
        {
            //public string EventIdentifier;
            //public string ScheduleIdentifier;
            //public string CampusIdentifier;

            //public string MeetingLocationDescription;
        }

        public class UpdateEventItemOccurrenceActionArgs : UpdateEntityActionArgsBase<EventItemOccurrenceInfo>
        {
            //public string EventIdentifier;
            //public string ScheduleIdentifier;
            //public string CampusIdentifier;

            //public string MeetingLocationDescription;
        }

        public bool DeleteEventItemOccurrence( string occurrenceIdentifier, RockContext context )
        {
            var occurrenceService = new EventItemOccurrenceService( context );
            var occurrence = occurrenceService.Get( occurrenceIdentifier );

            if ( occurrence == null )
            {
                return false;
            }

            return occurrenceService.Delete( occurrence );
        }

        /// <summary>
        /// Adds a new Event Item Occurrence - an actual set of one or more instances of an EventItem.
        /// The EventItem defines the template for the actual scheduled events represented by the EventItemOccurrence.
        /// </summary>
        public EventItemOccurrence AddEventItemOccurrence( CreateEventItemOccurrenceActionArgs args )
        {
            var rockContext = new RockContext();
            var occurrenceService = new EventItemOccurrenceService( rockContext );

            EventItemOccurrence occurrence = null;
            if ( args.Guid != null )
            {
                occurrence = occurrenceService.Get( args.Guid.Value );
                if ( occurrence != null )
                {
                    if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                    {
                        throw new Exception( "Item exists." );
                    }
                    else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Ignore )
                    {
                        return occurrence;
                    }
                    else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                    {
                        var isDeleted = DeleteEventItemOccurrence( args.Guid.ToString(), rockContext );
                        if ( !isDeleted )
                        {
                            throw new Exception( "Could not replace existing item." );
                        }
                        occurrence = null;
                    }
                }
            }

            if ( occurrence == null )
            {
                occurrence = new EventItemOccurrence();
                occurrenceService.Add( occurrence );
            }

            occurrence.Guid = args.Guid ?? Guid.NewGuid();
            occurrence.ForeignKey = args.ForeignKey;

            // Get Event
            var eventItemService = new EventItemService( rockContext );
            var eventItem = eventItemService.Queryable().GetByIdentifierOrThrow( args.Properties.EventIdentifier );

            occurrence.EventItemId = eventItem.Id;

            // Get Schedule
            var scheduleService = new ScheduleService( rockContext );
            var schedule = scheduleService.Queryable().GetByIdentifierOrThrow( args.Properties.ScheduleIdentifier );

            occurrence.ScheduleId = schedule.Id;

            // Get Campus
            if ( !string.IsNullOrWhiteSpace( args.Properties.CampusIdentifier ) )
            {
                var campusService = new CampusService( rockContext );
                var campus = campusService.Queryable().GetByIdentifierOrThrow( args.Properties.CampusIdentifier );

                occurrence.CampusId = campus.Id;
            }

            // Set properties.
            occurrence.Location = args.Properties.MeetingLocationDescription;

            rockContext.SaveChanges();

            return occurrence;
        }

        /// <summary>
        /// Adds a new Event Item Occurrence - an actual set of one or more instances of an EventItem.
        /// The EventItem defines the template for the actual scheduled events represented by the EventItemOccurrence.
        /// </summary>
        public EventItemOccurrence UpdateEventItemOccurrence( UpdateEventItemOccurrenceActionArgs args )
        {
            var rockContext = new RockContext();
            var occurrenceService = new EventItemOccurrenceService( rockContext );

            var occurrence = occurrenceService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

            UpdateEventItemOccurrencePropertiesFromInfo( occurrence, args.Properties, rockContext );

            rockContext.SaveChanges();

            return occurrence;
        }

        private void UpdateEventItemOccurrencePropertiesFromInfo( EventItemOccurrence occurrence, EventItemOccurrenceInfo actionInfo, RockContext rockContext )
        {
            // Get Event
            var eventItemService = new EventItemService( rockContext );

            // Update ForeignKey.
            if ( actionInfo.ForeignKey != null )
            {
                occurrence.ForeignKey = actionInfo.ForeignKey;
            }

            // Update Parent Event Item.
            if ( actionInfo.EventIdentifier != null )
            {
                var eventItem = eventItemService.Queryable().GetByIdentifierOrThrow( actionInfo.EventIdentifier );

                occurrence.EventItemId = eventItem.Id;
            }

            // Update Schedule
            if ( actionInfo.ScheduleIdentifier != null )
            {
                var scheduleService = new ScheduleService( rockContext );
                var schedule = scheduleService.Queryable().GetByIdentifierOrThrow( actionInfo.ScheduleIdentifier );

                occurrence.ScheduleId = schedule.Id;
            }

            // Get Campus
            if ( actionInfo.CampusIdentifier != null )
            {
                var campusService = new CampusService( rockContext );
                var campus = campusService.Queryable().GetByIdentifierOrThrow( actionInfo.CampusIdentifier );

                occurrence.CampusId = campus.Id;
            }

            // Set properties.
            if ( actionInfo.MeetingLocationDescription != null )
            {
                occurrence.Location = actionInfo.MeetingLocationDescription;
            }
        }

        #endregion

        #region Schedules

        public class AddScheduleDailyRecurrenceActionArgs : CreateEntityActionArgsBase
        {
            public DateTime? StartDateTime;
            public DateTime? EndDateTime;
            public TimeSpan? EventDuration;
            public int? OccurrenceCount;
        }

        public Schedule AddScheduleWithDailyRecurrence( AddScheduleDailyRecurrenceActionArgs args )
        {
            var rockContext = new RockContext();

            var startDateTime = args.StartDateTime ?? new DateTime( RockDateTime.Today.Ticks, DateTimeKind.Unspecified );
            var calendarEvent = GetICalCalendarEvent( startDateTime, args.EventDuration );

            var recurrence = GetICalDailyRecurrencePattern( args.EndDateTime, args.OccurrenceCount );
            var calendar = GetICalCalendar( calendarEvent, recurrence );
            var schedule = CreateSchedule( calendar );

            if ( args.Guid != null )
            {
                schedule.Guid = args.Guid.Value;
            }

            rockContext = rockContext ?? new RockContext();

            var scheduleService = new ScheduleService( rockContext );
            scheduleService.Add( schedule );

            rockContext.SaveChanges();

            return schedule;
        }

        public class AddScheduleSpecificDatesActionArgs : CreateEntityActionArgsBase
        {
            public List<DateTime> dates;
            public TimeSpan? startTime;
            public TimeSpan? eventDuration;
        }

        public Schedule AddScheduleWithSpecificDates( AddScheduleSpecificDatesActionArgs args )
        {
            if ( args.dates == null || !args.dates.Any() )
            {
                throw new ArgumentException( nameof( args.dates ) );
            }

            // Get the template calendar event.
            var firstDate = args.dates.First().Date.Add( args.startTime.GetValueOrDefault() );
            firstDate = DateTime.SpecifyKind( firstDate, DateTimeKind.Unspecified );

            var calendarEvent = GetICalCalendarEvent( firstDate, args.eventDuration );

            var recurrenceDates = new PeriodList();
            foreach ( var datetime in args.dates )
            {
                recurrenceDates.Add( new CalDateTime( datetime ) );
            }

            calendarEvent.RecurrenceDates.Add( recurrenceDates );

            var calendar = GetICalCalendar( calendarEvent );
            var schedule = CreateSchedule( calendar );

            var rockContext = new RockContext();

            var scheduleService = new ScheduleService( rockContext );
            scheduleService.Add( schedule );

            rockContext.SaveChanges();

            return schedule;
        }

        private static Schedule CreateSchedule( Calendar calendar )
        {
            var schedule = new Schedule();

            var serializer = new CalendarSerializer( calendar );

            schedule.iCalendarContent = serializer.SerializeToString();

            schedule.EnsureEffectiveStartEndDates();

            return schedule;
        }

        public Calendar GetICalCalendar( CalendarEvent calendarEvent, RecurrencePattern recurrencePattern = null )
        {
            if ( recurrencePattern != null )
            {
                calendarEvent.RecurrenceRules = new List<RecurrencePattern> { recurrencePattern };
            }

            var calendar = new Calendar();

            calendar.Events.Add( calendarEvent );

            return calendar;
        }

        public CalendarEvent GetICalCalendarEvent( DateTime eventStartDate, TimeSpan? eventDuration )
        {
            if ( eventStartDate.Kind != DateTimeKind.Unspecified )
            {
                throw new Exception( "The Event Start Date must have a Kind of Unspecified. Calendar Events do not store timezone information." );
            }

            var calendarEvent = new CalendarEvent
            {
                DtStamp = new CalDateTime( eventStartDate.Year, eventStartDate.Month, eventStartDate.Day )
            };

            var dtStart = new CalDateTime( eventStartDate );
            dtStart.HasTime = true;
            calendarEvent.DtStart = dtStart;

            if ( eventDuration != null )
            {
                var dtEnd = dtStart.Add( eventDuration.Value );
                dtEnd.HasTime = true;
                calendarEvent.DtEnd = dtEnd;
            }

            return calendarEvent;
        }

        public CalendarEvent GetICalCalendarEvent( DateTimeOffset eventStartDate, TimeSpan? eventDuration )
        {
            // Convert the start date to Rock time.
            var startDate = TimeZoneInfo.ConvertTime( eventStartDate, RockDateTime.OrgTimeZoneInfo );
            eventDuration = eventDuration ?? new TimeSpan( 1, 0, 0 );

            var dtStart = new CalDateTime( startDate.DateTime );
            dtStart.HasTime = true;

            var dtEnd = dtStart.Add( eventDuration.Value );
            dtEnd.HasTime = true;

            var calendarEvent = new CalendarEvent
            {
                DtStart = dtStart,
                DtEnd = dtEnd,
                DtStamp = new CalDateTime( eventStartDate.Year, eventStartDate.Month, eventStartDate.Day ),
            };

            return calendarEvent;
        }

        public RecurrencePattern GetICalDailyRecurrencePattern( DateTimeOffset? recurrenceEndDate = null, int? occurrenceCount = null, int? interval = 1 )
        {
            // Repeat daily from the start date until the specified end date or a set number of recurrences, at the specified interval.
            var pattern = $"RRULE:FREQ=DAILY;INTERVAL={interval}";

            if ( recurrenceEndDate != null )
            {
                pattern += $";UNTIL={recurrenceEndDate:yyyyMMdd}";
            }

            if ( occurrenceCount != null )
            {
                pattern += $";COUNT={occurrenceCount}";
            }

            var recurrencePattern = new RecurrencePattern( pattern );

            return recurrencePattern;
        }

        #endregion

        #region ICalendar Feed

        public class GetICalendarEventFeedActionArgs
        {
            public string calendarName = null;
            public string campusName = null;
            public DateTime? startDate = null;
            public DateTime? endDate = null;
            public string eventIdentifier = null;
        }

        public string GetICalendarEventFeed( GetICalendarEventFeedActionArgs actionArgs )
        {
            var rockContext = new RockContext();
            var calendarService = new EventCalendarService( rockContext );

            var args = new GetCalendarEventFeedArgs();

            if ( !string.IsNullOrWhiteSpace( actionArgs.calendarName ) )
            {
                var calendarId = EventCalendarCache.All()
                .Where( x => x.Name == actionArgs.calendarName )
                .Select( x => x.Id )
                .FirstOrDefault();

                args.CalendarId = calendarId;
            }

            args.StartDate = actionArgs.startDate ?? RockDateTime.New( 2015, 1, 1 ).Value;
            args.EndDate = actionArgs.endDate ?? RockDateTime.New( 2020, 1, 1 ).Value;

            if ( !string.IsNullOrWhiteSpace( actionArgs.campusName ) )
            {
                var campusId = CampusCache.All()
                    .Where( x => x.Name == actionArgs.campusName )
                    .Select( x => x.Id )
                    .FirstOrDefault();

                Assert.IsTrue( campusId != 0, "Invalid Campus." );

                args.CampusIds = new List<int> { campusId };
            }

            if ( !string.IsNullOrWhiteSpace( actionArgs.eventIdentifier ) )
            {
                var eventItemService = new EventItemService( rockContext );
                var eventItemId = eventItemService.Queryable()
                    .GetByIdentifierOrThrow( actionArgs.eventIdentifier )?.Id ?? 0;

                args.EventItemIds = new List<int> { eventItemId };
            }

            var calendarString1 = calendarService.CreateICalendar( args );
            return calendarString1;
        }

        #endregion

        #region Test Data

        /// <summary>
        /// Modifies the Rock Solid Finances Class to add multiple schedules and campuses.
        /// </summary>
        public void AddDataForRockSolidFinancesClass()
        {
            var rockContext = new RockContext();

            // Create Campus "Stepping Stone".
            TestDataHelper.GetOrAddCampusSteppingStone( rockContext );
            rockContext.SaveChanges();

            // Add an occurrence of this event for each Campus.
            var event1Args = new CreateEventItemOccurrenceActionArgs()
            {
                Guid = FinancesClassOccurrenceSat1630Guid.AsGuid(),
                ForeignKey = TestDataForeignKey,
                Properties = new EventItemOccurrenceInfo
                {
                    EventIdentifier = TestGuids.Events.EventIdentifierRockSolidFinancesClass,
                    MeetingLocationDescription = "Meeting Room 1",
                    ScheduleIdentifier = ScheduleSat1630Guid,
                    CampusIdentifier = TestGuids.Crm.CampusMain
                },
                ExistingItemStrategy = CreateExistingItemStrategySpecifier.Update
            };

            AddEventItemOccurrence( event1Args );

            var event2Args = new CreateEventItemOccurrenceActionArgs()
            {
                Guid = FinancesClassOccurrenceSun1200Guid.AsGuid(),
                ForeignKey = TestDataForeignKey,
                Properties = new EventItemOccurrenceInfo
                {
                    EventIdentifier = TestGuids.Events.EventIdentifierRockSolidFinancesClass,
                    MeetingLocationDescription = "Meeting Room 2",
                    ScheduleIdentifier = ScheduleSun1200Guid,
                    CampusIdentifier = TestGuids.Crm.CampusSteppingStone,
                },
                ExistingItemStrategy = CreateExistingItemStrategySpecifier.Update
            };

            AddEventItemOccurrence( event2Args );

        }

        /// <summary>
        /// Modify the standard sample data to scheduled events around the current date.
        /// This ensures some events will appear in the default calendar feed, which only includes upcoming events.
        /// </summary>
        public void UpdateSampleDataEventDates()
        {
            var rockContext = new RockContext();

            var effectiveDate = GetDefaultEffectiveDate();

            // Staff Meeting: Every other Wednesday @ 10:30am, starting on the first Wednesday after the effective date.
            var firstWednesday2020 = RockDateTime.New( 2020, 1, 1 ).Value.GetNextWeekday( DayOfWeek.Wednesday );
            SetStartDateForEvent( "Staff Meeting",
                firstWednesday2020.AddHours( 10 ).AddMinutes( 30 ),
                new TimeSpan( 1, 30, 0 ),
                rockContext );

            // Warrior Youth Event: 20th of next month @ 7:00pm.
            SetStartDateForEvent( "Warrior Youth Event",
                effectiveDate.AddDays( 19 ).AddHours( 19 ),
                null,
                rockContext );

            // Rock Solid Finances Class: 25th of next month @ 1:00pm.
            SetStartDateForEvent( "Rock Solid Finances Class",
                effectiveDate.AddDays( 24 ).AddHours( 13 ),
                new TimeSpan( 4, 0, 0 ),
                rockContext );

            // Customs & Classics Car Show: 10th of previous month @ 10:00am.
            SetStartDateForEvent( "Customs & Classics Car Show",
                effectiveDate.AddMonths( -2 ).AddDays( 9 ).AddHours( 10 ),
                null,
                rockContext );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the effective date for which Event test data is created.
        /// This date is the start of next month.
        /// </summary>
        /// <returns></returns>
        public DateTime GetDefaultEffectiveDate()
        {
            var nowDate = RockDateTime.Now.Date;
            var nextMonthStartDate = RockDateTime.New( nowDate.Year, nowDate.Month, 1 ).Value.AddMonths( 1 );

            return nextMonthStartDate;
        }

        private void SetStartDateForEvent( string eventName, DateTime startDate, TimeSpan? duration, RockContext rockContext )
        {
            var eventOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Set the start date for the first occurrence only.
            var occurrence = eventOccurrenceService.Queryable()
                .FirstOrDefault( i => i.EventItem.Name == eventName );

            Assert.That.IsNotNull( occurrence, $"Event not found. [Name=\"{eventName}\"]" );

            SetStartDateForSchedule( occurrence?.ScheduleId ?? 0,
                startDate,
                duration,
                rockContext );
        }

        private void SetStartDateForSchedule( int scheduleId, DateTime startDate, TimeSpan? duration, RockContext rockContext )
        {
            var scheduleService = new ScheduleService( rockContext );
            var schedule = scheduleService.Get( scheduleId );

            var endDate = startDate;

            if ( duration != null )
            {
                endDate = endDate.Add( duration.Value );
            }

            Assert.IsNotNull( schedule, $"Schedule not found. [ScheduleId={scheduleId}]" );

            var iCalEvent = InetCalendarHelper.CreateCalendarEvent( schedule.iCalendarContent );

            iCalEvent.DtStart = new CalDateTime( startDate );
            iCalEvent.DtEnd = new CalDateTime( endDate );

            schedule.EffectiveStartDate = startDate;

            schedule.iCalendarContent = InetCalendarHelper.SerializeToCalendarString( iCalEvent );
        }

        /// <summary>
        /// Modifies the Rock Solid Finances Class to remove additional test data.
        /// </summary>
        public void DeleteDataForRockSolidFinancesClass()
        {
            var rockContext = new RockContext();

            // Delete event occurrences.
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            var financeEvent1 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSat1630Guid.AsGuid() );
            if ( financeEvent1 != null )
            {
                eventItemOccurrenceService.Delete( financeEvent1 );
            }

            var financeEvent2 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSun1200Guid.AsGuid() );
            if ( financeEvent2 != null )
            {
                eventItemOccurrenceService.Delete( financeEvent2 );
            }

            // Remove campus.
            var campusService = new CampusService( rockContext );

            var campus2 = campusService.Get( TestGuids.Crm.CampusSteppingStone.AsGuid() );
            if ( campus2 != null )
            {
                campusService.Delete( campus2 );
            }

            rockContext.SaveChanges();
        }

        #endregion
    }
}
