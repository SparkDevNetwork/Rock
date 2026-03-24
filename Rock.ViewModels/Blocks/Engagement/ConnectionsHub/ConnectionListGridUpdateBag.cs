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

using Rock.Enums.Connection;
using Rock.Model;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the updated field values for a single row in the connection request grid after a server-side change.
    /// </summary>
    public class ConnectionListGridUpdateBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request this row represents.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the connection status column.
        /// </summary>
        public GroupingFieldBag StatusGrouping { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the connection state column.
        /// </summary>
        public GroupingFieldBag StateGrouping { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the connector column.
        /// </summary>
        public GroupingFieldBag ConnectorGrouping { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the connection opportunity column.
        /// </summary>
        public GroupingFieldBag OpportunityGrouping { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the campus column.
        /// </summary>
        public GroupingFieldBag CampusGrouping { get; set; }

        /// <summary>
        /// Gets or sets the grouping field data for the due status column.
        /// </summary>
        public GroupingFieldBag DueStatusGrouping { get; set; }

        /// <summary>
        /// Gets or sets the current connection status for this request.
        /// </summary>
        public ConnectionStatusBag ConnectionStatusBag { get; set; }

        /// <summary>
        /// Gets or sets the current connection state (e.g., Active, Inactive, Future Follow-up) for this request.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the currently assigned connector as a list item.
        /// </summary>
        public ListItemBag ConnectorDetails { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date for this request when it is in the Future Follow-up state.
        /// </summary>
        public DateTime? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the most recent activity logged against this request.
        /// </summary>
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the total number of activities logged against this request.
        /// </summary>
        public int? ActivityCount { get; set; }

        /// <summary>
        /// Gets or sets the celebration text to display for this request, if applicable.
        /// </summary>
        public string CelebrationText { get; set; }

        /// <summary>
        /// Gets or sets the due status indicating whether this request is on time, due soon, or overdue.
        /// </summary>
        public DueStatus DueStatus { get; set; }

        /// <summary>
        /// Gets or sets the date by which this request should be completed.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the date by which this request was completed.
        /// </summary>
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date from which this request is considered due soon.
        /// </summary>
        public DateTime? DueSoonDate { get; set; }
    }
}

