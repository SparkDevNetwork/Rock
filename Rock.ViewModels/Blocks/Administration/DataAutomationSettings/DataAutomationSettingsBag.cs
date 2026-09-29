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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// The complete set of editable data automation settings. This is the
    /// payload the block loads on initialization and posts back when saving.
    /// </summary>
    public class DataAutomationSettingsBag
    {
        /// <summary>
        /// Gets or sets the minimum confidence level required to automatically set a blank gender.
        /// A value of zero disables automatic gender determination.
        /// </summary>
        public double? GenderAutoFillConfidence { get; set; }

        /// <summary>
        /// Gets or sets the reactivate people settings.
        /// </summary>
        public ReactivatePeopleSettingsBag ReactivatePeople { get; set; }

        /// <summary>
        /// Gets or sets the inactivate people settings.
        /// </summary>
        public InactivatePeopleSettingsBag InactivatePeople { get; set; }

        /// <summary>
        /// Gets or sets the update family campus settings.
        /// </summary>
        public UpdateFamilyCampusSettingsBag UpdateFamilyCampus { get; set; }

        /// <summary>
        /// Gets or sets the move adult children settings.
        /// </summary>
        public MoveAdultChildrenSettingsBag MoveAdultChildren { get; set; }

        /// <summary>
        /// Gets or sets the update connection status settings.
        /// </summary>
        public UpdateConnectionStatusSettingsBag UpdateConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the update family status settings.
        /// </summary>
        public UpdateFamilyStatusSettingsBag UpdateFamilyStatus { get; set; }
    }
}
