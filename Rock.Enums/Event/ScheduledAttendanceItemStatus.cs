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
    /// Tracks the scheduled attendance status of the individual.
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum ScheduledAttendanceItemStatus
    {
        /// <summary>
        /// Person's attendance status is pending.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Person's attendance status is confirmed.
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Person's attendance status is declined.
        /// </summary>
        Declined = 2,

        /// <summary>
        /// Person isn't scheduled (they will be in the list of unscheduled resources).
        /// </summary>
        Unscheduled = 3,
    }
}
