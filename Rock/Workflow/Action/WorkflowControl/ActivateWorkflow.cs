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

using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Activates all the actions for the current action's activity.
    /// </summary>
    [ActionCategory("Workflow Control")]
    [Description("Activates a new workflow with the provided attribute values.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Activate Workflow")]

    [TextField("Workflow Name", "The name of your new workflow", true)]
    [WorkflowTypeField("Workflow Type", "The workflow type to activate", false, true)]
    [KeyValueListField("Workflow Attribute Key", "Used to match the current workflow's attribute keys to the keys of the new workflow. The new workflow will inherit the attribute values of the keys provided.", false, keyPrompt: "Source Attribute", valuePrompt: "Target Attribute")]

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
        public override bool Execute(RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            var workflowTypeGuid = GetAttributeValue(action, "WorkflowType").AsGuidOrNull();
            var workflowName = GetAttributeValue(action, "WorkflowName");

            WorkflowTypeCache workflowType = null;

            if ( workflowTypeGuid.HasValue )
            {
                workflowType = WorkflowTypeCache.Read( workflowTypeGuid.Value );
            }

            if ( workflowType != null && !string.IsNullOrEmpty(workflowName))
            {
                if ( !( workflowType.IsActive ?? true ) )
                {
                    errorMessages.Add( string.Format( "Workflow type {0} is not active", workflowType ) );
                    return true;
                }

                var sourceKeyMap = new Dictionary<string, string>();
                var workflowAttributeKeys = GetAttributeValue(action, "WorkflowAttributeKey");

                if (!string.IsNullOrWhiteSpace(workflowAttributeKeys))
                {
                    //TODO Find a way upstream to stop an additional being appended to the value
                    sourceKeyMap = workflowAttributeKeys.AsDictionaryOrNull();     
                    
                    var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );
                    workflow.LoadAttributes(rockContext);
                    foreach (var keyPair in sourceKeyMap)
                    {
                        //Does the source key exist as an attribute in the source workflow?
                        if (action.Activity.Workflow.Attributes.ContainsKey(keyPair.Key))
                        {
                            if (workflow.Attributes.ContainsKey(keyPair.Value))
                            {
                                var value = action.Activity.Workflow.AttributeValues[keyPair.Key].Value;
                                workflow.SetAttributeValue(keyPair.Value, value);
                            }
                            else
                            {
                                errorMessages.Add(string.Format("{0} not a key in {1}", keyPair.Value, action.Activity.Workflow.Name));
                            }
                                                    
                        }
                        else
                        {
                            errorMessages.Add(string.Format("{0} not a key in {1}", keyPair.Key, workflowName));
                        }
                    }

                    new Rock.Model.WorkflowService(rockContext).Process(workflow, out errorMessages);        
                    
                }        
            }
            else
            {
                errorMessages.Add("Workflow type or name not provided");
            }

            return true;
        }
    }
}