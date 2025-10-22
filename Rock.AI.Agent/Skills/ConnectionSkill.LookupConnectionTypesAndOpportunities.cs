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
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ConnectionSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves all configured connection types and opportunities in Rock. 
        /// </summary>
        [Description( "Retrieves all configured websites in Rock." )]
        [AgentPurpose( "Retrieves a list of all of the connection types and their configuration. This includes Connection Opportunities and Activity Types." )]
        [AgentPurpose( "This tool does not return any information about specific connection requests." )]
        [AgentToolGuid( "21870C06-126F-0882-47E3-DBFC1846BD92" )]
        public RockToolResult LookupConnectionTypesAndOpportunities()
        {
            var connectionTypes = RockCache.GetOrAddExisting( "rock.core.aiagent.lookupconnectiontypesandopportunties", null, () =>
            {
                return LoadConnectionTypes();
            }, TimeSpan.FromMinutes( 3 ) ) as List<ConnectionTypeResult>;

            return RockToolResult.Success( connectionTypes );
        }

        #endregion

        #region Helper Methods

        private List<ConnectionTypeResult> LoadConnectionTypes()
        {
            var connectionTypes = ConnectionTypeCache.All( AgentRequestContext.RockContext );

            if ( !connectionTypes.Any() )
            {
                return new List<ConnectionTypeResult>();
            }

            // TODO: is this the correct way to get the current person?
            var currentPerson = RockRequestContextAccessor.Current?.CurrentPerson;

            var connectionTypeResult = connectionTypes
                .Where( cr => cr.IsActive )
                .Where( cr => cr.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( cr => new ConnectionTypeResult
                {
                    Id = cr.Id,
                    Name = cr.Name,
                    Description = cr.Description,
                    Attributes = cr.AttributeValues
                        .Where( v => v.Value != null && v.Value.Value != null & v.Value.Value != string.Empty )
                        .Where( v => cr.Attributes[v.Key].IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                        .Select( a => new AttributeResult
                        {
                            Id = a.Value.AttributeId,
                            Key = a.Key,
                            Value = a.Value.PersistedTextValue,
                            Category = a.Value.AttributeCategoryIds.Select( cId => CategoryCache.Get( cId ) ).Where( c => c != null ).Select( c => c.Name ).FirstOrDefault()
                        } )
                        .ToList(),
                } )
                .ToList();

            // Add connection opportunities. There is no cache for this so we have to query it.
            var connectionOpportunities = new ConnectionOpportunityService( AgentRequestContext.RockContext )
                .Queryable().AsNoTracking()
                .Include( "ConnectionOpportunityCampuses" )
                .Where( o =>
                    o.IsActive
                    && o.ConnectionType.IsActive
                )
                .ToList();

            connectionOpportunities.LoadAttributes();

            foreach ( var connectionType in connectionTypeResult )
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
                        Campuses = co.ConnectionOpportunityCampuses.Select( c => new CampusResult
                        {
                            Id = c.CampusId,
                            Name = c.Campus != null ? c.Campus.Name : string.Empty
                        } ).ToList(),
                        Attributes = co.AttributeValues
                            .Where( v => v.Value != null && v.Value.Value != null & v.Value.Value != string.Empty )
                            .Where( v => co.Attributes[v.Key].IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                            .Select( a => new AttributeResult
                            {
                                Id = a.Value.AttributeId,
                                Key = a.Key,
                                Value = a.Value.PersistedTextValue,
                                Category = a.Value.AttributeCategoryIds.Select( cId => CategoryCache.Get( cId ) ).Where( c => c != null ).Select( c => c.Name ).FirstOrDefault()
                            } )
                            .ToList(),
                    } )
                    .ToList();
            }

            return connectionTypeResult;
        }

        #endregion
    }
}
