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

using System.ComponentModel;

namespace Rock.Enums.Blocks.Group.Scheduling
{
    /// <summary>
    /// The resource list source type that should be used when presenting available resources for the group scheduler.
    /// </summary>
    public enum ResourceListSourceType
    {
        /// <summary>
        /// Show all members of the selected group.
        /// </summary>
        [Description( "All Group Members" )]
        GroupMembers = 0,

        /// <summary>
        /// Show all members of the selected group that have a scheduling preference set for the selected week.
        /// </summary>
        [Description( "Matching Week" )]
        GroupMatchingPreference = 1,

        /// <summary>
        /// Show all group members from another group.
        /// </summary>
        AlternateGroup = 2,

        /// <summary>
        /// Show all members from the Parent group of the select group (if the selected group has a parent group).
        /// </summary>
        ParentGroup = 3,

        /// <summary>
        /// Show all people that exist in a selected data view.
        /// </summary>
        DataView = 4,

        /// <summary>
        /// Show all members of the selected group that have a scheduling preference set for the selected week AND whose assignment (location/schedule) matches the filters OR they have no assignment.
        /// </summary>
        [Description( "Matching Assignment" )]
        GroupMatchingAssignment = 5
    }
}
