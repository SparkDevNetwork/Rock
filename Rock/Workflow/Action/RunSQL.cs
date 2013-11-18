//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [Description( "Runs the specified SQL query to perform an action against the database." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Run SQL" )]
    [TextField( "SQLQuery", "The SQL query to run" )]
    public class RunSQL : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var query = GetAttributeValue( action, "SQLQuery" );
            int rows = new Service().ExecuteCommand( query, new object[] { } );

            action.AddLogEntry( "SQL query has been run" );

            return true;
        }
    }
}
