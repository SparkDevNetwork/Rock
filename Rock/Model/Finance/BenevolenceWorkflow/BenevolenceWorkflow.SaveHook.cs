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
    public partial class BenevolenceWorkflow
    {
        /// <summary>
        /// Save hook implementation for <see cref="BenevolenceWorkflow"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<BenevolenceWorkflow>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
                    DeleteBenevolenceRequestWorkflows();
                }

                base.PreSave();
            }

            /// <summary>
            /// Deletes any workflows tied to this benevolence type workflow.
            /// </summary>
            private void DeleteBenevolenceRequestWorkflows()
            {
                var rockContext = this.RockContext;
                var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );
                var benevolenceWorkflows = benevolenceWorkflowService.Queryable().Where( v => v.BenevolenceTypeId == Entity.BenevolenceTypeId );

                if ( benevolenceWorkflows.Any() )
                {
                    rockContext.BulkDelete( benevolenceWorkflows );
                }
            }
        }
    }
}
