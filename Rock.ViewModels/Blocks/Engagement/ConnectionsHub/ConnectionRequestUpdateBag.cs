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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to update the state, status, connector, or follow-up date of a connection request.
    /// </summary>
    public class ConnectionRequestUpdateBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request to update.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection status to assign to this request.
        /// </summary>
        public string ConnectionStatusGuid { get; set; }

        /// <summary>
        /// Gets or sets an optional note to record with this update.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the connection state to apply (e.g., Active, Inactive, Future Follow-up).
        /// </summary>
        public ConnectionState? ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date for this request when transitioning to the Future Follow-up state.
        /// </summary>
        public DateTimeOffset? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connector's person alias to assign to this request.
        /// </summary>
        public string ConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this update includes completing the request.
        /// </summary>
        public bool? CanCompleteRequest { get; set; }
    }
}
