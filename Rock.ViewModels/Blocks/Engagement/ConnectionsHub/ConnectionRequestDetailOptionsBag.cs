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
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the configuration options used to render the connection request detail panel.
    /// </summary>
    public class ConnectionRequestDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the Connection Type this request belongs to.
        /// </summary>
        public string ConnectionTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the current person can edit the Connection Request.
        /// </summary>
        public bool CanEditConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a placement group must be assigned before a request can be completed.
        /// </summary>
        public bool RequiresPlacementGroupToComplete { get; set; }

        /// <summary>
        /// Gets or sets the list of connectors (with photo URLs) available for assignment to the request.
        /// </summary>
        public List<ConnectorItemBag> ConnectorItems { get; set; }

        /// <summary>
        /// Gets or sets the list of connection statuses available for this Connection Type.
        /// </summary>
        public List<ConnectionStatusBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the list of connection states (e.g., Active, Inactive, Future Follow-up) available for selection.
        /// </summary>
        public List<ListItemBag> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the list of placement groups available for this connection opportunity.
        /// </summary>
        public List<PlacementGroupDetailsBag> PlacementGroups { get; set; }

        /// <summary>
        /// Gets or sets the list of activity types available for logging against this request.
        /// </summary>
        public List<ConnectionActivityTypeBag> ConnectionActivities { get; set; }

        /// <summary>
        /// Gets or sets the list of request source items available for selection.
        /// </summary>
        public List<ListItemBag> RequestSourceItems { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used to render the heading section of the request detail panel.
        /// </summary>
        public string LavaHeadingTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used to render the badge bar in the request detail panel.
        /// </summary>
        public string LavaBadgeBar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the future follow-up feature is enabled for this Connection Type.
        /// </summary>
        public bool IsFutureFollowUpEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per-request security is enabled.
        /// </summary>
        public bool IsRequestSecurityEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reminders are enabled for connection requests.
        /// </summary>
        public bool AreRemindersEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether celebrations are enabled for connection requests.
        /// </summary>
        public bool AreCelebrationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group placements are enabled for this Connection Type.
        /// </summary>
        public bool AreGroupPlacementsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether connection statuses must be progressed sequentially.
        /// </summary>
        public bool IsSequentialStatusMode { get; set; }

        /// <summary>
        /// Gets or sets the GUIDs of the person profile badges to display on the request detail panel.
        /// </summary>
        public List<Guid> BadgeGuids { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an active AI provider is configured.
        /// </summary>
        public bool IsAISummaryVisible { get; set; }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the logged in user can edit a connection request note.
        /// </summary>
        public bool CanEditConnectionRequestNote { get; set;  }
    }
}
