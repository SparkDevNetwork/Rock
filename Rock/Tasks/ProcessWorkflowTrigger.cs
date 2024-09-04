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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Processes a <seealso cref="WorkflowTrigger"/>
    /// </summary>
    public sealed class ProcessWorkflowTrigger : BusStartedTask<ProcessWorkflowTrigger.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var triggerGuids = message.WorkflowTriggerGuids ?? new List<Guid>();
            var hasTagsBeenSet = false;
            EntityTypeCache entityTypeCache = null;

            if ( message.WorkflowTriggerGuid.HasValue )
            {
                triggerGuids.Add( message.WorkflowTriggerGuid.Value );
            }

            if ( !triggerGuids.Any() )
            {
                return;
            }

            if ( message.EntityTypeId.HasValue )
            {
                entityTypeCache = EntityTypeCache.Get( message.EntityTypeId.Value );
            }

            using ( var activity = Observability.ObservabilityHelper.StartActivity( $"WT: {entityTypeCache?.FriendlyName}" ) )
            {
                foreach ( var triggerGuid in triggerGuids )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var trigger = new WorkflowTriggerService( rockContext ).Get( triggerGuid);
                        if ( trigger != null )
                        {
                            var workflowType = WorkflowTypeCache.Get( trigger.WorkflowTypeId );
                            if ( workflowType != null && ( trigger.IsActive ?? true ) )
                            {
                                var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                                var entity = GetEntity( rockContext, message );

                                if ( !hasTagsBeenSet && activity != null )
                                {
                                    activity.SetTag( "rock.trigger.entity_type_id", entity.TypeId );
                                    activity.SetTag( "rock.trigger.entity_type", entity.TypeName );
                                    activity.SetTag( "rock.trigger.entity_id", entity.Id );

                                    hasTagsBeenSet = true;
                                }

                                new Rock.Model.WorkflowService( rockContext ).Process( workflow, entity, out var _ );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public IEntity GetEntity( RockContext rockContext, Message message )
        {
            if ( !message.EntityTypeId.HasValue || !message.EntityId.HasValue )
            {
                return null;
            }

            var entityTypeService = new EntityTypeService( rockContext );
            var entity = entityTypeService.GetEntity( message.EntityTypeId.Value, message.EntityId.Value );
            return entity;
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the workflow trigger identifier.
            /// </summary>
            /// <value>
            /// The workflow trigger identifier.
            /// </value>
            public Guid? WorkflowTriggerGuid { get; set; }

            /// <summary>
            /// Gets or sets the workflow trigger identifier.
            /// </summary>
            /// <value>
            /// The workflow trigger identifier.
            /// </value>
            public List<Guid> WorkflowTriggerGuids { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public int? EntityId { get; set; }

            /// <summary>
            /// Gets or sets the entity type identifier.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public int? EntityTypeId { get; set; }
        }
    }
}