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
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Processes workflows that are triggered by changes to an entity.
    /// </summary>
    public abstract class LaunchEntityChangeWorkflows<TEntity, TTrigger, TMessage> : BusStartedTask<TMessage>
        where TEntity : class, IEntity
        where TMessage : LaunchEntityChangeWorkflows<TEntity, TTrigger, TMessage>.BaseMessage
    {
        #region Abstract

        /// <summary>
        /// Get a list of triggers that may be fired by a state change in the entity being processed.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected abstract List<TTrigger> GetEntityChangeTriggers( RockContext dataContext, TMessage message );

        /// <summary>
        /// Evaluate if a specific trigger should be processed for the target entity.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected abstract bool ShouldProcessTrigger( RockContext dataContext, TTrigger trigger, TMessage message );

        /// <summary>
        /// Get a set of known properties from the specified trigger.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="workflowTypeId"></param>
        /// <param name="triggerId"></param>
        /// <param name="triggerName"></param>
        protected abstract void GetWorkflowTriggerProperties( TTrigger trigger, out int workflowTypeId, out int triggerId, out string triggerName );

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

        #endregion Abstract

        #region Execute

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( TMessage message )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the workflow triggers associated with changes to the target entity.
                var triggers = GetEntityChangeTriggers( rockContext, message );

                // If no triggers to process, exit.
                if ( triggers == null || !triggers.Any() )
                {
                    return;
                }

                // Evaluate and process the workflow triggers.
                foreach ( var trigger in triggers )
                {
                    var shouldProcess = ShouldProcessTrigger( rockContext, trigger, message );

                    if ( !shouldProcess )
                    {
                        continue;
                    }

                    // TODO: Replace this method by implementing an interface on Trigger objects that provides access to these properties.
                    GetWorkflowTriggerProperties( trigger, out var workflowTypeId, out var triggerId, out var name );
                    LaunchWorkflow( rockContext, workflowTypeId, triggerId, name, message );
                }
            }
        }

        #endregion Execute

        #region Private Methods

        /// <summary>
        /// Create and launch a new instance of a workflow for the specified trigger.
        /// </summary>
        /// <param name="rockContext">The data context.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="triggerId">The trigger identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="message">The message.</param>
        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId, int triggerId, string name, TMessage message )
        {
            // Get the Workflow Type associated with this trigger.
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );

            if ( workflowType == null )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Workflow Type does not exist [WorkflowTypeId={ workflowTypeId }]." );
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
            var targetEntity = GetTargetEntity( rockContext, message.EntityGuid );

            if ( targetEntity == null )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Target entity does not exist [Guid={ message.EntityGuid }]." );
                return;
            }

            // Create and process a new Workflow of the specified Workflow Type for the target Entity.
            var workflow = Model.Workflow.Activate( workflowType, name );

            var workflowService = new WorkflowService( rockContext );
            workflowService.Process( workflow, targetEntity, out var workflowErrors );

            if ( workflow.Id == 0 )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Workflow instance could not be created [WorkflowName={ name }]." );
                return;
            }

            if ( workflowErrors.Any() )
            {
                ExceptionLogService.LogException( $"{GetType().Name} failed. Workflow execution failed with errors [WorkflowName={ name }].\n{ workflowErrors.AsDelimited( "\n" )}" );
                return;
            }

            // Create a new EntityWorkflow instance to track the association between the target entity and the workflow.
            // TODO: Create an interface IEntityWorkflowInstance(EntityId, WorkflowId, TriggerId) to allow the instance object to be created here using a generic implementation?
            CreateWorkflowInstanceEntity( rockContext, targetEntity.Id, workflow.Id, triggerId );

            rockContext.SaveChanges();
        }

        #endregion Private Methods

        /// <summary>
        /// Message Class
        /// </summary>
        public abstract class BaseMessage : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the state of the entity.
            /// </summary>
            /// <value>
            /// The state of the entity.
            /// </value>
            public EntityState EntityState { get; set; }

            /// <summary>
            /// Gets or sets the entity unique identifier.
            /// </summary>
            /// <value>
            /// The entity unique identifier.
            /// </value>
            public Guid EntityGuid { get; set; }
        }
    }
}