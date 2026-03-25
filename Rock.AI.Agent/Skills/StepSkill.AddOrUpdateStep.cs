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
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class StepSkill
{
    #region Tool(s)

    [Description( "Adds a new or updates an existing step." )]
    [AgentToolGuid( "9c7184d7-2bea-4e40-9ce3-ffaa339e2d10" )]
    public IAgentToolResult AddOrUpdateStep(
        string stepIdKey = null,

        string stepTypeIdKey = null,
        string personIdKey = null,
        string stepStatusIdKey = null,
        string campusIdKey = null,
        DateTime? startDateTime = null,
        DateTime? endDateTime = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        Step step;

        if ( stepIdKey.IsNotNullOrWhiteSpace() )
        {
            step = helper.GetRequiredEntity<Step>( stepIdKey );
        }
        else
        {
            step = rockContext.Set<Step>().Create();
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        helper.UpdateNavigationProperty( step, s => s.StepType, stepTypeIdKey );
        helper.UpdateNavigationProperty( step, s => s.PersonAlias, personIdKey );
        helper.UpdateNavigationProperty( step, s => s.StepStatus, stepStatusIdKey );
        helper.UpdateNavigationProperty( step, s => s.Campus, campusIdKey );
        helper.UpdateProperty( step, s => s.StartDateTime, startDateTime );
        helper.UpdateProperty( step, s => s.EndDateTime, endDateTime );

        if ( step.Id == 0 )
        {
            if ( step.StepTypeId == 0 )
            {
                helper.AddError( $"{nameof( stepTypeIdKey )} is required when creating a new step." );
            }

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

            // Unlike the normal pattern, this must be here because there is
            // logic in the Add method that makes sure all the properties
            // have been correctly configured.
            new StepService( rockContext ).Add( step );
        }

        if ( stepStatusIdKey.IsNotNullOrWhiteSpace() )
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
}
