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
    /// The supervision settings for a check-in configuration. Configures which capabilities supervisors and
    /// check-in managers have access to during check-in.
    /// </summary>
    public class CheckInSupervisionSettingsBag
    {
        /// <summary>
        /// Gets or sets whether the welcome screen shows an option (behind a passcode) to open the
        /// management screen.
        /// </summary>
        public bool EnableManager { get; set; }

        /// <summary>
        /// Gets or sets whether a supervisor may override age and/or grade requirements when checking a
        /// person in from the Check-in Manager.
        /// </summary>
        public bool EnableOverride { get; set; }

        /// <summary>
        /// Gets or sets whether check-out is available from the Check-in Manager.
        /// </summary>
        public bool AllowCheckoutInManager { get; set; }
    }
}
