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
    /// Tracks the attendance status of the individual.
    /// </summary>
    public enum AttendanceStatus
    {
        /// <summary>
        /// The individual did not attend and is not present in the room.
        /// </summary>
        DidNotAttend = 0,

        /// <summary>
        /// The individual did attend but may not be present in the room.
        /// </summary>
        DidAttend = 1,

        /// <summary>
        /// The individual did attend and is currently present in the room.
        /// </summary>
        IsPresent = 2
    }
}
