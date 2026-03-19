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

namespace Rock.Model.Connection.ConnectionType.DTO
{
    /// <summary>
    /// Represents a single status segment within the distribution of
    /// connection requests for a connection type.
    /// </summary>
    internal class ConnectionRequestStatusDistribution
    {
        /// <summary>
        /// The semantic color associated with the connection request status.
        /// This value is typically derived from the status definition.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// The name of the connection request status represented by this segment.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The number of connection requests that currently have this status.
        /// </summary>
        public int Count { get; set; }
    }
}
