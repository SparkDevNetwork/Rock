using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class StreakSkill
    {
        #region Tool(s)

        [Description( "Lists achievement records." )]
        [AgentPurpose( "Retrieves the achievements." )]
        [AgentToolGuid( "d1038401-a36a-4e08-af38-6825edbb1ded" )]
        public IAgentToolResult ListAchievements(
            string personIdKey = null,
            string achievementTypeIdKey = null,

            [Description( "Only include closed attempts if true, only include open attempts if false, or all attempts if null." )]
            bool? closedState = null,

            DateTime? startDate = null,
            DateTime? endDate = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>( true, AgentRequestContext.RockContext ).Id;
            var achievementTypeIds = AchievementTypeCache.All( AgentRequestContext.RockContext )
                .Where( at => at.AchieverEntityTypeId == personAliasEntityTypeId
                    && at.IsActive )
                .Select( at => at.Id )
                .ToList();
            var personAliasQry = new PersonAliasService( AgentRequestContext.RockContext )
                .Queryable();

            var qry = new AchievementAttemptService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( aa => aa.AchievementType.AchieverEntityTypeId == personAliasEntityTypeId )
                .Join( personAliasQry, aa => aa.AchieverEntityId, pa => pa.Id, ( aa, pa ) => new
                {
                    AchievementAttempt = aa,
                    pa.Person,
                } );

            qry = helper.WhereOptionalIdKey( qry, aa => aa.Person.Id, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, aa => aa.AchievementAttempt.AchievementTypeId, achievementTypeIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, aa => aa.AchievementAttempt.AchievementAttemptEndDateTime, startDate, endDate );

            if ( closedState == true )
            {
                qry = qry.Where( aa => aa.AchievementAttempt.IsClosed );
            }
            else if ( closedState == false )
            {
                qry = qry.Where( aa => !aa.AchievementAttempt.IsClosed );
            }

            if ( achievementTypeIdKey.IsNullOrWhiteSpace() && personIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddError( "At least one of personIdKey or achievementTypeIdKey must be provided." );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = qry
                .AsExpandable()
                .Select( aa => new AchievementAttemptResult
                {
                    Id = aa.AchievementAttempt.Id,
                    AchievementType = new KeyNameResult
                    {
                        Id = aa.AchievementAttempt.AchievementType.Id,
                        Name = aa.AchievementAttempt.AchievementType.Name,
                    },
                    Person = PersonResult.NameOnly( aa.Person ),
                    Progress = ( double ) aa.AchievementAttempt.Progress * 100,
                    IsClosed = aa.AchievementAttempt.IsClosed,
                    IsSuccessful = aa.AchievementAttempt.IsSuccessful,
                    AttemptStartDateTime = aa.AchievementAttempt.AchievementAttemptStartDateTime,
                    AttemptEndDateTime = aa.AchievementAttempt.AchievementAttemptEndDateTime,
                } )
                .OrderByDescending( aa => aa.AttemptEndDateTime )
                .ThenBy( aa => aa.Id );

            var page = helper.GetPaginatedItems( orderedQry, pageNumber );

            return helper.GetPaginatedResult( page )
                .WithInstructions( "Progress is on a scale of 0 to 100, though over achievement is allowed." );
        }

        #endregion
    }
}
