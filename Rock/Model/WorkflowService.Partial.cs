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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Workflow"/> entity objects
    /// </summary>
    public partial class WorkflowService 
    {
        /// <summary>
        /// Processes the specified workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool Process( Workflow workflow, out List<string> errorMessages )
        {
            return Process( workflow, null, out errorMessages );
        }

        /// <summary>
        /// Processes the specified <see cref="Rock.Model.Workflow" />
        /// </summary>
        /// <param name="workflow">The <see cref="Rock.Model.Workflow" /> instance to process.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}" /> that contains any error messages that were returned while processing the <see cref="Rock.Model.Workflow" />.</param>
        /// <returns></returns>
        public bool Process( Workflow workflow, object entity, out List<string> errorMessages )
        {
            var workflowType = WorkflowTypeCache.Get( workflow.WorkflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var rockContext = (RockContext)this.Context;

                if ( workflow.IsPersisted )
                {
                    workflow.IsProcessing = true;
                    rockContext.SaveChanges();
                }

                bool result = workflow.ProcessActivities( rockContext, entity, out errorMessages );

                if ( workflow.Status == "DeleteWorkflowNow" )
                {
                    if ( workflow.Id > 0 )
                    {
                        rockContext.SaveChanges();
                        Delete( workflow );
                        rockContext.SaveChanges();
                    }
                    result = true;
                }
                else
                {
                    if ( workflow.IsPersisted || workflowType.IsPersisted )
                    {
                        if ( workflow.Id == 0 )
                        {
                            Add( workflow );
                        }

                        rockContext.SaveChanges();

                        workflow.SaveAttributeValues( rockContext );
                        foreach ( var activity in workflow.Activities )
                        {
                            activity.SaveAttributeValues( rockContext );
                        }

                        workflow.IsProcessing = false;
                        rockContext.SaveChanges();
                    }
                }

                return result;
            }

            else
            {
                errorMessages = new List<string> { "Workflow Type is invalid or not active!" };
                return false;
            }

        }

        /// <summary>
        /// Gets the active <see cref="Rock.Model.Workflow">Workflows</see>.
        /// </summary>
        /// <returns>A queryable collection of active <see cref="Rock.Model.Workflow"/>entities ordered by LastProcessedDate.</returns>
        public IQueryable<Workflow> GetActive()
        {
            return this.Queryable()
                .Where( w =>
                    w.ActivatedDateTime.HasValue &&
                    !w.CompletedDateTime.HasValue )
                .OrderBy( w => w.LastProcessedDateTime );
        }

        /// <summary>
        /// Persists the workflow immediately. Do this if the next actions need a persisted workflow with Ids.
        /// </summary>
        /// <param name="action">The action.</param>
        public void PersistImmediately( WorkflowAction action )
        {
            var rockContext = ( RockContext ) this.Context;

            var workflow = action.Activity.Workflow;
            workflow.IsPersisted = true;
            workflow.IsProcessing = true;

            if ( workflow.Id == 0 )
            {
                Add( workflow );
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                workflow.SaveAttributeValues( rockContext );
                foreach ( var activity in workflow.Activities )
                {
                    activity.SaveAttributeValues( rockContext );
                }
            } );

            action.AddLogEntry( "Workflow has been persisted!" );
        }
    }
}