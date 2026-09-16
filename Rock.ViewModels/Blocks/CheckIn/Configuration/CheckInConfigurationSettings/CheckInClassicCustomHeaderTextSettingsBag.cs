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
    /// The classic custom header text settings for a check-in configuration. Configures the header text
    /// rendered at each step of the check-in process.
    /// </summary>
    public class CheckInClassicCustomHeaderTextSettingsBag
    {
        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the action select screen.
        /// </summary>
        public string ActionSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the check-out person select screen.
        /// </summary>
        public string CheckoutPersonSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the person select screen.
        /// </summary>
        public string PersonSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the multi-person select screen.
        /// </summary>
        public string MultiPersonSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the group type select screen.
        /// </summary>
        public string GroupTypeSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the time select screen.
        /// </summary>
        public string TimeSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the ability level select screen.
        /// </summary>
        public string AbilityLevelSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the location select screen.
        /// </summary>
        public string LocationSelectHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered as the header on the group select screen.
        /// </summary>
        public string GroupSelectHeaderTemplate { get; set; }
    }
}
