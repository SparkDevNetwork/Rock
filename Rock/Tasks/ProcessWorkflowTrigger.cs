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
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public sealed class ProcessWorkflowTrigger : BusStartedTask<ProcessWorkflowTrigger.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            if ( message.WorkflowTriggerGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var trigger = new WorkflowTriggerService( rockContext ).Get( message.WorkflowTriggerGuid.Value );
                    if ( trigger != null )
                    {
                        var workflowType = WorkflowTypeCache.Get( trigger.WorkflowTypeId );
                        if ( workflowType != null && ( trigger.IsActive ?? true ) )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                            var entity = GetEntity( rockContext, message );
                            new Rock.Model.WorkflowService( rockContext ).Process( workflow, entity, out var _ );
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