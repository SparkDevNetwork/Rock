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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail
{
    /// <summary>
    /// The response returned by the MovePerson block action.
    /// </summary>
    public class AttendanceDetailMovePersonResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the move was persisted.
        /// False means the modal should stay open and surface either the
        /// WarningMessage or ErrorMessage.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets a non-blocking warning message (e.g. the destination
        /// location has hit its firm room threshold and refused the move).
        /// The modal renders this in the "location-full" inline slot and does
        /// not treat it as a validation failure to clear on the next attempt.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a hard validation error message (missing required
        /// dropdown, check-out before check-in, attendance not found, etc.).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the freshly-composed detail bag for the view panel to
        /// rebind against once the move has succeeded. Null on failure.
        /// </summary>
        public AttendanceDetailBag RefreshedDetail { get; set; }
    }
}
