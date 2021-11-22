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
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Bulk Deletes the workflows
    /// </summary>
    public sealed class DeleteWorkflows : BusStartedTask<DeleteWorkflows.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            while ( message.WorkflowIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowIdSet = message.WorkflowIds.Take( 100 ).ToList();
                    message.WorkflowIds = message.WorkflowIds.Skip( 100 ).ToList();

                    var workflowService = new WorkflowService( rockContext );

                    var qry = workflowService.GetByIds( workflowIdSet );

                    foreach ( var workflow in qry )
                    {
                        workflowService.Delete( workflow );
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the list of workflow identifiers to delete.
            /// </summary>
            /// <value>
            /// The list of workflow to delete.
            /// </value>
            public List<int> WorkflowIds { get; set; }
        }
    }
}