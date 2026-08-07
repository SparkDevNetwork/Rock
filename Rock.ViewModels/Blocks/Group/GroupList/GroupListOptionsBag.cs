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

namespace Rock.ViewModels.Blocks.Group.GroupList
{
    /// <summary>
    /// The additional configuration options for the Group List block.
    /// </summary>
    public class GroupListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block is in person mode
        /// (i.e., a Person context entity is present and the grid lists groups
        /// the person is a member of rather than a plain group list).
        /// </summary>
        public bool IsPersonMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Type column should be shown.
        /// </summary>
        public bool ShowGroupTypeColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Description column should be shown.
        /// </summary>
        public bool ShowDescriptionColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Active Status column should be shown.
        /// </summary>
        public bool ShowActiveStatusColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Member Count column should be shown.
        /// Only relevant in GroupList mode.
        /// </summary>
        public bool ShowMemberCountColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the System column should be shown.
        /// </summary>
        public bool ShowSystemColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Security column should be shown.
        /// </summary>
        public bool ShowSecurityColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Elevated Security Level column
        /// should be shown. Only true when the block is limited to security role groups
        /// and is in GroupList mode (not person mode).
        /// </summary>
        public bool ShowElevatedSecurityColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the full group ancestor path
        /// instead of just the group name.
        /// </summary>
        public bool ShowGroupPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter panel should be shown.
        /// </summary>
        public bool ShowFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the active status filter dropdown
        /// should be shown inside the filter panel. It is hidden when the block's
        /// LimitToActiveStatus attribute already restricts to active or inactive only.
        /// </summary>
        public bool ShowActiveFilter { get; set; }

        /// <summary>
        /// Gets or sets the panel title override. When null or empty the title is
        /// auto-generated from the group type(s) or block name.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the panel icon CSS class override. When null or empty the icon
        /// is auto-generated from the group type or a default icon.
        /// </summary>
        public string PanelIcon { get; set; }

        /// <summary>
        /// Gets or sets the group picker type attribute value: "GroupPicker" or "Dropdown".
        /// </summary>
        public string GroupPickerType { get; set; }

        /// <summary>
        /// Gets or sets the root group Guid used as the tree starting point when the
        /// GroupPickerType is "GroupPicker". The Obsidian GroupPicker control expects
        /// a Guid (not an Id or IdKey), so this is stored as the raw Guid the block
        /// attribute already returns from <c>AsGuidOrNull()</c>.
        /// </summary>
        public Guid? RootGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the GroupType Guids the add-member tree picker should be
        /// restricted to. Mirrors the WebForms <c>gpGroup.IncludedGroupTypeIds</c>
        /// setup so users can only browse groups of the block's configured types.
        /// </summary>
        public List<Guid> IncludedGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the available group type items for the filter panel.
        /// Only populated when ShowFilter is true and more than one group type is available.
        /// </summary>
        public List<ListItemBag> FilterGroupTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the available group type purpose items for the filter panel.
        /// </summary>
        public List<ListItemBag> FilterPurposeItems { get; set; }
    }
}
