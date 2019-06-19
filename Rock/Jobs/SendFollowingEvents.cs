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
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Determines if any events have occurred for the enitities that a person follows, and if so notifies them
    /// </summary>
    [SystemEmailField( "Following Event Notification Email Template", required: true, order: 0, key: "EmailTemplate" )]
    [SecurityRoleField( "Eligible Followers", "The group that contains individuals who should receive following event notification", true, order: 1 )]
    [DisallowConcurrentExecution]
    public class SendFollowingEvents : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendFollowingEvents()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? groupGuid = dataMap.GetString( "EligibleFollowers" ).AsGuidOrNull();
            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();
            int followingEventsSent = 0;

            if ( groupGuid.HasValue && systemEmailGuid.HasValue )
            {
                var exceptionMsgs = new List<string>();

                using ( var rockContext = new RockContext() )
                {
                    var followingService = new FollowingService( rockContext );
                    var followingEventTypeService = new FollowingEventTypeService( rockContext );
                    var followingEventNotificationService = new FollowingEventNotificationService( rockContext );

                    // Get all the active event types
                    var eventTypes = followingEventTypeService
                        .Queryable().AsNoTracking()
                        .Where( e =>
                            e.EntityTypeId.HasValue &&
                            e.IsActive )
                        .OrderBy( e => e.Order )
                        .ToList();

                    // Get the required event types
                    var requiredEventTypes = eventTypes
                        .Where( e => e.IsNoticeRequired )
                        .ToList();

                    // The people who are eligible to get following event notices based on the group type setting for this job
                    var eligiblePersonIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group != null &&
                            m.Group.Guid.Equals( groupGuid.Value ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Person != null &&
                            m.Person.Email != null &&
                            m.Person.Email != string.Empty &&
                            m.Person.EmailPreference != EmailPreference.DoNotEmail &&
                            m.Person.IsEmailActive
                         )
                        .Select( m => m.PersonId )
                        .Distinct()
                        .ToList();

                    // Get all the subscriptions for the eligible people
                    var eventSubscriptions = new FollowingEventSubscriptionService( rockContext )
                        .Queryable( "PersonAlias" ).AsNoTracking()
                        .Where( f => eligiblePersonIds.Contains( f.PersonAlias.PersonId ) )
                        .ToList();

                    // Dictionaries used to store information that will be used to create notification
                    var personSubscriptions = new Dictionary<int, List<int>>();                     // Key: personId, Value: list of event type ids that person subscribes to
                    var personFollowings = new Dictionary<int, List<int>>();                        // Key: personId, Value: list of following ids that person follows
                    var eventsThatHappened = new Dictionary<int, Dictionary<int, string>>();        // Key: event type id Value: Dictionary of entity id and formatted event notice for the entity

                    //Get the subscriptions for each person
                    foreach ( int personId in eligiblePersonIds )
                    {
                        var personEventTypes = eventSubscriptions
                            .Where( s => s.PersonAlias.PersonId == personId )
                            .Select( s => s.EventType )
                            .ToList();
                        personEventTypes.AddRange( requiredEventTypes );
                        if ( personEventTypes.Any() )
                        {
                            personSubscriptions.AddOrIgnore( personId, personEventTypes
                                .OrderBy( e => e.Order )
                                .ThenBy( e => e.Name )
                                .Select( e => e.Id )
                                .Distinct()
                                .ToList() );
                        }
                    }

                    // Get a distinct list of each entitytype/entity that is being followed by anyone that subscribes to events
                    var followings = followingService
                        .Queryable( "PersonAlias" ).AsNoTracking()
                        .Where( f => personSubscriptions.Keys.Contains( f.PersonAlias.PersonId ) )
                        .ToList();

                    // group the followings by their type
                    var followedEntityIds = new Dictionary<int, List<int>>();
                    foreach ( var followedEntity in followings
                        .Select( f => new
                        {
                            f.EntityTypeId,
                            f.EntityId
                        } )
                        .Distinct() )
                    {
                        followedEntityIds.AddOrIgnore( followedEntity.EntityTypeId, new List<int>() );
                        followedEntityIds[followedEntity.EntityTypeId].Add( followedEntity.EntityId );
                    }

                    // group the followings by the follower
                    foreach ( int personId in personSubscriptions.Select( s => s.Key ) )
                    {
                        var personFollowing = followings
                            .Where( f => f.PersonAlias.PersonId == personId )
                            .Select( f => f.Id )
                            .ToList();

                        personFollowings.Add( personId, personFollowing );
                    }

                    var timestamp = RockDateTime.Now;

                    // foreach followed entitytype
                    foreach ( var keyVal in followedEntityIds )
                    {
                        // Get the entitytype
                        EntityTypeCache itemEntityType = EntityTypeCache.Get( keyVal.Key );
                        if ( itemEntityType != null && itemEntityType.AssemblyName != null )
                        {
                            // get the actual type of what is being followed 
                            Type entityType = itemEntityType.GetEntityType();
                            if ( entityType != null )
                            {
                                var dbContext = Reflection.GetDbContextForEntityType( entityType );
                                if ( dbContext != null )
                                {
                                    var serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
                                    if ( serviceInstance != null )
                                    {
                                        MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                                        var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                                        // If looking at person alias following, make sure to exclude deceased people
                                        if ( entityType == typeof( Rock.Model.PersonAlias ) )
                                        {
                                            var personAliasQry = entityQry as IQueryable<PersonAlias>;
                                            if ( personAliasQry != null )
                                            {
                                                entityQry = personAliasQry.Where( p => !p.Person.IsDeceased );
                                            }
                                        }

                                        var entityList = entityQry.Where( q => keyVal.Value.Contains( q.Id ) ).ToList();

                                        // If there are any followed entities of this type 
                                        if ( entityList.Any() )
                                        {
                                            // Get the active event types for this entity type
                                            foreach ( var eventType in eventTypes.Where( e => e.FollowedEntityTypeId == keyVal.Key ) )
                                            {
                                                try
                                                {
                                                    // Get the component
                                                    var eventComponent = eventType.GetEventComponent();
                                                    if ( eventComponent != null )
                                                    {
                                                        // Get the previous notifications for this event type
                                                        var previousNotifications = followingEventNotificationService
                                                            .Queryable()
                                                            .Where( n => n.FollowingEventTypeId == eventType.Id )
                                                            .ToList();

                                                        // check each entity that is followed (by anyone)
                                                        foreach ( IEntity entity in entityList )
                                                        {
                                                            var previousNotification = previousNotifications
                                                                .Where( n => n.EntityId == entity.Id )
                                                                .FirstOrDefault();
                                                            DateTime? lastNotification = previousNotification != null ? previousNotification.LastNotified : (DateTime?)null;

                                                            // If ok to send on this day
                                                            var today = RockDateTime.Today;
                                                            if ( eventType.SendOnWeekends || ( today.DayOfWeek != DayOfWeek.Saturday && today.DayOfWeek != DayOfWeek.Sunday ) )
                                                            {
                                                                // if the event happened
                                                                if ( eventComponent.HasEventHappened( eventType, entity, lastNotification ) )
                                                                {
                                                                    // Store the event type id and the entity for later processing of notifications
                                                                    eventsThatHappened.AddOrIgnore( eventType.Id, new Dictionary<int, string>() );
                                                                    eventsThatHappened[eventType.Id].Add( entity.Id, eventComponent.FormatEntityNotification( eventType, entity ) );

                                                                    if ( previousNotification == null )
                                                                    {
                                                                        previousNotification = new FollowingEventNotification();
                                                                        previousNotification.FollowingEventTypeId = eventType.Id;
                                                                        previousNotification.EntityId = entity.Id;
                                                                        followingEventNotificationService.Add( previousNotification );
                                                                    }
                                                                    previousNotification.LastNotified = timestamp;
                                                                }
                                                            }
                                                        }

                                                        rockContext.SaveChanges();
                                                    }

                                                    eventType.LastCheckDateTime = RockDateTime.Now;
                                                }

                                                catch ( Exception ex )
                                                {
                                                    exceptionMsgs.Add( string.Format( "An exception occurred calculating events for the '{0}' suggestion type:{1}    {2}", eventType.Name, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                                                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // send notifications
                    var possibleRecipients = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => personSubscriptions.Keys.Contains( p.Id ) )
                        .ToList();

                    // Loop through the possible recipients that actually subscribe to events
                    foreach ( var personSubscription in personSubscriptions )
                    {
                        // Get the recipient person
                        int personId = personSubscription.Key;
                        var person = possibleRecipients.Where( p => p.Id == personId ).FirstOrDefault();
                        if ( person != null )
                        {
                            try
                            {

                                // Make sure person is actually following anything
                                if ( personFollowings.ContainsKey( personId ) )
                                {
                                    // Dictionary to store the entities that had an event for each event type
                                    var personEventTypeNotices = new List<FollowingEventTypeNotices>();

                                    // Get the event types that person subscribes to
                                    foreach ( var eventType in eventsThatHappened.Where( e => personSubscription.Value.Contains( e.Key ) ) )
                                    {
                                        // Get the EntityTypeId for this event type
                                        int entityTypeId = eventTypes
                                            .Where( e => e.Id == eventType.Key )
                                            .Select( e => e.FollowedEntityTypeId.Value )
                                            .FirstOrDefault();

                                        // Find all the entities with this event type that the person follows
                                        var personFollowedEntityIds = followings
                                            .Where( f =>
                                                personFollowings[personId].Contains( f.Id ) &&
                                                f.EntityTypeId == entityTypeId )
                                            .Select( f => f.EntityId )
                                            .ToList();

                                        // Get any of those entities that had an event happen
                                        var personFollowedEntities = eventType.Value
                                            .Where( e => personFollowedEntityIds.Contains( e.Key ) )
                                            .ToList();

                                        // If any were found
                                        if ( personFollowedEntities.Any() )
                                        {
                                            // Add the entry 
                                            var eventTypeObj = eventTypes.Where( e => e.Id == eventType.Key ).FirstOrDefault();
                                            if ( eventTypeObj != null )
                                            {
                                                personEventTypeNotices.Add( new FollowingEventTypeNotices( eventTypeObj, personFollowedEntities.Select( e => e.Value ).ToList() ) );
                                            }
                                        }
                                    }

                                    // If there are any events for any of the entities that this person follows, send a notification
                                    if ( personEventTypeNotices.Any() )
                                    {
                                        // Send the notice
                                        var mergeFields = new Dictionary<string, object>();
                                        mergeFields.Add( "Person", person );
                                        mergeFields.Add( "EventTypes", personEventTypeNotices.OrderBy( e => e.EventType.Order ).ToList() );

                                        var emailMessage = new RockEmailMessage( systemEmailGuid.Value );
                                        emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                        var errors = new List<string>(); 
                                        emailMessage.Send(out errors);
                                        exceptionMsgs.AddRange( errors );
                                        if ( !errors.Any() )
                                        {
                                            followingEventsSent++;
                                        }
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                exceptionMsgs.Add( string.Format( "An exception occurred sending event notice to '{0}':{1}    {2}", person.FullName, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                            }
                        }
                    }
                }

                context.Result = string.Format( "{0} following events emails sent", followingEventsSent );

                if ( exceptionMsgs.Any() )
                {
                    throw new Exception( "One or more exceptions occurred calculating following events..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
                }
            }
        }
    }

    /// <summary>
    /// Helper class for following event notifications
    /// </summary>
    [DotLiquid.LiquidType( "EventType", "Notices" )]
    public class FollowingEventTypeNotices
    {

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public FollowingEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the notices.
        /// </summary>
        /// <value>
        /// The notices.
        /// </value>
        public List<string> Notices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingEventTypeNotices"/> class.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="notices">The notices.</param>
        public FollowingEventTypeNotices( FollowingEventType eventType, List<string> notices )
        {
            EventType = eventType;
            Notices = notices;
        }
    }
}
