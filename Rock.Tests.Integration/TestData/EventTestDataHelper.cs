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

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Events
        {
            private const string TestDataForeignKey = "test_data";
            private const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
            private const string FinancesClassOccurrenceSat1630Guid = "E7116C5A-9FEE-42D4-A0DB-7FEBFCCB6B8B";
            private const string FinancesClassOccurrenceSun1200Guid = "3F3EA420-E3F0-435A-9401-C2D058EF37DE";

            #region Event Item

            public class CreateEventItemActionArgs : CreateEntityActionArgsBase
            {
                public Schedule Schedule;
                public string EventName;
                public string MeetingLocation;
            }

            public static bool DeleteEventItem( string eventItemIdentifier, RockContext context )
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
            /// Creates a new Event Item - a template from which actual event occurrences can be created.
            /// </summary>
            public static EventItem CreateEventItem( CreateEventItemActionArgs actionInfo, RockContext rockContext )
            {
                rockContext = rockContext ?? new RockContext();

                var eventItemService = new EventItemService( rockContext );

                var newEvent = new EventItem();

                eventItemService.Add( newEvent );

                newEvent.Guid = actionInfo.Guid ?? Guid.NewGuid();
                newEvent.Name = actionInfo.EventName;
                newEvent.IsActive = true;
                newEvent.IsApproved = true;

                var eventCalendarService = new EventCalendarService( rockContext );
                var eventCalendarInternal = EventCalendarCache.All().FirstOrDefault( ec => ec.Name == "Internal" );

                var calendar = new EventCalendarItem();
                calendar.EventCalendarId = eventCalendarInternal.Id;
                calendar.EventItem = newEvent;

                newEvent.EventCalendarItems.Add( calendar );

                var newEvent1 = new EventItemOccurrence();
                newEvent.EventItemOccurrences.Add( newEvent1 );

                var mainCampusId = CampusCache.GetId( TestGuids.Crm.CampusMain.AsGuid() );

                newEvent1.Location = actionInfo.MeetingLocation;
                newEvent1.ForeignKey = TestDataForeignKey;
                newEvent1.Schedule = actionInfo.Schedule;
                newEvent1.CampusId = mainCampusId;

                return newEvent;
            }

            #endregion

            #region Event Item Occurrence

            public class CreateEventItemOccurrenceActionArgs : CreateEntityActionArgsBase
            {
                public string EventIdentifier;
                public string ScheduleIdentifier;
                public string CampusIdentifier;

                public string MeetingLocation;
            }

            public static bool DeleteEventItemOccurrence( string occurrenceIdentifier, RockContext context )
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
            /// Creates a new Event Item Occurrence - an actual set of one or more instances of an EventItem.
            /// The EventItem defines the template for the actual scheduled events represented by the EventItemOccurrence.
            /// </summary>
            public static EventItemOccurrence CreateEventItemOccurrence( CreateEventItemOccurrenceActionArgs actionInfo, RockContext rockContext )
            {
                rockContext = rockContext ?? new RockContext();

                // Get Event
                var eventItemService = new EventItemService( rockContext );
                var eventItem = eventItemService.Queryable().GetByIdentifierOrThrow( actionInfo.EventIdentifier );

                // Get Schedule
                var scheduleService = new ScheduleService( rockContext );
                var schedule = scheduleService.Queryable().GetByIdentifierOrThrow( actionInfo.ScheduleIdentifier );

                // Get Campus
                var campusService = new CampusService( rockContext );
                var campus = campusService.Queryable().GetByIdentifierOrThrow( actionInfo.CampusIdentifier );

                // Create new instance and set properties.
                var occurrenceService = new EventItemOccurrenceService( rockContext );

                var newOccurrence = new EventItemOccurrence();
                occurrenceService.Add( newOccurrence );

                newOccurrence.Guid = actionInfo.Guid ?? Guid.NewGuid();
                newOccurrence.ForeignKey = actionInfo.ForeignKey;
                newOccurrence.EventItemId = eventItem.Id;
                newOccurrence.Location = actionInfo.MeetingLocation;
                newOccurrence.ScheduleId = schedule.Id;
                newOccurrence.CampusId = campus.Id;

                return newOccurrence;
            }

            #endregion

            #region Schedules

            public class CreateScheduleDailyRecurrenceActionArgs : CreateEntityActionArgsBase
            {
                public DateTime? startDateTime;
                public DateTime? endDateTime;
                public TimeSpan? eventDuration;
                public int? occurrenceCount;
            }

            public static Schedule CreateScheduleWithDailyRecurrence( CreateScheduleDailyRecurrenceActionArgs args, RockContext rockContext = null )
            {
                var startDateTime = args.startDateTime ?? new DateTime( RockDateTime.Today.Ticks, DateTimeKind.Unspecified );
                var calendarEvent = GetICalCalendarEvent( startDateTime, args.eventDuration );

                var recurrence = GetICalDailyRecurrencePattern( args.endDateTime, args.occurrenceCount );

                var calendar = GetICalCalendar( calendarEvent, recurrence );

                var schedule = CreateSchedule( calendar );

                rockContext = rockContext ?? new RockContext();

                var scheduleService = new ScheduleService( rockContext );
                scheduleService.Add( schedule );

                return schedule;
            }

            public class CreateScheduleSpecificDatesActionArgs : CreateEntityActionArgsBase
            {
                public List<DateTime> dates;
                public TimeSpan? startTime;
                public TimeSpan? eventDuration;
            }

            public static Schedule CreateScheduleWithSpecificDates( CreateScheduleSpecificDatesActionArgs args, RockContext rockContext = null )
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

                rockContext = rockContext ?? new RockContext();

                var scheduleService = new ScheduleService( rockContext );
                scheduleService.Add( schedule );

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

            public static Calendar GetICalCalendar( CalendarEvent calendarEvent, RecurrencePattern recurrencePattern = null )
            {
                if ( recurrencePattern != null )
                {
                    calendarEvent.RecurrenceRules = new List<RecurrencePattern> { recurrencePattern };
                }

                var calendar = new Calendar();

                calendar.Events.Add( calendarEvent );

                return calendar;
            }

            public static CalendarEvent GetICalCalendarEvent( DateTime eventStartDate, TimeSpan? eventDuration )
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

            public static CalendarEvent GetICalCalendarEvent( DateTimeOffset eventStartDate, TimeSpan? eventDuration )
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

            public static RecurrencePattern GetICalDailyRecurrencePattern( DateTimeOffset? recurrenceEndDate = null, int? occurrenceCount = null, int? interval = 1 )
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

            public static string GetICalendarEventFeed( GetICalendarEventFeedActionArgs actionArgs )
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

            /// <summary>
            /// Modifies the Rock Solid Finances Class to add multiple schedules and campuses.
            /// </summary>
            public static void AddDataForRockSolidFinancesClass()
            {
                var rockContext = new RockContext();

                // Create Campus "Stepping Stone".
                var campus = TestDataHelper.GetOrAddCampusSteppingStone( rockContext );

                rockContext.SaveChanges();

                // Add an occurrence of this event for each Campus.
                var event1Args = new CreateEventItemOccurrenceActionArgs()
                {
                    Guid = FinancesClassOccurrenceSat1630Guid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    EventIdentifier = TestGuids.Events.EventIdentifierRockSolidFinancesClass,
                    MeetingLocation = "Meeting Room 1",
                    ScheduleIdentifier = ScheduleSat1630Guid,
                    CampusIdentifier = TestGuids.Crm.CampusMain
                };

                var financeEvent1 = CreateEventItemOccurrence( event1Args, rockContext );

                var event2Args = new CreateEventItemOccurrenceActionArgs()
                {
                    Guid = FinancesClassOccurrenceSun1200Guid.AsGuid(),
                    ForeignKey = TestDataForeignKey,
                    EventIdentifier = TestGuids.Events.EventIdentifierRockSolidFinancesClass,
                    MeetingLocation = "Meeting Room 2",
                    ScheduleIdentifier = ScheduleSat1630Guid,
                    CampusIdentifier = TestGuids.Crm.CampusSteppingStone
                };

                var financeEvent2 = CreateEventItemOccurrence( event2Args, rockContext );

                rockContext.SaveChanges();
            }
        }
    }
}
