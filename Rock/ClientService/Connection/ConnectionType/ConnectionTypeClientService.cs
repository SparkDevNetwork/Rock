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
using System.Linq;

using Rock.ClientService.Connection.ConnectionOpportunity;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.Connection.ConnectionRequest;

namespace Rock.ClientService.Connection.ConnectionType
{
    /// <summary>
    /// Provides methods to work with <see cref="ConnectionType"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class ConnectionTypeClientService : ClientServiceBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public ConnectionTypeClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type request counts for the given connection types. The
        /// Person that the service was initialized with is used to calculate
        /// <see cref="ConnectionRequestCountsViewModel.AssignedToYouCount"/>.
        /// </summary>
        /// <remarks>This method does not check security, it is assumed you have already done so.</remarks>
        /// <param name="connectionTypeIds">The connection type identifiers.</param>
        /// <returns>A dictionary of connection request count objects.</returns>
        public Dictionary<int, ConnectionRequestCountsViewModel> GetConnectionTypeCounts( IEnumerable<int> connectionTypeIds )
        {
            var opportunityClientService = new ConnectionOpportunityClientService( RockContext, Person );

            var opportunities = new ConnectionOpportunityService( RockContext )
                .Queryable()
                .Where( o => connectionTypeIds.Contains( o.ConnectionTypeId ) )
                .Select( o => new
                {
                    o.Id,
                    o.ConnectionTypeId
                } )
                .ToList();
            var opportunityIds = opportunities.Select( o => o.Id ).ToList();

            var requestCounts = opportunityClientService.GetOpportunityRequestCounts( opportunityIds )
                .Select( c => new
                {
                    TypeId = opportunities.Single( o => o.Id == c.Key ).ConnectionTypeId,
                    Counts = c.Value
                } )
                .GroupBy( c => c.TypeId )
                .ToDictionary( g => g.Key, g => new ConnectionRequestCountsViewModel
                {
                    AssignedToYouCount = g.Sum( c => c.Counts.AssignedToYouCount )
                } );

            // Fill in any missing types with empty counts.
            foreach ( var typeId in connectionTypeIds )
            {
                if ( !requestCounts.ContainsKey( typeId ) )
                {
                    requestCounts.Add( typeId, new ConnectionRequestCountsViewModel() );
                }
            }

            return requestCounts;
        }

        #endregion
    }
}
