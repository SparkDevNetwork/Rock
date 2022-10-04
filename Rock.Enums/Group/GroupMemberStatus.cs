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

namespace Rock.Model
{
    /// <summary>
    /// Represents the status of a <see cref="Rock.Model.GroupMember"/> in a <see cref="Rock.Model.Group"/>.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum GroupMemberStatus
    {
        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is not an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember">GroupMember's</see> membership in the <see cref="Rock.Model.Group"/> is pending.
        /// </summary>
        Pending = 2
    }
}
