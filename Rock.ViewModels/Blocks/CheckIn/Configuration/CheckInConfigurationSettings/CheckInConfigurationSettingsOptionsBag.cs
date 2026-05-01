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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The additional configuration options for the Check-in Configuration Settings block.
    /// </summary>
    public class CheckInConfigurationSettingsOptionsBag
    {
         /// <summary>
         /// Gets or sets whether the block should be hidden.
         /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets whether to show show classic check-in settings for this configuration.
        /// </summary>
        public bool ShowClassicCheckInSettings { get; set; }

        /// <summary>
        /// Gets or sets the available content channels that can be selected for the
        /// <see cref="CheckInDisplaySettingsBag.PromotionsContentChannelGuid"/> property.
        /// </summary>
        public List<ListItemBag> PromotionsContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the available search type options that can be selected for the
        /// <see cref="CheckInSearchSettingsBag.SearchType"/> property.
        /// </summary>
        public List<ListItemBag> SearchTypes { get; set; }

        /// <summary>
        /// Gets or sets the available achievement types that can be selected for the
        /// <see cref="CheckInDisplaySettingsBag.AchievementTypes"/> property.
        /// </summary>
        public List<ListItemBag> AchievementTypes { get; set; }

        /// <summary>
        /// Gets or sets the available person attributes that can be selected for the
        /// required/optional adult and child attribute pickers during registration.
        /// </summary>
        public List<ListItemBag> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the available family (group) attributes that can be selected for the
        /// required/optional family attribute pickers during registration.
        /// </summary>
        public List<ListItemBag> FamilyAttributes { get; set; }

        /// <summary>
        /// Gets or sets the available known-relationship roles (plus the synthetic "Child" entry with a value
        /// of "0") that can be selected in the Child Relationship Settings stack. Values are role IDs as
        /// strings, matching the legacy comma-delimited storage format on the related GroupType attributes.
        /// </summary>
        public List<ListItemBag> RelationshipTypes { get; set; }

        /// <summary>
        /// Gets or sets the flat list of leaf property names found inside the nested sub-bags of
        /// <see cref="CheckInConfigurationSettingsBag"/>. The client uses this list to seed the ValidProperties of its
        /// edit box so that server-side <c>IfValidProperty</c> checks (which reference leaf names) resolve correctly.
        /// </summary>
        public List<string> ValidProperties { get; set; }
    }
}
