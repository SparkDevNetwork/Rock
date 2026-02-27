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

        [Description( "Lists steps for a specific person." )]
        [AgentPurpose( "Retrieves the steps for a single person." )]
        [AgentToolGuid( "c2226aa4-6efb-4199-ad9f-9d471502b67f" )]
        public IAgentToolResult ListStepsForPerson(
            string personIdKey,

            [Description( "Only include completed steps if true, only include open steps if false, or all steps if null." )]
            bool? completed = null,

            string stepProgramIdKey = null,
            DateTime? beginStartDate = null,
            DateTime? beginEndDate = null,
            DateTime? completedStartDate = null,
            DateTime? completedEndDate = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var stepTypeIds = StepTypeCache.All( AgentRequestContext.RockContext )
                .Where( st => st.IsActive
                    && st.StepProgram.IsActive
                    && st.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
                .Select( st => st.Id )
                .ToList();

            var qry = new StepService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( s => stepTypeIds.Contains( s.StepTypeId ) );

            qry = helper.WhereRequiredIdKey( qry, spc => spc.PersonAlias.PersonId, personIdKey );
            qry = helper.WhereOptionalIdKey( qry, spc => spc.StepType.StepProgramId, stepProgramIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.StartDateTime, beginStartDate, beginEndDate );
            qry = helper.WhereOptionalPropertyBetween( qry, spc => spc.CompletedDateTime, completedStartDate, completedEndDate );

            if ( completed == true )
            {
                qry = qry.Where( s => s.CompletedDateTime.HasValue );
            }
            else if ( completed == false )
            {
                qry = qry.Where( s => !s.CompletedDateTime.HasValue );
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var orderedQry = stepProgramIdKey.IsNullOrWhiteSpace()
                ? qry
                    .OrderByDescending( s => s.EndDateTime )
                    .ThenByDescending( s => s.StartDateTime )
                    .ThenBy( s => s.Id )
                : qry
                    .OrderBy( s => s.StepType.Order )
                    .ThenBy( s => s.Id );

            var projectedQry = orderedQry
                .Select( s => new StepResult
                {
                    Id = s.Id,
                    StepType = new StepTypeResult
                    {
                        Id = s.StepType.Id,
                        Name = s.StepType.Name,
                        StepProgram = new KeyNameResult
                        {
                            Id = s.StepType.StepProgram.Id,
                            Name = s.StepType.StepProgram.Name,
                        },
                    },
                    StartDateTime = s.StartDateTime,
                    EndDateTime = s.EndDateTime,
                    CompletedDateTime = s.CompletedDateTime,
                } );

            var page = helper.GetPaginatedItems( projectedQry, pageNumber );

            return helper.GetPaginatedResult( page );
        }

        #endregion
    }
}
