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

namespace Rock.Enums.Event
{
    /// <summary>
    /// The check-in status of an Attendance record.
    /// </summary>
    public enum CheckInStatus
    {
        /// <summary>
        /// The state is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The attendance record has been written but not finalized. This
        /// indicates that a spot in the room has been reserved but the check-in
        /// is still in progress.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// The check-in process has been completed but the individual has
        /// not reached the room yet.
        /// </summary>
        NotPresent = 2,

        /// <summary>
        /// The individual is now present in the room.
        /// </summary>
        Present = 3,

        /// <summary>
        /// The individual has been checked out and is no longer in the room.
        /// </summary>
        CheckedOut = 4
    }
}