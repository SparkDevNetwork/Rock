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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ConnectionSkill
{
    #region Tool(s)

    [Description( "Returns a list of connection requests that match the filters." )]
    [AgentUsage( "Requests can be filtered by connection type, connection opportunity, requester or connector. Connectors are people who are assigned a request." )]
    [AgentToolGuid( "DC03271E-2C54-D5AF-4F18-9CCC69F25202" )]
    public AgentToolResult ListConnectionRequests(
        string connectionTypeIdKey = null,
        string connectionOpportunityIdKey = null,
        string requesterPersonIdKey = null,
        string connectorPersonIdKey = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new ConnectionRequestService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( cr => !cr.ConnectedDateTime.HasValue );

        query = helper.WhereOptionalIdKey( query, cr => cr.PersonAlias.PersonId, requesterPersonIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectorPersonAlias.PersonId, connectorPersonIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionTypeId, connectionTypeIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );

        var hasAnyFilters = !string.IsNullOrWhiteSpace( connectionTypeIdKey )
            || !string.IsNullOrWhiteSpace( connectionOpportunityIdKey )
            || !string.IsNullOrWhiteSpace( requesterPersonIdKey )
            || !string.IsNullOrWhiteSpace( connectorPersonIdKey );

        if ( !hasAnyFilters )
        {
            helper.AddError( "At least one filter parameter must be provided to limit the results returned." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<ConnectionRequest>( currentPerson, qry => qry
            .OrderByDescending( cr => cr.CreatedDateTime.HasValue )
            .ThenByDescending( cr => cr.CreatedDateTime )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( cr => new ConnectionRequestResult
            {
                Id = cr.Id,
                Guid = cr.Guid,
                Requester = PersonResult.NameOnly( cr.PersonAlias ),
                ConnectionState = cr.ConnectionState,
                ConnectionStatus = new KeyNameResult
                {
                    Id = cr.ConnectionStatus.Id,
                    Guid = cr.ConnectionStatus.Guid,
                    Name = cr.ConnectionStatus.Name
                },
                ConnectionOpportunity = new ConnectionOpportunityResult
                {
                    Id = cr.ConnectionOpportunity.Id,
                    Guid = cr.ConnectionOpportunity.Guid,
                    Name = cr.ConnectionOpportunity.Name,
                    ConnectionType = new ConnectionTypeResult
                    {
                        Id = cr.ConnectionOpportunity.ConnectionType.Id,
                        Guid = cr.ConnectionOpportunity.ConnectionType.Guid,
                        Name = cr.ConnectionOpportunity.ConnectionType.Name
                    }
                },
                CreatedDateTime = cr.CreatedDateTime,
                Connector = PersonResult.NameOnly( cr.ConnectorPersonAlias ),
                AttributeValues = cr.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cr => new KeyNameResult
        {
            Id = cr.Id,
            Guid = cr.Guid,
            Name = cr.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
