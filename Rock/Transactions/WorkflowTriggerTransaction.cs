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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public class WorkflowTriggerTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the workflow trigger.
        /// </summary>
        /// <value>
        /// The workflow trigger.
        /// </value>
        public WorkflowTrigger Trigger { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( Trigger != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowType = WorkflowTypeCache.Get(Trigger.WorkflowTypeId );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        var workflow = Rock.Model.Workflow.Activate( workflowType, Trigger.WorkflowName );

                        List<string> workflowErrors;
                        new Rock.Model.WorkflowService( rockContext ).Process( workflow, Entity, out workflowErrors );
                    }
                }
            }
        }
    }
}