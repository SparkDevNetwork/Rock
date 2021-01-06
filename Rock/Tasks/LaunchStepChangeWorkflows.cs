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
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Processes workflows that are triggered by changes to a Step entity.
    /// </summary>
    public sealed class LaunchStepChangeWorkflows : LaunchEntityChangeWorkflows<Step, StepWorkflowTrigger, LaunchStepChangeWorkflows.Message>
    {
        /// <summary>
        /// Get a list of triggers that may be fired by a state change in the entity being processed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override List<StepWorkflowTrigger> GetEntityChangeTriggers( RockContext rockContext, Message message )
        {
            // Get the triggers associated with the Step Type to which this Step is related.
            var triggers = new List<StepWorkflowTrigger>();
            var stepTypeService = new StepTypeService( rockContext );

            var stepType = stepTypeService.Queryable()
                .AsNoTracking()
                .Include( x => x.StepWorkflowTriggers )
                .FirstOrDefault( x => x.Id == message.StepTypeId );

            if ( stepType == null )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Step Type does not exist [StepTypeId={ message.StepTypeId }]." );
                return null;
            }

            var stepTypeTriggers = stepType.StepWorkflowTriggers
                .Where( w => w.TriggerType != StepWorkflowTrigger.WorkflowTriggerCondition.Manual )
                .ToList();

            triggers.AddRange( stepTypeTriggers );

            var stepProgramId = stepType.StepProgramId;

            // Get the triggers associated with the Step Program to which this Step is related, but are not associated with a specific Step Type.
            var stepProgramService = new StepProgramService( rockContext );

            var stepProgram = stepProgramService.Queryable()
                .AsNoTracking()
                .Include( x => x.StepWorkflowTriggers )
                .FirstOrDefault( x => x.Id == stepProgramId );

            if ( stepProgram == null )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Step Program does not exist [StepProgramId={ stepProgramId }]." );
                return null;
            }

            var stepProgramTriggers = stepProgram.StepWorkflowTriggers
                .Where( w =>
                    w.StepTypeId == null &&
                    w.TriggerType != StepWorkflowTrigger.WorkflowTriggerCondition.Manual )
                .ToList();

            triggers.AddRange( stepProgramTriggers );

            return triggers;
        }

        /// <summary>
        /// Evaluate if a specific trigger should be processed for the target entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override bool ShouldProcessTrigger( RockContext rockContext, StepWorkflowTrigger trigger, Message message )
        {
            if ( trigger.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                // Determine if this Step has transitioned between statuses that match the "To" and "From" qualifiers for this trigger.
                // If the condition does not specify a status, any status will match.
                // A new Step can only match this condition if it has a matching "To" State and a "From" State is not specified.
                if ( message.EntityState == EntityState.Added || message.EntityState == EntityState.Modified )
                {
                    var settings = new StepWorkflowTrigger.StatusChangeTriggerSettings( trigger.TypeQualifier );

                    if ( ( settings.FromStatusId == null || settings.FromStatusId.Value == message.PreviousStepStatusId.GetValueOrDefault( -1 ) )
                         && ( settings.ToStatusId == null || settings.ToStatusId.Value == message.CurrentStepStatusId.GetValueOrDefault( -1 ) ) )
                    {
                        return true;
                    }
                }
            }
            else if ( trigger.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete )
            {
                // Determine if this Step has transitioned from an incomplete status to a complete status.
                // Note that adding a new Step with a complete status will satisfy this trigger.
                if ( message.EntityState == EntityState.Added || message.EntityState == EntityState.Modified )
                {
                    var statusService = new StepStatusService( rockContext );

                    var fromStatus = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == message.PreviousStepStatusId );
                    var toStatus = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == message.CurrentStepStatusId );

                    if ( ( fromStatus == null || !fromStatus.IsCompleteStatus )
                         && ( toStatus != null && toStatus.IsCompleteStatus ) )
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        /// <summary>
        /// Create an associative entity that stores the relationship between the workflow instance and the target entity.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="targetEntityId"></param>
        /// <param name="workflowId"></param>
        /// <param name="triggerId"></param>
        protected override void CreateWorkflowInstanceEntity( RockContext rockContext, int targetEntityId, int workflowId, int triggerId )
        {
            // Create a new StepWorkflow instance to track the association between the Step and the Workflow.
            var stepWorkflow = new StepWorkflow
            {
                StepId = targetEntityId,
                WorkflowId = workflowId,
                StepWorkflowTriggerId = triggerId
            };

            var stepWorkflowService = new StepWorkflowService( rockContext );
            stepWorkflowService.Add( stepWorkflow );
        }

        /// <summary>
        /// Get the target entity.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override IEntity GetTargetEntity( RockContext rockContext, Guid entityGuid )
        {
            var stepService = new StepService( rockContext );
            var step = stepService.Get( entityGuid );
            return step;
        }

        /// <summary>
        /// Get a set of known properties from the specified trigger.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="workflowTypeId"></param>
        /// <param name="triggerId"></param>
        /// <param name="triggerName"></param>
        protected override void GetWorkflowTriggerProperties( StepWorkflowTrigger trigger, out int workflowTypeId, out int triggerId, out string triggerName )
        {
            triggerId = trigger.Id;

            workflowTypeId = trigger.WorkflowTypeId;

            if ( trigger.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                triggerName = "Status Changed";
            }
            else
            {
                triggerName = trigger.WorkflowName;
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BaseMessage
        {
            /// <summary>
            /// Gets or sets the current step status identifier.
            /// </summary>
            /// <value>
            /// The current step status identifier.
            /// </value>
            public int? CurrentStepStatusId { get; set; }

            /// <summary>
            /// Gets or sets the previous step status identifier.
            /// </summary>
            /// <value>
            /// The previous step status identifier.
            /// </value>
            public int? PreviousStepStatusId { get; set; }

            /// <summary>
            /// Gets or sets the step type identifier.
            /// </summary>
            /// <value>
            /// The step type identifier.
            /// </value>
            public int StepTypeId { get; set; }
        }
    }
}