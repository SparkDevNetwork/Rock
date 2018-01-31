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
    /// Marks a workflow as complete
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Marks the workflow as complete" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Workflow Complete" )]

    [WorkflowTextOrAttribute( "Status", "Status Attribute", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", false, "Completed", "", 0, "Status")]   
    public class CompleteWorkflow : ActionComponent
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

            string status = GetAttributeValue( action, "Status", true ).ResolveMergeFields( GetMergeFields( action ) );
            action.Activity.Workflow.MarkComplete( status );

            action.AddLogEntry( "Marked workflow complete" );

            return true;
        }
    }
}