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
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// The additional configuration options for a specfic Connection Type.
    /// </summary>
    public class ConnectionTypeOptionsBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the Connection Type
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the Connection Type these options describe.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the current user can edit Connection Requests.
        /// </summary>
        public bool CanEditConnectionRequests { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a placement group must be assigned before a connection request can be completed.
        /// </summary>
        public bool RequiresPlacementGroupToComplete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the future follow-up feature is enabled for this Connection Type.
        /// </summary>
        public bool IsFutureFollowUpEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reminders are enabled for connection requests.
        /// </summary>
        public bool AreRemindersEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether celebrations (milestone notifications) are enabled for connection requests.
        /// </summary>
        public bool AreCelebrationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group placements are enabled for this Connection Type.
        /// </summary>
        public bool AreGroupPlacementsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether connection statuses must be progressed sequentially rather than freely.
        /// </summary>
        public bool IsSequentialStatusMode { get; set; }

        /// <summary>
        /// Gets or sets the list of connection statuses available for this Connection Type.
        /// </summary>
        public List<ConnectionStatusBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the list of connection opportunities available within the current Connection Type.
        /// </summary>
        public List<ListItemBag> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the list of request source items available for filtering connection requests by their originating source.
        /// </summary>
        public List<ListItemBag> RequestSourceItems { get; set; }

        /// <summary>
        /// Gets or sets the list of workflows that can be launched from connection requests.
        /// </summary>
        public List<ConnectionWorkflowBag> WorkflowItems { get; set; }

        /// <summary>
        /// Gets or sets the complete list of connectors that can be assigned to connection requests.
        /// </summary>
        public List<ListItemBag> AllPossibleConnectors { get; set; }

        /// <summary>
        /// Gets or sets the list of activity types available to log against connection requests.
        /// </summary>
        public List<ConnectionActivityTypeBag> ConnectionActivities { get; set; }
    }
}
