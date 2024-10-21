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
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Event.InteractiveExperiences;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

/*
 * All "Id" values sent over RealTime messages should be the IdKey values unless
 * otherwise noted in the method documentation.
 * 
 * The following channels are used by this topic:
 *
 * experience:${experienceOccurrenceIdKey} - Messages to all experience participants.
 *                              ShowAction
 *                              ClearActions
 *
 * moderator:${experienceOccurrenceIdKey} - Messages to all experience moderators.
 *                             AnswerUpdated
 *                             AnswerRemoved
 *
 * visualizer:${experienceOccurrenceIdKey} - Messages to all visualizer screens.
 *                             ShowVisualizer
 *                             ClearVisualizer
 *                             AnswerUpdated
 *                             AnswerRemoved
 */
namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic for client devices to use when connecting to an Interactive
    /// Experience event.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.14.1", true )]
    [RealTimeTopic]
    internal class InteractiveExperienceParticipantTopic : Topic<IInteractiveExperienceParticipant>
    {
        #region Message Methods

        /// <summary>
        /// Joins an experience that is currently happening and subscribes
        /// to related message channels.
        /// </summary>
        /// <param name="experienceToken">The encrypted token data that allows this device to join the experience.</param>
        /// <param name="cancellationToken">A token that indicates when this operation should be cancelled.</param>
        /// <returns>A bag that identifies the occurrence and the initial state.</returns>
        public async Task<JoinExperienceResponseBag> JoinExperience( string experienceToken, CancellationToken cancellationToken )
        {
            var token = Security.Encryption.DecryptString( experienceToken ).FromJsonOrNull<ExperienceToken>();
            var occurrenceId = IdHasher.Instance.GetId( token?.OccurrenceId ?? string.Empty );

            if ( token == null || !occurrenceId.HasValue || occurrenceId.Value == 0 )
            {
                throw new RealTimeException( "Invalid experience token." );
            }

            var occurrenceState = new ExperienceOccurrenceState
            {
                InteractiveExperienceOccurrenceId = occurrenceId.Value,
                InteractionGuid = token.InteractionGuid,
                CampusId = token.CampusId,
                IsModerator = token.IsModerator,
                IsVisualizer = token.IsVisualizer
            };

            JoinExperienceResponseBag response = new JoinExperienceResponseBag();

            using ( var rockContext = new RockContext() )
            {
                var actionService = new InteractiveExperienceActionService( rockContext );
                var occurrence = new InteractiveExperienceOccurrenceService( rockContext )
                    .Queryable()
                    .Include( o => o.CurrentlyShownAction )
                    .Include( o => o.InteractiveExperienceSchedule )
                    .Where( o => o.Id == occurrenceId.Value )
                    .SingleOrDefault();

                if ( occurrence == null )
                {
                    throw new RealTimeException( "Requested occurrence was not found." );
                }

                occurrenceState.InteractiveExperienceId = occurrence.InteractiveExperienceSchedule.InteractiveExperienceId;

                response.OccurrenceIdKey = occurrence.IdKey;
                response.CurrentActionIdKey = occurrence.CurrentlyShownAction?.IdKey;
                response.CurrentActionConfiguration = actionService.GetActionRenderBag( occurrence.CurrentlyShownAction );

                // If the connection is a moderator visualizer, populate the
                // information about the current visualizer to sync UI state.
                if ( occurrenceState.IsModerator || occurrenceState.IsVisualizer )
                {
                    var state = occurrence.StateJson.FromJsonOrNull<ExperienceOccurrenceStateBag>();

                    if ( state != null && state.CurrentlyShownVisualizerActionId.HasValue )
                    {
                        var responseAction = state.CurrentlyShownVisualizerActionId == occurrence.CurrentlyShownActionId
                            ? occurrence.CurrentlyShownAction
                            : actionService.GetNoTracking( state.CurrentlyShownVisualizerActionId.Value );

                        response.CurrentVisualizerActionIdKey = responseAction?.IdKey;
                        response.CurrentVisualizerConfiguration = actionService.GetVisualizerRenderBag( responseAction );
                    }
                }

                // Get the person alias to work with.
                if ( Context.CurrentPersonId.HasValue )
                {
                    occurrenceState.PersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( Context.CurrentPersonId.Value );
                }
                else
                {
                    occurrenceState.PersonAliasId = Context.VisitorAliasId;
                }
            }

            Context.GetConnectionState<ExperienceState>()
                .JoinedOccurrences
                .AddOrReplace( token.OccurrenceId, occurrenceState );

            // If we are supposed to track participation then record the interaction.
            if ( token.InteractionGuid.HasValue )
            {
                InteractiveExperienceOccurrenceService.CreateOrUpdateInteraction( occurrenceId.Value,
                    occurrenceState.InteractionGuid.Value,
                    occurrenceState.PersonAliasId,
                    token.PersonalDeviceId,
                    token.CampusId,
                    Context.GetRequestContext(),
                    out var interactionSessionId );

                occurrenceState.InteractionSessionId = interactionSessionId;
            }

            // Always put them in the general experience channel.
            await Channels.AddToChannelAsync( Context.ConnectionId, GetParticipantChannel( token.OccurrenceId ) );

            if ( occurrenceState.IsModerator )
            {
                await Channels.AddToChannelAsync( Context.ConnectionId, GetModeratorChannel( token.OccurrenceId ), cancellationToken );
                await Channels.AddToChannelAsync( Context.ConnectionId, GetVisualizerChannel( token.OccurrenceId ), cancellationToken );
            }

            if ( occurrenceState.IsVisualizer )
            {
                await Channels.AddToChannelAsync( Context.ConnectionId, GetVisualizerChannel( token.OccurrenceId ), cancellationToken );
            }

            return response;
        }

        /// <summary>
        /// Leaves the experience and unsubscribes from all related channels.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event to leave as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A token that indicates when this operation should be cancelled.</param>
        public async Task LeaveExperience( string occurrenceIdKey, CancellationToken cancellationToken )
        {
            var removed = Context.GetConnectionState<ExperienceState>()
                .JoinedOccurrences
                .TryRemove( occurrenceIdKey, out var state );

            if ( !removed )
            {
                throw new RealTimeException( "Experience has not been joined." );
            }

            if ( state.InteractionGuid.HasValue )
            {
                InteractiveExperienceOccurrenceService.FinalizeInteraction( state.InteractionGuid.Value );
            }

            await Channels.RemoveFromChannelAsync( Context.ConnectionId, GetParticipantChannel( occurrenceIdKey ), cancellationToken );

            if ( state.IsModerator )
            {
                await Channels.RemoveFromChannelAsync( Context.ConnectionId, GetModeratorChannel( occurrenceIdKey ), cancellationToken );
            }

            if ( state.IsVisualizer )
            {
                await Channels.RemoveFromChannelAsync( Context.ConnectionId, GetVisualizerChannel( occurrenceIdKey ), cancellationToken );
            }
        }

        /// <summary>
        /// Shows the action on the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="sendNotifications">Set to <c>true</c> if notifications should be sent; otherwise set to <c>false</c>.</param>
        public Task ShowAction( string occurrenceIdKey, string actionId, bool sendNotifications )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( !state.IsModerator )
            {
                throw new RealTimeException( "Only moderators can show actions." );
            }

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );
            var actionIntegerId = IdHasher.Instance.GetId( actionId );

            if ( !actionIntegerId.HasValue )
            {
                throw new RealTimeException( "Invalid action specified." );
            }

            return InteractiveExperienceOccurrenceService.ShowActionAsync( occurrenceId.Value, actionIntegerId.Value, sendNotifications );
        }

        /// <summary>
        /// Clears all actions from the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        public Task ClearActions( string occurrenceIdKey )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( !state.IsModerator )
            {
                throw new RealTimeException( "Only moderators can clear actions." );
            }

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );

            return InteractiveExperienceOccurrenceService.ClearActionsAsync( occurrenceId.Value );
        }

        /// <summary>
        /// Shows the visualizer on the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        /// <param name="actionId">The action identifier whose visualizer should be shown.</param>
        public Task ShowVisualizer( string occurrenceIdKey, string actionId )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( !state.IsModerator )
            {
                throw new RealTimeException( "Only moderators can show visualizers." );
            }

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );
            var actionIntegerId = IdHasher.Instance.GetId( actionId );

            if ( !actionIntegerId.HasValue )
            {
                throw new RealTimeException( "Invalid action specified." );
            }

            return InteractiveExperienceOccurrenceService.ShowVisualizerAsync( occurrenceId.Value, actionIntegerId.Value );
        }

        /// <summary>
        /// Clear the visualizer from the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        public Task ClearVisualizer( string occurrenceIdKey )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( !state.IsModerator )
            {
                throw new RealTimeException( "Only moderators can clear actions." );
            }

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );

            return InteractiveExperienceOccurrenceService.ClearVisualizerAsync( occurrenceId.Value );
        }

        /// <summary>
        /// Pings the experience to inform the server that the participant is
        /// still online and active.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        public Task<PingExperienceResponseBag> PingExperience( string occurrenceIdKey )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( state.InteractionGuid.HasValue )
            {
                InteractiveExperienceOccurrenceService.UpdateInteractionDuration( state.InteractionGuid.Value );
            }

            var experience = InteractiveExperienceCache.Get( state.InteractiveExperienceId );
            var activeOccurrenceIds = experience.GetOrCreateAllCurrentOccurrenceIds();

            var response = new PingExperienceResponseBag
            {
                IsActive = activeOccurrenceIds.Contains( state.InteractiveExperienceOccurrenceId )
            };

            return Task.FromResult( response );
        }

        /// <summary>
        /// Posts a response to an action.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="response">The response content.</param>
        /// <returns>
        ///     <para>200-OK if the answer was saved.</para>
        ///     <para>400-BadRequest if the parameters provided were not valid.</para>
        ///     <para>409-Conflict if an answer was already posted and multiple answers are not allowed.</para>
        /// </returns>
        public async Task<int> PostResponse( string occurrenceIdKey, string actionId, string response )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );
            var actionIntegerId = IdHasher.Instance.GetId( actionId );

            if ( !actionIntegerId.HasValue )
            {
                throw new RealTimeException( "Invalid action specified." );
            }

            var status = await InteractiveExperienceAnswerService.RecordActionResponse( occurrenceId.Value, actionIntegerId.Value, state.InteractionSessionId, state.PersonAliasId, state.CampusId, response );

            if ( status == RecordActionResponseStatus.Success )
            {
                return ( int ) HttpStatusCode.OK;
            }
            else if ( status == RecordActionResponseStatus.DuplicateResponse )
            {
                return ( int ) HttpStatusCode.Conflict;
            }
            else
            {
                return ( int ) HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Gets the number of participants in the event.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the event as returned by <see cref="JoinExperience(string, CancellationToken)"/>.</param>
        /// <returns>The number of participants in the event.</returns>
        public Task<int> GetParticipantCount( string occurrenceIdKey )
        {
            var state = GetOccurrenceState( occurrenceIdKey );

            if ( !state.IsModerator )
            {
                throw new RealTimeException( "Only moderators can request participant count." );
            }

            var occurrenceId = IdHasher.Instance.GetId( occurrenceIdKey );

            using ( var rockContext = new RockContext() )
            {
                var count = new InteractiveExperienceOccurrenceService( rockContext )
                    .GetRecentParticipantCount( occurrenceId.Value );

                return Task.FromResult( count );
            }
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the tracked state of the connection for the specified experience occurrence.
        /// </summary>
        /// <param name="experienceOccurrenceId">The experience occurrence identifier.</param>
        /// <returns>An instance of <see cref="ExperienceOccurrenceState"/> or throws an exception if not yet joined.</returns>
        private ExperienceOccurrenceState GetOccurrenceState( string experienceOccurrenceId )
        {
            var experienceState = Context.GetConnectionState<ExperienceState>();

            if ( experienceState.JoinedOccurrences.TryGetValue( experienceOccurrenceId, out var state ) )
            {
                return state;
            }

            throw new RealTimeException( "Experience has not been joined." );
        }

        /// <summary>
        /// Gets the participant channel for the specified occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The occurrence identifier key.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetParticipantChannel( string occurrenceIdKey )
        {
            return $"experience:{occurrenceIdKey}";
        }

        /// <summary>
        /// Gets the moderator channel for the specified occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The occurrence identifier key.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetModeratorChannel( string occurrenceIdKey )
        {
            return $"moderator:{occurrenceIdKey}";
        }

        /// <summary>
        /// Gets the visualizer channel for the specified occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The occurrence identifier key.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetVisualizerChannel( string occurrenceIdKey )
        {
            return $"visualizer:{occurrenceIdKey}";
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Holds the state for the interactive experience topic.
        /// </summary>
        private class ExperienceState
        {
            /// <summary>
            /// Gets or sets the joined experience occurrences.
            /// </summary>
            /// <value>The joined experience occurrences.</value>
            public ConcurrentDictionary<string, ExperienceOccurrenceState> JoinedOccurrences { get; set; } = new ConcurrentDictionary<string, ExperienceOccurrenceState>();
        }

        /// <summary>
        /// Holds the state for the participation in a specific experience
        /// occurrence.
        /// </summary>
        private class ExperienceOccurrenceState
        {
            /// <summary>
            /// Gets or sets the interactive experience identifier.
            /// </summary>
            /// <value>The interactive experience identifier.</value>
            public int InteractiveExperienceId { get; set; }

            /// <summary>
            /// Gets or sets the interactive experience occurrence identifier.
            /// </summary>
            /// <value>The interactive experience occurrence identifier.</value>
            public int InteractiveExperienceOccurrenceId { get; set; }

            /// <summary>
            /// Gets or sets the interaction unique identifier. If this has a
            /// value then participant should be recorded in the Interaction
            /// table.
            /// </summary>
            /// <value>The interaction unique identifier.</value>
            public Guid? InteractionGuid { get; set; }

            /// <summary>
            /// Gets or sets the interaction session identifier. This
            /// will be null if <see cref="InteractionGuid"/> is null.
            /// </summary>
            /// <value>The interaction session identifier.</value>
            public int? InteractionSessionId { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier that this token will be associated
            /// with. This is used to imply the campus the individual is currently at
            /// as opposed to what campus they normally attend.
            /// </summary>
            /// <value>The identifier of the campus physically present at.</value>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier that joined the experience.
            /// </summary>
            /// <value>The person alias identifier that joined the experience.</value>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this participant is a
            /// moderator.
            /// </summary>
            /// <value><c>true</c> if this participant is a moderator; otherwise, <c>false</c>.</value>
            public bool IsModerator { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this participant is a visualizer.
            /// </summary>
            /// <value><c>true</c> if this participant is a visualizer; otherwise, <c>false</c>.</value>
            public bool IsVisualizer { get; set; }
        }

        #endregion
    }
}
