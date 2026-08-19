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
using Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventRegistrationSkill
{
    #region Tool(s)

    [Description( "Retrieves all registration templates in Rock." )]
    [AgentPurpose( "Retrieves all registration templates in Rock." )]
    [AgentToolGuid( "ec52ca7b-7dd0-4947-9bf2-1930b3731acf" )]
    public AgentToolResult LookupRegistrationTemplates()
    {
        var templateResults = new RegistrationTemplateService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( rt => rt.IsActive )
            .ToList()
            .Where( rt => rt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( rt => new RegistrationTemplateResult
            {
                Id = rt.Id,
                Guid = rt.Guid,
                Name = rt.Name,
            } )
            .OrderBy( kn => kn.Name )
            .ToList();

        var result = Success( templateResults );

        if ( templateResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
