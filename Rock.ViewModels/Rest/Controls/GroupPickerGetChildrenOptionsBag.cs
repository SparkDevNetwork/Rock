// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to { get; set; } software
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
    /// The options that can be passed to the GetGroupMembers API action of
    /// the GroupMemberPicker control.
    /// </summary>
    public class GroupPickerGetChildrenOptionsBag
    {
        /// <summary>
        /// GUID of the group the member is part of.
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// GUID of the group you want to use as the root.
        /// </summary>
        public Guid? RootGroupGuid { get; set; }

        /// <summary>
        /// List of group types IDs to limit to groups of those types.
        /// </summary>
        public List<Guid> IncludedGroupTypeGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Whether to include inactive groups or not.
        /// </summary>
        public bool IncludeInactiveGroups { get; set; } = false;

        /// <summary>
        /// Whether to limit to only groups that have scheduling enabled.
        /// </summary>
        public bool LimitToSchedulingEnabled { get; set; } = false;

        /// <summary>
        /// Whether to limit to only groups that have RSVPs enabled.
        /// </summary>
        public bool LimitToRSVPEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
