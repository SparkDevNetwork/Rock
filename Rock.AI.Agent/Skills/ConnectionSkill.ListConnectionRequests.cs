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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ConnectionSkill
    {
        #region Tool(s)

        [Description( "Returns a list of connection requests for the user." )]
        [AgentPurpose( "Retrieves a list of connection requests." )]
        [AgentUsage( "Requests can be filtered by connection type, connection opportunity, requester or connector. Connectors are people who are assigned a request." )]
        [AgentToolGuid( "DC03271E-2C54-D5AF-4F18-9CCC69F25202" )]
        public RockToolResult ListConnectionRequests(
            string connectionTypeIdKey = null,
            string connectionOpportunityIdKey = null,
            string requesterPersonIdKey = null,
            string connectorPersonIdKey = null,
            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            // We need to get a list of connection opportunities that the current user is authorized to see.
            // TODO: This could be optimized by creating a connection opportunity cache. 
            var authorizedConnectionOpportunityIds = AuthorizedConnectionOpportunityIds();

            var connectionRequestsQry = new ConnectionRequestService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( cr => authorizedConnectionOpportunityIds.Contains( cr.ConnectionOpportunityId ) );

            // Filter by requester
            if ( requesterPersonIdKey.IsNotNullOrWhiteSpace() )
            {
                var requesterPersonId = IdHasher.Instance.GetId( requesterPersonIdKey );
                connectionRequestsQry = connectionRequestsQry
                    .Where( cr => cr.PersonAlias != null && cr.PersonAlias.PersonId == requesterPersonId );
            }

            // Filter by connector
            if ( connectorPersonIdKey.IsNotNullOrWhiteSpace() )
            {
                var connectorPersonId = IdHasher.Instance.GetId( connectorPersonIdKey );
                connectionRequestsQry = connectionRequestsQry
                    .Where( cr => cr.ConnectorPersonAlias != null && cr.ConnectorPersonAlias.PersonId == connectorPersonId );
            }

            // Filter by connection type
            if ( connectionTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var connectionTypeId = IdHasher.Instance.GetId( connectionTypeIdKey );
                connectionRequestsQry = connectionRequestsQry
                    .Where( cr => cr.ConnectionTypeId == connectionTypeId );
            }

            // Filter by connection opportunity
            if ( connectionOpportunityIdKey.IsNotNullOrWhiteSpace() )
            {
                var connectionOpportunityId = IdHasher.Instance.GetId( connectionOpportunityIdKey );
                connectionRequestsQry = connectionRequestsQry
                    .Where( cr => cr.ConnectionOpportunityId == connectionOpportunityId );
            }

            var connectionRequestQry = connectionRequestsQry
                .AsExpandable()
                .Select( cr => new ConnectionRequestResult
                {
                    Id = cr.Id,
                    Requester = PersonResult.NameOnly( cr.PersonAlias ),
                    ConnectionState = cr.ConnectionState,
                    ConnectionStatus = new KeyNameResult
                    {
                        Id = cr.ConnectionStatus.Id,
                        Name = cr.ConnectionStatus.Name
                    },
                    ConnectionOpportunity = new ConnectionOpportunityResult
                    {
                        Id = cr.ConnectionOpportunity.Id,
                        Name = cr.ConnectionOpportunity.Name,
                        ConnectionType = new ConnectionTypeResult
                        {
                            Id = cr.ConnectionOpportunity.ConnectionType.Id,
                            Name = cr.ConnectionOpportunity.ConnectionType.Name
                        }
                    },
                    CreatedDateTime = cr.CreatedDateTime,
                    Connector = PersonResult.NameOnly( cr.ConnectorPersonAlias ),
                    AttributeValues = cr.ConnectionRequestAttributeValues.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
                } )
                .OrderByDescending( cr => cr.CreatedDateTime.HasValue )
                .ThenByDescending( cr => cr.CreatedDateTime )
                .ThenBy( cr => cr.Id );

            return helper.GetPaginatedResult( connectionRequestQry, pageNumber, 5 );
        }

        #endregion

        #region Helper Methods

        private List<int> AuthorizedConnectionOpportunityIds()
        {
            var authorizedConnectionOpportunityIds = new List<int>();

            var connectionOpportunities = new ConnectionOpportunityService( AgentRequestContext.RockContext ).Queryable().AsNoTracking();

            foreach ( var opportunity in connectionOpportunities )
            {
                if ( opportunity.IsAuthorized( Rock.Security.Authorization.VIEW, AgentRequestContext.RockRequestContext.CurrentPerson ) )
                {
                    authorizedConnectionOpportunityIds.Add( opportunity.Id );
                }
            }

            return authorizedConnectionOpportunityIds;
        }

        #endregion
    }
}
