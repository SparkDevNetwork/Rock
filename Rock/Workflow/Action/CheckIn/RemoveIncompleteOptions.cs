//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes any options that do not have an active group type, location, group and schedule
    /// </summary>
    [Description("Removes any options that do not have an active group type, location, group and schedule")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Remove Incomplete Options" )]
    public class RemoveIncompleteOptions : ActionComponent
    {
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            throw new NotImplementedException();
        }
    }
}