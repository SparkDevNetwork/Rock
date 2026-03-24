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
    /// Represents the data required to create or update a connection request.
    /// </summary>
    public class ConnectionRequestBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of this connection request.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection opportunity this request belongs to.
        /// </summary>
        public string ConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the requester as a list item containing the person's value and display name.
        /// </summary>
        public ListItemBag Requester { get; set; }

        /// <summary>
        /// Gets or sets the Campus for the connection request.
        /// </summary>
        public string CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connector's person alias assigned to this request.
        /// </summary>
        public string ConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the connection state (e.g., Active, Inactive, Future Follow-up) of this request.
        /// </summary>
        public ConnectionState? ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date for this request when it is in the Future Follow-up state.
        /// </summary>
        public DateTimeOffset? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection status to assign to this request.
        /// </summary>
        public string ConnectionStatusGuid { get; set; }

        /// <summary>
        /// Gets or sets the note to record when the connection status history is updated.
        /// </summary>
        public string ConnectionStatusHistoryNote { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the placement group assigned to this request.
        /// </summary>
        public string PlacementGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the group member role to assign when placing the requester in the placement group.
        /// </summary>
        public string GroupMemberRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the group member status to assign when placing the requester in the placement group.
        /// </summary>
        public GroupMemberStatus? GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the comments or notes on this connection request.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the request source defined value for this connection request.
        /// </summary>
        public string RequestSourceGuid { get; set; }

        /// <summary>
        /// Gets or sets the public attribute definitions for the connection request attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ConnectionRequestAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values for the Connection Request.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> ConnectionRequestAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the public attribute definitions for the placement group member attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> PlacementGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values for the placement group member.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> PlacementGroupMemberAttributeValues { get; set; }
    }
}
