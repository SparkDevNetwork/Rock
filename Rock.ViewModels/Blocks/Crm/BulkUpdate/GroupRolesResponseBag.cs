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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Response payload for the GetGroupRoles block action. Carries the
    /// picked group's group type identifier (so follow-up lookups can be
    /// scoped to the same type) plus the role options for the role dropdown.
    /// </summary>
    public class GroupRolesResponseBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group's group type.
        /// </summary>
        public string GroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the role options available for the picked group.
        /// </summary>
        public List<ListItemBag> Roles { get; set; }
    }
}
