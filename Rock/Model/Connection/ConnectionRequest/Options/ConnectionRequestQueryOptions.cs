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

namespace Rock.Model.Connection.ConnectionRequest.Options
{
    /// <summary>
    /// The filtering options when getting requests.
    /// </summary>
    public class ConnectionRequestQueryOptions
    {
        /// <summary>
        /// Gets or sets the connection opportunity unique identifiers to limit results to.
        /// </summary>
        /// <value>
        /// The connection opportunity unique identifiers to limit results to.
        /// </value>
        public List<Guid> ConnectionOpportunityGuids { get; set; }

        /// <summary>
        /// Gets or sets the connector person identifiers to limit the
        /// results to.
        /// </summary>
        /// <value>
        /// The connector person identifiers.
        /// </value>
        public List<int> ConnectorPersonIds { get; set; }

        /// <summary>
        /// Gets or sets the states that results will be limited to.
        /// </summary>
        /// <value>
        /// The states that results will be limited to.
        /// </value>
        public List<ConnectionState> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the campus that results will be limited to.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets whether or not to only include requests that are past due.
        /// </summary>
        public bool IsFutureFollowUpPastDueOnly { get; set; }

        /// <summary>
        /// The sort option to use when getting requests.
        /// </summary>
        public ConnectionRequestViewModelSortProperty? SortProperty { get; set; }
    }
}
