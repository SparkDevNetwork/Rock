// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Web.Compilation;

using Rock.Data;

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
            var rockContext = (RockContext)this.Context;

            if ( workflow.IsPersisted )
            {
                workflow.IsProcessing = true;
                rockContext.SaveChanges();
            }

            bool result = workflow.ProcessActivities( rockContext, entity, out errorMessages );

            if ( workflow.IsPersisted || workflow.WorkflowType.IsPersisted )
            {
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

                workflow.IsProcessing = false;
                rockContext.SaveChanges();
            }

            return result;
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

    }
}