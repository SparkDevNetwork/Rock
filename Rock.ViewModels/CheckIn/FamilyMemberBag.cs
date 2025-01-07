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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A single member of a family. A family member may not belong to the actual
    /// family as they may be assocaited via a "can check-in" relationship. This
    /// can be determined by the <see cref="IsInPrimaryFamily"/> value.
    /// </summary>
    public class FamilyMemberBag
    {
        /// <summary>
        /// Gets or sets the person that represents this family member.
        /// </summary>
        /// <value>The person.</value>
        public PersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets the primary family identifier this person belongs to.
        /// </summary>
        /// <value>The family identifier.</value>
        public string FamilyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendee is in the
        /// primary family matched during the search operation.
        /// </summary>
        public bool IsInPrimaryFamily { get; set; }

        /// <summary>
        /// Gets or sets the group role order. This can be used to order parents
        /// above children.
        /// </summary>
        /// <value>The group role order.</value>
        public int RoleOrder { get; set; }
    }
}
