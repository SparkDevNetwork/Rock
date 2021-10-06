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

using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class ConnectionWorkflow
    {
        /// <summary>
        /// Save hook implementation for <see cref="ConnectionWorkflow"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ConnectionWorkflow>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
                    DeleteConnectionRequestWorkflows();
                }

                base.PreSave();
            }

            /// <summary>
            /// Deletes any connection request workflows tied to this connection workflow.
            /// </summary>
            private void DeleteConnectionRequestWorkflows()
            {
                var rockContext = ( RockContext ) this.RockContext;
                var connectionRequestWorkflowService = new ConnectionRequestWorkflowService( rockContext );
                var connectionRequestWorkflows = connectionRequestWorkflowService.Queryable().Where( c => c.ConnectionWorkflowId == Entity.Id );

                if ( connectionRequestWorkflows.Any() )
                {
                    rockContext.BulkDelete( connectionRequestWorkflows );
                }
            }
        }
    }
}
