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
using Rock.Model;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.DeviceDetail
{
    public class DeviceBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a description of the device.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DefinedValue that represents the type of the device.
        /// </summary>
        public ListItemBag DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has camera.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has camera; otherwise, <c>false</c>.
        /// </value>
        public bool HasCamera { get; set; }

        /// <summary>
        /// Gets or sets the physical location or geographic fence for the device.
        /// </summary>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the device name. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the printer that is associated with this device. 
        /// </summary>
        public ListItemBag PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets where print jobs for this device originates from.
        /// </summary>
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// Gets or sets a flag that overrides which printer the print job is set to.
        /// </summary>
        public PrintTo PrintToOverride { get; set; }

        /// <summary>
        /// Gets or sets the geop point.
        /// </summary>
        /// <value>
        /// The geop point.
        /// </value>
        public string GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the geop fence.
        /// </summary>
        /// <value>
        /// The geop fence.
        /// </value>
        public string GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the type of the camera barcode configuration.
        /// </summary>
        /// <value>
        /// The type of the camera barcode configuration.
        /// </value>
        public CameraBarcodeConfiguration? CameraBarcodeConfigurationType { get; set; }

        /// <summary>
        /// Gets or sets the type of the kiosk.
        /// </summary>
        /// <value>
        /// The type of the kiosk.
        /// </value>
        public KioskType? KioskType { get; set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public List<ListItemBag> Locations { get; set; }
    }
}
