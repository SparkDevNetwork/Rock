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
using Rock.Enums.Mobile;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonalDevices
{
    /// <summary>
    /// Represents a single personal device.
    /// </summary>
    public class PersonalDeviceListItemBag
    {
        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the item.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        public ListItemBag DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the icon to display for this device.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the device platform.
        /// </summary>
        public ListItemBag Platform { get; set; }

        /// <summary>
        /// Gets or sets the device version.
        /// </summary>
        public string DeviceVersion { get; set; }

        /// <summary>
        /// Gets or sets the MAC address, if provided.
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notifications are enabled.
        /// </summary>
        public bool NotificationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the location permission status.
        /// </summary>
        public LocationPermissionStatus LocationPermissionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether precise location is enabled.
        /// </summary>
        public bool IsPreciseLocationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this device is enabled for beacon monitoring.
        /// </summary>
        public bool IsBeaconMonitoringEnabled { get; set; }

        /// <summary>
        /// Gets or sets when the device was created (discovered).
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets when the device was last seen by the server.
        /// </summary>
        public DateTime? LastSeenDateTime { get; set; }
    }
}

