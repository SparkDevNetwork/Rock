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
    /// Represents the data returned when opening the transfer dialog for a connection request, including the current values and available transfer options.
    /// </summary>
    public class TransferConnectionRequestDetailsBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request being transferred.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection opportunity this request currently belongs to.
        /// </summary>
        public Guid CurrentConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector currently assigned to this request.
        /// </summary>
        public string CurrentConnectorName { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the current connector's person alias.
        /// </summary>
        public Guid? CurrentConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the campus currently assigned to this request.
        /// </summary>
        public Guid? CurrentCampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection status currently assigned to this request.
        /// </summary>
        public Guid CurrentConnectionStatusGuid { get; set; }

        /// <summary>
        /// Gets or sets the list of connection opportunities available as transfer targets.
        /// </summary>
        public List<ConnectionOpportunityBag> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the list of campuses available for selection during the transfer.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the list of connection statuses available for selection during the transfer.
        /// </summary>
        public List<ListItemBag> Statuses { get; set; }

        /// <summary>
        /// Gets or sets the Connector Items for the selected Opportunity
        /// </summary>
        public List<ListItemBag> ConnectorItems { get; set; }

        /// <summary>
        /// Gets or sets the public attribute definitions for the connection request, used to display attribute fields during transfer.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
