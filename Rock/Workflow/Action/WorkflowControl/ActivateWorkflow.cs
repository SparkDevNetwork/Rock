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
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Activates all the actions for the current action's activity.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Activates a new workflow with the provided attribute values." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Workflow" )]

    [TextField( "Workflow Name", "The name of your new workflow", true, order: 1 )]
    [WorkflowTypeField( "Workflow Type", "The workflow type to activate.  To set the Workflow Type from an Attribute, leave this blank and set Workflow Type from Attribute.", false, false, order: 2 )]
    [WorkflowAttribute( "Workflow Type from Attribute", "The workflow type to activate. Either this or Workflow Type must be set.", false, fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.WorkflowTypeFieldType" }, order: 3 )]
    [KeyValueListField( "Workflow Attribute Key", "Used to match the current workflow's attribute keys to the keys of the new workflow. The new workflow will inherit the attribute values of the keys provided.", false, keyPrompt: "Source Attribute", valuePrompt: "Target Attribute", order: 4 )]
    [WorkflowAttribute( "Workflow Attribute", "The attribute to hold the new activated workflow. ", false, "", "", 5, null, new string[] { "Rock.Field.Types.WorkflowFieldType" } )]
    public class ActivateWorkflow : ActionComponent
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
            var workflowTypeGuid = GetAttributeValue( action, "WorkflowType" ).AsGuidOrNull();
            var workflowTypeFromAttributeGuid = GetAttributeValue( action, "WorkflowTypefromAttribute", true ).AsGuidOrNull();
            var workflowName = GetAttributeValue( action, "WorkflowName" );

            WorkflowTypeCache workflowType = null;

            if ( workflowTypeGuid.HasValue )
            {
                workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
            }
            else if ( workflowTypeFromAttributeGuid.HasValue )
            {
                workflowType = WorkflowTypeCache.Get( workflowTypeFromAttributeGuid.Value );
            }

            if ( workflowType == null )
            {
                errorMessages.Add( "Workflow type is required" );
                return false;
            }

            if ( string.IsNullOrEmpty( workflowName ) )
            {
                errorMessages.Add( "Workflow name is required" );
                return false;
            }

            if ( !( workflowType.IsActive ?? true ) )
            {
                errorMessages.Add( string.Format( "Workflow type {0} is not active", workflowType ) );
                return true;
            }

            Dictionary<string, string> sourceKeyMap = null;
            var workflowAttributeKeys = GetAttributeValue( action, "WorkflowAttributeKey" );
            if ( !string.IsNullOrWhiteSpace( workflowAttributeKeys ) )
            {
                // TODO Find a way upstream to stop an additional being appended to the value
                sourceKeyMap = workflowAttributeKeys.AsDictionaryOrNull();
            }

            sourceKeyMap = sourceKeyMap ?? new Dictionary<string, string>();

            var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );
            workflow.LoadAttributes( rockContext );
            var newWorkFlowAttr = SetWorkflowAttributeValue( action, "WorkflowAttribute", workflow.Guid );

            foreach ( var keyPair in sourceKeyMap )
            {
                // Does the source key exist as an attribute in the source workflow?
                if ( action.Activity.Workflow.Attributes.ContainsKey( keyPair.Key ) )
                {
                    if ( workflow.Attributes.ContainsKey( keyPair.Value ) )
                    {
                        var value = action.Activity.Workflow.AttributeValues[keyPair.Key].Value;
                        workflow.SetAttributeValue( keyPair.Value, value );
                    }
                    else
                    {
                        errorMessages.Add( string.Format( "'{0}' is not an attribute key in the activated workflow: '{1}'", keyPair.Value, workflow.Name ) );
                    }
                }
                else
                {
                    errorMessages.Add( string.Format( "'{0}' is not an attribute key in this workflow: '{1}'", keyPair.Key, action.Activity.Workflow.Name ) );
                }
            }

            List<string> workflowErrorMessages = new List<string>();
            new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrorMessages );
            errorMessages.AddRange( workflowErrorMessages );

            return true;
        }
    }
}