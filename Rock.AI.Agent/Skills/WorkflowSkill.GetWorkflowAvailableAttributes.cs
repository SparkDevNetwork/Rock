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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class WorkflowSkill
    {
        #region Tool(s)

        [Description( "Gets the available attributes that can be set when adding or updating a workflow." )]
        [AgentPurpose( "Provides a list of attribute definitions for Workflows and any value format instructions." )]
        [AgentToolGuid( "b7736059-2d51-4e6f-80ef-7f1d2dbc9757" )]
        public IAgentToolResult GetWorkflowAvailableAttributes(
            string workflowIdKey = null,
            string workflowTypeIdKey = null )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            Model.Workflow workflow;

            if ( workflowIdKey.IsNotNullOrWhiteSpace() )
            {
                workflow = helper.GetRequiredEntity<Model.Workflow>( workflowIdKey, checkSecurity: true );

                if ( workflow == null )
                {
                    return helper.ErrorResult;
                }
            }
            else
            {
                var workflowType = helper.GetRequiredEntity<Model.WorkflowType>( workflowTypeIdKey, checkSecurity: true );

                if ( workflowType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( LookupWorkflowTypes )} function to determine available workflow types." );
                }
                else if ( !GetConfiguredWorkflowTypes().Any( t => t.Id == workflowType.Id ) )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"The specified workflow type is not configured for use with this skill. Call the {nameof( LookupWorkflowTypes )} function to determine available workflow types." );
                }

                workflow = new Model.Workflow
                {
                    WorkflowTypeId = workflowType.Id,
                };
            }

            workflow.LoadAttributes( AgentRequestContext.RockContext );

            return Success( helper.GetAvailableAttributes( workflow ) );
        }

        #endregion
    }
}
