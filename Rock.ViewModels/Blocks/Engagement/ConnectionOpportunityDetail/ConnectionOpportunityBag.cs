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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityDetail
{
    /// <summary>
    /// The item details for the Connection Opportunity Detail block.
    /// </summary>
    public class ConnectionOpportunityBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BinaryFile that contains the Opportunity's photo.
        /// </summary>
        public ListItemBag Photo { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show campus on transfer].
        /// </summary>
        public bool ShowCampusOnTransfer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show connect button].
        /// </summary>
        public bool ShowConnectButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show status on transfer].
        /// </summary>
        public bool ShowStatusOnTransfer { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the name of the associated connection type.
        /// </summary>
        public string ConnectionTypeName { get; set; }

        /// <summary>
        /// Gets or sets the url of the connection type.
        /// </summary>
        public string ConnectionTypeUrl { get; set; }

        /// <summary>
        ///  Gets or sets the connection request attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> ConnectionRequestAttributes { get; set; }

        /// <summary>
        /// Gets or sets the inherited connection request attributes.
        /// </summary>
        /// <value>
        /// The inherited connection request attributes.
        /// </value>
        public List<InheritedAttributeBag> InheritedConnectionRequestAttributes { get; set; }

        /// <summary>
        /// Minimal list of placement group configs for the selected opportunity.
        /// </summary>
        public List<PlacementGroupConfigBag> PlacementGroupConfigs { get; set; }

        /// <summary>
        /// Minimal list of placement groups (Group as ListItemBag) for the selected opportunity.
        /// </summary>
        public List<PlacementGroupBag> PlacementGroups { get; set; }

        /// <summary>
        /// Minimal list of connector groups (Group as ListItemBag) for the selected opportunity.
        /// </summary>
        public List<ConnectorGroupBag> ConnectorGroups { get; set; }

        /// <summary>
        /// Minimal list of workflow types associated to the selected opportunity.
        /// </summary>
        public List<ConnectionWorkflowBag> ConnectionWorkflows { get; set; }

        /// <summary>
        /// Minimal list of workflow types inherited from the connection type.
        /// </summary>
        public List<InheritedConnectionWorkflowBag> InheritedConnectionWorkflows { get; set; }

        /// <summary>
        /// List of all the campuses that this connection opportunity relates to.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets a map of ConnectionOpportunityCampus.Id to the currently assigned default connector person (by alias id).
        /// </summary>
        public Dictionary<Guid, ListItemBag> DefaultConnectors { get; set; }
    }
}
