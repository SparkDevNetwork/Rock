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
    /// Represents the full detail data for a connection request as displayed in the request detail panel.
    /// </summary>
    public class ConnectionRequestDetailsBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of this connection request.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the profile data for the person who submitted this connection request.
        /// </summary>
        public RequesterPersonBag RequesterPerson { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the requester's person alias.
        /// </summary>
        public Guid RequesterPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the list of other open connection requests associated with the same requester.
        /// </summary>
        public List<AdditionalRequestBag> AdditionalRequests { get; set; }

        /// <summary>
        /// Gets or sets the current connection state (e.g., Active, Inactive, Future Follow-up) of this request.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the current connection status of this request.
        /// </summary>
        public ConnectionStatusBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date for this request when it is in the Future Follow-up state.
        /// </summary>
        public DateTimeOffset? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection opportunity this request belongs to.
        /// </summary>
        public string ConnectionOpportunityName { get; set; }

        /// <summary>
        /// Gets or sets the CSS icon class for the connection opportunity this request belongs to.
        /// </summary>
        public string ConnectionOpportunityIcon { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus selected on this connection request.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector person assigned to this request.
        /// </summary>
        public string ConnectorPerson { get; set; }

        /// <summary>
        /// Gets or sets the date and time this connection request was created.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date by which this request should be completed.
        /// </summary>
        public DateTimeOffset? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the due status indicating whether this request is on time, due soon, or overdue.
        /// </summary>
        public DueStatus DueStatus { get; set; }

        /// <summary>
        /// Gets or sets the list of action items (e.g., workflows) available for this connection request.
        /// </summary>
        public List<ListItemBag> ActionItems { get; set; }

        /// <summary>
        /// Gets or sets the comments or notes on this connection request.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the display value of the request source defined value for this request.
        /// </summary>
        public string ConnectionTypeSource { get; set; }

        /// <summary>
        /// Gets or sets the celebration text to display for this request, if applicable.
        /// </summary>
        public string CelebrationText { get; set; }

        /// <summary>
        /// Gets or sets the number of active reminders set on this connection request.
        /// </summary>
        public int ReminderCount { get; set; }

        /// <summary>
        /// Gets or sets the placement group details for the group assigned to this request.
        /// </summary>
        public PlacementGroupDetailsBag PlacementGroup { get; set; }

        /// <summary>
        /// Gets or sets the list of person notes associated with the requester.
        /// </summary>
        public List<PersonNoteBag> PersonNotes { get; set; }

        /// <summary>
        /// Gets or sets the list of activity feed entries for this connection request.
        /// </summary>
        public List<ActivityEntryBag> ActivityEntries { get; set; }

        /// <summary>
        /// The attributes for the selected Connection Request.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// The attribute values for the selected Connection Request.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
