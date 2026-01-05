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
    /// Represents configurable options available for group placement settings,
    /// including attribute filters and registration instance selections.
    /// </summary>
    public class PlacementConfigurationSettingOptionsBag
    {
        /// <summary>
        /// A list of selectable source-side attributes that can be used for filtering or display.
        /// </summary>
        public List<ListItemBag> SourceAttributes { get; set; }

        /// <summary>
        /// A list of selectable attributes available on destination groups that can be used for filtering or configuration.
        /// </summary>
        public List<ListItemBag> DestinationGroupAttributes { get; set; }

        /// <summary>
        /// A list of selectable attributes available on destination group members for filtering or display purposes.
        /// </summary>
        public List<ListItemBag> DestinationGroupMemberAttributes { get; set; }

        /// <summary>
        /// A list of registration instances available for selection in placement scenarios using template-based modes.
        /// </summary>
        public List<ListItemBag> RegistrationInstances { get; set; }
    }

}
