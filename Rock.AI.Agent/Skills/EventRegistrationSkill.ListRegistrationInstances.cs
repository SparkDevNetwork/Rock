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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventRegistrationSkill
{
    #region Tool(s)

    [Description( "Retrieves a list of registration instances." )]
    [AgentPurpose( "Retrieves a list of registration instances." )]
    [AgentUsage( "The startDate and endDate parameters filter instances that open within the window, close within the window, or exist during the entire window." )]
    [AgentToolGuid( "8a4a5cf4-a213-427f-a0c1-4e4d9082148c" )]
    public IAgentToolResult ListRegistrationInstances(
        string registrationTemplateIdKey = null,
        string partialName = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new RegistrationInstanceService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( ri => ri.RegistrationTemplate )
            .Where( ri => ri.IsActive
                && ri.RegistrationTemplate.IsActive
                && ri.StartDateTime.HasValue );

        query = helper.WhereOptionalIdKey( query, ri => ri.RegistrationTemplateId, registrationTemplateIdKey );

        if ( startDate.HasValue || endDate.HasValue )
        {
            if ( startDate.HasValue && endDate.HasValue )
            {
                query = query.Where( ri => ( ri.StartDateTime.Value >= startDate.Value && ri.StartDateTime.Value <= endDate.Value )
                    || ( ri.EndDateTime.HasValue && ri.EndDateTime.Value >= startDate.Value && ri.EndDateTime.Value <= endDate.Value )
                    || ( ri.StartDateTime.HasValue && !ri.EndDateTime.HasValue && ri.StartDateTime.Value < startDate.Value )
                    || ( ri.StartDateTime.HasValue && ri.EndDateTime.HasValue && ri.StartDateTime.Value < startDate.Value && ri.EndDateTime.Value > endDate.Value ) );
            }
            else if ( startDate.HasValue )
            {
                query = query.Where( ri => ri.StartDateTime.Value >= startDate.Value
                    || ( !ri.EndDateTime.HasValue && ri.StartDateTime.Value < startDate.Value )
                    || ( ri.EndDateTime.HasValue && ri.StartDateTime.Value < startDate.Value && ri.EndDateTime.Value >= startDate.Value ) );
            }
            else if ( endDate.HasValue )
            {
                query = query.Where( ri => ri.StartDateTime.Value <= endDate.Value );
            }
        }
        else
        {
            helper.AddError( $"At least one of {nameof( startDate )} or {nameof( endDate )} must be provided." );
        }

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( ri => ri.Name.Contains( partialName ) );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<RegistrationInstance>( currentPerson, qry => qry
            .OrderByDescending( ri => ri.StartDateTime )
            .ThenBy( ri => ri.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( ri => new RegistrationInstanceResult
            {
                Id = ri.Id,
                Name = ri.Name,
                RegistrationTemplate = new RegistrationTemplateResult
                {
                    Id = ri.RegistrationTemplate.Id,
                    Name = ri.RegistrationTemplate.Name,
                },
                StartDateTime = ri.StartDateTime,
                EndDateTime = ri.EndDateTime,
            } )
            .ToList() );

        var historyResultPage = cursorPage.WithItems( resultPage.Items
            .Select( ri => new RegistrationInstanceResult
            {
                Id = ri.Id,
                Name = ri.Name,
            } )
            .ToList() );

        return helper.GetPaginatedResult( resultPage, historyResultPage );
    }

    #endregion
}
