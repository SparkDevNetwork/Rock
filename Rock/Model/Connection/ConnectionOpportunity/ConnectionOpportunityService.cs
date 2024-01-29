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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model.Connection.ConnectionOpportunity.Options;

namespace Rock.Model
{
    public partial class ConnectionOpportunityService
    {
        #region Default Options

        /// <summary>
        /// The default options to use if not specified. This saves a few
        /// CPU cycles from having to create a new one each time.
        /// </summary>
        private static readonly ConnectionOpportunityQueryOptions DefaultGetConnectionTypesOptions = new ConnectionOpportunityQueryOptions();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the connection opportunities queryable that matches the specified
        /// options.
        /// </summary>
        /// <param name="options">The filter to apply to the query.</param>
        /// <returns>A queryable of <see cref="ConnectionOpportunity"/> objects.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<ConnectionOpportunity> GetConnectionOpportunitiesQuery( ConnectionOpportunityQueryOptions options = null )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            options = options ?? DefaultGetConnectionTypesOptions;

            var qry = Queryable();

            if ( options.ConnectionTypeGuids != null && options.ConnectionTypeGuids.Any() )
            {
                qry = qry.Where( o => options.ConnectionTypeGuids.Contains( o.ConnectionType.Guid ) );
            }

            if ( options.ConnectorPersonIds != null && options.ConnectorPersonIds.Any() )
            {
                var connectorRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                    .Where( r => r.ConnectionState != ConnectionState.Connected
                        && r.ConnectorPersonAliasId.HasValue
                        && options.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) )
                    .Select( r => r.Id );

                qry = qry.Where( o => o.ConnectionRequests.Any( r => connectorRequestsQry.Contains( r.Id ) ) );
            }

            if ( !options.IncludeInactive )
            {
                qry = qry.Where( o => o.IsActive && o.ConnectionType.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Get all the Connection Statuses for a Connection Opportunity
        /// </summary>
        /// <param name="connectionOpportunityId"></param>
        /// <returns></returns>
        public List<ConnectionStatus> GetStatuses( int connectionOpportunityId )
        {
            return Queryable()
                .AsNoTracking()
                .Where( co => co.Id == connectionOpportunityId  )
                .SelectMany( co => co.ConnectionType.ConnectionStatuses )
                .Where( cs => cs.IsActive )
                .ToList()
                .OrderBy( cs => cs.Order )
                .ToList();
        }

        #endregion
    }
}
