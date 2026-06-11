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
    /// Represents a connector option for selection in the Connections Hub, including a profile photo for display in the connector picker.
    /// </summary>
    public class ConnectorItemBag
    {
        /// <summary>
        /// Gets or sets the list item containing the connector's identifier value and display name.
        /// </summary>
        public ListItemBag ListItemBag { get; set; }

        /// <summary>
        /// Gets or sets the URL of the connector's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether this connector is available to all campuses (true) or only to specific campuses (false).
        /// </summary>
        public bool IsAvailableToAllCampuses { get; set; }

        /// <summary>
        /// Gets or sets the list of campus identifiers that this connector is associated with.
        /// </summary>
        public List<Guid> CampusGuids { get; set; }
    }
}
