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
            // Paging
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            var isInternal = AgentRequestContext.AudienceType == AudienceType.Internal;

            // We need to get a list of connection opportunities that the current user is authorized to see.
            // TODO: This could be optimized by creating a connection opportunity cache. 
            var authorizedConnectionOpportunityIds = AuthorizedConnectionOpportunityIds();

            var connectionRequestsQry = new ConnectionRequestService( AgentRequestContext.RockContext ).Queryable()
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

            var connectionRequests = connectionRequestsQry
                .Select( cr => new ConnectionRequestResult
                {
                    Id = cr.Id,
                    Requester = new PersonResult
                    {
                        Id = cr.PersonAlias.Person.Id,
                        FirstName = cr.PersonAlias.Person.FirstName,
                        LastName = cr.PersonAlias.Person.LastName,
                        NickName = cr.PersonAlias.Person.NickName,
                        PhotoId = cr.PersonAlias.Person.PhotoId
                    },
                    Comments = cr.Comments,
                    ConnectionState = new KeyNameResult { Id = ( int ) cr.ConnectionState, Name = cr.ConnectionState.ToString() },
                    ConnectionStatus = new KeyNameResult { Id = cr.ConnectionStatus.Id, Name = cr.ConnectionStatus.Name },
                    ConnectionOpportunity = new ConnectionOpportunityResult
                    {
                        Id = cr.ConnectionOpportunity.Id,
                        Name = cr.ConnectionOpportunity.Name,
                        ConnectionType = new ConnectionTypeResult { Id = cr.ConnectionOpportunity.ConnectionType.Id, Name = cr.ConnectionOpportunity.ConnectionType.Name }
                    },
                    CreatedDateTime = cr.CreatedDateTime,
                    ModifiedDateTime = cr.ModifiedDateTime,
                    FollowupDate = cr.FollowupDate,
                    Campus = cr.Campus != null ? new CampusResult { Id = cr.Campus.Id, Name = cr.Campus.Name } : null,
                    AssignedGroup = cr.AssignedGroup != null ? new GroupResult { Id = cr.AssignedGroup.Id, Name = cr.AssignedGroup.Name } : null,
                    Connector = cr.ConnectorPersonAlias != null ? new PersonResult
                    {
                        Id = cr.ConnectorPersonAlias.Person.Id,
                        FirstName = cr.ConnectorPersonAlias.Person.FirstName,
                        LastName = cr.ConnectorPersonAlias.Person.LastName,
                        NickName = cr.ConnectorPersonAlias.Person.NickName,
                        PhotoId = cr.ConnectorPersonAlias.Person.PhotoId
                    } : null,
                    Activities = cr.ConnectionRequestActivities.Select( a => new ConnectionRequestActivityResult
                    {
                        Id = a.Id,
                        ActivityType = new KeyNameResult { Id = a.ConnectionActivityTypeId, Name = a.ConnectionActivityType.Name },
                        Note = a.Note,
                        CreatedDateTime = a.CreatedDateTime,
                        Connector = a.ConnectorPersonAlias != null ? new PersonResult
                        {
                            Id = a.CreatedByPersonAlias.Person.Id,
                            FirstName = a.CreatedByPersonAlias.Person.FirstName,
                            LastName = a.CreatedByPersonAlias.Person.LastName,
                            NickName = a.CreatedByPersonAlias.Person.NickName,
                            PhotoId = a.CreatedByPersonAlias.Person.PhotoId
                        } : null
                    } ).ToList(),
                    Attributes = cr.ConnectionRequestAttributeValues
                        .Where( a => isInternal || a.IsPublic )
                        .Select( a =>
                            new AttributeResult { Id = a.AttributeId, Value = a.PersistedTextValue, Name = a.Name } ).ToList()

                } )
                .OrderBy( cr => cr.Id )
                .Skip( offset )
                .Take( take )
                .ToList();

            // Run security on each person (removes any data they shouldn't see)
            foreach ( var request in connectionRequests )
            {
                request.SanitizeForSecurity( AgentRequestContext.RockRequestContext.CurrentPerson );
            }

            var hasMore = connectionRequests.Count > basePageSize;
            if ( hasMore )
            {
                connectionRequests.RemoveAt( connectionRequests.Count - 1 ); // drop lookahead row
            }

            var meta = new Dictionary<string, object>
                {
                    { "personKey", requesterPersonIdKey },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", connectionRequests.Count },
                    { "hasMore", hasMore }
                };

            if ( !connectionRequests.Any() )
            {
                return RockToolResult.NoData()
                    .WithMetadata( meta );
            }

            return RockToolResult.Success( connectionRequests )
                .WithMetadata( meta );
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
