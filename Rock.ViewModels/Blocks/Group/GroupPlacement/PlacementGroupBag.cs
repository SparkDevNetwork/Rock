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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// The SMS Conversations Initialization Box
    /// </summary>
    public class PlacementGroupBag
    {
        /// <summary>
        /// Gets or sets the group unique identifier. (TODO: Remove
        /// this when Group Details is converted to Obsidian.)
        /// </summary>
        public int GroupId { get; set; }

        public string GroupIdKey { get; set; }

        public string GroupName { get; set; }

        public int GroupOrder { get; set; }

        public string GroupTypeIdKey { get; set; }

        public int? GroupCapacity { get; set; }

        public string RegistrationInstanceIdKey { get; set; }

        public bool IsShared { get; set; }

        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        public Dictionary<string, string> AttributeValues { get; set; }

        public List<GroupMemberBag> GroupMembers { get; set; }
    }
}
