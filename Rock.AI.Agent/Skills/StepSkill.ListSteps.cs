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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.StepSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class StepSkill
{
    #region Tool(s)

    [Description( "List steps that match the filters." )]
    [AgentPurpose( "List steps that math the filters." )]
    [AgentToolGuid( "c2226aa4-6efb-4199-ad9f-9d471502b67f" )]
    public IAgentToolResult ListSteps(
        string personIdKey = null,
        string stepTypeIdKey = null,
        string stepProgramIdKey = null,

        [Description( "Only include completed steps if true, only include open steps if false, or all steps if null." )]
        bool? completed = null,

        DateTime? beginStartDate = null,
        DateTime? beginEndDate = null,
        DateTime? completedStartDate = null,
        DateTime? completedEndDate = null,

        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var stepTypeIds = StepTypeCache.All( AgentRequestContext.RockContext )
            .Where( st => st.IsActive
                && st.StepProgram.IsActive
                && st.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( st => st.Id )
            .ToList();

        var qry = new StepService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( s => stepTypeIds.Contains( s.StepTypeId ) );

        qry = helper.WhereOptionalIdKey( qry, spc => spc.PersonAlias.PersonId, personIdKey );
        qry = helper.WhereOptionalIdKey( qry, spc => spc.StepTypeId, stepTypeIdKey );
        qry = helper.WhereOptionalIdKey( qry, spc => spc.StepType.StepProgramId, stepProgramIdKey );
        qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.StartDateTime, beginStartDate, beginEndDate );
        qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.CompletedDateTime, completedStartDate, completedEndDate );

        if ( completed == true )
        {
            qry = qry.Where( s => s.CompletedDateTime.HasValue );
        }
        else if ( completed == false )
        {
            qry = qry.Where( s => !s.CompletedDateTime.HasValue );
        }

        if ( personIdKey.IsNullOrWhiteSpace() && stepTypeIdKey.IsNullOrWhiteSpace() && stepProgramIdKey.IsNullOrWhiteSpace() )
        {
            helper.AddError( $"At least one of {nameof( personIdKey )}, {nameof( stepTypeIdKey )}, or {nameof( stepProgramIdKey )} must be provided." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedQry = stepProgramIdKey.IsNullOrWhiteSpace()
            ? qry
                .OrderByDescending( s => s.EndDateTime )
                .ThenByDescending( s => s.StartDateTime )
                .ThenBy( s => s.Id )
            : qry
                .OrderBy( s => s.StepType.Order )
                .ThenBy( s => s.Id );

        var projectedQry = orderedQry
            .Select( s => new StepResult
            {
                Id = s.Id,
                StepType = new StepTypeResult
                {
                    Id = s.StepType.Id,
                    Name = s.StepType.Name,
                    StepProgram = new StepProgramResult
                    {
                        Id = s.StepType.StepProgram.Id,
                        Name = s.StepType.StepProgram.Name,
                    },
                },
                Status = s.StepStatusId.HasValue
                    ? new StepStatusResult
                    {
                        Id = s.StepStatusId.Value,
                        Name = s.StepStatus.Name,
                    }
                    : null,
                StartDateTime = s.StartDateTime,
                EndDateTime = s.EndDateTime,
                CompletedDateTime = s.CompletedDateTime,
            } );

        var page = helper.GetPaginatedItems( projectedQry, pageNumber );

        return helper.GetPaginatedResult( page );
    }

    #endregion
}
