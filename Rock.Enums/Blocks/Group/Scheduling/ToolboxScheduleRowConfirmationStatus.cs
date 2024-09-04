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

namespace Rock.Enums.Blocks.Group.Scheduling
{
    /// <summary>
    /// The confirmation statuses that can be applied to a schedule row within the group schedule toolbox.
    /// </summary>
    public enum ToolboxScheduleRowConfirmationStatus
    {
        /// <summary>
        /// Person has not yet confirmed availability.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Person has committed to attending.
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Person has declined a scheduled attendance.
        /// </summary>
        Declined = 2,

        /// <summary>
        /// Person is unavailable to attend.
        /// </summary>
        Unavailable = 3
    }
}
