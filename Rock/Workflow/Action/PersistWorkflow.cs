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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Changes an unpersisted workflow be persisted
    /// </summary>
    [Description( "Changes an unpersisted workflow be persisted" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Persist Workflow" )]

    [BooleanField( "Persist Immediately", "This action will normally cause the workflow to be persisted (saved) once all the current activites/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.",  false )]
    public class PersistWorkflow : ActionComponent
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
            workflow.IsPersisted = true;

            if ( GetAttributeValue( action, "PersistImmediately" ).AsBoolean( false ) )
            {
                var service = new WorkflowService( rockContext );
                if ( workflow.Id == 0 )
                {
                    service.Add( workflow );
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
            }

            action.AddLogEntry( "Updated workflow to be persisted!" );

            return true;
        }
    }
}