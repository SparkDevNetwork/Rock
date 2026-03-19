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
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a summary view of a connection request as displayed in the bulk action grid.
    /// </summary>
    public class BulkRequestViewBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of this connection request.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the person field data for the requester.
        /// </summary>
        public PersonFieldBag Requester { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection opportunity this request is associated with.
        /// </summary>
        public string ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the display value of the request source (e.g., a defined value description).
        /// </summary>
        public string ConnectionTypeSource { get; set; }

        /// <summary>
        /// Gets or sets the currently assigned connector for this request.
        /// </summary>
        public ListItemBag Connector { get; set; }

        /// <summary>
        /// Gets or sets the formatted due date for this request.
        /// </summary>
        public string DueDate { get; set; }

        /// <summary>
        /// Gets or sets the current connection status of this request.
        /// </summary>
        public ConnectionStatusBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the celebration text to display for this request, if applicable.
        /// </summary>
        public string CelebrationText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this request has a placement group assigned.
        /// </summary>
        public bool? HasPlacementGroup { get; set; }
    }
}
