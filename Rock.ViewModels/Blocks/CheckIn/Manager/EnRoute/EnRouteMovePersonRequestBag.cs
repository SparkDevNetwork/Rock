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
    /// Payload the En Route Move Person modal sends to the MovePerson block
    /// action.
    /// </summary>
    public class EnRouteMovePersonRequestBag
    {
        /// <summary>
        /// Gets or sets the attendance identifier to move.
        /// </summary>
        public int AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the selected schedule identifier.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the selected location identifier.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the selected group identifier.
        /// </summary>
        public int? GroupId { get; set; }
    }
}
