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

namespace Rock.ViewModels.Blocks.Group.GroupTreeView
{
    /// <summary>
    /// The runtime data the Group Tree View block ships to its Obsidian component.
    /// </summary>
    public class GroupTreeViewBag
    {
        /// <summary>
        /// Gets or sets the groups selected on load, resolved from the page parameter for deep-linking.
        /// </summary>
        public List<Guid> SelectedGroupGuids { get; set; }

        /// <summary>
        /// Gets or sets the groups to expand on load.
        /// </summary>
        public List<Guid> ExpandedGroupGuids { get; set; }

        /// <summary>
        /// Gets or sets an error message to display in place of the tree.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the add-group chrome is shown (block edit or elevated child-add auth).
        /// </summary>
        public bool IsAddGroupVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Top-Level action is enabled.
        /// </summary>
        public bool IsAddRootEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Child To Selected action is enabled for the current selection.
        /// </summary>
        public bool IsAddChildEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether inactive groups are hidden, reflecting the person's saved preference
        /// (or the block's initial active setting when no preference is stored).
        /// </summary>
        public bool HideInactiveGroups { get; set; }

        /// <summary>
        /// Gets or sets whether the tree is limited to public groups.
        /// </summary>
        public bool LimitToPublic { get; set; }

        /// <summary>
        /// Gets or sets the counts mode: 0 = None, 1 = Child Groups, 2 = Group Members.
        /// </summary>
        public int CountsType { get; set; }

        /// <summary>
        /// Gets or sets the campus Guid used to filter the tree, when a campus filter is active.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets whether groups with no campus are included when a campus filter is active.
        /// </summary>
        public bool IncludeNoCampus { get; set; }

        /// <summary>
        /// Gets or sets a URL the client should navigate to immediately when no group is selected
        /// and auto-select first group is enabled. Null when no redirect is needed.
        /// </summary>
        public string AutoSelectUrl { get; set; }
    }
}
