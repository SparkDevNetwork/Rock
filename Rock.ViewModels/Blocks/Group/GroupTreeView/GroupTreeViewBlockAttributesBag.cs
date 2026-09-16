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
    /// The Group Tree View block's configured settings.
    /// </summary>
    public class GroupTreeViewBlockAttributesBag
    {
        /// <summary>
        /// Gets or sets the optional title for the panel that wraps the tree.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the group the tree treats as its root, when one is configured.
        /// </summary>
        public Guid? RootGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the group types to include. When empty, all navigation-visible types
        /// are included except those in <see cref="ExcludedGroupTypeGuids"/>.
        /// </summary>
        public List<Guid> IncludedGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the group types to exclude. Only effective when
        /// <see cref="IncludedGroupTypeGuids"/> is empty.
        /// </summary>
        public List<Guid> ExcludedGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets whether the tree is limited to security-role groups.
        /// </summary>
        public bool LimitToSecurityRoleGroups { get; set; }

        /// <summary>
        /// Gets or sets whether the settings panel (filters / counts) is shown.
        /// </summary>
        public bool ShowSettingsPanel { get; set; }

        /// <summary>
        /// Gets or sets whether inactive campuses are included in the campus filter list.
        /// </summary>
        public bool DisplayInactiveCampuses { get; set; }

        /// <summary>
        /// Gets or sets whether auto-selecting the first group when none is selected is disabled.
        /// </summary>
        public bool DisableAutoSelectFirstGroup { get; set; }

        /// <summary>
        /// Gets or sets the block attribute default for counts when the person has no
        /// saved Show Count For preference: 0 = None, 1 = Child Groups, 2 = Group Members.
        /// </summary>
        public int InitialCountSetting { get; set; }
    }
}
