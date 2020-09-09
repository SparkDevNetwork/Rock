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
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches multiple workflows and optionally sets the entity, name and/or attribute values
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class LaunchWorkflowsTransaction : ITransaction
    {
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
        /// Gets or sets the initiator person alias identifier.
        /// </summary>
        /// <value>
        /// The initiator person alias identifier.
        /// </value>
        public int? InitiatorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the workflow details.
        /// </summary>
        /// <value>
        /// The workflow details.
        /// </value>
        public List<LaunchWorkflowDetails> WorkflowDetails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction" /> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowDetails">The workflow details.</param>
        public LaunchWorkflowsTransaction( Guid workflowTypeGuid, List<LaunchWorkflowDetails> workflowDetails )
        {
            WorkflowTypeGuid = workflowTypeGuid;
            WorkflowDetails = workflowDetails;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction" /> class.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowDetails">The workflow details.</param>
        public LaunchWorkflowsTransaction( int workflowTypeId, List<LaunchWorkflowDetails> workflowDetails )
        {
            WorkflowTypeId = workflowTypeId;
            WorkflowDetails = workflowDetails;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            if ( WorkflowDetails != null && WorkflowDetails.Any() )
            {
                WorkflowTypeCache workflowType = null;
                if ( WorkflowTypeGuid.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( WorkflowTypeGuid.Value );
                }

                if ( workflowType == null && WorkflowTypeId.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( WorkflowTypeId.Value );
                }

                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    foreach ( var wfDetail in WorkflowDetails )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, wfDetail.Name );
                            workflow.InitiatorPersonAliasId = InitiatorPersonAliasId;

                            if ( wfDetail.AttributeValues != null )
                            {
                                foreach ( var keyVal in wfDetail.AttributeValues )
                                {
                                    workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                                }
                            }
                            List<string> workflowErrors;
                            new Rock.Model.WorkflowService( rockContext ).Process( workflow, wfDetail.Entity, out workflowErrors );
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Details about a workflow that should be started
    /// </summary>
    public class LaunchWorkflowDetails
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowDetails"/> class.
        /// </summary>
        public LaunchWorkflowDetails()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowDetails"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public LaunchWorkflowDetails( IEntity entity )
        {
            Entity = entity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowDetails"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributeValues">The attribute values.</param>
        public LaunchWorkflowDetails( IEntity entity, Dictionary<string, string> attributeValues ) : this(entity)
        {
            AttributeValues = attributeValues;
        }
    }
}