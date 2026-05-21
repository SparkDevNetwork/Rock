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

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The classic display settings for a check-in configuration. Controls how often the kiosk refreshes its
    /// check-in configuration and what is displayed on the success screen.
    /// </summary>
    public class CheckInClassicDisplaySettingsBag
    {
        /// <summary>
        /// Gets or sets how often (in seconds) the kiosk refreshes its check-in configuration from the
        /// server.
        /// </summary>
        public int? RefreshInterval { get; set; }

        /// <summary>
        /// Gets or sets how the success template is applied on the check-in success screen: never shown,
        /// replacing the default content, or appended to it.
        /// </summary>
        public SuccessLavaTemplateDisplayMode? SuccessTemplateDisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered on the check-in success screen when the success
        /// template display mode is set to Replace or Append.
        /// </summary>
        public string SuccessTemplate { get; set; }
    }
}
