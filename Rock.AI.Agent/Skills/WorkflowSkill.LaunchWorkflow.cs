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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.WorkflowSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class WorkflowSkill
{
    #region Tool(s)

    [Description( "Launches a new workflow with the provides values." )]
    [AgentPurpose( "Launches a new workflow with the provides attribute values." )]
    [AgentToolGuid( "f1d4e4d1-1e92-4851-b83d-4ae7bcdfb5a2" )]
    public AgentToolResult LaunchWorkflow(
        string workflowTypeIdKey = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var workflowType = helper.GetRequiredEntity<Model.WorkflowType>( workflowTypeIdKey, checkSecurity: true );

        // An empty configuration means the agent may launch any workflow type the
        // person is authorized to view (security was already checked above). When
        // specific types are configured, launching is limited to that list.
        var launchableWorkflowTypes = GetConfiguredWorkflowTypes();

        if ( launchableWorkflowTypes.Any() && !launchableWorkflowTypes.Any( wt => wt.Id == workflowType.Id ) )
        {
            // The workflow type is valid; the agent simply has not been granted
            // permission to launch it. This is a configuration choice, not a
            // mistake in the request, so the instructions point at the fix
            // rather than suggesting the caller try a different type.
            helper.AddError( "This workflow type exists, but this agent is not configured to launch it." );
            helper.AddInstructions( "This workflow type exists and the request was well formed, but the agent's settings do not allow it to launch this type. To permit it, add this workflow type to the Launchable Workflow Types setting on the agent's Workflow skill configuration." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var workflowService = new WorkflowService( rockContext );
        var workflow = Rock.Model.Workflow.Activate( WorkflowTypeCache.Get( workflowType.Id ), null, rockContext );

        helper.SetAttributeValues( workflow, attributeValues );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( !workflowService.Process( workflow, out var errorMessages ) )
        {
            return Error( errorMessages );
        }

        var launchResult = new LaunchWorkflowResult
        {
            Id = workflowType.Id,
            Name = workflow.Name,
        };

        var publicAttributeKeys = workflow.Attributes.Values
            .Where( a => a.IsPublic )
            .Select( a => a.Key )
            .ToList();

        launchResult.AttributeValues = workflow.GetAttributeValueResults( AgentRequestContext )
            .Where( av => publicAttributeKeys.Contains( av.Key ) )
            .ToList();

        var result = workflow.Id == 0
            ? Success( launchResult )
                .WithInstructions( "Workflow was successfully launched." )
            : Success( launchResult )
                .WithInstructions( "Workflow was successfully launched, but it was not persisted so no follow up actions are available." );

        if ( launchResult.AttributeValues.Any() )
        {
            result = result.WithInstructions( "Attribute values may include result or status information about the workflow and should usually be displayed." );
        }

        return result;
    }

    #endregion
}
