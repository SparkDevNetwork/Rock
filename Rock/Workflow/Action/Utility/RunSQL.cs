// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Runs the specified SQL query to perform an action against the database." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "SQL Run" )]
    [CodeEditorField( "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", Web.UI.Controls.CodeEditorMode.Sql, Web.UI.Controls.CodeEditorTheme.Rock, 400, true, "", "", 0 )]
    [KeyValueListField( "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", false, "", "Parameter", "", order: 1 )]
    [WorkflowAttribute( "Result Attribute", "An optional attribute to set to the scaler result of SQL query.", false, "", "", 2 )]
    [BooleanField( "Continue On Error", "Should processing continue even if SQL Error occurs?", false, "", 3 )]
    public class RunSQL : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var query = GetAttributeValue( action, "SQLQuery" );
            var parametersValue = GetAttributeValue( action, "Parameters" );
            var parameterList = new Field.Types.KeyValueListFieldType().GetValuesFromString( null, parametersValue, null, false );

            var mergeFields = GetMergeFields( action );
            query = query.ResolveMergeFields( mergeFields );

            var parameters = new Dictionary<string, object>();
            foreach ( var p in parameterList )
            {
                var value = p.Value != null ? p.Value.ToString().ResolveMergeFields( mergeFields ) : null;

                parameters.AddOrReplace( p.Key, value );
            }

            try
            {
                object sqlResult = DbService.ExecuteScaler( query, System.Data.CommandType.Text, parameters );
                action.AddLogEntry( "SQL query has been run" );

                if ( sqlResult != null )
                {
                    string resultValue = sqlResult.ToString();
                    var attribute = SetWorkflowAttributeValue( action, "ResultAttribute", resultValue );
                    if ( attribute != null )
                    {
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, resultValue ) );
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                action.AddLogEntry( ex.Message, true );

                if ( !GetAttributeValue( action, "ContinueOnError" ).AsBoolean() )
                {
                    errorMessages.Add( ex.Message );
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }
    }
}
