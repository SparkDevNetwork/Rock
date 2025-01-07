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
    /// The action types that can be performed for a schedule row within the group schedule toolbox.
    /// </summary>
    public enum ToolboxScheduleRowActionType
    {
        /// <summary>
        /// Accept a pending, scheduled attendance.
        /// </summary>
        Accept = 0,

        /// <summary>
        /// Decline a pending, scheduled attendance.
        /// </summary>
        Decline = 1,

        /// <summary>
        /// Cancel a confirmed, scheduled attendance.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// Delete a person schedule exclusion.
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Schedule oneself (or one's family member).
        /// </summary>
        [Description( "Self-Schedule" )]
        SelfSchedule = 4
    }
}
