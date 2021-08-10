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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Launch a workflow using the Message Bus.
    /// Or to use the Transaction Queue, use <seealso cref="Rock.Transactions.LaunchWorkflowTransaction" />
    /// </summary>
    public sealed class LaunchWorkflow : BusStartedTask<LaunchWorkflow.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                WorkflowTypeCache workflowType = null;
                if ( message.WorkflowTypeGuid.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( message.WorkflowTypeGuid.Value );
                }

                if ( workflowType == null && message.WorkflowTypeId.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( message.WorkflowTypeId.Value );
                }

                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, message.WorkflowName );
                    workflow.InitiatorPersonAliasId = message.InitiatorPersonAliasId;

                    if ( message.WorkflowAttributeValues != null )
                    {
                        foreach ( var keyVal in message.WorkflowAttributeValues )
                        {
                            workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                        }
                    }

                    var entity = GetEntity( rockContext, message );
                    new WorkflowService( rockContext ).Process( workflow, entity, out var _ );
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
            /// Gets or sets the initiator person alias identifier.
            /// </summary>
            /// <value>
            /// The initiator person alias identifier.
            /// </value>
            public int? InitiatorPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the workflow type unique identifier.
            /// </summary>
            /// <value>
            /// The workflow type unique identifier.
            /// </value>
            public Guid? WorkflowTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the workflow type identifier.
            /// </summary>
            /// <value>
            /// The workflow type identifier.
            /// </value>
            public int? WorkflowTypeId { get; set; }

            /// <summary>
            /// Gets or sets the name of the workflow.
            /// </summary>
            /// <value>
            /// The name of the workflow.
            /// </value>
            public string WorkflowName { get; set; }

            /// <summary>
            /// Gets or sets the workflow attribute values.
            /// </summary>
            /// <value>
            /// The workflow attribute values.
            /// </value>
            public Dictionary<string, string> WorkflowAttributeValues { get; set; }

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