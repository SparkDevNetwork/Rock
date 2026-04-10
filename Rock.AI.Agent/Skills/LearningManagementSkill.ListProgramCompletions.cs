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
using Rock.AI.Agent.Classes.Skills.LearningManagementSkill;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LearningManagementSkill
{
    #region Tool(s)

    [Description( "Returns a list of in progress or completed programs." )]
    [AgentPurpose( "Returns a list of in progress or completed programs." )]
    [AgentToolGuid( "3b726cae-a7a1-4d53-9727-1fc2c6cf60ae" )]
    public AgentToolResult ListProgramCompletions(
        string learningProgramIdKey = null,
        string personIdKey = null,
        CompletionStatus? completionStatus = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        if ( learningProgramIdKey.IsNotNullOrWhiteSpace() )
        {
            var program = helper.GetRequiredEntity<LearningProgram>( learningProgramIdKey, checkSecurity: true );

            if ( program != null && !program.IsCompletionStatusTracked )
            {
                helper.AddError( "The specified learning program does not track completion status." );
            }
        }

        var query = new LearningProgramCompletionService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( lpc => lpc.LearningProgram )
            .Where( lpc => lpc.LearningProgram.IsActive );

        query = helper.WhereOptionalIdKey( query, lpc => lpc.LearningProgramId, learningProgramIdKey );
        query = helper.WhereOptionalIdKey( query, lpc => lpc.PersonAlias.PersonId, personIdKey );
        query = helper.WhereOptionalProperty( query, lpc => lpc.CompletionStatus, completionStatus );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<LearningProgramCompletion>( currentPerson, qry => qry
            .OrderBy( lpc => lpc.EndDate )
            .ThenByDescending( lpc => lpc.StartDate )
            .ThenBy( lpc => lpc.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( lpc => new LearningProgramCompletionResult
            {
                Id = lpc.Id,
                Person = PersonResult.NameOnly( lpc.PersonAlias ),
                LearningProgram = new LearningProgramResult
                {
                    Id = lpc.LearningProgram.Id,
                    Name = lpc.LearningProgram.Name,
                },
                StartDate = lpc.StartDate,
                EndDate = lpc.EndDate,
                Status = lpc.CompletionStatus,
            } )
            .ToList() );

        return helper.GetPaginatedResult( resultPage, resultPage );
    }

    #endregion
}
