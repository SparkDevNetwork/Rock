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
using System.Data.Entity.Infrastructure;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to process changes that occur to an attempt
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class AchievementAttemptChangeTransaction : ITransaction
    {
        /// <summary>
        /// Gets the streak achievement attempt unique identifier.
        /// </summary>
        private Guid AchievementAttemptGuid { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is starting.
        /// </summary>
        private bool IsNowStarting { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is ending.
        /// </summary>
        private bool IsNowEnding { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is successful.
        /// </summary>
        private bool IsNowSuccessful { get; }

        /// <summary>
        /// Gets the achievement type identifier.
        /// </summary>
        private int AchievementTypeId { get; }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        private DateTime StartDate { get; }

        /// <summary>
        /// Gets the end date.
        /// </summary>
        private DateTime? EndDate { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementAttemptChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public AchievementAttemptChangeTransaction( DbEntityEntry entry )
        {
            if ( entry.State != EntityState.Added && entry.State != EntityState.Modified )
            {
                return;
            }

            var achievementAttempt = entry.Entity as AchievementAttempt;

            var wasClosed = entry.State != EntityState.Added && ( entry.Property( "IsClosed" )?.OriginalValue as bool? ?? false );
            var wasSuccessful = entry.State != EntityState.Added && ( entry.Property( "IsSuccessful" )?.OriginalValue as bool? ?? false );

            AchievementAttemptGuid = achievementAttempt.Guid;
            IsNowStarting = entry.State == EntityState.Added;
            IsNowEnding = !wasClosed && achievementAttempt.IsClosed;
            IsNowSuccessful = !wasSuccessful && achievementAttempt.IsSuccessful;
            AchievementTypeId = achievementAttempt.AchievementTypeId;
            StartDate = achievementAttempt.AchievementAttemptStartDateTime;
            EndDate = achievementAttempt.AchievementAttemptEndDateTime;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute()
        {
            var achievementTypeCache = AchievementTypeCache.Get( AchievementTypeId );

            if ( achievementTypeCache == null || !achievementTypeCache.IsActive )
            {
                return;
            }

            if ( IsNowStarting && achievementTypeCache.AchievementStartWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementStartWorkflowTypeId.Value );
            }

            if ( IsNowEnding && !IsNowSuccessful && achievementTypeCache.AchievementFailureWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementFailureWorkflowTypeId.Value );
            }

            if ( IsNowSuccessful && achievementTypeCache.AchievementSuccessWorkflowTypeId.HasValue )
            {
                LaunchWorkflow( achievementTypeCache.AchievementSuccessWorkflowTypeId.Value );
            }

            if ( IsNowSuccessful &&
                achievementTypeCache.AchievementStepStatusId.HasValue &&
                achievementTypeCache.AchievementStepTypeId.HasValue )
            {
                var rockContext = new RockContext();
                var achievementAttemptService = new AchievementAttemptService( rockContext );
                var achieverEntityId = achievementAttemptService.Queryable()
                    .AsNoTracking()
                    .Where( aa => aa.Guid == AchievementAttemptGuid )
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
                    AddStep( achievementTypeCache.AchievementStepTypeId.Value,
                        achievementTypeCache.AchievementStepStatusId.Value, personAliasId );
                }
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        private void LaunchWorkflow( int workflowTypeId )
        {
            var attempt = GetAchievementAttempt();
            ITransaction transaction;

            if ( attempt != null )
            {
                transaction = new LaunchWorkflowTransaction<AchievementAttempt>( workflowTypeId, attempt.Id );
            }
            else
            {
                transaction = new LaunchWorkflowTransaction( workflowTypeId );
            }

            transaction.Enqueue();
        }

        /// <summary>
        /// Gets the achievement attempt.
        /// </summary>
        /// <returns></returns>
        private AchievementAttempt GetAchievementAttempt()
        {
            if ( _achievementAttempt != null )
            {
                return _achievementAttempt;
            }

            var rockContext = new RockContext();
            var service = new AchievementAttemptService( rockContext );
            _achievementAttempt = service.Get( AchievementAttemptGuid );
            return _achievementAttempt;
        }
        private AchievementAttempt _achievementAttempt = null;

        /// <summary>
        /// Adds the step.
        /// </summary>
        /// <param name="stepTypeId">The step type identifier.</param>
        /// <param name="stepStatusId">The step status identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        private void AddStep( int stepTypeId, int stepStatusId, int personAliasId )
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
                CompletedDateTime = stepStatus.IsCompleteStatus ? EndDate : null,
                StartDateTime = StartDate,
                EndDateTime = stepType.HasEndDate ? EndDate : null,
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
    }
}