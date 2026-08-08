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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets one configured action with its values whole.
    /// </summary>
    /// <remarks>
    /// This is what makes clipping safe in the tree read. A caller that sees a
    /// value marked as truncated comes here for the real thing, so no value is ever
    /// unreachable. Its parent activity and workflow type are filled in because a
    /// caller can arrive here holding only an action key.
    /// </remarks>
    [Description( "Gets one configured workflow action with its settings and form markup returned in full, not clipped." )]
    [AgentPurpose( "Retrieves the complete value of a setting or form markup that the workflow type read reported as truncated." )]
    [AgentToolPrerequisite( "Call GetWorkflowType to determine the actionTypeIdKey." )]
    [AgentToolGuid( "B6EB95E5-80EA-4E77-A4DE-89BB327D382F" )]
    public AgentToolResult GetWorkflowActionType( string actionTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var actionType = helper.GetRequiredEntity<Rock.Model.WorkflowActionType>( actionTypeIdKey );

        if ( actionType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available actions." );
        }

        // Setting values that point at an activity or attribute in the same workflow
        // get a readable name alongside the raw value, exactly as the tree read does.
        var workflowTypeId = actionType.ActivityType?.WorkflowTypeId;

        var referenceNames = workflowTypeId.HasValue
            ? GetWorkflowReferenceNames( workflowTypeId.Value, rockContext )
            : null;

        // Not clipped. That is the whole reason this tool exists.
        var result = GetActionTypeResult( actionType, rockContext, clipLongValues: false, referenceNames );

        var activityType = actionType.ActivityType;

        if ( activityType != null )
        {
            result.ActivityType = new KeyNameResult
            {
                Id = activityType.Id,
                Guid = activityType.Guid,
                Name = activityType.Name
            };

            var workflowType = activityType.WorkflowType;

            if ( workflowType != null )
            {
                result.WorkflowType = new KeyNameResult
                {
                    Id = workflowType.Id,
                    Guid = workflowType.Guid,
                    Name = workflowType.Name
                };
            }
        }

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this workflow action." );
        }

        // A single setting here can be a forty kilobyte template, which is the whole
        // point of the tool and exactly what should not go into chat history.
        return Success( result )
            .WithHistoryKey( $"workflow-action-{actionType.IdKey}" )
            .WithoutHistoryContent();
    }

    #endregion
}
