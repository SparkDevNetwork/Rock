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

using Rock.Model;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Group.GroupMember
{
    /// <summary>
    /// Details about an group member record that is transmitted over
    /// the RealTime engine.
    /// </summary>
    public class GroupMemberUpdatedMessageBag
    {
        /// <summary>
        /// The encrypted identifier key for the group.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group type.
        /// </summary>
        public string GroupTypeIdKey { get; set; }

        /// <summary>
        /// The GUID of the group.
        /// </summary>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the group member unique identifier. (TODO: Remove
        /// this when Group Member Detail is converted to Obsidian.)
        /// </summary>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group member.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// The GUID of the group member.
        /// </summary>
        public Guid GroupMemberGuid { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group role.
        /// </summary>
        public string GroupRoleIdKey { get; set; }

        /// <summary>
        /// The person associated with the group member.
        /// </summary>
        public PersonBag Person { get; set; }

    }
}
