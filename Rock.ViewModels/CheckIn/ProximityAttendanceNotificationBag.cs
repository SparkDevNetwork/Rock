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
namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Represents the notification details sent to the client when a proximity-based
    /// attendance event occurs.
    /// </summary>
    public class ProximityAttendanceNotificationBag
    {
        /// <summary>
        /// Gets or sets the title of the notification (e.g. the heading presented to the user).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the body/message content of the notification displayed to the user.
        /// </summary>
        public string Message { get; set; }
    }
}
