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
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds an activity to a workflow type or updates an existing one.
    /// </summary>
    /// <remarks>
    /// An activity is a group of actions that run together. Order matters, because
    /// activities run in sequence unless an action or a form button redirects the
    /// workflow, which is why this positions by naming a neighbour rather than by
    /// taking a number.
    /// </remarks>
    [Description( "Adds an activity to a workflow type, or updates an existing one. An activity is a group of actions that run together." )]
    [AgentUsage( "name is required when adding. Supplying activityTypeIdKey updates that activity and leaves any parameter you omit unchanged. Supply at most one of insertAfterActivityTypeIdKey or insertBeforeActivityTypeIdKey." )]
    [AgentUsage( "At least one activity in a workflow needs isActivatedWithWorkflow set to true, or the workflow starts and does nothing." )]
    [AgentToolPrerequisite( "Call LookupWorkflowTypes to determine the workflowTypeIdKey, then GetWorkflowTypeConfiguration to see the existing activities before positioning a new one." )]
    [AgentToolGuid( "85129AA8-724E-4BAB-BBAB-7DF9253F11DE" )]
    public AgentToolResult AddOrUpdateWorkflowActivityType(
        [Description( "Required when editing an existing activity." )]
        string activityTypeIdKey = null,
        [Description( "Required when adding a new activity." )]
        string workflowTypeIdKey = null,
        string name = null,
        SetOrClear<string> description = null,
        bool? isActive = null,
        [Description( "Whether this activity starts as soon as the workflow starts. Defaults to false, because only the first activity normally sets it and defaulting it true makes every activity fire at once." )]
        bool? isActivatedWithWorkflow = null,
        [Description( "The key of the activity this one should follow." )]
        string insertAfterActivityTypeIdKey = null,
        [Description( "The key of the activity this one should precede." )]
        string insertBeforeActivityTypeIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var activityTypeService = new WorkflowActivityTypeService( rockContext );

        var placement = ResolveSiblingPlacement( insertAfterActivityTypeIdKey, insertBeforeActivityTypeIdKey, helper );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        WorkflowActivityType activityType;
        Rock.Model.WorkflowType workflowType;
        var isNew = activityTypeIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            activityType = helper.GetRequiredEntity<WorkflowActivityType>( activityTypeIdKey );

            if ( activityType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowTypeConfiguration )} function to determine the available activities." );
            }

            // The parent comes from the activity on an update, so a caller holding
            // only the activity key does not have to supply it. When it is supplied
            // anyway it is checked rather than trusted, because a mismatch means the
            // caller is working from a stale read of a different workflow.
            workflowType = activityType.WorkflowType;

            if ( workflowTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var suppliedWorkflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

                if ( suppliedWorkflowType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( WorkflowSkill.LookupWorkflowTypes )} function to determine the available workflow types." );
                }

                if ( activityType.WorkflowTypeId != suppliedWorkflowType.Id )
                {
                    return Error( $"The activity '{activityType.Name}' does not belong to the workflow type '{suppliedWorkflowType.Name}'." )
                        .WithInstructions( $"Call the {nameof( GetWorkflowTypeConfiguration )} function to determine which activities belong to this workflow type." );
                }
            }
        }
        else
        {
            if ( workflowTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( workflowTypeIdKey )} is required when adding an activity." )
                    .WithInstructions( $"Call the {nameof( WorkflowSkill.LookupWorkflowTypes )} function to determine the available workflow types, or supply {nameof( activityTypeIdKey )} to update an existing activity instead." );
            }

            workflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

            if ( workflowType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( WorkflowSkill.LookupWorkflowTypes )} function to determine the available workflow types." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding an activity." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            activityType = rockContext.Set<WorkflowActivityType>().Create();

            activityType.WorkflowTypeId = workflowType.Id;

            // An inactive activity never runs, which is not what someone adding
            // one is asking for. isActivatedWithWorkflow is left at its own
            // default of false on purpose; see the parameter description.
            activityType.IsActive = true;

            activityTypeService.Add( activityType );
        }

        helper.UpdateProperty( activityType, at => at.Name, name );
        helper.UpdateProperty( activityType, at => at.Description, description );
        helper.UpdateProperty( activityType, at => at.IsActive, isActive );
        helper.UpdateProperty( activityType, at => at.IsActivatedWithWorkflow, isActivatedWithWorkflow );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Ordering runs after the save so a newly added activity is among the
        // siblings being renumbered rather than missing from them. An update that
        // said nothing about position is left where it is, so renaming an activity
        // never moves it.
        if ( placement.IsSpecified || isNew )
        {
            var siblings = activityTypeService.Queryable()
                .Where( at => at.WorkflowTypeId == activityType.WorkflowTypeId )
                .OrderBy( at => at.Order )
                .ThenBy( at => at.Id )
                .ToList();

            PlaceAmongSiblings( siblings, siblings.First( at => at.Id == activityType.Id ), placement, at => at.Id, ( at, order ) => at.Order = order );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        var result = new WorkflowActivityTypeResult
        {
            Id = activityType.Id,
            Guid = activityType.Guid,
            Name = activityType.Name,
            Description = activityType.Description,
            Order = activityType.Order,
            IsActive = activityType.IsActive ?? true,
            IsActivatedWithWorkflow = activityType.IsActivatedWithWorkflow
        };

        return Success( result )
            .WithInstructions( isNew
                ? $"The activity has been created. Add the steps it performs with {nameof( AddOrUpdateWorkflowActionType )}."
                : "The activity has been updated." )
            .WithHistoryContent( new KeyNameResult( activityType.Id, activityType.Guid, activityType.Name ) );
    }

    #endregion
}
