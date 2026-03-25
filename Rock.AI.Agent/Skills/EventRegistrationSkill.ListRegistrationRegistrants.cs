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
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventRegistrationSkill
{
    #region Tool(s)

    [Description( "Retrieves a list of registrants that have registered for a registration instance." )]
    [AgentPurpose( "Retrieves a list of registrants that have registered for a registration instance." )]
    [AgentUsage( "The startDate and endDate parameters filter the date the person was registered." )]
    [AgentToolGuid( "3a9fdd49-a029-4a98-9dc5-cfca1a53cfac" )]
    public IAgentToolResult ListRegistrationRegistrants(
        string registrationInstanceIdKey = null,
        string personIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new RegistrationRegistrantService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( rr => rr.Registration.RegistrationInstance )
            .Where( rr => rr.Registration.RegistrationInstance.IsActive
                && rr.Registration.RegistrationInstance.RegistrationTemplate.IsActive
                && rr.Registration.RegistrationInstance.StartDateTime.HasValue );

        query = helper.WhereOptionalIdKey( query, rr => rr.Registration.RegistrationInstanceId, registrationInstanceIdKey );
        query = helper.WhereOptionalIdKey( query, rr => rr.PersonAlias.PersonId, personIdKey );
        query = helper.WhereOptionalPropertyBetween( query, rr => rr.CreatedDateTime, startDate, endDate );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<RegistrationRegistrant>( currentPerson, qry => qry
            .OrderByDescending( rr => rr.CreatedDateTime )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( rr => new RegistrationRegistrantResult
            {
                Id = rr.Id,
                Person = PersonResult.NameOnly( rr.PersonAlias ),
                RegistrationInstance = new RegistrationInstanceResult
                {
                    Id = rr.Registration.RegistrationInstance.Id,
                    Name = rr.Registration.RegistrationInstance.Name,
                },
                RegisteredDateTime = rr.CreatedDateTime,
            } )
            .ToList() );

        var historyResultPage = cursorPage.WithItems( resultPage.Items
            .Select( rr => new RegistrationRegistrantResult
            {
                Id = rr.Id,
                Person = rr.Person,
                RegistrationInstance = rr.RegistrationInstance,
                RegisteredDateTime = rr.RegisteredDateTime,
            } )
            .ToList() );

        return helper.GetPaginatedResult( resultPage, historyResultPage );
    }

    #endregion
}
