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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// A bag of data representing a group in the placement process.
    /// </summary>
    public class PlacementGroupBag
    {
        /// <summary>
        /// The group ID.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// The group GUID.
        /// </summary>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// The group ID key.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// The group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The group order.
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// The group type ID key.
        /// </summary>
        public string GroupTypeIdKey { get; set; }

        /// <summary>
        /// The group capacity.
        /// </summary>
        public int? GroupCapacity { get; set; }

        /// <summary>
        /// The registration instance ID key.
        /// </summary>
        public string RegistrationInstanceIdKey { get; set; }

        /// <summary>
        /// Whether the group is shared.
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// The group attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// The group attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// The group members.
        /// </summary>
        public List<GroupMemberBag> GroupMembers { get; set; }
    }

}
