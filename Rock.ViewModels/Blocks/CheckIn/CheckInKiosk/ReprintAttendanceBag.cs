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
using System;

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Represents a single attendance record that can have its labels re-printed.
    /// </summary>
    public class ReprintAttendanceBag
    {
        /// <summary>
        /// The encrypted identifier of the attendance record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The date and time the individual was checked in.
        /// </summary>
        public DateTimeOffset StartDateTime { get; set; }

        /// <summary>
        /// The nick name of the individual.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// The last name of the individual.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The security code that was associated with the attendance record.
        /// </summary>
        public string SecurityCode { get; set; }

        /// <summary>
        /// The group that was checked into.
        /// </summary>
        public CheckInItemBag Group { get; set; }

        /// <summary>
        /// The location that was checked into.
        /// </summary>
        public CheckInItemBag Location { get; set; }

        /// <summary>
        /// The schedule that was used for the attendance record.
        /// </summary>
        public CheckInItemBag Schedule { get; set; }
    }
}
