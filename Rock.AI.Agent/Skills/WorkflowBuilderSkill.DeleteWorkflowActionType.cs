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
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Removes one action from a workflow activity.
    /// </summary>
    /// <remarks>
    /// Three things have to happen by hand, in order. The action's form is not
    /// reached by any cascade. Its execution history is not either, because
    /// <c>WorkflowAction.ActionType</c> is declared with
    /// <c>WillCascadeOnDelete( false )</c>, so the generated <c>CanDelete</c> refuses
    /// while any instance row remains. Only then can the action go, after which the
    /// surviving siblings are renumbered so the activity has no gap.
    /// </remarks>
    [Description( "Removes one action from a workflow activity, along with its form and its execution history across every existing workflow instance." )]
    [AgentUsage( "Report the execution history count to the person before calling this. It is the only signal that history is about to disappear." )]
    [AgentGuardrail( "This permanently deletes the action and its execution history across all existing workflow instances. Confirm with the person before proceeding." )]
    [AgentToolPrerequisite( "Call GetWorkflowType to determine the actionTypeIdKey." )]
    [AgentToolGuid( "31D70F62-B03A-47F7-8086-1C00637847CB" )]
    public AgentToolResult DeleteWorkflowActionType( string actionTypeIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var actionTypeService = new WorkflowActionTypeService( rockContext );
        var actionService = new WorkflowActionService( rockContext );

        var actionType = helper.GetRequiredEntity<WorkflowActionType>( actionTypeIdKey );

        if ( actionType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available actions." );
        }

        var actionName = actionType.Name;
        var activityTypeId = actionType.ActivityTypeId;
        var hasForm = actionType.WorkflowFormId.HasValue;

        // Counted before anything is removed. After the save there is nothing left
        // to count, and this figure is the only report the person gets of how much
        // history went with the action.
        var instanceQuery = actionService.Queryable().Where( a => a.ActionTypeId == actionType.Id );
        var instanceCount = instanceQuery.Count();

        DeleteFormsForActionTypes( new List<WorkflowActionType> { actionType }, rockContext );

        DeleteInstancesInBatches( actionService, instanceQuery, rockContext );

        if ( !actionTypeService.CanDelete( actionType, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        actionTypeService.Delete( actionType );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Renumbered so the activity's sequence has no gap. Nothing depends on the
        // numbers being contiguous, but a gap reads as a missing step.
        var siblings = actionTypeService.Queryable()
            .Where( at => at.ActivityTypeId == activityTypeId )
            .OrderBy( at => at.Order )
            .ThenBy( at => at.Id )
            .ToList();

        for ( var index = 0; index < siblings.Count; index++ )
        {
            siblings[index].Order = index;
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new
        {
            IsDeleted = true,
            Name = actionName,
            DeletedFormCount = hasForm ? 1 : 0,
            DeletedInstanceCount = instanceCount,
            ActivityTypeIdKey = activityTypeId.AsIdKey()
        } );
    }

    #endregion
}
