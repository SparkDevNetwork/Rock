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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The kiosk feature settings for a check-in configuration. Controls the basic kiosk-facing features
    /// available during check-in and check-out.
    /// </summary>
    public class CheckInKioskFeaturesSettingsBag
    {
        /// <summary>
        /// Gets or sets whether individuals may check themselves out at the kiosk.
        /// </summary>
        public bool AllowCheckoutAtKiosk { get; set; }

        /// <summary>
        /// Gets or sets whether "Can Check-in" relationships may be removed at the kiosk without a supervisor
        /// login. This action removes all known relationships marked as "Can Check-in." Next-gen check-in only.
        /// </summary>
        public bool EnableRemoveFamilyKiosk { get; set; }

        /// <summary>
        /// Gets or sets whether the attendance record stays pending until an assistant marks the individual as
        /// "Present" in the Check-in Manager.
        /// </summary>
        public bool EnablePresence { get; set; }
    }
}
