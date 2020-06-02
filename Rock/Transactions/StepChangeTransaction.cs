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
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Processes workflows that are triggered by changes to a Step entity.
    /// </summary>
    public class StepChangeTransaction : EntityChangeWorkflowTriggerTransactionBase<Step, StepWorkflowTrigger>
    {
        private int? StepTypeId;
        private int? CurrentStepStatusId;
        private int? PreviousStepStatusId;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public StepChangeTransaction( DbEntityEntry entry )
            : base( entry )
        {
        }

        /// <summary>
        /// Called when [capture entity change parameters].
        /// </summary>
        /// <param name="entry">The entry.</param>
        protected override void OnCaptureEntityChangeParameters( DbEntityEntry entry )
        {
            var entity = ( Step ) entry.Entity;

            // Store a reference to the Step Type so we can get the relevant triggers for this Step more easily.
            this.StepTypeId = entity.StepTypeId;

            // Store the Step status change parameters.
            if ( entity.StepStatus != null )
            {
                this.CurrentStepStatusId = entity.StepStatus.Id;
            }

            if ( entry.State == EntityState.Modified )
            {
                var dbProperty = entry.Property( "StepStatusId" );

                if ( dbProperty != null )
                {
                    this.PreviousStepStatusId = dbProperty.OriginalValue as int?;
                }
            }
        }

        /// <summary>
        /// Get a list of triggers that may be fired by a state change in the entity being processed.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override List<StepWorkflowTrigger> GetEntityChangeTriggers( RockContext dataContext, Guid entityGuid )
        {
            // Get the triggers associated with the Step Type to which this Step is related.
            var triggers = new List<StepWorkflowTrigger>();

            var stepTypeService = new StepTypeService( dataContext );

            var stepType = stepTypeService.Queryable()
                .AsNoTracking()
                .Include( x => x.StepWorkflowTriggers )
                .FirstOrDefault( x => x.Id == this.StepTypeId );

            if ( stepType == null )
            {
                ExceptionLogService.LogException( $"StepChangeTransaction failed. Step Type does not exist [StepTypeId={ StepTypeId }]." );
                return null;
            }

            var stepTypeTriggers = stepType.StepWorkflowTriggers
                    .Where( w => w.TriggerType != StepWorkflowTrigger.WorkflowTriggerCondition.Manual )
                    .ToList();

            triggers.AddRange( stepTypeTriggers );

            var stepProgramId = stepType.StepProgramId;

            // Get the triggers associated with the Step Program to which this Step is related, but are not associated with a specific Step Type.
            var stepProgramService = new StepProgramService( dataContext );

            var stepProgram = stepProgramService.Queryable()
                .AsNoTracking()
                .Include( x => x.StepWorkflowTriggers )
                .FirstOrDefault( x => x.Id == stepProgramId );

            if ( stepProgram == null )
            {
                ExceptionLogService.LogException( $"StepChangeTransaction failed. Step Program does not exist [StepProgramId={ stepProgramId }]." );
                return null;
            }

            var stepProgramTriggers = stepProgram.StepWorkflowTriggers
                    .Where( w => w.StepTypeId == null
                                 && w.TriggerType != StepWorkflowTrigger.WorkflowTriggerCondition.Manual )
                    .ToList();

            triggers.AddRange( stepProgramTriggers );

            return triggers;
        }

        /// <summary>
        /// Evaluate if a specific trigger should be processed for the target entity.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="trigger"></param>
        /// <param name="entityGuid"></param>
        /// <param name="entityState"></param>
        /// <returns></returns>
        protected override bool ShouldProcessTrigger( RockContext dataContext, StepWorkflowTrigger trigger, Guid entityGuid, EntityState entityState )
        {
            if ( trigger.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                // Determine if this Step has transitioned between statuses that match the "To" and "From" qualifiers for this trigger.
                // If the condition does not specify a status, any status will match.
                // A new Step can only match this condition if it has a matching "To" State and a "From" State is not specified.
                if ( entityState == EntityState.Added || entityState == EntityState.Modified )
                {
                    var settings = new StepWorkflowTrigger.StatusChangeTriggerSettings( trigger.TypeQualifier );

                    if ( ( settings.FromStatusId == null || settings.FromStatusId.Value == PreviousStepStatusId.GetValueOrDefault( -1 )  )
                         && ( settings.ToStatusId == null || settings.ToStatusId.Value == CurrentStepStatusId.GetValueOrDefault( -1 ) ) )
                    {
                        return true;
                    }
                }
            }
            else if ( trigger.TriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete )
            {
                // Determine if this Step has transitioned from an incomplete status to a complete status.
                // Note that adding a new Step with a complete status will satisfy this trigger.
                if ( entityState == EntityState.Added || entityState == EntityState.Modified )
                {
                    var statusService = new StepStatusService( dataContext );

                    var fromStatus = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == this.PreviousStepStatusId );
                    var toStatus = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == this.CurrentStepStatusId );

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
        /// <param name="dataContext"></param>
        /// <param name="targetEntityId"></param>
        /// <param name="workflowId"></param>
        /// <param name="triggerId"></param>
        protected override void CreateWorkflowInstanceEntity( RockContext dataContext, int targetEntityId, int workflowId, int triggerId )
        {
            // Create a new StepWorkflow instance to track the association between the Step and the Workflow.
            var stepWorkflow = new StepWorkflow();

            stepWorkflow.StepId = targetEntityId;
            stepWorkflow.WorkflowId = workflowId;

            stepWorkflow.StepWorkflowTriggerId = triggerId;

            var stepWorkflowService = new StepWorkflowService( dataContext );

            stepWorkflowService.Add( stepWorkflow );
        }

        /// <summary>
        /// Get the target entity.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override IEntity GetTargetEntity( RockContext dataContext, Guid entityGuid )
        {
            var stepService = new StepService( dataContext );

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
    }

    #region Support Classes

    /// <summary>
    /// Provides a pattern and base functionality for a Rock transaction that initiates one or more workflows triggered by changes to a target Entity.
    /// </summary>
    public abstract class EntityChangeWorkflowTriggerTransactionBase<TEntity, TTrigger> : ITransaction
        where TEntity : class, IEntity
    {
        private EntityState _State;
        private Guid? _EntityGuid;

        /// <summary>
        /// Override this method to capture any change parameters that need to be available to execute the transaction.
        /// Execution is deferred until database updates are completed, so any change parameters required for the transaction must be captured during this initialization process.
        /// </summary>
        /// <param name="dbEntry">The Entity Framework object containing information about the change.</param>
        protected abstract void OnCaptureEntityChangeParameters( DbEntityEntry dbEntry );

        /// <summary>
        /// Get a list of triggers that may be fired by a state change in the entity being processed.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns></returns>
        protected abstract List<TTrigger> GetEntityChangeTriggers( RockContext dataContext, Guid entityGuid );

        /// <summary>
        /// Evaluate if a specific trigger should be processed for the target entity.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="entityState">State of the entity.</param>
        /// <returns></returns>
        protected abstract bool ShouldProcessTrigger( RockContext dataContext, TTrigger trigger, Guid entityGuid, EntityState entityState );

        /// <summary>
        /// Create an associative entity that stores the relationship between the workflow instance and the target entity.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="targetEntityId"></param>
        /// <param name="workflowId"></param>
        /// <param name="triggerId"></param>
        protected abstract void CreateWorkflowInstanceEntity( RockContext dataContext, int targetEntityId, int workflowId, int triggerId );

        /// <summary>
        /// Gets the target entity.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns></returns>
        protected abstract IEntity GetTargetEntity( RockContext dataContext, Guid entityGuid );

        /// <summary>
        /// Get a set of known properties from the specified trigger.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="workflowTypeId"></param>
        /// <param name="triggerId"></param>
        /// <param name="triggerName"></param>
        protected abstract void GetWorkflowTriggerProperties( TTrigger trigger, out int workflowTypeId, out int triggerId, out string triggerName );

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityChangeWorkflowTriggerTransactionBase{TEntity, TTrigger}"/> class.
        /// </summary>
        /// <param name="dbEntry">The database entry.</param>
        /// <exception cref="Exception">EntityChangeWorkflowTriggerTransaction initialization failed. A target entity of Type \"{ typeof( TEntity ).Name }\" is required. [EntityType=\"{ dbEntry.Entity.GetType().Name }\"]</exception>
        public EntityChangeWorkflowTriggerTransactionBase( DbEntityEntry dbEntry )
        {
            // Verify that the entity if of the expected type for this transaction.
            var targetEntity = dbEntry.Entity as TEntity;

            if ( targetEntity == null )
            {
                throw new Exception( $"EntityChangeWorkflowTriggerTransaction initialization failed. A target entity of Type \"{ typeof( TEntity ).Name }\" is required. [EntityType=\"{ dbEntry.Entity.GetType().Name }\"]" );
            }

            _State = dbEntry.State;
            _EntityGuid = targetEntity.Guid;

            // Store any change parameters that need to be available to execute the transaction.
            // Execution of this transaction is deferred until after the database update is completed, so change parameters must be captured during the initialization process.
            this.OnCaptureEntityChangeParameters( dbEntry );
        }

        /// <summary>
        /// Execute this transaction to launch the workflows that should be triggered by the Entity updates.
        /// </summary>
        public void Execute()
        {
            var dataContext = new RockContext();

            // Get the workflow triggers associated with changes to the target entity.
            var triggers = this.GetEntityChangeTriggers( dataContext, _EntityGuid.GetValueOrDefault() );

            // If no triggers to process, exit.
            if ( triggers == null || !triggers.Any() )
            {
                return;
            }

            // Evaluate and process the workflow triggers.
            foreach ( var trigger in triggers )
            {
                bool shouldProcess = this.ShouldProcessTrigger( dataContext, trigger, _EntityGuid.Value, _State );

                if ( !shouldProcess )
                {
                    continue;
                }

                int workflowTypeId;
                int triggerId;
                string name;

                // TODO: Replace this method by implementing an interface on Trigger objects that provides access to these properties.
                this.GetWorkflowTriggerProperties( trigger, out workflowTypeId, out triggerId, out name );

                this.LaunchWorkflow( dataContext, workflowTypeId, triggerId, name );
            }
        }

        /// <summary>
        /// Create and launch a new instance of a workflow for the specified trigger.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="triggerId">The trigger identifier.</param>
        /// <param name="name">The name.</param>
        private void LaunchWorkflow( RockContext dataContext, int workflowTypeId, int triggerId, string name )
        {
            // Get the Workflow Type associated with this trigger.
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );

            if ( workflowType == null )
            {
                ExceptionLogService.LogException( $"EntityChangeTransaction failed. Workflow Type does not exist [WorkflowTypeId={ workflowTypeId }]." );
                return;
            }

            if ( !( workflowType.IsActive ?? true ) )
            {
                return;
            }

            // TODO: Extend IService interface to include Get(guid) method?
            // Then we could eliminate the need for the GetTargetEntity callback, and just do this:
            //      var entityService = Reflection.GetServiceForEntityType( typeof( TEntity ), rockContext );
            //      var targetEntity = entityService.Get( this.EntityGuid.GetValueOrDefault() );

            var targetEntity = this.GetTargetEntity( dataContext, _EntityGuid.GetValueOrDefault() );

            if ( targetEntity == null )
            {
                ExceptionLogService.LogException( $"EntityChangeTransaction failed. Target entity does not exist [Guid={ _EntityGuid }]." );
                return;
            }

            // Create and process a new Workflow of the specified Workflow Type for the target Entity.
            var workflow = Rock.Model.Workflow.Activate( workflowType, name );

            var workflowService = new WorkflowService( dataContext );

            List<string> workflowErrors;

            workflowService.Process( workflow, targetEntity, out workflowErrors );

            if ( workflow.Id == 0 )
            {
                ExceptionLogService.LogException( $"EntityChangeTransaction failed. Workflow instance could not be created [WorkflowName={ name }]." );
                return;
            }

            if ( workflowErrors.Any() )
            {
                ExceptionLogService.LogException( $"EntityChangeTransaction failed. Workflow execution failed with errors [WorkflowName={ name }].\n{ workflowErrors.AsDelimited( "\n" )}" );
                return;
            }

            // Create a new EntityWorkflow instance to track the association between the target entity and the workflow.
            // TODO: Create an interface IEntityWorkflowInstance(EntityId, WorkflowId, TriggerId) to allow the instance object to be created here using a generic implementation?
            this.CreateWorkflowInstanceEntity( dataContext, targetEntity.Id, workflow.Id, triggerId );

            dataContext.SaveChanges();
        }
    }

    #endregion

}