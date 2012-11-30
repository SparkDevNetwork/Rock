//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Data;

namespace Rock.Util.WorkflowAction
{
    /// <summary>
    /// Marks a workflow as complete
    /// </summary>
    [Description( "Marks the workflow as complete" )]
    [Export(typeof(WorkflowActionComponent))]
    [ExportMetadata( "ComponentName", "Complete Workflow" )]
    public class CompleteWorkflow : WorkflowActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( Action action, IDto dto, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            action.Activity.Workflow.MarkComplete();
            action.AddLogEntry( "Marked workflow complete" );

            return true;
        }
    }
}