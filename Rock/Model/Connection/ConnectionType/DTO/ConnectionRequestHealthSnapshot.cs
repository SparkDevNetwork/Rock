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
    /// Represents aggregate health counts for Connection Requests within a single Connection Type.
    /// Used to support health and timeliness visualizations such as the Request Health Overview chart.
    /// </summary>
    internal class ConnectionRequestHealthSnapshot
    {
        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the number of Connection Requests that are currently on track.
        /// </summary>
        public int OnTrackCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Connection Requests that are approaching their due threshold.
        /// </summary>
        public int DueSoonCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Connection Requests that are past their due threshold.
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Connection Requests that have not been assigned to a Connector.
        /// </summary>
        public int UnassignedCount { get; set; }

        /// <summary>
        /// Gets or sets the number of currently active Connection Requests.
        /// </summary>
        public int ActiveCount { get; set; }
    }
}
