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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;

#if REVIEW_NET5_0_OR_GREATER
using NetTopologySuite.Geometries;
#endif

using Rock.Communication;
using Rock.Data;
using Rock.Enums.Event;
using Rock.Event.InteractiveExperiences;
using Rock.Net;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceOccurrenceService
    {
        /// <summary>
        /// Show an action on the specified experience occurrence. This handles
        /// updating the database, sending out RealTime messages and sending
        /// any notifications if requested.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="sendNotifications">If <c>true</c> then push notifications will be sent.</param>
        /// <returns>A <see cref="Task"/> representing when the database has updated and notifications queued.</returns>
        internal static async Task ShowActionAsync( int occurrenceId, int actionId, bool sendNotifications )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var actionService = new InteractiveExperienceActionService( rockContext );

                // Load and validate the occurrence.
                var occurrence = occurrenceService.GetInclude( occurrenceId, o => o.InteractiveExperienceSchedule.InteractiveExperience );

                if ( occurrence == null )
                {
                    throw new Exception( "Experience occurrence was not found." );
                }

                // Load and validate the action.
                var action = actionService.GetNoTracking( actionId );

                if ( action == null )
                {
                    throw new Exception( "Experience action was not found." );
                }
                else if ( action.InteractiveExperienceId != occurrence.InteractiveExperienceSchedule.InteractiveExperienceId )
                {
                    throw new Exception( "Invalid action for this experience." );
                }

                // Get and validate the action component.
                var actionType = ActionTypeContainer.GetComponentFromEntityType( action.ActionEntityTypeId );

                if ( actionType == null )
                {
                    throw new Exception( "Action is no longer supported." );
                }

                // Check if we are already in the correct state.
                if ( occurrence.CurrentlyShownActionId == action.Id )
                {
                    return;
                }

                // Update the database to record that this action is now visible.
                occurrence.CurrentlyShownActionId = action.Id;
                rockContext.SaveChanges();

                // Send RealTime messages to participants.
                action.LoadAttributes( rockContext );
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();
                var configuration = actionType.GetRenderConfiguration( action );

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetParticipantChannel( occurrence.IdKey ) )
                    .ShowAction( occurrence.IdKey, action.IdKey, configuration );

                // Send out push notifications to participants.
                var experience = occurrence.InteractiveExperienceSchedule.InteractiveExperience;
                if ( experience.PushNotificationType != InteractiveExperiencePushNotificationType.Never )
                {
                    if ( sendNotifications || experience.PushNotificationType == InteractiveExperiencePushNotificationType.EveryAction )
                    {
                        var mergeFields = new Dictionary<string, object>
                        {
                            ["InteractiveExperience"] = experience,
                        };

                        var title = experience.PushNotificationTitle.ResolveMergeFields( mergeFields );
                        var detail = experience.PushNotificationDetail.ResolveMergeFields( mergeFields );

                        var deviceQry = occurrenceService.GetActiveParticipantDevices( occurrenceId );
                        var deviceIds = deviceQry
                            .Where( pd => pd.NotificationsEnabled && !string.IsNullOrEmpty( pd.DeviceRegistrationId ) )
                            .Select( pd => pd.DeviceRegistrationId )
                            .Distinct()
                            .ToList();

                        // Don't wait for the push notifications to be sent.
                        _ = Task.Run( async () => await SendPushNotificationsAsync( occurrence.IdKey, title, detail, deviceIds ) );
                    }
                }
            }
        }

        /// <summary>
        /// Clear all actions from the experience occurrence. This also handles
        /// sending out any RealTime messages to participants.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <returns>A <see cref="Task"/> representing when the database has been updated and notifications queued.</returns>
        internal static async Task ClearActionsAsync( int occurrenceId )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load and validate the occurrence.
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var occurrence = occurrenceService.GetInclude( occurrenceId, o => o.InteractiveExperienceSchedule );

                if ( occurrence == null )
                {
                    throw new Exception( "Experience occurrence was not found." );
                }

                // Check if we are already in the correct state.
                if ( !occurrence.CurrentlyShownActionId.HasValue )
                {
                    return;
                }

                // Update the database.
                occurrence.CurrentlyShownActionId = null;
                rockContext.SaveChanges();

                // Send RealTime messages to all participants.
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetParticipantChannel( occurrence.IdKey ) )
                    .ClearActions( occurrence.IdKey );
            }
        }

        /// <summary>
        /// Show a visualizer on the specified experience occurrence. This handles
        /// updating the database and sending out RealTime messages.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <param name="actionId">The identifier of the action whose visualizer is to be shown.</param>
        /// <returns>A <see cref="Task"/> representing when the database has updated and RealTime messages queued.</returns>
        internal static async Task ShowVisualizerAsync( int occurrenceId, int actionId )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var actionService = new InteractiveExperienceActionService( rockContext );

                // Load and validate the occurrence.
                var occurrence = occurrenceService.Get( occurrenceId );

                if ( occurrence == null )
                {
                    throw new Exception( "Experience occurrence was not found." );
                }

                // Get the experience for this occurrence.
                var experience = InteractiveExperienceScheduleCache.Get( occurrence.InteractiveExperienceScheduleId )
                    ?.InteractiveExperience;

                // Load and validate the action.
                var action = actionService.GetNoTracking( actionId );

                if ( action == null )
                {
                    throw new Exception( "Experience action was not found." );
                }
                else if ( action.InteractiveExperienceId != experience.Id )
                {
                    throw new Exception( "Invalid action for this experience." );
                }

                VisualizerTypeComponent visualizerType = null;
                if ( action.ResponseVisualEntityTypeId.HasValue )
                {
                    // Get and validate the visualizer component.
                    visualizerType = VisualizerTypeContainer.GetComponentFromEntityType( action.ResponseVisualEntityTypeId.Value );

                    if ( visualizerType == null )
                    {
                        throw new Exception( "Visualizer is no longer supported." );
                    }
                }

                var state = occurrence.StateJson.FromJsonOrNull<ExperienceOccurrenceStateBag>() ?? new ExperienceOccurrenceStateBag();

                // Check if we are already in the correct state.
                if ( state.CurrentlyShownVisualizerActionId == action.Id )
                {
                    return;
                }

                // Update the database to record that this action is now visible.
                state.CurrentlyShownVisualizerActionId = action.Id;
                occurrence.StateJson = state.ToJson();
                rockContext.SaveChanges();

                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();
                VisualizerRenderConfigurationBag configuration = null;

                // Send RealTime messages to participants.
                if ( visualizerType != null )
                {
                    action.LoadAttributes( rockContext );
                    configuration = visualizerType.GetRenderConfiguration( action );
                }

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetParticipantChannel( occurrence.IdKey ) )
                    .ShowVisualizer( occurrence.IdKey, action.IdKey, configuration );
            }
        }

        /// <summary>
        /// Clear the visualzier from the experience occurrence. This also handles
        /// sending out any RealTime messages to participants.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <returns>A <see cref="Task"/> representing when the database has been updated and notifications queued.</returns>
        internal static async Task ClearVisualizerAsync( int occurrenceId )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load and validate the occurrence.
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var occurrence = occurrenceService.GetInclude( occurrenceId, o => o.InteractiveExperienceSchedule );

                if ( occurrence == null )
                {
                    throw new Exception( "Experience occurrence was not found." );
                }

                var state = occurrence.StateJson.FromJsonOrNull<ExperienceOccurrenceStateBag>() ?? new ExperienceOccurrenceStateBag();

                // Check if we are already in the correct state.
                if ( !state.CurrentlyShownVisualizerActionId.HasValue )
                {
                    return;
                }

                // Update the database.
                state.CurrentlyShownVisualizerActionId = null;
                occurrence.StateJson = state.ToJson();
                rockContext.SaveChanges();

                // Send RealTime messages to all participants.
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetVisualizerChannel( occurrence.IdKey ) )
                    .ClearVisualizer( occurrence.IdKey );
            }
        }

        /// <summary>
        /// Sends out a push notification to all of the device registration
        /// identifiers.
        /// </summary>
        /// <param name="key">The interactive experience occurrence identifier key.</param>
        /// <param name="title">The title of the push notification.</param>
        /// <param name="detail">The detailed message of the push notification.</param>
        /// <param name="deviceRegistrationIds">The tokens that specify the devices to receive the push notifications.</param>
        /// <returns>A task that represents this operation.</returns>
        private static async Task SendPushNotificationsAsync( string key, string title, string detail, List<string> deviceRegistrationIds )
        {
            if ( !MediumContainer.HasActivePushTransport() )
            {
                return;
            }

            var pushMessage = new RockPushMessage
            {
                Title = title.ToStringOrDefault( "New Experience Activity" ),
                Message = detail.ToStringOrDefault( "The experience has a new activity showing." ),
                Data = new PushData()
            };

            pushMessage.Data.CustomData = new Dictionary<string, string>
            {
                ["Rock-InteractiveExperienceOccurrenceKey"] = key
            };

            var mergeFields = new Dictionary<string, object>();

            pushMessage.SetRecipients( deviceRegistrationIds.Select( d => RockPushMessageRecipient.CreateAnonymous( d, mergeFields ) ).ToList() );

            await pushMessage.SendAsync();
        }

        /// <summary>
        /// Creates or updates an interaction representing the participation in
        /// the experience occurrence.
        /// </summary>
        /// <remarks>
        /// This should be called whenever a participant has joined an experience.
        /// </remarks>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <param name="interactionGuid">The interaction unique identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="personalDeviceId">The personal device identifier.</param>
        /// <param name="campusId">The identifier of the campus being attended during this experience.</param>
        /// <param name="requestContext">The request context.</param>
        /// <param name="interactionSessionId">On exit, contains the interaction session identifier associated with the interaction.</param>
        internal static void CreateOrUpdateInteraction( int occurrenceId, Guid interactionGuid, int? personAliasId, int? personalDeviceId, int? campusId, RockRequestContext requestContext, out int interactionSessionId )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

                var occurrenceInfo = occurrenceService.GetSelect( occurrenceId, o => new
                {
                    o.InteractiveExperienceSchedule.InteractiveExperience.Name,
                    o.OccurrenceDateTime
                } );

                var occurrenceName = $"{occurrenceInfo.Name} at {occurrenceInfo.OccurrenceDateTime.ToShortDateTimeString()}";

                // Get the interaction channel and component information.
                var interactionChannelId = GetInteractionChannelId();
                var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId, occurrenceId, occurrenceName );

                // Try to find an existing interaction. This can happen if the
                // RealTime client disconnects and then re-connects.
                var interaction = interactionService.Get( interactionGuid );

                if ( interaction == null )
                {
                    // Create the basic interaction (in-memory).
                    interaction = interactionService.CreateInteraction( interactionComponentId,
                        requestContext?.ClientInformation.UserAgent,
                        null,
                        requestContext?.ClientInformation.IpAddress,
                        null );

                    // Update interaction details.
                    interaction.Guid = interactionGuid;
                    interaction.InteractionSummary = $"Participated at {RockDateTime.Now.ToShortDateTimeString()}";
                    interaction.Operation = "Participate";
                    interaction.InteractionLength = 0;

                    interactionService.Add( interaction );
                }

                interaction.PersonAliasId = personAliasId;
                interaction.PersonalDeviceId = personalDeviceId;
                interaction.EntityId = campusId;
                interaction.InteractionEndDateTime = RockDateTime.Now;
                interaction.ChannelCustomIndexed1 = string.Empty;

                rockContext.SaveChanges();

                interactionSessionId = interaction.InteractionSessionId.Value;
            }
        }

        /// <summary>
        /// Updates the duration of the interaction. The EndDateTime and Length
        /// values are updated to reflect the current timestamp.
        /// </summary>
        /// <remarks>
        /// This should be called whenever a participant notifies the server
        /// that it is still online and participating.
        /// </remarks>
        /// <param name="interactionGuid">The interaction unique identifier.</param>
        internal static void UpdateInteractionDuration( Guid interactionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );
                var interaction = interactionService.Get( interactionGuid );

                // Somebody went and deleted the original interaction. We can't create it now.
                if ( interaction == null )
                {
                    return;
                }

                interaction.InteractionEndDateTime = RockDateTime.Now;
                interaction.InteractionLength = ( interaction.InteractionEndDateTime.Value - interaction.InteractionDateTime ).TotalSeconds;

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the interaction to show that the participant is no longer
        /// participating in the experience.
        /// </summary>
        /// <remarks>
        /// This should be called only when a participant explicitely informs
        /// the server that they have left the experience. It should not be
        /// called on timeout disconnects.
        /// </remarks>
        /// <param name="interactionGuid">The interaction unique identifier.</param>
        internal static void FinalizeInteraction( Guid interactionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );
                var interaction = interactionService.Get( interactionGuid );

                // Somebody went and deleted the original interaction. We can't create it now.
                if ( interaction == null )
                {
                    return;
                }

                interaction.InteractionEndDateTime = RockDateTime.Now;
                interaction.InteractionLength = ( interaction.InteractionEndDateTime.Value - interaction.InteractionDateTime ).TotalSeconds;
                interaction.ChannelCustomIndexed1 = "Inactive";

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the number of recent participants in the experience
        /// occurrence. This is roughly the number of people that are currently
        /// online. But since we can't determine a true online count it should
        /// be considered a close estimate.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <returns>The number of participants that are assumed to be online based on the interaction data.</returns>
        internal int GetRecentParticipantCount( int occurrenceId )
        {
            var interactionChannelId = GetInteractionChannelId();
            var activeDateTime = RockDateTime.Now.AddSeconds( -60 );

            // Find interactions that:
            // Are for the experience occurrence.
            // Have been recently active.
            return new InteractionService( Context as RockContext )
                .Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.InteractionComponent.EntityId == occurrenceId
                    && i.InteractionEndDateTime >= activeDateTime )
                .Count();
        }

        /// <summary>
        /// Gets all of the active participant devices for the experience
        /// occurrence. A device is considered active if it has not explicitely
        /// told us that it has left the experience.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <returns>A queryable of <see cref="PersonalDevice"/> that represents the online devices.</returns>
        internal IQueryable<PersonalDevice> GetActiveParticipantDevices( int occurrenceId )
        {
            var interactionChannelId = GetInteractionChannelId();

            // Find interactions that:
            // Are for the experience occurrence.
            // Have not been marked inactive.
            // Have a personal device attached.
            return new InteractionService( Context as RockContext )
                .Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.InteractionComponent.EntityId == occurrenceId
                    && i.ChannelCustomIndexed1 != "Inactive"
                    && i.PersonalDeviceId.HasValue )
                .Select( i => i.PersonalDevice );
        }

        /// <summary>
        /// Gets all the active occurrences.
        /// </summary>
        /// <returns>A queryable of active occurrences that can be further filtered.</returns>
        internal IQueryable<InteractiveExperienceOccurrence> GetActiveOccurrences()
        {
            var occurrenceIds = new List<int>();
            var experiences = InteractiveExperienceCache.All()
                .Where( e => e.IsActive )
                .ToList();

            // Loop through all experiences and get all the occurrence ids
            // that are currently active. Create them as needed.
            foreach ( var experience in experiences )
            {
                occurrenceIds.AddRange( experience.GetOrCreateAllCurrentOccurrenceIds() );
            }

            // Load all occurrences in a single query.
            return Queryable()
                .Where( o => occurrenceIds.Contains( o.Id ) );
        }

        /// <summary>
        /// Gets all the active and valid occurrences for the specified person.
        /// This is meant to be used, for example, when showing the individual
        /// a list of live experiences they can join.
        /// </summary>
        /// <param name="currentPerson">The current person to use when filtering occurrences.</param>
        /// <param name="latitude">Contains the geo-location latitude for this individual or <c>null</c> if not known.</param>
        /// <param name="longitude">Contains the geo-location longitude for this individual or <c>null</c> if not known.</param>
        /// <returns>An object that contains the occurrences and additional details about whether a login or geo-location will improve results.</returns>
        internal ValidOccurrencesResult GetValidOccurrences( Person currentPerson, double? latitude, double? longitude )
        {
            var campusFenceCache = new Dictionary<int, bool>();
            var results = new List<InteractiveExperienceOccurrence>();
            var occurrenceIds = new List<int>();
#if REVIEW_NET5_0_OR_GREATER
            Point geoPoint = null;
#else
            DbGeography geoPoint = null;
#endif
            var loginRecommended = false;
            var geoLocationRecommended = false;

            if ( latitude.HasValue && longitude.HasValue )
            {
#if REVIEW_NET5_0_OR_GREATER
                geoPoint = new Point( longitude.Value, latitude.Value );
#else
                geoPoint = DbGeography.FromText( $"POINT({longitude} {latitude})" );
#endif
            }

            var experiences = InteractiveExperienceCache.All()
                .Where( e => e.IsActive )
                .ToList();

            // Loop through all experiences and get all the occurrence ids
            // that are currently active. Create them as needed.
            foreach ( var experience in experiences )
            {
                occurrenceIds.AddRange( experience.GetOrCreateAllCurrentOccurrenceIds() );
            }

            // Load all occurrences in a single query.
            var occurrences = Queryable()
                .Include( o => o.InteractiveExperienceSchedule )
                .Where( o => occurrenceIds.Contains( o.Id ) )
                .ToList();

            // No occurrences means nothing to return anyway, don't bother filtering.
            if ( !occurrences.Any() )
            {
                return new ValidOccurrencesResult();
            }

            // Loop through the experiences a second time, performing all
            // the filtering that is required.
            foreach ( var experience in experiences )
            {
                var filterByGeoFence = experience.ExperienceSettings.CampusBehavior == InteractiveExperienceCampusBehavior.FilterSchedulesByCampusGeofences;

                // If a schedule specified 3 campuses, then it will generate 3
                // occurrences. So to make sure we don't perform requirements
                // checks those additional times, group by the schedule id.
                var scheduleGroups = occurrences
                    .Where( o => o.InteractiveExperienceSchedule.InteractiveExperienceId == experience.Id )
                    .GroupBy( o => o.InteractiveExperienceScheduleId )
                    .ToList();

                // Loop through all schedules and check requirements.
                foreach ( var scheduleOccurrenceGroup in scheduleGroups )
                {
                    // Check any DataView or Group membership requirements.
                    if ( !InteractiveExperienceScheduleService.AreRequirementsMetForPerson( scheduleOccurrenceGroup.Key, currentPerson ) )
                    {
                        if ( currentPerson == null )
                        {
                            // No person logged in, indicate it is needed.
                            loginRecommended = true;
                        }

                        continue;
                    }

                    // Loop through each occurrence and perform final filtering.
                    foreach ( var occurrence in scheduleOccurrenceGroup )
                    {
                        // If the experience is configured to filter by geofence and
                        // this occurrence was associated with a specific campus then
                        // perform filtering on it.
                        if ( filterByGeoFence && occurrence.CampusId.HasValue )
                        {
                            if ( geoPoint == null )
                            {
                                // No geo location provided, indicate it is needed.
                                geoLocationRecommended = true;
                                continue;
                            }

                            if ( !campusFenceCache.TryGetValue( occurrence.CampusId.Value, out var inFence ) )
                            {
                                inFence = CampusCache.Get( occurrence.CampusId.Value ).ContainsGeoPoint( geoPoint );
                                campusFenceCache.Add( occurrence.CampusId.Value, inFence );
                            }

                            if ( !inFence )
                            {
                                continue;
                            }
                        }

                        results.Add( occurrence );
                    }
                }
            }

            return new ValidOccurrencesResult( results, loginRecommended, geoLocationRecommended );
        }

        /// <summary>
        /// Gets the interaction channel identifier to be used when recording
        /// interactions.
        /// </summary>
        /// <returns>The interaction channel identifier value.</returns>
        private static int GetInteractionChannelId()
        {
            var channelGuid = SystemGuid.InteractionChannel.INTERACTIVE_EXPERIENCES.AsGuid();
            var channelId = InteractionChannelCache.GetId( channelGuid );

            if ( channelId.HasValue )
            {
                return channelId.Value;
            }

            using ( var rockContext = new RockContext() )
            {
                var systemEventsMediumValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS.AsGuid() );
                var interactionChannelService = new InteractionChannelService( rockContext );

                var interactionChannel = new InteractionChannel
                {
                    Guid = channelGuid,
                    Name = "Interactive Experiences",
                    ChannelTypeMediumValueId = systemEventsMediumValueId,
                    ComponentEntityTypeId = EntityTypeCache.GetId<InteractiveExperienceOccurrence>()
                };

                interactionChannelService.Add( interactionChannel );

                try
                {
                    rockContext.SaveChanges();

                    return interactionChannel.Id;
                }
                catch
                {
                    // Assume this is because another thread got this written before us.
                    // If this fails we'll get an exception which is probably good.
                    return InteractionChannelCache.GetId( channelGuid ).Value;
                }
            }
        }

        /// <summary>
        /// Gets the current occurrence identifier for the schedule in relation
        /// to the given campus. If no occurrence exists but one should exist based
        /// on the schedule then a new occurrence is created.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>An integer that represents the matching occurrence identifier or <c>null</c> if an occurrence should not be active right now.</returns>
        internal static int? GetOrCreateCurrentOccurrenceId( int interactiveExperienceScheduleId, int? campusId )
        {
            var experienceSchedule = InteractiveExperienceScheduleCache.Get( interactiveExperienceScheduleId );

            if ( experienceSchedule == null )
            {
                return null;
            }

            var occurrenceDateTime = experienceSchedule.GetCurrentOccurrenceDateTime();

            if ( !occurrenceDateTime.HasValue )
            {
                return null;
            }

            // We have a valid occurrence date and time. Get or create
            // the occurrence for that date and time.
            return GetOrCreateCurrentOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime.Value );
        }

        /// <summary>
        /// Gets the current occurrence identifier for the schedule, campus and
        /// date-time. If no occurrence exists then a new occurrence is created.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <returns>An integer that represents the matching occurrence identifier.</returns>
        private static int GetOrCreateCurrentOccurrenceId( int interactiveExperienceScheduleId, int? campusId, DateTime occurrenceDateTime )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

                var occurrenceId = occurrenceService.GetExistingOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime );

                if ( occurrenceId.HasValue )
                {
                    return occurrenceId.Value;
                }

                // An existing occurrence wasn't found, create a new one.
                var occurrence = new InteractiveExperienceOccurrence
                {

                    InteractiveExperienceScheduleId = interactiveExperienceScheduleId,
                    CampusId = campusId,
                    OccurrenceDateTime = occurrenceDateTime,
                };

                occurrenceService.Add( occurrence );

                try
                {
                    rockContext.SaveChanges();

                    return occurrence.Id;
                }
                catch ( Exception ex )
                {
                    // If an exception happens, assume it was a unique constraint
                    // violation. Meaning, another thread or server created the
                    // occurrence after we checked for it. Try to load again.
                    occurrenceId = occurrenceService.GetExistingOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime );

                    if ( occurrenceId.HasValue )
                    {
                        return occurrenceId.Value;
                    }

                    throw new Exception( "Failed to load or create InteractiveExperienceOccurrence", ex );
                }
            }
        }

        /// <summary>
        /// Gets the existing occurrence identifier for the specified schedule,
        /// campus and date-time.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <returns>An integer that represents the matching occurrence identifier or <c>null</c> if an occurrence was not found.</returns>
        private int? GetExistingOccurrenceId( int interactiveExperienceScheduleId, int? campusId, DateTime occurrenceDateTime )
        {
            return Queryable()
                .Where( ieo => ieo.InteractiveExperienceScheduleId == interactiveExperienceScheduleId
                    && ( ( !ieo.CampusId.HasValue && !campusId.HasValue ) || ieo.CampusId == campusId )
                    && ieo.OccurrenceDateTime == occurrenceDateTime )
                .OrderBy( ieo => ieo.Id )
                .Select( ieo => ( int? ) ieo.Id )
                .FirstOrDefault();
        }
    }
}