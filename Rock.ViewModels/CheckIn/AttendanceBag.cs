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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A single attendance record used by the check-in system.
    /// </summary>
    public class AttendanceBag
    {
        /// <summary>
        /// Gets or sets the attendance identifier.
        /// </summary>
        /// <value>The attendance identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the person this attendance record is for.
        /// </summary>
        /// <value>The person this attendance record is for.</value>
        public PersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets the check-in status.
        /// </summary>
        /// <value>The check-in status.</value>
        public CheckInStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the area for this attendance record.
        /// </summary>
        /// <value>The area for this attendance record.</value>
        public CheckInItemBag Area { get; set; }

        /// <summary>
        /// Gets or sets the group for this attendance record.
        /// </summary>
        /// <value>The group for this attendance record.</value>
        public CheckInItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the location for this attendance record.
        /// </summary>
        /// <value>The location for this attendance record.</value>
        public CheckInItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule for this attendance record.
        /// </summary>
        /// <value>The schedule for this attendance record.</value>
        public CheckInItemBag Schedule { get; set; }
    }
}
