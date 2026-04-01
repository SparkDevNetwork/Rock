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

using Rock.Enums.Connection;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to transfer a connection request to a different connection opportunity.
    /// </summary>
    public class TransferConnectionRequestBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request to transfer.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection opportunity to transfer this request to.
        /// </summary>
        public Guid NewConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the connector assignment option (e.g., keep current, assign default, assign specific person).
        /// </summary>
        public string ConnectorOption { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the person alias to assign as the connector after the transfer, when a specific connector is selected.
        /// </summary>
        public Guid? ConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection status to assign after the transfer.
        /// </summary>
        public Guid? StatusGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the campus to assign after the transfer.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets an optional note to record with the transfer activity.
        /// </summary>
        public string Note { get; set; }
    }
}
