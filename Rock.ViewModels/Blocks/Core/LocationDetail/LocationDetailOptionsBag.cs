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

namespace Rock.ViewModels.Blocks.Core.LocationDetail
{
    /// <summary>
    /// Contains extra configuration details for the block.
    /// </summary>
    public class LocationDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the printer device options.
        /// </summary>
        /// <value>
        /// The printer device options.
        /// </value>
        public List<ListItemBag> PrinterDeviceOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the a PersonId was passed as a URL parameter
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has person identifier; otherwise, <c>false</c>.
        /// </value>
        public bool HasPersonId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a parent Id was passed in the URL indicating the block is in TreeView.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in TreeView; otherwise, <c>false</c>.
        /// </value>
        public bool HasParentLocationId { get; set; }

        /// <summary>
        /// Gets or sets the panel title.
        /// </summary>
        /// <value>
        /// The panel title.
        /// </value>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the map style unique identifier.
        /// </summary>
        /// <value>
        /// The map style unique identifier.
        /// </value>
        public Guid MapStyleGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current user has Administrate authorizartion
        /// </summary>
        /// <value>
        ///   <c>true</c> if user can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the location has NamedLocationFeaturesEnabled set to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the location has named location features values enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonIdAvailable { get; set; }
    }
}
