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
        /// Gets or sets a list of Campus Labels
        /// </summary>
        public List<CampusLabelBag> CampusLabels { get; set; }

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
        public List<ConnectionWorkflowBag> WorkflowItems { get; set; }

        /// <summary>
        /// Gets or sets the complete list of connectors that can be assigned to connection requests.
        /// </summary>
        public List<ListItemBag> AllPossibleConnectors { get; set; }

        /// <summary>
        /// Gets or sets the list of column options that control which data fields are displayed in the request grid.
        /// </summary>
        public List<GridDataToShowItemBag> GridDataToShowItems { get; set; }

        /// <summary>
        /// Gets or sets the list of activity types available to log against connection requests.
        /// </summary>
        public List<ConnectionActivityTypeBag> ConnectionActivities { get; set; }

        /// <summary>
        /// Gets or sets the Connection Opportunity details resolved from the current filter state, used to populate the detail panel.
        /// </summary>
        public ConnectionOpportunityDetailBag ConnectionOpportunityDetailsFromFilter { get; set; }

        /// <summary>
        /// Gets or sets the enabled views for this connection type.
        /// </summary>
        public EnabledViewFlags EnabledViews { get; set; }

        /// <summary>
        /// Gets whether list view is enabled for this connection type.
        /// </summary>
        public bool IsListViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.List );

        /// <summary>
        /// Gets whether board view is enabled for this connection type.
        /// </summary>
        public bool IsBoardViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.Board );

        /// <summary>
        /// Gets whether grid view is enabled for this connection type.
        /// </summary>
        public bool IsGridViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.Grid );

        /// <summary>
        /// Gets or sets the available groupings for each grouping dimension. The dictionary is keyed
        /// by grouping field name (e.g., "statusGroupingKey") and contains the complete list of
        /// possible <see cref="GroupingFieldBag"/> values for that dimension, including groups that
        /// may not have any data rows.
        /// </summary>
        public Dictionary<string, List<GroupingFieldBag>> AvailableGroupings { get; set; }

        /// <summary>
        /// Gets or sets the list of attributes available for filtering the connection request grid.
        /// Each entry pairs the public attribute (used to render the <c>RockAttributeFilter</c>
        /// control in the View Options modal) with the Connection Opportunity Guids that scope it,
        /// so the client can hide filters for attributes that do not apply to the active opportunity.
        /// </summary>
        public List<ConnectionRequestAttributeFilterBag> AttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current view is the "My Connections" view.
        /// </summary>
        public bool IsMyConnectionsView { get; set; }

        /// <summary>
        /// Gets or sets the list of connection types for the context slicer filter in the "My Connections" view.
        /// </summary>
        public List<ListItemBag> ConnectionTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the options for each Connection Type, keyed by the Connection Type's encrypted identifier key.
        /// Populated with a single entry in standard mode and with one entry per active Connection Type in My Connections mode.
        /// This allows the client to adjust its behavior and available UI actions based on the Connection Type that owns
        /// the currently selected request or opportunity.
        /// TODO - Migrate remaining consumers off the flat per-Type fields on this bag (ConnectionStatuses, RequestSourceItems,
        /// IsSequentialStatusMode, AllPossibleConnectors, ConnectionActivities, WorkflowItems, ConnectionOpportunities) so they
        /// can be removed.
        /// </summary>
        public Dictionary<string, ConnectionTypeOptionsBag> ConnectionTypeOptionsByIdKey { get; set; }
    }
}
