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
    /// Launches multiple workflows and optionally sets the entity, name and/or attribute values
    /// Or to use the Transaction Queue, use <seealso cref="Rock.Transactions.LaunchWorkflowsTransaction" />
    /// </summary>
    public sealed class LaunchWorkflows : BusStartedTask<LaunchWorkflows.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            if ( message.WorkflowDetails != null && message.WorkflowDetails.Any() )
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
                    foreach ( var wfDetail in message.WorkflowDetails )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, wfDetail.Name );
                            workflow.InitiatorPersonAliasId = message.InitiatorPersonAliasId;

                            if ( wfDetail.WorkflowAttributeValues != null )
                            {
                                foreach ( var keyVal in wfDetail.WorkflowAttributeValues )
                                {
                                    workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                                }
                            }

                            var entity = GetEntity( rockContext, wfDetail );
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
        /// <param name="workflowDetail">The workflow detail.</param>
        /// <returns></returns>
        public IEntity GetEntity( RockContext rockContext, WorkflowDetail workflowDetail )
        {
            if ( !workflowDetail.EntityTypeId.HasValue || !workflowDetail.EntityId.HasValue )
            {
                return null;
            }

            var entityTypeService = new EntityTypeService( rockContext );
            var entity = entityTypeService.GetEntity( workflowDetail.EntityTypeId.Value, workflowDetail.EntityId.Value );
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
            /// Gets or sets the workflow detail.
            /// </summary>
            /// <value>
            /// The workflow detail.
            /// </value>
            public List<WorkflowDetail> WorkflowDetails { get; set; }
        }

        /// <summary>
        /// Details about a workflow that should be started
        /// </summary>
        public class WorkflowDetail
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

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