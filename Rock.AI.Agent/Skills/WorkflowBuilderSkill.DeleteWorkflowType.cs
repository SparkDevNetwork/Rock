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
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Removes a whole workflow type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The largest blast radius in the skill. Most of it happens through the
    /// database with no code: activities, actions, instances, and all execution
    /// history cascade from the workflow type. That is the danger, not the
    /// convenience, which is why everything is counted before the delete rather than
    /// discovered afterwards.
    /// </para>
    /// <para>
    /// Two things are not reached by the cascade and are removed here. The forms
    /// attached to user entry actions, and the workflow's own attributes, which are
    /// Attribute rows qualified by the workflow type rather than rows with a foreign
    /// key to it.
    /// </para>
    /// <para>
    /// The confirmation flag is separate from the guardrail text on purpose. The
    /// text tells the agent to ask; the flag makes the tool refuse to act until it
    /// has, so a single mistaken call cannot take a workflow and its history with
    /// it. This is the only tool in either skill with one, and that is deliberate:
    /// if every destructive tool had a flag it would become noise the model learns
    /// to satisfy automatically.
    /// </para>
    /// </remarks>
    [Description( "Removes a whole workflow type: its attributes and their stored values, its activities and actions, their forms, and every workflow instance with its entire execution history." )]
    [AgentUsage( "Call this once without isConfirmed to get the counts. Offer deactivation, which is fully reversible, then report the counts and ask the person to confirm. Only call again with isConfirmed set to true after they agree in their own words." )]
    [AgentGuardrail( "This permanently deletes the workflow type, every activity and action in it, every workflow attribute and its stored values, and every workflow instance and its entire execution history. It cannot be undone. Deactivating the workflow type is the non-destructive alternative. Confirm with the person before proceeding." )]
    [AgentToolPrerequisite( "Call LookupWorkflowTypes to determine the workflowTypeIdKey." )]
    [AgentToolGuid( "6C0A6C0E-9E24-4C1B-B60E-2B2A2A5FA7F1" )]
    public AgentToolResult DeleteWorkflowType(
        string workflowTypeIdKey,
        [Description( "Must be true for anything to be deleted. Set this only after the person has confirmed, in the conversation, that they want this workflow type and its history removed." )]
        bool isConfirmed = false )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var workflowTypeService = new WorkflowTypeService( rockContext );

        var workflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

        if ( workflowType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( WorkflowSkill.LookupWorkflowTypes )} function to determine the available workflow types." );
        }

        // ADMINISTRATE rather than EDIT, matching what Rock's own workflow type
        // block requires. This is a strictly larger action than editing one.
        if ( !workflowType.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            return Error( $"You do not have permission to delete the workflow type '{workflowType.Name}'." );
        }

        var actionTypes = workflowType.ActivityTypes
            .SelectMany( at => at.ActionTypes )
            .ToList();

        // Both scopes. The workflow's own attributes and every activity's, since
        // neither is reached by a cascade and the activities are about to go.
        var attributeIds = GetWorkflowAttributes( workflowType.Id, rockContext )
            .Select( a => a.Id )
            .ToList();

        foreach ( var activityType in workflowType.ActivityTypes )
        {
            attributeIds.AddRange( GetActivityAttributes( activityType.Id, rockContext ).Select( a => a.Id ) );
        }

        attributeIds = attributeIds.Distinct().ToList();

        // Everything is counted first. After SaveChanges the cascade has removed the
        // rows, so this is the only opportunity to say what the delete cost.
        var activityCount = workflowType.ActivityTypes.Count;
        var actionCount = actionTypes.Count;
        var formCount = actionTypes.Count( at => at.WorkflowFormId.HasValue );
        var attributeCount = attributeIds.Count;

        var storedValueCount = attributeCount > 0
            ? new AttributeValueService( rockContext ).Queryable().Count( av => attributeIds.Contains( av.AttributeId ) )
            : 0;

        var instanceCount = GetWorkflowInstanceCount( workflowType.Id, rockContext );

        if ( !isConfirmed )
        {
            return Error( $"Nothing was deleted. Deleting the workflow type '{workflowType.Name}' would permanently remove {activityCount} activity(s), {actionCount} action(s), {attributeCount} attribute(s) holding {storedValueCount} stored value(s), and {instanceCount} workflow instance(s) with their entire execution history." )
                .WithInstructions( $"Setting isActive to false with {nameof( AddOrUpdateWorkflowType )} stops the workflow running and is fully reversible; offer that first. If the person still wants it deleted, report these counts to them, get their agreement, and only then call this function again with isConfirmed set to true." );
        }

        if ( !workflowTypeService.CanDelete( workflowType, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var workflowTypeName = workflowType.Name;

        // Forms first. Nothing else will take them, and once the actions are gone
        // there is no way left to find them.
        DeleteFormsForActionTypes( actionTypes, rockContext );

        // Then the workflow's attributes, which have no foreign key to the workflow
        // type, so the cascade cannot reach them either. Their stored values do
        // cascade from the attribute.
        if ( attributeCount > 0 )
        {
            var attributeService = new AttributeService( rockContext );
            var attributeEntities = attributeService.Queryable().Where( a => attributeIds.Contains( a.Id ) ).ToList();

            attributeService.DeleteRange( attributeEntities );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        // Then the type itself, and the database cascade takes the activities,
        // actions, instances, and history.
        workflowTypeService.Delete( workflowType );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new
        {
            IsDeleted = true,
            Name = workflowTypeName,
            DeletedActivityCount = activityCount,
            DeletedActionCount = actionCount,
            DeletedFormCount = formCount,
            DeletedAttributeCount = attributeCount,
            DeletedStoredValueCount = storedValueCount,
            DeletedInstanceCount = instanceCount
        } );
    }

    #endregion
}
