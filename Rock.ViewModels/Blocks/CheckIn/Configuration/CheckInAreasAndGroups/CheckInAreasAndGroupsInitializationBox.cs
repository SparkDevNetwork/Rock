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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The box that contains all the initialization information for the check-in areas and groups block.
    /// </summary>
    public class CheckInAreasAndGroupsInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the display name of the parent check-in configuration. Used as the panel title.
        /// </summary>
        public string CheckInTypeName { get; set; }

        /// <summary>
        /// Gets or sets the top-level areas under this check-in configuration. Drives the area slicer dropdown.
        /// </summary>
        public List<ListItemBag> AreaItems { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the currently-selected area (empty / <c>null</c> for "All Areas"), resolved
        /// from the configuration-scoped person preference.
        /// </summary>
        public Guid? SelectedAreaGuid { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the check-in configuration. The client scopes the shared
        /// selected-area person preference to this configuration entity when persisting a new selection.
        /// </summary>
        public string ConfigurationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the initial areas-and-groups tree.
        /// </summary>
        public List<CheckInTreeNodeBag> Tree { get; set; }

        /// <summary>
        /// Gets or sets the check-in setup types selectable in the Area / Group editor's "Inherit Check-in
        /// Setup Type From" dropdown. Sourced from the configuration's check-in filter group types. The
        /// first entry is a "None" option (empty value) so the individual can clear the selection.
        /// </summary>
        public List<ListItemBag> InheritedGroupTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the public attribute schemas applicable when each check-in setup type is selected
        /// in the inherit-from dropdown. The outer key is the setup type's <see cref="System.Guid"/>; the
        /// inner key is the attribute key. Lets the editor swap the conditional well's AVC schema in
        /// response to the individual changing the dropdown without a server round-trip.
        /// </summary>
        public Dictionary<Guid, Dictionary<string, PublicAttributeBag>> InheritedAttributesByGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Classic Check-in Labels section should be shown on
        /// the Area editor.
        /// </summary>
        public bool IsClassicCheckInLabelsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a map of campus <see cref="System.Guid"/> to that campus's root named-location
        /// <see cref="System.Guid"/>. The Group editor uses it to scope the Named Locations picker to the
        /// active campus's location tree, so the individual can only attach locations that belong to the
        /// campus the slicer is set to.
        /// </summary>
        public Dictionary<string, string> CampusRootLocations { get; set; }
    }
}
