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
using org.newpointe.eSpace.Utility;

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

            var approvalAttributeGuid = dataMap.GetString( AttributeKey_ApprovedAttribute ).AsGuid();
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

            // Get all future events
            var eSpaceEvents = await client.GetEvents( new GetEventsOptions { StartDate = DateTime.Now } );

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
                    // Use the first item as the main event - Note that some properties
                    // here are actually part of the schedule, not the event
                    var eSpaceEvent = eSpaceEventGroup.FirstOrDefault();

                    // Skip draft events
                    if ( eSpaceEvent.Status == "Draft" ) continue;

                    // Update the job status
                    context.UpdateLastStatusMessage( $@"Syncing event {eventSyncedCount} of {eventTotalCount} ({Math.Round( (double) eventSyncedCount / eventTotalCount * 100, 0 )}%, {eventErrorCount} events with errors)" );

                    // Sync the event
                    await SyncHelper.SyncEvent(
                        client,
                        eSpaceEvent,
                        new GetEventOccurrencesOptions
                        {
                            StartDate = DateTime.Now
                        },
                        calendar,
                        publicCalendar,
                        null,
                        approvalAttribute?.Key
                    );


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

    }

}
