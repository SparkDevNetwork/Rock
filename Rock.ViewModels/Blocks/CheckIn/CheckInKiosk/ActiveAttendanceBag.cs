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
using Rock.Enums.Event;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// A minimal representation of a single attendance record used to track
    /// counts.
    /// </summary>
    public class ActiveAttendanceBag
    {
        /// <summary>
        /// The encrypted identifier of the attendance record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The encrypted identifier of the group used for check-in.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The encrypted identifier of the location used for check-in.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// The status of the attendance record.
        /// </summary>
        public CheckInStatus Status { get; set; }
    }
}
