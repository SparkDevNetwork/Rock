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
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Removes an activity from a workflow type, along with every action inside it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The actions go with the activity by database cascade. Two things do not. The
    /// forms attached to those actions are never reached, and the activity's
    /// execution history is not either, because <c>WorkflowActivity.ActivityType</c>
    /// is declared with <c>WillCascadeOnDelete( false )</c>. Removing the instance
    /// rows also takes their actions with them, since those do cascade from the
    /// instance side.
    /// </para>
    /// <para>
    /// Form buttons elsewhere in the workflow that activated this activity are left
    /// pointing at nothing. A button stores its target inside a delimited column
    /// that nothing indexes, so there is no reference to find or repair.
    /// </para>
    /// </remarks>
    [Description( "Removes an activity from a workflow type, along with every action inside it and its execution history across every existing workflow instance." )]
    [AgentUsage( "Offer deactivation first. Setting isActive to false on the activity stops it running and is fully reversible, which is usually what someone means by getting rid of a step." )]
    [AgentGuardrail( "This permanently deletes the activity, every action inside it, and the execution history of that activity across all existing workflow instances. Deactivating the activity is the non-destructive alternative. Confirm with the person before proceeding." )]
    [AgentToolPrerequisite( "Call GetWorkflowType to determine the activityTypeIdKey and to see what the activity contains." )]
    [AgentToolGuid( "02164475-C6BB-4F8A-822C-BC37FEA77F03" )]
    public AgentToolResult DeleteWorkflowActivityType( string activityTypeIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var activityTypeService = new WorkflowActivityTypeService( rockContext );
        var activityService = new WorkflowActivityService( rockContext );

        var activityType = helper.GetRequiredEntity<WorkflowActivityType>( activityTypeIdKey );

        if ( activityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available activities." );
        }

        var activityName = activityType.Name;
        var workflowTypeId = activityType.WorkflowTypeId;
        var actionTypes = activityType.ActionTypes.ToList();

        // Counted before anything is removed, because after the save there is
        // nothing left to count.
        var actionCount = actionTypes.Count;
        var formCount = actionTypes.Count( at => at.WorkflowFormId.HasValue );
        var instanceQuery = activityService.Queryable().Where( a => a.ActivityTypeId == activityType.Id );
        var instanceCount = instanceQuery.Count();

        var attributeIds = GetActivityAttributes( activityType.Id, rockContext )
            .Select( a => a.Id )
            .ToList();

        var attributeCount = attributeIds.Count;

        DeleteFormsForActionTypes( actionTypes, rockContext );

        // The activity's own attributes have no foreign key to it, so nothing removes
        // them. Their stored values cascade from the attribute.
        if ( attributeCount > 0 )
        {
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.Queryable().Where( a => attributeIds.Contains( a.Id ) ).ToList();

            attributeService.DeleteRange( attributes );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        DeleteInstancesInBatches( activityService, instanceQuery, rockContext );

        if ( !activityTypeService.CanDelete( activityType, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        activityTypeService.Delete( activityType );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var siblings = activityTypeService.Queryable()
            .Where( at => at.WorkflowTypeId == workflowTypeId )
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
            Name = activityName,
            DeletedActionCount = actionCount,
            DeletedFormCount = formCount,
            DeletedAttributeCount = attributeCount,
            DeletedInstanceCount = instanceCount,
            WorkflowTypeIdKey = workflowTypeId.AsIdKey()
        } );
    }

    #endregion
}
