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

namespace Rock.Model.Connection.ConnectionType.Options
{
    /// <summary>
    /// The standard filtering options when getting connection types.
    /// </summary>
    public class ConnectionTypeQueryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive types
        /// should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive types are included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets the connector person identifiers to limit the
        /// results to. If an type does not have a non-connected
        /// request that is assigned to one of these identifiers it will
        /// not be included.
        /// </summary>
        /// <value>
        /// The connector person identifiers.
        /// </value>
        public List<int> ConnectorPersonIds { get; set; }
    }
}
