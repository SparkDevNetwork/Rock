//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets a workflow status
    /// </summary>
    [Description( "Set the workflow status" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Set Status")]
    [TextField( "Status", "The status to set workflow to" )]
    public class SetStatus : ActionComponent
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

            string status = GetAttributeValue( action, "Status" );
            action.Activity.Workflow.Status = status;
            action.AddLogEntry( string.Format( "Set Status to '{0}'", status ) );
            
            return true;
        }
    }
}