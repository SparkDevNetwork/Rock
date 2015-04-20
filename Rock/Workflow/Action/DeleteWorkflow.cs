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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected value
    /// </summary>
    [Description( "Deletes the current workflow instance." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Delete Workflow" )]

    public class DeleteWorkflow : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var workflow = action.Activity.Workflow;
            if ( workflow.Id >= 0 )
            {
                // Create a new RockContext so that any previous updates are ignored and
                // workflow can be sucessfully deleted
                var newRockContext = new RockContext();
                var workflowService = new WorkflowService( newRockContext );
                var workflowToDelete = workflowService.Get( workflow.Id );
                if ( workflowToDelete != null )
                {
                    workflowService.Delete( workflowToDelete );
                    newRockContext.SaveChanges();
                }
            }

            workflow.Id = 0;
            workflow.IsPersisted = false;

            return true;
        }

    }
}