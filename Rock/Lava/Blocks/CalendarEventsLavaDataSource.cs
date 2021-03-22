﻿// <copyright>
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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Ical.Net.DataTypes;
using Rock.Utility;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Creates filtered sets of calendar events as Lava data objects suitable for use in a Lava template.
    /// </summary>
    public class EventOccurrencesLavaDataSource
    {
        #region Filter Parameter Names

        /// <summary>
        /// Parameter name for specifying the Calendar from which the Event Occurrences should be retrieved. If not specified, all calendars are considered.
        /// </summary>
        public static readonly string ParameterCalendarId = "calendarid";
        /// <summary>
        /// Parameter name for specifying the Event for which occurrences should be retrieved.
        /// </summary>
        public static readonly string ParameterEventId = "eventid";
        /// <summary>
        /// Parameter name for specifying maximum occurrences. If not specified, the default value is 100.
        /// </summary>
        public static readonly string ParameterMaxOccurrences = "maxoccurrences";
        /// <summary>
        /// Parameter name for specifying the start date of the filter period. If not specified, the default value is today.
        /// </summary>
        public static readonly string ParameterStartDate = "startdate";
        /// <summary>
        /// Parameter name for specifying the maximum date range from the start date. If not specified, the default value is 100 years.
        /// </summary>
        public static readonly string ParameterDateRange = "daterange";
        /// <summary>
        /// Parameter name for specifying a filter for the intended audiences of the Event Occurrences. If not specified, all audiences are considered.
        /// </summary>
        public static readonly string ParameterAudienceIds = "audienceids";

        #endregion

        /// <summary>
        /// The maximum number of events that can be retrieved for this data source, regardless of parameter settings.
        /// </summary>
        public static readonly int MaximumResultSetSize = 10000;

        /// <summary>
        /// Get a filtered set of occurrences for a specific calendar.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public List<EventOccurrenceSummary> GetEventOccurrencesForCalendar( LavaElementAttributes settings )
        {
            // Check for invalid parameters.
            var unknownNames = settings.GetUnmatchedAttributes( new List<string> { ParameterCalendarId, ParameterAudienceIds, ParameterDateRange, ParameterMaxOccurrences, ParameterStartDate } );

            if ( unknownNames.Any() )
            {
                throw new Exception( $"Invalid configuration setting \"{unknownNames.AsDelimited( "," )}\"." );
            }

            var rockContext = new RockContext();

            // Get the Event Calendar.
            var calendar = ResolveCalendarSettingOrThrow( rockContext, settings.GetStringValue( ParameterCalendarId ) );

            // Get the Date Range.
            var startDate = settings.GetDateTimeValue( ParameterStartDate, RockDateTime.Today );

            var dateRange = settings.GetStringValue( ParameterDateRange, string.Empty ).ToLower();

            var endDate = GetEndDateFromStartDateAndRange( startDate, dateRange );

            // Get the Maximum Occurrences.
            int maxOccurrences = 100;

            if ( settings.HasValue( ParameterMaxOccurrences ) )
            {
                maxOccurrences = settings.GetIntegerValue( ParameterMaxOccurrences, null ) ?? 0;

                if ( maxOccurrences == 0 )
                {
                    throw new Exception( $"Invalid configuration setting \"maxoccurrences\"." );
                }
            }

            // Get the Audiences.
            var audienceIdList = ResolveAudienceSettingOrThrow( settings.GetStringValue( ParameterAudienceIds, string.Empty ) );

            // Get the result set.
            var qryOccurrences = GetBaseEventOccurrenceQuery( rockContext );

            qryOccurrences = qryOccurrences.Where( m => m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == calendar.Id ) );

            var summaries = GetFilteredEventOccurrenceSummaries( qryOccurrences, audienceIdList, maxOccurrences, startDate, endDate );

            return summaries;
        }

        /// <summary>
        /// Get a filtered set of occurrences for a specific calendar.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public List<EventOccurrenceSummary> GetEventOccurrencesForEvent( LavaElementAttributes settings )
        {
            // Check for invalid parameters.
            var unknownNames = settings.GetUnmatchedAttributes( new List<string> { ParameterEventId, ParameterDateRange, ParameterMaxOccurrences, ParameterStartDate } );

            if ( unknownNames.Any() )
            {
                throw new Exception( $"Invalid configuration setting \"{unknownNames.AsDelimited( "," )}\"." );
            }

            var rockContext = new RockContext();

            // Get the Event.
            var eventItem = ResolveEventSettingOrThrow( rockContext, settings.GetStringValue( ParameterEventId ) );

            // Get the Date Range.
            var startDate = settings.GetDateTimeValue( ParameterStartDate, RockDateTime.Today );

            var dateRange = settings.GetStringValue( ParameterDateRange, string.Empty ).ToLower();

            var endDate = GetEndDateFromStartDateAndRange( startDate, dateRange );

            // Get the Maximum Occurrences.
            int maxOccurrences = 100;

            if ( settings.HasValue( ParameterMaxOccurrences ) )
            {
                maxOccurrences = settings.GetIntegerValue( ParameterMaxOccurrences, null ) ?? 0;

                if ( maxOccurrences == 0 )
                {
                    throw new Exception( $"Invalid configuration setting \"maxoccurrences\"." );
                }
            }

            // Get the Audiences.
            var audienceIdList = ResolveAudienceSettingOrThrow( settings.GetStringValue( ParameterAudienceIds, string.Empty ) );

            // Get the result set.
            var qryOccurrences = GetBaseEventOccurrenceQuery( rockContext );

            qryOccurrences = qryOccurrences.Where( m => m.EventItem.Id == eventItem.Id );

            var summaries = GetFilteredEventOccurrenceSummaries( qryOccurrences, audienceIdList, maxOccurrences, startDate, endDate );

            return summaries;
        }

        private DateTime? GetEndDateFromStartDateAndRange( DateTime? startDate, string dateRangeExpression )
        {
            // Get the Date Range.
            DateTime? endDate = null;

            if ( !string.IsNullOrEmpty( dateRangeExpression ) )
            {
                string rangePeriod;

                if ( dateRangeExpression.IsDigitsOnly() )
                {
                    rangePeriod = "d";
                }
                else
                {
                    rangePeriod = dateRangeExpression.Right( 1 );

                    dateRangeExpression = dateRangeExpression.Substring( 0, dateRangeExpression.Length - 1 );
                }

                int? rangeAmount = dateRangeExpression.AsIntegerOrNull();

                if ( rangeAmount == null )
                {
                    throw new Exception( "The specified Date Range is invalid." );
                }

                // Get the end date by adding the range increment to the start date.
                // A range of 1 indicates that the start date and end date are the sameno change to the start date.
                var increment = rangeAmount.Value - 1;

                if ( rangePeriod == "m" )
                {
                    // Range in Months
                    endDate = startDate.Value.AddMonths( rangeAmount.Value );

                }
                else if ( rangePeriod == "w" )
                {
                    // Range in Weeks.
                    endDate = startDate.Value.AddDays( increment * 7 );
                }
                else
                {
                    // Range in Days.
                    // A range of 1 day indicates that the start date and end date should be the same.
                    endDate = startDate.Value.AddDays( increment );
                }

                // Adjust the calculated end date to the previous day because the time period is inclusive of the start and end dates.
                // For example, a range of 1 day requires that the start date and end date are the same.
                endDate = endDate.Value.AddDays( -1 );
            }

            return endDate;
        }

        private List<int> ResolveAudienceSettingOrThrow( string audienceSettingValue )
        {
            var audienceIdList = new List<int>();

            if ( !string.IsNullOrWhiteSpace( audienceSettingValue ) )
            {
                var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.CONTENT_CHANNEL_AUDIENCE_TYPE );

                var audiences = audienceSettingValue.SplitDelimitedValues( "," );

                foreach ( var audience in audiences )
                {
                    DefinedValueCache definedValue = null;

                    // Get by ID.
                    var audienceId = audience.AsIntegerOrNull();

                    if ( audienceId != null )
                    {
                        definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Id == audienceId );

                    }

                    // Get by Guid.
                    if ( definedValue == null )
                    {
                        var audienceGuid = audience.AsGuidOrNull();

                        if ( audienceGuid != null )
                        {
                            definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Guid == audienceGuid );
                        }
                    }

                    // Get by Value.
                    if ( definedValue == null )
                    {
                        var audienceValue = audience.Trim();

                        definedValue = definedType.DefinedValues.FirstOrDefault( x => x.Value == audienceValue );
                    }

                    // Report an error if the Audience is invalid.
                    if ( definedValue == null )
                    {
                        throw new Exception( $"Cannot apply an audience filter for the reference \"{ audience }\"." );
                    }

                    audienceIdList.Add( definedValue.Id );
                }
            }

            return audienceIdList;
        }

        private EventCalendar ResolveCalendarSettingOrThrow( RockContext rockContext, string calendarSettingValue )
        {
            var calendarService = new EventCalendarService( rockContext );

            EventCalendar calendar = null;

            // Verify that a calendar reference has been provided.
            if ( string.IsNullOrWhiteSpace( calendarSettingValue ) )
            {
                throw new Exception( $"A calendar reference must be specified." );
            }

            // Get by ID.
            var calendarId = calendarSettingValue.AsIntegerOrNull();

            if ( calendarId != null )
            {
                calendar = calendarService.Get( calendarId.Value );
            }

            // Get by Guid.
            if ( calendar == null )
            {
                var calendarGuid = calendarSettingValue.AsGuidOrNull();

                if ( calendarGuid != null )
                {
                    calendar = calendarService.Get( calendarGuid.Value );
                }
            }

            // Get By Name.
            if ( calendar == null )
            {
                var calendarName = calendarSettingValue.ToString();

                if ( !string.IsNullOrWhiteSpace( calendarName ) )
                {
                    calendar = calendarService.Queryable()
                        .Where( x => x.Name != null && x.Name.Equals( calendarName, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
                }
            }

            if ( calendar == null )
            {
                throw new Exception( $"Cannot find a calendar matching the reference \"{ calendarSettingValue }\"." );
            }

            return calendar;
        }

        private EventItem ResolveEventSettingOrThrow( RockContext rockContext, string eventSettingValue )
        {
            var eventItemService = new EventItemService( rockContext );

            EventItem eventItem = null;

            // Verify that an Event reference has been provided.
            if ( string.IsNullOrWhiteSpace( eventSettingValue ) )
            {
                throw new Exception( $"An Event reference must be specified." );
            }

            // Get by ID.
            var eventId = eventSettingValue.AsIntegerOrNull();

            if ( eventId != null )
            {
                eventItem = eventItemService.Get( eventId.Value );
            }

            // Get by Guid.
            if ( eventItem == null )
            {
                var eventGuid = eventSettingValue.AsGuidOrNull();

                if ( eventGuid != null )
                {
                    eventItem = eventItemService.Get( eventGuid.Value );
                }
            }

            // Get By Name.
            if ( eventItem == null )
            {
                var eventName = eventSettingValue.ToString();

                if ( !string.IsNullOrWhiteSpace( eventName ) )
                {
                    eventItem = eventItemService.Queryable()
                        .Where( x => x.Name != null && x.Name.Equals( eventName, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();
                }
            }

            if ( eventItem == null )
            {
                throw new Exception( $"Cannot find an Event matching the reference \"{ eventSettingValue }\"." );
            }

            return eventItem;
        }

        private IQueryable<EventItemOccurrence> GetBaseEventOccurrenceQuery( RockContext rockContext )
        {
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Get active and approved Event Occurrences.
            var qryOccurrences = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,Schedule" );

            return qryOccurrences;
        }

        private List<EventOccurrenceSummary> GetFilteredEventOccurrenceSummaries( IQueryable<EventItemOccurrence> qryOccurrences, List<int> audienceIdList, int maxOccurrences, DateTime? startDate, DateTime? endDate )
        {
            qryOccurrences = qryOccurrences.Where( x => x.EventItem.IsActive && x.EventItem.IsApproved );

            // Filter by Audience
            if ( audienceIdList != null
                 && audienceIdList.Any() )
            {
                qryOccurrences = qryOccurrences.Where( i => i.EventItem.EventItemAudiences.Any( c => audienceIdList.Contains( c.DefinedValueId ) ) );
            }

            // Get the occurrences
            if ( maxOccurrences < 1 || maxOccurrences > MaximumResultSetSize )
            {
                maxOccurrences = 100;
            }

            if ( startDate == null )
            {
                startDate = RockDateTime.Today;
            }

            // Querying the schedule occurrences requires a specific end date, but the ICal library throws an Exception for values of Date.MaxValue,
            // so we must set an arbitrary date here.
            if ( endDate == null )
            {
                endDate = startDate.Value.AddYears( 100 );
            }

            var occurrencesWithDates = qryOccurrences.ToList()
                .Select( o =>
                {
                    var eventOccurrenceDate = new EventOccurrenceDate
                    {
                        EventItemOccurrence = o

                    };

                    if ( o.Schedule != null )
                    {
                        eventOccurrenceDate.ScheduleOccurrences = o.Schedule.GetICalOccurrences( startDate.Value, endDate ).ToList();
                    }
                    else
                    {
                        eventOccurrenceDate.ScheduleOccurrences = new List<Occurrence>();
                    }

                    return eventOccurrenceDate;
                } )
                .Where( d => d.ScheduleOccurrences.Any() )
                .ToList();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();

            bool finished = false;

            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;

                foreach ( var scheduleOccurrence in occurrenceDates.ScheduleOccurrences )
                {
                    var datetime = scheduleOccurrence.Period.StartTime.Value;
                    var occurrenceEndTime = scheduleOccurrence.Period.EndTime;

                    if ( datetime >= startDate
                         && ( endDate == null || datetime < endDate ) )
                    {
                        eventOccurrenceSummaries.Add( new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            EndDate = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortDateString() : null,
                            EndTime = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortTimeString() : null,
                            Campus = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            LocationDescription = eventItemOccurrence.Location,
                            Description = eventItemOccurrence.EventItem.Description,
                            Summary = eventItemOccurrence.EventItem.Summary,
                            OccurrenceNote = eventItemOccurrence.Note.SanitizeHtml(),
                            DetailPage = string.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl,
                            CalendarNames = eventItemOccurrence.EventItem.EventCalendarItems.Select( x => x.EventCalendar.Name ).ToList(),
                            AudienceNames = eventItemOccurrence.EventItem.EventItemAudiences.Select( x => x.DefinedValue.Value ).ToList(),
                        } );

                        // Exit if the occurrence limit has been reached.
                        if ( eventOccurrenceSummaries.Count >= maxOccurrences )
                        {
                            finished = true;
                            break;
                        }
                    }
                }

                if ( finished )
                {
                    break;
                }
            }

            return eventOccurrenceSummaries;
        }
    }

    #region Helper Classes

    /// <summary>
    /// A class to store event item occurrence data for use in a Lava Template.
    /// </summary>
    //[DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "EndDate", "EndTime", "Campus", "Location", "LocationDescription", "Description", "Summary", "OccurrenceNote", "DetailPage", "CalendarNames", "AudienceNames" )]
    public class EventOccurrenceSummary : RockDynamic
    {
        /// <summary>
        /// The data model for this event occurrence.
        /// </summary>
        public EventItemOccurrence EventItemOccurrence { get; set; }

        /// <summary>
        /// The start date/time of this event occurrence.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The name of this event.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The start date of this event occurrence, without the time component.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// The start time of this event occurrence, without the date component.
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// The end date of this event occurrence, without the time component.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// The end time of this event occurrence, without the date component.
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// The Campus with which this event occurrence is associated.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// The name of the Location where this event occurrence is held.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The description of the Location where this event occurrence is held.
        /// </summary>
        public string LocationDescription { get; set; }

        /// <summary>
        /// A summary of the event occurrence.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The description of the event occurrence.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The note associated with this event occurrence.
        /// </summary>
        public string OccurrenceNote { get; set; }

        /// <summary>
        /// The URL for the details page associated with this event occurrence.
        /// </summary>
        public string DetailPage { get; set; }

        /// <summary>
        /// A list of the calendars with which this event occurrence is associated.
        /// </summary>
        public List<string> CalendarNames { get; set; }

        /// <summary>
        /// A list of the audiences targeted for this event occurrence.
        /// </summary>
        public List<string> AudienceNames { get; set; }
    }

    /// <summary>
    /// An internal data structure to store event item occurrences dates.
    /// </summary>
    internal class EventOccurrenceDate
    {
        public EventItemOccurrence EventItemOccurrence { get; set; }

        public List<Occurrence> ScheduleOccurrences { get; set; }
    }

    #endregion
}
