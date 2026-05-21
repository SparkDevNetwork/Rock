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
    /// The special needs settings for a check-in configuration. Configures check-in settings to accommodate
    /// individuals with special needs.
    /// </summary>
    public class CheckInSpecialNeedsSettingsBag
    {
        /// <summary>
        /// Gets or sets whether groups designated as special needs are hidden from the list of available
        /// check-in opportunities.
        /// </summary>
        public bool HideSpecialNeedsGroups { get; set; }

        /// <summary>
        /// Gets or sets whether groups not designated as special needs are hidden from the list of available
        /// check-in opportunities.
        /// </summary>
        public bool HideNonSpecialNeedsGroups { get; set; }
    }
}
