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
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

using MetricResult = Rock.AI.Agent.Classes.Skills.MetricSkill.MetricResult;

namespace Rock.AI.Agent.Skills;

internal partial class MetricSkill
{
    #region Tool(s)

    [Description( "Retrieves the metrics configured in Rock." )]
    [AgentPurpose( "Retrieves the metrics configured in Rock." )]
    [AgentToolGuid( "5f38b6bf-b747-4a0f-a277-11776d2a3ff1" )]
    public IAgentToolResult LookupMetrics()
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var metricCategoryGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.Categories, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        var metricResults = new MetricCategoryService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( mc => mc.Metric )
            .Where( mc => metricCategoryGuids.Contains( mc.Category.Guid ) )
            .ToList()
            .Where( mc => mc.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( mc => mc.Metric )
            .DistinctBy( m => m.Id )
            .OrderBy( m => m.Title )
            .Select( m => new MetricResult
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ChampionPerson = PersonResult.NameOnly( m.MetricChampionPersonAlias ),
            } )
            .ToList();

        var result = Success( metricResults );

        if ( metricResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
