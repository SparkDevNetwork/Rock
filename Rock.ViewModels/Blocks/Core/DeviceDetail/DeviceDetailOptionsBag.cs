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

namespace Rock.ViewModels.Blocks.Core.DeviceDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class DeviceDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the kiosk type options.
        /// </summary>
        /// <value>
        /// The kiosk type options.
        /// </value>
        public List<ListItemBag> KioskTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the camera barcode configuration options.
        /// </summary>
        /// <value>
        /// The camera barcode configuration options.
        /// </value>
        public List<ListItemBag> CameraBarcodeConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets the printer options.
        /// </summary>
        /// <value>
        /// The printer options.
        /// </value>
        public List<ListItemBag> PrinterOptions { get; set; }

        /// <summary>
        /// Gets or sets the print from options.
        /// </summary>
        /// <value>
        /// The print from options.
        /// </value>
        public List<ListItemBag> PrintFromOptions { get; set; }

        /// <summary>
        /// Gets or sets the print to options.
        /// </summary>
        /// <value>
        /// The print to options.
        /// </value>
        public List<ListItemBag> PrintToOptions { get; set; }

        /// <summary>
        /// Gets or sets the map style value unique identifier.
        /// </summary>
        /// <value>
        /// The map style value unique identifier.
        /// </value>
        public string MapStyleValueGuid { get; set; }
    }
}
