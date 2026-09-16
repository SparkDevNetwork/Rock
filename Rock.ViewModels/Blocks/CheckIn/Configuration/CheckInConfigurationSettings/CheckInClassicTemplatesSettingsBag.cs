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
    /// The classic template settings for a check-in configuration. Customizes the Lava content shown on the
    /// start page, family selection screen, and person selection screen.
    /// </summary>
    public class CheckInClassicTemplatesSettingsBag
    {
        /// <summary>
        /// Gets or sets the Lava template rendered on the check-in start screen.
        /// </summary>
        public string StartTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered on the family selection screen.
        /// </summary>
        public string FamilySelectTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template rendered on the person selection screen.
        /// </summary>
        public string PersonSelectTemplate { get; set; }
    }
}
