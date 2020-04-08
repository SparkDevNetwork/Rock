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
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a workflow and optionally sets the name and attribute values
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class LaunchWorkflowTransaction : ITransaction
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
        /// Gets or sets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets the initiator person alias identifier.
        /// </summary>
        /// <value>
        /// The initiator person alias identifier.
        /// </value>
        public int? InitiatorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the workflow attribute values.
        /// </summary>
        /// <value>
        /// The workflow attribute values.
        /// </value>
        public Dictionary<string, string> WorkflowAttributeValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid )
        {
            WorkflowTypeGuid = workflowTypeGuid;
            WorkflowAttributeValues = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid, string workflowName ) : this( workflowTypeGuid )
        {
            WorkflowName = workflowName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        public LaunchWorkflowTransaction( int workflowTypeId )
        {
            WorkflowTypeId = workflowTypeId;
            WorkflowAttributeValues = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        public LaunchWorkflowTransaction( int workflowTypeId, string workflowName ) : this( workflowTypeId )
        {
            WorkflowName = workflowName;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
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
                    var workflow = Rock.Model.Workflow.Activate( workflowType, WorkflowName );
                    workflow.InitiatorPersonAliasId = InitiatorPersonAliasId;

                    foreach ( var keyVal in WorkflowAttributeValues )
                    {
                        workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                    }

                    List<string> workflowErrors;
                    new Rock.Model.WorkflowService( rockContext ).Process( workflow, GetEntity(), out workflowErrors );
                }
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <returns></returns>
        public virtual IEntity GetEntity()
        {
            return null;
        }
    }

    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public class LaunchWorkflowTransaction<T> : LaunchWorkflowTransaction
    where T : Rock.Data.Entity<T>, new()
    {

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction{T}"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid, int entityId ) : base( workflowTypeGuid )
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction{T}"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="entityId">The entity identifier.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid, string workflowName, int entityId ) : base( workflowTypeGuid, workflowName )
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction{T}"/> class.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        public LaunchWorkflowTransaction( int workflowTypeId, int entityId ) : base( workflowTypeId )
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction{T}"/> class.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="entityId">The entity identifier.</param>
        public LaunchWorkflowTransaction( int workflowTypeId, string workflowName, int entityId ) : base( workflowTypeId, workflowName )
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <returns></returns>
        public override IEntity GetEntity()
        {
            T entity = null;

            Type modelType = typeof( T );
            if ( modelType != null )
            {
                Rock.Data.DbContext dbContext = Reflection.GetDbContextForEntityType( modelType ) as Rock.Data.DbContext;
                if ( dbContext != null )
                {
                    var serviceInstance = new Service<T>( dbContext );
                    entity = serviceInstance.Get( EntityId.Value );
                }
            }

            return entity;
        }
    }
}