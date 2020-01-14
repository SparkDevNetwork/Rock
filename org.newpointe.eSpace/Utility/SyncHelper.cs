using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using NewPointe.eSpace;
using ESpace = NewPointe.eSpace.Models;

namespace org.newpointe.eSpace.Utility
{
    static class SyncHelper
    {
        public const string ForeignKey_eSpaceEventId = "eSpaceEventId";
        public const string ForeignKey_eSpaceOccurrenceId = "eSpaceOccurrenceId";

        public const string ESpaceStatus_Draft = "Draft";
        public const string ESpaceStatus_Pending = "Pending";
        public const string ESpaceStatus_PartialApproved = "PartialApproved";
        public const string ESpaceStatus_Approved = "Approved";
        public const string ESpaceStatus_Canceled = "Canceled";

        /// <summary>
        /// Returns all Campuses that match the given eSpace Locations.
        /// </summary>
        /// <param name="eSpaceLocations">A list of eSpace locations</param>
        /// <returns>A list of matching Campuses</returns>
        public static CampusCache[] MatchLocations( ESpace.Location[] eSpaceLocations )
        {
            if ( eSpaceLocations == null ) return new CampusCache[0];
            var allCampuses = CampusCache.All();
            var eSpaceShortCodes = eSpaceLocations.Select( l => l.LocationCode ).ToArray();
            return allCampuses.Where( c => eSpaceShortCodes.Contains( c.ShortCode ) ).ToArray();
        }

        public static DefinedValueCache[] MatchCategories( ESpace.Category[] eSpaceCategories )
        {
            var allCategories = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ).DefinedValues;
            var eSpaceCategoryNames = eSpaceCategories.Select( c => c.Name ).ToArray();
            return allCategories.Where( c => eSpaceCategoryNames.Contains( c.Value ) ).ToArray();
        }

