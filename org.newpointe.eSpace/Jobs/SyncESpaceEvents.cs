using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using NewPointe.eSpace;

namespace org.newpointe.eSpace.Jobs
{
    /// <summary>
    /// Syncs events from eSpace into Rock
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "eSpace Event Sync" )]
    [Description( "This job will sync events from eSpace into Rock so that they show up on the calendar and can be linked to registrations." )]
    [TextField( "eSpace Username", "The username of the eSpace account to use for syncing - You should set up a dedicated account for this.", true, "", "", 0, AttributeKey_eSpaceUsername, false )]
    [EncryptedTextField( "eSpace Password", "The password of the eSpace account to use for syncing.", true, "", "", 1, AttributeKey_eSpacePassword, true )]
    [EventCalendarField( "Calendar", "The calendar to sync all events to.", true, "", "", 2, AttributeKey_Calendar )]
    [EventCalendarField( "Public Calendar", "The calendar to sync all public events to.", false, "", "", 2, AttributeKey_PublicCalendar )]
    [AttributeField( "71632e1a-1e7f-42b9-a630-ec99f375303a", "Occurrence Approved Attribute", "The Attribute that indicates an Occurrence is approved", false, false, "", "", 3, AttributeKey_ApprovedAttribute )]
    class SyncESpaceEvents : IJob
    {

        const string AttributeKey_eSpaceUsername = "eSpaceUsername";
        const string AttributeKey_eSpacePassword = "eSpacePassword";
        const string AttributeKey_Calendar = "Calendar";
        const string AttributeKey_PublicCalendar = "PublicCalendar";
        const string AttributeKey_ApprovedAttribute = "ApprovedAttribute";

        public void Execute( IJobExecutionContext context )
        {
            ExecuteAsync( context ).Wait();
        }

        public async Task ExecuteAsync( IJobExecutionContext context )
        {
            // Get the configuration for the job
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var approvalAttributeGuid = dataMap.GetString( AttributeKey_Calendar ).AsGuid();
            var approvalAttribute = AttributeCache.Get( approvalAttributeGuid );

            // Collect some values for matching
            var campuses = CampusCache.All();
            var audiences = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ).DefinedValues;

            // Get the calendars
            var calendarGuid = dataMap.GetString( AttributeKey_Calendar ).AsGuid();
            var publicCalendarGuid = dataMap.GetString( AttributeKey_PublicCalendar ).AsGuidOrNull();

            var calendar = EventCalendarCache.Get( calendarGuid );
            var publicCalendar = publicCalendarGuid.HasValue ? EventCalendarCache.Get( publicCalendarGuid.Value ) : null;

            // Get the login credentials
            string eSpaceUsername = dataMap.GetString( AttributeKey_eSpaceUsername );
            string eSpacePassword = dataMap.GetString( AttributeKey_eSpacePassword );

            // Decrypt the password
            eSpacePassword = Rock.Security.Encryption.DecryptString( eSpacePassword );

            // Create a new api client
            var client = new Client();

            // Log in
            client.SetCredentials( eSpaceUsername, eSpacePassword );

            // Get all events
            var eSpaceEvents = await client.GetEvents();

            // Group by event id (the eSpace api returns "events" as a merged event and schedule)
            var eSpaceEventsById = eSpaceEvents.GroupBy( e => e.EventId );
            var eventTotalCount = eSpaceEventsById.Count();

            // Loop through each eSpace event group
            var eventSyncedCount = 0;
            var eventErrorCount = 0;
            foreach ( var eSpaceEventGroup in eSpaceEventsById )
            {
                eventSyncedCount++;

                try
                {

                    // Create our Rock services
                    var rockContext = new RockContext();
                    var personService = new PersonService( rockContext );
                    var eventItemService = new EventItemService( rockContext );
                    var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                    var eventItemAudienceService = new EventItemAudienceService( rockContext );

                    // Use the first item as the main event - Note that some properties
                    // here are actually part of the schedule, not the event
                    var eSpaceEvent = eSpaceEventGroup.FirstOrDefault();

                    // Update the job status
                    context.UpdateLastStatusMessage( $@"Syncing event {eventSyncedCount} of {eventTotalCount} ({Math.Round( (double) eventSyncedCount / eventTotalCount * 100, 0 )}%, {eventErrorCount} events with errors)" );

                    // Get the best match for this event's campus
                    var eSpaceEventLocationCodes = new List<string>();
                    if ( eSpaceEvent.Locations != null ) eSpaceEventLocationCodes.AddRange( eSpaceEvent.Locations.Select( l => l.LocationCode ) );
                    if ( eSpaceEvent.PublicLocations != null ) eSpaceEventLocationCodes.AddRange( eSpaceEvent.PublicLocations.Select( l => l.LocationCode ) );
                    var eventCampusId = campuses.FirstOrDefault( c => eSpaceEventLocationCodes.Contains( c.ShortCode, StringComparer.OrdinalIgnoreCase ) )?.Id;

                    var rockEventItem = CreateOrUpdateEvent(
                        eventItemService,
                        eSpaceEvent,
                        calendar,
                        publicCalendar
                    );

                    // Get all eSpace occurrences for the event
                    var eSpaceEventOccurrences = await client.GetEventOccurrences( new GetEventOccurrencesOptions { EventId = eSpaceEvent.EventId } );

                    // Loop through each eSpace occurrences
                    foreach ( var eSpaceEventOccurrence in eSpaceEventOccurrences )
                    {

                        // Create or Update the Rock Occurrence
                        CreateOrUpdateOccurrence(
                            personService,
                            eventItemOccurrenceService,
                            eventCampusId,
                            approvalAttribute,
                            rockEventItem,
                            eSpaceEvent,
                            eSpaceEventOccurrence
                        );

                    }

                    // Save our changes
                    rockContext.SaveChanges();

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                    eventErrorCount++;
                }

            }

            // Update the job status
            context.UpdateLastStatusMessage( $@"Synced {eventSyncedCount} events with {eventErrorCount} errors." );

        }

        public static EventItem CreateOrUpdateEvent(
            EventItemService eventItemService,
            NewPointe.eSpace.Models.Event eSpaceEvent,
            EventCalendarCache calendar,
            EventCalendarCache publicCalendar
        )
        {

            // Look for a matching Rock event
            EventItem eventItem = eventItemService.Queryable().FirstOrDefault(
                e => e.ForeignKey == "eSpaceEventId" && e.ForeignId == eSpaceEvent.EventId
            );

            // If we didn't find any, create a new one
            if ( eventItem == null )
            {
                eventItem = new EventItem
                {
                    ForeignKey = "eSpaceEventId",
                    ForeignId = eSpaceEvent.EventId
                };
                eventItemService.Add( eventItem );
            }

            // Update the event 
            eventItem.Name = eSpaceEvent.EventName;
            eventItem.IsApproved = eSpaceEvent.Status == "Approved";
            eventItem.Summary = eSpaceEvent.PublicNotes;

            // If it's not on the calendar, add it
            if ( !eventItem.EventCalendarItems.Any( ci => ci.EventCalendarId == calendar.Id ) )
            {
                eventItem.EventCalendarItems.Add( new EventCalendarItem { EventCalendarId = calendar.Id } );
            }

            // If it's public add it to the public calendar
            if ( ( eSpaceEvent.IsPublic ?? false ) && publicCalendar != null )
            {
                if ( !eventItem.EventCalendarItems.Any( ci => ci.EventCalendarId == publicCalendar.Id ) )
                {
                    eventItem.EventCalendarItems.Add( new EventCalendarItem { EventCalendarId = publicCalendar.Id } );
                }
            }

            return eventItem;

        }

        public static void CreateOrUpdateOccurrence(
            PersonService personService,
            EventItemOccurrenceService eventItemOccurrenceService,
            int? eventCampusId,
            AttributeCache approvalAttribute,
            EventItem eventItem,
            NewPointe.eSpace.Models.Event eSpaceEvent,
            NewPointe.eSpace.Models.Occurrence eSpaceEventOccurrence
        )
        {

            // Try to find an existing occurrence
            EventItemOccurrence occurrence = eventItemOccurrenceService.Queryable().FirstOrDefault(
                o => o.ForeignKey == "eSpaceOccurrenceId" && o.ForeignId == eSpaceEventOccurrence.OccurrenceId
            );

            // If we didn't find any, create a new one
            if ( occurrence == null )
            {
                occurrence = new EventItemOccurrence
                {
                    ForeignKey = "eSpaceOccurrenceId",
                    ForeignId = eSpaceEventOccurrence.OccurrenceId
                };
                eventItemOccurrenceService.Add( occurrence );
            }

            // If we didn't find any, create a new one
            if ( occurrence == null ) eventItemOccurrenceService.Add( occurrence = new EventItemOccurrence() );

            // Update the Occurrence Info
            occurrence.EventItem = eventItem;
            occurrence.Location = eSpaceEvent.OffsiteLocation;
            occurrence.CampusId = eventCampusId;
            occurrence.Schedule = CreateOrUpdateSchedule( occurrence.Schedule, eSpaceEvent, eSpaceEventOccurrence );

            if ( eSpaceEvent.Contacts != null && eSpaceEvent.Contacts.Length > 0 )
            {
                occurrence.ContactEmail = eSpaceEvent.Contacts[0].Email;
                occurrence.ContactPhone = eSpaceEvent.Contacts[0].Phone;

                Person contactPerson = personService.FindPerson(
                    eSpaceEvent.Contacts[0].FirstName,
                    eSpaceEvent.Contacts[0].LastName,
                    eSpaceEvent.Contacts[0].Email,
                    false
                );

                if ( contactPerson != null )
                {
                    occurrence.ContactPersonAlias = contactPerson.PrimaryAlias;
                }

            }

            if ( eSpaceEventOccurrence.EventStatus == "Approved" && approvalAttribute != null )
            {
                occurrence.LoadAttributes();
                occurrence.SetAttributeValue( approvalAttribute.Key, "Approved" );
            }


        }

        private static Schedule CreateOrUpdateSchedule( Schedule schedule, NewPointe.eSpace.Models.Event eSpaceEvent, NewPointe.eSpace.Models.Occurrence eSpaceEventOccurrence )
        {

            // Check that the schedule is correct
            if ( schedule != null )
            {

                schedule.EffectiveStartDate = eSpaceEventOccurrence.EventStart;
                schedule.EffectiveEndDate = eSpaceEventOccurrence.EventEnd;
                schedule.Name = eSpaceEventOccurrence.EventName.Truncate( 50 );


                // Get the calendar event
                var calendarEvent = schedule.GetCalendarEvent();

                // Set the start/end dates
                if ( eSpaceEventOccurrence.EventStart.HasValue )
                {
                    calendarEvent.Start = new DDay.iCal.iCalDateTime( eSpaceEventOccurrence.EventStart.Value );
                }

                if ( eSpaceEventOccurrence.EventEnd.HasValue )
                {
                    calendarEvent.End = new DDay.iCal.iCalDateTime( eSpaceEventOccurrence.EventEnd.Value );
                }

                if ( eSpaceEventOccurrence.IsAllDay ?? false )
                {
                    calendarEvent.IsAllDay = true;
                }

                // Clear any recurrence rules
                calendarEvent.RecurrenceRules = new List<DDay.iCal.IRecurrencePattern>();
                calendarEvent.RecurrenceDates = new List<DDay.iCal.IPeriodList>();
                calendarEvent.ExceptionRules = new List<DDay.iCal.IRecurrencePattern>();
                calendarEvent.ExceptionDates = new List<DDay.iCal.IPeriodList>();

                // Serialize the event
                var calendar = new DDay.iCal.iCalendar();
                calendar.Events.Add( calendarEvent );
                var serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer( calendar );

                // Set the new iCal data
                schedule.iCalendarContent = serializer.SerializeToString( calendar );

            }
            else
            {

                // Create the calendar event
                var calendarEvent = new DDay.iCal.Event
                {
                    Start = new DDay.iCal.iCalDateTime( eSpaceEventOccurrence.EventStart.Value ),
                    End = new DDay.iCal.iCalDateTime( eSpaceEventOccurrence.EventEnd.Value ),
                    IsAllDay = eSpaceEventOccurrence.IsAllDay ?? false
                };

                // Serialize the event
                var calendar = new DDay.iCal.iCalendar();
                calendar.Events.Add( calendarEvent );
                var serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer( calendar );
                var iCalendarContent = serializer.SerializeToString( calendar );

                // Create the schedule
                schedule = new Schedule
                {
                    EffectiveStartDate = eSpaceEventOccurrence.EventStart,
                    EffectiveEndDate = eSpaceEventOccurrence.EventEnd,
                    Name = eSpaceEventOccurrence.EventName.Truncate( 50 ),
                    ForeignKey = "eSpaceOccurrenceId",
                    ForeignId = eSpaceEventOccurrence.OccurrenceId,
                    iCalendarContent = iCalendarContent
                };

            }

            return schedule;

        }

    }

}
