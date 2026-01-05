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
    /// Options bag for the InGroupFilter's GetGroupRolesForGroups method.
    /// </summary>
    public class InGroupFilterGetGroupRolesForGroupsOptionsBag
    {
        /// <summary>
        /// Guids of the groups that are of the group types we wish to get roles for.
        /// </summary>
        public List<Guid> GroupGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Whether or not to include the direct child groups of the groups listed in GroupGuids.
        /// If this is true and IncludeSelectedGroups is false, then the groups in GroupGuids will be excluded.
        /// </summary>
        public bool IncludeChildGroups { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then the groups in GroupGuids
        /// will be included along with the child groups when determining the group types.
        /// </summary>
        public bool IncludeSelectedGroups { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then also include the child groups
        /// all the way down the hierarchy when determining the group types.
        /// </summary>
        public bool IncludeAllDescendants { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then also include inactive groups
        /// </summary>
        public bool IncludeInactiveGroups { get; set; } = false;

        /// <summary>
        /// The security grant token.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
