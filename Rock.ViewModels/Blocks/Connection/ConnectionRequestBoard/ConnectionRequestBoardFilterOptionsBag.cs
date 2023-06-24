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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains filter options information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardFilterOptionsBag
    {
        /// <summary>
        /// Gets or sets the "connector" people that may be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> Connectors { get; set; }

        /// <summary>
        /// Gets or sets the campuses that may be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses that may be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection states that may be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> ConnectionStates { get; set; }

        /// <summary>
        /// Gets or sets the connection activity types that may be used to filter connection requests.
        /// </summary>
        public List<ListItemBag> ConnectionActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the properties that may be used to sort connection requests.
        /// </summary>
        public List<ConnectionRequestBoardSortPropertyBag> SortProperties { get; set; }
    }
}
