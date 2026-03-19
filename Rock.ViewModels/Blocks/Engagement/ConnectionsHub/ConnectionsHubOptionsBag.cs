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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// The additional configuration options for the Connections Hub block.
    /// </summary>
    public class ConnectionsHubOptionsBag
    {
        /// <summary>
        /// Gets or sets the title to display for the Connections Hub block.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the icon to display alongside the block title.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether the current user can edit Connection Requests.
        /// </summary>
        public bool CanEditConnectionRequests { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the Connection Type being viewed.
        /// </summary>
        public string ConnectionTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the Connection Request being viewed or edited.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a placement group must be assigned before a connection request can be completed.
        /// </summary>
        public bool RequiresPlacementGroupToComplete { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the Connection Opportunity supplied via a page parameter, used to pre-filter the view.
        /// </summary>
        public Guid? ConnectionOpportunityGuidFromPageParameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the future follow-up feature is enabled for this Connection Type.
        /// </summary>
        public bool IsFutureFollowUpEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per-request security is enabled, allowing individual requests to have their own security settings.
        /// </summary>
        public bool IsRequestSecurityEnabled { get; set; }

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
        /// Gets or sets the currently selected connector used to filter the request list.
        /// </summary>
        public ListItemBag SelectedConnector { get; set; }

        /// <summary>
        /// Gets or sets the GUIDs of the person profile badges to display on connection requests.
        /// </summary>
        public List<Guid> BadgeGuids { get; set; }

        /// <summary>
        /// Gets or sets the list of connection statuses available for this Connection Type.
        /// </summary>
        public List<ConnectionStatusBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the list of connection opportunities available within the current Connection Type.
        /// </summary>
        public List<ListItemBag> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the list of connection states (e.g., Active, Inactive, Future Follow-up) available for filtering.
        /// </summary>
        public List<ListItemBag> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the list of request source items available for filtering connection requests by their originating source.
        /// </summary>
        public List<ListItemBag> RequestSourceItems { get; set; }

        /// <summary>
        /// Gets or sets the list of workflows that can be launched from connection requests.
        /// </summary>
        public List<ListItemBag> WorkflowItems { get; set; }

        /// <summary>
        /// Gets or sets the complete list of connectors that can be assigned to connection requests.
        /// </summary>
        public List<ListItemBag> AllPossibleConnectors { get; set; }

        /// <summary>
        /// Gets or sets the list of column options that control which data fields are displayed in the request grid.
        /// </summary>
        public List<ListItemBag> GridDataToShowItems { get; set; }

        /// <summary>
        /// Gets or sets the list of activity types available to log against connection requests.
        /// </summary>
        public List<ConnectionActivityTypeBag> ConnectionActivities { get; set; }

        /// <summary>
        /// Gets or sets the Connection Opportunity details resolved from the current filter state, used to populate the detail panel.
        /// </summary>
        public ConnectionOpportunityDetailBag ConnectionOpportunityDetailsFromFilter { get; set; }

        /// <summary>
        /// Gets or sets the attributes for Connection Request attributes specified at the Connection Type level.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, PublicAttributeBag> ConnectionTypeRequestAttributes { get; set; }
    }
}
