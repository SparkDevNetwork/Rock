// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.IO;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Reflection;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public class LaunchWorkflowTransaction<T> : ITransaction
        where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Gets or sets the workflow type unique identifier.
        /// </summary>
        /// <value>
        /// The workflow type unique identifier.
        /// </value>
        public Guid WorkflowTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        /// <value>
        /// The name of the workflow.
        /// </value>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid )
        {
            WorkflowTypeGuid = workflowTypeGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowTransaction"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        public LaunchWorkflowTransaction( Guid workflowTypeGuid, int entityId ) : this( workflowTypeGuid )
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            T entity = GetEntity();

            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                var workflowType = workflowTypeService.Get( WorkflowTypeGuid );

                if ( workflowType != null )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, WorkflowName );

                    List<string> workflowErrors;
                    new Rock.Model.WorkflowService( rockContext ).Process( workflow, entity, out workflowErrors );
                }
            }
        }

        private T GetEntity()
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