        /// <summary>
        /// Syncs an eSpace event into Rock.
        /// </summary>
        /// <param name="eSpaceClient">The eSpace client</param>
        /// <param name="eSpaceEvent">The eSpace event</param>
        /// <param name="occurrencesFilter">The filter to use when syncing occurrences</param>
        /// <returns>The synced Rock event</returns>
        public static async Task SyncEvent(
            Client eSpaceClient,
            ESpace.Event eSpaceEvent,
            GetEventOccurrencesOptions occurrencesFilter,
            EventCalendarCache globalCalendar,
            EventCalendarCache publicCalendar,
            EventCalendarCache privateCalendar,
            string occurrenceApprovedAttributeKey
        )
        {
            using ( var rockContext = new RockContext() )
            {
                // Create our services
                var eventItemService = new EventItemService( rockContext );
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                var eventItemAudienceService = new EventItemAudienceService( rockContext );
                var personService = new PersonService( rockContext );

                // Get or create the linked Rock event
                var rockEvent = eventItemService.GetOrCreateByForeignId(
                    "EventCalendarItems,EventItemOccurrences",
                    ForeignKey_eSpaceEventId,
                    eSpaceEvent.EventId.Value,
                    out var _
                );

                // Track if we needed to update anything
                var changed = false;

                // Update the Name
                if ( rockEvent.Name != eSpaceEvent.EventName )
                {
                    changed = true;
                    rockEvent.Name = eSpaceEvent.EventName;
                }

                // Update the Active State
                var eSpaceEventIsActive = eSpaceEvent.Status != ESpaceStatus_Draft;
                if ( rockEvent.IsActive != eSpaceEventIsActive )
                {
                    changed = true;
                    rockEvent.IsActive = eSpaceEventIsActive;
                }

                // Update the Approval State
                var eSpaceEventIsApproved = eSpaceEvent.Status == ESpaceStatus_Approved;
                if ( rockEvent.IsApproved != eSpaceEventIsApproved )
                {
                    changed = true;
                    rockEvent.IsApproved = eSpaceEventIsApproved;

                    if ( eSpaceEventIsApproved ) rockEvent.ApprovedOnDateTime = DateTime.Now;
                    else rockEvent.ApprovedOnDateTime = null;
                }

                // Update the Summary
                if ( rockEvent.Summary != eSpaceEvent.Description )
                {
                    changed = true;
                    rockEvent.Summary = eSpaceEvent.Description;
                }

                // Update the details Url
                var eSpaceEventPublicLink = eSpaceEvent.PublicLink?.ToString();
                if ( rockEvent.DetailsUrl != eSpaceEventPublicLink )
                {
                    changed = true;
                    rockEvent.DetailsUrl = eSpaceEventPublicLink;
                }

                // Update the audiences
                var eSpaceCategories = MatchCategories( eSpaceEvent.Categories );


                // Check global calendar
                if ( globalCalendar != null )
                {
                    rockEvent.AddToCalendar( globalCalendar, out var addedToCalendar );
                    changed = changed || addedToCalendar;
                }

                var eventCalendarItemService = new EventCalendarItemService( rockContext );

                // Check public calendar
                if ( publicCalendar != null )
                {
                    if ( eSpaceEvent.IsPublic ?? false )
                    {
                        rockEvent.AddToCalendar( publicCalendar, out var addedToCalendar );
                        changed = changed || addedToCalendar;
                    }
                    else
                    {
                        rockEvent.RemoveFromCalendar( eventCalendarItemService, publicCalendar, out var removedFromCalendar );
                        changed = changed || removedFromCalendar;
                    }
                }

                // Check private calendar
                if ( privateCalendar != null )
                {
                    if ( !( eSpaceEvent.IsPublic ?? false ) )
                    {
                        rockEvent.AddToCalendar( privateCalendar, out var addedToCalendar );
                        changed = changed || addedToCalendar;
                    }
                    else
                    {
                        rockEvent.RemoveFromCalendar( eventCalendarItemService, privateCalendar, out var removedFromCalendar );
                        changed = changed || removedFromCalendar;
                    }

                }

                // Fetch the occurrences for the event
                if ( occurrencesFilter == null ) occurrencesFilter = new GetEventOccurrencesOptions { StartDate = DateTime.Now };
                occurrencesFilter.EventId = eSpaceEvent.EventId;
                var eSpaceEventOccurrences = await eSpaceClient.GetEventOccurrences( occurrencesFilter );

                // Calculate some stuff for the occurrences
                var campusLocation = MatchLocations( eSpaceEvent.Locations ).Concat( MatchLocations( eSpaceEvent.PublicLocations ) ).FirstOrDefault();
                var contactPerson = personService.FindPerson( eSpaceEvent.Contacts.FirstOrDefault() );

                var firstESpaceOccurrence = eSpaceEventOccurrences.FirstOrDefault();
                if ( firstESpaceOccurrence != null )
                {
                    // Update the Description
                    if ( rockEvent.Description != firstESpaceOccurrence.PublicHtmlNotes )
                    {
                        rockEvent.Description = firstESpaceOccurrence.PublicHtmlNotes;
                        changed = true;
                    }

                }

                var syncedRockOccurrences = new List<EventItemOccurrence>(); ;
                var rockOccurrencesWithAttributeChanges = new List<EventItemOccurrence>();

                // Update each occurrence
                foreach ( var eSpaceOccurrence in eSpaceEventOccurrences )
                {
                    var rockOccurrence = SyncOccurrence( eSpaceEvent, eSpaceOccurrence, rockEvent, campusLocation, contactPerson, occurrenceApprovedAttributeKey, out var occurrenceChanged, out var occurrenceAttributeChanged );
                    changed = changed || occurrenceChanged;
                    syncedRockOccurrences.Add( rockOccurrence );
                    if ( occurrenceAttributeChanged )
                    {
                        rockOccurrencesWithAttributeChanges.Add( rockOccurrence );
                    }
                }

                // Remove any desynced occurrences
                var removedOccurrences = rockEvent.EventItemOccurrences.Except( syncedRockOccurrences ).ToList();
                foreach ( var occurrence in removedOccurrences )
                {
                    rockEvent.EventItemOccurrences.Remove( occurrence );
                    eventItemOccurrenceService.Delete( occurrence );
                    changed = true;
                }

                // If anything was updated, save it
                if ( changed )
                {
                    rockContext.SaveChanges();
                }

                // If any occurrences had attributes modified, save them
                foreach ( var rockOccurrence in rockOccurrencesWithAttributeChanges )
                {
                    rockOccurrence.SaveAttributeValues();
                }

            }
        }

        public static EventItemOccurrence SyncOccurrence(
            ESpace.Event eSpaceEvent,
            ESpace.Occurrence eSpaceOccurrence,
            EventItem rockEvent,
            CampusCache campus,
            Person contactPerson,
            string occurrenceApprovedAttributeKey,
            out bool changed,
            out bool attributeChanged
        )
        {
            attributeChanged = false;

            // Get or create the linked Rock occurrence
            var rockOccurrence = rockEvent.EventItemOccurrences.GetOrCreateByForeignId( ForeignKey_eSpaceOccurrenceId, eSpaceOccurrence.OccurrenceId.Value, out changed );

            // Update the campus
            var CampusId = ( eSpaceEvent.IsOffSite ?? false ) ? null : campus?.Id;
            if ( rockOccurrence.CampusId != CampusId )
            {
                rockOccurrence.CampusId = CampusId;
                changed = true;
            }

            // Update the linked Contact Person
            if ( rockOccurrence.ContactPersonAliasId != contactPerson?.PrimaryAliasId )
            {
                rockOccurrence.ContactPersonAliasId = contactPerson?.PrimaryAliasId;
                changed = true;
            }

            // Get the contact data
            var eventContact = eSpaceEvent.Contacts.FirstOrDefault();

            // Update the Contact Email
            var eventContactEmail = eventContact?.Email ?? "";
            if ( rockOccurrence.ContactEmail != eventContactEmail )
            {
                rockOccurrence.ContactEmail = eventContactEmail;
                changed = true;
            }

            // Update the Contact Phone
            var eventContactPhone = PhoneNumber.FormattedNumber( null, eventContact?.Phone, false );
            if ( rockOccurrence.ContactPhone != eventContactPhone )
            {
                rockOccurrence.ContactPhone = eventContactPhone;
                changed = true;
            }

            // Update the event location
            var eventLocation = eSpaceEvent.OffsiteLocation ?? "";
            if ( rockOccurrence.Location != eventLocation )
            {
                rockOccurrence.Location = eventLocation;
                changed = true;
            }

            // Sync the schedule
            SyncSchedule( eSpaceOccurrence, rockOccurrence, out var scheduleChanged );
            changed = changed || scheduleChanged;

            // Check the approved attribute
            if ( !string.IsNullOrEmpty( occurrenceApprovedAttributeKey ) )
            {
                rockOccurrence.LoadAttributes();

                var eSpaceApprovedValue = eSpaceOccurrence.OccurrenceStatus == ESpaceStatus_Approved ? "Approved" : "";
                var rockApprovedValue = rockOccurrence.GetAttributeValue( occurrenceApprovedAttributeKey );
                if ( rockApprovedValue != eSpaceApprovedValue )
                {
                    rockOccurrence.SetAttributeValue( occurrenceApprovedAttributeKey, eSpaceApprovedValue );
                    attributeChanged = true;
                }
            }

            return rockOccurrence;

        }


