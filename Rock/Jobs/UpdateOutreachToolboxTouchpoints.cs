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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Communication;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Enums.Core;
using Rock.Enums.Engagement;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Updates touchpoints for people using the outreach toolbox and sends any required notifications.
    /// </summary>
    [DisplayName( "Update Outreach Toolbox Touchpoints" )]
    [Description( "Updates touchpoints for people using the outreach toolbox and sends any required notifications." )]

    #region Job Attributes

    [IntegerField( "Morning Notification Hour",
        Description = "The hour during which morning touchpoint notifications will be sent.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Key = AttributeKey.MorningNotificationHour,
        Order = 0 )]

    [IntegerField( "Afternoon Notification Hour",
        Description = "The hour during which afternoon touchpoint notifications will be sent.",
        IsRequired = false,
        DefaultIntegerValue = 13,
        Key = AttributeKey.AfternoonNotificationHour,
        Order = 1 )]

    [IntegerField( "Evening Notification Hour",
        Description = "The hour during which evening touchpoint notifications will be sent.",
        IsRequired = false,
        DefaultIntegerValue = 18,
        Key = AttributeKey.EveningNotificationHour,
        Order = 2 )]

    [IntegerField( "Relationship Pulse Interval in Days",
        Description = "The number of days between relationship pulse touchpoints. These are used to ask the individual if there have been any changes in their relationship.",
        IsRequired = false,
        DefaultIntegerValue = 120,
        Key = AttributeKey.RelationshipPulseIntervalInDays,
        Order = 3 )]

    [IntegerField( "Maximum Active Pulse Touchpoints",
        Description = "The maximum number of active relationship pulse touchpoints that can exist for a person at any given time.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Key = AttributeKey.MaximumActivePulseTouchpoints,
        Order = 4 )]

    [SiteField( "Mobile Application",
        Description = "The mobile application to use when sending push notifications. If not set, then the first active application will be used.",
        SiteTypes = SiteTypeFlags.Mobile,
        IsRequired = false,
        Key = AttributeKey.MobileApplication,
        Order = 5 )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 5,
        Key = AttributeKey.CommandTimeout,
        Order = 6 )]

    #endregion Job Attributes

    public class UpdateOutreachToolboxTouchpoints : RockJob
    {
        #region Constants

        /// <summary>
        /// The daily notification messages that are randomly selected
        /// when sending the push notification.
        /// </summary>
        private static readonly string[] DailyNotificationMessages = new[]
        {
            "on your connection list today. Take a moment to reach out.",
            "ready for a check in today. Small moments matter.",
            "on today’s lineup. One message can make a difference.",
            "waiting for a touchpoint today.",
            "on your list today. Keep the connection going."
        };

        /// <summary>
        /// The number of days inactive that a reminder notification will be
        /// sent to an individual. We only notify for up to a year, but we
        /// include additional backoff values in case there is a desire to
        /// increase the frequency by a configuration value.
        /// </summary>
        private static readonly int[] InactiveNotificationBackoffInterval = new[]
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384,
        };

        #endregion

        #region Fields

        /// <summary>
        /// The random number generator used by the job.
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        /// The size of each batch when processing people.
        /// </summary>
        private readonly int _batchSize = 100;

        #endregion

        #region Keys

        private static class AttributeKey
        {
            public const string MorningNotificationHour = "MorningNotificationHour";
            public const string AfternoonNotificationHour = "AfternoonNotificationHour";
            public const string EveningNotificationHour = "EveningNotificationHour";
            public const string RelationshipPulseIntervalInDays = "RelationshipPulseIntervalInDays";
            public const string MaximumActivePulseTouchpoints = "MaximumActivePulseTouchpoints";
            public const string MobileApplication = "MobileApplication";
            public const string CommandTimeout = "CommandTimeout";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute()
        {
            var morningHour = GetAttributeValue( AttributeKey.MorningNotificationHour ).AsInteger();
            var afternoonHour = GetAttributeValue( AttributeKey.AfternoonNotificationHour ).AsInteger();
            var eveningHour = GetAttributeValue( AttributeKey.EveningNotificationHour ).AsInteger();
            var runContext = new RunContext( RockDateTime.Now );
            var lastProcessed = GetLastProcessed();
            var processedAnything = false;

            // Check if we should process the morning run.
            if ( runContext.ProcessingDateTime.Hour >= morningHour )
            {
                if ( !lastProcessed.LastMorningRun.HasValue || lastProcessed.LastMorningRun.Value.Date < runContext.ProcessingDateTime.Date )
                {
                    ExecuteForTimeOfDay( runContext, OutreachNotificationTimeOfDay.Morning );
                    lastProcessed.LastMorningRun = runContext.ProcessingDateTime;
                    processedAnything = true;
                }
            }

            // Check if we should process the afternoon run.
            if ( runContext.ProcessingDateTime.Hour >= afternoonHour )
            {
                if ( !lastProcessed.LastAfternoonRun.HasValue || lastProcessed.LastAfternoonRun.Value.Date < runContext.ProcessingDateTime.Date )
                {
                    ExecuteForTimeOfDay( runContext, OutreachNotificationTimeOfDay.Afternoon );
                    lastProcessed.LastAfternoonRun = runContext.ProcessingDateTime;
                    processedAnything = true;
                }
            }

            // Check if we should process the evening run.
            if ( runContext.ProcessingDateTime.Hour >= eveningHour )
            {
                if ( !lastProcessed.LastEveningRun.HasValue || lastProcessed.LastEveningRun.Value.Date < runContext.ProcessingDateTime.Date )
                {
                    ExecuteForTimeOfDay( runContext, OutreachNotificationTimeOfDay.Evening );
                    lastProcessed.LastEveningRun = runContext.ProcessingDateTime;
                    processedAnything = true;
                }
            }

            if ( !processedAnything )
            {
                Result = "<i class='ti ti-circle-filled text-success'></i> Nothing to process.";
                return;
            }

            // Save the last processing time before doing anything else so that
            // an un-caught exception does not cause us to process everything
            // again.
            SaveLastProcessed( lastProcessed );

            SendNotifications( runContext );

            if ( runContext.Errors.Count > 0 )
            {
                runContext.Warning( "Some steps have errors. See exception log for details." );
            }

            Result = "Summary\n\n" + string.Join( "\n", runContext.Messages );

            if ( runContext.Errors.Count > 0 )
            {
                var exception = new AggregateException( runContext.Errors );

                throw new RockJobWarningException( $"{GetType().Name.SplitCase()} job completed with errors.", exception );
            }
        }

        /// <summary>
        /// Gets the last processed dates from the metadata. If no data is
        /// found then a new object is created.
        /// </summary>
        /// <returns>An instance of <see cref="LastProcessed"/>.</returns>
        private LastProcessed GetLastProcessed()
        {
            using ( var rockContext = CreateRockContext() )
            {
                var json = ServiceJob.GetMetadataValue( "lastProcessed", rockContext );

                return json?.FromJsonOrNull<LastProcessed>() ?? new LastProcessed();
            }
        }

        /// <summary>
        /// Saves the last processed information back to metadata.
        /// </summary>
        /// <param name="lastProcessed">The object that contains the last process dates.</param>
        private void SaveLastProcessed( LastProcessed lastProcessed )
        {
            using ( var rockContext = CreateRockContext() )
            {
                ServiceJob.SaveMetadataValue( "lastProcessed", lastProcessed.ToJson(), rockContext );
            }
        }

        /// <summary>
        /// Executes all the job logic for the specified time of day period.
        /// </summary>
        /// <param name="runContext">The job run context information.</param>
        /// <param name="timeOfDay">The time of day period to process.</param>
        private void ExecuteForTimeOfDay( RunContext runContext, OutreachNotificationTimeOfDay timeOfDay )
        {
            UpdateLastStatusMessage( $"Processing {timeOfDay.ToString().ToLower()} prayer touchpoints..." );

            using ( var rockContext = CreateRockContext() )
            {
                ProcessGeneralTouchpoints( rockContext, runContext, timeOfDay, TouchpointType.Prayer );
            }

            UpdateLastStatusMessage( $"Processing {timeOfDay.ToString().ToLower()} connection touchpoints..." );

            using ( var rockContext = CreateRockContext() )
            {
                ProcessGeneralTouchpoints( rockContext, runContext, timeOfDay, TouchpointType.Connection );
            }

            // These are only processed in the morning.
            if ( timeOfDay == OutreachNotificationTimeOfDay.Morning )
            {
                UpdateLastStatusMessage( $"Processing annual touchpoints..." );

                using ( var rockContext = CreateRockContext() )
                {
                    ProcessAnnualTouchpoints( rockContext, runContext );
                }

                UpdateLastStatusMessage( $"Processing reminder touchpoints..." );

                using ( var rockContext = CreateRockContext() )
                {
                    ProcessReminderTouchpoints( rockContext, runContext );
                }

                UpdateLastStatusMessage( $"Processing pulse touchpoints..." );

                using ( var rockContext = CreateRockContext() )
                {
                    ProcessPulseTouchpoints( rockContext, runContext );
                }
            }

            if ( runContext.InactivePeople.Any() )
            {
                UpdateLastStatusMessage( $"Processing inactive people..." );

                using ( var rockContext = CreateRockContext() )
                {
                    ProcessInactivePeople( runContext, rockContext );
                }
            }
        }

        /// <summary>
        /// Processes people by handling the loading of contacts for each person
        /// and then calling the <paramref name="processor"/>. Any errors are
        /// caught and recorded.
        /// </summary>
        /// <param name="rockContext">The context to use when reading from the database.</param>
        /// <param name="runContext">The job run context information.</param>
        /// <param name="people">The people to process.</param>
        /// <param name="processor">The function to call for each person.</param>
        private void ProcessPeople( RockContext rockContext, RunContext runContext, List<Person> people, Action<Person, List<Contact>> processor )
        {
            var personIds = people.Select( p => p.Id ).ToList();

            var contacts = new ContactService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c => personIds.Contains( c.OwnerPersonAlias.PersonId ) )
                .GroupBy( c => c.OwnerPersonAlias.PersonId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            foreach ( var person in people )
            {
                if ( !contacts.TryGetValue( person.Id, out var personContacts ) )
                {
                    continue;
                }

                try
                {
                    processor( person, personContacts );
                }
                catch ( Exception ex )
                {
                    runContext.Errors.Add( new Exception( $"Error processing person {person.FullName} (#{person.Id}).", ex ) );
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="RockContext"/> that is properly configured
        /// for use on this instance.
        /// </summary>
        /// <returns>A new instance of <see cref="RockContext"/>.</returns>
        private RockContext CreateRockContext()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            rockContext.Database.SetCommandTimeout( GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300 );

            return rockContext;
        }

        #endregion

        #region General Touchpoint Processing (Prayer/Connection)

        /// <summary>
        /// Processes general (prayer and connection) touchpoints for this run.
        /// </summary>
        /// <param name="rockContext">The context to use when reading data from the database.</param>
        /// <param name="runContext">The job run context information.</param>
        /// <param name="timeOfDay">The time period of the day being processed.</param>
        /// <param name="touchpointType">The type of touchpoint being processed.</param>
        private void ProcessGeneralTouchpoints( RockContext rockContext, RunContext runContext, OutreachNotificationTimeOfDay timeOfDay, TouchpointType touchpointType )
        {
            var peopleToProcess = GetPeopleToProcessForGeneralTouchpoints( rockContext, runContext.ProcessingDateTime, timeOfDay, touchpointType );
            var processingContext = new GeneralProcessingContext( runContext, touchpointType, rockContext );
            var touchpointCount = 0;

            foreach ( var peopleChunk in peopleToProcess.Chunk( _batchSize ) )
            {
                ProcessPeople( rockContext, runContext, peopleChunk.ToList(), ( person, personContacts ) =>
                {
                    touchpointCount += ProcessPersonForGeneralTouchpoints( person, personContacts, processingContext );
                } );
            }

            runContext.Success( $"{touchpointCount:N0} {timeOfDay} {touchpointType} {"Touchpoint".PluralizeIf( touchpointCount != 1 )} Created For {peopleToProcess.Count:N0} People" );
        }

        /// <summary>
        /// Gets the set of people that need to be processed for general
        /// touchpoints for this run.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="touchpointDate">The date and time that will be used for processing touchpoints.</param>
        /// <param name="timeOfDay">The time period of the day being processed.</param>
        /// <param name="touchpointType">The type of touchpoints being processed.</param>
        /// <returns>The set of people that should be processed.</returns>
        private List<Person> GetPeopleToProcessForGeneralTouchpoints( RockContext rockContext, DateTime touchpointDate, OutreachNotificationTimeOfDay timeOfDay, TouchpointType touchpointType )
        {
            var date = touchpointDate.Date;

            // Touchpoints that were created on or after today. This is faster
            // then checking for touchpoints created "today" because it can use
            // an index on ScheduledDateTime.
            var touchpointsQry = new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.ScheduledDateTime >= date
                    && t.Type == touchpointType );

            var dayOfWeekFlag = touchpointDate.DayOfWeek.AsFlags();
            var contactsQry = new ContactService( rockContext ).Queryable();

            // Include people:
            // - Who have enabled touchpoint generation
            // - Whose notification day(s) includes today
            // - Whose notification time of day matches the current run
            // - Who have at least one contact
            // - Who do not have touchpoints created today
            return new PersonService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( p => p.OutreachTouchpointGenerationEnabled
                    && ( p.OutreachTouchpointSchedule & dayOfWeekFlag ) != 0
                    && p.OutreachNotificationTimeOfDay == timeOfDay
                    && contactsQry.Any( c => c.OwnerPersonAlias.PersonId == p.Id )
                    && !touchpointsQry.Any( t => t.Contact.OwnerPersonAlias.PersonId == p.Id ) )
                .ToList();
        }

        /// <summary>
        /// Processes a single person for general touchpoint creation, which
        /// is the prayer and connection types.
        /// </summary>
        /// <param name="person">The person being processed.</param>
        /// <param name="contacts">The contacts for this person.</param>
        /// <param name="processingContext">The processing context for this process run.</param>
        /// <returns>The number of touchpoints that were created.</returns>
        private int ProcessPersonForGeneralTouchpoints( Person person, List<Contact> contacts, GeneralProcessingContext processingContext )
        {
            var numberOfTouchpointDays = person.OutreachTouchpointSchedule.AsDayOfWeekList().Count;
            var maxCount = ContactTouchpointService.GetDailyTouchpointCount( contacts, processingContext.Type, numberOfTouchpointDays );

            // This helps smooth out cases where we end up with 0-touchpoint days
            // by alternating between rounding up and rounding down on fractional
            // counts.
            var count = ( ( ( int ) ( processingContext.Run.ProcessingDateTime - DateTime.MinValue ).TotalDays ) & 0x01 ) == 0x01
                ? ( int ) Math.Ceiling( maxCount )
                : ( int ) Math.Floor( maxCount );

            if ( processingContext.ActiveTouchpointCounts.TryGetValue( person.Id, out var existingCount ) )
            {
                // Special case. If they are full, then that means they haven't
                // done anything with their existing touchpoints. In that case,
                // drop them in a bucket to receive a reminder to catch up.
                if ( existingCount >= count )
                {
                    processingContext.Run.InactivePeople.Add( person );

                    return 0;
                }

                count -= existingCount;
            }

            if ( count <= 0 )
            {
                return 0;
            }

            var newTouchpoints = GetNextGeneralTouchpoints( contacts, processingContext, count );

            if ( newTouchpoints.Count > 0 )
            {
                using ( var saveRockContext = CreateRockContext() )
                {
                    new ContactTouchpointService( saveRockContext ).AddRange( newTouchpoints );

                    saveRockContext.SaveChanges();
                }

                foreach ( var touchpoint in newTouchpoints )
                {
                    var contact = contacts.First( c => c.Id == touchpoint.ContactId );

                    processingContext.Run.Notifications.AddContact( person.Id, contact );
                }
            }

            return newTouchpoints.Count;
        }

        /// <summary>
        /// Gets the new set of general touchpoints (prayer and connection) that
        /// need to be created for the set of contacts.
        /// </summary>
        /// <param name="contacts">The set of contacts that are being processed for a person.</param>
        /// <param name="processingContext">The context for the current processing run.</param>
        /// <param name="count">The number of touchpoints that should be created.</param>
        /// <returns>A set of touchpoints that need to be saved to the database.</returns>
        private ICollection<ContactTouchpoint> GetNextGeneralTouchpoints( List<Contact> contacts, GeneralProcessingContext processingContext, int count )
        {
            var contactsNeedingTouchpoints = contacts
                .Where( c => c.PrayerCadence != OutreachCadence.Paused )
                .Select( c =>
                {
                    DateTime? lastTouchpointDate = null;

                    if ( processingContext.LatestTouchpoints.TryGetValue( c.Id, out var touchpointDate ) )
                    {
                        // Trim off the time portion. Otherwise we might end up
                        // ignoring some contacts based on small clock drifts.
                        lastTouchpointDate = touchpointDate.Date;
                    }

                    return new
                    {
                        Contact = c,
                        LastContactDate = lastTouchpointDate
                    };
                } )
                // Exclude anything that has a recently completed touchpoint, meaning
                // completed within the expected cadence.
                .Where( ct => !ct.LastContactDate.HasValue || !ContactTouchpointService.HasRecentTouchpoint( ct.Contact.PrayerCadence, ct.LastContactDate.Value, processingContext.Run.ProcessingDateTime ) )
                // Exclude anything that has an incomplete touchpoint already scheduled.
                .Where( ct => !processingContext.ContactsWithActiveTouchpoints.Contains( ct.Contact.Id ) )
                // Of the contacts that need touchpoints, find the next scheduled touchpoints.
                .OrderByDescending( ct => !ct.LastContactDate.HasValue )
                // Add a slight randomness to the LastContactDate. This helps
                // smooth out touchpoints so the same person is less likely to
                // end up being picked first, and thus getting more touchpoints.
                .ThenBy( ct => ct.LastContactDate.HasValue ? ct.LastContactDate.Value.AddDays( _random.Next( -1, 2 ) ) : ( DateTime? ) null )
                .ThenBy( ct => ct.Contact.Id )
                .Select( ct => ct.Contact )
                .Take( count );

            return contactsNeedingTouchpoints
                .Select( contact => new ContactTouchpoint
                {
                    ContactId = contact.Id,
                    Type = processingContext.Type,
                    ScheduledDateTime = processingContext.Run.ProcessingDateTime,
                } )
                .ToList();
        }

        #endregion

        #region Annual Touchpoint Processing

        /// <summary>
        /// Processes all annual touchpoints that need to be created for today.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="runContext">The current job run context information.</param>
        private void ProcessAnnualTouchpoints( RockContext rockContext, RunContext runContext )
        {
            var peopleToProcess = GetPeopleToProcessForAnnualTouchpoints( rockContext, runContext.ProcessingDateTime );
            var processingContext = new AnnualProcessingContext( runContext, rockContext );
            var touchpointCount = 0;

            foreach ( var peopleChunk in peopleToProcess.Chunk( _batchSize ) )
            {
                ProcessPeople( rockContext, runContext, peopleChunk.ToList(), ( person, personContacts ) =>
                {
                    touchpointCount += ProcessPersonForAnnualTouchpoints( person, personContacts, processingContext );
                } );
            }

            runContext.Success( $"{touchpointCount:N0} Annual {"Touchpoint".PluralizeIf( touchpointCount != 1 )} Created For {peopleToProcess.Count:N0} People" );
        }

        /// <summary>
        /// Gets the set of people that need to be processed for the current
        /// run.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="touchpointDate">The run date that will be used when checking existing touchpoints.</param>
        /// <returns>A set of people that need to be processed.</returns>
        private List<Person> GetPeopleToProcessForAnnualTouchpoints( RockContext rockContext, DateTime touchpointDate )
        {
            var date = touchpointDate.Date;

            // Touchpoints that were created on or after today. This is faster
            // then checking for touchpoints created "today" because it can use
            // an index on ScheduledDateTime.
            var touchpointsQry = new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.ScheduledDateTime >= date )
                .Where( t => t.Type == TouchpointType.Birthday
                    || t.Type == TouchpointType.WeddingAnniversary
                    || t.Type == TouchpointType.BaptismAnniversary
                    || t.Type == TouchpointType.SalvationAnniversary );

            var dayOfWeekFlag = touchpointDate.DayOfWeek.AsFlags();
            var contactsQry = new ContactService( rockContext ).Queryable();

            // Include people:
            // - Who have enabled touchpoint generation
            // - Who have at least one contact
            // - Who do not have touchpoints created today
            return new PersonService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( p => p.OutreachTouchpointGenerationEnabled
                    && contactsQry.Any( c => c.OwnerPersonAlias.PersonId == p.Id )
                    && !touchpointsQry.Any( t => t.Contact.OwnerPersonAlias.PersonId == p.Id ) )
                .ToList();
        }

        /// <summary>
        /// Processes a single person's annual touchpoints that need to be created.
        /// </summary>
        /// <param name="person">The person being processed.</param>
        /// <param name="contacts">The contacts for this person.</param>
        /// <param name="processingContext">The context that contains information for the current process.</param>
        /// <returns>The number of touchpoints that were created.</returns>
        private int ProcessPersonForAnnualTouchpoints( Person person, List<Contact> contacts, AnnualProcessingContext processingContext )
        {
            var newTouchpoints = GetTodaysAnnualTouchpoints( contacts, processingContext );

            var staleDate = processingContext.Run.ProcessingDateTime.AddDays( -14 );
            var staleTouchpoints = processingContext.ActiveTouchpoints
                .SelectMany( kvp => kvp.Value )
                .Where( t => t.ScheduledDateTime < staleDate )
                .ToList();

            if ( newTouchpoints.Count > 0 || staleTouchpoints.Count > 0 )
            {
                using ( var saveRockContext = CreateRockContext() )
                {
                    var service = new ContactTouchpointService( saveRockContext );

                    if ( newTouchpoints.Count > 0 )
                    {
                        service.AddRange( newTouchpoints );
                    }

                    if ( staleTouchpoints.Count > 0 )
                    {
                        // Existing touchpoints exist on a different RockContext, so
                        // we need to re-load them on this context.
                        var staleTouchpointIds = staleTouchpoints.Select( t => t.Id ).ToList();
                        var touchpointsToRemove = service.Queryable()
                            .Where( t => staleTouchpointIds.Contains( t.Id ) );

                        service.DeleteRange( touchpointsToRemove );
                    }

                    saveRockContext.SaveChanges();
                }

                foreach ( var touchpoint in newTouchpoints )
                {
                    // We need the Contact object later for notifications.
                    touchpoint.Contact = contacts.First( c => c.Id == touchpoint.ContactId );

                    processingContext.Run.Notifications.AddAnnualTouchpoint( person.Id, touchpoint );
                }
            }

            return newTouchpoints.Count;
        }

        /// <summary>
        /// Gets the set of touchpoints that need to be created for any annual
        /// anniversary type events.
        /// </summary>
        /// <param name="contacts">The contacts to search through for annual events.</param>
        /// <param name="processingContext">The context that contains information for the current process.</param>
        /// <returns>A set of <see cref="ContactTouchpoint"/> objects that need to be saved.</returns>
        private ICollection<ContactTouchpoint> GetTodaysAnnualTouchpoints( List<Contact> contacts, AnnualProcessingContext processingContext )
        {
            var date = processingContext.Run.ProcessingDateTime.Date;
            var newTouchpoints = new List<ContactTouchpoint>();

            foreach ( var contact in contacts )
            {
                processingContext.ActiveTouchpoints.TryGetValue( contact.Id, out var activeTouchpoints );

                if ( contact.BirthDay == date.Day && contact.BirthMonth == date.Month )
                {
                    if ( activeTouchpoints == null || !activeTouchpoints.Any( t => t.Type == TouchpointType.Birthday ) )
                    {
                        newTouchpoints.Add( new ContactTouchpoint
                        {
                            ContactId = contact.Id,
                            Type = TouchpointType.Birthday,
                            ScheduledDateTime = processingContext.Run.ProcessingDateTime,
                        } );
                    }
                }

                if ( contact.WeddingDay == date.Day && contact.WeddingMonth == date.Month )
                {
                    if ( activeTouchpoints == null || !activeTouchpoints.Any( t => t.Type == TouchpointType.WeddingAnniversary ) )
                    {
                        newTouchpoints.Add( new ContactTouchpoint
                        {
                            ContactId = contact.Id,
                            Type = TouchpointType.WeddingAnniversary,
                            ScheduledDateTime = processingContext.Run.ProcessingDateTime,
                        } );
                    }
                }

                if ( contact.BaptismDay == date.Day && contact.BaptismMonth == date.Month )
                {
                    if ( activeTouchpoints == null || !activeTouchpoints.Any( t => t.Type == TouchpointType.BaptismAnniversary ) )
                    {
                        newTouchpoints.Add( new ContactTouchpoint
                        {
                            ContactId = contact.Id,
                            Type = TouchpointType.BaptismAnniversary,
                            ScheduledDateTime = processingContext.Run.ProcessingDateTime,
                        } );
                    }
                }

                if ( contact.SalvationDay == date.Day && contact.SalvationMonth == date.Month )
                {
                    if ( activeTouchpoints == null || !activeTouchpoints.Any( t => t.Type == TouchpointType.SalvationAnniversary ) )
                    {
                        newTouchpoints.Add( new ContactTouchpoint
                        {
                            ContactId = contact.Id,
                            Type = TouchpointType.SalvationAnniversary,
                            ScheduledDateTime = processingContext.Run.ProcessingDateTime,
                        } );
                    }
                }
            }

            return newTouchpoints;
        }

        #endregion

        #region Reminder Touchpoint Processing

        /// <summary>
        /// Processes all reminder touchpoints that were previously created by
        /// the individual. These are simply added to the notification queue.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="runContext">The current job run context information.</param>
        private void ProcessReminderTouchpoints( RockContext rockContext, RunContext runContext )
        {
            var today = runContext.ProcessingDateTime.Date;
            var tomorrow = runContext.ProcessingDateTime.Date.AddDays( 1 );
            var count = 0;

            // Touchpoints that were created on or after today but before
            // tomorrow. This is faster then checking for touchpoints created
            // "today" because it can use an index on ScheduledDateTime.
            var contactsQry = new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.ScheduledDateTime >= today
                    && t.ScheduledDateTime < tomorrow
                    && t.Type == TouchpointType.Reminder )
                .Select( t => new
                {
                    t.Contact.OwnerPersonAlias.PersonId,
                    t.Contact
                } );

            foreach ( var result in contactsQry )
            {
                runContext.Notifications.AddContact( result.PersonId, result.Contact );
                count++;
            }

            runContext.Success( $"{count:N0} Reminder {"Touchpoint".PluralizeIf( count != 1 )} Processed" );
        }

        #endregion

        #region Pulse Touchpoint Processing

        /// <summary>
        /// Processes all pulse touchpoints that need to be created for today.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="runContext">The current job run context information.</param>
        private void ProcessPulseTouchpoints( RockContext rockContext, RunContext runContext )
        {
            var contactsToProcess = GetContactsToProcessForPulseTouchpoints( rockContext, runContext.ProcessingDateTime );
            var existingPulseCounts = GetExistingPulseTouchpointCounts( rockContext );
            var maxActivePulseTouchpoints = GetAttributeValue( AttributeKey.MaximumActivePulseTouchpoints ).AsInteger();
            int touchpointsCreated = 0;

            try
            {
                using ( var saveRockContext = CreateRockContext() )
                {
                    var service = new ContactTouchpointService( saveRockContext );
                    var touchpoints = contactsToProcess
                        .Where( c =>
                        {
                            if ( existingPulseCounts.TryGetValue( c.PersonId, out var activeCount ) )
                            {
                                if ( activeCount >= maxActivePulseTouchpoints )
                                {
                                    return false;
                                }

                                existingPulseCounts[c.PersonId] = activeCount + 1;
                            }
                            else
                            {
                                existingPulseCounts[c.PersonId] = 1;
                            }

                            return true;
                        } )
                        .Select( c => new ContactTouchpoint
                        {
                            ContactId = c.Contact.Id,
                            Type = TouchpointType.Pulse,
                            ScheduledDateTime = runContext.ProcessingDateTime,
                        } )
                        .ToList();

                    service.AddRange( touchpoints );
                    touchpointsCreated = touchpoints.Count;

                    saveRockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                runContext.Errors.Add( new Exception( "Error creating pulse touchpoints.", ex ) );
                return;
            }

            foreach ( var result in contactsToProcess )
            {
                runContext.Notifications.AddContact( result.PersonId, result.Contact );
            }

            runContext.Success( $"{touchpointsCreated:N0} Pulse {"Touchpoint".PluralizeIf( touchpointsCreated != 1 )} Created" );
        }

        /// <summary>
        /// Gets the set of contacts that need to be processed for the current
        /// run.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="touchpointDate">The run date that will be used when checking existing touchpoints.</param>
        /// <returns>A set of people that need to be processed.</returns>
        private List<(int PersonId, Contact Contact)> GetContactsToProcessForPulseTouchpoints( RockContext rockContext, DateTime touchpointDate )
        {
            var targetLastPulseDate = touchpointDate.AddDays( -GetAttributeValue( AttributeKey.RelationshipPulseIntervalInDays ).AsInteger() );
            var nearTargetLastPulseDate = targetLastPulseDate.AddDays( 7 );

            var activeTouchpointsQry = new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.Type == TouchpointType.Pulse
                    && !t.CompletedDateTime.HasValue );

            // Pre-loading the touchpoints details all at once is much
            // faster than individual queries. With 5,000 contacts, this query
            // only adds 5MB to the heap.
            var latestTouchpoints = new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.Type == TouchpointType.Pulse
                    && t.CompletedDateTime.HasValue )
                .GroupBy( t => t.ContactId )
                .Select( g => new
                {
                    ContactId = g.Key,
                    LatestCompletedDateTime = g.Max( t => t.CompletedDateTime.Value )
                } )
                .ToDictionary( k => k.ContactId, v => v.LatestCompletedDateTime );

            var dayOfWeekFlag = touchpointDate.DayOfWeek.AsFlags();
            var contactsQry = new ContactService( rockContext ).Queryable();

            // Include contacts:
            // - Who have enabled touchpoint generation
            // - Whose notification day(s) includes today
            // - Who do not have an active pulse touchpoint already
            // - Who have not had a pulse touchpoint created recently
            var contacts = new ContactService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.OwnerPersonAlias.Person.OutreachTouchpointGenerationEnabled
                    && ( c.OwnerPersonAlias.Person.OutreachTouchpointSchedule & dayOfWeekFlag ) != 0
                    && !activeTouchpointsQry.Any( t => t.Contact.OwnerPersonAlias.PersonId == c.OwnerPersonAlias.PersonId ) )
                .Select( c => new
                {
                    c.OwnerPersonAlias.PersonId,
                    Contact = c,
                } )
                .AsEnumerable()
                .Where( c =>
                {
                    // Logic for recent pulses.
                    DateTime lastPulseDate = DateTime.MinValue;

                    if ( latestTouchpoints.TryGetValue( c.Contact.Id, out var latestCompletedDateTime ) )
                    {
                        lastPulseDate = latestCompletedDateTime.Date;
                    }
                    else if ( c.Contact.CreatedDateTime.HasValue )
                    {
                        lastPulseDate = c.Contact.CreatedDateTime.Value.Date;
                    }

                    // If we are getting close to reaching the target date for
                    // a new pulse to be sent, then add +/- up to one week of
                    // randomness so not all pulses go out on the exact same
                    // day when an individual added a number of contacts at
                    // the same time.
                    //
                    // This means a pulse might go out a week early, or a week
                    // late.
                    if ( lastPulseDate != DateTime.MinValue && lastPulseDate <= nearTargetLastPulseDate )
                    {
                        lastPulseDate = lastPulseDate.AddDays( 7 - _random.Next( 14 ) );
                    }

                    return lastPulseDate <= targetLastPulseDate;
                } );

            return contacts
                .Select( c => (c.PersonId, c.Contact) )
                .ToList();
        }

        /// <summary>
        /// Gets the number of existing active pulse touchpoints per person.
        /// </summary>
        /// <param name="rockContext">The context to use when querying the database.</param>
        /// <returns>A dictionary of person identifier keys and active pulse touchpoint count values.</returns>
        private Dictionary<int, int> GetExistingPulseTouchpointCounts( RockContext rockContext )
        {
            return new ContactTouchpointService( rockContext )
                .Queryable()
                .Where( t => t.Type == TouchpointType.Pulse
                    && !t.CompletedDateTime.HasValue )
                .GroupBy( t => t.Contact.OwnerPersonAlias.PersonId )
                .ToDictionary( g => g.Key, g => g.Count() );
        }

        #endregion

        #region Inactive Person Processing

        /// <summary>
        /// Processes inactive people. These are individuals that have not
        /// completed their existing touchpoints and are now blocked from
        /// receiving new touchpoints.
        /// </summary>
        /// <param name="runContext">The context describing the current run.</param>
        /// <param name="rockContext">The context to use when reading from the database.</param>
        private void ProcessInactivePeople( RunContext runContext, RockContext rockContext )
        {
            var today = RockDateTime.Today;
            var personEntityType = EntityTypeCache.Get<Person>( true, rockContext );
            var distinctPeople = runContext.InactivePeople
                .Where( p => p.OutreachEnableDailyNotification
                    && p.OutreachTouchpointGenerationEnabled
                    && !runContext.Notifications.ContainsKey( p.Id ) )
                .DistinctBy( p => p.Id )
                .ToList();

            foreach ( var batch in distinctPeople.Chunk( _batchSize ) )
            {
                var personIds = batch.Select( p => p.Id ).ToList();
                var lastActivityDates = new ContactTouchpointService( rockContext )
                    .Queryable()
                    .Where( ct => personIds.Contains( ct.Contact.OwnerPersonAlias.PersonId )
                        && ( ct.Type == TouchpointType.Prayer || ct.Type == TouchpointType.Connection ) )
                    .Select( ct => new
                    {
                        ct.Contact.OwnerPersonAlias.PersonId,
                        LastDateTime = ct.CompletedDateTime ?? ct.ScheduledDateTime,
                    } )
                    .GroupBy( a => a.PersonId )
                    .ToDictionary( grp => grp.Key, grp => grp.Max( ct => ct.LastDateTime ) );

                foreach ( var person in batch )
                {
                    // This shouldn't happen, but just in case skip the person.
                    if ( !lastActivityDates.TryGetValue( person.Id, out var lastActivityDate ) )
                    {
                        continue;
                    }

                    lastActivityDate = lastActivityDate.Date;

                    // If it has been more than a year, just give up.
                    if ( lastActivityDate < today.AddYears( -1 ) )
                    {
                        continue;
                    }

                    var daysInactive = CalculateDaysInactive( lastActivityDate, today, person.OutreachTouchpointSchedule );

                    if ( InactiveNotificationBackoffInterval.Contains( daysInactive ) )
                    {
                        runContext.Notifications.AddInactiveNotification( person.Id );
                    }
                }
            }

            runContext.Success( $"{distinctPeople.Count:N0} Inactive People Processed" );
        }

        /// <summary>
        /// Calculate the number of days a person has been inactive based on the
        /// last activity date and their scheduled days. Meaning, if it has been
        /// a full two weeks they have been inactive, but they are only scheduled
        /// for Monday, Wednesday and Friday, then this would return 6.
        /// </summary>
        /// <param name="lastActivityDate">The date they were last active.</param>
        /// <param name="today">Today's date.</param>
        /// <param name="scheduledDays">The days of the week they are scheduled to process touchpoints.</param>
        /// <returns>The number of inactive days.</returns>
        private static int CalculateDaysInactive( DateTime lastActivityDate, DateTime today, DaysOfWeekFlags scheduledDays )
        {
            int numberOfDays = 0;

            for ( var date = lastActivityDate; date < today; date = date.AddDays( 1 ) )
            {
                var dayOfWeekFlag = date.DayOfWeek.AsFlags();

                if ( scheduledDays.HasFlag( dayOfWeekFlag ) )
                {
                    numberOfDays++;
                }
            }

            return numberOfDays;
        }

        #endregion

        #region Notification Processing

        /// <summary>
        /// Sends all notifications that have been queued up during the job run.
        /// </summary>
        /// <param name="runContext">The job run context information.</param>
        private void SendNotifications( RunContext runContext )
        {
            var (siteId, notificationPage) = GetNotificationSiteAndPageId();

            if ( !MediumContainer.HasActivePushTransport() )
            {
                runContext.Warning( "No Push Notification Transport Configured; Notifications Not Sent" );
                return;
            }

            if ( !siteId.HasValue )
            {
                runContext.Warning( "Unable To Find Mobile Application; Notifications Not Sent" );
                return;
            }

            UpdateLastStatusMessage( "Sending notifications..." );

            var sentCount = 0;

            foreach ( var batch in runContext.Notifications.Chunk( _batchSize ) )
            {
                using ( var rockContext = CreateRockContext() )
                {
                    var personIds = batch.Select( k => k.Key ).ToList();
                    var personalDeviceRegistrationIds = new PersonalDeviceService( rockContext )
                        .Queryable()
                        .Where( pd => personIds.Contains( pd.PersonAlias.PersonId )
                            && pd.NotificationsEnabled
                            && pd.SiteId == siteId.Value
                            && !string.IsNullOrEmpty( pd.DeviceRegistrationId ) )
                        .GroupBy( pd => pd.PersonAlias.PersonId )
                        .ToDictionary( g => g.Key, g => g.Select( pd => pd.DeviceRegistrationId ).ToList() );

                    var personNotificationStates = new PersonService( rockContext )
                        .Queryable()
                        .Where( p => personIds.Contains( p.Id ) )
                        .Select( p => new
                        {
                            p.Id,
                            p.OutreachEnableDailyNotification,
                            p.OutreachEnableSpecialEventsNotification,
                        } )
                        .ToDictionary( k => k.Id, v => new PersonNotificationState
                        {
                            EnableDailyNotifications = v.OutreachEnableDailyNotification,
                            EnableSpecialEventNotifications = v.OutreachEnableSpecialEventsNotification,
                        } );

                    sentCount += SendNotificationsBatch( runContext, rockContext, batch, personalDeviceRegistrationIds, personNotificationStates, notificationPage );
                }
            }

            runContext.Success( $"{sentCount:N0} Notifications Sent." );
        }

        /// <summary>
        /// Get the site and page identifier to use for sending notifications.
        /// </summary>
        /// <returns>A tuple that contains the site and page identifiers.</returns>
        private (int? SiteId, PageCache Page) GetNotificationSiteAndPageId()
        {
            using ( var rockContext = CreateRockContext() )
            {
                // If they specified the app in the job settings then use that.
                var siteId = GetAttributeValue( AttributeKey.MobileApplication ).AsIntegerOrNull();

                if ( siteId.HasValue )
                {
                    var site = SiteCache.Get( siteId.Value, rockContext );
                    var settings = site?.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
                    var pageId = settings?.OutreachToolboxTouchpointPageId;
                    var page = pageId.HasValue ? PageCache.Get( pageId.Value, rockContext ) : null;

                    if ( site != null )
                    {
                        return (site.Id, page);
                    }
                }

                // Try getting the first active mobile app that has a notification
                // page configured.
                return SiteCache.All( rockContext )
                    .Where( s => s.IsActive
                        && s.SiteType == SiteType.Mobile )
                    .OrderBy( s => s.Id )
                    .Select( s => new
                    {
                        s.Id,
                        PageId = s.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>()?.OutreachToolboxTouchpointPageId
                    } )
                    .Select( s => (s.Id, s.PageId.HasValue ? PageCache.Get( s.PageId.Value, rockContext ) : null ) )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Sends a single batch of notifications.
        /// </summary>
        /// <param name="runContext">The job run context information.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="batch">The batch of person identifier keys and notification data.</param>
        /// <param name="personalDeviceRegistrationIds">The dictionary of person identifiers and personal device registration identifiers.</param>
        /// <param name="personNotificationStates">The dictionary of person identifiers and their notification state.</param>
        /// <param name="notificationPage">The notification page.</param>
        /// <returns>The number of notifications that were sent for this batch.</returns>
        private int SendNotificationsBatch( RunContext runContext, RockContext rockContext, IEnumerable<KeyValuePair<int, TouchpointNotifications>> batch, Dictionary<int, List<string>> personalDeviceRegistrationIds, Dictionary<int, PersonNotificationState> personNotificationStates, PageCache notificationPage )
        {
            var sentCount = 0;

            foreach ( var kvp in batch )
            {
                var personId = kvp.Key;
                var notifications = kvp.Value;

                if ( !personalDeviceRegistrationIds.TryGetValue( personId, out var registrationIds ) )
                {
                    continue;
                }

                if ( !personNotificationStates.TryGetValue( personId, out var notificationState ) )
                {
                    continue;
                }

                try
                {
                    sentCount += SendPersonNotifications( registrationIds, notificationState, notifications, notificationPage );
                }
                catch ( Exception ex )
                {
                    var person = new PersonService( rockContext ).GetNoTracking( personId );

                    runContext.Errors.Add( new Exception( $"Error sending notifications to {person?.FullName} (#{personId}).", ex ) );
                }
            }

            return sentCount;
        }

        /// <summary>
        /// Sends the notifications for a single person to the specified set of
        /// device registration identifiers.
        /// </summary>
        /// <param name="deviceRegistrationIds">The device identifiers that will receive the notifications.</param>
        /// <param name="notificationState">Determines which notifications this person wants to receive.</param>
        /// <param name="notifications">The object containing the information about which notifications to send.</param>
        /// <param name="notificationPage">The notification page.</param>
        /// <returns>The number of notifications that were sent.</returns>
        private int SendPersonNotifications( List<string> deviceRegistrationIds, PersonNotificationState notificationState, TouchpointNotifications notifications, PageCache notificationPage )
        {
            var mergeFields = new Dictionary<string, object>();
            var recipient = RockPushMessageRecipient.CreateAnonymous( string.Join( ",", deviceRegistrationIds ), mergeFields );
            var sentCount = 0;

            if ( notificationState.EnableSpecialEventNotifications )
            {
                foreach ( var touchpoint in notifications.AnnualTouchpoints )
                {
                    SendAnnualNotification( recipient, touchpoint, notificationPage );

                    sentCount += 1;
                }
            }

            if ( notificationState.EnableDailyNotifications && notifications.Contacts.Count > 0 )
            {
                SendDailyNotification( recipient, notifications.Contacts, notificationPage );

                sentCount += 1;
            }

            if ( notifications.SendInactiveNotification )
            {
                SendInactiveNotification( recipient, notificationPage );

                sentCount += 1;
            }

            return sentCount;
        }

        /// <summary>
        /// Sends the push notification for a contact's annual touchpoint, such as a birthday.
        /// </summary>
        /// <param name="recipient">The person who will receive the push notification.</param>
        /// <param name="touchpoint">The touchpoint that describes what to send.</param>
        /// <param name="notificationPage">The page to open when the notification is tapped.</param>
        private void SendAnnualNotification( RockPushMessageRecipient recipient, ContactTouchpoint touchpoint, PageCache notificationPage )
        {
            var pushMessage = GetAnnualNotificationMessage( touchpoint );

            if ( pushMessage == null )
            {
                return;
            }

            if ( notificationPage != null )
            {
                pushMessage.OpenAction = Utility.PushOpenAction.LinkToMobilePage;
                pushMessage.Data = new PushData
                {
                    MobilePageId = notificationPage.Id,
                    MobilePageQueryString = new Dictionary<string, string>
                    {
                        { "TouchpointIdKeys", touchpoint.IdKey }
                    }
                };
            }

            pushMessage.AddRecipient( recipient );
            pushMessage.Send();
        }

        /// <summary>
        /// Sends the push notification for the daily touchpoints that were
        /// created for the set of contacts.
        /// </summary>
        /// <param name="recipient">The person who will receive the push notification.</param>
        /// <param name="contacts">The contacts that had touchpoints created for them.</param>
        /// <param name="notificationPage">The page to open when the notification is tapped.</param>
        private void SendDailyNotification( RockPushMessageRecipient recipient, List<Contact> contacts, PageCache notificationPage )
        {
            string prefix;

            if ( contacts.Count == 1 )
            {
                prefix = $"{contacts[0].FirstName} is";
            }
            else if ( contacts.Count == 2 )
            {
                prefix = $"{contacts[0].FirstName} and {contacts[1].FirstName} are";
            }
            else
            {
                prefix = $"{contacts[0].FirstName}, {contacts[1].FirstName}, and {contacts.Count - 2} others are";
            }

            var messageIndex = _random.Next( 0, DailyNotificationMessages.Length );

            var pushMessage = new RockPushMessage
            {
                Title = "Keep the connection going!",
                Message = $"{prefix} {DailyNotificationMessages[messageIndex]}",
                Data = new PushData()
            };

            if ( notificationPage != null )
            {
                pushMessage.OpenAction = Utility.PushOpenAction.LinkToMobilePage;
                pushMessage.Data = new PushData
                {
                    MobilePageId = notificationPage.Id,
                };
            }

            pushMessage.AddRecipient( recipient );
            pushMessage.Send();
        }

        /// <summary>
        /// Sends the push notification for the people who have become inactive.
        /// </summary>
        /// <param name="recipient">The person who will receive the push notification.</param>
        /// <param name="notificationPage">The page to open when the notification is tapped.</param>
        private void SendInactiveNotification( RockPushMessageRecipient recipient, PageCache notificationPage )
        {
            var pushMessage = new RockPushMessage
            {
                Title = "Don't fall behind!",
                Message = $"You have people that are waiting to connect with you.",
                Data = new PushData()
            };

            if ( notificationPage != null )
            {
                pushMessage.OpenAction = Utility.PushOpenAction.LinkToMobilePage;
                pushMessage.Data = new PushData
                {
                    MobilePageId = notificationPage.Id,
                };
            }

            pushMessage.AddRecipient( recipient );
            pushMessage.Send();
        }

        /// <summary>
        /// Creates a push notification message for a contact's annual milestone,
        /// such as a birthday, wedding anniversary, baptism anniversary, or
        /// salvation anniversary.
        /// </summary>
        /// <param name="touchpoint">The contact touchpoint representing the annual milestone.</param>
        /// <returns>The <see cref="RockPushMessage"/> that should be sent for this touchpoint or <c>null</c> if the touchpoint was not valid.</returns>
        private static RockPushMessage GetAnnualNotificationMessage( ContactTouchpoint touchpoint )
        {
            var pronoun = "their";

            if ( touchpoint.Contact.Gender == Gender.Male )
            {
                pronoun = "his";
            }
            else if ( touchpoint.Contact.Gender == Gender.Female )
            {
                pronoun = "her";
            }

            if ( touchpoint.Type == TouchpointType.Birthday )
            {
                return new RockPushMessage
                {
                    Title = $"🎂Celebrate {touchpoint.Contact.FirstName} Today!",
                    Message = $"It's {touchpoint.Contact.FirstName} {touchpoint.Contact.LastName}'s birthday - a perfect moment to reach out, encourage, and show you care.",
                    Data = new PushData()
                };
            }
            else if ( touchpoint.Type == TouchpointType.WeddingAnniversary )
            {
                return new RockPushMessage
                {
                    Title = $"💍Celebrate {touchpoint.Contact.FirstName} Today!",
                    Message = $"It's {touchpoint.Contact.FirstName}'s wedding anniversary - a wonderful moment to send encouragement and share in {pronoun} joy.",
                    Data = new PushData()
                };
            }
            else if ( touchpoint.Type == TouchpointType.BaptismAnniversary )
            {
                return new RockPushMessage
                {
                    Title = $"💧Celebrate {touchpoint.Contact.FirstName} Today!",
                    Message = $"It's {touchpoint.Contact.FirstName}'s baptism anniversary - a wonderful moment to send encouragement and share in {pronoun} joy.",
                    Data = new PushData()
                };
            }
            else if ( touchpoint.Type == TouchpointType.SalvationAnniversary )
            {
                return new RockPushMessage
                {
                    Title = $"✝Celebrate {touchpoint.Contact.FirstName} Today!",
                    Message = $"It's {touchpoint.Contact.FirstName}'s salvation anniversary - a wonderful moment to send encouragement and share in {pronoun} joy.",
                    Data = new PushData()
                };
            }

            return null;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The context data for the job run.
        /// </summary>
        private class RunContext
        {
            /// <summary>
            /// The date that processing started. We use a single date for all
            /// records since it is possible the job might run past midnight and
            /// we don't want the confusion that might bring.
            /// </summary>
            public DateTime ProcessingDateTime { get; }

            /// <summary>
            /// The dictionary of records that need notifications to be sent.
            /// </summary>
            public TouchpointNotificationsDictionary Notifications { get; } = new TouchpointNotificationsDictionary();

            /// <summary>
            /// The messages to display in the job history.
            /// </summary>
            public List<string> Messages { get; } = new List<string>();

            /// <summary>
            /// The list of people that have not completed a touchpoint
            /// recently. This is determined by trying to create touchpoints
            /// for them but having no remaining capacity to do so because
            /// of existing touchpoints.
            /// </summary>
            public List<Person> InactivePeople { get; } = new List<Person>();

            /// <summary>
            /// Errors that occurred and will be reported after the job finishes.
            /// </summary>
            public List<Exception> Errors { get; } = new List<Exception>();

            /// <summary>
            /// Creates a new instance of <see cref="RunContext"/>.
            /// </summary>
            /// <param name="processingDateTime">The date that processing started.</param>
            public RunContext( DateTime processingDateTime )
            {
                ProcessingDateTime = processingDateTime;
            }

            /// <summary>
            /// Add a successful message to the messages list.
            /// </summary>
            /// <param name="message">The message text.</param>
            public void Success( string message )
            {
                Messages.Add( $"<i class='ti ti-circle-filled text-success'></i> {message}" );
            }

            /// <summary>
            /// Add a warning message to the messages list.
            /// </summary>
            /// <param name="message">The message text.</param>
            public void Warning( string message )
            {
                Messages.Add( $"<i class='ti ti-circle-filled text-warning'></i> {message}" );
            }
        }

        /// <summary>
        /// The processing context used when handling general touchpoints such
        /// as prayer and connection.
        /// </summary>
        private class GeneralProcessingContext
        {
            /// <summary>
            /// The context for the whole job run.
            /// </summary>
            public RunContext Run { get; }

            /// <summary>
            /// The date and time of the latest completed touchpoint for each
            /// contact of the current <see cref="Type"/>.
            /// </summary>
            public Dictionary<int, DateTime> LatestTouchpoints { get; }

            /// <summary>
            /// The identifiers of all contacts with active touchpoints for the
            /// current <see cref="Type"/>.
            /// </summary>
            public HashSet<int> ContactsWithActiveTouchpoints { get; }

            /// <summary>
            /// The dictionary of person identifiers and the number of active
            /// touchpoints they have for <see cref="Type"/>.
            /// </summary>
            public Dictionary<int, int> ActiveTouchpointCounts { get; }

            /// <summary>
            /// The type of touchpoint being processed.
            /// </summary>
            public TouchpointType Type { get; }

            /// <summary>
            /// Creates a new instance of <see cref="GeneralProcessingContext"/>.
            /// </summary>
            /// <param name="run">The run context that provides information about the current processing run.</param>
            /// <param name="type">The type of touchpoint being processed for this context.</param>
            /// <param name="rockContext">The context to use when reading from the database.</param>
            public GeneralProcessingContext( RunContext run, TouchpointType type, RockContext rockContext )
            {
                Run = run;
                Type = type;

                // Pre-loading the touchpoints details all at once is much
                // faster than individual queries. With 5,000 contacts, these two
                // queries only add 5MB to the heap.
                LatestTouchpoints = new ContactTouchpointService( rockContext )
                    .Queryable()
                    .Where( t => t.Type == type
                        && t.CompletedDateTime.HasValue )
                    .GroupBy( t => t.ContactId )
                    .Select( g => new
                    {
                        ContactId = g.Key,
                        LatestCompletedDateTime = g.Max( t => t.CompletedDateTime.Value )
                    } )
                    .ToDictionary( k => k.ContactId, v => v.LatestCompletedDateTime );

                var contactsWithActiveTouchpointsQry = new ContactTouchpointService( rockContext )
                    .Queryable()
                    .Where( t => t.Type == type
                        && !t.CompletedDateTime.HasValue )
                    .Select( t => t.ContactId )
                    .Distinct();

                ContactsWithActiveTouchpoints = new HashSet<int>( contactsWithActiveTouchpointsQry );

                ActiveTouchpointCounts = new ContactTouchpointService( rockContext )
                    .Queryable()
                    .Where( t => t.Type == type
                        && !t.CompletedDateTime.HasValue )
                    .GroupBy( t => t.Contact.OwnerPersonAlias.PersonId )
                    .ToDictionary( g => g.Key, g => g.Count() );
            }
        }

        /// <summary>
        /// The processing context used when handling annual touchpoints.
        /// </summary>
        private class AnnualProcessingContext
        {
            /// <summary>
            /// The context for the whole job run.
            /// </summary>
            public RunContext Run { get; }

            /// <summary>
            /// The active touchpoints for each contact.
            /// </summary>
            public Dictionary<int, List<ContactTouchpoint>> ActiveTouchpoints { get; }

            /// <summary>
            /// Creates a new instance of <see cref="AnnualProcessingContext"/>.
            /// </summary>
            /// <param name="run">The context for the whole job run.</param>
            /// <param name="rockContext">The context to use when reading data from the database.</param>
            public AnnualProcessingContext( RunContext run, RockContext rockContext )
            {
                Run = run;

                // Pre-loading the touchpoints details all at once is much
                // faster than individual queries later on.
                ActiveTouchpoints = new ContactTouchpointService( rockContext )
                    .Queryable()
                    .Where( t => t.Type == TouchpointType.Birthday
                        || t.Type == TouchpointType.WeddingAnniversary
                        || t.Type == TouchpointType.BaptismAnniversary
                        || t.Type == TouchpointType.SalvationAnniversary )
                    .Where( t => !t.CompletedDateTime.HasValue )
                    .GroupBy( t => t.ContactId )
                    .ToDictionary( g => g.Key, g => g.ToList() );
            }
        }

        /// <summary>
        /// The notifications that a person should receive when they are processed.
        /// </summary>
        private class TouchpointNotifications
        {
            /// <summary>
            /// The contacts that will be mentioned in the group notification.
            /// This includes prayer, connection and reminder touchpoints. Sending
            /// these notifications is controlled by
            /// <see cref="Person.OutreachEnableDailyNotification"/>.
            /// </summary>
            public List<Contact> Contacts { get; } = new List<Contact>();

            /// <summary>
            /// The annual touchpoints that will each get their own notification.
            /// Sending these notifications is controlled by
            /// <see cref="Person.OutreachEnableSpecialEventsNotification"/>.
            /// </summary>
            public List<ContactTouchpoint> AnnualTouchpoints { get; } = new List<ContactTouchpoint>();

            /// <summary>
            /// Determines if the "you have been inactive" notification should
            /// be sent for the person.
            /// </summary>
            public bool SendInactiveNotification { get; set; }
        }

        /// <summary>
        /// Special dictionary to hold the notification information for each
        /// person being processed. The key is the person identifier.
        /// </summary>
        private class TouchpointNotificationsDictionary : Dictionary<int, TouchpointNotifications>
        {
            /// <summary>
            /// Gets an existing notifications object for the specified person
            /// or adds it to the dictionary.
            /// </summary>
            /// <param name="personId">The person identifier.</param>
            /// <returns>An instance of <see cref="TouchpointNotifications"/>.</returns>
            private TouchpointNotifications GetOrAddPerson( int personId )
            {
                if ( !TryGetValue( personId, out var notifications ) )
                {
                    notifications = new TouchpointNotifications();
                    this[personId] = notifications;
                }

                return notifications;
            }

            /// <summary>
            /// Adds a contact to the notifications for the person. This is used
            /// for the grouped notifications of prayer, connection and reminder.
            /// </summary>
            /// <param name="personId">The identifier of the person that will receive the notification.</param>
            /// <param name="contact">The contact that had a touchpoing added.</param>
            public void AddContact( int personId, Contact contact )
            {
                var notifications = GetOrAddPerson( personId );

                if ( !notifications.Contacts.Any( c => c.Id == contact.Id ) )
                {
                    notifications.Contacts.Add( contact );
                }
            }

            /// <summary>
            /// Adds an annual touchpoint. This is used to send one-off notifications
            /// for each touchpoint.
            /// </summary>
            /// <param name="personId">The identifier of the person that will receive the notification.</param>
            /// <param name="touchpoint">The touchpoint the person should be notified about.</param>
            public void AddAnnualTouchpoint( int personId, ContactTouchpoint touchpoint )
            {
                var notifications = GetOrAddPerson( personId );

                notifications.AnnualTouchpoints.Add( touchpoint );
            }

            /// <summary>
            /// Adds a notification about the person being inactive.
            /// </summary>
            /// <param name="personId">The identifier of the person that will receive the notification.</param>
            public void AddInactiveNotification( int personId )
            {
                var notifications = GetOrAddPerson( personId );

                notifications.SendInactiveNotification = true;
            }
        }

        /// <summary>
        /// This stores the information about when each time period was last
        /// processed. This lets us handle running multiple times a day and
        /// ensure we only run each time period once that day.
        /// </summary>
        private class LastProcessed
        {
            /// <summary>
            /// The date and time the last morning processing completed.
            /// </summary>
            public DateTime? LastMorningRun { get; set; }

            /// <summary>
            /// The date and time the last afternoon processing completed.
            /// </summary>
            public DateTime? LastAfternoonRun { get; set; }

            /// <summary>
            /// The date and time the last evening processing completed.
            /// </summary>
            public DateTime? LastEveningRun { get; set; }
        }

        /// <summary>
        /// Notification states for a person.
        /// </summary>
        private class PersonNotificationState
        {
            /// <summary>
            /// Determines if daily notifications are enabled for the person.
            /// </summary>
            public bool EnableDailyNotifications { get; set; }

            /// <summary>
            /// Determines if special event notifications are enabled for the person.
            /// </summary>
            public bool EnableSpecialEventNotifications { get; set; }
        }

        #endregion
    }
}
