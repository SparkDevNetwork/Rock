using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;

using com.centralaz.DpsMatch.Model;
using com.centralaz.DpsMatch.Data;
namespace com.centralaz.DpsMatch.Workflow.Action
{
    /// <summary>
    /// Populates the custom _com_centralaz_DpsMatch_Match table using the data from the custom Offender table and Person data.
    /// </summary>
    [ActionCategory( "com_centralaz: Dept Public Saftey" )]
    [Description( "Populates the custom _com_centralaz_DpsMatch_Match table using the data from the custom Offender table and Person data. Uses the custom stored procedure called _com_centralaz_spDpsMatch_Match." )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Populate Matches Table" )]
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
