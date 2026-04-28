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

namespace Rock.Model.Connection.ConnectionType.Options
{
    /// <summary>
    /// The options that can be specified when querying for connection request completion metrics.
    /// </summary>
    internal class ConnectionRequestCompletionMetricsQueryOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier of the campus to filter the metrics by.
        /// If this is not set then metrics will not be filtered by campus.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the connection opportunity to filter the metrics by.
        /// If this is not set then the metrics will not be filtered by connection opportunity.
        /// </summary>
        public Guid? ConnectionOpportunityGuid { get; set; }
    }
}
