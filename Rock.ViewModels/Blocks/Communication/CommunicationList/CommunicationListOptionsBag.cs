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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationList
{
    /// <summary>
    /// The additional configuration options for the Communication List block.
    /// </summary>
    public class CommunicationListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user can approve new communications.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the can approve new communications; otherwise, <c>false</c>.
        /// </value>
        public bool CanApprove { get; set; }

        /// <summary>
        /// Gets or sets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public ListItemBag CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the communication types for the communication type filter dropdown.
        /// </summary>
        /// <value>
        /// The communication types.
        /// </value>
        public List<ListItemBag> CommunicationTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the status types for the status filter dropdown.
        /// </summary>
        /// <value>
        /// The statuses.
        /// </value>
        public List<ListItemBag> StatusItems { get; set; }
    }
}
