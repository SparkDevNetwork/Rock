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

using System;
using System.ComponentModel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.StepSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class StepSkill
{
    #region Tool(s)

    [Description( "Adds a new or updates an existing step." )]
    [AgentToolGuid( "9c7184d7-2bea-4e40-9ce3-ffaa339e2d10" )]
    public AgentToolResult AddOrUpdateStep(
        string stepIdKey = null,

        [Description( "Only valid and required when adding a new step." )]
        string stepTypeIdKey = null,

        [Description( "Only valid and required when adding a new step." )]
        string personIdKey = null,

        string stepStatusIdKey = null,
        string campusIdKey = null,
        DateTime? startDateTime = null,
        DateTime? endDateTime = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        Step step;
        Model.StepType stepType;

        if ( stepIdKey.IsNotNullOrWhiteSpace() )
        {
            step = helper.GetRequiredEntity<Step>( stepIdKey );
            stepType = step?.StepType;

            // The step entry block does not allow the step type or person to
            // be changed once the step exists, so we match that behavior.
            if ( stepTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"A step cannot be moved to a new step type, do not provide a {nameof( stepTypeIdKey )} when editing." );
            }

            if ( personIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"A step cannot be moved to a new person, do not provide a {nameof( personIdKey )} when editing." );
            }

            // Step.IsAuthorized() also grants EDIT to anybody with EDIT or
            // MANAGE_STEPS on the step type.
            if ( step != null && !step.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                helper.AddError( "You are not authorized to edit this step." );
            }
        }
        else
        {
            step = rockContext.Set<Step>().Create();
            stepType = helper.GetRequiredEntity<Model.StepType>( stepTypeIdKey );

            if ( stepType != null && !stepType.IsActive )
            {
                helper.AddError( "Steps cannot be added to an inactive step type." );
            }

            if ( stepType != null && !IsAuthorizedToAddStep( stepType, currentPerson ) )
            {
                helper.AddError( "You are not authorized to add a step of this type." );
            }
        }

        if ( stepType != null && !stepType.AllowManualEditing )
        {
            helper.AddError( "Steps of this type cannot be manually added or edited." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( step.Id == 0 )
        {
            step.StepType = stepType;
            step.StepTypeId = stepType.Id;
            helper.UpdateNavigationProperty( step, s => s.PersonAlias, personIdKey );
        }

        helper.UpdateNavigationProperty( step, s => s.StepStatus, stepStatusIdKey );
        helper.UpdateNavigationProperty( step, s => s.Campus, campusIdKey );
        helper.UpdateProperty( step, s => s.StartDateTime, startDateTime );
        helper.UpdateProperty( step, s => s.EndDateTime, endDateTime );

        // Statuses belong to the program, so make sure the selected status
        // is one that is valid for this step type.
        if ( stepStatusIdKey.IsNotNullOrWhiteSpace() && step.StepStatus != null && step.StepStatus.StepProgramId != stepType.StepProgramId )
        {
            helper.AddError( $"The {nameof( stepStatusIdKey )} is not valid for this step type." );
        }

        if ( step.Id == 0 )
        {
            if ( step.PersonAliasId == 0 )
            {
                helper.AddError( $"{nameof( personIdKey )} is required when creating a new step." );
            }

            if ( !step.StepStatusId.HasValue )
            {
                helper.AddError( $"{nameof( stepStatusIdKey )} is required when creating a new step." );
            }

            if ( !step.StartDateTime.HasValue )
            {
                step.StartDateTime = RockDateTime.Now;
            }

            // StepService.Add() throws if the step is not valid, so return
            // any errors before calling it.
            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Unlike the normal pattern, this must be here because there is
            // logic in the Add method that makes sure all the properties
            // have been correctly configured.
            new StepService( rockContext ).Add( step );
        }

        if ( stepStatusIdKey.IsNotNullOrWhiteSpace() && step.StepStatus != null )
        {
            if ( step.StepStatus.IsCompleteStatus && !step.CompletedDateTime.HasValue )
            {
                step.CompletedDateTime = endDateTime ?? RockDateTime.Now;
            }
        }

        var isNew = step.Id == 0;

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var stepResult = new StepResult
        {
            Id = step.Id,
            StepType = new StepTypeResult
            {
                Id = step.StepType.Id,
                Name = step.StepType.Name,
            },
            Status = new StepStatusResult
            {
                Id = step.StepStatus.Id,
                Name = step.StepStatus.Name,
            },
            StartDateTime = step.StartDateTime,
            EndDateTime = step.EndDateTime,
            CompletedDateTime = step.CompletedDateTime,
        };

        return Success( stepResult )
            .WithHistoryContent( new KeyNameResult
            {
                Id = step.Id,
            } )
            .WithInstructions( $"The step has been {( isNew ? "added" : "updated" )}." );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines if the person is allowed to add new steps of the specified
    /// type. This matches the logic used by the step entry block.
    /// </summary>
    /// <param name="stepType">The step type the new step will belong to.</param>
    /// <param name="person">The person that is adding the step.</param>
    /// <returns><c>true</c> if the person can add a step of this type; otherwise <c>false</c>.</returns>
    private static bool IsAuthorizedToAddStep( Model.StepType stepType, Model.Person person )
    {
        return stepType.IsAuthorized( Authorization.EDIT, person )
            || stepType.IsAuthorized( Authorization.MANAGE_STEPS, person );
    }

    #endregion
}
