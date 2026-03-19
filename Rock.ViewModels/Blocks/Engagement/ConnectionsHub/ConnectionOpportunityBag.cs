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
    /// Represents a connection opportunity and the data needed to display and interact with it during a transfer.
    /// </summary>
    public class ConnectionOpportunityBag
    {
        /// <summary>
        /// Gets or sets the name of this connection opportunity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of this connection opportunity.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the list of connectors available for assignment within this opportunity.
        /// </summary>
        public List<ListItemBag> PotentialConnectors { get; set; }

        /// <summary>
        /// Gets or sets the URL of the photo associated with this connection opportunity.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the icon representing this connection opportunity.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the description of this connection opportunity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the status selector should be shown during a transfer to this opportunity.
        /// </summary>
        public bool ShowStatusOnTransfer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus selector should be shown during a transfer to this opportunity.
        /// </summary>
        public bool ShowCampusOnTransfer { get; set; }

        /// <summary>
        /// Gets or sets the list of campuses available for selection when transferring to this opportunity.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the attribute values for this connection opportunity.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
