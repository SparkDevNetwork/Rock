//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;

using Rock.Cms;
using Rock.Web.UI;

namespace Rock.Util.WorkflowAction
{
    /// <summary>
    /// Sets a workflow status
    /// </summary>
    [Description( "Set the workflow status" )]
    [Export(typeof(WorkflowActionComponent))]
    [ExportMetadata("ComponentName", "Set Status")]
    [BlockProperty( 1, "Status", "Status", "The status to set workflow to", true, "" )]
    public class SetStatus : WorkflowActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool Execute(Action action)
        {
            action.Activity.Workflow.Status = action.AttributeValues["Status"][0].Value;
            return true;
        }
    }
}