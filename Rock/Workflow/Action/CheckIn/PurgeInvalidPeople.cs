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
    /// Removes family members without any active group types
    /// </summary>
    [Description("Removes family members without any active group types")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Purge Invalid People" )]
    public class PurgeInvalidPeople : ActionComponent
    {
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            throw new NotImplementedException();
        }
    }
}