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

using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains filters information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardFiltersBag
    {
        /// <summary>
        /// Gets or sets the "connector" person alias identifier to be used to filter connection requests.
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person alias list item bag to be used with a person picker, in order to filter connection requests.
        /// </summary>
        public ListItemBag Requester { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person alias identifier to be used with a preexisting v1 API endpoint, in order to filter connection requests.
        /// </summary>
        public int? RequesterPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person identifier to be used with person preferences, in order to filter connection requests.
        /// </summary>
        public int? RequesterPersonId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier to be used to filter various aspects of the connection request board.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date range to be used to filter connection requests (using the last activity date).
        /// </summary>
        public SlidingDateRangeBag DateRange { get; set; }

        /// <summary>
        /// Gets or sets the minimum date to be used to filter connection requests (using the last activity date).
        /// </summary>
        public DateTimeOffset? MinDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum date to be used to filter connection requests (using the last activity date).
        /// </summary>
        public DateTimeOffset? MaxDate { get; set; }

        /// <summary>
        /// Gets or sets whether to include only connection requests that are "due today or already past due".
        /// </summary>
        public bool PastDueOnly { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses to be used to filter connection requests.
        /// </summary>
        public List<string> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection states to be used to filter connection requests.
        /// </summary>
        public List<string> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the connection activity types to be used to filter connection requests.
        /// </summary>
        public List<string> ConnectionActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the model property (and direction: ascending vs descending) to be used for sorting connection requests.
        /// </summary>
        public ConnectionRequestViewModelSortProperty SortProperty { get; set; }
    }
}
