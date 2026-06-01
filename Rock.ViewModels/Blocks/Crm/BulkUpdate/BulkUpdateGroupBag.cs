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

using Rock.Enums.Crm;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a group update operation in a bulk update save request.
    /// </summary>
    public class BulkUpdateGroupBag
    {
        /// <summary>
        /// Gets or sets the action to perform on the group.
        /// </summary>
        public BulkUpdateActionSpecifier Action { get; set; }

        /// <summary>
        /// Gets or sets the group unique identifier.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the group role unique identifier.
        /// </summary>
        public ListItemBag GroupRole { get; set; }

        /// <summary>
        /// Gets or sets the member status.
        /// </summary>
        public Rock.Model.GroupMemberStatus? MemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the member attributes.
        /// </summary>
        public Dictionary<string, string> MemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets which group member fields the user toggled on for update
        /// when the action is <see cref="BulkUpdateActionSpecifier.Update"/>.
        /// </summary>
        public Dictionary<string, bool> UpdatedFields { get; set; }
    }
}
