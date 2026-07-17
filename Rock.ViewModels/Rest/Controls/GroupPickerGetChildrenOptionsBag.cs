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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GroupPickerGetChildren API action
    /// of the Group Picker control (and Group Tree View).
    /// </summary>
    public class GroupPickerGetChildrenOptionsBag
    {
        /// <summary>
        /// Guid of the parent group whose children should be loaded.
        /// When null or empty, children of the root (or top-level groups) are returned.
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// GUID of the group you want to use as the root.
        /// </summary>
        public Guid? RootGroupGuid { get; set; }

        /// <summary>
        /// List of group type Guids to limit results to groups of those types.
        /// </summary>
        public List<Guid> IncludedGroupTypeGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Whether to include inactive groups or not.
        /// </summary>
        public bool IncludeInactiveGroups { get; set; } = false;

        /// <summary>
        /// When true, show no groups unless IncludedGroupTypeGuids has values.
        /// </summary>
        public bool ExcludeAllByDefault { get; set; } = false;

        /// <summary>
        /// Whether to limit to only groups that have scheduling enabled.
        /// </summary>
        public bool LimitToSchedulingEnabled { get; set; } = false;

        /// <summary>
        /// Whether to limit to only groups that have RSVPs enabled.
        /// </summary>
        public bool LimitToRSVPEnabled { get; set; } = false;

        /// <summary>
        /// Whether to limit results to security-role groups only.
        /// </summary>
        public bool LimitToSecurityRoleGroups { get; set; } = false;

        /// <summary>
        /// List of group type Guids to exclude. Only applied when
        /// <see cref="IncludedGroupTypeGuids"/> is empty.
        /// </summary>
        public List<Guid> ExcludedGroupTypeGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Optional campus Guid used to filter groups by campus.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// When a campus filter is set, whether to also include groups that have no campus.
        /// </summary>
        public bool IncludeNoCampus { get; set; } = false;

        /// <summary>
        /// Whether to limit results to public groups only.
        /// </summary>
        public bool LimitToPublic { get; set; } = false;

        /// <summary>
        /// The count mode to attach to each node: 0 = None, 1 = Child Groups, 2 = Group Members.
        /// Counts are written to <c>TreeItemBag.ChildCount</c>.
        /// </summary>
        public int CountsType { get; set; } = 0;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }

        /// <summary>
        /// Gets or sets the values that need to be expanded to. This is used
        /// when opening the tree view with an already selected value. Each
        /// selected value is included in this property. When getting the list
        /// of root items, you should automatically expand your results until
        /// each of these values is reached.
        /// </summary>

        public List<string> ExpandToValues { get; set; }
    }
}
