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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets a workflow status
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Sets the workflow status in a different workflow" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Set Status (other workflow)" )]

    [TextField( "Status", "The status to set workflow to. <span class='tip tip-lava'></span>", true, "", "", 0 )]
    [WorkflowAttribute( "Workflow", "The workflow to set the status of.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.WorkflowFieldType" } )]
    public class SetStatusInOtherWorkflow : ActionComponent
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

            Guid? workflowGuid = GetAttributeValue( action, "Workflow", true ).AsGuidOrNull();
            if ( workflowGuid.HasValue )
            {
                var workflow = new WorkflowService( rockContext ).Get( workflowGuid.Value );
                if ( workflow != null )
                {
                    string status = GetAttributeValue( action, "Status" ).ResolveMergeFields( GetMergeFields( action ) );
                    workflow.Status = status;

                    action.AddLogEntry( string.Format( "Set Status to '{0}' on another workflow: {1}", status, workflow ) );

                    return true;
                }

                action.AddLogEntry( "Could not find selected workflow." );
            }

            action.AddLogEntry( "Invalid workflow setting." );

            return false;
        }
    }
}