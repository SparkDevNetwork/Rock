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
//

using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.StepSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class StepSkill
{
    #region Tool(s)

    [Description( "Lists completed step programs." )]
    [AgentPurpose( "Retrieves the completed step programs." )]
    [AgentToolGuid( "6cc0233d-3897-4f9a-9fea-e530094d40d3" )]
    public IAgentToolResult ListStepProgramCompletions(
        string personIdKey = null,
        string stepProgramIdKey = null,

        DateTime? startDate = null,
        DateTime? endDate = null,

        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var stepProgramIds = StepProgramCache.All( AgentRequestContext.RockContext )
            .Where( sp => sp.IsActive
                && sp.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( sp => sp.Id )
            .ToList();

        var qry = new StepProgramCompletionService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( spc => stepProgramIds.Contains( spc.StepProgramId ) );

        qry = helper.WhereOptionalIdKey( qry, spc => spc.PersonAlias.PersonId, personIdKey );
        qry = helper.WhereOptionalIdKey( qry, spc => spc.StepProgramId, stepProgramIdKey );
        qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.EndDateTime, startDate, endDate );

        if ( personIdKey.IsNullOrWhiteSpace() && stepProgramIdKey.IsNullOrWhiteSpace() )
        {
            helper.AddError( $"At least one of {nameof( personIdKey )} or {nameof( stepProgramIdKey )} must be provided." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedQry = qry
            .AsExpandable()
            .Select( spc => new StepProgramCompletionResult
            {
                Id = spc.Id,
                StepProgram = new KeyNameResult
                {
                    Id = spc.StepProgram.Id,
                    Name = spc.StepProgram.Name,
                },
                Person = PersonResult.NameOnly( spc.PersonAlias ),
                Steps = spc.Steps
                    .OrderBy( s => s.StepType.Order )
                    .Select( s => new StepResult
                    {
                        CompletedDateTime = s.CompletedDateTime,
                        StepType = new StepTypeResult
                        {
                            Id = s.StepType.Id,
                            Name = s.StepType.Name,
                        },
                    } )
                    .ToList(),
                StartDateTime = spc.StartDateTime,
                EndDateTime = spc.EndDateTime,
            } )
            .OrderByDescending( spc => spc.EndDateTime )
            .ThenBy( spc => spc.Id );

        var page = helper.GetPaginatedItems( orderedQry, pageNumber );

        return helper.GetPaginatedResult( page );
    }

    #endregion
}
