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

namespace Rock.Enums.Connection
{
    /// <summary>
    /// System Update Types
    /// </summary>
    public enum SystemUpdateType
    {
        /// <summary>
        /// Creation type.
        /// </summary>
        Creation = 0,

        /// <summary>
        /// Status Set type
        /// </summary>
        StatusSet = 1,

        /// <summary>
        /// Status Updated type
        /// </summary>
        StatusUpdated = 2,

        /// <summary>
        /// Status Cleared type
        /// </summary>
        StatusCleared = 3,

        /// <summary>
        /// State Change type.
        /// </summary>
        StateChange = 4,

        /// <summary>
        /// Assignment type.
        /// </summary>
        Assignment = 5,

        /// <summary>
        /// Unassignment type.
        /// </summary>
        Unassignment = 6,

        /// <summary>
        /// Reassignment type.
        /// </summary>
        Reassignment = 7,

        /// <summary>
        /// Transfer type.
        /// </summary>
        Transfer = 8,

        /// <summary>
        /// Due Date Change type.
        /// </summary>
        DueDateChange = 9,

        /// <summary>
        /// Due Soon Date Change type.
        /// </summary>
        DueSoonDateChange = 10,

        /// <summary>
        /// Completion type.
        /// </summary>
        Completion = 11
    }
}
