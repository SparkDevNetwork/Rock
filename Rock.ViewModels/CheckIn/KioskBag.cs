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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Details about a single kiosk device.
    /// </summary>
    public class KioskBag : CheckInItemBag
    {
        /// <summary>
        /// Gets or sets the kiosk type.
        /// </summary>
        /// <value>The kiosk type.</value>
        public KioskType? Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this kiosk has the camera enabled.
        /// </summary>
        /// <value><c>true</c> if the camera is enabled; otherwise, <c>false</c>.</value>
        public bool IsCameraEnabled { get; set; }

        /// <summary>
        /// Determines how the camera should operate on this kiosk device.
        /// </summary>
        public CameraBarcodeConfiguration CameraMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this kiosk supports
        /// registration mode.
        /// </summary>
        /// <value><c>true</c> if this kiosk supports registration mode; otherwise, <c>false</c>.</value>
        public bool IsRegistrationModeEnabled { get; set; }
    }
}
