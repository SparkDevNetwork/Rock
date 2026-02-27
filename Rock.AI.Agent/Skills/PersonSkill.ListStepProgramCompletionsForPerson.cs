using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Lists completed step programs for a specific person." )]
        [AgentPurpose( "Retrieves the completed step programs for a single person." )]
        [AgentToolGuid( "6cc0233d-3897-4f9a-9fea-e530094d40d3" )]
        public IAgentToolResult ListStepProgramCompletionsForPerson(
            string personIdKey,

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

            qry = helper.WhereRequiredIdKey( qry, spc => spc.PersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.EndDateTime, startDate, endDate );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = qry
                .Select( spc => new StepProgramCompletionResult
                {
                    Id = spc.Id,
                    StepProgram = new KeyNameResult
                    {
                        Id = spc.StepProgram.Id,
                        Name = spc.StepProgram.Name,
                    },
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
}
