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

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.EventSubscription;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block for users to select which following events they would like to subscribe to.
    /// </summary>
    [DisplayName( "Event Subscription" )]
    [Category( "Follow" )]
    [Description( "Block for users to select which following events they would like to subscribe to." )]
    [IconCssClass( "ti ti-flag" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "2E52E258-373F-48AE-9FA8-7DF0F2CBAB65" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "49F69ACD-0F77-4604-ACED-5E476EDEEFC3" )]
    [Rock.SystemGuid.BlockTypeGuid( "F72A4100-001E-47F9-9406-5529F2A45131" )]
    public class EventSubscription : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<EventSubscriptionBag, EventSubscriptionOptionsBag>();

            var currentPerson = RequestContext.CurrentPerson;
            var isAuthenticated = currentPerson != null && currentPerson.PrimaryAliasId.HasValue;

            var bag = new EventSubscriptionBag
            {
                IsAuthenticated = isAuthenticated,
                EntityTypes = new List<EventSubscriptionEntityTypeBag>()
            };

            if ( isAuthenticated )
            {
                var subscribedEventTypeIds = GetSubscribedEventTypeIds( currentPerson.Id );
                bag.EntityTypes = GetEntityTypeBags( GetAuthorizedEventTypes(), subscribedEventTypeIds );
            }

            box.Bag = bag;

            return box;
        }

        /// <summary>
        /// Gets the active following event types the current person is authorized
        /// to view, ordered for display.
        /// </summary>
        /// <returns>The list of authorized following event types.</returns>
        private List<FollowingEventType> GetAuthorizedEventTypes()
        {
            var currentPerson = RequestContext.CurrentPerson;

            return new FollowingEventTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( f => f.IsActive && f.FollowedEntityTypeId.HasValue )
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList()
                .Where( f => f.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the distinct following event type identifiers the person is
        /// subscribed to across all of their person aliases.
        /// </summary>
        /// <param name="personId">The identifier of the person.</param>
        /// <returns>The set of subscribed following event type identifiers.</returns>
        private HashSet<int> GetSubscribedEventTypeIds( int personId )
        {
            var subscribedEventTypeIds = new FollowingEventSubscriptionService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( s => s.PersonAlias.PersonId == personId )
                .Select( s => s.EventTypeId )
                .Distinct()
                .ToList();

            return new HashSet<int>( subscribedEventTypeIds );
        }

        /// <summary>
        /// Groups the event types by their followed entity type and converts
        /// them into bags for the client.
        /// </summary>
        /// <param name="eventTypes">The event types to group, already ordered for display.</param>
        /// <param name="subscribedEventTypeIds">The event type identifiers the person is subscribed to.</param>
        /// <returns>The list of entity type group bags.</returns>
        private List<EventSubscriptionEntityTypeBag> GetEntityTypeBags( List<FollowingEventType> eventTypes, HashSet<int> subscribedEventTypeIds )
        {
            return eventTypes
                .GroupBy( f => f.FollowedEntityTypeId.Value )
                .Select( g => new
                {
                    // "Person Alias" events are presented as "Person" events,
                    // since the alias is an implementation detail to the user.
                    Name = EntityTypeCache.Get( g.Key )?.FriendlyName?.Replace( " Alias", string.Empty ) ?? string.Empty,
                    EventTypes = g
                } )
                .OrderBy( g => g.Name )
                .Select( g => new EventSubscriptionEntityTypeBag
                {
                    Name = g.Name,
                    Events = g.EventTypes
                        .Select( f => new EventSubscriptionEventBag
                        {
                            Guid = f.Guid.ToString(),
                            Name = f.Name,
                            Description = f.Description,
                            IsNoticeRequired = f.IsNoticeRequired,
                            IsSubscribed = subscribedEventTypeIds.Contains( f.Id )
                        } )
                        .ToList()
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the current person's following event subscriptions. Only event
        /// types visible to the person are affected; subscriptions to inactive
        /// or unauthorized event types are left untouched.
        /// </summary>
        /// <param name="selectedEventTypeGuids">The unique identifiers of the following event types the person selected.</param>
        /// <returns>A result indicating whether the save succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( List<Guid> selectedEventTypeGuids )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var currentPersonAliasId = currentPerson?.PrimaryAliasId;

            if ( currentPerson == null || !currentPersonAliasId.HasValue )
            {
                return ActionUnauthorized( "You must be logged in to manage your event subscriptions." );
            }

            selectedEventTypeGuids = selectedEventTypeGuids ?? new List<Guid>();

            var subscriptionService = new FollowingEventSubscriptionService( RockContext );
            var existingSubscriptions = subscriptionService.Queryable()
                .Where( s => s.PersonAlias.PersonId == currentPerson.Id )
                .ToList();

            foreach ( var eventType in GetAuthorizedEventTypes() )
            {
                // Notice-required events are always subscribed, regardless of
                // what the client sent.
                var isSelected = eventType.IsNoticeRequired || selectedEventTypeGuids.Contains( eventType.Guid );
                var subscriptions = existingSubscriptions
                    .Where( s => s.EventTypeId == eventType.Id )
                    .ToList();

                if ( isSelected && !subscriptions.Any() )
                {
                    subscriptionService.Add( new FollowingEventSubscription
                    {
                        EventTypeId = eventType.Id,
                        PersonAliasId = currentPersonAliasId.Value
                    } );
                }
                else if ( !isSelected )
                {
                    foreach ( var subscription in subscriptions )
                    {
                        subscriptionService.Delete( subscription );
                    }
                }
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
