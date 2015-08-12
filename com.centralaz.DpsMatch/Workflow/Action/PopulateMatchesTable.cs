using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using com.centralaz.DpsMatch.Model;
using com.centralaz.DpsMatch.Data;
namespace com.centralaz.DpsMatch.Workflow.Action
{
    /// <summary>
    /// Sets the name of the workflow
    /// </summary>
    [Description( "Sets the name of the workflow" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Workflow Name" )]
    public class PopulateMatchesTable : Rock.Workflow.ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            DbService.ExecuteCommand( "_com_centralaz_spDpsMatch_Match", System.Data.CommandType.StoredProcedure );

            return true;
        }

    }
}
