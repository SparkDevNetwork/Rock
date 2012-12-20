//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Marks a workflow as complete
    /// </summary>
    [Description( "Marks the workflow as complete" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Complete Workflow" )]
    public class CompleteWorkflow : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, IEntity entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            action.Activity.Workflow.MarkComplete();
            action.AddLogEntry( "Marked workflow complete" );

            return true;
        }
    }
}