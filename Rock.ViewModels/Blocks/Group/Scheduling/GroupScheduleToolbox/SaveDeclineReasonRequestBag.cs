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

using Rock.ViewModels.Utility;

using System;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about a declined schedule row to be saved for the group schedule toolbox block.
    /// </summary>
    public class SaveDeclineReasonRequestBag
    {
        /// <summary>
        /// Gets or sets the selected person unique identifier.
        /// </summary>
        public Guid SelectedPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance unique identifier.
        /// </summary>
        public Guid AttendanceGuid { get; set; }

        /// <summary>
        /// Gets or sets whether this schedule row was previously confirmed.
        /// Will be true if this request represents a cancellation.
        /// </summary>
        public bool WasSchedulePreviouslyConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the decline reason.
        /// </summary>
        public ListItemBag DeclineReason { get; set; }

        /// <summary>
        /// Gets or sets the decline reason note.
        /// </summary>
        public string DeclineReasonNote { get; set; }
    }
}
