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
    /// Represents the kind of change a single event on the Group History timeline describes.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum GroupHistoryEventType
    {
        /// <summary>
        /// A change that does not fit one of the known event types. The event's
        /// caption text is used to describe it.
        /// </summary>
        Other = 0,

        /// <summary>
        /// The group was created.
        /// </summary>
        GroupCreated = 1,

        /// <summary>
        /// One or more of the group's properties or attributes were changed.
        /// </summary>
        GroupUpdated = 2,

        /// <summary>
        /// One or more members were added to the group.
        /// </summary>
        MembersAdded = 3,

        /// <summary>
        /// One or more members were removed from the group.
        /// </summary>
        MembersRemoved = 4,

        /// <summary>
        /// A group member's membership details (such as role or status) were changed.
        /// </summary>
        MemberUpdated = 5
    }
}