        public static Schedule SyncSchedule(
            ESpace.Occurrence eSpaceOccurrence,
            EventItemOccurrence rockOccurrence,
            out bool changed
        )
        {
            changed = false;

            if ( rockOccurrence.Schedule == null )
            {
                rockOccurrence.Schedule = new Schedule
                {
                    ForeignKey = ForeignKey_eSpaceOccurrenceId,
                    ForeignId = eSpaceOccurrence.OccurrenceId.Value
                };
                changed = true;
            }

            var schedule = rockOccurrence.Schedule;


            if ( schedule.EffectiveStartDate != eSpaceOccurrence.EventStart )
            {
                schedule.EffectiveStartDate = eSpaceOccurrence.EventStart;
                changed = true;
            }

            if ( schedule.EffectiveEndDate != eSpaceOccurrence.EventEnd )
            {
                schedule.EffectiveEndDate = eSpaceOccurrence.EventEnd;
                changed = true;
            }

            var scheduleName = eSpaceOccurrence.EventName.Truncate( 50 );
            if ( schedule.Name != scheduleName )
            {
                schedule.Name = scheduleName;
                changed = true;
            }

            var iCalEvent = SyncICalEvent( eSpaceOccurrence, schedule, out var iCalChanged );
            if ( iCalChanged )
            {

                // Serialize the event
                var calendar = new DDay.iCal.iCalendar();
                calendar.Events.Add( iCalEvent );
                var serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer( calendar );
                schedule.iCalendarContent = serializer.SerializeToString( calendar );
                changed = true;

            }

            return schedule;

        }

        public static DDay.iCal.Event SyncICalEvent(
            ESpace.Occurrence eSpaceOccurrence,
            Schedule rockSchedule,
            out bool changed
        )
        {
            changed = false;

            var iCalEvent = rockSchedule.GetCalendarEvent();
            if ( iCalEvent == null )
            {
                iCalEvent = new DDay.iCal.Event();
                changed = true;
            }

            // Check for any recurrence dates
            if ( iCalEvent.RecurrenceDates.Any() )
            {
                iCalEvent.RecurrenceDates = null;
                changed = true;
            }

            // Check for any exception dates
            if ( iCalEvent.ExceptionDates.Any() )
            {
                iCalEvent.ExceptionDates = null;
                changed = true;
            }

            // Check for any recurrence rules
            if ( iCalEvent.RecurrenceRules.Any() )
            {
                iCalEvent.RecurrenceRules = null;
                changed = true;
            }

            // Check for any exception rules
            if ( iCalEvent.ExceptionRules.Any() )
            {
                iCalEvent.ExceptionRules = null;
                changed = true;
            }

            // Update the start date
            if ( eSpaceOccurrence.EventStart.HasValue )
            {
                var startICalDate = new DDay.iCal.iCalDateTime( eSpaceOccurrence.EventStart.Value );
                if ( iCalEvent.Start != startICalDate )
                {
                    iCalEvent.Start = startICalDate;
                    changed = true;
                }

            }

            // Update the end date
            if ( eSpaceOccurrence.EventEnd.HasValue )
            {
                var endICalDate = new DDay.iCal.iCalDateTime( eSpaceOccurrence.EventEnd.Value );
                if ( iCalEvent.End != endICalDate )
                {
                    iCalEvent.End = endICalDate;
                    changed = true;
                }

            }

            // Update IsAllDay
            var isAllDay = eSpaceOccurrence.IsAllDay ?? false;
            if ( iCalEvent.IsAllDay != isAllDay )
            {
                iCalEvent.IsAllDay = isAllDay;
                changed = true;
            }

            return iCalEvent;
        }
    }
}
