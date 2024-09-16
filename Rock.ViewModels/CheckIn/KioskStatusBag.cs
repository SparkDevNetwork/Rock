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
using System.Collections.Generic;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines the current status of a kiosk configuration.
    /// </summary>
    public class KioskStatusBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether check-in is currently active.
        /// </summary>
        /// <value><c>true</c> if check-in is active; otherwise, <c>false</c>.</value>
        public bool IsCheckInActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this kiosk has any open locations.
        /// </summary>
        /// <value><c>true</c> if this kiosk has any open locations; otherwise, <c>false</c>.</value>
        public bool HasOpenLocations { get; set; }

        /// <summary>
        /// Gets or sets the next start date time today. A value of <c>null</c>
        /// indicates that this kiosk will not open again today.
        /// </summary>
        /// <value>The next start date time today.</value>
        public DateTimeOffset? NextStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the next stop date time today.
        /// </summary>
        /// <value>The next stop date time today.</value>
        public DateTimeOffset? NextStopDateTime { get; set; }

        /// <summary>
        /// Gets or sets the campus current date time.
        /// </summary>
        /// <value>The campus current date time.</value>
        public DateTimeOffset CampusCurrentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the server current date time.
        /// </summary>
        /// <value>The server current date time.</value>
        public DateTimeOffset ServerCurrentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the location identifiers. A change in one of
        /// these locations should trigger an update of the kiosk status.
        /// </summary>
        /// <value>The location identifiers.</value>
        public List<string> LocationIds { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifiers. A change in one of
        /// these schedules should trigger an update of the kiosk status.
        /// </summary>
        /// <value>The schedule identifiers.</value>
        public List<string> ScheduleIds { get; set; }
    }
}
