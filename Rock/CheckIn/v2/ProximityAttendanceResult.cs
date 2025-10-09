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
using Rock.Model;
using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Represents the result of attempting to create or retrieve a proximity-based
    /// attendance record.
    /// </summary>
    /// <remarks>
    /// This container indicates if the operation was successful, whether a new
    /// <see cref="Attendance"/> record was created, returns the <see cref="Attendance"/>
    /// entity itself and any notification data to be surfaced to the client.
    /// </remarks>
    internal class ProximityAttendanceResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the proximity attendance operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Attendance"/> entity that was found or created.
        /// </summary>
        public Attendance Attendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Attendance"/> was newly created
        /// (true) or an existing record was returned (false).
        /// </summary>
        public bool IsNewAttendance { get; set; }

        /// <summary>
        /// Gets or sets the notification payload to be sent back to the client (e.g. mobile app)
        /// giving user-facing context about the attendance action.
        /// </summary>
        public ProximityAttendanceNotificationBag NotificationData { get; set; }
    }
}
