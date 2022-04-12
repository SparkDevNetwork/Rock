﻿// <copyright>
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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Connection;

namespace Rock.ClientService.Connection.ConnectionOpportunity
{
    /// <summary>
    /// Provides methods to work with <see cref="ConnectionOpportunity"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class ConnectionOpportunityClientService : ClientServiceBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public ConnectionOpportunityClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets both the opportunity request counts (TotalCount and AssignedToYouCount) for the
        /// given opportunities. The Person the service was initialized with is used to calculate
        /// <see cref="ConnectionRequestCountsBag.AssignedToYouCount"/>.
        /// </summary>
        /// <remarks>This method does not check security, it is assumed you have already done so.</remarks>
        /// <param name="connectionOpportunityIds">The connection opportunity identifiers.</param>
        /// <returns>A dictionary of connection request count objects.</returns>
        public Dictionary<int, ConnectionRequestCountsBag> GetOpportunityRequestCounts( IEnumerable<int> connectionOpportunityIds )
        {
            var connectionRequestService = new ConnectionRequestService( RockContext );

            // Create all counts as initially empty, this method must always return
            // a value for each opportunity id.
            var requestCounts = connectionOpportunityIds.ToDictionary( id => id, _ => new ConnectionRequestCountsBag() );

            // Find all the connection requests assigned to the person.
            var activeConnectionRequestQry = connectionRequestService.Queryable()
                .Where( r => connectionOpportunityIds.Contains( r.ConnectionOpportunityId )
                    && r.ConnectionState == ConnectionState.Active );

            // Group them by the connection opportunity and get the counts for
            // each opportunity.
            if ( Person != null )
            {
                // If we have a Person (Person.Id) count their requests into the AssignedToYouCount along with the TotalCount.
                activeConnectionRequestQry
                    .GroupBy( r => r.ConnectionOpportunityId )
                    .Select( g => new
                    {
                        Id = g.Key,
                        Count = g.Count( r => r.ConnectorPersonAliasId.HasValue && r.ConnectorPersonAlias.PersonId == Person.Id ),
                        TotalCount = g.Count()
                    } )
                    .ToList()
                    .ForEach( o => { requestCounts[o.Id].AssignedToYouCount = o.Count; requestCounts[o.Id].TotalCount = o.TotalCount; } );
            }
            else
            {
                // Otherwise, we can only count the total into the TotalCount.
                activeConnectionRequestQry
                    .GroupBy( r => r.ConnectionOpportunityId )
                    .Select( g => new
                    {
                        Id = g.Key,
                        TotalCount = g.Count()
                    } )
                    .ToList()
                    .ForEach( o =>  requestCounts[o.Id].TotalCount = o.TotalCount );
            }

            return requestCounts;
        }

        #endregion
    }
}
