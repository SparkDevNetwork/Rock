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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.LearningManagementSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LearningManagementSkill
{
    #region Tool(s)

    [Description( "Returns a list of classes." )]
    [AgentPurpose( "Returns a list of classes." )]
    [AgentToolGuid( "f80d1c2f-ebf1-4bb6-b1ef-081eb8fdb74d" )]
    public IAgentToolResult ListClasses(
        string learningProgramIdKey = null,
        string learningCourseIdKey = null,
        string enrolledPersonIdKey = null,
        bool includePastClasses = false,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new LearningClassService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( lc => lc.Campus )
            .Include( lc => lc.LearningCourse.LearningProgram )
            .Where( lc => lc.IsActive
                && lc.LearningCourse.IsActive
                && lc.LearningCourse.LearningProgram.IsActive );

        query = helper.WhereOptionalIdKey( query, lc => lc.LearningCourse.LearningProgramId, learningProgramIdKey );
        query = helper.WhereOptionalIdKey( query, lc => lc.LearningCourseId, learningCourseIdKey );

        if ( !includePastClasses )
        {
            var today = RockDateTime.Today;

            query = query.Where( lc => lc.LearningCourse.LearningProgram.ConfigurationMode == Enums.Lms.ConfigurationMode.OnDemandLearning
                || !lc.LearningSemesterId.HasValue
                || !lc.LearningSemester.EndDate.HasValue
                || lc.LearningSemester.EndDate >= today );
        }

        if ( enrolledPersonIdKey.IsNotNullOrWhiteSpace() )
        {
            var enrolledPersonId = IdHasher.Instance.GetId( enrolledPersonIdKey );

            if ( enrolledPersonId.HasValue )
            {
                query = query.Where( lc => lc.Members.Any( gm => gm.PersonId == enrolledPersonId.Value ) );
            }
            else
            {
                helper.AddError( $"The value of {nameof( enrolledPersonIdKey )} is not valid." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<LearningClass>( currentPerson, qry => qry
            .OrderBy( cr => cr.Name )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( g => new LearningClassResult
            {
                Id = g.Id,
                Name = g.Name,
                LearningCourse = new LearningCourseResult
                {
                    Id = g.LearningCourse.Id,
                    Name = g.LearningCourse.Name,
                    LearningProgram = new LearningProgramResult
                    {
                        Id = g.LearningCourse.LearningProgram.Id,
                        Name = g.LearningCourse.LearningProgram.Name,
                    },
                },
                Campus = g.Campus != null
                    ? new CampusResult
                    {
                        Id = g.Campus.Id,
                        Name = g.Campus.Name,
                    }
                    : null,
                AttributeValues = [.. g.GetGridAttributeValueResults( AgentRequestContext )],
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cr => new KeyNameResult
        {
            Id = cr.Id,
            Name = cr.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
