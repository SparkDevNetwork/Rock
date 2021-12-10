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

namespace Rock.Model.Connection.ConnectionOpportunity.Options
{
    /// <summary>
    /// The filtering options when getting opportunities.
    /// </summary>
    public class ConnectionOpportunityQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive opportunities
        /// should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive opportunities are included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets the connection type unique identifiers to limit results to.
        /// </summary>
        /// <value>The connection type unique identifiers to limit results to.</value>
        public List<Guid> ConnectionTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the connector person identifiers to limit the
        /// results to. If an opportunity does not have a non-connected
        /// request that is assigned to one of these identifiers it will
        /// not be included.
        /// </summary>
        /// <value>
        /// The connector person identifiers.
        /// </value>
        public List<int> ConnectorPersonIds { get; set; }
    }
}
