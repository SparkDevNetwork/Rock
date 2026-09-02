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
    /// Initialization payload for the Attendance Detail block.
    /// </summary>
    public class AttendanceDetailInitializationBox
    {
        /// <summary>
        /// Gets or sets the read-only detail payload for the view panel.
        /// Null when the attendance could not be resolved; the client then
        /// falls back to the ErrorMessage.
        /// </summary>
        public AttendanceDetailBag Detail { get; set; }

        /// <summary>
        /// Gets or sets the static block-level options resolved from the
        /// block settings.
        /// </summary>
        public AttendanceDetailOptionsBag Options { get; set; }

        /// <summary>
        /// Gets or sets a page-level error message shown in place of the
        /// view panel when Detail could not be built (e.g. no page parameter,
        /// attendance not found).
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
