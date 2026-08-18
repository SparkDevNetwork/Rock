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
using Rock.AI.Agent.Classes.Skills.LearningManagementSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal partial class LearningManagementSkill
{
    #region Tool(s)

    [Description( "Retrieves the LMS programs and courses configured in Rock." )]
    [AgentPurpose( "Retrieves the LMS programs and courses configured in Rock." )]
    [AgentToolGuid( "c476d5c7-ef21-4557-9d76-5e639496ba74" )]
    public AgentToolResult LookupPrograms()
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var learningProgramResults = new LearningProgramService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( lp => lp.LearningCourses )
            .Where( lp => lp.IsActive )
            .ToList()
            .Where( lp => lp.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .OrderBy( lp => lp.Name )
            .Select( lp => new LearningProgramResult
            {
                Id = lp.Id,
                Guid = lp.Guid,
                Name = lp.Name,
                Courses = lp.LearningCourses
                    .Where( lc => lc.IsActive
                        && lc.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
                    .Select( r => new LearningCourseResult
                    {
                        Id = r.Id,
                        Guid = r.Guid,
                        Name = r.Name,
                    } )
                    .ToList(),
            } )
            .ToList();

        var result = Success( learningProgramResults );

        if ( learningProgramResults.SelectMany( lp => lp.Courses ).Count() > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
