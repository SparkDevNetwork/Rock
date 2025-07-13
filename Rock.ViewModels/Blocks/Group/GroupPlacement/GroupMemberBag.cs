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
    /// Represents a group member with associated role, attributes, person details, and metadata for placement and UI purposes.
    /// </summary>
    public class GroupMemberBag
    {
        /// <summary>
        /// Gets or sets the group member unique identifier.
        /// (TODO: Remove this when Group Member Detail is converted to Obsidian.)
        /// </summary>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group member.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// The encrypted identifier key for the group role assigned to the member.
        /// </summary>
        public string GroupRoleIdKey { get; set; }

        /// <summary>
        /// The collection of public attribute metadata for this group member.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// The collection of attribute values associated with this group member.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// The person associated with this group member.
        /// </summary>
        public PersonBag Person { get; set; }

        /// <summary>
        /// The date and time when the group member record was created, if available.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }
    }
}
