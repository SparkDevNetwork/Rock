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
using Rock.AI.Agent.Classes.Skills.StepSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class StepSkill
{
    #region Tool(s)

    [Description( "Retrieves the step programs and step types for each program." )]
    [AgentPurpose( "Retrieves the configuration for the step programs and step types within each program." )]
    [AgentToolGuid( "70dfbfee-7231-4762-90b9-916c1c0108bd" )]
    public IAgentToolResult LookupStepPrograms()
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var stepStatuses = new StepStatusService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( ss => ss.IsActive )
            .OrderBy( ss => ss.Order )
            .GroupBy( ss => ss.StepProgramId )
            .ToDictionary( g => g.Key,
                g => g.Select( ss => new StepStatusResult
                {
                    Id = ss.Id,
                    Name = ss.Name,
                    IndicatesStepCompleted = ss.IsCompleteStatus,
                } )
                .ToList() );

        var stepProgramResults = StepProgramCache.All( AgentRequestContext.RockContext )
            .Where( sp => sp.IsActive
                && sp.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .OrderBy( sp => sp.Name )
            .Select( sp => new StepProgramResult
            {
                Id = sp.Id,
                Name = sp.Name,
                StepTypes = sp.StepTypes
                    .OrderBy( st => st.Order )
                    .Select( s => new StepTypeResult
                    {
                        Id = s.Id,
                        Name = s.Name
                    } )
                    .ToList(),
                StepStatuses = stepStatuses[sp.Id],
            } )
            .ToList();

        var result = Success( stepProgramResults );

        if ( stepProgramResults.SelectMany( sp => sp.StepTypes ).Count() > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
