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
    /// Removes the grouptypes from each family member that are not specific to their age
    /// </summary>
    [Description("Removes the grouptypes from each family member that are not specific to their age")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Filter By Age" )]
    public class FilterByAge : ActionComponent
    {
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            throw new NotImplementedException();
        }
    }
}