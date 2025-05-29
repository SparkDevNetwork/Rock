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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents the configuration settings used to control various display and filtering behaviors 
    /// in the group placement block.
    /// </summary>
    public class PlacementConfigurationSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show registration instance name].
        /// Note this setting only applies when in Template Mode
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show registration instance name]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowRegistrationInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the included registration instance ids.
        /// Note this setting only applies when in Template Mode
        /// </summary>
        /// <value>
        /// The included registration instance ids.
        /// </value>
        public List<string> IncludedRegistrationInstanceIds { get; set; }

        /// <summary>
        /// Gets or sets the displayed campus identifier.
        /// </summary>
        /// <value>
        /// The displayed campus identifier.
        /// </value>
        public ListItemBag DisplayedCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show fees].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show fees]; otherwise, <c>false</c>.
        /// </value>
        public bool AreFeesDisplayed { get; set; }

        /// <summary>
        /// Gets or sets the displayed registrant attribute ids.
        /// </summary>
        /// <value>
        /// The displayed registrant attribute ids.
        /// </value>
        public List<string> SourceAttributesToDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether source attributes should also be displayed 
        /// on destination group members for comparison or reference.
        /// </summary>
        /// <value><c>true</c> if source attributes should be shown on destination group members; otherwise, <c>false</c>.</value>
        public bool AreSourceAttributesDisplayedOnDestinationGroupMembers { get; set; }

        /// <summary>
        /// Gets or sets the displayed group attribute ids.
        /// </summary>
        /// <value>
        /// The displayed group attribute ids.
        /// </value>
        public List<string> DestinationGroupAttributesToDisplay { get; set; }

        /// <summary>
        /// Gets or sets the displayed group member attribute ids.
        /// </summary>
        /// <value>
        /// The displayed group member attribute ids.
        /// </value>
        public List<string> DestinationGroupMemberAttributesToDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide full groups].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide full groups]; otherwise, <c>false</c>.
        /// </value>
        public bool AreFullGroupsHidden { get; set; }
    }
}
