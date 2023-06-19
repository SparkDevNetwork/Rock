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
    /// The options that can be passed to the GetGroupMembers API action of
    /// the GroupMemberPicker control.
    /// </summary>
    public class GroupRolePickerGetAllForGroupRoleOptionsBag
    {
        /// <summary>
        /// Unique identifier of a group role. Used to get the rest of the data
        /// about this role, and the group type it belongs to.
        /// </summary>
        public Guid GroupRoleGuid { get; set; }

        /// <summary>
        /// Exclude group roles from the returned list that match any of the given identifiers.
        /// </summary>
        public List<Guid> ExcludeGroupRoles { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
