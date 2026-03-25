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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ConnectionSkill
    {
        #region Tool(s)

        [Description( "Retrieves all configured connection types and opportunities in Rock." )]
        [AgentPurpose( "Retrieves a list of all of the connection types and their configuration. This includes Connection Opportunities and Activity Types." )]
        [AgentPurpose( "This tool does not return any information about specific connection requests." )]
        [AgentToolGuid( "21870C06-126F-0882-47E3-DBFC1846BD92" )]
        public IAgentToolResult LookupConnectionTypesAndOpportunities()
        {
            var connectionTypes = LoadConnectionTypes();

            return Success( connectionTypes )
                .WithHistoryContent( connectionTypes
                    .Select( ct => new ConnectionTypeResult
                    {
                        Id = ct.Id,
                        Name = ct.Name,
                        Opportunities = ct.Opportunities
                            .Select( o => new ConnectionOpportunityResult
                            {
                                Id = o.Id,
                                Name = o.Name,
                            } )
                            .ToList(),
                        Statuses = ct.Statuses
                            .Select( s => new KeyNameResult
                            {
                                Id = s.Id,
                                Name = s.Name
                            } )
                            .ToList(),
                    }
                ) );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Load all the connection types, opportunities and statuses that the
        /// current person has access to.
        /// </summary>
        /// <returns>A list of <see cref="ConnectionTypeResult"/> objects.</returns>
        private List<ConnectionTypeResult> LoadConnectionTypes()
        {
            var connectionTypes = ConnectionTypeCache.All( AgentRequestContext.RockContext );

            if ( !connectionTypes.Any() )
            {
                return [];
            }

            var currentPerson = AgentRequestContext.CurrentPerson;

            var connectionTypeResults = connectionTypes
                .Where( cr => cr.IsActive )
                .Where( cr => cr.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( cr => new ConnectionTypeResult
                {
                    Id = cr.Id,
                    Name = cr.Name,
                    Description = cr.Description,
                    AttributeValues = cr.GetAttributeValueResults( AgentRequestContext ).ToList(),
                } )
                .ToList();

            // Add connection statuses. There is no cache for this so we have to query it.
            var connectionStatuses = new ConnectionStatusService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( s => s.IsActive )
                .GroupBy( s => s.ConnectionTypeId )
                .ToDictionary( g => g.Key, g => g.Select( s => new KeyNameResult( s.Id, s.Name ) ).ToList() );

            foreach ( var connectionType in connectionTypeResults )
            {
                if ( connectionStatuses.TryGetValue( connectionType.Id, out var statuses ) )
                {
                    connectionType.Statuses = statuses;
                }
            }

            // Add connection opportunities. There is no cache for this so we have to query it.
            var connectionOpportunities = new ConnectionOpportunityService( AgentRequestContext.RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( co => co.ConnectionOpportunityCampuses )
                .Where( o => o.IsActive && o.ConnectionType.IsActive )
                .ToList();

            connectionOpportunities.LoadAttributes();

            foreach ( var connectionType in connectionTypeResults )
            {
                connectionType.Opportunities = connectionOpportunities
                    .Where( co => co.ConnectionTypeId == connectionType.Id )
                    .Where( co => co.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                    .Select( co => new ConnectionOpportunityResult
                    {
                        Id = co.Id,
                        Name = co.Name,
                        Description = co.Description,
                        PublicName = co.PublicName,
                        Summary = co.Summary,
                        PhotoId = co.PhotoId,
                        Campuses = co.ConnectionOpportunityCampuses
                            .Select( c => new CampusResult
                            {
                                Id = c.CampusId,
                                Name = c.Campus.Name
                            } )
                            .ToList(),
                        AttributeValues = co.GetAttributeValueResults( AgentRequestContext ).ToList(),
                    } )
                    .ToList();
            }

            connectionTypeResults.ForEach( ct => ct.Sanitize( AgentRequestContext ) );

            return connectionTypeResults;
        }

        #endregion
    }
}
