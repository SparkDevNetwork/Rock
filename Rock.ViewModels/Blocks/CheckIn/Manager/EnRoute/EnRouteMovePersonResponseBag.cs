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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.EnRoute
{
    /// <summary>
    /// The response returned by the En Route MovePerson block action.
    /// </summary>
    public class EnRouteMovePersonResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the move was persisted
        /// successfully. When false, the modal should stay open and surface
        /// either the WarningMessage or ErrorMessage.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets a non-blocking warning message (e.g. the destination
        /// location has hit its firm room threshold and refused the move).
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a hard validation error message (missing required
        /// dropdown selection, attendance not found, etc.).
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
