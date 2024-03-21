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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Enums.Event;
using Rock.Event.InteractiveExperiences;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Utility;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceAnswerService
    {
        /// <summary>
        /// Records a new answer to the specified action.This also handles
        /// sending out any RealTime messages to participants.
        /// </summary>
        /// <param name="occurrenceId">The identifier of the occurrence this answer is associated with.</param>
        /// <param name="actionId">The identifier of the action this answer is for.</param>
        /// <param name="interactionSessionId">The interaction session identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="campusId">The identifier of the campus the response is originating from.</param>
        /// <param name="response">The response text.</param>
        /// <returns><c>true</c> if the answer was recorded, <c>false</c> otherwise.</returns>
        internal static async Task<RecordActionResponseStatus> RecordActionResponse( int occurrenceId, int actionId, int? interactionSessionId, int? personAliasId, int? campusId, string response )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var actionService = new InteractiveExperienceActionService( rockContext );
                var answerService = new InteractiveExperienceAnswerService( rockContext );

                var occurrence = occurrenceService.GetInclude( occurrenceId, o => o.InteractiveExperienceSchedule );
                var action = actionService.Get( actionId );

                if ( occurrence == null || action == null || occurrence.InteractiveExperienceSchedule.InteractiveExperienceId != action.InteractiveExperienceId )
                {
                    return RecordActionResponseStatus.InvalidParameters;
                }

                // If multiple submissions are not allowed then check if they
                // already submitted an answer.
                if ( !action.IsMultipleSubmissionAllowed && ( personAliasId.HasValue || interactionSessionId.HasValue ) )
                {
                    var answerQry = answerService.Queryable()
                        .Where( a => a.InteractiveExperienceOccurrenceId == occurrenceId
                            && a.InteractiveExperienceActionId == actionId );

                    if ( personAliasId.HasValue && !action.IsResponseAnonymous )
                    {
                        answerQry = answerQry.Where( a => a.PersonAliasId == personAliasId );

                        if ( answerQry.Any() )
                        {
                            return RecordActionResponseStatus.DuplicateResponse;
                        }
                    }
                    else if ( interactionSessionId.HasValue )
                    {
                        answerQry = answerQry.Where( a => a.InteractionSessionId == interactionSessionId.Value );

                        if ( answerQry.Any() )
                        {
                            return RecordActionResponseStatus.DuplicateResponse;
                        }
                    }
                }

                // Create the answer as a proxy so that navigation properties work below.
                var answer = rockContext.Set<InteractiveExperienceAnswer>().Create();
                answer.InteractiveExperienceOccurrenceId = occurrenceId;
                answer.InteractiveExperienceActionId = actionId;
                answer.Response = response;
                answer.ResponseDateTime = RockDateTime.Now;
                answer.PersonAliasId = !action.IsResponseAnonymous ? personAliasId : null;
                answer.CampusId = campusId;
                answer.InteractionSessionId = interactionSessionId;
                answer.ApprovalStatus = action.IsModerationRequired
                    ? InteractiveExperienceApprovalStatus.Pending
                    : InteractiveExperienceApprovalStatus.Approved;

                answerService.Add( answer );

                rockContext.SaveChanges();

                // Send RealTime messages to all participants.
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();
                var answerBag = answer.ToExperienceAnswerBag();

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetModeratorChannel( occurrence.IdKey ) )
                    .AnswerSubmitted( occurrence.IdKey, answerBag );

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetVisualizerChannel( occurrence.IdKey ) )
                    .AnswerSubmitted( occurrence.IdKey, answerBag );

                return RecordActionResponseStatus.Success;
            }
        }

        /// <summary>
        /// Deletes the action answer from the database. This also handles
        /// sending out any RealTime messages to participants.
        /// </summary>
        /// <param name="answerId">The identifier of the answer to be deleted.</param>
        /// <returns><c>true</c> if the answer was deleted, <c>false</c> otherwise.</returns>
        internal static async Task<bool> DeleteAnswer( int answerId )
        {
            using ( var rockContext = new RockContext() )
            {
                var answerService = new InteractiveExperienceAnswerService( rockContext );
                var answer = answerService.Get( answerId );

                if ( answer == null )
                {
                    return false;
                }

                answerService.Delete( answer );

                rockContext.SaveChanges();

                // Send RealTime messages to all participants.
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();
                var occurrenceIdKey = IdHasher.Instance.GetHash( answer.InteractiveExperienceOccurrenceId );

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetModeratorChannel( occurrenceIdKey ) )
                    .AnswerRemoved( occurrenceIdKey, answer.IdKey );

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetVisualizerChannel( occurrenceIdKey ) )
                    .AnswerRemoved( occurrenceIdKey, answer.IdKey );

                return true;
            }
        }

        /// <summary>
        /// Updates the answer approval status. This also handles sending out
        /// any RealTime messages to participants.
        /// </summary>
        /// <param name="answerId">The identifier of the answer to be updated.</param>
        /// <param name="status">The new approval status.</param>
        /// <returns><c>true</c> if the answer was updated, <c>false</c> otherwise.</returns>
        internal static async Task<bool> UpdateAnswerStatus( int answerId, InteractiveExperienceApprovalStatus status )
        {
            using ( var rockContext = new RockContext() )
            {
                var answerService = new InteractiveExperienceAnswerService( rockContext );
                var answer = answerService.Get( answerId );

                if ( answer == null )
                {
                    return false;
                }

                if ( answer.ApprovalStatus == status )
                {
                    return true;
                }

                answer.ApprovalStatus = status;

                rockContext.SaveChanges();

                // Send RealTime messages to all participants.
                var topic = RealTimeHelper.GetTopicContext<IInteractiveExperienceParticipant>();
                var occurrenceIdKey = IdHasher.Instance.GetHash( answer.InteractiveExperienceOccurrenceId );
                var answerBag = answer.ToExperienceAnswerBag();

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetModeratorChannel( occurrenceIdKey ) )
                    .AnswerUpdated( occurrenceIdKey, answerBag );

                await topic.Clients.Channel( InteractiveExperienceParticipantTopic.GetVisualizerChannel( occurrenceIdKey ) )
                    .AnswerUpdated( occurrenceIdKey, answerBag );

                return true;
            }
        }

        /// <summary>
        /// Gets the answer bags for the specified experience occurrence. All
        /// answers for the occurrence are included, no filtering is performed.
        /// </summary>
        /// <param name="occurrenceId">The occurrence identifier.</param>
        /// <returns>A collection of <see cref="ExperienceAnswerBag"/> objects that represent the answers.</returns>
        internal IEnumerable<ExperienceAnswerBag> GetAnswerBagsForOccurrence( int occurrenceId )
        {
            return Queryable()
                .Where( a => a.InteractiveExperienceOccurrenceId == occurrenceId )
                .Select( a => new
                {
                    a.Id,
                    a.InteractiveExperienceActionId,
                    a.CampusId,
                    a.PersonAlias.Person.NickName,
                    a.PersonAlias.Person.LastName,
                    a.ApprovalStatus,
                    a.Response
                } )
                .ToList()
                .Select( a =>
                {
                    var campus = a.CampusId.HasValue
                        ? CampusCache.Get( a.CampusId.Value )
                        : null;

                    var name = a.NickName.IsNotNullOrWhiteSpace() && a.LastName.IsNotNullOrWhiteSpace()
                        ? $"{a.NickName} {a.LastName}"
                        : null;

                    return new ExperienceAnswerBag
                    {
                        IdKey = IdHasher.Instance.GetHash( a.Id ),
                        ActionIdKey = IdHasher.Instance.GetHash( a.InteractiveExperienceActionId ),
                        CampusGuid = campus?.Guid,
                        CampusName = campus?.Name,
                        SubmitterName = name,
                        Status = a.ApprovalStatus,
                        Response = a.Response
                    };
                } )
                .ToList();
        }
    }
}
