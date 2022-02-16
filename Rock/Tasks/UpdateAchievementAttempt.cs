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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Task to process actions that should occur after adding/updating an <see cref="AchievementAttempt" />.
    /// </summary>
    public sealed class UpdateAchievementAttempt : BusStartedTask<UpdateAchievementAttempt.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var achievementTypeCache = AchievementTypeCache.Get( message.AchievementTypeId );

            if ( achievementTypeCache == null || !achievementTypeCache.IsActive )
            {
                return;
            }

            if ( message.IsNowStarting && achievementTypeCache.AchievementStartWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementStartWorkflowTypeId.Value, message );
            }

            if ( message.IsNowEnding && !message.IsNowSuccessful && achievementTypeCache.AchievementFailureWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementFailureWorkflowTypeId.Value, message );
            }

            if ( message.IsNowSuccessful && achievementTypeCache.AchievementSuccessWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementSuccessWorkflowTypeId.Value, message );
            }

            if ( message.IsNowSuccessful &&
                achievementTypeCache.AchievementStepStatusId.HasValue &&
                achievementTypeCache.AchievementStepTypeId.HasValue )
            {
                var rockContext = new RockContext();
                var achievementAttemptService = new AchievementAttemptService( rockContext );
                var achieverEntityId = achievementAttemptService.Queryable()
                    .AsNoTracking()
                    .Where( aa => aa.Guid == message.AchievementAttemptGuid )
                    .Select( s => s.AchieverEntityId )
                    .FirstOrDefault();

                var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
                var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
                int personAliasId = default;

                if ( achievementTypeCache.AchieverEntityTypeId == personAliasEntityTypeId )
                {
                    personAliasId = achieverEntityId;
                }
                else if ( achievementTypeCache.AchieverEntityTypeId == personEntityTypeId )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    personAliasId = personAliasService.Queryable()
                        .AsNoTracking()
                        .Where( pa => pa.PersonId == achieverEntityId )
                        .Select( pa => pa.Id )
                        .FirstOrDefault();
                }

                if ( personAliasId != default )
                {
                    AddStep(
                        achievementTypeCache.AchievementStepTypeId.Value,
                        achievementTypeCache.AchievementStepStatusId.Value,
                        personAliasId,
                        message );
                }
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="message"></param>
        private void LaunchWorkflow( int workflowTypeId, Message message )
        {
            var attempt = GetAchievementAttempt( message );

            if ( attempt == null )
            {
                return;
            }

            var workflowTransaction =new LaunchWorkflowTransaction<AchievementAttempt>( workflowTypeId, attempt.Id );
            workflowTransaction.InitiatorPersonAliasId = message.InitiatorPersonAliasId;
            workflowTransaction.Execute();
        }

        /// <summary>
        /// Gets the achievement attempt.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private AchievementAttempt GetAchievementAttempt( Message message )
        {
            if ( _achievementAttempt != null )
            {
                return _achievementAttempt;
            }

            var rockContext = new RockContext();
            var service = new AchievementAttemptService( rockContext );
            _achievementAttempt = service.Get( message.AchievementAttemptGuid );
            return _achievementAttempt;
        }

        private AchievementAttempt _achievementAttempt = null;

        /// <summary>
        /// Adds the step.
        /// </summary>
        /// <param name="stepTypeId">The step type identifier.</param>
        /// <param name="stepStatusId">The step status identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="message"></param>
        private void AddStep( int stepTypeId, int stepStatusId, int personAliasId, Message message )
        {
            var rockContext = new RockContext();
            var stepService = new StepService( rockContext );
            var stepProgramService = new StepProgramService( rockContext );

            // Get the step program with step types and statuses to better calculate the dates for the new step
            var stepProgram = stepProgramService.Queryable( "StepTypes, StepStatuses" ).FirstOrDefault( sp =>
                sp.StepTypes.Any( st => st.Id == stepTypeId ) &&
                sp.StepStatuses.Any( ss => ss.Id == stepStatusId ) );

            var stepType = stepProgram?.StepTypes.FirstOrDefault( st => st.Id == stepTypeId );
            var stepStatus = stepProgram?.StepStatuses.FirstOrDefault( ss => ss.Id == stepStatusId );

            if ( stepType == null )
            {
                ExceptionLogService.LogException( $"Error adding step related to an achievement. The step type {stepTypeId} did not resolve." );
                return;
            }

            if ( stepStatus == null )
            {
                ExceptionLogService.LogException( $"Error adding step related to an achievement. The step status {stepStatusId} did not resolve." );
                return;
            }

            // Add the new step
            var step = new Step
            {
                StepTypeId = stepTypeId,
                StepStatusId = stepStatusId,
                CompletedDateTime = stepStatus.IsCompleteStatus ? message.EndDate : null,
                StartDateTime = message.StartDate,
                EndDateTime = stepType.HasEndDate ? message.EndDate : null,
                PersonAliasId = personAliasId
            };

            // If the person cannot be added to the step type, then don't add anything since some step types only allow one step
            // or require pre-requisites
            if ( stepService.CanAdd( step, out _ ) )
            {
                stepService.Add( step );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the streak achievement attempt unique identifier.
            /// </summary>
            public Guid AchievementAttemptGuid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is starting.
            /// </summary>
            public bool IsNowStarting { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is ending.
            /// </summary>
            public bool IsNowEnding { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is successful.
            /// </summary>
            public bool IsNowSuccessful { get; set; }

            /// <summary>
            /// Gets or sets the achievement type identifier.
            /// </summary>
            public int AchievementTypeId { get; set; }

            /// <summary>
            /// Gets or sets the start date.
            /// </summary>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date.
            /// </summary>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Gets or sets the workflow initiator person alias identifier.
            /// </summary>
            /// <value>The workflow initiator person alias identifier.</value>
            public int? InitiatorPersonAliasId { get; set; }
        }
    }
}