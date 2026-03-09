using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.AI.Agent.Classes.Skills.StreakSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class StreakSkill
    {
        #region Tool(s)

        [Description( "Lists streak records." )]
        [AgentPurpose( "Retrieves the streaks." )]
        [AgentUsage( "startDate and endDate refer to the date range of when the current streak started." )]
        [AgentToolGuid( "b02e509f-e674-41f1-8e7c-051ea7ef6946" )]
        public IAgentToolResult ListStreaks(
            string personIdKey = null,
            string streakTypeIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var streakTypeIds = StreakTypeCache.All( AgentRequestContext.RockContext )
                .Where( st => st.IsActive
                    && st.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
                .Select( at => at.Id )
                .ToList();

            var qry = new StreakService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( s => streakTypeIds.Contains( s.StreakTypeId ) );

            qry = helper.WhereOptionalIdKey( qry, s => s.PersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, s => s.StreakTypeId, streakTypeIdKey );
            qry = helper.WhereOptionalIdKey( qry, s => s.StreakTypeId, streakTypeIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, s => s.CurrentStreakStartDate, startDate, endDate );

            if ( streakTypeIdKey.IsNullOrWhiteSpace() && personIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddError( "At least one of personIdKey or streakTypeIdKey must be provided." );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = qry
                .AsExpandable()
                .Select( s => new StreakResult
                {
                    Id = s.Id,
                    StreakType = new StreakTypeResult
                    {
                        Id = s.StreakTypeId,
                        Name = s.StreakType.Name,
                        OccurrenceFrequency = s.StreakType.OccurrenceFrequency,
                    },
                    Person = PersonResult.NameOnly( s.PersonAlias ),
                    EnrollmentDate = s.EnrollmentDate,
                    CurrentStreakStartDate = s.CurrentStreakStartDate,
                    CurrentStreakCount = s.CurrentStreakCount,
                    LongestStreakStartDate = s.LongestStreakStartDate,
                    LongestStreakCount = s.LongestStreakCount,
                } )
                .OrderByDescending( s => s.CurrentStreakStartDate )
                .ThenBy( aa => aa.Id );

            var page = helper.GetPaginatedItems( orderedQry, pageNumber );

            return helper.GetPaginatedResult( page );
        }

        #endregion
    }
}
