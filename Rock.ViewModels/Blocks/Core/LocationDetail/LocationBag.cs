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
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.LocationDetail
{
    /// <summary>
    /// Contains details of the location.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class LocationBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets threshold that will prevent checkin (no option to override)
        /// </summary>
        public string FirmRoomThreshold { get; set; }

        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        public ListItemBag Image { get; set; }

        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        public string ImageUrlParam { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if GeoPoint is locked (shouldn't be geocoded again)
        /// </summary>
        public bool? IsGeoPointLocked { get; set; }

        /// <summary>
        /// Gets or sets the Id of the LocationType Rock.Model.DefinedValue that is used to identify the type of Rock.Model.Location
        /// that this is. Examples: Campus, Building, Room, etc
        /// </summary>
        public ListItemBag LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Location's Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the if the location's parent Location. 
        /// </summary>
        public ListItemBag ParentLocation { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Device Id of the printer (if any) associated with the location.
        /// </summary>
        public ListItemBag PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets a threshold that will prevent checkin unless a manager overrides
        /// </summary>
        public string SoftRoomThreshold { get; set; }

        /// <summary>
        /// Gets or sets the address fields.
        /// </summary>
        /// <value>The address fields.</value>
        public AddressControlBag AddressFields { get; set; }

        /// <summary>
        /// Gets or sets the formatted HTML address.
        /// </summary>
        /// <value>The formatted HTML address.</value>
        public string FormattedHtmlAddress { get; set; }

        /// <summary>
        /// Gets or sets the GeoFence image HTML.
        /// </summary>
        /// <value>The GeoFence image HTML.</value>
        public string GeoFenceImageHtml { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint image HTML.
        /// </summary>
        /// <value>The GeoPoint image HTML.</value>
        public string GeoPointImageHtml { get; set; }

        /* Update these to whatever they need to be when the GeoPicker is ready */

        /// <summary>
        /// Gets or sets the geo point well known text.
        /// </summary>
        /// <value>
        /// The geo point well known text.
        /// </value>
        public string GeoPoint_WellKnownText { get; set; }

        /// <summary>
        /// Gets or sets the geo fence well known text.
        /// </summary>
        /// <value>
        /// The geo fence well known text.
        /// </value>
        public string GeoFence_WellKnownText { get; set; }

        /// <summary>
        /// Gets or sets the guid.
        /// </summary>
        /// <value>
        /// The guid.
        /// </value>
        public Guid Guid { get; set; }
    }
}
